using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Events;
using Features.Level;
using Features.ScoringVisualize;
using Features.Social;
using Infrastructure.Ads;
using Infrastructure.ApplicationInterrupt;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Reward;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.Utilities;
using R3;
using UnityEngine;
using Progress = Infrastructure.PersistentProgress.Progress;

namespace Features.Competition
{
    /// <summary>
    /// Main scope
    /// </summary>
    public class CompetitionManager : ISavable, ISystemTickable
    {
        public const string Tag = "[Competition]";
        public event Action OnStateChanged;
        public event Action OnTimeChanged;

        private readonly CompetitionConfig config;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly DateTimeService dateTimeService;
        private readonly LevelService levelService;
        private readonly LeaderboardService leaderboardService;
        private readonly ScoringVisualizeService scoringVisualizeService;
        private readonly AppInterruptObserver appInterruptObserver;
        private readonly CompositeDisposable disposable;
        private readonly CancellationTokenSource cts;
        private readonly CompetitionAnalytics analytics;
        private readonly RewardTrackProgressCalculator rewardTrackProgressCalculator;
        private CompetitionState saveState;
        private double timeTillReset;
        private int winCountPrevStep;
        private bool isDebugLog;
        private bool isInterrupted;
        private bool isInit;

        public CompetitionManager(ConfigProvider configProvider,
                                  LevelFinishEvent levelFinishEvent,
                                  DateTimeService dateTimeService,
                                  LevelService levelService,
                                  LeaderboardService leaderboardService,
                                  ScoringVisualizeService scoringVisualizeService,
                                  AppInterruptObserver appInterruptObserver,
                                  AnalyticsContextCreator analyticsContextCreator,
                                  RewardApplyController rewardApplyController,
                                  RewardVisualDataService rewardVisualDataService)
        {
            this.config = configProvider.CompetitionConfig;
            this.levelFinishEvent = levelFinishEvent;
            this.dateTimeService = dateTimeService;
            this.levelService = levelService;
            this.leaderboardService = leaderboardService;
            this.scoringVisualizeService = scoringVisualizeService;
            this.appInterruptObserver = appInterruptObserver;
            rewardTrackProgressCalculator = new RewardTrackProgressCalculator(rewardVisualDataService, config.RewardTracks);
            analytics = new CompetitionAnalytics(analyticsContextCreator, rewardApplyController, Tag, config.Feature.isDebugLog);
            disposable = new CompositeDisposable();
            cts = new CancellationTokenSource();
            this.isDebugLog = config.Feature.isDebugLog;
        }

#region Public methods

        void ISavable.Load(Progress progress)
        {
            saveState = progress.gameState.competitionState;
        }

        void ISavable.Save(Progress progress)
        {
            progress.gameState.competitionState = saveState;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;

            if (!IsFeatureEnabled())
                return;

            rewardTrackProgressCalculator.Initialize();
            
            if (string.IsNullOrEmpty(saveState.leaderboardName))
            {
                saveState.leaderboardName = config.Feature.leaderboardName;
            }

            await EnterQualificationOrFinishAsync(0, cancellationToken);
            SubscribeFinishLevel();
            ActualizeWinCountPrevStep();

            dateTimeService.OnChange += DateTimeService_OnChange;
            appInterruptObserver.Interrupt += AppInterruptObserver_Interrupt;
            appInterruptObserver.Resume += AppInterruptObserver_Resume;

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            cts.Cancel();
            cts.Dispose();

            disposable.Dispose();
            dateTimeService.OnChange -= DateTimeService_OnChange;
            appInterruptObserver.Interrupt -= AppInterruptObserver_Interrupt;
            appInterruptObserver.Resume -= AppInterruptObserver_Resume;

            isInit = false;
        }

        void ISystemTickable.Tick()
        {
            if (isInterrupted)
                return;

            if (!IsTimerState())
                return;

            timeTillReset -= Time.unscaledDeltaTime;

            if (timeTillReset > 0)
                return;

            SetNextStateAfterTimerOff();
        }

        public bool IsFeatureEnabled()
        {
            return config.Feature.isEnabled;
        }

        public bool IsUnlocked()
        {
            return GetUnlockLevel() <= levelService.CurrentLevelNumber;
        }

        public int GetUnlockLevel()
        {
            return config.Feature.unlockLevel;
        }

