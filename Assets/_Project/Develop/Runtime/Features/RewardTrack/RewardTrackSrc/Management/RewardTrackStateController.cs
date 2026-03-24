using System;
using System.Collections.Generic;
using System.Linq;
using Features.Collectables;
using Features.Level;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.TimeCycles;
using Infrastructure.Utilities;

namespace Features.RewardTrack
{
    public class RewardTrackStateController : ISavable
    {
        public event Action OnTrackStateChanged;

        private readonly RewardTrackConfig rewardTrackConfig;
        private readonly CollectablesConfig collectablesConfig;

        private readonly DateTimeService dateTimeService;
        private readonly TimeCyclesService timeCyclesService;
        private readonly LevelService levelService;
        private readonly RewardApplyController rewardApplyController;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly RewardVisualDataService rewardVisualDataService;
        private readonly RewardTrackHudProvider rewardTrackHud;

        private RewardTrackData currentRewardTrack;
        private RewardTrackState saveState;
        private RewardTargetData currentCollectTarget;

        private List<RewardTrackRewardData> rewards = new();
        private readonly List<int /*collectCountCumulative*/> itemCountsBySteps = new();

        private readonly List<ProgressDto> lastCollectProgress = new();
        private int lastCollectedItemsCount;

        private bool isInit;


        public RewardTrackStateController(ConfigProvider configProvider,
                                          MainUIProvider mainUIProvider,
                                          DateTimeService dateTimeService,
                                          TimeCyclesService timeCyclesService,
                                          LevelService levelService,
                                          RewardApplyController rewardApplyController,
                                          SpriteAtlasService spriteAtlasService,
                                          RewardVisualDataService rewardVisualDataService)
        {
            this.rewardTrackConfig = configProvider.RewardTrackConfig;
            this.collectablesConfig = configProvider.CollectablesConfig;
            this.rewardTrackHud = mainUIProvider.HudProvider.RewardTrackHud;
            this.dateTimeService = dateTimeService;
            this.timeCyclesService = timeCyclesService;
            this.levelService = levelService;
            this.rewardApplyController = rewardApplyController;
            this.spriteAtlasService = spriteAtlasService;
            this.rewardVisualDataService = rewardVisualDataService;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            if (!IsEnabledByConfig())
            {
                rewardTrackHud.SetObjectActive(false);
                return;
            }

            rewardTrackHud.SetObjectActive(true);

            InitCurrentRewardTrack();
            timeCyclesService.OnCycleReset += TimeCyclesService_OnCycleReset;
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            timeCyclesService.OnCycleReset -= TimeCyclesService_OnCycleReset;

            isInit = false;
        }


        void ISavable.Load(Progress progress)
        {
            saveState = progress.gameState.rewardTrack;
        }


        void ISavable.Save(Progress progress)
        {
            progress.gameState.rewardTrack = saveState;
        }


        public bool IsEnabledByConfig()
        {
            return rewardTrackConfig.Feature.isEnabled;
        }


        public bool IsUnlocked()
        {
            return rewardTrackConfig.Feature.unlockLevel <= levelService.CurrentLevelNumber;
        }


        public int GetUnlockLevel()
        {
            return rewardTrackConfig.Feature.unlockLevel;
        }


        public TimeSpan GetTimeUntilReset()
        {
            dateTimeService.TryGetServerTime(out var serverTime);
            return saveState.endDate - serverTime;
        }


        public bool TryGetCurrentProgress(out ProgressDto result)
        {
            if (!TryGetRewardTrack(saveState.collectedItemCount, out (RewardTrackRewardData reward, int idx) rewardConfigItem))
            {
                result = null;
                return false;
            }

            int prevCumulative = rewardConfigItem.idx - 1 >= 0 ? itemCountsBySteps[rewardConfigItem.idx - 1] : 0;
            int currentStepState = saveState.collectedItemCount - prevCumulative;
            bool isIdxLast = rewardConfigItem.idx >= rewards.Count - 1;

            result = ConvertToProgressDto(rewardConfigItem.reward, rewardConfigItem.idx, currentStepState, currentStepState, isIdxLast);
            return true;
        }


        public bool IsCurrentTrackCompleted()
        {
            if (itemCountsBySteps.Count < 1)
                return true;

            return saveState.collectedItemCount >= itemCountsBySteps[^1];
        }


