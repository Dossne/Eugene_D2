using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.PlayerProfile;
using Features.Social;
using Infrastructure.CurrencyHud;
using Infrastructure.Popups;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using R3;
using UnityEngine;

namespace Features.Competition
{
    public class CompetitionStatePopupController
    {
        public event Action OnCloseStatePopup;
        public event Action OnCloseFinishPopup;

        private readonly CompetitionManager manager;
        private readonly CompetitionRewardController rewardController;
        private readonly CompetitionUIDataController uiDataController;

        private readonly PopupService popupService;
        private readonly CurrencyHudService currencyHudService;
        private readonly RewardVisualDataService rewardVisualDataService;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly PlayerProfileController playerProfileController;
        private readonly UserDataUpdateEvent userDataUpdateEvent;

        private readonly List<RewardTrackProgressDto> scheduledProgress = new();
        private readonly CompositeDisposable disposables;

        //config
        private readonly List<CompetitionMultiplierData> scoreMultipliers;
        private readonly bool isMultiplierEnabled;
        private readonly int leaderboardCount;

        private readonly CancellationTokenSource popupCts;
        private CancellationTokenSource animationCts;
        private CompetitionStatePopup statePopup;
        private CompetitionFinishPopup finishPopup;
        private int scheduledLeaderboardIdx;
        private bool isInit;

        public CompetitionStatePopupController(CompetitionManager manager,
                                               CompetitionRewardController rewardController,
                                               CompetitionUIDataController uiDataController,
                                               PopupService popupService,
                                               CurrencyHudService currencyHudService,
                                               RewardVisualDataService rewardVisualDataService,
                                               SpriteAtlasService spriteAtlasService,
                                               PlayerProfileController playerProfileController,
                                               UserDataUpdateEvent userDataUpdateEvent,
                                               List<CompetitionMultiplierData> scoreMultipliers,
                                               int leaderboardCount,
                                               bool isMultiplierEnabled)
        {
            this.manager = manager;
            this.rewardController = rewardController;
            this.uiDataController = uiDataController;
            this.popupService = popupService;
            this.currencyHudService = currencyHudService;
            this.rewardVisualDataService = rewardVisualDataService;
            this.spriteAtlasService = spriteAtlasService;
            this.playerProfileController = playerProfileController;
            this.userDataUpdateEvent = userDataUpdateEvent;
            this.scoreMultipliers = new List<CompetitionMultiplierData>(scoreMultipliers);
            this.isMultiplierEnabled = isMultiplierEnabled;
            this.leaderboardCount = leaderboardCount;

            this.popupCts = new CancellationTokenSource();
            this.disposables = new CompositeDisposable();
        }

#region Public Methods

        public void Initialize()
        {
            if (isInit)
                return;

            userDataUpdateEvent.Subscribe(_ => { UserDataUpdateEvent_Executed(userDataUpdateEvent.PlayerMetaData, userDataUpdateEvent.DisplayName); }).AddTo(disposables);

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            CancelAnimations();
            DeinitializeStatePopup();
            DeinitializeStateFinishPopup();

            popupCts.Cancel();
            popupCts.Dispose();

            OnCloseStatePopup = null;
            OnCloseFinishPopup = null;

            isInit = false;
        }

        public UniTask OpenStatePopupAsync(CancellationToken cancellationToken)
        {
            return OpenStatePopupAsync_Internal(cancellationToken);
        }

        public UniTask OpenFinishStatePopupAsync(CancellationToken cancellationToken)
        {
            return OpenFinishStatePopupAsync_Internal(cancellationToken);
        }

        public void ResyncTimer()
        {
            if (IsStatePopupOpened())
                statePopup.RefreshTimer(manager.GetTimeRest());
        }

#endregion

#region StatePopup

