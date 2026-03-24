using System;
using UnityEngine;

namespace Features.ProgressBar
{
    [Serializable]
    public class NormalizedProgressBarImageParams
    {
        [Header("Slider params")]
        [Range(0, 1)] public float minValue;
        [Range(0, 1)] public float maxValue;

        [Header("Animations params")]
        public float moveDuration;
        public AnimationCurve moveCurve;
    }
}