        public int GetCountForMax()
        {
            return itemCountsBySteps[^1];
        }


        public List<RewardTrackRewardData> GetRewardsConfig()
        {
            return rewards;
        }


        public RewardTrackData GetCurrentTrackConfig()
        {
            return currentRewardTrack;
        }


        public RewardTargetData GetCurrentTargetConfig()
        {
            return currentCollectTarget;
        }


        public (int itemCount, List<ProgressDto> progress) GetLastCollectedProgress()
        {
            return (lastCollectedItemsCount, lastCollectProgress);
        }


        public void ClearLastCollectedProgress()
        {
            lastCollectedItemsCount = 0;
            lastCollectProgress.Clear();
        }


        public void ApplyCollectedCount(CollectableType itemType, int addedItemCount)
        {
            if (currentCollectTarget == null || currentCollectTarget.item != itemType) //occured time cycle reset during level
                return;

            if (addedItemCount == 0)
                return;

            int itemCountTotalOnBefore = saveState.collectedItemCount;
            saveState.collectedItemCount += addedItemCount;
            lastCollectedItemsCount = addedItemCount;

            lastCollectProgress.Clear();
            PrepareLastCollectedProgressDto(lastCollectProgress, itemCountTotalOnBefore, addedItemCount);

            
        }

        public void ApplyLastProgressReward() 
        {
            foreach (var progressItem in lastCollectProgress)
            {
                if (progressItem.CanGiveReward())
                {
                    ApplyReward(progressItem.reward);
                    RewardTrackAnalytics.SendRewardReceived(currentRewardTrack.id, progressItem.stepNumber, progressItem.isLast);
                }
            }
        }


        public ProgressDto ConvertToProgressDto(RewardTrackRewardData rewardDataConfig, int configIdx, int collectedCountOnStart, int collectedCountOnEnd, bool isLast)
        {
            ComplexReward reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardDataConfig.rewardJson);

            string collectableIcon = null;
            string resurrectIcon = null;

            if (collectablesConfig.TryGet(currentCollectTarget.item, out CollectablesData collectablesData))
            {
                collectableIcon = collectablesData.iconName;
                resurrectIcon = collectablesData.iconResurrectName;
            }

