using Features.Collectables;
using Infrastructure.Reward;
using UnityEngine;

namespace Features.RewardTrack
{
    public class ProgressDto
    {
        public CollectableType targetItemType;
        public Sprite targetIcon;
        public Sprite targetResurrectIcon;
        public int collectedCountOnStart;
        public int collectedCountOnEnd;
        public int targetCountByConfig;

        //Analytics params
        public int stepNumber;
        public bool isLast;
        public int configIdx;

        //Reward params
        public ComplexReward reward;
        public RewardItemVisualData rewardVisual;


        public bool CanGiveReward()
        {
            return collectedCountOnEnd == targetCountByConfig;
        }
    }
}