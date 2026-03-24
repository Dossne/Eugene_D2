using System.Collections.Generic;
using Infrastructure.Reward;

namespace Features.Competition
{
    public class RewardTrackProgressCalculator
    {
        private readonly List<int /*collectCountCumulative*/> rewardTrackTargets = new();
        private readonly List<CompetitionRewardTrackData> rewardTracksConfig;
        private readonly RewardVisualDataService rewardVisualDataService;

        public RewardTrackProgressCalculator(RewardVisualDataService rewardVisualDataService, List<CompetitionRewardTrackData> rewardTracksConfig)
        {
            this.rewardVisualDataService = rewardVisualDataService;
            this.rewardTracksConfig = rewardTracksConfig;
        }

        public void Initialize()
        {
            InitializeRewards();
        }

        public void CalculateCollectedProgress(List<RewardTrackProgressDto> inputToFill, int itemCountOnStart, int giveCount)
        {
            inputToFill.Clear();

            if (giveCount == 0)
                return;

            if (!TryGetCurrentRewardData(itemCountOnStart, out var rewardConfigItem))
                return;

            var rewardIdx = rewardConfigItem.idx;

            if (rewardIdx >= rewardTracksConfig.Count)
            {
                return;
            }

            while (true)
            {
                var configToAdd = rewardTracksConfig[rewardIdx];
                var isIdxLast = rewardIdx >= rewardTracksConfig.Count - 1;
                var prevCumulative = rewardIdx - 1 >= 0 ? rewardTrackTargets[rewardIdx - 1] : 0;
                var countOnStart = itemCountOnStart - prevCumulative;
                countOnStart = countOnStart > 0 ? countOnStart : 0;

                var diff = countOnStart + giveCount - configToAdd.collectCount;

                if (diff > 0)
                {
                    inputToFill.Add(ConvertToProgressDto(configToAdd, rewardIdx, countOnStart, configToAdd.collectCount, isIdxLast));
                    giveCount -= (configToAdd.collectCount - countOnStart);
                    rewardIdx++;
                }
                else
                {
                    var countOnEnd = countOnStart + giveCount;
                    inputToFill.Add(ConvertToProgressDto(configToAdd, rewardIdx, countOnStart, countOnEnd, isIdxLast));
                }

                if (isIdxLast || diff <= 0)
                    break;
            }
        }

        public int GetTarget(int idx)
        {
            return rewardTrackTargets[idx];
        }

        public int GetMax()
        {
            if (rewardTrackTargets.Count > 0)
                return rewardTrackTargets[^1];

            return 0;
        }

        public bool TryGetCurrentRewardData(int itemCount, out (CompetitionRewardTrackData reward, int idx) result)
        {
            for (var i = 0; i < rewardTrackTargets.Count; i++)
            {
                if (itemCount >= rewardTrackTargets[i])
                    continue;

                result = (rewardTracksConfig[i], i);
                return true;
            }

            //don't have current, completed all
            var lastIdx = rewardTracksConfig.Count - 1;
            result = (rewardTracksConfig[lastIdx], lastIdx);
            return false;
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

        public int GetRewardTrackStepIdxCalculated(int total)
        {
            var result = -1;

            for (var i = 0; i < rewardTrackTargets.Count; i++)
            {
                if (total >= rewardTrackTargets[i])
                    result = i;
            }

            return result;
        }

        private void InitializeRewards()
        {
            rewardTrackTargets.Clear();
            var countCumulative = 0;

            rewardTracksConfig.Sort(RewardTrackDataAscByStepNumComparer.Instance);

            for (var i = 0; i < rewardTracksConfig.Count; i++)
            {
                var rewardData = rewardTracksConfig[i];
                countCumulative += rewardData.collectCount;

                rewardTrackTargets.Add(countCumulative);
            }
        }
    }
}