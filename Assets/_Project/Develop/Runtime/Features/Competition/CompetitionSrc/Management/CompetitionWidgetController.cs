using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.HudLevelButtons;
using Features.Social;
using Features.Warnings;
using Features.Widgets;
using Infrastructure.Localization;
using Infrastructure.Pool.FloatingText;
using Infrastructure.Popups;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.Competition
{
    /// <summary>
    /// Meta scope
    /// </summary>
    public class CompetitionWidgetController
    {
        private readonly CompetitionManager manager;
        private readonly CompetitionStatePopupController statePopupController;
        private readonly CompetitionUIDataController uiDataController;
        private readonly WidgetManager widgetManager;
        private readonly PopupService popupService;
        private readonly StartLevelButtonController startLevelButtonController;
        private readonly WarningService warningService;
        private readonly WidgetId widgetId;
        private readonly string widgetAssetGuid;

        private readonly CancellationTokenSource cts;
        private CompetitionWidget widget;

        private float refreshSec;
        private bool isTickTimer;
        private bool isBusyOnTap;
        private bool isInit;

        public CompetitionWidgetController(CompetitionManager manager,
                                           CompetitionStatePopupController statePopupController,
                                           CompetitionUIDataController uiDataController,
                                           WidgetManager widgetManager,
                                           PopupService popupService,
                                           StartLevelButtonController startLevelButtonController,
                                           WarningService warningService,
                                           WidgetId widgetId,
                                           string widgetAssetGuid)
        {
            this.manager = manager;
            this.statePopupController = statePopupController;
            this.uiDataController = uiDataController;
            this.widgetManager = widgetManager;
            this.popupService = popupService;
            this.startLevelButtonController = startLevelButtonController;
            this.warningService = warningService;
            this.widgetId = widgetId;
            this.widgetAssetGuid = widgetAssetGuid;
            cts = new CancellationTokenSource();
        }

        public async UniTask InitializeAsync(CancellationToken token)
        {
            if (isInit)
                return;

            if (!manager.IsFeatureEnabled())
                return;

            if (!manager.CanShowWidget())
                return;

            await InitializeModulesAsync(token);
            widget.DeinitializePanels();
            widget.SetObjectActive(IsWidgetActiveState() || IsWidgetShowInLockedState());

            manager.OnStateChanged += CompetitionManager_OnStateChanged;
            uiDataController.OnUiDataChanged += CompetitionUiDataController_OnUiDataChanged;
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            cts.Cancel();
            cts.Dispose();

            if (widget != null)
            {
                widget.Deinitialize();
                widget.DeinitializePanels();
            }

            manager.OnStateChanged -= CompetitionManager_OnStateChanged;
            uiDataController.OnUiDataChanged -= CompetitionUiDataController_OnUiDataChanged;

            isTickTimer = false;

            isInit = false;
        }

        public void Tick()
        {
            if (!isTickTimer)
                return;

            refreshSec -= Time.unscaledDeltaTime;

            if (refreshSec > 0)
                return;

            RefreshWidgetTimerText();
            refreshSec = 1;
        }

        public async UniTask ExecuteScheduledAsync(CancellationToken token)
        {
            if (!isInit)
                return;

            var multiplierState = uiDataController.MultiplierState;

            if (!uiDataController.HaveFirstRewardTrack && !multiplierState.IsMultiplierChanged())
                return;

            widget.InitializePanels();

            if (uiDataController.HaveFirstRewardTrack)
            {
                var rewardVisualInfo = uiDataController.FirstRewardVisualInfo;
                widget.ConstructRewardPanel(rewardVisualInfo);
                widget.InitializeRewardPanel();
            }

            if (multiplierState.IsMultiplierChanged())
            {
                widget.ConstructMultiplierPanel(multiplierState.multiplierPrevNum, multiplierState.multiplierCurNum);
                widget.InitializeMultiplierPanel();
            }

            try
            {
                if (uiDataController.HaveFirstRewardTrack)
                    await widget.PlayRewardAnimationAsync(token);

                if (multiplierState.IsMultiplierChanged())
                    await widget.PlayMultiplierAnimationAsync(token);
            }
            catch (OperationCanceledException)
            {
                //
            }
            finally
            {
                widget.DeinitializePanels();
            }
        }

        public void ResyncWidget()
        {
            if (widget == null)
                return;

            var haveNotifier = uiDataController.HaveNewReward || uiDataController.IsFinishedWithPrize;
            var isTimerText = !haveNotifier && manager.IsTimerState();

            if (!isTimerText)
                return;

            RefreshWidgetTimerText();
        }

        private async UniTask InitializeModulesAsync(CancellationToken token)
        {
            widget = await widgetManager.GetOrAddWidgetAsync<CompetitionWidget>(widgetId, widgetAssetGuid, null, null, true, token);

            RefreshWidgetView();

            widget.SetAction(DoWidgetActionByState);
            widget.Initialize();
        }

        private void DoWidgetActionByState()
        {
            DoWidgetActionByStateAsync().Forget();
        }

        private async UniTask DoWidgetActionByStateAsync()
        {
            if (isBusyOnTap)
            {
                ShowAlertBusy();
                return;
            }

            if (!manager.IsUnlocked())
            {
                widget.ShowTooltip(LocalizationService.I.Get(LocKeys.Competition.WidgetTooltip, manager.GetUnlockLevel().ToString()));
                return;
            }

            if (manager.IsNoneState())
            {
                ShowAlertNoConnect();
                return;
            }

            isBusyOnTap = true;

            try
            {
                await manager.ResyncTimeTillResetByServerAsync(cts.Token);

                if (manager.IsQualificationState())
                {
                    await OpenBeginPopupAsync(cts.Token);
                }
                else if (manager.IsFinishedTimeState())
                {
                    var reward = await manager.GetRewardAsync(cts.Token);

                    if (reward.isAvailableOnServer)
                    {
                        await statePopupController.OpenFinishStatePopupAsync(cts.Token);
                    }
                    else
                    {
                        ShowAlertNoConnect();
                    }
                }
                else
                {
                    await statePopupController.OpenStatePopupAsync(cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                isBusyOnTap = false;
            }
        }

        private void RefreshWidgetView()
        {
            var rewardState = uiDataController.RewardTrackState;
            var haveNotifier = uiDataController.HaveNewReward || uiDataController.IsFinishedWithPrize;
            isTickTimer = !haveNotifier && manager.IsTimerState();

            SetWidgetNotificator(rewardState.notifyCount);
            SetWidgetMainText(haveNotifier);
            widget.SetLeaderboardPositionText(uiDataController.LeaderboardWidgetPosition);
            SetWidgetVisualState(haveNotifier);
        }

        private void SetWidgetNotificator(int notifierCount)
        {
            bool haveRewardsToGive = notifierCount > 0;

            widget.SetNotifierActive(haveRewardsToGive);

            if (haveRewardsToGive)
            {
                widget.SetNotifierText(notifierCount.ToString());
            }
        }

        private void SetWidgetMainText(bool haveNewReward)
        {
            if (!manager.IsUnlocked())
            {
                widget.SetText(LocalizationService.I.Get(LocKeys.Competition.WidgetLevel, manager.GetUnlockLevel().ToString()));
            }
            else if (haveNewReward)
            {
                widget.SetText(LocalizationService.I.Get(LocKeys.Competition.WidgetOpen));
            }
            else if (manager.IsFinishedTimeState())
            {
                widget.SetText(LocalizationService.I.Get(LocKeys.Competition.WidgetFinished));
            }
            else if (manager.IsTimerState())
            {
                RefreshWidgetTimerText();
            }
            else
            {
                widget.SetText(null);
            }
        }

        private void SetWidgetVisualState(bool haveNotification)
        {
            if (!manager.IsUnlocked())
            {
                widget.SeStateImages(WidgetState.Locked);
            }
            else if (haveNotification)
            {
                widget.SeStateImages(WidgetState.NewNotification);
            }
            else
            {
                widget.SeStateImages(WidgetState.Default);
            }
        }

        private void RefreshWidgetTimerText()
        {
            var timeRest = manager.GetTimeRest();
            string timeStr = timeRest < 86400 ? TimeUtils.GetTimeString(timeRest) : TimeUtils.GetTimeString(timeRest, 100f);
            widget.SetText(timeStr);
        }

        private void OnCloseBeginCompetitionPopup()
        {
            OnCloseBeginCompetitionPopupAsync().Forget();
        }

        private async UniTask OnCloseBeginCompetitionPopupAsync()
        {
            var haveInternetConnect = await manager.HaveInternetConnectAsync(cts.Token);

            if (haveInternetConnect)
            {
                startLevelButtonController.HandleStartLevelClick();
            }
            else
            {
                warningService.ShowAlert(LocalizationService.I.Get(LocKeys.Competition.AlertNoConnectionStart), FloatingTextType.WhiteAlert);
            }
        }

        private async UniTask OpenBeginPopupAsync(CancellationToken cancellationToken)
        {
            var popup = await popupService.OpenAsync<CompetitionBeginPopup>(cancellationToken);
            popup.Construct(OnCloseBeginCompetitionPopup);
            manager.SendStartPopupShownAnalytics();
        }

        private bool IsWidgetActiveState()
        {
            return !manager.IsCompletedState();
        }

        private bool IsWidgetShowInLockedState()
        {
            return manager.CanShowWidget() && !manager.IsUnlocked();
        }

        private void ShowAlertBusy()
        {
            var text = LocalizationService.I.Get(LocKeys.Competition.AlertWait);
            warningService.ShowAlert(text, FloatingTextType.RewAdsText, widget.IconPosition, false);
        }

        private void ShowAlertNoConnect()
        {
            warningService.ShowAlert(LocalizationService.I.Get(LocKeys.Competition.AlertNoConnection), FloatingTextType.WhiteAlert);
        }

        private void CompetitionUiDataController_OnUiDataChanged()
        {
            RefreshWidgetView();
        }

        private void CompetitionManager_OnStateChanged()
        {
            if (widget == null)
                return;

            bool isActive = IsWidgetActiveState();
            widget.SetObjectActive(isActive);
            isTickTimer = manager.IsTimerState();

            if (!isActive)
                return;

            RefreshWidgetView();
        }
    }
}