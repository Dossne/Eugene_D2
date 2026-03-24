using Features.Boosters.Model;
using Infrastructure.AudioControl;

namespace Features.Boosters.Behaviour.InGameBooster
{
    public abstract class InGameBoosterBehaviour
    {
        protected readonly BoosterModel model;
        private readonly InGameBoosterSlotView view;
        private float currentTime;


        protected InGameBoosterBehaviour(BoosterModel model, InGameBoosterSlotView view)
        {
            this.model = model;
            this.view = view;
        }


        public bool IsOff => currentTime <= 0;
        public float CurrentTime => currentTime;
        public BoosterModel Model => model;


        public void Tick(float dt)
        {
            currentTime -= dt;
            view.SetProgressSlider(currentTime / model.DurationSec);
            ActionsOnTick(dt);
        }


        public void Apply()
        {
            this.currentTime = model.DurationSec;
            model.Decrease();
            view.SetButtonEnabled(false);
            view.StartShakeIcon();
            AudioService.I.PlaySfx(SfxType.BoosterButton);
            ActionsOnApply();
        }


        public void ApplyRestore(float time)
        {
            this.currentTime = time;
            view.SetButtonEnabled(false);
            view.StartShakeIcon();
            ActionsOnApply();
        }


        public void Unapply()
        {
            view.SetButtonEnabled(true);
            view.StopShakeIcon();
            ActionsOnUnapply();
        }


        public virtual void Deinitialize(){}
        protected virtual void ActionsOnTick(float dt) {}

        protected abstract void ActionsOnApply();
        protected abstract void ActionsOnUnapply();

    }
}