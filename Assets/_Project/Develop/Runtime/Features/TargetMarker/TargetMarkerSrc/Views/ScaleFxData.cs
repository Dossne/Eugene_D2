using System;
using UnityEngine;

namespace Features.TargetMarker
{
    [Serializable]
    public class ScaleFxData
    {
        public float duration = 1f;
        public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 1);
    }
}