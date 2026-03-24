using Features.Level;
using Infrastructure.Configs;

namespace Features.MainMenuUnlock
{
    public class MainMenuUnlockInfoService
    {
        private readonly int unlockLevelNumber;
        private readonly LevelService levelService;


        public MainMenuUnlockInfoService(ConfigProvider configProvider, LevelService levelService)
        {
            this.levelService = levelService;
            this.unlockLevelNumber = configProvider.MainMenuUnlockConfig.Item.unlockLevelNumber;
        }


        public bool IsUnlocked => levelService.CurrentLevelNumber >= unlockLevelNumber;
    }
}