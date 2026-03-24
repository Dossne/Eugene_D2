using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Banner;
using Features.Boosters;
using Features.Competition;
using Features.CurrencyRewardDisplay;
using Features.Warnings;
using Features.FreePaidOffer;
using Features.LavaQuest;
using Features.Life;
using Features.LifeUi;
using Features.NoAdsUi;
using Features.PhysicsLogic;
using Features.PlayerProfile;
using Features.PurchaseUi;
using Features.RecommendationUi;
using Features.RewardTrack;
using Features.ShopUi;
using Features.Social;
using Features.SuperDiscountUi;
using Features.SuperSpeedMode;
using Features.Tutorial;
using Features.Widgets;
using Features.WinStreak;
using Infrastructure.Ads;
using Infrastructure.ApplicationInterrupt;
using Infrastructure.AssetManagement;
using Infrastructure.AudioControl;
using Infrastructure.BroTweens;
using Infrastructure.Configs;
using Infrastructure.CurrencyHud;
using Infrastructure.DateTimeControl;
using Infrastructure.Events;
using Infrastructure.HapticControl;
using Infrastructure.InputControl;
using Infrastructure.LoadScreen;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Pool;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SceneManagement;
using Infrastructure.Settings;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.SystemModules;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.TimeCycles;
using R3;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using Features.Team;
using Features.Level;
using Features.ScoringVisualize;
using Infrastructure.WelcomeBonus;

namespace Infrastructure.Scope
{
    public class MainSceneFlow : IAsyncStartable, ITickable, IFixedTickable, ILateTickable
    {
#if PR_CHEAT
        private readonly Cheat.CheatService cheatService;
#endif
        private readonly SystemsRegistry systemsRegistry;
        private readonly MainSceneScope ownerScope;
        private readonly SdkService sdkService;
        private readonly ConfigProvider configProvider;
        private readonly LoadScreenController loadScreenController;
        private readonly MainUICanvasHandler mainUICanvasHandler;
        private readonly AddressableDownloadService addressableDownloadService;
        private readonly SceneLoadController sceneLoadController;
        private readonly AssetProvider assetProvider;
        private readonly SaveStorage saveStorage;
        private readonly SocialService socialService;
        private readonly LeaderboardService leaderboardService;
        private readonly MailService mailService;
        private readonly InputService inputService;
        private readonly InputUiService inputUiService;
        private readonly PoolService poolService;
        private readonly PopupService popupService;
        private readonly AppInterruptObserver appInterruptObserver;
        private readonly AppSettingsController appSettingsController;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly SoftResetEvent softResetEvent;
        private readonly AdsFlyingTextsService adsFlyingTextsService;
        private readonly CurrencyHudService currencyHudService;
        private readonly CurrencyHudFxService currencyHudFxService;
        private readonly CurrencyRewardDisplayService currencyRewardDisplayService;
        private readonly WidgetManager widgetManager;
        private readonly BoostersManager boostersManager;
        private readonly IPurchaseManager purchaseManager;
        private readonly PurchaseOfferContainer purchaseOfferContainer;
        private readonly PurchaseUiController purchaseUiController;
        private readonly ShopUiController shopUiController;
        private readonly TeamUiController teamUiController;
        private readonly NoAdsUiController noAdsUiController;
        private readonly FreePaidOfferService freePaidOfferService;
        private readonly FreePaidOfferUiController freePaidOfferUiController;
        private readonly SuperDiscountUiController superDiscountUiController;
        private readonly WarningService warningService;
        private readonly LifeController lifeController;
        private readonly LifeUiController lifeUiController;
        private readonly RecommendationUiManager recommendationUiManager;
        private readonly WinStreakStateController winStreakStateController;
        private readonly BannerController bannerController;
        private readonly DateTimeService dateTimeService;
        private readonly TimeCyclesService timeCyclesService;
        private readonly RewardTrackStateController rewardTrackStateController;
        private readonly RewardTrackStatePopupController rewardTrackStatePopupController;
        private readonly LavaQuestStateController lavaQuestStateController;
        private readonly LavaQuestEventPopupController lavaQuestEventPopupController;
        private readonly LavaQuestStartPopupController lavaQuestStartPopupController;
        private readonly PlayerProfileController playerProfileController;
        private readonly TutorialService tutorialService;
        private readonly SuperSpeedController superSpeedController;
        private readonly LevelService levelService;
        private readonly CancellationTokenSource startCts;
        private readonly CompositeDisposable compositeDisposable;
        private readonly CompetitionManager competitionManager;
        private readonly ScoringVisualizeService scoringVisualizeService;
        private readonly FirstLaunchHandler firstLaunchHandler;

