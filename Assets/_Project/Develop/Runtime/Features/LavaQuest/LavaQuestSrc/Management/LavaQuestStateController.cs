using System;
using System.Collections.Generic;
using System.Linq;
using Features.Events;
using Features.Level;
using Features.LevelSessionStateControl;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Reward;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.TimeCycles;
using R3;
using UnityEngine;
using Progress = Infrastructure.PersistentProgress.Progress;

namespace Features.LavaQuest
{
    /// <summary>
    /// Main scene scope
    /// </summary>
    public class LavaQuestStateController : ISavable, ISystemTickable
    {
        public event Action OnChangeState;
        public event Action OnChangeProgress;

        private readonly LevelStartedEvent levelStartedEvent;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly RewardApplyController rewardApplyController;
        private readonly LavaQuestConfig lavaQuestConfig;
        private readonly DateTimeService dateTimeService;
        private readonly TimeCyclesService timeCyclesService;
        private readonly LevelService levelService;
        private readonly RewardVisualDataService rewardVisualDataService;
        private readonly LevelSessionStateService levelSessionStateService;
        private readonly Timer timer;

        private CompositeDisposable disposable;
        private LavaQuestState saveState;
        private List<LavaQuestStepData> configSteps = new();

        private LavaQuestRewardData currentRewardData;
        private ComplexReward currentReward;
        private RewardItemVisualData currentRewardVisual;

        private int currentChainId;
        private int maxStepIdx;
        private bool isInit;


        public LavaQuestStateController(ConfigProvider configProvider,
                                        DateTimeService dateTimeService,
                                        TimeCyclesService timeCyclesService,
                                        LevelService levelService,
                                        LevelStartedEvent levelStartedEvent,
                                        RewardApplyController rewardApplyController,
                                        RewardVisualDataService rewardVisualDataService, 
                                        LevelSessionStateService levelSessionStateService)
        {
            this.lavaQuestConfig = configProvider.LavaQuestConfig;
            this.dateTimeService = dateTimeService;
            this.timeCyclesService = timeCyclesService;
            this.levelService = levelService;
            this.levelStartedEvent = levelStartedEvent;
            this.rewardApplyController = rewardApplyController;
            this.rewardVisualDataService = rewardVisualDataService;
            this.levelSessionStateService = levelSessionStateService;
            timer = new Timer(0);
        }


        public string BillboardRewardIcon => currentRewardData.billboardRewardIcon;
        public RewardItemVisualData CurrentRewardVisualInfo => currentRewardVisual;
        public LavaQuestStepData LavaQuestStepData => configSteps[CurrentStepIdx];
        public DateTime EndDate => saveState.endDate;
        public DateTime NextResetDate => saveState.nextDailyReset;
        public LQStateType CurrentState => saveState.currentState;
        public float TimeRest => timer.TimeRest();
        public int CurrentChainId => currentChainId;
        public int CurrentStepIdx => saveState.stepIndex;
        public int MaxStepIdx => maxStepIdx;
        public int ReadyStartTimes => saveState.readyStartTimes;
        public int UnlockLevel => lavaQuestConfig.Feature.unlockLevel;


        void ISavable.Load(Progress progress)
        {
            saveState = progress.gameState.lavaQuest;
        }


        void ISavable.Save(Progress progress)
        {
            progress.gameState.lavaQuest = saveState;
        }


        public void Initialize()
        {
            if (isInit || !IsFeatureEnabled())
                return;

            if (IsGameLevelFailed())
            {
                ScheduleActionLoose();
                ResetLevelBeginCount();
            }

            InitCurrenState();

            this.disposable = new CompositeDisposable();
            levelStartedEvent.Subscribe(_ => LevelStartedEventHandle(levelStartedEvent.LevelNumber)).AddTo(disposable);

            dateTimeService.OnChange += DateTimeService_OnChange;
            timeCyclesService.OnCycleReset += TimeCyclesService_OnCycleReset;

            if (saveState.currentState == LQStateType.None && !IsUnlocked())
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

            dateTimeService.OnChange -= DateTimeService_OnChange;
            timeCyclesService.OnCycleReset -= TimeCyclesService_OnCycleReset;
            levelService.OnLevelIncremented -= LevelService_OnLevelIncremented;

            isInit = false;
        }


