using System;
using UnityEngine;

namespace Features.WinStreak
{
    [Serializable]
    public class WinStreakBoosterFlyParams
    {
        [Header("Move")]
        public float moveDuration;
        public AnimationCurve curveY;
        public AnimationCurve moveEaseCurve;

        [Header("Scale")]
        public Vector3 startScale;
        public float scaleDuration;
        public AnimationCurve scaleCurve;

        [Header("Rotation")]
        public float rotateDuration;
        public float rotateDirection = 90f;
    }
}