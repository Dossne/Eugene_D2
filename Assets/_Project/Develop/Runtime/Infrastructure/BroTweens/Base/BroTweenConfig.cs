using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public class ButtonBounceParams
    {
        public AnimationCurve curve;
        public float duration;
        public float callbackDelay;
    }

    [CreateAssetMenu(fileName = "BroTweenConfig", menuName = "Config/System/BroTweenConfig")]
    public class BroTweenConfig : ScriptableObject
    {
        public ButtonBounceParams buttonBounce;
        public int activeUidsStartCapacity = 500;
        public int activeScaledStartCapacity = 500;
        public int activeUnscaledStartCapacity = 100;

        private Dictionary<Type, (int startCapacity, int preWarmCount)> poolParams = new()
        {
            { typeof(BroSequence), (startCapacity: 30, preWarmCount: 30) },
            { typeof(BroSequenceItem), (startCapacity: 30, preWarmCount: 30) },
            { typeof(ScaleByCurveTween), (startCapacity: 500, preWarmCount: 500) },
            { typeof(FloatTween), (startCapacity: 10, preWarmCount: 10) },
        };

        public (int startCapacity, int preWarmCount) GetPoolParams(Type key)
        {
            if (poolParams.TryGetValue(key, out var poolParam))
            {
                return (poolParam.startCapacity, poolParam.preWarmCount);
            }

            return (0, 0);
        }
    }
}