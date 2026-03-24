using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.CameraFollow;
using Features.Character;
using Features.Collectables;
using Features.Events;
using Features.ExtraItemSpawn;
using Features.FlyingTaskIcon;
using Features.FlyingText;
using Features.LevelTasks;
using Features.LevelComplete;
using Features.LevelTime;
using Features.LevelConfiguration;
using Features.LevelUp;
using Features.LevelLoose;
using Infrastructure.Ads;
using Infrastructure.AssetManagement;
using Infrastructure.InputControl;
using Infrastructure.LoadScreen;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.SystemsLifeCycle;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using Features.Level;
using Features.TargetMarker;
using UnityEngine;
using Features.Life;
using Infrastructure.CurrencyHud;
using Features.CollectableCurrency;
using Features.WinStreak;
using Features.Banner;
using Features.ExtraCollectableUi;
using Features.LevelSessionStateControl;
using Features.RewardTrack;
using Features.Tutorial;
using Infrastructure.GameSceneUIControl;
using Features.Shadows;
using Infrastructure.AudioControl;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Features.PlayerProfile;

namespace Infrastructure.Scope
{
    public class GameSceneFlow : IAsyncStartable, IDisposable
    {
        private readonly GameSceneScope ownerScope;
        private readonly SystemsRegistry systemsRegistry;
        private readonly ConfigProvider configProvider;
        private readonly GameSceneUIService gameSceneUIService;
        private readonly LoadScreenController loadScreenController;
        private readonly AssetProvider assetProvider;
        private readonly PopupOpedListener popupOpedListener;
        private readonly InputService inputService;
        private readonly SceneLoadController sceneLoadController;
        private readonly PopupService popupService;
        private readonly LevelSessionStateController levelSessionStateController;
        private readonly LevelCreateManager levelCreateManager;
        private readonly CharacterManager characterManager;
        private readonly CameraManager cameraManager;
        private readonly ShadowsService shadowsService;
        private readonly LevelUpManager levelUpManager;
        private readonly ItemCollectManager itemCollectManager;
        private readonly CannonBombShooter cannonBombShooter;
        private readonly CollectableItemDeactivator collectableItemDeactivator;
        private readonly LevelTaskManager levelTaskManager;
        private readonly LevelFlyingTextManager flyingTextManager;
        private readonly LevelCompleteManager levelCompleteManager;
        private readonly LevelLooseManager levelLooseManager;
        private readonly LevelTimeManager levelTimeManager;
        private readonly FlyingTaskIconManager flyingTaskIconManager;
        private readonly GameBaseAnalytics gameBaseAnalytics;
        private readonly LevelStartedEvent startedEvent;
        private readonly InGameBoosterController inGameBoosterController;
        private readonly LevelService levelService;
        private readonly PreBoosterInGamePopupController preBoosterInGamePopupController;
        private readonly WinStreakBonusApplier winStreakBonusApplier;
        private readonly WinStreakStateController winStreakStateController;
        private readonly TargetMarkerService targetMarkerService;
        private readonly LifeController lifeController;
        private readonly CurrencyHudService currencyHudService;
        private readonly CollectableCurrencyService collectableCurrencyService;
        private readonly BannerController bannerController;
        private readonly RewardTrackCollectController rewardTrackCollectController;
        private readonly ExtraCollectableUIService extraCollectableUIService;
        private readonly ExtraItemsSpawner extraItemsSpawner;
        private readonly AppBaseAnalytics appBaseAnalytics;
        private readonly DateTimeService dateTimeService;
        private readonly TutorialTriggerEvent tutorialTriggerEvent;
        private readonly PlayerProfileController playerProfileController;
        private readonly CancellationTokenSource startCts;


