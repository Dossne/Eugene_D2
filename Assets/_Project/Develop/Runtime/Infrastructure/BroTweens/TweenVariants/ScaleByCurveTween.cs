using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Change scale by curve according to defaultScale
    /// </summary>
    [Serializable]
    public sealed class ScaleByCurveTween : BroTweenBase
    {
        private AnimationCurve curve;
        [TriInspector.ShowInInspector] private Transform target;
        [TriInspector.ShowInInspector] private Vector3 defaultScale;

        public ScaleByCurveTween SetParams(Transform target, in float duration, AnimationCurve curve)
        {
            SetParams(target, Vector3.one, in duration, curve);
            return this;
        }

        public ScaleByCurveTween SetParams(Transform target, in Vector3 defaultScale, in float duration, AnimationCurve curve)
        {
            base.SetDefaultParams(in duration);
            this.target = target;
            this.defaultScale = defaultScale;
            this.curve = curve;
            return this;
        }

        protected override void OnTick(in float dt)
        {
            float t = GetProgress();
            target.localScale = defaultScale * curve.Evaluate(t);
        }

        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            curve = null;
            defaultScale = default;
        }
    }
}