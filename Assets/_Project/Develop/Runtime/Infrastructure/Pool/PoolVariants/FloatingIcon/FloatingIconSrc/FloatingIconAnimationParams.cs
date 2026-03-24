using System;
using TriInspector;
using UnityEngine;

namespace Infrastructure.Pool.FloatingIcon
{
    [Serializable]
    public class FloatingIconAnimationParams
    {
        [Title("Move")]
        public float moveDuration = 1f;
        public AnimationCurve movementEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Space]
        public AnimationCurve movementCurveX = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve horizontalOffsetCurve = AnimationCurve.Linear(0, 0, 1, 0);
        public float offsetHorizontalMulti = -150;

        [Space]
        public AnimationCurve movementCurveY = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve verticalOffsetCurve = AnimationCurve.Linear(0, 0, 1, 0);
        public float offsetVerticalMulti;

        [Title("Scale")]
        public float scaleDuration = 1f;
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Title("Rotate")]
        public float rotateSpeedZ = 0.0f;
        public float rotateDuration = 1f;
        public AnimationCurve rotateEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }
}