        public GameSceneFlow(
            GameSceneScope ownerScope,
            SystemsRegistry systemsRegistry,
            IEnumerable<ISavable> savables,
            IEnumerable<ISystemTickable> tickables,
            IEnumerable<ISystemFixedTickable> fixedTickables,
            IEnumerable<ISystemLateTickable> lateTickables,
            GameSceneUIService gameSceneUIService,
            LoadScreenController loadScreenController,
            AssetProvider assetProvider,
            PopupOpedListener popupOpedListener,
            InputService inputService,
            SceneLoadController sceneLoadController,
            PopupService popupService,
            LevelSessionStateController levelSessionStateController,
            LevelCreateManager levelCreateManager,
            ExtraCollectableUIService extraCollectableUIService,
            CharacterManager characterManager,
            CameraManager cameraManager,
            ShadowsService shadowsService,
            LevelUpManager levelUpManager,
            ItemCollectManager itemCollectManager,
            CannonBombShooter cannonBombShooter,
            CollectableItemDeactivator collectableItemDeactivator,
            LevelTaskManager levelTaskManager,
            LevelFlyingTextManager flyingTextManager,
            LevelCompleteManager levelCompleteManager,
            LevelLooseManager levelLooseManager,
            LevelTimeManager levelTimeManager,
            FlyingTaskIconManager flyingTaskIconManager,
            GameBaseAnalytics gameBaseAnalytics,
            LevelStartedEvent startedEvent,
            InGameBoosterController inGameBoosterController,
            LevelService levelService,
            PreBoosterInGamePopupController preBoosterInGamePopupController,
            TargetMarkerService targetMarkerService,
            LifeController lifeController,
            CurrencyHudService currencyHudService, 
            CollectableCurrencyService collectableCurrencyService,
            WinStreakBonusApplier winStreakBonusApplier, 
            WinStreakStateController winStreakStateController,
            BannerController bannerController, 
            RewardTrackCollectController rewardTrackCollectController, 
            ExtraItemsSpawner extraItemsSpawner,
            AppBaseAnalytics appBaseAnalytics,
            ConfigProvider configProvider,
            DateTimeService dateTimeService,
            TutorialTriggerEvent tutorialTriggerEvent,
            PlayerProfileController playerProfileController)
        {
            this.ownerScope = ownerScope;
            this.systemsRegistry = systemsRegistry;
            this.systemsRegistry.Register(ownerScope, savables);
            this.systemsRegistry.Register(ownerScope, tickables);
            this.systemsRegistry.Register(ownerScope, fixedTickables);
            this.systemsRegistry.Register(ownerScope, lateTickables);

            this.configProvider = configProvider;
            this.gameSceneUIService = gameSceneUIService;
            this.loadScreenController = loadScreenController;
            this.assetProvider = assetProvider;
            this.popupOpedListener = popupOpedListener;
            this.inputService = inputService;
            this.sceneLoadController = sceneLoadController;
            this.popupService = popupService;
            this.levelSessionStateController = levelSessionStateController;
            this.levelCreateManager = levelCreateManager;
            this.extraCollectableUIService = extraCollectableUIService;
            this.characterManager = characterManager;
            this.cameraManager = cameraManager;
            this.shadowsService = shadowsService;
            this.levelUpManager = levelUpManager;
            this.itemCollectManager = itemCollectManager;
            this.cannonBombShooter = cannonBombShooter;
            this.levelTaskManager = levelTaskManager;
            this.flyingTextManager = flyingTextManager;
            this.levelCompleteManager = levelCompleteManager;
            this.levelLooseManager = levelLooseManager;
            this.levelTimeManager = levelTimeManager;
            this.collectableItemDeactivator = collectableItemDeactivator;
            this.flyingTaskIconManager = flyingTaskIconManager;
            this.gameBaseAnalytics = gameBaseAnalytics;
            this.startedEvent = startedEvent;
            this.inGameBoosterController = inGameBoosterController;
            this.levelService = levelService;
            this.preBoosterInGamePopupController = preBoosterInGamePopupController;
            this.targetMarkerService = targetMarkerService;
            this.lifeController = lifeController;
            this.currencyHudService = currencyHudService;
            this.collectableCurrencyService = collectableCurrencyService;
            this.winStreakBonusApplier = winStreakBonusApplier;
            this.winStreakStateController = winStreakStateController;
            this.bannerController = bannerController;
            this.rewardTrackCollectController = rewardTrackCollectController;
            this.extraItemsSpawner = extraItemsSpawner;
            this.appBaseAnalytics = appBaseAnalytics;
            this.dateTimeService = dateTimeService;
            this.tutorialTriggerEvent = tutorialTriggerEvent;
            this.playerProfileController = playerProfileController;

            startCts = new CancellationTokenSource();

        }


        public async UniTask StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                inputService.ResetInput();
                await dateTimeService.TimeResyncAsync(startCts.Token);
                await InitializeSystems();
                sceneLoadController.OnProcessBegin += SceneLoaderScoped_OnProcessBegin;
                await LoadScreenPlayFakeProgreeAsync();
                appBaseAnalytics.TrackAppLoaded();
                loadScreenController.Close(LoadScreenUI.Reason.LoadScene);
                AudioService.I.PlayMusic(SfxType.LevelTheme);

                await inGameBoosterController.ExecuteScheduledAsync(startCts.Token);
                await winStreakBonusApplier.ExecuteScheduledAsync(startCts.Token);
                await levelSessionStateController.ExecuteScheduledAsync(startCts.Token);

                tutorialTriggerEvent.Execute(TutorialTrigger.SceneLoad);
                inputService.EnableInput();
                
                await UniTask.WaitUntil(() => { return inputService.HasInput() || inGameBoosterController.HasActiveBoosters; }, cancellationToken: startCts.Token);
                
