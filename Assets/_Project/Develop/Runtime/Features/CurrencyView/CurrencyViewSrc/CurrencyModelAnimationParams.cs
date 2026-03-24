using System;
using UnityEngine;


namespace Features.CurrencyView
{
    [Serializable]
    public class CurrencyModelAnimationParams
    {
        [Header("Move")]
        public float moveDuration = 1f;
        public AnimationCurve movementEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve curveY = AnimationCurve.Linear(0, 0, 1, 1);


        [Header("Scale")]
        public float scaleDuration = 1f;
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1, 1, 1);


        [Header("Rotate")]
        public float rotateDuration = 1f;
        public AnimationCurve rotateEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float rotateSpeedY = 0.0f;
    }
}
