using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class PunchPositionTween : VectorStepReachTween
    {
        /// <summary>
        /// <see cref="SetParams(Transform, in Vector3, Vector3, in float, in bool, in int, in float)"/>.
        /// </summary>
        public PunchPositionTween SetParams(Transform target, in Vector3 direction, in float duration, in bool isUiElement, in int countPerSec = 10, in float elasticity = 1f)
        {
            SetParams(target, target.localPosition, direction, in duration, in isUiElement, in countPerSec, in elasticity);
            return this;
        }

        /// <summary>
        /// Randomly move target to calculated local positions side by side (like punch)
        /// </summary>
        /// <param name="target">target transform</param>
        /// <param name="defaultLocalPos">default local position of target transform</param>
        /// <param name="direction">direction to punch</param>
        /// <param name="duration">length of tween</param>
        /// <param name="isUiElement">For ui element z should be 0 for correct gpu batching</param>
        /// <param name="countPerSec">count times per second</param>
        /// <param name="elasticity">if 0 then magnitude sets between [defaultLocalPos...direction] - more soft.
        /// If 1 then magnitude sets between [-direction...direction] - more powerful </param>
        /// <returns>self</returns>
        public PunchPositionTween SetParams(Transform target, in Vector3 defaultLocalPos, Vector3 direction, in float duration, in bool isUiElement, in int countPerSec = 10, in float elasticity = 1f)
        {
            if (isUiElement)
            {
                direction.z = 0;
            }

            VectorStepsCalculator.GetRandomPunches(scheduledSteps, in defaultLocalPos, in direction, in duration, in countPerSec,  elasticity);
            SetParams_Internal(target);
            return this;
        }

        /// <summary>
        /// Set precalculated target points for controlled shake
        /// </summary>
        public PunchPositionTween SetParams(Transform target, List<VectorStep> steps)
        {
            SetParams_Internal(target, steps);
            return this;
        }

        protected override void ApplyResult(float progress)
        {
            var result = Vector3.LerpUnclamped(from, to, progress);
            target.localPosition = result;
        }

        protected override Vector3 GetCurrentState()
        {
            return target.localPosition;
        }
    }
}