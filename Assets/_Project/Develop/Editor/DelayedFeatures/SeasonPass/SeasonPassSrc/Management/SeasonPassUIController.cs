using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Popups;
using Infrastructure.Reward;
using Infrastructure.SystemsLifeCycle;
using UnityEngine;

namespace Features.SeasonPass
{
    public class SeasonPassUIController : ISystemTickable
    {
        private readonly SeasonPassWidget widget;
        private readonly SeasonPassStateController stateController;
        private readonly PopupService popupService;
        private readonly CancellationTokenSource cts;
        private SeasonPassStatePopup seasonPassStatePopup;
        private bool isInit;

        public SeasonPassUIController(MainUIProvider            mainUIProvider,
                                      SeasonPassStateController stateController,
                                      PopupService              popupService)
        {
            //this.widget          = mainUIProvider.HudProvider.SeasonPassWidget;
            this.stateController = stateController;
            this.popupService    = popupService;

            this.cts = new CancellationTokenSource();
        }

        public void Initialize()
        {
            if (isInit || !stateController.IsFeatureEnabled())
                return;

            SetWidgetState();

            if (stateController.CanShowWidget())
            {
                InitializeUIControllerModules();
            }

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            stateController.OnStateChanged   -= SeasonPassStateController_OnChangeState;
            stateController.OnChangeProgress -= SeasonPassStateController_OnChangeProgress;

            if (seasonPassStatePopup != null)
            {
                seasonPassStatePopup.Deinitialize();
            }

            widget.Deinitialize();

            isInit = false;
        }

        void ISystemTickable.Tick() { }

        //public async UniTask ExecuteScheduledAsync(CancellationToken token) { }

        private void SetWidgetState()
        {
            bool isActive = stateController.CanShowWidget() && !stateController.IsCompleteState();
            widget.SetObjectActive(isActive);

            if (isActive)
            {
                widget.SetUnlockedState(stateController.IsUnlocked());
            }
        }

        private void InitializeUIControllerModules()
        {
            widget.SetAction(DoWidgetActionByState);
            widget.Initialize();

            stateController.OnStateChanged   += SeasonPassStateController_OnChangeState;
            stateController.OnChangeProgress += SeasonPassStateController_OnChangeProgress;
        }

        private void DoWidgetActionByState()
        {
            if (!stateController.IsUnlocked())
            {
                widget.ShowTooltip(LocalizationService.I.Get(LocKeys.SeasonPass.WidgetTooltip, stateController.GetUnlockLevel().ToString()));
                return;
            }

            OpenSeasonPassStatePopup(cts.Token).Forget();
        }

        private async UniTaskVoid OpenSeasonPassStatePopup(CancellationToken token)
        {
            if (seasonPassStatePopup == null)
            {
                seasonPassStatePopup = await popupService.GetAsync<SeasonPassStatePopup>(token, false, false);

                var isRewardExist = stateController.TryGetRewardsData(out List<SeasonPassRewardData> rewardDatas);

                if (!isRewardExist)
                {
                    Debug.LogWarning("[SeasonPass] Season pass popup could not be opened. Reward data is not available.");
                    return;
                }

                var viewDatas = new ScrollElementDataDto [rewardDatas.Count];

                for (var i = 0; i < rewardDatas.Count; i++)
                {
                    stateController.TryGetSlotClaimState(i, out SlotClaimState claimState);
                    var slots = new List<SlotViewData>
                    {
                        new SlotViewData
                        {
                            stepIdx    = i,
                            viewType   = SlotViewType.Free,
                            rewardInfo = stateController.GetFirstRewardView(rewardDatas[i].rewardJson),
                            stateType  = GetSlotViewStateType(i, SlotViewType.Free, claimState)
                        },

                        new SlotViewData
                        {
                            stepIdx    = i,
                            viewType   = SlotViewType.InAppPremium,
                            rewardInfo = stateController.GetFirstRewardView(rewardDatas[i].premiumRewardJson),
                            stateType  = GetSlotViewStateType(i, SlotViewType.InAppPremium, claimState)
                        },
                    };

                    viewDatas[i] = new ScrollElementDataDto(slots);
                }

                seasonPassStatePopup.Construct(viewDatas);
                seasonPassStatePopup.Initialize();

                seasonPassStatePopup.OnClicked += SeasonPassStatePopup_OnClicked;
            }

            seasonPassStatePopup.Open();
        }

        private SlotViewStateType GetSlotViewStateType(int checkStepIdx, SlotViewType viewType, SlotClaimState claimState)
        {
            if (claimState != null && claimState.IsClaimed(viewType))
                return SlotViewStateType.Claimed;

            if (!stateController.IsUnlockedByProgress(checkStepIdx))
                return SlotViewStateType.LockedByProgress;

            if (viewType is SlotViewType.InAppPremium && !stateController.IsPremiumBought())
                return SlotViewStateType.LockedByInApp;

            return SlotViewStateType.Available;
        }

        private void ShowLockProgressTooltip(SlotViewData slotViewData)
        {
            if (!seasonPassStatePopup.TryGetTooltip(slotViewData.viewType, out var tooltip))
            {
                Debug.LogError($"[SeasonPass] \"{slotViewData.viewType.ToString()}\". Tooltip could not be found.");
                return;
            }

            var text = LocalizationService.I.Get(LocKeys.SeasonPass.LockProgressTooltip);
            tooltip.Show(text, slotViewData.tooltipTarget.position);
        }

        private void ShowLockInAppTooltip(SlotViewData slotViewData)
        {
            if (!seasonPassStatePopup.TryGetTooltip(slotViewData.viewType, out var tooltip))
            {
                Debug.LogError($"[SeasonPass] \"{slotViewData.viewType.ToString()}\". Tooltip could not be found.");
                return;
            }

            var text = LocalizationService.I.Get(LocKeys.SeasonPass.LockInAppTooltip);
            tooltip.Show(text, slotViewData.tooltipTarget.position);
        }

        private void SeasonPassStateController_OnChangeState() { }

        private void SeasonPassStateController_OnChangeProgress() { }

        private void SeasonPassStatePopup_OnClicked(SlotViewData slotViewData)
        {
            if (slotViewData.stateType is SlotViewStateType.LockedByProgress)
            {
                ShowLockProgressTooltip(slotViewData);
                return;
            }

            else if (slotViewData.stateType is SlotViewStateType.LockedByInApp)
            {
                ShowLockInAppTooltip(slotViewData);
                return;
            }

            Debug.Log($"[SeasonPass] \"{slotViewData.viewType.ToString()}\" slot {slotViewData.stepIdx} index is clicked. SlotState: {slotViewData.stateType}");
        }
    }
}