using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class PositionLocalTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private Transform target;
        [TriInspector.ShowInInspector] private Vector3 from;
        [TriInspector.ShowInInspector] private Vector3 to;


        public PositionLocalTween SetParams(Transform target, in Vector3 to, in float duration)
        {
            return SetParams(target, target.localPosition, in to, in duration);
        }


        public PositionLocalTween SetParams(Transform target, in Vector3 from, in Vector3 to, in float duration)
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
            target.localPosition = Vector3.LerpUnclamped(from, to, t);
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = default;
            to = default;
        }
    }
}