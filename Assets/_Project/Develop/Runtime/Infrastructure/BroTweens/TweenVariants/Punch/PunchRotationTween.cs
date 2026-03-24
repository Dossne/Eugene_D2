using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Similar to ShakeRotationTween
    /// </summary>
    [Serializable]
    public sealed class PunchRotationTween : VectorStepReachTween
    {
        /// <summary>
        /// <see cref="SetParams(Transform, in Vector3, in Vector3, in float, in int, in float)"/>.
        /// </summary>
        public PunchRotationTween SetParams(Transform target, in Vector3 direction, in float duration, in int countPerSec = 10, in float elasticity = 1f)
        {
            SetParams(target, target.localEulerAngles, in direction, in duration, in countPerSec, in elasticity);
            return this;
        }

        /// <summary>
        /// Randomly rotates target to calculated local positions side by side (like punch)
        /// </summary>
        /// <param name="target">target transform</param>
        /// <param name="defaultLocalRotation">default local position of target transform</param>
        /// <param name="direction">direction to punch</param>
        /// <param name="duration">length of tween</param>
        /// <param name="countPerSec">count times per second</param>
        /// <param name="elasticity">if 0 then magnitude sets between [defaultLocalRotation...direction] - more soft.
        /// If 1 then magnitude sets between [-direction...direction] - more powerful </param>
        /// <returns>self</returns>
        public PunchRotationTween SetParams(Transform target, in Vector3 defaultLocalRotation, in Vector3 direction, in float duration, in int countPerSec = 10, in float elasticity = 1f)
        {
            VectorStepsCalculator.GetRandomPunches(scheduledSteps, in defaultLocalRotation, in direction, in duration, in countPerSec, elasticity);
            SetParams_Internal(target, duration);
            return this;
        }

        /// <summary>
        /// Set precalculated target points for controlled shake
        /// </summary>
        public PunchRotationTween SetParams(Transform target, List<VectorStep> steps)
        {
            SetParams_Internal(target, steps);
            return this;
        }

        protected override void ApplyResult(float progress)
        {
            target.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(from), Quaternion.Euler(to), progress);
        }

        protected override Vector3 GetCurrentState()
        {
            return target.localEulerAngles;
        }
    }
}