        public bool CanShowWidget()
        {
            return levelService.CurrentLevelNumber >= config.Feature.showWidgetLevel;
        }

        public bool IsNoneState()
        {
            return saveState.currentState is CStateType.None;
        }
        
        public bool IsTimerState()
        {
            return saveState.currentState is CStateType.Qualification or CStateType.InProgress;
        }

        public bool IsQualificationState()
        {
            return saveState.currentState is CStateType.Qualification;
        }

        public bool IsProgressState()
        {
            return saveState.currentState is CStateType.InProgress;
        }

        public bool IsFinishedTimeState()
        {
            return saveState.currentState is CStateType.FinishedTime;
        }
        
        public bool IsCompletedState()
        {
            return saveState.currentState is CStateType.Completed;
        }
        
        public bool IsInterrupted()
        {
            return isInterrupted;
        }

        public double GetTimeRest()
        {
            return timeTillReset;
        }

        public int GetSavedLeaderboardIndex()
        {
            return saveState.lastLeaderboardIdx;
        }

        public void SetLastLeaderboardIndex(int currentValue)
        {
            saveState.lastLeaderboardIdx = currentValue;
        }

        public RewardTrackProgressCalculator GetCalculator()
        {
            return rewardTrackProgressCalculator;
        }

        public int GetCollectedCount()
        {
            return saveState.collectedCount;
        }

        public int GetScheduledCount()
        {
            return saveState.scheduledCount;
        }

        public int GivenRewardIdx()
        {
            return saveState.givenRewardIdx;
        }

        public int GetNotifierRewardIdx()
        {
            return saveState.notifierRewardIdx;
        }

        public int GetNotifierRewardCount()
        {
            return saveState.notifierRewardCount;
        }

        public void SetNotifierRewardIdx(int value)
        {
            saveState.notifierRewardIdx = value;
        }

        public void SetNotifierRewardCount(int value)
        {
            saveState.notifierRewardCount = value;
        }

        public void SetGivenRewardTrackIdx(int value)
        {
            saveState.givenRewardIdx = value;
        }

        public void ActualizeWinCountPrevStep()
        {
            winCountPrevStep = saveState.winCountInRow;
        }

        public MultiplierStateDto GetMultiplierState()
        {
            return new MultiplierStateDto
            {
                multiplierPrevNum = GetFeatureMultiplier(winCountPrevStep),
                multiplierPrevIdx = GetMultiplierIdx(winCountPrevStep),
                multiplierCurNum = GetFeatureMultiplier(saveState.winCountInRow),
                multiplierCurIdx = GetMultiplierIdx(saveState.winCountInRow),
            };
        }

        public void ApplyScheduledCount()
        {
            saveState.collectedCount += saveState.scheduledCount;
            saveState.scheduledCount = 0;
        }

        public void SetNextStateAfterFinish()
        {
            SetState(CStateType.Completed);
            EnterQualificationOrFinishAsync(config.Feature.connectToNewLeaderboardDelaySec, cts.Token).Forget();
        }

        public async UniTask ResyncTimeTillResetByServerAsync(CancellationToken cancellationToken)
        {
            if (!IsTimerState() && !IsFinishedTimeState())
                return;

            if (!dateTimeService.IsSynced && await HaveInternetConnectAsync(cancellationToken))
            {
                await dateTimeService.TimeResyncAsync(cancellationToken);
            }

            if (!dateTimeService.TryGetServerTime(out var now))
            {
                return;
            }

            timeTillReset = (saveState.endDate - now).TotalSeconds;

            if (timeTillReset > 0 && IsFinishedTimeState())
            {
                var leaderboard = await GetRunningLeaderboardAsync(GetCachedLeaderboardName(), cancellationToken);

                if (!leaderboard.isRunning)
                    return;

                SetState(CStateType.InProgress);
            }
        }

