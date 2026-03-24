using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Life
{
    [CreateAssetMenu(fileName = "LifeConfiguration", menuName = "Config/Game/LifeConfiguration")]
    public class LifeConfiguration : ScriptableObject
    {
        public int lifeCooldownSec;
        public int defaultMaxCount;
        public int lifePriceCoin;
        public int unspendableLastLifeLevel;
    }
}