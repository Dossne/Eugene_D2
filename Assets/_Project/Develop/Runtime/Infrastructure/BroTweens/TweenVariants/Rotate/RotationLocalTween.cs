using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Rotate transform in local space.
    /// Have 2 rotate options: Shortest (750°: rotates only 30°) and Full (750°: rotates 2 times by 360° + 30°)
    /// </summary>
    [Serializable]
    public sealed class RotationLocalTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] private Transform target;
        [TriInspector.ShowInInspector] private Vector3 vFrom;
        [TriInspector.ShowInInspector] private Vector3 vTo;
        [TriInspector.ShowInInspector] private Quaternion qFrom;
        [TriInspector.ShowInInspector] private Quaternion qTo;
        [TriInspector.ShowInInspector] private RotateMode mode;


        public RotationLocalTween SetParams(Transform target, in Vector3 to, in float duration, RotateMode mode = RotateMode.Shortest)
        {
            SetParams(target, target.localEulerAngles, in to, in duration, mode);
            return this;
        }


        public RotationLocalTween SetParams(Transform target, in Vector3 from, in Vector3 to, in float duration, RotateMode mode = RotateMode.Shortest)
        {
            base.SetDefaultParams(duration);
            this.target = target;
            switch (mode)
            {
                case RotateMode.Full:
                    this.vFrom = from;
                    this.vTo = to;
                    break;
                case RotateMode.Shortest:
                    qFrom = Quaternion.Euler(from);
                    qTo = Quaternion.Euler(to);
                    break;
            }
            this.mode = mode;
            return this;
        }

        public RotationLocalTween SetParams(Transform target, in Quaternion to, in float duration)
        {
            base.SetDefaultParams(duration);
            this.target = target;
            qFrom = target.localRotation;
            qTo = to;
            this.mode = RotateMode.Shortest;
            return this;
        }


        public RotationLocalTween SetParams(Transform target, in Quaternion from, in Quaternion to, in float duration)
        {
            base.SetDefaultParams(duration);
            this.target = target;
            qFrom = from;
            qTo = to;
            this.mode = RotateMode.Shortest;
            return this;
        }
        
        protected override void OnTick(in float dt)
        {
            float t = GetProgress();

            if (mode == RotateMode.Shortest)
            {
                target.localRotation = Quaternion.LerpUnclamped(qFrom, qTo, t);
            }
            else
            {
                target.localEulerAngles = Vector3.LerpUnclamped(vFrom, vTo, t);
            }
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            vFrom = default;
            vTo = default;
            qFrom = default;
            qTo = default;
        }
    }
}