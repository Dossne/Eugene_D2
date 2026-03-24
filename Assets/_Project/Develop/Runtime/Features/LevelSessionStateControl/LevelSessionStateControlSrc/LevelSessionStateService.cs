using Features.Level;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;

namespace Features.LevelSessionStateControl
{
    //Main scope
    public class LevelSessionStateService : ISavable
    {
        private readonly LevelService levelService;
        private readonly LevelSessionStateControlConfiguration levelSessionStateControlConfiguration;
        private LevelSessionState saveState;


        public LevelSessionStateService(LevelService levelService, ConfigProvider configProvider)
        {
            this.levelService = levelService;
            this.levelSessionStateControlConfiguration = configProvider.LevelSessionStateControlConfiguration;
        }


        public SessionStateType CurrentState => saveState.state;
        

        public void Load(Progress progress)
        {
            saveState = progress.levelSessionState;
        }


        public void Save(Progress progress)
        {
        }

        public bool LevelSessionStateEnabled => levelSessionStateControlConfiguration.IsFeatureUnlocked();

        public bool NeedLoadGameLevel()
        {
            return LevelSessionStateEnabled && IsGameLevelInProcess();
        }


        public bool IsGameLevelInProcess()
        {
            return levelService.CurrentLevelNumber == saveState.lastLevelNumber
                && saveState.state is SessionStateType.InProcess or SessionStateType.PendingResurrect;
        }


        public bool IsGameLevelLoose(int levelNumber)
        {
            if (!LevelSessionStateEnabled)
                return saveState.state is not SessionStateType.Win;

            return levelNumber == saveState.lastLevelNumber
                && levelService.CurrentLevelNumber == saveState.lastLevelNumber
                && saveState.state is SessionStateType.Loose;
        }
    }
}