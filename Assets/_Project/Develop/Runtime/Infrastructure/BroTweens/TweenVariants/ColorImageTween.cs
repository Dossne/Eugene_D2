using System;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class ColorImageTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private Image target;
        [TriInspector.ShowInInspector] private Color from;
        [TriInspector.ShowInInspector] private Color to;


        public ColorImageTween SetParams(Image target, in Color to, in float duration)
        {
            return SetParams(target, target.color, to, duration);
        }


        public ColorImageTween SetParams(Image target, in Color from, in Color to, in float duration)
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
            target.color = Color.Lerp(from, to, t);
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = default;
            to = default;
        }
    }
}