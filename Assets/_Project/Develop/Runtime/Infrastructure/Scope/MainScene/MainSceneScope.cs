using Features.Events;
using Infrastructure.WalletSystem;
using Infrastructure.Ads;
using Infrastructure.ApplicationInterrupt;
using Infrastructure.AssetManagement;
using Infrastructure.CameraControl;
using Infrastructure.Configs;
using Infrastructure.CurrencyHud;
using Infrastructure.Events;
using Infrastructure.InputControl;
using Infrastructure.LoadScreen;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PauseControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Pool;
using Infrastructure.Popups;
using Infrastructure.QualityGraphicsControl;
using Infrastructure.SceneManagement;
using Infrastructure.Settings;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.SystemModules;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.WelcomeBonus;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Features.Boosters;
using Features.Warnings;
using Features.Widgets;
using Features.ShopUi;
using Infrastructure.PurchaseSystem;
using Features.CurrencyRewardDisplay;
using Features.Life;
using Features.LifeUi;
using Features.Level;
using Features.NoAdsUi;
using Features.PurchaseUi;
using Features.RecommendationUi;
using Features.WinStreak;
using Features.RateUs;
using Features.Banner;
using Features.Competition;
using Features.RewardTrack;
using Features.SuperDiscountUi;
using Infrastructure.DateTimeControl;
using Infrastructure.Reward;
using Infrastructure.TimeCycles;
using Features.LavaQuest;
using Features.Tutorial;
using Features.FreePaidOffer;
using Features.LevelSessionStateControl;
using Features.MainMenuUnlock;
using Infrastructure.TooltipControl;
using Features.SuperSpeedMode;
using Features.PlayerProfile;
using Features.ScoringVisualize;
using Features.Social;
using Features.Team;


namespace Infrastructure.Scope
{
    public class MainSceneScope : LifetimeScope
    {
        [SerializeField] private PoolService poolService;
        [SerializeField] private ConfigProvider configProvider;
        [SerializeField] private MainUIProvider uiProvider;
        [SerializeField] private InputService inputService;
        [SerializeField] private AppInterruptObserver appInterruptObserver;
        [SerializeField] private CameraService cameraService;
        [SerializeField] private Overlay3dCameraService overlay3dCameraService;
        [SerializeField] private Transform instantiatorItemsRoot;
        [SerializeField] private CurrencyHudFxService currencyHudFxService;


