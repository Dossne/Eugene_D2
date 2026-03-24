using System;
using UnityEngine;

namespace Features.InfoPopup
{
    [Serializable]
    public class IPChangeTextByIntAnimationParams
    {
        public float startDelaySec;

        public float changeDuration;
        public AnimationCurve changeCurve;

        public float bounceDuration;
        public AnimationCurve bounceCurve;
    }
}