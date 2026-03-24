using Infrastructure.Ads;

namespace Features.RewardTrack
{
    public class RewardTrackAnalytics
    {
        private const string RewardReceived = "reward_track_reward_received";
        
        public static void SendRewardReceived(int rewardTrackId, int stepNumber, bool isLast)
        {
            int last = isLast ? 1 : 0;
            AnalyticSender.SendEvent(RewardReceived, rewardTrackId, stepNumber, last, null);
        }
    }
}