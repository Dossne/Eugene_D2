using R3;

namespace Features.Events
{
    public class LevelStartedEvent : ReactiveCommand
    {
        public string LevelId { get; private set; }
        public int LevelNumber { get; private set; }
        public int WinStreakLevel { get; private set; }


        public void Execute(string levelId, int levelNumber, int winStreakLevel)
        {
            LevelNumber = levelNumber;
            LevelId = levelId;
            WinStreakLevel = winStreakLevel;
            base.Execute(Unit.Default);
        }
    }
}