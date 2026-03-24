using Features.Events;
using Features.Level;
using Features.LevelSessionStateControl;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using Infrastructure.SpriteAtlasControl;
using R3;
using UnityEngine;
using Progress = Infrastructure.PersistentProgress.Progress;

namespace Features.WinStreak
{
    public class WinStreakStateController : ISavable
    {
        private readonly WinStreakConfig winStreakConfig;
        private readonly LevelStartedEvent levelStartedEvent;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly LevelService levelService;
        private readonly LevelSessionStateService levelSessionStateService;

        private readonly CompositeDisposable disposable;

        private WinStreakState saveState;
        private int winStreakMaxLevel;
        private bool isIncremented;
        private bool isInit;


        public WinStreakStateController(ConfigProvider configProvider,
                                        LevelStartedEvent levelStartedEvent,
                                        LevelFinishEvent levelFinishEvent,
                                        SpriteAtlasService spriteAtlasService,
                                        LevelService levelService,
                                        LevelSessionStateService levelSessionStateService)
        {
            winStreakConfig = configProvider.WinStreakConfig;
            this.levelStartedEvent = levelStartedEvent;
            this.levelFinishEvent = levelFinishEvent;
            this.spriteAtlasService = spriteAtlasService;
            this.levelService = levelService;
            this.levelSessionStateService = levelSessionStateService;
            this.disposable = new CompositeDisposable();
        }


        public bool IsFeatureEnabled => winStreakConfig.WinStreakFeature.isFeatureEnabled;
        public int CurrentLevel => saveState.winStreakLevel;
        public int MaxLevel => winStreakMaxLevel;
        public int UnlockLevel => winStreakConfig.WinStreakFeature.unlockLevel;
        public bool IsUnlocked => levelService.CurrentLevelNumber >= UnlockLevel;
        public bool IsMaxLevel => CurrentLevel == MaxLevel;
        public bool IsIncremented => isIncremented;
        public bool IsMultiplierEnabled => winStreakConfig.WinStreakFeature.isMultiplierEnabled;
        public int RewardMultiplier => IsMaxLevel && winStreakConfig.WinStreakFeature.isMultiplierEnabled ? winStreakConfig.WinStreakFeature.rewardMultiplier : 1;


        void ISavable.Load(Progress progress)
        {
            saveState = progress.gameState.winSteak;
        }


        void ISavable.Save(Progress progress)
        {
        }


        public void Initialize()
        {
            if (isInit)
                return;

            levelStartedEvent.Subscribe(_ => LevelStartedEventHandle(levelStartedEvent.LevelNumber)).AddTo(disposable);
            levelFinishEvent.Subscribe(_ => HandleMultiplierOnLoose(levelFinishEvent.isWin)).AddTo(disposable);

            winStreakMaxLevel = winStreakConfig.GetMaxWinStreakLevel();

            if (IsGameLevelFailed())
            {
                ResetWinStreakLevel();
            }

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposable.Dispose();
            isInit = false;
        }


        public (Sprite icon, int level) GetWinStreakData(int level)
        {
            var data = winStreakConfig.GetWinStreakData(level);
            return (spriteAtlasService.GetFromMain(data.iconName), data.lvl);
        }


        public void HandleWin()
        {
            if (!IsFeatureEnabled || levelService.CurrentLevelNumber + 1 < UnlockLevel)
                return;

            IncrementLevel();
        }


        private void IncrementLevel()
        {
            if (CurrentLevel >= MaxLevel)
            {
                saveState.winStreakLevel = MaxLevel;
                isIncremented = false;
            }
            else
            {
                saveState.winStreakLevel++;
                saveState.levelBeginCount = 0;
                isIncremented = true;
            }
        }


        private void ResetLevel()
        {
            saveState.winStreakLevel = 0;
            isIncremented = false;
        }

        private void ResetWinStreakLevel()
        {
            saveState.winStreakLevel = 0;
            saveState.levelBeginCount = 0;
        }


        private bool IsGameLevelFailed()
        {
            return levelSessionStateService.IsGameLevelLoose(saveState.startedLevelNumber) && saveState.levelBeginCount > 0;
        }
        

        private void HandleMultiplierOnLoose(bool isWin)
        {
            if (isWin || !IsFeatureEnabled)
                return;

            ResetLevel();
        }


        private void LevelStartedEventHandle(int levelNumber)
        {
            if (!IsFeatureEnabled)
                return;

            if (saveState.startedLevelNumber < levelNumber)
                saveState.levelBeginCount = 1;
            else
                saveState.levelBeginCount++;

            saveState.startedLevelNumber = levelNumber;
        }


#region Cheats

#if PR_CHEAT
        public void CheatAdd()
        {
            IncrementLevel();
        }


        public void CheatReset()
        {
            ResetLevel();
        }
#endif

#endregion


    }
}