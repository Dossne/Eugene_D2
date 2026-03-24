using System;
using Infrastructure.HapticControl;
using UnityEngine;

namespace Features.InfoPopup
{
    [Serializable]
    public class IPShowPlaceAnimationParams
    {
        [Header("All animations")]
        public float startDelaySec;

        [Header("First phase. Show")]
        public float itemDelaySec;
        public AnimationCurve showScaleXCurve;
        public AnimationCurve showScaleYCurve;
        public float showScaleDurationSec;

        [Header("Second phase. Move down")]
        public float moveDurationSec;
        public Vector3 startOffsetPos;
        public AnimationCurve moveCurve;

        [Header("Second phase. Haptics on end move")]
        public bool doHaptics;
        public HapticType hapticType = HapticType.Selection;
        public int hapticCount;
        
        [Header("Second phase. Bounce")]
        public float bounceDurationSec;
        public AnimationCurve bounceScaleXCurve;
        public AnimationCurve bounceScaleYCurve;
    }
}