using Features.Boosters;
using Features.Collectables;
using Features.LavaQuest;
using Features.Level;
using Features.PlayerProfile;
using Features.RewardTrack;
using Features.SuperSpeedMode;
using Features.WinStreak;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.TooltipControl;
using VContainer;


namespace Features.Tutorial
{
    public class TutorialFactory
    {
        private readonly ConfigProvider configProvider;
        private readonly WinStreakStateController winStreakStateController;
        private readonly RewardTrackStateController rewardTrackStateController;
        private readonly PopupService popupService;
        private readonly TooltipService tooltipService;
        private readonly MainUIProvider mainUIprovider;
        private readonly SceneLoadController sceneLoadController;
        private readonly LevelService levelService;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly InputService inputService;
        private readonly InputUiService inputUiService;
        private readonly TaskCompleteTutorialEvent taskCompleteTutorialEvent;
        private readonly LavaQuestStateController lavaQuestStateController;
        private readonly SuperSpeedController superSpeedController;
        private readonly PlayerProfileController playerProfileController;

        [Inject]
        public TutorialFactory(PopupService popupService,
                               TooltipService tooltipService,
                               MainUIProvider mainUIProvider,
                               ConfigProvider configProvider,
                               SceneLoadController sceneLoadController,
                               LevelService levelService,
                               WinStreakStateController winStreakStateController,
                               RewardTrackStateController rewardTrackStateController,
                               SpriteAtlasService spriteAtlasService,
                               InputService inputService,
                               InputUiService inputUiService,
                               TaskCompleteTutorialEvent taskCompleteTutorialEvent, 
                               LavaQuestStateController lavaQuestStateController,
                               SuperSpeedController superSpeedController,
                               PlayerProfileController playerProfileController)
        {
            this.popupService = popupService;
            this.tooltipService = tooltipService;
            this.mainUIprovider = mainUIProvider;
            this.sceneLoadController = sceneLoadController;
            this.levelService = levelService;
            this.spriteAtlasService = spriteAtlasService;
            this.inputService = inputService;
            this.inputUiService = inputUiService;
            this.configProvider = configProvider;
            this.winStreakStateController = winStreakStateController;
            this.rewardTrackStateController = rewardTrackStateController;
            this.taskCompleteTutorialEvent = taskCompleteTutorialEvent;
            this.lavaQuestStateController = lavaQuestStateController;
            this.superSpeedController = superSpeedController;
            this.playerProfileController = playerProfileController;
        }

        public TutorialBase CreateBoosterPopupTutorial(string tutorialId, BoosterType boosterType) 
        {
            return new BoosterPopupTutorial(tutorialId,
                                            popupService,
                                            levelService,
                                            sceneLoadController,
                                            spriteAtlasService,
                                            configProvider.BoosterConfig.InGameBoosters.Find(x => x.type == boosterType));
        }

        public TutorialBase CreateBoosterInGameTutorial(string tutorialId, BoosterType boosterType)
        {
            return new BoosterInGameTutorial(tutorialId,
                                             mainUIprovider,                                             
                                             sceneLoadController,
                                             levelService,
                                             tooltipService,
                                             configProvider.BoosterConfig.InGameBoosters.Find(x => x.type == boosterType),
                                             configProvider.TutorialConfiguration.GetInGameBoosterUseData(boosterType));
        }

        public TutorialBase CreatePreBoosterTutorial(string tutorialId, BoosterType boosterType)
        {
            return new PreBoosterTutorial(tutorialId,
                                          popupService,
                                          mainUIprovider,
                                          sceneLoadController,
                                          levelService,
                                          inputUiService,
                                          configProvider.BoosterConfig.PreBoosters.Find(x => x.type == boosterType),
                                          configProvider.TutorialConfiguration.GetPreBoosterUseData(boosterType));
        }

        public TutorialBase CreateSuperSpeedWidgetTutorial(string tutorialId)
        {
            return new SuperSpeedWidgetTutorial(tutorialId,
                                                popupService,
                                                mainUIprovider,
                                                sceneLoadController,
                                                levelService,
                                                inputUiService,
                                                configProvider.SuperSpeedConfig.SuperSpeedData,
                                                configProvider.TutorialConfiguration.GetSuperSpeedWidgetData());
        }

        public TutorialBase CreateSuperSpeedTooltipTutorial(string tutorialId)
        {
            return new SuperSpeedTooltipTutorial(tutorialId,
                                                 sceneLoadController,
                                                 tooltipService,
                                                 mainUIprovider, 
                                                 inputService,
                                                 superSpeedController,
                                                 configProvider.CustomTooltipConfiguration);
        }

        public TutorialBase CollectablePopupTutorial(string tutorialId, CollectableType collectableType, string descriptionKey)
        {
            return new CollectablePopupTutorial(tutorialId,
                                                popupService,
                                                levelService,
                                                sceneLoadController,
                                                spriteAtlasService,
                                                configProvider.CollectablesConfig,
                                                collectableType,
                                                descriptionKey);
        }

        public TutorialBase CreateMoveTooltipTutorial(string tutorialId)
        {
            return new MoveTooltipTutorial(tutorialId,
                                           sceneLoadController,
                                           tooltipService,
                                           mainUIprovider,
                                           inputService,
                                           levelService,
                                           configProvider.CustomTooltipConfiguration);
        }

        public TutorialBase CreateEatTooltipTutorial(string tutorialId)
        {
            return new EatTooltipTutorial(tutorialId,
                                          sceneLoadController,
                                          tooltipService,
                                          mainUIprovider,                                          
                                          inputService,
                                          levelService,
                                          spriteAtlasService,
                                          configProvider,
                                          taskCompleteTutorialEvent);
        }

        public TutorialBase CreateFinishLevelTooltipTutorial(string tutorialId)
        {
            return new FinishLevelTooltipTutorial(tutorialId,
                                                  sceneLoadController,
                                                  tooltipService,
                                                  mainUIprovider,
                                                  levelService,
                                                  inputService,
                                                  configProvider.CustomTooltipConfiguration);
        }

        public TutorialBase CreateWinStreakUnlockTutorial(string tutorialId)
        {
            return new WinStreakUnlockTutorial(tutorialId,
                                               popupService,
                                               mainUIprovider,
                                               levelService,
                                               inputUiService,
                                               winStreakStateController,
                                               sceneLoadController,
                                               configProvider.TutorialConfiguration.GetWinStreakUnlockData());
        }


        public TutorialBase CreateRewardTrackUnlockTutorial(string tutorialId)
        {
            return new RewardTrackUnlockTutorial(tutorialId,
                                                 inputUiService,
                                                 popupService,
                                                 mainUIprovider,
                                                 levelService,
                                                 rewardTrackStateController,
                                                 sceneLoadController,
                                                 configProvider.TutorialConfiguration.GetRewardTrackUnlockData());
        }
        
        
        public TutorialBase CreateLavaQuestStartTutorial(string tutorialId)
        {
            return new LavaQuestStartTutorial(tutorialId,
                                              inputUiService,
                                              popupService,
                                              sceneLoadController,
                                              configProvider.TutorialConfiguration.GetLavaQuestStartData(),
                                              mainUIprovider.MaskTutorialPanel,
                                              lavaQuestStateController);
        }

        public TutorialBase CreatePlayerProfileTutorial(string tutorialId)
        {
            return new PlayerProfileTutorial(tutorialId,
                                             mainUIprovider,
                                             inputUiService,
                                             popupService,
                                             sceneLoadController,
                                             configProvider,
                                             spriteAtlasService,
                                             mainUIprovider.MaskTutorialPanel,
                                             playerProfileController);
        }
    }
}