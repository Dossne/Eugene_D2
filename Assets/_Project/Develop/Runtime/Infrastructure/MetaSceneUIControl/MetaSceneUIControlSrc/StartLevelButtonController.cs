using Features.Level;
using Features.LevelComplete;
using Features.Life;
using Features.LifeUi;
using Features.WinStreak;
using Infrastructure.MainUICanvasControl;
using R3;
using System;

namespace Features.HudLevelButtons
{
    public struct StartLevelArgs
    {
        public int levelNumber;
        public LevelDifficulty difficulty;
    }

    public class StartLevelButtonController
    {
        public Subject<StartLevelArgs> OnStartClicked = new();

        private readonly StartLevelButtonView startLevelButtonView;
        private readonly LevelService levelService;
        private readonly LifeController lifeController;
        private readonly LifeUiController lifeUiController;
        private readonly WinStreakStateController winStreakStateController;
        private bool isInit;

        public StartLevelButtonController(MainUIProvider mainUIProvider,
                                          LevelService levelService,
                                          LifeController lifeController,
                                          LifeUiController lifeUiController,
                                          WinStreakStateController winStreakStateController)
        {
            this.startLevelButtonView = mainUIProvider.HudProvider.StartLevelButtonView;
            this.levelService = levelService;
            this.lifeController = lifeController;
            this.lifeUiController = lifeUiController;
            this.winStreakStateController = winStreakStateController;
        }

        public void Initialize()
        {
            if (isInit)
                return;

            startLevelButtonView.Construct(levelService.CurrentLevelNumber,
                                           levelService.GetCurrentLevelData().difficulty,
                                           winStreakStateController.RewardMultiplier,
                                           winStreakStateController.IsMaxLevel);

            startLevelButtonView.Initialize();
            EnableButtonAction();
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            startLevelButtonView.Deinitialize();
            DisableButtonAction();
            isInit = false;
        }

        public void EnableButtonAction()
        {
            startLevelButtonView.OnClicked += StartLevelButtonView_OnClicked;
        }

        public void DisableButtonAction()
        {
            startLevelButtonView.OnClicked -= StartLevelButtonView_OnClicked;
        }

        public void HandleStartLevelClick()
        {
            if (lifeController.IsInfiniteLife()
             || lifeController.LifeAmount > 0)
                OnStartClicked.OnNext(new StartLevelArgs { levelNumber = levelService.CurrentLevelNumber, difficulty = levelService.GetCurrentLevelData().difficulty });
            else
                lifeUiController.ShowBuyLifePopup();
        }

        private void StartLevelButtonView_OnClicked()
        {
            HandleStartLevelClick();
        }
    }
}