        public async UniTask<LeaderBoardStateDto> GetLeaderboardPositionsStateAsync(CancellationToken cancellationToken)
        {
            var list = await leaderboardService.ListLeaderboardRecordsAroundOwnerAsync(GetCachedLeaderboardName(), config.Feature.leaderboardCount, cancellationToken);

            bool haveRecords = list != null && list.records != null && list.records.Any();

            if (!haveRecords)
            {
                //request to join leaderboard -> success, request leaderboard's top -> fail
                return new LeaderBoardStateDto
                {
                    records = new List<LeaderboardRecord>(),
                    inProgressState = false,
                    prevSavedIdx = 0,
                    actualIdx = 0,
                };
            }

            var records = list.records;
            records.Sort(LeaderboardDescByScoreComparer.Instance);

            var playerPosIdx = 0;

            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].IsPlayer())
                {
                    playerPosIdx = i;
                    break;
                }
            }

            var cachedIdx = GetSavedLeaderboardIndex();

            if (cachedIdx < 0 || playerPosIdx > cachedIdx) //first launch or loose positions
            {
                SetLastLeaderboardIndex(playerPosIdx);
            }

            return new LeaderBoardStateDto
            {
                records = records,
                prevSavedIdx = GetSavedLeaderboardIndex(),
                actualIdx = playerPosIdx,
                inProgressState = IsProgressState(),
                inFinishedWithPrize = IsFinishedTimeState() && playerPosIdx <= config.ServerRewards.Count - 1,
            };
        }

        public (int newPlayerPosIdx, List<LeaderboardRecord> records) GetActualizedRecords(List<LeaderboardRecord> records, int serverRewardIdx = -1)
        {
            return leaderboardService.GetActualizedRecords(records, config.Feature.leaderboardCount, config.ServerRewards.Count, serverRewardIdx);
        }

        public UniTask<bool> HaveInternetConnectAsync(CancellationToken cancellationToken)
        {
            if (!config.Feature.isCheckInternetConnect)
                return UniTask.FromResult(true);

            return InternetConnectChecker.Have(config.Feature.connectCheckUrl, config.Feature.connectCheckTimeoutMs, cancellationToken);
        }

        public async UniTask<(bool isAvailableOnServer, LeaderboardReceivedReward reward)> GetRewardAsync(CancellationToken cancellationToken)
        {
            bool haveConnect = await HaveInternetConnectAsync(cancellationToken);

            if (!haveConnect)
            {
                return (false, null);
            }

            (bool isServerAvailable, LeaderboardReceivedReward reward) = await leaderboardService.GetRewardAsync(GetCachedLeaderboardName(), cancellationToken);
            return (isServerAvailable, reward);
        }

        public UniTask<bool> ClaimReceivedRewardAsync(string letterId, CancellationToken cancellationToken)
        {
            return leaderboardService.ClaimReceivedRewardAsync(letterId, cancellationToken);
        }

        public UniTask EnterQualificationIfStateIsNone(CancellationToken cancellationToken)
        {
            if (IsUnlocked() && saveState.currentState == CStateType.None)
                return EnterQualificationOrFinishAsync(0, cancellationToken);

            return UniTask.CompletedTask;
        }

#endregion