        private async UniTask OpenStatePopupAsync_Internal(CancellationToken cancellationToken)
        {
            if (statePopup == null)
            {
                statePopup = await popupService.GetAsync<CompetitionStatePopup>(cancellationToken, false, false);
                ConstructStatePopup(statePopup, true);
                statePopup.Initialize();
                statePopup.OnInfoRequested += CompetitionStatePopup_OnInfoRequested;
                statePopup.OnChangeState += CompetitionStatePopup_OnChangeState;
                statePopup.OnSlotClicked += CompetitionStatePopup_OnSlotClicked;
            }

            if (rewardController.TryGetCurrentRewardTrackState(out RewardTrackProgressDto currentProgress))
            {
                statePopup.SetRewardTarget(currentProgress.rewardVisual.icon,
                                           currentProgress.rewardVisual.amountText,
                                           currentProgress.rewardVisual.isDisplayRibbon,
                                           currentProgress.rewardVisual.isDisplayInfinityIcon);

                statePopup.RefreshProgress(currentProgress.collectedCountOnEnd, currentProgress.targetCountByConfig);
            }
            else
            {
                statePopup.SetSliderToMax();
            }

            statePopup.RefreshTimer(manager.GetTimeRest());
            statePopup.SwitchRewardTrackToCompleteState(rewardController.IsRewardTrackComplete());

            if (isMultiplierEnabled)
            {
                var multiplierState = uiDataController.MultiplierState;
                statePopup.SetMultiplierIdx(multiplierState.multiplierPrevIdx);
            }

            await RefreshSlotViewsForStatePopupAsync();

            await statePopup.OpenAsync(cancellationToken);
        }

        private void ExecuteScheduledAnimationsForStatePopup()
        {
            PlayMultiplierAnimation();
            PlayRewardTrackAnimationsAsync(statePopup.RewardTrackProgress).Forget();
            AnimatedScrollSlotInStatePopup();
        }

        private void AnimatedScrollSlotInStatePopup()
        {
            statePopup.PlayScrollAnimation(scheduledLeaderboardIdx);
            uiDataController.ActualizeLeaderboardPosition();
        }

        private void PlayMultiplierAnimation()
        {
            if (!isMultiplierEnabled)
                return;

            MultiplierStateDto multiplierState = uiDataController.MultiplierState;

            if (!multiplierState.IsMultiplierChanged())
                return;

            statePopup.SetCursorAnimated(multiplierState.multiplierCurIdx);
            uiDataController.ActualizeMultiplierState();
        }

        private async UniTask RefreshSlotViewsForStatePopupAsync()
        {
            await uiDataController.CacheLeaderboardStateAsync(popupCts.Token);
            var actualIdx = uiDataController.LeaderBoardState.actualIdx;
            var leaderboardMaxIdx = Math.Max(0, uiDataController.LeaderBoardState.records.Count - 1);
            var prevSavedIdx = Math.Min(uiDataController.LeaderBoardState.prevSavedIdx, leaderboardMaxIdx);
            var isPosIncremented = uiDataController.LeaderBoardState.actualIdx < prevSavedIdx;
            scheduledLeaderboardIdx = isPosIncremented ? actualIdx : prevSavedIdx;

            statePopup.SetPlayerIdx(prevSavedIdx);

            var recordsTemp = new List<LeaderboardRecord>(uiDataController.LeaderBoardState.records);
            if (isPosIncremented)
            {
                SwapDataBeforeAnimations(recordsTemp, actualIdx, prevSavedIdx);
            }

            statePopup.RefreshSlotsViews(recordsTemp, spriteAtlasService);
            var targetIdx = statePopup.PlayerPosIdx > scheduledLeaderboardIdx ? statePopup.PlayerPosIdx : 0;
            statePopup.InstantScroll(targetIdx);
        }

        private void SwapDataBeforeAnimations(List<LeaderboardRecord> leaderboardRecords, int removeIdx, int insertIntoIdx)
        {
            if (leaderboardRecords.Count <= 1)
                return;

            var item = leaderboardRecords[removeIdx];
            leaderboardRecords.RemoveAt(removeIdx);
            leaderboardRecords.Insert(insertIntoIdx, item);
        }

        private void DeinitializeStatePopup()
        {
            if (statePopup == null)
                return;

            statePopup.OnInfoRequested -= CompetitionStatePopup_OnInfoRequested;
            statePopup.OnChangeState -= CompetitionStatePopup_OnChangeState;
            statePopup.OnSlotClicked -= CompetitionStatePopup_OnSlotClicked;

            popupService.Dispose(statePopup);
        }

        private bool IsStatePopupOpened()
        {
            return statePopup != null && statePopup.IsOpened;
        }

#endregion

#region StateFinishPopup

