using System;
using UnityEngine;

namespace Features.InfoPopup
{
    [Serializable]
    public class IPScaleAnimationParams
    {
        public float startDelaySec;
        public AnimationCurve curve;
        public float durationSec;
    }
    
    [CreateAssetMenu(fileName = "IPScaleAnimationGlobal", menuName = "Config/Animations/IPScaleAnimationGlobal")]
    public class IPScaleAnimationGlobal : ScriptableObject
    {
        public IPScaleAnimationParams param;
    }
}