using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Reward
{
    [System.Serializable]
    public class RewardContainerVisualData
    {
        public List<RewardItemVisualData> rewards;
        public List<RewardItemVisualData> containerRewards;
        public bool haveContainer;
        public Sprite containerIcon;
    }
}