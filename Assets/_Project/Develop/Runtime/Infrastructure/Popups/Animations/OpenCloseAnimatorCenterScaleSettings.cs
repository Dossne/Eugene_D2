using System;
using UnityEngine;

namespace Infrastructure.Popups
{
    [Serializable]
    public class OpenCloseAnimatorCenterScaleSettings
    {
        [Header("Settings open scale")]
        [Min(0)]public float openDuration = 0.2f;
        public AnimationCurve openScaleCurve;
        [Min(0)] public float bgFadeDuration = 0.5f;

        [Header("Settings close scale")]
        public AnimationCurve closeScaleCurve;
        [Min(0)] public float closeDuration = 0.13f;
        [Min(0)] public float bgFadeOutDuration;

        [Header("Settings alpha")]
        [Min(0)] public float alphaMin = 0f;
    }
}