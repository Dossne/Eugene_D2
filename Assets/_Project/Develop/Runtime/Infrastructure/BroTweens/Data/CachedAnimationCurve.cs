using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Precalculated curve with less resolution than AnimationCurve, faster, but takes approx 1KB GC Alloc each
    /// </summary>
    [Serializable]
    public sealed class CachedAnimationCurve
    {
        private readonly float[] samples;
        private readonly int resolutionMinus1;
        
        /// <param name="curve">AnimationCurve</param>
        /// <param name="resolution">sample count: 128=>256=>512</param>
        public CachedAnimationCurve(AnimationCurve curve, int resolution = 256)
        {
            if (resolution < 2)
                resolution = 2;

            samples = new float[resolution];
            resolutionMinus1 = resolution - 1;

            for (int i = 0; i < resolution; i++)
            {
                float t = (float)i / resolutionMinus1;
                samples[i] = curve.Evaluate(t);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Evaluate(float t)
        {
            if (t <= 0f)
                return samples[0];

            if (t >= 1f)
                return samples[resolutionMinus1];

            float scaled = t * resolutionMinus1;
            int idx = (int)scaled;

            float a = samples[idx];
            float b = samples[idx + 1];

            return a + (b - a) * (scaled - idx);
        }
    }
}