        protected override void Configure(IContainerBuilder builder)
        {
            {   //scene dependencies
                builder.RegisterComponent(poolService);
                builder.RegisterComponent(configProvider);
                builder.RegisterComponent(cameraService);
                builder.RegisterComponent(overlay3dCameraService);
                builder.RegisterComponent(uiProvider);
                builder.RegisterComponent(inputService);
                builder.RegisterComponent(appInterruptObserver);
                builder.RegisterComponent(currencyHudFxService);
            }
            
            {   //systems
                builder.Register<SystemsRegistry>(Lifetime.Singleton).WithParameter(this);
                builder.Register<InterstitialService>(Lifetime.Singleton);
                builder.Register<SdkService>(Lifetime.Singleton);
                builder.Register<BuildInformationService>(Lifetime.Singleton);
                builder.Register<LoadScreenController>(Lifetime.Singleton);
                builder.Register<GraphicsQualityHandler>(Lifetime.Singleton);
                builder.Register<ShadowsController>(Lifetime.Singleton);
                builder.Register<MainUICanvasHandler>(Lifetime.Singleton);
                builder.Register<PauseService>(Lifetime.Singleton);
                builder.Register<AppSettingsController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<DateTimeService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<TimeCyclesService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            }

            {   //Social
                builder.Register<SocialService>(Lifetime.Singleton);
                builder.Register<LeaderboardService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<LeaderboardNakamaClient>(Lifetime.Singleton);
                builder.Register<LeaderboardNakamaClientWrapper>(Lifetime.Singleton);
                builder.Register<MailService>(Lifetime.Singleton);
                builder.Register<MailReceivedEvent>(Lifetime.Singleton);
                builder.Register<UserAuthenticateEvent>(Lifetime.Singleton);
                builder.Register<UserDataUpdateEvent>(Lifetime.Singleton);
                builder.Register<LeaderboardCachedEvent>(Lifetime.Singleton);
                builder.Register<LeaderboardOnGetStateStartEvent>(Lifetime.Singleton);
                builder.Register<LeaderboardRequestEvent>(Lifetime.Singleton);
            }

            builder.Register<SaveStorage>(Lifetime.Singleton);
            builder.Register<SceneLoadController>(Lifetime.Singleton).WithParameter(this);
            builder.Register<AssetDownloadReporter>(Lifetime.Singleton);
            builder.Register<AddressableDownloadService>(Lifetime.Singleton);
            builder.Register<InputUiService>(Lifetime.Singleton);

            {
                //Independent for each scene for correct resources CleanUp (Addressables, Resources)
                builder.Register<AssetProvider>(Lifetime.Scoped);
                builder.Register<Instantiator>(Lifetime.Scoped).WithParameter(instantiatorItemsRoot);
            }
            {
                builder.Register<PopupService>(Lifetime.Singleton).WithParameter("popupsParent", uiProvider.PopupRoot).WithParameter("screenParent", uiProvider.ScreenRoot);
                builder.Register<PopupFactory>(Lifetime.Singleton);
            }
            builder.Register<SpriteAtlasService>(Lifetime.Singleton);
            builder.Register<AppBaseAnalytics>(Lifetime.Singleton);
            builder.Register<AnalyticsContextCreator>(Lifetime.Singleton);
            {
                builder.Register<AdsFlyingTextsService>(Lifetime.Singleton);
                builder.Register<AdsFlyTextEvent>(Lifetime.Singleton);
            }

            builder.Register<LevelSessionStateService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<Wallet>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            {
                builder.Register<WarningService>(Lifetime.Singleton);
                builder.Register<CurrencyHudService>(Lifetime.Singleton);
                builder.Register<CurrencyRewardDisplayService>(Lifetime.Singleton);
            }

            builder.Register<LevelService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LifeController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LifeUiController>(Lifetime.Singleton);

            {
                builder.Register<PurchaseOfferFactory>(Lifetime.Singleton);
                builder.Register<PurchaseOfferContainer>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                RegisterPurchaseManager(builder);
                builder.Register<RewardApplyController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<RewardVisualDataService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            }

            builder.Register<PurchaseUiController>(Lifetime.Singleton);
            builder.Register<ShopUiController>(Lifetime.Singleton);
            builder.Register<TeamUiController>(Lifetime.Singleton);
            builder.Register<NoAdsUiController>(Lifetime.Singleton);
            builder.Register<SuperDiscountUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<RateUsController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<BannerController>(Lifetime.Singleton);
            builder.Register<RecommendationUiManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            {
                builder.Register<WidgetFactory>(Lifetime.Singleton);
                builder.Register<WidgetManager>(Lifetime.Singleton);
            }

            builder.Register<FirstLaunchHandler>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<BoostersManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            {
                //events
                builder.Register<SoftResetEvent>(Lifetime.Singleton);
                builder.Register<LevelStartedEvent>(Lifetime.Singleton);
                builder.Register<LevelFinishEvent>(Lifetime.Singleton);
                builder.Register<DeathEvent>(Lifetime.Singleton);
                builder.Register<LevelProgressChangeEvent>(Lifetime.Singleton);
                builder.Register<LevelProgressChangeRequestEvent>(Lifetime.Singleton);
            }

            builder.Register<WinStreakStateController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            {
                //reward track
                builder.Register<RewardTrackStateController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<RewardTrackStatePopupController>(Lifetime.Singleton);
                builder.Register<RewardTrackRequestEvent>(Lifetime.Singleton);
            }

            { //lava quest
                builder.Register<LavaQuestStateController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<LavaQuestStartPopupController>(Lifetime.Singleton);
                builder.Register<LQConnectPlayersPopupController>(Lifetime.Singleton);
                builder.Register<LavaQuestEventPopupController>(Lifetime.Singleton);
            }


            { //Free Paid Offer
                builder.Register<FreePaidOfferService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<FreePaidOfferUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            }

            builder.Register<TooltipService>(Lifetime.Singleton);
            builder.Register<TaskCompleteTutorialEvent>(Lifetime.Singleton);
            builder.Register<TutorialTriggerEvent>(Lifetime.Singleton);
            builder.Register<TutorialFactory>(Lifetime.Singleton);
            builder.Register<TutorialService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<SuperSpeedController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<MainMenuUnlockInfoService>(Lifetime.Singleton);

            {//Player Profile
                builder.Register<PlayerProfileController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            }

            {
                builder.Register<CompetitionManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<CompetitionStatePopupController>(Lifetime.Singleton);
            }


            builder.Register<ScoringVisualizeService>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<MainSceneFlow>().WithParameter(this);
            
#if PR_CHEAT
            builder.Register<Cheat.CheatService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
#endif
        }

        private void RegisterPurchaseManager(IContainerBuilder builder)
        {
#if PR_SAYKIT_ENABLED
            builder.Register<PurchaseManagerSaykit>(Lifetime.Singleton).AsImplementedInterfaces();
#else
            builder.Register<PurchaseManagerMock>(Lifetime.Singleton).AsImplementedInterfaces();
#endif
        }
    }
}