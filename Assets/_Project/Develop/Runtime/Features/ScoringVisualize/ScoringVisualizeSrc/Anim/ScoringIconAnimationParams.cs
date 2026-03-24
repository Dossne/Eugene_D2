using System;
using UnityEngine;

namespace Features.ScoringVisualize
{
    [Serializable]
    public class ScoringIconAnimationParams
    {
        [Header("Show. Fade")]
        public float fadeInDuration;
        public AnimationCurve fadeInEase;

        [Header("Show. Bounce")]
        public float bounceDuration;
        public AnimationCurve scaleCurveX;
        public AnimationCurve scaleCurveY;
        
        [Header("Stay")]
        public float stayDuration;

        [Header("Move")]
        public float moveDuration = 1f;
        public AnimationCurve movementEase = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve verticalOffsetCurve = AnimationCurve.Linear(0, 0, 1, 0);
        public float offsetVerticalMulti;
    }
}