                ApplyActionsOnGameProcessBegin();
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning(e);
            }
        }


        void IDisposable.Dispose()
        {
            startCts.Cancel();
            startCts.Dispose();
        }


        private async UniTask InitializeSystems()
        {
            InitializeProgress();
            levelSessionStateController.Initialize();
            
            gameSceneUIService.Initialize();
            gameBaseAnalytics.Initialize();
            popupOpedListener.Initialize();
            targetMarkerService.Initialize();
            await levelCreateManager.InitializeAsync(startCts.Token);
            await extraItemsSpawner.InitializeAsync(startCts.Token);
            await characterManager.InitializeAsync(startCts.Token);
            levelUpManager.Initialize();
            cameraManager.Initialize();
            shadowsService.Initialize();
            itemCollectManager.Initialize();
            await cannonBombShooter.InitializeAsync(startCts.Token);
            collectableItemDeactivator.Initialize();
            levelTaskManager.Initialize();
            extraCollectableUIService.Initialize();
            await collectableCurrencyService.InitializeAsync(startCts.Token);
            flyingTextManager.Initialize();
            levelCompleteManager.Initialize();
            levelLooseManager.Initialize();
            levelTimeManager.Initialize();
            flyingTaskIconManager.Initialize();
            inGameBoosterController.Initialize();
            preBoosterInGamePopupController.Initialize();
            InitializeCurrencyHudService();
            winStreakBonusApplier.Initialize();
            await rewardTrackCollectController.InitializeAsync(startCts.Token);
            //TODO add here

            playerProfileController.HideWidget();

            SetSystemsActive(); //in the end
            bannerController.ShowBanner();
        }


        private void DeinitializeSystems()
        {
            AudioService.I.StopMusic();

            bannerController.HideBanner();
            gameSceneUIService.Deinitialize();
            
            UnregisterSystemsByCurrentScope();
            CollectableId.Reset();
            
            sceneLoadController.OnProcessBegin -= SceneLoaderScoped_OnProcessBegin;
            popupService.CloseActivePopups();
            inputService.DisableInput();
            gameBaseAnalytics.Deinitialize();
            popupOpedListener.Deinitialize();
            assetProvider.Deinitialize();
            levelSessionStateController.Deinitialize();
            characterManager.Deinitialize();
            levelCreateManager.Deinitialize();
            extraItemsSpawner.Deinitialize();
            levelUpManager.Deinitialize();
            cameraManager.Deinitialize();
            shadowsService.Deinitialize();
            itemCollectManager.Deinitialize();
            cannonBombShooter.Deinitialize();
            collectableItemDeactivator.Deinitialize();
            levelTaskManager.Deinitialize();
            extraCollectableUIService.Deinitialize();
            collectableCurrencyService.Deinitialize();
            flyingTextManager.Deinitialize();
            levelCompleteManager.Deinitialize();
            levelLooseManager.Deinitialize();
            levelTimeManager.Deinitialize();
            flyingTaskIconManager.Deinitialize();
            inGameBoosterController.Deinitialize();
            preBoosterInGamePopupController.Deinitialize();
            targetMarkerService.Deinitialize();
            winStreakBonusApplier.Deinitialize();
            rewardTrackCollectController.Deinitialize();


            //TODO continue from here

        }


        private void InitializeCurrencyHudService()
        {
            currencyHudService.SetMode(CurrencyHudMode.Game);
        }


        private void SetSystemsActive()
        {
            systemsRegistry.SetActive(ownerScope);
        }


        private void InitializeProgress()
        {
            systemsRegistry.Load(ownerScope);
        }


        private void UnregisterSystemsByCurrentScope()
        {
            systemsRegistry.Unregister(ownerScope);
        }


        private void ApplyActionsOnGameProcessBegin()
        {
            startedEvent.Execute(levelService.GetCurrentLevelId(), levelService.CurrentLevelNumber, winStreakStateController.CurrentLevel);
            inGameBoosterController.DecreaseSelectedPreBoosters();
            levelTimeManager.SetTimerActive();
            lifeController.SetLastStartTimeStamp();
            levelTaskManager.ExecuteScheduled();
        }


        private void SceneLoaderScoped_OnProcessBegin(string sceneName, LoadSceneMode loadScene)
        {
            DeinitializeSystems();
        }

        private async UniTask LoadScreenPlayFakeProgreeAsync()
        {
            if (levelService.CurrentLevelNumber != 1 || !configProvider.SystemSettingsConfig.SettingsData.haveFirstLoadProlongation)
                return;
            
            float random = UnityEngine.Random.Range(5f, 6f);
            loadScreenController.PlayProgressToStageAsync(random * 0.5f, LoadScreenUI.Stage08, startCts.Token, true).Forget();
            await UniTask.WaitForSeconds(random * 0.5f, true, cancellationToken: startCts.Token);
            loadScreenController.PlayProgressToStageAsync(random * 0.5f, LoadScreenUI.Stage10, startCts.Token, true).Forget();
            await UniTask.WaitForSeconds(random * 0.5f, true, cancellationToken: startCts.Token);            
        }
    }
}
