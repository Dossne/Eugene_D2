using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.BottomPanel;
using Features.Competition;
using Features.HudLevelButtons;
using Features.LavaQuest;
using Features.Level;
using Features.PlayerProfile;
using Features.RecommendationUi;
using Features.RewardTrack;
using Features.ScoringVisualize;
using Features.SuperSpeedMode;
using Features.Tutorial;
using Infrastructure.Ads;
using Infrastructure.AssetManagement;
using Infrastructure.AudioControl;
using Infrastructure.CurrencyHud;
using Infrastructure.DateTimeControl;
using Infrastructure.LoadScreen;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.SystemModules;
using Infrastructure.SystemsLifeCycle;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Infrastructure.Scope
{
    public class MetaSceneFlow : IAsyncStartable, IDisposable
    {
        private readonly MetaSceneScope ownerScope;
        private readonly SystemsRegistry systemsRegistry;

        private readonly SdkService sdkService;
        private readonly LoadScreenController loadScreenController;
        private readonly AssetProvider assetProvider;
        private readonly SceneLoadController sceneLoadController;
        private readonly PopupService popupService;
        private readonly MetaSceneUIService metaSceneUIService;
        private readonly AppBaseAnalytics appBaseAnalytics;
        private readonly CurrencyHudService currencyHudService;
        private readonly StartLevelButtonController startLevelButtonController;
        private readonly PreBoosterPopupController preBoosterPopupController;
        private readonly CancellationTokenSource startCts;
        private readonly CurrencyHudFxService currencyHudFxService;
        private readonly RecommendationUiManager recommendationUiManager;
        private readonly RewardTrackUiController rewardTrackUiController;
        private readonly LavaQuestUIController lavaQuestUIController;
        private readonly TutorialService tutorialService;
        private readonly DateTimeService dateTimeService;
        private readonly LevelService levelService;
        private readonly SuperSpeedController superSpeedController;
        private readonly TutorialTriggerEvent tutorialTriggerEvent;
        private readonly PlayerProfileController playerProfileController;
        private readonly BottomPanelController bottomPanelController;
        private readonly CompetitionUIController competitionUIController;
        private readonly ScoringVisualizeService scoringVisualizeService;

        public MetaSceneFlow(
            MetaSceneScope ownerScope,
            SystemsRegistry systemsRegistry,
            IEnumerable<ISavable> savables,
            IEnumerable<ISystemTickable> tickables,
            IEnumerable<ISystemFixedTickable> fixedTickables,
            IEnumerable<ISystemLateTickable> lateTickables,
            SdkService sdkService,
            AssetProvider assetProvider,
            SceneLoadController sceneLoadController,
            PopupService popupService,
            LoadScreenController loadScreenController,
            MetaSceneUIService metaSceneUIService,
            AppBaseAnalytics appBaseAnalytics,
            CurrencyHudService currencyHudService,
            CurrencyHudFxService currencyHudFxService,
            StartLevelButtonController startLevelButtonController,
            PreBoosterPopupController preBoosterPopupController,
            RecommendationUiManager recommendationUiManager, 
            RewardTrackUiController rewardTrackUiController,
			LavaQuestUIController lavaQuestUIController,
            TutorialService tutorialService,
            DateTimeService dateTimeService,
            LevelService levelService,
            SuperSpeedController superSpeedController,
            TutorialTriggerEvent tutorialTriggerEvent,
            PlayerProfileController playerProfileController,
            BottomPanelController bottomPanelController, 
            CompetitionUIController competitionUIController, 
            ScoringVisualizeService scoringVisualizeService)
        {
            this.ownerScope = ownerScope;
            this.systemsRegistry = systemsRegistry;
            this.systemsRegistry.Register(ownerScope, savables);
            this.systemsRegistry.Register(ownerScope, tickables);
            this.systemsRegistry.Register(ownerScope, fixedTickables);
            this.systemsRegistry.Register(ownerScope, lateTickables);

            this.sdkService = sdkService;
            this.assetProvider = assetProvider;
            this.sceneLoadController = sceneLoadController;
            this.popupService = popupService;
            this.loadScreenController = loadScreenController;
            this.appBaseAnalytics = appBaseAnalytics;
            this.currencyHudService = currencyHudService;
            this.startLevelButtonController = startLevelButtonController;
            this.preBoosterPopupController = preBoosterPopupController;
            this.metaSceneUIService = metaSceneUIService;
            this.currencyHudFxService = currencyHudFxService;
            this.recommendationUiManager = recommendationUiManager;
            this.rewardTrackUiController = rewardTrackUiController;
            this.lavaQuestUIController = lavaQuestUIController;
            this.tutorialService = tutorialService;
            this.dateTimeService = dateTimeService;
            this.levelService = levelService;
            this.superSpeedController = superSpeedController;
            this.tutorialTriggerEvent = tutorialTriggerEvent;
            this.playerProfileController = playerProfileController;
            this.bottomPanelController = bottomPanelController;
            this.competitionUIController = competitionUIController;
            this.scoringVisualizeService = scoringVisualizeService;

            startCts = new CancellationTokenSource();
        }


        async UniTask IAsyncStartable.StartAsync(CancellationToken token)
        {
            AudioService.I.PlayMusic(SfxType.MainTheme);
            await InitializeSystemsAsync();

            sceneLoadController.OnProcessBegin += SceneLoaderScoped_OnProcessBegin;
            var startToken = startCts.Token;
          
            try
            {
                await UniTask.WaitUntil(() => sdkService.IsInitialized(), cancellationToken: startToken);
                await dateTimeService.TimeResyncAsync(startToken);
                appBaseAnalytics.TrackAppLoaded();
                loadScreenController.Close(LoadScreenUI.Reason.LoadScene);

                if (tutorialService.NeedLockLevelStart(levelService.CurrentLevelNumber)) 
                    startLevelButtonController.DisableButtonAction();

                await superSpeedController.ExecuteScheduledAsync(startToken);
                await currencyHudFxService.ExecuteScheduledAsync(startToken);
                await rewardTrackUiController.ExecuteScheduledAsync(startToken);
                await scoringVisualizeService.ExecuteScheduledAsync(startToken);
                await recommendationUiManager.ExecuteScheduledAsync(startToken);
                await lavaQuestUIController.ExecuteScheduledAsync(startToken);
                await competitionUIController.ExecuteScheduledAsync(startToken);
                //require await for all related popups close in scheduled method
                //https://www.notion.so/whalergames/2abb9933364c803199c3d2369b284c2f
                //Disable-enable input only for related popups open waiting in scheduled method

                if (tutorialService.NeedLockLevelStart(levelService.CurrentLevelNumber))
                    startLevelButtonController.EnableButtonAction();

                tutorialTriggerEvent.Execute(TutorialTrigger.SceneLoad);
            }
            catch (OperationCanceledException e)
            {
                UnityEngine.Debug.LogWarning(e);
            }
        }


        void IDisposable.Dispose()
        {
            startCts.Cancel();
            startCts.Dispose();
        }


        private async UniTask InitializeSystemsAsync()
        {
            systemsRegistry.Load(ownerScope);
            metaSceneUIService.Initialize();
            currencyHudService.SetMode(CurrencyHudMode.Meta);
            startLevelButtonController.Initialize();
            preBoosterPopupController.Initialize();
            rewardTrackUiController.Initialize();
            await lavaQuestUIController.InitializeAsync(startCts.Token);
            playerProfileController.ShowWidget();
            bottomPanelController.Initialize();
            await competitionUIController.InitializeAsync(startCts.Token);
            //TODO continue from here

            SetSystemsActive(); //in the end
        }


        private void DeinitializeSystems()
        {
            AudioService.I.StopMusic();
            UnregisterSystemsByCurrentScope();
            sceneLoadController.OnProcessBegin -= SceneLoaderScoped_OnProcessBegin;
            popupService.CloseActivePopups();
            metaSceneUIService.Deinitialize();
            assetProvider.Deinitialize();
            startLevelButtonController.Deinitialize();
            preBoosterPopupController.Deinitialize();
            rewardTrackUiController.Deinitialize();
            lavaQuestUIController.Deinitialize();
            bottomPanelController.Deinitilize();
            competitionUIController.Deinitialize();
            //TODO continue from here
        }


        private void SetSystemsActive()
        {
            systemsRegistry.SetActive(ownerScope);
        }


        private void UnregisterSystemsByCurrentScope()
        {
            systemsRegistry.Unregister(ownerScope);
        }


        private void SceneLoaderScoped_OnProcessBegin(string sceneName, LoadSceneMode loadMode)
        {
            DeinitializeSystems();
        }
    }
}