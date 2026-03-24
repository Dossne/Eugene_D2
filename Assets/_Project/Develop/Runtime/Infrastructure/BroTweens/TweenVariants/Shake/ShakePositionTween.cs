using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.BroTweens
{

    [Serializable]
    public sealed class ShakePositionTween : VectorStepReachTween
    {
        /// <summary>
        /// <see cref="SetParams(Transform, in Vector3, Vector3, in float, in bool, in int, in float , in bool, in bool)"/>.
        /// </summary>
        public ShakePositionTween SetParams(Transform target, in Vector3 strength, in float duration, in bool isUiElement, in int countPerSec = 10, in float angleRandomness = 90f, in bool fadeOut = true,
                                            in bool isFullRandomness = true)
        {
            SetParams(target, target.localPosition, strength, in duration, in isUiElement, in countPerSec, in angleRandomness, in fadeOut, in isFullRandomness);
            return this;
        }

        /// <summary>
        /// Randomly move target to calculated local positions
        /// </summary>
        /// <param name="target">target transform</param>
        /// <param name="duration">length of tween</param>
        /// <param name="defaultLocalPos">default local position of target transform</param>
        /// <param name="strength">max move positions by x, y, z</param>
        /// <param name="isUiElement">For ui element z should be 0 for correct gpu batching</param>
        /// <param name="countPerSec">count times per second</param>
        /// <param name="angleRandomness">move angle</param>
        /// <param name="fadeOut"> fade out at end of tween</param>
        /// <param name="isFullRandomness">if false then magnitude sets between [defaultLocalPos...randomness] - more soft.
        /// If true then magnitude sets between [-randomness...randomness] - more powerful </param>
        /// <returns>self</returns>
        public ShakePositionTween SetParams(Transform target, in Vector3 defaultLocalPos, Vector3 strength, in float duration, in bool isUiElement, in int countPerSec = 10,
                                            in float angleRandomness = 90f, in bool fadeOut = true, in bool isFullRandomness = true)
        {
            if (isUiElement)
            {
                strength.z = 0;
            }

            VectorStepsCalculator.GetRandomShakes(scheduledSteps, in defaultLocalPos, in duration,  strength, in countPerSec, in angleRandomness, in fadeOut, in isFullRandomness);
            SetParams_Internal(target, duration);
            return this;
        }

        /// <summary>
        /// Set precalculated target points for controlled shake
        /// </summary>
        public ShakePositionTween SetParams(Transform target, List<VectorStep> steps)
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