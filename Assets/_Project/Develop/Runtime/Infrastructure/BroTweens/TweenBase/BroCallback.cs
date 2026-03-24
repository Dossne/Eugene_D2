using System;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class BroCallback : BroTweenBase
    {
        public void SetParams(in Action callback, in float delay = 0)
        {
            base.OnComplete_Internal(in callback);
            base.WithDelay_Internal(in delay);
        }


        public void SetParams<T>(T target, in Action<T> onComplete, in float delay = 0) where T : class
        {
            base.OnComplete_Internal(target, in onComplete);
            base.WithDelay_Internal(in delay);
        }


        protected override void OnTick(in float dt) { }


        protected override void OnClearParamsWhenReturnToPool() { }
    }
}