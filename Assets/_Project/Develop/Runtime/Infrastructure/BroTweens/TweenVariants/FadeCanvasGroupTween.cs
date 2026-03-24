using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class FadeCanvasGroupTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private CanvasGroup target;
        [TriInspector.ShowInInspector] private float from;
        [TriInspector.ShowInInspector] private float to;


        public FadeCanvasGroupTween SetParams(CanvasGroup target, in float to, in float duration)
        {
            return SetParams(target, target.alpha, to, duration);
        }


        public FadeCanvasGroupTween SetParams(CanvasGroup target, in float from, in float to, in float duration)
        {
            base.SetDefaultParams(duration);
            this.target = target;
            this.from = Mathf.Clamp01(from);
            this.to = Mathf.Clamp01(to);
            return this;
        }


        protected override void OnTick(in float dt)
        {
            float t = GetProgress();
            target.alpha = Mathf.Lerp(from, to, t);
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = 0;
            to = 0;
        }
    }
}