using System;
using UnityEngine;

namespace Features.RewardTrack
{
    [Serializable]
    public class RewardTrackStateUiElementAnimationParams
    {
        [Header("Root scale")]
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 1, 1, 1);
        public float rootBounceDuration = 0.15f;
        
        [Header("Lock Rotation")]
        public AnimationCurve rotateCurve = AnimationCurve.Linear(0f, 0, 1, 0);
        public float lockRotateDuration = 1f;
        public float maxZAngle = 35f;
    }
}