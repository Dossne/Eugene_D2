using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class RectSizeDeltaTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private RectTransform target;
        [TriInspector.ShowInInspector] private Vector2 from;
        [TriInspector.ShowInInspector] private Vector2 to;


        public RectSizeDeltaTween SetParams(RectTransform target, in Vector2 to, in float duration)
        {
            return SetParams(target, target.sizeDelta, in to, in duration);
        }


        public RectSizeDeltaTween SetParams(RectTransform target, in Vector2 from, in Vector2 to, in float duration)
        {
            base.SetDefaultParams(in duration);
            this.target = target;
            this.from = from;
            this.to = to;
            return this;
        }


        protected override void OnTick(in float dt)
        {
            float t = GetProgress();
            target.sizeDelta = Vector2.LerpUnclamped(from, to, t);
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = default;
            to = default;
        }
    }
}