        public MainSceneFlow(
#if PR_CHEAT
            Cheat.CheatService cheatService,
#endif
            MainSceneScope ownerScope,
            SystemsRegistry systemsRegistry,
            IEnumerable<ISavable> savables,
            IEnumerable<ISystemTickable> tickables,
            IEnumerable<ISystemFixedTickable> fixedTickables,
            IEnumerable<ISystemLateTickable> lateTickables,
            SdkService sdkService,
            ConfigProvider configProvider,
            LoadScreenController loadScreenController,
            MainUICanvasHandler mainUICanvasHandler,
            AddressableDownloadService addressableDownloadService,
            AppInterruptObserver appInterruptObserver,
            SceneLoadController sceneLoadController,
            AssetProvider assetProvider,
            SaveStorage saveStorage,
            SocialService socialService,
            LeaderboardService leaderboardService,
            MailService mailService,
            InputService inputService,
            InputUiService inputUiService,
            PoolService poolService,
            PopupService popupService,
            AppSettingsController appSettingsController,
            SpriteAtlasService spriteAtlasService,
            SoftResetEvent softResetEvent,
            AdsFlyingTextsService adsFlyingTextsService,
            CurrencyHudService currencyHudService,
            CurrencyHudFxService currencyHudFxService,
            CurrencyRewardDisplayService currencyRewardDisplayService,
            WidgetManager widgetManager,
            BoostersManager boostersManager,
            IPurchaseManager purchaseManager,
            PurchaseOfferContainer purchaseOfferContainer,
            PurchaseUiController purchaseUiController,
            ShopUiController shopUiController,
            TeamUiController teamUiController,
            NoAdsUiController noAdsUiController,
            FreePaidOfferService freePaidOfferService,
            FreePaidOfferUiController freePaidOfferUiController,
            SuperDiscountUiController superDiscountUiController,
            WarningService currencyWarningService,
            LifeController lifeController,
            LifeUiController lifeUiController,
            RecommendationUiManager recommendationUiManager,
            WinStreakStateController winStreakStateController,
            BannerController bannerController, 
            DateTimeService dateTimeService, 
            TimeCyclesService timeCyclesService, 
            RewardTrackStateController rewardTrackStateController, 
            RewardTrackStatePopupController rewardTrackStatePopupController,
            LavaQuestStateController lavaQuestStateController, 
            LavaQuestEventPopupController lavaQuestEventPopupController,
            LavaQuestStartPopupController lavaQuestStartPopupController, 
            TutorialService tutorialService,
            PlayerProfileController playerProfileController,
            LevelService levelService, 
            CompetitionManager competitionManager, 
            ScoringVisualizeService scoringVisualizeService,
            FirstLaunchHandler firstLaunchHandler)
        {
#if PR_CHEAT
            this.cheatService = cheatService;
#endif
            this.systemsRegistry = systemsRegistry;
            this.ownerScope = ownerScope;
            this.systemsRegistry.Register(this.ownerScope, savables);
            this.systemsRegistry.Register(this.ownerScope, tickables);
            this.systemsRegistry.Register(this.ownerScope, fixedTickables);
            this.systemsRegistry.Register(this.ownerScope, lateTickables);
            this.systemsRegistry.Register(this.ownerScope, HapticService.I);
            this.systemsRegistry.Register(this.ownerScope, BroTweenService.I);

            this.sdkService = sdkService;
            this.configProvider = configProvider;
            this.loadScreenController = loadScreenController;
            this.mainUICanvasHandler = mainUICanvasHandler;
            this.addressableDownloadService = addressableDownloadService;
            this.appInterruptObserver = appInterruptObserver;
            this.sceneLoadController = sceneLoadController;
            this.assetProvider = assetProvider;
            this.saveStorage = saveStorage;
            this.socialService = socialService;
            this.leaderboardService = leaderboardService;
            this.mailService = mailService;
            this.inputService = inputService;
            this.inputUiService = inputUiService;
            this.poolService = poolService;
            this.popupService = popupService;
            this.appSettingsController = appSettingsController;
            this.spriteAtlasService = spriteAtlasService;
            this.softResetEvent = softResetEvent;
            this.adsFlyingTextsService = adsFlyingTextsService;
            this.currencyHudService = currencyHudService;
            this.currencyHudFxService = currencyHudFxService;
            this.currencyRewardDisplayService = currencyRewardDisplayService;
            this.widgetManager = widgetManager;
            this.boostersManager = boostersManager;            
            this.purchaseManager = purchaseManager;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.purchaseUiController = purchaseUiController;
            this.noAdsUiController = noAdsUiController;
            this.freePaidOfferService = freePaidOfferService;
            this.freePaidOfferUiController = freePaidOfferUiController;
            this.superDiscountUiController = superDiscountUiController;
            this.shopUiController = shopUiController;
            this.teamUiController = teamUiController;
            this.warningService = currencyWarningService;
            this.lifeController = lifeController;
            this.lifeUiController = lifeUiController;
            this.recommendationUiManager = recommendationUiManager;
            this.winStreakStateController = winStreakStateController;
            this.bannerController = bannerController;
            this.dateTimeService = dateTimeService;
            this.timeCyclesService = timeCyclesService;
            this.rewardTrackStateController = rewardTrackStateController;
            this.rewardTrackStatePopupController = rewardTrackStatePopupController;
            this.lavaQuestStateController = lavaQuestStateController;
            this.lavaQuestEventPopupController = lavaQuestEventPopupController;
            this.tutorialService = tutorialService;
            this.lavaQuestStartPopupController = lavaQuestStartPopupController;
            this.playerProfileController = playerProfileController;
            this.levelService = levelService;
            this.competitionManager = competitionManager;
            this.scoringVisualizeService = scoringVisualizeService;
            this.firstLaunchHandler = firstLaunchHandler;
            startCts = new CancellationTokenSource();
            compositeDisposable = new CompositeDisposable();
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            ShowLoadScreen();

            softResetEvent.Subscribe(_ => { SoftReset().Forget(); }).AddTo(compositeDisposable);

#if UNITY_EDITOR
            await sceneLoadController.UnloadAllAdditiveScenes(startCts.Token);
#endif

            if (configProvider.UseRemoteConfigs)
            {
                await UniTask.WaitUntil(() => sdkService.IsRemoteConfigLoaded(), cancellationToken: startCts.Token);
                //Additional pause for correct initialization of SayKit on iOS devices
                //(https://www.notion.so/whalergames/iOS-SayKit-isInitialized-a4553313e2b6409e9e208d974cf4ad99?pvs=4)
                await UniTask.NextFrame(cancellationToken: startCts.Token);

                configProvider.SetConfigsFromRemoteSource();
            }

            configProvider.Initialize();

            ConstructSingletons();
            InitializeInput();
            InitializeUiInput();
            InitializeLoadScreen();
            LoadScreenPlayFakeProgreeAsync().Forget();
            await addressableDownloadService.InitializeAsync(startCts.Token);
            await InitializeSystems();
            await sceneLoadController.LoadNextSceneAsync(startCts.Token);
            appInterruptObserver.Interrupt += AppInterruptObserver_Interrupt;
            sceneLoadController.OnProcessBegin += SceneLoaderScoped_OnProcessBegin;            
        }

