using Features.LevelComplete;
using R3;

namespace Features.Events
{
    public class LevelFinishEvent : ReactiveCommand
    {
        public LoseReason reason;
        public bool isWin;
        public int specialItemsCount;
        public int specialItemsCountMultiplied;
        public int winStreakMultiplier;
        public int levelDifficultyMultiplier;

        public void ExecuteWithWin(int specialItemsCount, int specialItemsCountMultiplied, int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            reason = LoseReason.None;
            isWin = true;

            this.specialItemsCount = specialItemsCount;
            this.specialItemsCountMultiplied = specialItemsCountMultiplied;
            this.winStreakMultiplier = winStreakMultiplier;
            this.levelDifficultyMultiplier = levelDifficultyMultiplier;

            Execute(Unit.Default);
        }

        public void ExecuteWithFail(LoseReason reason)
        {
            this.reason = reason;
            isWin = false;
            specialItemsCount = 0;
            specialItemsCountMultiplied = 0;
            Execute(Unit.Default);
        }
    }
}