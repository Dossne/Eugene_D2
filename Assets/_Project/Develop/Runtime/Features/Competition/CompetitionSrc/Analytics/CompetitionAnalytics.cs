using System.Text;
using Infrastructure.Ads;
using Infrastructure.Reward;
using Infrastructure.Utilities;

namespace Features.Competition
{
    public class CompetitionAnalytics
    {
        private readonly AnalyticsContextCreator analyticsContextCreator;
        private readonly RewardApplyController rewardApplyController;
        private static readonly StringBuilder sb = new();
        private string featureTag;
        private bool isDebugLog;

        private string availableEvent = "competition_available";
        private string popupAvailableEvent = "competition_popup_available";
        private string startedEvent = "competition_started";
        private string rewardAvailableEvent = "competition_reward_available";
        private string rewardClaimedEvent = "competition_reward_claimed";
        private string finishedEvent = "competition_finished";

        public CompetitionAnalytics(AnalyticsContextCreator analyticsContextCreator, RewardApplyController rewardApplyController, string featureTag, bool isDebugLog)
        {
            this.analyticsContextCreator = analyticsContextCreator;
            this.rewardApplyController = rewardApplyController;
            this.featureTag = featureTag;
            this.isDebugLog = isDebugLog;
        }

        public void SendCompetitionAvailable(int startCountTimes)
        {
            AnalyticSender.SendEvent(availableEvent, startCountTimes, analyticsContextCreator.MainContext);

            if (isDebugLog)
                UnityEngine.Debug.Log($"{featureTag} Track: {availableEvent}. StartCountTimes {startCountTimes}");
        }

        public void SendStartPopupAvailable(int startCountTimes)
        {
            AnalyticSender.SendEvent(popupAvailableEvent, startCountTimes, analyticsContextCreator.MainContext);

            if (isDebugLog)
                UnityEngine.Debug.Log($"{featureTag} Track: {popupAvailableEvent}. StartCountTimes {startCountTimes}");
        }

        public void SendCompetitionStarted(int startCountTimes, int leaderboardDivision, int totalPlayersInGroup)
        {
            AnalyticSender.SendEvent(startedEvent, startCountTimes, leaderboardDivision, totalPlayersInGroup,
                                     analyticsContextCreator.MainContext);

            if (isDebugLog)
                UnityEngine.Debug.Log($"{featureTag} Track: {startedEvent}. StartCountTimes {startCountTimes}; LeaderboardDivision: {leaderboardDivision}; TotalPlayersInGroup: {totalPlayersInGroup}");
        }

        public void SendCompetitionRewardAvailable(int startCountTimes, int rewardOrderNumber, int score, ComplexReward inputReward, bool isRewardTrack)
        {
            ComplexReward fullRewardData = rewardApplyController.GetRewardDataIncludeContainer(inputReward);
            string rewardJson = CreateRewardStateJson(score, fullRewardData, isRewardTrack ? "client" : "server");
            AnalyticSender.SendEvent(rewardAvailableEvent, startCountTimes, rewardOrderNumber, rewardJson,
                                     analyticsContextCreator.MainContext);

            if (isDebugLog)
                UnityEngine.Debug.Log($"{featureTag} Track: {rewardAvailableEvent}. StartCountTimes {startCountTimes}; RewardOrderNumber: {rewardOrderNumber}; RewardJson: {rewardJson}");
        }

        public void SendCompetitionRewardClaimed(int startCountTimes, int rewardOrderNumber, int score, ComplexReward inputReward, bool isRewardTrack)
        {
            ComplexReward fullRewardData = rewardApplyController.GetRewardDataIncludeContainer(inputReward);

            string rewardJson = CreateRewardStateJson(score, fullRewardData, isRewardTrack ? "client" : "server");
            AnalyticSender.SendEvent(rewardClaimedEvent, startCountTimes, rewardOrderNumber, rewardJson,
                                     analyticsContextCreator.MainContext);

            if (isDebugLog)
                UnityEngine.Debug.Log($"{featureTag} Track: {rewardClaimedEvent}. StartCountTimes {startCountTimes}; RewardOrderNumber: {rewardOrderNumber}; RewardJson: {rewardJson}");
        }

        public void SendCompetitionFinished(int startCountTimes, int leaderboardDivision, int totalPlayersInGroup, int place, int score)
        {
            string json = CreateFinishStateJson(place, score);
            AnalyticSender.SendEvent(finishedEvent, startCountTimes, leaderboardDivision, totalPlayersInGroup, json,
                                     analyticsContextCreator.MainContext);

            if (isDebugLog)
                UnityEngine.Debug.Log($"{featureTag} Track: {finishedEvent}. StartCountTimes {startCountTimes}; leaderboardDivision: {leaderboardDivision}; TotalPlayersInGroup: {totalPlayersInGroup}; Json {json}");
        }

        private string CreateFinishStateJson(int place, int score)
        {
            sb.Clear();
            sb.Append('{');
            sb.Append(JsonUtils.CreateValue("place", place));
            sb.Append(JsonUtils.CreateValue("score", score));
            sb.AppendLast('}');
            return sb.ToString();
        }

        private static string CreateRewardStateJson(int score, ComplexReward rewards, string rewardType)
        {
            sb.Clear();
            sb.Append('{');

            sb.Append(JsonUtils.CreateValue("score", score));
            sb.Append(JsonUtils.CreateValue("reward", AnalyticsRewardJsonCreator.CreateJson(rewards), false));
            sb.Append(JsonUtils.CreateValue("type", rewardType));

            sb.AppendLast('}');
            return sb.ToString();
        }
    }
}