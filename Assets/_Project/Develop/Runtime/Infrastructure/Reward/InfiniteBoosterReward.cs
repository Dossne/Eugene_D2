using System;
using Features.Boosters;

namespace Infrastructure.Reward
{
    [Serializable]
    public class InfiniteBoosterReward
    {
        public BoosterType boosterType;
        public int timeLengthMinutes;
        public bool isDisplayTimeText;
        public bool isDisplayRibbon;
        public bool isDisplayInfinityIcon;
    }
}