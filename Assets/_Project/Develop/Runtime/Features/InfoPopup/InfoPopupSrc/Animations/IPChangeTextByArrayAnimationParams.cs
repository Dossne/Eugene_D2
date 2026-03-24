using System;
using UnityEngine;

namespace Features.InfoPopup
{
    [Serializable]
    public class IPChangeTextByArrayAnimationParams
    {
        [Header("ChangeText")]
        public float startDelaySec;
        public string[] texts;
        public float changeTextDuration;
        public AnimationCurve changeCurve;

        [Header("Bounce")]
        public IPScaleAnimationParams[] bounces;
    }
}