        void ITickable.Tick()
        {
            systemsRegistry.Tick();
        }

        void IFixedTickable.FixedTick()
        {
            systemsRegistry.FixedTick();
        }

        void ILateTickable.LateTick()
        {
            systemsRegistry.LateTick();
        }

        private async UniTaskVoid SoftReset()
        {
            await DeinitializeSystems();
            sceneLoadController.LoadMainSceneAsync().Forget();
        }

        private async UniTask InitializeSystems()
        {
            LayerConstants.Initialize();
            BroTweenService.I.Initialize();

            SayKitApp.OnRemoteConfigUpdated += SayKitApp_OnRemoteConfigUpdated;
#if PR_CHEAT
            await cheatService.InitializeAsync();
#endif
            InitializeProgress();
            await levelService.InitializeAsync(startCts.Token);
            await dateTimeService.InitializeAsync(startCts.Token);
            timeCyclesService.Initialize();
            socialService.Initialize();
            leaderboardService.Initialize();
            mailService.Initialize();
            await spriteAtlasService.InitializeAsync(startCts.Token);
            firstLaunchHandler.Initialize();
            appSettingsController.Initialize();
            HapticService.I.Initialize();
            AudioService.I.Initialize();
            mainUICanvasHandler.Initialize();
            await poolService.InitializeAsync(startCts.Token);
            adsFlyingTextsService.Initialize();
            warningService.Initialize();
            await currencyHudService.InitializeAsync(startCts.Token);
            currencyHudFxService.Initialize();
            currencyRewardDisplayService.Initialize();
            await purchaseManager.InitializeAsync(startCts.Token);
            purchaseOfferContainer.Initialize();
            purchaseUiController.Initialize();
            await widgetManager.InitializeAsync(startCts.Token);
            boostersManager.Initialize();            
            await shopUiController.InitializeAsync(startCts.Token);
            await teamUiController.InitializeAsync(startCts.Token);
            await noAdsUiController.InitializeAsync(startCts.Token);
            superDiscountUiController.Initialize();
            lifeController.Initialize();
            lifeUiController.Initialize();
            await recommendationUiManager.InitializeAsync(startCts.Token);
            winStreakStateController.Initialize();
            bannerController.Initialize();
            rewardTrackStateController.Initialize();
            rewardTrackStatePopupController.Initialize();
            lavaQuestStateController.Initialize();
            lavaQuestEventPopupController.Initialize();
            lavaQuestStartPopupController.Initialize();            
            freePaidOfferService.Initialize();
            await freePaidOfferUiController.InitializeAsync();
            tutorialService.Initialize();   
            playerProfileController.Initialize();
            
            await UniTask.WaitUntil(() => 
            { 
                return !(Advertisement.IsGdprApplicable() ?? false)
                    || Advertisement.GetGdprStatus(); 
            });

            await competitionManager.InitializeAsync(startCts.Token);
            scoringVisualizeService.Initialize();
            //TODO continue from here

            SetSystemsActive(); //in the end
        }

