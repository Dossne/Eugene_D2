using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Competition.Reward;
using Features.PurchaseUi;
using Infrastructure.Collections;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.Reward;

namespace Features.Competition
{
    public class CompetitionRewardPopupController
    {
        private readonly CompetitionRewardController rewardController;
        private readonly CompetitionUIDataController uiDataController;
        private readonly PopupService popupService;
        private readonly CancellationTokenSource popupCts;
        private readonly CompetitionManager manager;
        private readonly CompetitionStatePopupController statePopupController;
        private CompetitionStageRewardPopup rewardPopup;

        private bool isInit;
        private bool finishPopupWatched;

        public CompetitionRewardPopupController(CompetitionManager manager,
                                                CompetitionRewardController rewardController,
                                                CompetitionStatePopupController statePopupController,
                                                CompetitionUIDataController uiDataController,
                                                PopupService popupService)
        {
            this.rewardController = rewardController;
            this.uiDataController = uiDataController;
            this.popupService = popupService;
            this.manager = manager;
            this.statePopupController = statePopupController;
            this.popupCts = new CancellationTokenSource();
        }

        public void Initialize()
        {
            if (isInit)
                return;

            statePopupController.OnCloseStatePopup += CompetitionStatePopupController_OnCloseStatePopup;
            statePopupController.OnCloseFinishPopup += CompetitionStatePopupController_OnCloseFinishPopup;
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            popupService.Dispose(rewardPopup);

            statePopupController.OnCloseStatePopup -= CompetitionStatePopupController_OnCloseStatePopup;
            statePopupController.OnCloseFinishPopup -= CompetitionStatePopupController_OnCloseFinishPopup;

            isInit = false;
        }

        private bool TryOpenServerRewardPopup()
        {
            if (!uiDataController.IsServerRewardPopupScheduled())
                return false;

            OpenServerRewardPopupAsync().Forget();
            return true;
        }

        private async UniTask CreateRewardsPopup()
        {
            if (rewardPopup == null)
            {
                rewardPopup = await popupService.GetAsync<CompetitionStageRewardPopup>(popupCts.Token);
                rewardPopup.Initialize();
            }
        }

        private async UniTaskVoid OpenStageRewardPopupAsync()
        {
            await CreateRewardsPopup();

            rewardPopup.Construct(() =>
            {
                if (!TryOpenServerRewardPopup())
                    CloseRewardPopup();
            }, LocKeys.Competition.StageRewardPopupHeader);

            RewardItemVisualData mainRewardData = null;
            var boosterRewards = new List<RewardItemVisualData>();
            var preBoosterRewards = new List<RewardItemVisualData>();

            MergeEqualItems(uiDataController.AppliedRewardTrackVisual);
            SplitItemsIntoLists(uiDataController.AppliedRewardTrackVisual, ref mainRewardData, boosterRewards, preBoosterRewards);
            rewardPopup.SetRewards(mainRewardData, boosterRewards, preBoosterRewards);
            rewardPopup.Open();
            uiDataController.MarkRewardTrackPopupShown();
        }

        private async UniTaskVoid OpenServerRewardPopupAsync()
        {
            if (!rewardController.TryGetServerReward(uiDataController.ServerLeaderboardReward.place - 1, out var rewardConfigData))
            {
                uiDataController.MarkServerRewardPopupShown();
                return;
            }

            await CreateRewardsPopup();

            rewardPopup.Construct(CloseRewardPopup, LocKeys.Competition.ServerRewardPopupHeader);

            RewardContainerVisualData container = rewardController.GetContainerData(rewardConfigData.rewardJson);

            using PooledList<RewardItemVisualData> allRewards = PooledList<RewardItemVisualData>.Get();

            if (container.containerRewards != null && container.containerRewards.Count > 0)
                allRewards.AddRange(container.containerRewards);

            if (container.rewards != null && container.rewards.Count > 0)
                allRewards.AddRange(container.rewards);

            RewardItemVisualData mainRewardData = null;
            var boosterRewards = new List<RewardItemVisualData>();
            var preBoosterRewards = new List<RewardItemVisualData>();

            MergeEqualItems(allRewards);
            SplitItemsIntoLists(allRewards, ref mainRewardData, boosterRewards, preBoosterRewards);

            rewardPopup.SetRewards(mainRewardData, boosterRewards, preBoosterRewards);
            rewardPopup.Open();
            uiDataController.MarkServerRewardPopupShown();
        }

        private void CloseRewardPopup()
        {
            rewardPopup.Close();
            OpenNextPopupOrCompleteCompetition();
        }

        private void OpenNextPopupOrCompleteCompetition()
        {
            if (uiDataController.IsRewardTrackPopupScheduled())
            {
                OpenStageRewardPopupAsync().Forget();
            }
            else if (uiDataController.IsServerRewardPopupScheduled())
            {
                OpenServerRewardPopupAsync().Forget();
            }
            else if (finishPopupWatched && manager.IsFinishedTimeState() && uiDataController.IsFinishLogicCompleted())
            {
                manager.SetNextStateAfterFinish();
                uiDataController.ResetFinishLogicCompletion();
            }
        }

        private void MergeEqualItems(List<RewardItemVisualData> input)
        {
            RewardVisualDataService.MergeEqualItems(input);
        }

        private void SplitItemsIntoLists(List<RewardItemVisualData> inputList,
                                         ref RewardItemVisualData mainRewardData,
                                         List<RewardItemVisualData> boosterRewards,
                                         List<RewardItemVisualData> preBoosterRewards)
        {
            foreach (RewardItemVisualData rewardVisual in inputList)
            {
                switch (rewardVisual.rewardType)
                {
                    case PurchaseRewardType.Currency:
                        mainRewardData ??= rewardVisual;
                        break;
                    case PurchaseRewardType.Booster:
                        boosterRewards.Add(rewardVisual);
                        break;
                    case PurchaseRewardType.PreBooster or PurchaseRewardType.InfinitePreBooster or PurchaseRewardType.InfiniteLife:
                        preBoosterRewards.Add(rewardVisual);
                        break;
                }
            }
        }

        private void CompetitionStatePopupController_OnCloseStatePopup()
        {
            if (uiDataController.IsRewardTrackPopupScheduled())
            {
                OpenStageRewardPopupAsync().Forget();
            }
        }

        private void CompetitionStatePopupController_OnCloseFinishPopup()
        {
            finishPopupWatched = true;
            OpenNextPopupOrCompleteCompetition();
        }
    }
}