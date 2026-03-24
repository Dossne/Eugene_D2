using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Social;
using Infrastructure.Reward;

namespace Features.Competition
{
    public class CompetitionRewardController
    {
        private readonly List<CompetitionRewardTrackData> rewardTracksConfig;
        private readonly List<CompetitionServerRewardData> serverRewardsConfig;
        private readonly RewardVisualDataService rewardVisualDataService;
        private readonly RewardApplyController rewardApplyController;
        private readonly CompetitionManager manager;

        private readonly RewardTrackProgressCalculator rewardTrackCalculator;

        private bool isInit;

        public CompetitionRewardController(List<CompetitionRewardTrackData> rewardTrackConfig,
                                           List<CompetitionServerRewardData> serverRewardsConfig,
                                           RewardVisualDataService rewardVisualDataService,
                                           RewardApplyController rewardApplyController,
                                           CompetitionManager manager,
                                           RewardTrackProgressCalculator rewardTrackCalculator)
        {
            this.rewardTrackCalculator = rewardTrackCalculator;
            this.rewardTracksConfig = new List<CompetitionRewardTrackData>(rewardTrackConfig);
            this.serverRewardsConfig = new List<CompetitionServerRewardData>(serverRewardsConfig);
            this.rewardVisualDataService = rewardVisualDataService;
            this.rewardApplyController = rewardApplyController;
            this.manager = manager;
        }

        public void Initialize()
        {
            if (isInit)
                return;


            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            isInit = false;
        }

        public bool IsRewardTrackComplete()
        {
            return rewardTracksConfig.Count == 0 || rewardTracksConfig.Count > 0 && manager.GivenRewardIdx() >= rewardTracksConfig.Count - 1;
        }

        public UniTask<(bool isAvailableOnServer, LeaderboardReceivedReward reward)> GetServerReward(CancellationToken cancellationToken = default)
        {
            return manager.GetRewardAsync(cancellationToken);
        }

        public UniTask<bool> TryClaimReceivedRewardAsync(string letterId, CancellationToken cancellationToken)
        {
            return manager.ClaimReceivedRewardAsync(letterId, cancellationToken);
        }

        public List<CompetitionServerRewardData> GetServerRewardsConfig()
        {
            return serverRewardsConfig;
        }

        public List<CompetitionRewardTrackData> GetRewardTracksConfig()
        {
            return rewardTracksConfig;
        }

        public bool TryGetServerReward(int idx, out CompetitionServerRewardData result)
        {
            if (idx < 0 || idx >= serverRewardsConfig.Count)
            {
                result = null;
                return false;
            }

            result = serverRewardsConfig[idx];
            return true;
        }

        public void ApplyServerReward(string rewardJson)
        {
            var reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardJson);
            ApplyRewardToClient(reward, CompetitionAnalyticsReasons.LeaderboardRewardGiveReason, true);
            manager.SendCompetitionServerRewardClaimed(reward);
        }

        public RewardItemVisualData GetFirstRewardVisualInfo()
        {
            if (rewardTracksConfig.Count == 0)
                return null;

            var reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardTracksConfig[0].rewardJson);
            return rewardVisualDataService.GetFirstRewardVisualData(reward);
        }

        public RewardContainerVisualData GetContainerData(string rewardJson)
        {
            return rewardVisualDataService.GetContainerData(rewardJson);
        }

        public RewardTrackStateDto GetRewardTrackState()
        {
            var notifiedRewardIdx = manager.GetNotifierRewardIdx();
            var currentIdxCalculated = rewardTrackCalculator.GetRewardTrackStepIdxCalculated(manager.GetCollectedCount() + manager.GetScheduledCount());
            var curStepNum = currentIdxCalculated >= 0 ? rewardTracksConfig[currentIdxCalculated].stepNumber : 0;
            var notifyStepNum = notifiedRewardIdx >= 0 ? rewardTracksConfig[notifiedRewardIdx].stepNumber : 0;
            var newRewardCount = curStepNum - notifyStepNum;

            return new RewardTrackStateDto
            {
                notifyCount = manager.GetNotifierRewardCount(),
                currentIdx = currentIdxCalculated,
                newRewardCount = newRewardCount,
                haveFirstRewardTrack = newRewardCount > 0 && manager.GetNotifierRewardIdx() < 0,
            };
        }

        public RewardTrackProgressDto ConvertToProgressDto(CompetitionRewardTrackData rewardDataConfig,
                                                           int configIdx,
                                                           int collectedCountOnStart,
                                                           int collectedCountOnEnd,
                                                           bool isLast)
        {
            var reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardDataConfig.rewardJson);

            var result = new RewardTrackProgressDto
            {
                reward = reward,
                stepNumber = rewardDataConfig.stepNumber,
                configIdx = configIdx,
                collectedCountOnStart = collectedCountOnStart,
                collectedCountOnEnd = collectedCountOnEnd,
                targetCountByConfig = rewardDataConfig.collectCount,
                isLast = isLast
            };
            result.rewardVisual = rewardVisualDataService.GetFirstRewardVisualData(result.reward);
            return result;
        }

        public bool TryGetCurrentRewardTrackState(out RewardTrackProgressDto result)
        {
            var collectedCount = manager.GetCollectedCount();
            bool haveCurrent = rewardTrackCalculator.TryGetCurrentRewardData(collectedCount, out var rewardConfigItem);

            var prevCumulate = rewardConfigItem.idx - 1 >= 0 ? rewardTrackCalculator.GetTarget(rewardConfigItem.idx - 1) : 0;
            var curStep = collectedCount - prevCumulate;
            var isIdxLast = rewardConfigItem.idx >= rewardTracksConfig.Count - 1;

            result = ConvertToProgressDto(rewardConfigItem.reward, rewardConfigItem.idx, curStep, curStep, isIdxLast);
            return haveCurrent;
        }

        public bool TryGetScheduledProgressDto(List<RewardTrackProgressDto> input)
        {
            rewardTrackCalculator.CalculateCollectedProgress(input, manager.GetCollectedCount(), manager.GetScheduledCount());
            return input.Count > 0;
        }

        public void ApplyProgress(List<RewardTrackProgressDto> progress)
        {
            if (progress.Count == 0)
                return;

            foreach (var progressItem in progress)
            {
                if (progressItem.CanGiveReward())
                {
                    ApplyRewardToClient(progressItem.reward, CompetitionAnalyticsReasons.RewardTrackGiveReason, true);
                    manager.SetGivenRewardTrackIdx(progressItem.configIdx);
                    manager.SendCompetitionRewardTrackClaimed(progressItem.stepNumber, progressItem.reward);
                }
            }

            manager.ApplyScheduledCount();
        }

        private void ApplyRewardToClient(ComplexReward reward, string reason, bool updateUi)
        {
            rewardApplyController.ApplyComplexReward(reward, reason, updateUi);
        }
    }
}