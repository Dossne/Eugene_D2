using System;
using System.Collections.Generic;
using System.Linq;
using Features.Collectables;
using Features.Events;
using Features.Level;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.TimeCycles;
using R3;

namespace Features.SeasonPass
{
    public class SeasonPassStateController : ISavable
    {
        public event Action OnStateChanged;
        public event Action OnChangeProgress;

        private readonly CompositeDisposable disposable;
        private readonly SeasonPassConfig config;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly TimeCyclesService timeCyclesService;
        private readonly DateTimeService dateTimeService;
        private readonly LevelService levelService;
        private readonly RewardVisualDataService rewardVisualDataService;

        private List<SeasonPassRewardData> rewards = new();
        private readonly List<int /*collectCountCumulative*/> itemCountsBySteps = new();
        private readonly List<ProgressDto> currentProgressDto = new(); //TODO temp remove

        private SeasonPassState saveState;
        private int currentStepIdx = -1;

        private bool isInit;

        public SeasonPassStateController(ConfigProvider              configProvider,
                                         LevelFinishEvent            levelFinishEvent,
                                         TimeCyclesService           timeCyclesService,
                                         DateTimeService             dateTimeService,
                                         LevelService                levelService,
                                         RewardVisualDataService rewardVisualDataService)
        {
            //this.config            = configProvider.SeasonPassConfig;
            this.levelFinishEvent  = levelFinishEvent;
            this.timeCyclesService = timeCyclesService;
            this.dateTimeService   = dateTimeService;
            this.levelService      = levelService;
            this.rewardVisualDataService = rewardVisualDataService;
            disposable             = new CompositeDisposable();
        }

        void ISavable.Load(Infrastructure.PersistentProgress.Progress progress)
        {
           // saveState = progress.gameState.seasonPass;
        }

        void ISavable.Save(Infrastructure.PersistentProgress.Progress progress)
        {
           // progress.gameState.seasonPass = saveState;
        }

        public void Initialize()
        {
            if (isInit)
                return;

            if (!IsFeatureEnabled())
                return;

            if (IsUnlocked())
            {
                InitializeInternal();
            }
            else
            {
                levelService.OnLevelIncremented += LevelService_OnLevelIncremented;
            }

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposable.Dispose();
            timeCyclesService.OnCycleReset  -= TimeCyclesService_OnCycleReset;
            levelService.OnLevelIncremented -= LevelService_OnLevelIncremented;

            isInit = false;
        }

        public void ApplyScheduledAction()
        {
            OnChangeProgress?.Invoke();
        }

        public bool IsFeatureEnabled()
        {
            return config.Feature.isEnabled;
        }

        public bool IsUnlocked()
        {
            return config.Feature.unlockLevel <= levelService.CurrentLevelNumber;
        }

        public bool IsUnlockedByProgress(int stepIdx)
        {
            if (currentStepIdx < 0)
                return false;

            return currentStepIdx >= stepIdx;
        }

        public bool IsPremiumBought()
        {
            return saveState.isPremiumBought;
        }

        public int GetUnlockLevel()
        {
            return config.Feature.unlockLevel;
        }

        public bool CanShowWidget()
        {
            return levelService.CurrentLevelNumber >= config.Feature.showWidgetLevel;
        }

        public bool IsCompleteState()
        {
            return false; //TODO implement
        }

        public bool TryGetSlotClaimState(int slotIdx, out SlotClaimState claimState)
        {
            foreach (var slot in saveState.slots)
            {
                if (slot.idx == slotIdx)
                {
                    claimState = slot;
                    return false;
                }
            }

            claimState = null;
            return false;
        }

        public void SetSlotClaimed(int slotIdx, SlotViewType slotViewType)
        {
            foreach (var slot in saveState.slots)
            {
                if (slot.idx == slotIdx)
                {
                    slot.SetClaimed(slotViewType);
                    return;
                }
            }

            var newItem = new SlotClaimState(slotIdx);
            newItem.SetClaimed(slotViewType);
            saveState.slots.Add(newItem);
        }

        public bool TryGetRewardsData(out List<SeasonPassRewardData> result)
        {
            if (rewards != null && rewards.Count > 0)
            {
                result = rewards;
                return true;
            }

            result = null;
            return false;
        }

        public RewardItemVisualData GetFirstRewardView(string rewardJson)
        {
            ComplexReward reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardJson);
            return rewardVisualDataService.GetFirstRewardVisualData(reward);
        }

        private void InitializeInternal()
        {
            InitCurrentState();
            SubscribeFinishLevel();
            timeCyclesService.OnCycleReset += TimeCyclesService_OnCycleReset;
        }

