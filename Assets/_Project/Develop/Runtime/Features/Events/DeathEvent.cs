using Features.LevelComplete;
using R3;

namespace Features.Events
{
    public class DeathEvent : ReactiveCommand
    {
        public LoseReason reason;
        
        public void ExecuteWithBomb()
        {
            reason = LoseReason.Bomb;
            Execute(Unit.Default);
        }
        
        public void ExecuteWithTime()
        {
            reason = LoseReason.Timer;
            Execute(Unit.Default);
        }
    }
}