        private async UniTask DeinitializeSystems()
        {
            AudioService.I.Deinitialize();
            HapticService.I.Deinitialize();
            BroTweenService.I.Deinitialize();

            SayKitApp.OnRemoteConfigUpdated -= SayKitApp_OnRemoteConfigUpdated;

            systemsRegistry.Dispose();
            appInterruptObserver.Interrupt -= AppInterruptObserver_Interrupt;
            sceneLoadController.OnProcessBegin -= SceneLoaderScoped_OnProcessBegin;
            compositeDisposable.Dispose();
            startCts.Cancel();
            startCts.Dispose();

            mailService.Deinitialize();
            leaderboardService.Deinitialize();
            socialService.Deinitialize();
            dateTimeService.Deinitialize();
            timeCyclesService.Deinitialize();
            configProvider.Deinitialize();
            await sceneLoadController.DeinitializeAsync();
            poolService.Deinitialize();
            assetProvider.Deinitialize();
            appSettingsController.Deinitialize();
            popupService.Deinitialize();
            adsFlyingTextsService.Deinitialize();
            warningService.Deinitialize();
            currencyHudService.Deinitialize();
            currencyHudFxService.Deinitialize();
            currencyRewardDisplayService.Deinitialize();
            widgetManager.Deinitialize();
            boostersManager.Deinitialize();
            purchaseOfferContainer.Deinitialize();
            purchaseManager.Deinitialize();            
            shopUiController.Deinitialize();
            teamUiController.Deinitialize();
            noAdsUiController.Deinitialize();
            freePaidOfferService.Deinitialize();
            freePaidOfferUiController.Deinitialize();
            superDiscountUiController.Deinitialize();
            purchaseUiController.Deinitialize();
            lifeController.Deinitialize();
            lifeUiController.Deinitialize();
            recommendationUiManager.Deinitialize();
            winStreakStateController.Deinitialize();
            bannerController.Deinitialize();
            rewardTrackStateController.Deinitialize();
            rewardTrackStatePopupController.Deinitialize();
            lavaQuestStateController.Deinitialize();
            lavaQuestEventPopupController.Deinitialize();
            lavaQuestStartPopupController.Deinitialize();
            tutorialService.Deinitialize();
            playerProfileController.Deinitialize();
            levelService.Deinitialize();
            competitionManager.Deinitialize();
            scoringVisualizeService.Deinitialize();
            //TODO continue from here

#if PR_CHEAT
            cheatService.Deinitialize();
#endif

        }

