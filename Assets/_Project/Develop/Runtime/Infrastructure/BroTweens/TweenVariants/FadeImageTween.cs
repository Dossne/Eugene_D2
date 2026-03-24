using System;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class FadeImageTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private Image target;
        [TriInspector.ShowInInspector] private float from;
        [TriInspector.ShowInInspector] private float to;


        public FadeImageTween SetParams(Image target, in float to, in float duration)
        {
            return SetParams(target, target.color.a, to, duration);
        }


        public FadeImageTween SetParams(Image target, in float from, in float to, in float duration)
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
            var c = target.color;
            c.a = Mathf.Lerp(from, to, t);
            target.color = c;
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = 0;
            to = 0;
        }
    }
}