using Infrastructure.Reward;
using UnityEngine;

namespace Features.SeasonPass
{
    public class SlotViewData
    {
        public int stepIdx;
        public SlotViewType viewType;
        public SlotViewStateType stateType;
        public RewardItemVisualData rewardInfo;
        public Transform tooltipTarget;
    }
}