#region Analytics

        public void SendStartPopupShownAnalytics()
        {
            if (saveState.startPopupShownTimes > 0)
                return;

            analytics.SendStartPopupAvailable(saveState.startCountTimes);
            saveState.startPopupShownTimes++;
        }

        public void SendCompetitionRewardTrackClaimed(int stepNumber, ComplexReward reward)
        {
            analytics.SendCompetitionRewardClaimed(saveState.startCountTimes, stepNumber, GetTotalCollectedCount(), reward, true);
        }

        public void SendCompetitionServerRewardClaimed(ComplexReward reward)
        {
            analytics.SendCompetitionRewardClaimed(saveState.startCountTimes, 1, GetTotalCollectedCount(), reward, false);
        }

        public void SendServerRewardAvailableAnalytics(int place)
        {
            if (saveState.rewardServerNumberAvailable > 0)
                return;

            if (place > 0 && place <= config.ServerRewards.Count - 1)
            {
                var serverRewardJson = config.ServerRewards[place - 1].rewardJson;
                var reward = RewardUtils.ConvertFromJson<ComplexReward>(serverRewardJson);
                analytics.SendCompetitionRewardAvailable(saveState.startCountTimes, 1, GetTotalCollectedCount(), reward, false);
                saveState.rewardServerNumberAvailable++;
            }
        }

        private void SendAvailableAnalytics()
        {
            analytics.SendCompetitionAvailable(saveState.startCountTimes);
        }

        private async UniTask SendStartedAnalyticsAsync(int divisionId)
        {
            var leaderboard = await GetLeaderboardPositionsStateAsync(cts.Token);
            analytics.SendCompetitionStarted(saveState.startCountTimes, divisionId, leaderboard.records.Count);
        }

        private async UniTask SendFinishAnalyticsAsync()
        {
            if (saveState.finishSendAnalyticTimes > 0)
                return;

            var state = await leaderboardService.GetLeaderboardStateAsync(GetCachedLeaderboardName());
            var leaderboard = await GetLeaderboardPositionsStateAsync(cts.Token);

            var score = 0;
            var place = config.Feature.leaderboardCount;

            for (int i = 0; i < leaderboard.records.Count; i++)
            {
                if (!leaderboard.records[i].IsPlayer())
                    continue;

                score = (int)leaderboard.records[i].score;
                place = i + 1;
                break;
            }

            analytics.SendCompetitionFinished(saveState.startCountTimes, (int)state.division, leaderboard.records.Count, place, score);
            saveState.finishSendAnalyticTimes++;
        }

        private async UniTask SendServerRewardAvailableAnalyticsAsync()
        {
            if (saveState.rewardServerNumberAvailable > 0)
                return;

            var (isServerAvailable, rewardState) = await GetRewardAsync(cts.Token);

            if (!isServerAvailable || rewardState == null)
                return;

            SendServerRewardAvailableAnalytics(rewardState.place);
        }

        private void TrackRewardTrackAvailable(int scheduledCount, int currentCount)
        {
            if (scheduledCount <= 0)
                return;

            var scheduledProgress = new List<RewardTrackProgressDto>();

            rewardTrackProgressCalculator.CalculateCollectedProgress(scheduledProgress, currentCount, scheduledCount);

            foreach (var item in scheduledProgress)
            {
                if (item.CanGiveReward())
                    SendCompetitionRewardTrackAvailable(item.stepNumber, item.reward);
            }
        }

        private void SendCompetitionRewardTrackAvailable(int rewardOrderNumber, ComplexReward reward)
        {
            if (rewardOrderNumber <= saveState.rewardTrackNumberAvailable)
                return;

            analytics.SendCompetitionRewardAvailable(saveState.startCountTimes, rewardOrderNumber, GetTotalCollectedCount(), reward, true);
            saveState.rewardTrackNumberAvailable = rewardOrderNumber;
        }

#endregion