        private void InitCurrentState()
        {
            if (!timeCyclesService.TryGetCycleItemReadable(TimeCycleType.Monthly, out var cycleItem))
                return;

            DateTime expDate = cycleItem.NextResetDate;

            dateTimeService.TryGetServerTime(out var currentDateTime);

            if (expDate < currentDateTime) //waiting for daily reset in subscription
                return;

            bool reset = TryReset(expDate);

            InitializeRewards();

            if (reset)
                OnStateChanged?.Invoke();

            /*UnityEngine.Debug.Log($"[SeasonPass] ExpirationDate: {TimeUtils.GetStringFromDateTime(expDate)}. Dow is: {expDate.DayOfWeek}. " +
                                  $"Resets in: {TimeUtils.GetTimeString(TimeSpan.FromSeconds((expDate - currentDateTime).TotalSeconds))}");*/
        }

        private void InitializeRewards()
        {
            rewards.Clear();
            itemCountsBySteps.Clear();
            int countCumulative = 0;

            rewards.AddRange(config.Rewards);

            rewards = rewards.OrderBy(x => x.stepNumber).ToList();

            for (var i = 0; i < rewards.Count; i++)
            {
                var rewardData = rewards[i];
                countCumulative += rewardData.collectCount;

                itemCountsBySteps.Add(countCumulative);

                if (saveState.collectedCount >= countCumulative)
                {
                    currentStepIdx = i;
                }
            }
        }

        private bool TryReset(DateTime checkTime)
        {
            if (saveState.endDate >= checkTime)
                return false;

            saveState.collectedCount  = 0;
            saveState.endDate         = checkTime;
            saveState.isPremiumBought = false;
            saveState.slots.Clear();
            currentStepIdx = -1;

            return true;
        }

        /*private ProgressDto ConvertToProgressDto(SeasonPassRewardData rewardDataConfig, int configIdx, int collectedCountOnStart, int collectedCountOnEnd, bool isLast)
        {
            ComplexReward reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardDataConfig.rewardJson);

            var result = new ProgressDto
            {
                stepNumber            = rewardDataConfig.stepNumber,
                configIdx             = configIdx,
                collectedCountOnStart = collectedCountOnStart,
                collectedCountOnEnd   = collectedCountOnEnd,
                targetCountByConfig   = rewardDataConfig.collectCount,
                isLast                = isLast,
                reward                = reward
            };
            result.rewardVisual = rewardInfoService.GetFirstRewardViewParams(result.reward);
            return result;
        }

        private void PrepareCurrentProgressDto(int itemCountTotal, int collectedCount)
        {
            int scheduled = collectedCount;

            if (!TryGetCurrentReward(itemCountTotal, out var rewardConfigItem))
                return;

            int rewardIdx = rewardConfigItem.idx;

            if (rewardIdx >= rewards.Count)
            {
                return;
            }

            while (true)
            {
                var  configToAdd = rewards[rewardIdx];
                bool isIdxLast   = rewardIdx >= rewards.Count - 1;

                int prevCumulative = rewardIdx - 1 >= 0 ? itemCountsBySteps[rewardIdx - 1] : 0;
                int countOnStart   = itemCountTotal - prevCumulative;
                countOnStart = countOnStart > 0 ? countOnStart : 0;

                int diff = countOnStart + scheduled - configToAdd.collectCount;

                if (diff > 0)
                {
                    currentProgressDto.Add(ConvertToProgressDto(configToAdd, rewardIdx, countOnStart, configToAdd.collectCount, isIdxLast));
                    scheduled -= (configToAdd.collectCount - countOnStart);
                    rewardIdx++;
                }
                else
                {
                    int countOnEnd = countOnStart + scheduled;
                    currentProgressDto.Add(ConvertToProgressDto(configToAdd, rewardIdx, countOnStart, countOnEnd, isIdxLast));
                }

                if (isIdxLast || diff <= 0)
                    break;
            }
        }

        private bool TryGetCurrentReward(int itemCount, out (SeasonPassRewardData reward, int idx) result)
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
        }*/

        private void SubscribeFinishLevel()
        {
            levelFinishEvent.Subscribe(_ =>
                             {
                                 if (levelFinishEvent.isWin)
                                     ScheduleApplyWin(levelFinishEvent.winStreakMultiplier, levelFinishEvent.levelDifficultyMultiplier);
                             })
                            .AddTo(disposable);
        }

        private void ScheduleApplyWin(int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            int giveCount = config.Feature.giveCountPerLevel * winStreakMultiplier * levelDifficultyMultiplier;
            saveState.scheduledCount     = giveCount;
            saveState.scheduledWinStreak = winStreakMultiplier > 1;

            // UnityEngine.Debug.Log($"[SeasonPass] Handle win. WinStreakMultiplier: {winStreakMultiplier}. LevelDifficultyMultiplier: {levelDifficultyMultiplier}");
        }

        private void TimeCyclesService_OnCycleReset(TimeCycleType cycleType)
        {
            if (cycleType is not TimeCycleType.Monthly)
                return;

            //UnityEngine.Debug.Log("[SeasonPass] MonthlyReset");
            InitCurrentState();
        }

        private void LevelService_OnLevelIncremented()
        {
            if (!IsUnlocked())
                return;

            levelService.OnLevelIncremented -= LevelService_OnLevelIncremented;
            InitializeInternal();
        }
    }
}