        private void SetSystemsActive()
        {
            systemsRegistry.SetActive(ownerScope);
        }

        private void ConstructSingletons()
        {
            LocalizationService.I.Construct(configProvider.LocalizationConfig, configProvider.SystemSettingsConfig);
            HapticService.I.Construct(configProvider.HapticsCustomPresetsConfig);
            BroTweenService.I.Construct(configProvider.BroTweenConfig);
        }

        private void InitializeLoadScreen()
        {
            loadScreenController.Initialize();
        }

        private void InitializeInput()
        {
            inputService.Initialize();
            inputService.DisableInput();
        }

        private void InitializeUiInput()
        {
            inputUiService.Initialize();
            inputUiService.EnableInput();
        }

        private void InitializeProgress()
        {
            saveStorage.Initialize();
            systemsRegistry.Load(ownerScope);
        }

        private void SaveProgress()
        {
            systemsRegistry.PrepareSave();
            saveStorage.Save();
        }

        private void ShowLoadScreen()
        {
            loadScreenController.Open(LoadScreenUI.Reason.LoadScene, false);
        }

        private async UniTask LoadScreenPlayFakeProgreeAsync()
        {
            float random = UnityEngine.Random.Range(2.5f, 3f);
            float random07 = random * 0.7f;
            loadScreenController.PlayProgressToStageAsync(random07, LoadScreenUI.Stage07, startCts.Token, true).Forget();
            await UniTask.WaitForSeconds(random07, true, cancellationToken: startCts.Token);
            await UniTask.WaitUntil(() => saveStorage.IsInitialized);
            if (saveStorage.Progress.gameState.levelIdx != 0 || !configProvider.SystemSettingsConfig.SettingsData.haveFirstLoadProlongation)
            {
                loadScreenController.PlayProgressToStageAsync((random - random07) * 0.5f, LoadScreenUI.Stage08, startCts.Token).Forget();
                await UniTask.WaitForSeconds((random - random07) * 0.5f, true, cancellationToken: startCts.Token);
                loadScreenController.PlayProgressToStageAsync((random - random07) * 0.5f, LoadScreenUI.Stage10, startCts.Token).Forget();
                await UniTask.WaitForSeconds((random - random07) * 0.5f, true, cancellationToken: startCts.Token);
            }
        }

        private void AppInterruptObserver_Interrupt()
        {
            SaveProgress();
        }

        private void SceneLoaderScoped_OnProcessBegin(string sceneName, LoadSceneMode sceneMode)
        {
            SaveProgress();
            poolService.ReleaseAllToPool();
            BroTweenService.I.CompleteBetweenScenes();

            currencyHudService.SetMode(CurrencyHudMode.HideAll);
        }

        private void SayKitApp_OnRemoteConfigUpdated()
        {
            sceneLoadController.ScheduleRemoteUpdate();
        }
    }
}