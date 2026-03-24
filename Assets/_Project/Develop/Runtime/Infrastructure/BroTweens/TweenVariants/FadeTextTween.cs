using System;
using TMPro;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class FadeTextTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private TMP_Text target;
        [TriInspector.ShowInInspector] private float from;
        [TriInspector.ShowInInspector] private float to;


        public FadeTextTween SetParams(TMP_Text target, in float to, in float duration)
        {
            return SetParams(target, target.alpha, to, duration);
        }


        public FadeTextTween SetParams(TMP_Text target, in float from, in float to, in float duration)
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