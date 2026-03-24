using Features.LevelUp;
using R3;

namespace Features.Events
{
    public class LevelProgressChangeEvent : ReactiveCommand<LevelProgressArgs>
    {
        public void Request(LevelProgressArgs args)
        {
            base.Execute(args);
        }
    }
}