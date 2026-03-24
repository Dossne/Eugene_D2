using System;
using TMPro;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class ColorTextTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private TMP_Text target;
        [TriInspector.ShowInInspector] private Color from;
        [TriInspector.ShowInInspector] private Color to;


        public ColorTextTween SetParams(TMP_Text target, in Color to, in float duration)
        {
            return SetParams(target, target.color, to, duration);
        }


        public ColorTextTween SetParams(TMP_Text target, in Color from, in Color to, in float duration)
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