#region Private methods

        private async UniTask<(bool isRunning, LeaderboardState state)> GetRunningLeaderboardAsync(string leaderboardName, CancellationToken token)
        {
            var state = await leaderboardService.GetLeaderboardStateAsync(leaderboardName, token);

            if (!dateTimeService.TryGetServerTime(out _))
            {
                return (false, state);
            }

            bool isRunning = leaderboardService.IsLeaderboardRunning(state);
            return (isRunning, state);
        }

        private int GetTotalCollectedCount()
        {
            return saveState.collectedCount + saveState.scheduledCount;
        }

        private int GetFeatureMultiplier(int winCount)
        {
            if (!IsFeatureMultiplierEnabled())
                return 1;

            var idx = GetMultiplierIdx(winCount);
            return config.Multipliers[idx].value;
        }

        private int GetMultiplierIdx(int winCount)
        {
            return Mathf.Min(winCount, config.Multipliers.Count - 1);
        }

        private void SetState(CStateType newState)
        {
            if (newState == saveState.currentState)
                return;

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
                DebugChangeState(saveState.currentState, newState);
#endif

            saveState.currentState = newState;

            OnStateChanged?.Invoke();
        }

        private async UniTask EnterQualificationOrFinishAsync(float delaySec, CancellationToken cancellationToken)
        {
            if (!IsUnlocked())
            {
                return;
            }

            if (delaySec > 0)
            {
                await UniTask.WaitForSeconds(delaySec, ignoreTimeScale: true, cancellationToken: cts.Token);
            }

            var leaderboardNameByConfig = config.Feature.leaderboardName;
            
            var isServerTime = dateTimeService.TryGetServerTime(out var nowTime);
            var leaderboard = await GetRunningLeaderboardAsync(leaderboardNameByConfig, cancellationToken);
            var canEnteredQualification = isServerTime && leaderboard.isRunning && (saveState.currentState is CStateType.None or CStateType.Qualification or CStateType.Completed);

            if (canEnteredQualification)
            {
                timeTillReset = (leaderboard.state.nextTick - nowTime).TotalSeconds;
                SetQualificationState(leaderboardNameByConfig, leaderboard.state.nextTick);
            }
            else
            {
                timeTillReset = (saveState.endDate - nowTime).TotalSeconds;
                EnterFinishStateIfTimerOff();
            }

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
                DebugInitializeState();
#endif
        }

        private void SetNextStateAfterTimerOff()
        {
            if (IsQualificationState())
            {
                SetState(CStateType.None);
                EnterQualificationOrFinishAsync(config.Feature.connectToNewLeaderboardDelaySec, cts.Token).Forget();
            }
            else if (IsProgressState())
            {
                EnterFinishStateIfTimerOff();
            }
        }

        private void EnterFinishStateIfTimerOff()
        {
            if (timeTillReset <= 0 && IsProgressState())
            {
                SetState(CStateType.FinishedTime);
                SendFinishAnalyticsAsync().Forget();
                SendServerRewardAvailableAnalyticsAsync().Forget();
            }
        }

        private void SetQualificationState(string leaderboardName, DateTime endDate)
        {
            saveState.leaderboardName = leaderboardName;

            saveState.collectedCount = 0;
            saveState.scheduledCount = 0;
            saveState.endDate = endDate;
            saveState.winCountInRow = 0;
            saveState.givenRewardIdx = -1;
            saveState.notifierRewardCount = 0;
            saveState.notifierRewardIdx = -1;
            saveState.lastLeaderboardIdx = config.Feature.leaderboardCount - 1;
            saveState.scheduledWinStreak = false;
            saveState.startPopupShownTimes = 0;
            saveState.rewardTrackNumberAvailable = 0;
            saveState.rewardServerNumberAvailable = 0;
            saveState.finishSendAnalyticTimes = 0;
            winCountPrevStep = 0;
            SetState(CStateType.Qualification);
            saveState.startCountTimes++;
            SendAvailableAnalytics();
        }

        private void SubscribeFinishLevel()
        {
            levelFinishEvent.Subscribe(_ => LevelFinishEvent_Executed(levelFinishEvent.isWin, levelFinishEvent.winStreakMultiplier, levelFinishEvent.levelDifficultyMultiplier))
                            .AddTo(disposable);
        }

        private bool IsFeatureMultiplierEnabled()
        {
            return config.Feature.isMultiplierEnabled;
        }

        private string GetCachedLeaderboardName()
        {
            return saveState.leaderboardName;
        }

        private void ApplyCountersIfTimerRunning(bool isWin, int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            if (!IsQualificationState() && !IsProgressState())
            {
                return;
            }

            if (isWin)
            {
                var multi = GetFeatureMultiplier(saveState.winCountInRow);
                var addCount = GetCalculatedGiveCount(winStreakMultiplier, levelDifficultyMultiplier, multi);
                var isWs = winStreakMultiplier > 1;

                if (IsQualificationState())
                    FirstApplyWinAsync(addCount, isWs, winStreakMultiplier, levelDifficultyMultiplier).Forget();
                else
                    TryApplyWinAsync(addCount, isWs, winStreakMultiplier, levelDifficultyMultiplier).Forget();
            }
            else
            {
                ApplyLoose(winStreakMultiplier, levelDifficultyMultiplier);
            }
        }

        private async UniTask FirstApplyWinAsync(int addCount, bool isWinStreak, int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            var isApplied = await TryApplyWinAsync(addCount, isWinStreak, winStreakMultiplier, levelDifficultyMultiplier);

            if (!isApplied)
                return;

            var leaderboard = await GetRunningLeaderboardAsync(GetCachedLeaderboardName(), cts.Token);

            if (!leaderboard.isRunning)
                return;

            SendStartedAnalyticsAsync((int)leaderboard.state.division).Forget();
            SetState(CStateType.InProgress);
        }

        private async UniTask<bool> TryApplyWinAsync(int addCount, bool isWinStreak, int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            var isApplied = await leaderboardService.TryWriteRecordAsync(GetCachedLeaderboardName(), addCount);

            if (!isApplied)
            {
                return false;
            }

            winCountPrevStep = saveState.winCountInRow;
            saveState.winCountInRow++;
            saveState.scheduledCount += addCount;
            saveState.scheduledWinStreak = isWinStreak;
            scoringVisualizeService.Schedule(ScoringFeature.Competition, addCount);

            TrackRewardTrackAvailable(saveState.scheduledCount, saveState.collectedCount);

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
                DebugCompleteLevel(true, winStreakMultiplier, levelDifficultyMultiplier, addCount, GetFeatureMultiplier(saveState.winCountInRow));
#endif

            return true;
        }

        private void ApplyLoose(int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            winCountPrevStep = saveState.winCountInRow;
            saveState.winCountInRow = 0;

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
                DebugCompleteLevel(false, winStreakMultiplier, levelDifficultyMultiplier, 0, GetFeatureMultiplier(saveState.winCountInRow));
#endif
        }

        private int GetCalculatedGiveCount(int winStreakMultiplier, int levelDifficultyMultiplier, int featureMultiplier)
        {
            return config.Feature.giveCountPerLevel * winStreakMultiplier * levelDifficultyMultiplier * featureMultiplier;
        }

        private void ResyncTimeTillReset()
        {
            if (!IsTimerState())
                return;

            dateTimeService.TryGetServerTime(out var now);
            timeTillReset = (saveState.endDate - now).TotalSeconds;
            OnTimeChanged?.Invoke();
        }