        void ISystemTickable.Tick()
        {
            if (!IsTimerState())
                return;

            if (!timer.IsOffAfterUpdate(Time.unscaledDeltaTime))
                return;

            if (saveState.currentState is LQStateType.Started && !IsLastStep())
            {
                SetState(LQStateType.FinishedInRuntime);
            }
            else
            {
                InitCurrenState();
            }
        }


        public void ScheduleActionWin()
        {
            if (!IsStartedState())
                return;

            saveState.scheduledAction = LQScheduledAction.LevelWin;
            ResetLevelBeginCount();

            if (saveState.stepIndex + 1 >= maxStepIdx)
            {
                ApplyReward();
            }

            LavaQuestAnalytics.SendLevelExtraCompleted(saveState.startedLevelNumber, saveState.stepIndex);
        }


        public void ScheduleActionLoose()
        {
            if (!IsStartedState())
                return;

            saveState.scheduledAction = LQScheduledAction.LevelLoose;
        }


        public void ApplyScheduledAction()
        {
            if (!AnyActionScheduled())
                return;

            if (IsScheduledWin())
            {
                saveState.stepIndex += 1;
            }

            if (IsScheduledLoose())
            {
                SetLooseCooldownState();
                LavaQuestAnalytics.SendLevelExtraFailed(saveState.startedLevelNumber, saveState.stepIndex);
            }

            saveState.scheduledAction = LQScheduledAction.None;
            OnChangeProgress?.Invoke();

            if (IsLastStep())
            {
                SetState(LQStateType.Complete);
                InitCurrenState();
            }
        }


        public void SetReadyToStart()
        {
            if (saveState.currentState is not LQStateType.FinishedInRuntime)
                return;

            InitCurrenState();
        }


        public void SetStartedState()
        {
            if (saveState.currentState is not LQStateType.ReadyStart)
                return;

            dateTimeService.TryGetServerTime(out DateTime currentDateTime);
            DateTime endDate = currentDateTime.AddSeconds(lavaQuestConfig.Feature.durationSec);
            saveState.endDate = endDate;

            InitCurrentId();
            InitStepsData();

            timer.Reset(lavaQuestConfig.Feature.durationSec);
            SetState(LQStateType.Started);
        }


        public float GetProgress()
        {
            if (!IsStartedState())
                return 0;

            return CurrentStepIdx / (float)MaxStepIdx;
        }


        public int GetCurrentPlayersFakeCount()
        {
            return GetPlayersFakeCount(CurrentStepIdx);
        }


        public int GetPlayersFakeCount(int stepIdx)
        {
            stepIdx = Mathf.Clamp(stepIdx, 0, configSteps.Count - 1);
            return saveState.playersFakeCounts[stepIdx];
        }


        public int GetDividedByPlayersAmount(int rewardAmount, int playersCount)
        {
            return Mathf.CeilToInt(rewardAmount * 1f / (playersCount + 1));
        }


        public bool IsFeatureEnabled()
        {
            return lavaQuestConfig.Feature.isFeatureEnabled;
        }


        public bool IsUnlocked()
        {
            return levelService.CurrentLevelNumber >= lavaQuestConfig.Feature.unlockLevel;
        }

        public bool CanShowWidget()
        {
            return levelService.CurrentLevelNumber >= lavaQuestConfig.Feature.showWidgetLevel;
        }
        
        
        public bool IsStartedState()
        {
            return saveState.currentState is LQStateType.Started;
        }


        public bool IsTimerState()
        {
            return saveState.currentState is LQStateType.Started or LQStateType.LooseCooldown;
        }


        public bool IsFinishedInRuntimeState()
        {
            return saveState.currentState is LQStateType.FinishedInRuntime;
        }


        public LavaQuestStepData GetFirstLavaQuestStepData()
        {
            return configSteps[0];
        }


        public bool IsCompleteState()
        {
            return saveState.currentState is LQStateType.Complete;
        }


        public bool AnyActionScheduled()
        {
            return saveState.scheduledAction != LQScheduledAction.None;
        }


