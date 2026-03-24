using System;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class SliderImageMoveTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private Image target;
        [TriInspector.ShowInInspector] private float from;
        [TriInspector.ShowInInspector] private float to;

        public SliderImageMoveTween SetParams(Image target, in float from, in float to, in float duration)
        {
            if (target.fillAmount > to)
                target.fillAmount = 0;

            base.SetDefaultParams(duration);
            this.target = target;
            this.from = from;
            this.to = to;
            return this;
        }


        protected override void OnTick(in float dt)
        {
            float t = GetProgress();
            target.fillAmount = Mathf.LerpUnclamped(from, to, t);
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = 0;
            to = 0;
        }
    }
}