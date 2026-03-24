using System;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public class SliderMoveTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private Slider target;
        [TriInspector.ShowInInspector] private float from;
        [TriInspector.ShowInInspector] private float to;

        public SliderMoveTween SetParams(Slider target, in float from, in float to, in float duration)
        {
            base.SetDefaultParams(duration);
            this.target = target;
            this.from = from;
            this.to = to;
            return this;
        }


        protected override void OnTick(in float dt)
        {
            float t = GetProgress();
            target.value = Mathf.LerpUnclamped(from, to, t);
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = 0;
            to = 0;
        }
    }
}