        public bool IsScheduledWin()
        {
            return saveState.scheduledAction == LQScheduledAction.LevelWin;
        }


        public bool IsScheduledLoose()
        {
            return saveState.scheduledAction == LQScheduledAction.LevelLoose;
        }


        private void InitCurrenState()
        {
            if (!IsUnlocked())
                return;

            if (!timeCyclesService.TryGetCycleItemReadable(TimeCycleType.Daily, out ITimeCycleReadable cycleItem))
                return;

            dateTimeService.TryGetServerTime(out DateTime currentDateTime);

            if (currentDateTime >= saveState.nextDailyReset && cycleItem.NextResetDate <= saveState.nextDailyReset) //waiting for daily reset in subscription
                return;

            bool dailyResetApplied = currentDateTime >= saveState.nextDailyReset && saveState.nextDailyReset <= cycleItem.NextResetDate;
            bool isResetTime = IsResetTime(dailyResetApplied, currentDateTime);

            if (isResetTime)
            {
                SetReadyStartState(currentDateTime, cycleItem.NextResetDate, dailyResetApplied);
            }
            else if (IsTimerState())
            {
                timer.Reset((float)(saveState.endDate - currentDateTime).TotalSeconds);
            }

            InitCurrentId();
            InitStepsData();
            InitCurrentReward();

            if (isResetTime || saveState.playersFakeCounts == null || saveState.playersFakeCounts.Length != configSteps.Count)
                InitPlayerFakeCount();
        }


        private bool IsResetTime(bool dailyResetApplied, DateTime currentDateTime)
        {
            return saveState.currentState is LQStateType.Complete && dailyResetApplied
                || saveState.currentState is not LQStateType.ReadyStart && currentDateTime >= saveState.endDate;
        }


        private void SetLooseCooldownState()
        {
            dateTimeService.TryGetServerTime(out var serverTime);
            saveState.endDate = serverTime.AddSeconds(lavaQuestConfig.Feature.looseCdSec);
            timer.Reset(lavaQuestConfig.Feature.looseCdSec);
            SetState(LQStateType.LooseCooldown);
        }


        private void SetReadyStartState(DateTime endDate, DateTime dailyReset, bool isIncrementChain)
        {
            if (isIncrementChain)
            {
                IncrementChain();
            }

            saveState.readyStartTimes++;
            saveState.stepIndex = 0;
            saveState.nextDailyReset = dailyReset;
            saveState.endDate = endDate;
            saveState.scheduledAction = LQScheduledAction.None;
            ResetLevelBeginCount();

            SetState(LQStateType.ReadyStart);
        }


        private void SetState(LQStateType state)
        {
            if (saveState.currentState == state)
                return;

            saveState.currentState = state;
            //Debug.Log($"[LavaQuestStateController]. Change state to: {state}. ChainIdx. {saveState.chainIdx}. Ends: {saveState.endDate}. NextDailyReset: {saveState.nextDailyReset}");
            OnChangeState?.Invoke();
            LavaQuestAnalytics.SendOnChangeState(state, levelService.CurrentLevelNumber, ReadyStartTimes);
        }


        private void InitCurrentId()
        {
            currentChainId = saveState.chainIdx >= 0 ? GetChainData(saveState.chainIdx).id : -1;
        }


        private void InitStepsData()
        {
            configSteps.Clear();

            foreach (var step in lavaQuestConfig.Steps)
            {
                if (step.id == currentChainId)
                    configSteps.Add(step);
            }

            configSteps = configSteps.OrderBy(x => x.stepIdx).ToList();

            if (configSteps.Count > 0)
                maxStepIdx = configSteps[^1].stepIdx;
        }


        private void InitCurrentReward()
        {
            currentRewardData = null;

            foreach (var lqRewardData in lavaQuestConfig.Rewards)
            {
                if (lqRewardData.id == currentChainId)
                    currentRewardData = lqRewardData;
            }

            if (currentRewardData == null)
            {
                Debug.LogError($"{nameof(LavaQuestStateController)}. Reward data not found for id: {currentChainId}");
                return;
            }
            
            currentReward = RewardUtils.ConvertFromJson<ComplexReward>(currentRewardData.rewardJson);
            currentRewardVisual = rewardVisualDataService.GetFirstRewardVisualData(currentReward);
        }