        private async UniTask OpenFinishStatePopupAsync_Internal(CancellationToken cancellationToken)
        {
            if (finishPopup == null)
            {
                finishPopup = await popupService.GetAsync<CompetitionFinishPopup>(cancellationToken, false, false);
                ConstructStatePopup(finishPopup, false);
                finishPopup.Initialize();
                finishPopup.OnInfoRequested += CompetitionStatePopup_OnInfoRequested;
                finishPopup.OnChangeState += CompetitionFinishPopup_OnChangeState;
                finishPopup.OnSlotClicked += CompetitionStatePopup_OnSlotClicked;
            }

            if (rewardController.TryGetCurrentRewardTrackState(out RewardTrackProgressDto currentProgress))
            {
                finishPopup.SetRewardTarget(currentProgress.rewardVisual.icon,
                                            currentProgress.rewardVisual.amountText,
                                            currentProgress.rewardVisual.isDisplayRibbon,
                                            currentProgress.rewardVisual.isDisplayInfinityIcon);

                finishPopup.RefreshProgress(currentProgress.collectedCountOnEnd, currentProgress.targetCountByConfig);
            }
            else
            {
                finishPopup.SetSliderToMax();
            }

            await uiDataController.CacheLeaderboardStateAsync(popupCts.Token);
            uiDataController.ActualizeLeaderboardPosition();
            var records = uiDataController.LeaderBoardState.records;
            var playerPosIdx = uiDataController.LeaderBoardState.actualIdx;

            (bool isServerAvailable, LeaderboardReceivedReward serverReward) = await rewardController.GetServerReward(popupCts.Token);

            var haveServerReward = serverReward != null;
            var isClaimedOnServer = false;

            if (haveServerReward)
            {
                manager.SendServerRewardAvailableAnalytics(serverReward.place);

                rewardController.TryGetServerReward(serverReward.place - 1, out var rewardConfigData);

                isClaimedOnServer = await rewardController.TryClaimReceivedRewardAsync(serverReward.letterId, popupCts.Token);

                if (isClaimedOnServer)
                {
                    rewardController.ApplyServerReward(rewardConfigData.rewardJson);

                    var rewardFullInfo = rewardVisualDataService.GetContainerData(rewardConfigData.rewardJson);
                    finishPopup.SetResultReward(true, rewardFullInfo);

                    uiDataController.ScheduleServerRewardPopup(serverReward);
                }
            }
            else
            {
                finishPopup.SetResultReward(false, null);
            }

            var actualized = manager.GetActualizedRecords(records, haveServerReward ? serverReward.place - 1 : -1);
            playerPosIdx = actualized.newPlayerPosIdx;
            records = actualized.records;
            
            uiDataController.SetFinishPopupLogicCompleted(isServerAvailable && (!haveServerReward || isClaimedOnServer));

            finishPopup.SetRankText(playerPosIdx + 1);
            finishPopup.SwitchRewardTrackToCompleteState(rewardController.IsRewardTrackComplete());
            RefreshSlotViewsForFinishPopup(records, playerPosIdx);

            await finishPopup.OpenAsync(cancellationToken);
        }

        private void RefreshSlotViewsForFinishPopup(List<LeaderboardRecord> leaderboardRecords, int currentIdx)
        {
            finishPopup.SetPlayerIdx(currentIdx);
            finishPopup.RefreshSlotsViews(leaderboardRecords, spriteAtlasService);
            finishPopup.InstantScroll(currentIdx);
        }

        private void DeinitializeStateFinishPopup()
        {
            if (finishPopup == null)
                return;

            finishPopup.OnInfoRequested -= CompetitionStatePopup_OnInfoRequested;
            finishPopup.OnChangeState -= CompetitionFinishPopup_OnChangeState;
            finishPopup.OnSlotClicked -= CompetitionStatePopup_OnSlotClicked;

            popupService.Dispose(finishPopup);
        }

#endregion

#region StatesPopupSharedLogic

        private async UniTaskVoid PlayRewardTrackAnimationsAsync(CompetitionRewardTrackProgress rewardTrackProgress)
        {
            if (!rewardController.TryGetScheduledProgressDto(scheduledProgress))
                return;

            foreach (var progressDto in scheduledProgress)
            {
                if (progressDto.CanGiveReward())
                    uiDataController.AddToScheduleRewardTrackPopup(progressDto.rewardVisual);
            }

            rewardController.ApplyProgress(scheduledProgress);

            rewardController.TryGetCurrentRewardTrackState(out RewardTrackProgressDto currentProgress);
            animationCts = new CancellationTokenSource();

            for (var i = 0; i < scheduledProgress.Count; i++)
            {
                var progress = scheduledProgress[i];

                await rewardTrackProgress.RewardTrackSliderAnimatedMoveAsync(progress.collectedCountOnStart,
                                                                             progress.collectedCountOnEnd,
                                                                             progress.targetCountByConfig,
                                                                             animationCts.Token);

                if (progress.CanGiveReward())
                {
                    var next = i < scheduledProgress.Count - 1 ? scheduledProgress[i + 1] : currentProgress;

                    rewardTrackProgress.PlayDownUpScaleRewardIcon(() =>
                    {
                        if (next != null)
                        {
                            rewardTrackProgress.SetRewardTarget(next.rewardVisual.icon,
                                                                next.rewardVisual.amountText,
                                                                next.rewardVisual.isDisplayRibbon,
                                                                next.rewardVisual.isDisplayInfinityIcon);
                        }
                    });

                    await UniTask.WaitForSeconds(rewardTrackProgress.RewardIconDurationTotal, true, cancellationToken: animationCts.Token);
                }
            }

            if (currentProgress != null)
                rewardTrackProgress.RefreshProgress(currentProgress.collectedCountOnEnd, currentProgress.targetCountByConfig);

            rewardTrackProgress.SwitchRewardTrackToCompleteState(rewardController.IsRewardTrackComplete());
        }