            var result = new ProgressDto
            {
                targetItemType = currentCollectTarget.item,
                targetIcon = spriteAtlasService.GetFromMain(collectableIcon),
                targetResurrectIcon = spriteAtlasService.GetFromMain(resurrectIcon),
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


        public static bool TryGetTrackAndNextDow(DateTime nextDailyReset, List<RewardTrackData> trackDatas, out RewardTrackData rewardTd, out DateTime expDate)
        {
            DateTime currentResetDate = nextDailyReset.AddDays(-1);

            if (!TryGetActiveTrack(trackDatas, currentResetDate, out rewardTd))
            {
                expDate = DateTime.MinValue;
                return false;
            }

            DateTime closestDow = TimeCyclesService.GetNextResetDate(currentResetDate, TimeCycleType.Weekly, nextDailyReset.Hour, nextDailyReset.Minute, rewardTd.endDay, true);
            expDate = TimeCyclesService.GetNextResetDate(closestDow, TimeCycleType.Daily, nextDailyReset.Hour, nextDailyReset.Minute);
            return true;
        }


        private void PrepareLastCollectedProgressDto(List<ProgressDto> progress, int itemCountTotal, int collectedCount)
        {
            int scheduled = collectedCount;

            if (!TryGetRewardTrack(itemCountTotal, out (RewardTrackRewardData reward, int idx) rewardConfigItem))
                return;

            int rewardIdx = rewardConfigItem.idx;

            if (rewardIdx >= rewards.Count)
            {
                return;
            }

            while (true)
            {
                RewardTrackRewardData configToAdd = rewards[rewardIdx];
                bool isIdxLast = rewardIdx >= rewards.Count - 1;

                int prevCumulative = rewardIdx - 1 >= 0 ? itemCountsBySteps[rewardIdx - 1] : 0;
                int countOnStart = itemCountTotal - prevCumulative;
                countOnStart = countOnStart > 0 ? countOnStart : 0;

                int diff = countOnStart + scheduled - configToAdd.collectCount;

                if (diff > 0)
                {
                    progress.Add(ConvertToProgressDto(configToAdd, rewardIdx, countOnStart, configToAdd.collectCount, isIdxLast));
                    scheduled -= (configToAdd.collectCount - countOnStart);
                    rewardIdx++;
                }
                else
                {
                    int countOnEnd = countOnStart + scheduled;
                    progress.Add(ConvertToProgressDto(configToAdd, rewardIdx, countOnStart, countOnEnd, isIdxLast));
                }

                if (isIdxLast || diff <= 0)
                    break;
            }
        }


        private void ApplyReward(ComplexReward complexReward)
        {
            rewardApplyController.ApplyComplexReward(complexReward, "reward_track_reward", false);
        }


        private bool TryGetRewardTrack(int itemCount, out (RewardTrackRewardData reward, int idx) result)
        {
            for (var i = 0; i < itemCountsBySteps.Count; i++)
            {
                if (itemCount >= itemCountsBySteps[i])
                    continue;

                result = (rewards[i], i);
                return true;
            }

            result = (null, -1);
            return false;
        }


        private void InitCurrentRewardTrack()
        {
            if (!timeCyclesService.TryGetCycleItemReadable(TimeCycleType.Daily, out var cycleItem))
                return;

            if (!TryGetTrackAndNextDow(cycleItem.NextResetDate, rewardTrackConfig.TrackDatas, out currentRewardTrack, out DateTime expDate))
                return;

            dateTimeService.TryGetServerTime(out var currentDateTime);

            // UnityEngine.Debug.Log($"[RewardTrack]. ExpDate: {expDate}. CurrentDateTime{currentDateTime}");

            if (expDate < currentDateTime) //waiting for daily reset in subscription
                return;

            bool reset = TryReset(expDate);

            InitCurrentRewardTrackData();
            InitCurrentTarget();

            if (reset)
                OnTrackStateChanged?.Invoke();

            /*UnityEngine.Debug.Log($"[RewardTrack]. Current Id: {currentRewardTrack.id}. " +
                      $"TargetIdx: {saveState.collectTargetIdx}. " +
                      $"ExpirationDate: {TimeUtils.GetStringFromDateTime(expDate)}. Dow is: {expDate.DayOfWeek}. " +
                      $"Resets in: {TimeUtils.GetTimeString(TimeSpan.FromSeconds((expDate - currentDateTime).TotalSeconds))}");*/
        }


        private void InitCurrentRewardTrackData()
        {
            rewards.Clear();
            itemCountsBySteps.Clear();
            int countCumulative = 0;

            foreach (var rewardData in rewardTrackConfig.Rewards)
            {
                if (rewardData.id == currentRewardTrack.id)
                {
                    rewards.Add(rewardData);
                }
            }

            rewards = rewards.OrderBy(x => x.stepNumber).ToList();

            foreach (var rewardData in rewards)
            {
                countCumulative += rewardData.collectCount;
                itemCountsBySteps.Add(countCumulative);
            }
        }


        private void InitCurrentTarget()
        {
            currentCollectTarget = rewardTrackConfig.Targets[saveState.collectTargetIdx];
        }


        private static bool TryGetActiveTrack(List<RewardTrackData> trackDatas, DateTime currentResetDate, out RewardTrackData result)
        {
            for (int i = 0; i < trackDatas.Count; i++)
            {
                if (TimeUtils.IsBetween(currentResetDate.DayOfWeek, trackDatas[i].startDay, trackDatas[i].endDay))
                {
                    result = trackDatas[i];
                    return true;
                }
            }

            result = null;
            return false;
        }


        private bool TryReset(DateTime checkTime)
        {
            if (saveState.endDate >= checkTime)
                return false;

            saveState.collectTargetIdx++;

            if (saveState.collectTargetIdx >= rewardTrackConfig.Targets.Count)
                saveState.collectTargetIdx = 0;

            saveState.collectedItemCount = 0;
            saveState.endDate = checkTime;

            return true;
        }


        private void TimeCyclesService_OnCycleReset(TimeCycleType cycleType)
        {
            if (cycleType == TimeCycleType.Daily)
                InitCurrentRewardTrack();
        }


#if PR_CHEAT || UNITY_EDITOR

        public void CheatReset()
        {
            saveState.collectedItemCount = 0;
            lastCollectedItemsCount = 0;
            lastCollectProgress.Clear();
        }
#endif
    }
}