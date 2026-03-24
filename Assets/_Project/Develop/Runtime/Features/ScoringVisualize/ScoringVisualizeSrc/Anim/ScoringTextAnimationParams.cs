using System;
using UnityEngine;

namespace Features.ScoringVisualize
{
    [Serializable]
    public class ScoringTextAnimationParams
    {
        [Header("Show")]
        public AnimationCurve fadeInEase;
        public float fadeInDuration;

        [Header("Stay")]
        public float stayDuration;

        [Header("Hide. Move")]
        public Vector3 hidePosOffset;
        public AnimationCurve hideMoveEase;
        public float hideMoveDuration;

        [Header("Hide. FadeOut")]
        public AnimationCurve fadeOutEase;
        public float fadeOutDuration;
    }
}