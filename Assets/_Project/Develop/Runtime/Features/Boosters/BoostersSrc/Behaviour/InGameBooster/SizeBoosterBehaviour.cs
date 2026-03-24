using Features.Boosters.Model;
using Features.Events;
using Features.LevelUp;
using Infrastructure.Utilities;

namespace Features.Boosters.Behaviour.InGameBooster
{
    public sealed class SizeBoosterBehaviour : InGameBoosterBehaviour
    {
        private readonly LevelProgressChangeEvent levelEvent;


        public SizeBoosterBehaviour(InGameBoosterModelBase model, InGameBoosterSlotView view, LevelProgressChangeEvent levelEvent) : base(model, view)
        {
            this.levelEvent = levelEvent;
        }


        protected override void ActionsOnApply()
        {
            levelEvent.Request(new LevelProgressArgs(Source.SizeBooster, ActionType.Add, 0, (int)model.BoostedValue));
        }


        protected override void ActionsOnUnapply()
        {
            levelEvent.Request(new LevelProgressArgs(Source.SizeBooster, ActionType.Remove, 0, (int)model.BoostedValue));
        }
    }
}