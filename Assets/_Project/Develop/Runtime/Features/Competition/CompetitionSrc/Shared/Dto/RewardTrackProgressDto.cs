using Infrastructure.Reward;

namespace Features.Competition
{
    public class RewardTrackProgressDto
    {
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


