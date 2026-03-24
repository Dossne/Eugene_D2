using Features.LevelUp;
using R3;

namespace Infrastructure.Events
{
    //Request to LevelUpManager to set character level
    public class LevelProgressChangeRequestEvent : ReactiveCommand<LevelProgressArgs>
    {
        public void Request(LevelProgressArgs args)
        {
            base.Execute(args);
        }
    }
}