        private void CancelAnimations()
        {
            animationCts?.Cancel();
            animationCts?.Dispose();
            animationCts = null;
        }

        private void ConstructStatePopup(CompetitionStatePopup popup, bool isMultiplierRootEnable)
        {
            var rewardContainers = new List<RewardContainerVisualData>();

            var serverRewardsConfig = rewardController.GetServerRewardsConfig();

            foreach (var rewardData in serverRewardsConfig)
            {
                rewardContainers.Add(rewardVisualDataService.GetContainerData(rewardData.rewardJson));
            }

            var multiplierTexts = new List<string>();

            if (isMultiplierRootEnable && isMultiplierEnabled)
            {
                foreach (var item in scoreMultipliers)
                {
                    multiplierTexts.Add($"{statePopup.MultiplierPrefix}{item.value}");
                }
            }

            var rewardTracksConfig = rewardController.GetRewardTracksConfig();
            popup.Construct(rewardContainers, multiplierTexts, rewardController, spriteAtlasService, rewardTracksConfig, leaderboardCount);
            popup.SetMultiplierObjectActive(isMultiplierRootEnable && isMultiplierEnabled);
        }

#endregion

#region EventHandlers

        private void CompetitionStatePopup_OnInfoRequested()
        {
            popupService.OpenAsync<CompetitionInfoPopup>(popupCts.Token, false, false).Forget();
        }

        private void CompetitionStatePopup_OnChangeState(PopupBase popup, PopupState state)
        {
            switch (state)
            {
                case PopupState.Opened:
                    uiDataController.ResetNotifier();
                    ExecuteScheduledAnimationsForStatePopup();
                    break;
                case PopupState.BeginClose:
                {
                    OnCloseStatePopup?.Invoke();
                    break;
                }
                case PopupState.Closed:
                    CancelAnimations();
                    break;
            }
        }

        private void CompetitionFinishPopup_OnChangeState(PopupBase popup, PopupState state)
        {
            switch (state)
            {
                case PopupState.Opened:
                    PlayRewardTrackAnimationsAsync(finishPopup.RewardTrackProgress).Forget();
                    break;
                case PopupState.BeginClose:
                {
                    OnCloseFinishPopup?.Invoke();
                    break;
                }
                case PopupState.Closed:
                    CancelAnimations();
                    break;
            }
        }

        private void CompetitionStatePopup_OnSlotClicked(int slotIdx)
        {
            var records = uiDataController.LeaderBoardState.records;

            if (slotIdx >= records.Count)
                return;

            LeaderboardRecord record = uiDataController.LeaderBoardState.records[slotIdx];

            var firstWinCount = record.metaData.playerStatistics.firstWinCount;
            var totalWinCount = record.metaData.playerStatistics.totalWinCount;
            var maxWinStreak = record.metaData.playerStatistics.maxWinStreak;
            var level = totalWinCount + 1;

            playerProfileController.OpenProfilePopupAsync(record.metaData.playerAvatar.avatarId,
                                                          record.displayName,
                                                          level,
                                                          firstWinCount,
                                                          totalWinCount,
                                                          maxWinStreak,
                                                          record.IsPlayer(),
                                                          popupCts.Token)
                                   .Forget();
        }

        private void UserDataUpdateEvent_Executed(PlayerMetaData playerMetaData, string displayName)
        {
            if (statePopup != null && statePopup.IsOpened)
            {
                statePopup.RefreshPlayerData(spriteAtlasService.GetFromMain(playerMetaData.playerAvatar.avatarId), displayName);
            }
            else if (finishPopup != null && finishPopup.IsOpened)
            {
                finishPopup.RefreshPlayerData(spriteAtlasService.GetFromMain(playerMetaData.playerAvatar.avatarId), displayName);
            }
        }

#endregion
    }
}