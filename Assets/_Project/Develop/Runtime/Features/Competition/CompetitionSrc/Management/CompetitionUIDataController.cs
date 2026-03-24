using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.ScoringVisualize;
using Features.Social;
using Infrastructure.Reward;

namespace Features.Competition
{
    /// <summary>
    /// Provides data between ui controllers in consistent way
    /// </summary>
    public class CompetitionUIDataController
    {
        public event Action OnUiDataChanged;

        private readonly CompetitionManager manager;
        private readonly CompetitionRewardController rewardController;
        private readonly ScoringVisualizeService scoringVisualizeService;
        private readonly ScoringFeature featureType;
        private readonly List<RewardItemVisualData> appliedRewardTrackVisual = new();

        private LeaderboardReceivedReward serverLeaderboardReward;
        private MultiplierStateDto multiplierStateCached;
        private RewardTrackStateDto rewardTrackStateCached;
        private LeaderBoardStateDto leaderboardStateCached;
        private CancellationTokenSource cts;
        private bool isFinishPopupLogicCompleted;
        private bool isInit;

        public CompetitionUIDataController(CompetitionManager manager,
                                           CompetitionRewardController rewardController,
                                           ScoringVisualizeService scoringVisualizeService,
                                           ScoringFeature featureType)
        {
            this.manager = manager;
            this.rewardController = rewardController;
            this.scoringVisualizeService = scoringVisualizeService;
            this.featureType = featureType;
            cts = new CancellationTokenSource();
        }

        public List<RewardItemVisualData> AppliedRewardTrackVisual => appliedRewardTrackVisual;
        public LeaderboardReceivedReward ServerLeaderboardReward => serverLeaderboardReward;

        public MultiplierStateDto MultiplierState => multiplierStateCached;

        public RewardTrackStateDto RewardTrackState => rewardTrackStateCached;

        public RewardItemVisualData FirstRewardVisualInfo => rewardController.GetFirstRewardVisualInfo();
        public LeaderBoardStateDto LeaderBoardState => leaderboardStateCached;

        public string LeaderboardWidgetPosition { get; private set; }
        public bool HaveFirstRewardTrack { get; private set; }
        public bool HaveNewReward { get; private set; }
        public bool IsFinishedWithPrize => leaderboardStateCached.inFinishedWithPrize;

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;

            await CacheLeaderboardStateAsync(cancellationToken);
            CacheLeaderboardWidgetTextByPrevState();

            InitRewardTrackData();
            CacheMultiplierState();
            scoringVisualizeService.OnComplete += ScoringVisualizeService_OnComplete;
            manager.OnStateChanged += CompetitionManager_OnStateChanged;

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            ActualizeLeaderboardPosition();
            scoringVisualizeService.OnComplete -= ScoringVisualizeService_OnComplete;
            manager.OnStateChanged -= CompetitionManager_OnStateChanged;
            cts.Cancel();
            cts.Dispose();

            isInit = false;
        }

        public void AddToScheduleRewardTrackPopup(RewardItemVisualData rewardTrack)
        {
            this.appliedRewardTrackVisual.Add(rewardTrack);
        }

        public void ActualizeMultiplierState()
        {
            manager.ActualizeWinCountPrevStep();
            multiplierStateCached = manager.GetMultiplierState();
        }

        /// <summary>
        /// Refresh counter (do not have any reward tracks to give)
        /// </summary>
        public void ResetNotifier()
        {
            InitHaveNewReward();
            manager.SetNotifierRewardCount(0);
            CacheRewardTrackState();
            OnUiDataChanged?.Invoke();
        }

        /// <summary>
        /// Refresh after scoring complete: actual counter and haveNewRewards = true)
        /// </summary>
        private void ActualizeRewardTrackState()
        {
            InitHaveNewReward();
            ActualizeNotifiersData();
            CacheRewardTrackState();
            OnUiDataChanged?.Invoke();
        }

        public void ScheduleServerRewardPopup(LeaderboardReceivedReward serverLeaderboardReward)
        {
            this.serverLeaderboardReward = serverLeaderboardReward;
        }

        public void SetFinishPopupLogicCompleted(bool value)
        {
            isFinishPopupLogicCompleted = value;
        }

        public void ResetFinishLogicCompletion()
        {
            SetFinishPopupLogicCompleted(false);
        }

        public void MarkRewardTrackPopupShown()
        {
            appliedRewardTrackVisual.Clear();
        }

        public void MarkServerRewardPopupShown()
        {
            serverLeaderboardReward = null;
        }

        public bool IsFinishLogicCompleted()
        {
            return isFinishPopupLogicCompleted && !IsRewardTrackPopupScheduled() && !IsServerRewardPopupScheduled();
        }

        public bool IsServerRewardPopupScheduled()
        {
            return serverLeaderboardReward != null;
        }

        public bool IsRewardTrackPopupScheduled()
        {
            return appliedRewardTrackVisual.Count > 0;
        }

        public async UniTask CacheLeaderboardStateAsync(CancellationToken cancellationToken)
        {
            leaderboardStateCached = await manager.GetLeaderboardPositionsStateAsync(cancellationToken);
            CacheLeaderboardWidgetText();
        }

        public void ActualizeLeaderboardPosition()
        {
            manager.SetLastLeaderboardIndex(leaderboardStateCached.actualIdx);
        }

        private void InitRewardTrackData()
        {
            CacheRewardTrackState();
            HaveFirstRewardTrack = rewardTrackStateCached.haveFirstRewardTrack;

            if (!scoringVisualizeService.IsScheduled(featureType))
            {
                InitHaveNewReward();
                ActualizeNotifiersData();
                CacheRewardTrackState();
            }
            else
            {
                HaveNewReward = false;
            }
        }

        private void CacheMultiplierState()
        {
            multiplierStateCached = manager.GetMultiplierState();
        }

        private void CacheRewardTrackState()
        {
            rewardTrackStateCached = rewardController.GetRewardTrackState();
        }

        private void InitHaveNewReward()
        {
            HaveNewReward = rewardTrackStateCached.newRewardCount > 0;
        }

        private void ActualizeNotifiersData()
        {
            manager.SetNotifierRewardIdx(rewardTrackStateCached.currentIdx);
            manager.SetNotifierRewardCount(rewardTrackStateCached.notifyCount + rewardTrackStateCached.newRewardCount);
        }

        private void CacheLeaderboardWidgetText()
        {
            CacheLeaderboardWidgetText(leaderboardStateCached.actualIdx);
        }

        private void CacheLeaderboardWidgetTextByPrevState()
        {
            CacheLeaderboardWidgetText(manager.GetSavedLeaderboardIndex());
        }

        private void CacheLeaderboardWidgetText(int playerIdx)
        {
            if (leaderboardStateCached.inProgressState || leaderboardStateCached.inFinishedWithPrize)
                LeaderboardWidgetPosition = (playerIdx + 1).ToString();
            else
                LeaderboardWidgetPosition = null;
        }

        private async UniTask CacheDataOnStateChangedAsync()
        {
            await CacheLeaderboardStateAsync(cts.Token);

            HaveNewReward = false;
            CacheRewardTrackState();
            CacheLeaderboardWidgetText();
            OnUiDataChanged?.Invoke();
        }

        private void ScoringVisualizeService_OnComplete(ScoringFeature featureType)
        {
            if (this.featureType != featureType)
                return;

            CacheLeaderboardWidgetText();
            ActualizeRewardTrackState();
        }

        private void CompetitionManager_OnStateChanged()
        {
            if (manager.IsProgressState())
                return;

            CacheDataOnStateChangedAsync().Forget();
        }
    }
}