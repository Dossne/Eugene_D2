using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Widgets;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.LavaQuest
{
    /// <summary>
    /// Meta scene scope
    /// </summary>
    public class LavaQuestUIController : ISystemTickable
    {
        private readonly LavaQuestStateController stateController;
        private readonly WidgetManager widgetManager;
        private readonly LQConnectPlayersPopupController connectPlayersPopupController;
        private readonly LavaQuestEventPopupController eventPopupController;
        private readonly LavaQuestStartPopupController startPopupController;
        private readonly PopupService popupService;
        private readonly CancellationTokenSource cts;
        private LavaQuestWidget lavaQuestWidget;

        private float refreshTimeCurrent;
        private bool isInit;


        public LavaQuestUIController(LavaQuestStateController stateController,
                                     WidgetManager widgetManager,
                                     PopupService popupService,
                                     LQConnectPlayersPopupController connectPlayersPopupController,
                                     LavaQuestEventPopupController eventPopupController,
                                     LavaQuestStartPopupController startPopupController)
        {
            this.stateController = stateController;
            this.widgetManager = widgetManager;
            this.popupService = popupService;
            this.connectPlayersPopupController = connectPlayersPopupController;
            this.eventPopupController = eventPopupController;
            this.startPopupController = startPopupController;
            this.cts = new CancellationTokenSource();
        }


        public async UniTask InitializeAsync(CancellationToken token)
        {
            if (isInit || !stateController.IsFeatureEnabled())
                return;

            if (stateController.CanShowWidget())
            {
                await InitializeUIControllerModulesAsync(token);
                SetWidgetState();
            }
            

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            cts.Cancel();
            cts.Dispose();

            stateController.ApplyScheduledAction();
            stateController.OnChangeState -= LavaQuestStateController_OnChangeState;
            stateController.OnChangeProgress -= LavaQuestStateController_OnChangeProgress;

            startPopupController.OnStartRequested -= LavaQuestStartPopupController_OnStartRequested;

            connectPlayersPopupController.OnRequestClose -= LQConnectPlayersPopupController_OnOnRequestClose;
            connectPlayersPopupController.Deinitialize();

            lavaQuestWidget.Deinitialize();
            lavaQuestWidget.Hide();
            lavaQuestWidget = null;
            isInit = false;
        }


        void ISystemTickable.Tick()
        {
            if (!CanTickTimer())
                return;

            refreshTimeCurrent += Time.unscaledDeltaTime;

            if (refreshTimeCurrent < 1)
                return;

            RefreshWidgetTimerText();
            refreshTimeCurrent = 1;
        }


        public async UniTask ExecuteScheduledAsync(CancellationToken token)
        {
            if (!stateController.IsFeatureEnabled() || !stateController.IsUnlocked())
                return;

            bool needShowStartPopup = !startPopupController.IsShown;

            if (stateController.CurrentState == LQStateType.ReadyStart && needShowStartPopup)
            {
                await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: token);
                OpenStartPopup(token);
                await UniTask.WaitUntil(() => startPopupController.StartPopupOpened, cancellationToken: token);
                await UniTask.WaitUntil(() => startPopupController.StartPopupClosed, cancellationToken: token);
            }
        }


        private async UniTask InitializeUIControllerModulesAsync(CancellationToken token)
        {
            lavaQuestWidget = await widgetManager.GetOrAddWidgetAsync<LavaQuestWidget>(WidgetId.LavaQuest, "LavaQuestWidget", null, null, true, token) ;
            SetWidgetTextByState();
            SetWidgetNotifier();
            SetWidgetProgress();
            lavaQuestWidget.SetAction(DoWidgetActionByState);
            lavaQuestWidget.Initialize();

            stateController.OnChangeState += LavaQuestStateController_OnChangeState;
            stateController.OnChangeProgress += LavaQuestStateController_OnChangeProgress;

            connectPlayersPopupController.Initialize();
            connectPlayersPopupController.OnRequestClose += LQConnectPlayersPopupController_OnOnRequestClose;

            startPopupController.OnStartRequested += LavaQuestStartPopupController_OnStartRequested;
        }


        private void SetWidgetTextByState()
        {
            if (!stateController.IsUnlocked())
            {
                lavaQuestWidget.SetText(LocalizationService.I.Get(LocKeys.LavaQuest.WidgetLevel, stateController.UnlockLevel.ToString()));
            }
            else if (stateController.CurrentState == LQStateType.FinishedInRuntime || stateController.IsScheduledLoose())
            {
                lavaQuestWidget.SetText(LocalizationService.I.Get(LocKeys.LavaQuest.Finish));
            }
            else if (stateController.CurrentState == LQStateType.ReadyStart)
            {
                lavaQuestWidget.SetText(LocalizationService.I.Get(LocKeys.LavaQuest.Start));
            }
            else if (stateController.CurrentState is LQStateType.Started or LQStateType.LooseCooldown)
            {
                RefreshWidgetTimerText();
            }
        }


        private void DoWidgetActionByState()
        {
            if (!stateController.IsUnlocked())
            {
                lavaQuestWidget.ShowTooltip(LocalizationService.I.Get(LocKeys.LavaQuest.WidgetTooltip, stateController.UnlockLevel.ToString()));
                return;
            }

            if (stateController.AnyActionScheduled() && eventPopupController.TryOpenPopupOnScheduledAction())
                return;

            switch (stateController.CurrentState)
            {
                case LQStateType.ReadyStart or LQStateType.LooseCooldown:
                    OpenStartPopup(cts.Token);
                    break;
                case LQStateType.Started:
                    OpenEventPopupOnStartedState();
                    break;
                case LQStateType.FinishedInRuntime:
                    OpenEventPopupOnFinishInRuntimeState();
                    break;
            }
        }


        private void RefreshWidgetTimerText()
        {
            string timeStr = TimeUtils.GetTimeString(stateController.TimeRest);
            lavaQuestWidget.SetText(timeStr);
        }


        private void SetWidgetNotifier()
        {
            lavaQuestWidget.SetNotifierActive(stateController.IsUnlocked() && stateController.CurrentState is LQStateType.ReadyStart or LQStateType.FinishedInRuntime);
        }


        private void SetWidgetProgress()
        {
            float progress = stateController.IsUnlocked() ? stateController.GetProgress() : 1;
            lavaQuestWidget.SetProgress(progress);
        }


        private void OpenStartPopup(CancellationToken token)
        {
            startPopupController.OpenPopupAsync(token).Forget();
        }


        private async UniTask OpenConnectPlayerPopupAsync()
        {
            await connectPlayersPopupController.OpenConnectPlayersInfoPopupAsync();

            stateController.SetStartedState();

            startPopupController.ClosePopup();
        }


        private async UniTask OpenEventPopupOnReadyToStartStateAsync()
        {
            await eventPopupController.OpenPopupOnState(LQEventPopupState.ReadyToStart);
            connectPlayersPopupController.ClosePopup();
        }


        private void OpenEventPopupOnStartedState()
        {
            eventPopupController.OpenPopupOnState(LQEventPopupState.Started).Forget();
        }


        private void OpenEventPopupOnFinishInRuntimeState()
        {
            eventPopupController.OpenPopupOnState(LQEventPopupState.FinishedInRuntime).Forget();
        }


        private bool CanTickTimer()
        {
            return stateController.IsTimerState() && !stateController.IsScheduledLoose();
        }


        private void SetWidgetState()
        {
            bool isActive = stateController.CanShowWidget() && !stateController.IsCompleteState();
            lavaQuestWidget.SetObjectActive(isActive);

            if (isActive)
            {
                lavaQuestWidget.SetUnlockedState(stateController.IsUnlocked());
            }
        }


        private void LavaQuestStartPopupController_OnStartRequested()
        {
            OpenConnectPlayerPopupAsync().Forget();
        }


        private void LQConnectPlayersPopupController_OnOnRequestClose()
        {
            OpenEventPopupOnReadyToStartStateAsync().Forget();
        }


        private void LavaQuestStateController_OnChangeState()
        {
            SetWidgetState();
            SetWidgetTextByState();
            SetWidgetNotifier();
            SetWidgetProgress();
        }


        private void LavaQuestStateController_OnChangeProgress()
        {
            SetWidgetProgress();
        }
    }
}