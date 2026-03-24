using System;
using UnityEngine;

namespace Features.WinStreak
{
    [Serializable]
    public class WinStreakUIBannerParams
    {
        public float delayBeforeAnimations;
        public AnimationCurve moveCurve;
        public float moveDuration;
        public Vector2 moveFromPosition;
    }
}