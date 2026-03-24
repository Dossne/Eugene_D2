using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class PositionTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private Transform target;
        [TriInspector.ShowInInspector] private Vector3 from;
        [TriInspector.ShowInInspector] private Vector3 to;


        public PositionTween SetParams(Transform target, in Vector3 to, in float duration)
        {
            return SetParams(target, target.position, in to, in duration);
        }


        public PositionTween SetParams(Transform target, in Vector3 from, in Vector3 to, in float duration)
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
            target.position = Vector3.LerpUnclamped(from, to, t);
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            from = default;
            to = default;
        }
    }
}