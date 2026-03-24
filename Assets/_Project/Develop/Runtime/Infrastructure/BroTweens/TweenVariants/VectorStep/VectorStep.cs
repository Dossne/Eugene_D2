using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public struct VectorStep
    {
        public Vector3 target;
        public float duration;
    }
}