        private void InitPlayerFakeCount()
        {
            saveState.playersFakeCounts = new int[configSteps.Count];

            for (int i = 0; i < configSteps.Count; i++)
            {
                LavaQuestStepData configData = configSteps[i];
                saveState.playersFakeCounts[i] = UnityEngine.Random.Range(configData.minPlayers, configData.maxPlayers + 1);
            }
        }


        private void IncrementChain()
        {
            saveState.chainIdx++;

            if (saveState.chainIdx >= lavaQuestConfig.Chain.Count)
                saveState.chainIdx = 0;
        }


        private bool IsLastStep()
        {
            return saveState.stepIndex >= maxStepIdx;
        }


        private bool IsGameLevelFailed()
        {
            return saveState.currentState is LQStateType.Started && levelSessionStateService.IsGameLevelLoose(saveState.startedLevelNumber) && saveState.levelBeginCount > 0;
        }


        private LavaQuestChainData GetChainData(int idx)
        {
            idx = Mathf.Clamp(idx, 0, lavaQuestConfig.Chain.Count - 1);
            return lavaQuestConfig.Chain[idx];
        }


        private void LevelStartedEventHandle(int levelNumber)
        {
            if (!IsStartedState())
                return;

            if (saveState.startedLevelNumber < levelNumber)
                saveState.levelBeginCount = 1;
            else
                saveState.levelBeginCount++;

            saveState.startedLevelNumber = levelNumber;

            LavaQuestAnalytics.SendLevelExtraStarted(levelNumber, saveState.stepIndex);
        }


        private void ApplyReward()
        {
            if (currentReward.currencyOfferRewards.Count > 0)
            {
                foreach (var reward in currentReward.currencyOfferRewards)
                {
                    int playersFakeCount = saveState.playersFakeCounts[^1];
                    reward.amount = GetDividedByPlayersAmount(reward.amount, playersFakeCount);
                }
            }

            rewardApplyController.ApplyComplexReward(currentReward, "lava_quest_complete");
        }


        private void ResetLevelBeginCount()
        {
            saveState.levelBeginCount = 0;
        }


        private void TimeCyclesService_OnCycleReset(TimeCycleType cycle)
        {
            if (cycle is not TimeCycleType.Daily)
                return;

            InitCurrenState();
        }


        private void DateTimeService_OnChange(TimeChangeReason reason)
        {
            if (!IsTimerState())
                return;

            dateTimeService.TryGetServerTime(out DateTime currentDateTime);
            timer.Reset((float)(saveState.endDate - currentDateTime).TotalSeconds);
        }


        private void LevelService_OnLevelIncremented()
        {
            if (saveState.currentState == LQStateType.None && IsUnlocked())
            {
                InitCurrenState();
            }
        }


#if PR_CHEAT


        public void CheatReset()
        {
            saveState = new LavaQuestState();
            Deinitialize();
            Initialize();
        }


        public void CheatScheduleWin()
        {
            if (saveState.currentState is not LQStateType.Started)
                return;

            ScheduleActionWin();
        }


        public void CheatScheduleLoose()
        {
            if (saveState.currentState is not LQStateType.Started)
                return;

            ScheduleActionLoose();
        }


        public void CheatExpireTimer()
        {
            if (!IsTimerState())
                return;

            dateTimeService.TryGetServerTime(out DateTime currentDateTime);
            saveState.endDate = currentDateTime;
            timer.Reset(5);
        }


        public void CheatIncreaseStep()
        {
            if (!IsStartedState())
                return;

            ScheduleActionWin();
            ApplyScheduledAction();
        }


        public void CheatDecreaseStep()
        {
            if (!IsStartedState())
                return;

            saveState.stepIndex--;
            if (saveState.stepIndex < 0)
                saveState.stepIndex = 0;

            saveState.scheduledAction = LQScheduledAction.None;
            OnChangeProgress?.Invoke();
        }
#endif
    }
}