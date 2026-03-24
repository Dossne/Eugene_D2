using System;
using TriInspector;
using UnityEngine;

namespace Features.RewardTrack
{
    [Serializable]
    public class ProgressSliderAnimationParams
    {
        public AnimationCurve moveCurve;
        public float moveDuration = 1f;
        public bool isDurationProportional;
        [ShowIf("isDurationProportional")] public float minDurationProportional = 0.25f;
    }
}