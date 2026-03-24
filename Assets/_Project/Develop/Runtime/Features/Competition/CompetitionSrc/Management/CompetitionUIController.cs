using System.Threading;
using Cysharp.Threading.Tasks;
using Features.HudLevelButtons;
using Features.PlayerProfile;
using Features.ScoringVisualize;
using Features.Social;
using Features.Warnings;
using Features.Widgets;
using Infrastructure.Configs;
using Infrastructure.CurrencyHud;
using Infrastructure.Popups;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.SystemsLifeCycle;

namespace Features.Competition
{
    /// <summary>
    /// Meta scene
    /// </summary>
    public class CompetitionUIController : ISystemTickable
    {
        private readonly CompetitionConfig config;
        private readonly RewardVisualDataService rewardVisualDataService;
        private readonly RewardApplyController rewardApplyController;
        private readonly CompetitionManager competitionManager;
        private readonly PopupService popupService;
        private readonly CurrencyHudService currencyHudService;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly WidgetManager widgetManager;
        private readonly StartLevelButtonController startLevelButtonController;
        private readonly WarningService warningService;
        private readonly ScoringVisualizeService scoringVisualizeService;
        private readonly PlayerProfileController playerProfileController;
        private readonly UserDataUpdateEvent userDataUpdateEvent;

        private CompetitionRewardController rewardController;
        private CompetitionUIDataController uiDataController;
        private CompetitionStatePopupController statePopupController;
        private CompetitionRewardPopupController rewardPopupController;
        private CompetitionWidgetController widgetController;

        private bool isInit;

        public CompetitionUIController(ConfigProvider configProvider,
                                       RewardVisualDataService rewardVisualDataService,
                                       RewardApplyController rewardApplyController,
                                       CompetitionManager competitionManager,
                                       PopupService popupService,
                                       CurrencyHudService currencyHudService,
                                       SpriteAtlasService spriteAtlasService,
                                       WidgetManager widgetManager,
                                       StartLevelButtonController startLevelButtonController,
                                       WarningService warningService,
                                       ScoringVisualizeService scoringVisualizeService,
                                       PlayerProfileController  playerProfileController,
                                       UserDataUpdateEvent userDataUpdateEvent)
        {
            this.config = configProvider.CompetitionConfig;
            this.rewardVisualDataService = rewardVisualDataService;
            this.rewardApplyController = rewardApplyController;
            this.competitionManager = competitionManager;
            this.popupService = popupService;
            this.currencyHudService = currencyHudService;
            this.spriteAtlasService = spriteAtlasService;
            this.widgetManager = widgetManager;
            this.startLevelButtonController = startLevelButtonController;
            this.warningService = warningService;
            this.scoringVisualizeService = scoringVisualizeService;
            this.playerProfileController = playerProfileController;
            this.userDataUpdateEvent = userDataUpdateEvent;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;

            if(!competitionManager.IsFeatureEnabled())
                return;
            
            CreateDependencies();
            await competitionManager.EnterQualificationIfStateIsNone(cancellationToken);
            rewardController.Initialize();
            await uiDataController.InitializeAsync(cancellationToken);
            statePopupController.Initialize();
            rewardPopupController.Initialize();
            await widgetController.InitializeAsync(cancellationToken);

            competitionManager.OnTimeChanged += CompetitionManager_OnTimeChanged;

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            uiDataController.Deinitialize();
            rewardController.Deinitialize();
            statePopupController.Deinitialize();
            rewardPopupController.Deinitialize();
            widgetController.Deinitialize();

            competitionManager.OnTimeChanged -= CompetitionManager_OnTimeChanged;

            isInit = false;
        }

        void ISystemTickable.Tick()
        {
            if (competitionManager.IsInterrupted())
                return;

            widgetController.Tick();
        }

        public UniTask ExecuteScheduledAsync(CancellationToken cancellationToken)
        {
            return widgetController.ExecuteScheduledAsync(cancellationToken);
        }

        private void CreateDependencies()
        {
            CreateRewardController();
            CreateUiDataController();
            CreateStatePopupController();
            CreateRewardPopupController();
            CreateWidgetController();
        }

        private void CreateRewardController()
        {
            rewardController = new CompetitionRewardController(config.RewardTracks, 
                                                               config.ServerRewards, 
                                                               rewardVisualDataService,
                                                               rewardApplyController, 
                                                               competitionManager, 
                                                               competitionManager.GetCalculator());
        }

        private void CreateUiDataController()
        {
            uiDataController = new CompetitionUIDataController(competitionManager, rewardController, scoringVisualizeService, ScoringFeature.Competition);
        }

        private void CreateStatePopupController()
        {
            statePopupController = new CompetitionStatePopupController(competitionManager, 
                                                                       rewardController, 
                                                                       uiDataController, 
                                                                       popupService, 
                                                                       currencyHudService,
                                                                       rewardVisualDataService, 
                                                                       spriteAtlasService, 
                                                                       playerProfileController, 
                                                                       userDataUpdateEvent,
                                                                       config.Multipliers, 
                                                                       config.Feature.leaderboardCount, 
                                                                       config.Feature.isMultiplierEnabled);
        }

        private void CreateRewardPopupController()
        {
            rewardPopupController = new CompetitionRewardPopupController(competitionManager, rewardController, statePopupController, uiDataController, popupService);
        }

        private void CreateWidgetController()
        {
            widgetController = new CompetitionWidgetController(competitionManager, 
                                                               statePopupController, 
                                                               uiDataController, 
                                                               widgetManager,
                                                               popupService, 
                                                               startLevelButtonController,
                                                               warningService, 
                                                               WidgetId.Competition,
                                                               "CompetitionWidget");
        }

        private void CompetitionManager_OnTimeChanged()
        {
            statePopupController.ResyncTimer();
            widgetController.ResyncWidget();
        }
    }
}