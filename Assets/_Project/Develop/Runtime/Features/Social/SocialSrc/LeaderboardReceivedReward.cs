using Infrastructure.Reward;
using System;

namespace Features.Social
{
    [Serializable]
    public class LeaderboardReceivedReward
    {
        public string letterId;
        public long mailCode;
        public int score;
        public int place;
        public ComplexReward rewards;
        public DateTime CreatedAt;



        public bool IsClaimed { get; private set; } = false;



        public void SetClaimed()
        {
            IsClaimed = true;
        }
    }
}