#endregion

#region Event handlers

        private void DateTimeService_OnChange(TimeChangeReason reason)
        {
            ResyncTimeTillReset();
        }

        private void AppInterruptObserver_Interrupt()
        {
            isInterrupted = true;
        }

        private void AppInterruptObserver_Resume()
        {
            isInterrupted = false;
        }

        private void LevelFinishEvent_Executed(bool isWin, int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            ApplyCountersIfTimerRunning(isWin, winStreakMultiplier, levelDifficultyMultiplier);
        }

#endregion

#region Cheat

#if PR_CHEAT || UNITY_EDITOR

        public UniTask CheatApplyWinMaxAsync(bool isWinStreak, int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            return CheatApplyWinAsync(rewardTrackProgressCalculator.GetMax(), isWinStreak, winStreakMultiplier, levelDifficultyMultiplier);
        }

        public UniTask CheatApplyWinAsync(int addCount, bool isWinStreak, int winStreakMultiplier, int levelDifficultyMultiplier)
        {
            if (IsQualificationState())
                return FirstApplyWinAsync(addCount, isWinStreak, winStreakMultiplier, levelDifficultyMultiplier);

            return TryApplyWinAsync(addCount, isWinStreak, winStreakMultiplier, levelDifficultyMultiplier);
        }

        public async UniTask CheatResetAsync()
        {
            var leaderboardName = GetCachedLeaderboardName();

            var state = await leaderboardService.GetLeaderboardStateAsync(leaderboardName, cts.Token);

            var leaderboardPositionsState = await GetLeaderboardPositionsStateAsync(cts.Token);
            var idx = leaderboardPositionsState.actualIdx;
            var score = leaderboardPositionsState.records[idx].score;

            leaderboardService.TryWriteRecordAsync(leaderboardName, -score).Forget();
            SetQualificationState(leaderboardName, state.nextTick);
            await EnterQualificationOrFinishAsync(0, cts.Token);
            OnStateChanged?.Invoke();
        }

        private void DebugInitializeState()
        {
            Debug.Log($"{Tag} State: {saveState.currentState}. ExpirationDate: {TimeUtils.GetStringFromDateTime(saveState.endDate)}.Resets in: {timeTillReset}; ");
        }

        private void DebugCompleteLevel(bool isWin, int winStreak, int levelDifficulty, int addedCount, int multiplier)
        {
            Debug.Log($"{Tag} LevelComplete. IsWin: {isWin}. "            +
                      $"\nWinStreakMultiplier: {winStreak}. "             +
                      $"\nLevelDifficultyMultiplier: {levelDifficulty}. " +
                      $"\nFeatureMultiplier: {multiplier}. "              +
                      $"\n AddedCount: {addedCount}. "                    +
                      $"\nScheduledCountTotal {saveState.scheduledCount}");
        }

        private void DebugChangeState(CStateType from, CStateType to)
        {
            Debug.Log($"{Tag} Set State. From: \"{from}\" to \"{to}\"");
        }

#endif

#endregion

    }
}