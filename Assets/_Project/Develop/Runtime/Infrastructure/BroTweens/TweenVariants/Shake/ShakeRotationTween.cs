using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Similar to PunchRotationTween
    /// </summary>
    [Serializable]
    public sealed class ShakeRotationTween : VectorStepReachTween
    {
        /// <summary>
        /// <see cref="SetParams(Transform, in Vector3, in Vector3, in float, in int, in float, in bool, in bool)"/>.
        /// </summary>
        public ShakeRotationTween SetParams(Transform target, in Vector3 strength, in float duration, in int countPerSec = 10, in float angleRandomness = 90f, in bool fadeOut = true,
                                            in bool isFullRandomness = true)
        {
            SetParams(target, target.localEulerAngles, in strength, in duration, in countPerSec, in angleRandomness, in fadeOut, in isFullRandomness);
            return this;
        }


        /// <summary>
        /// Randomly rotates target to calculated local positions
        /// </summary>
        /// <param name="target">target transform</param>
        /// <param name="defaultLocalRotation">default local rotation of target transform</param>
        /// <param name="strength">max move positions by x, y, z</param>
        /// <param name="duration">length of tween</param>
        /// <param name="countPerSec">count times per second</param>
        /// <param name="angleRandomness">rotate angle</param>
        /// <param name="fadeOut"> fade out at end of tween</param>
        /// <param name="isFullRandomness">if false then magnitude sets between [defaultLocalRotation...randomness] - more soft.
        /// If true then magnitude sets between [-randomness...randomness] - more powerful </param>
        /// <returns>self</returns>
        public ShakeRotationTween SetParams(Transform target, in Vector3 defaultLocalRotation, in Vector3 strength, in float duration, in int countPerSec = 10,
                                            in float angleRandomness = 90f, in bool fadeOut = true, in bool isFullRandomness = true)
        {
            VectorStepsCalculator.GetRandomShakes(scheduledSteps, in defaultLocalRotation, in duration, strength, in countPerSec, in angleRandomness, in fadeOut,
                                                  in isFullRandomness);
            SetParams_Internal(target, duration);
            return this;
        }


        /// <summary>
        /// Set precalculated target points for controlled shake
        /// </summary>
        public ShakeRotationTween SetParams(Transform target, List<VectorStep> steps)
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