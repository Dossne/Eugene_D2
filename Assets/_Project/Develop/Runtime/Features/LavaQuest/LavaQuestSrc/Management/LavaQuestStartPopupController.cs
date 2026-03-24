using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Localization;
using Infrastructure.Popups;

namespace Features.LavaQuest
{
    public class LavaQuestStartPopupController
    {
        public event Action OnStartRequested;
        private readonly LavaQuestStateController stateController;
        private readonly PopupService popupService;

        private LavaQuestStartPopup startPopup;


        public LavaQuestStartPopupController(LavaQuestStateController lavaQuestStateController, PopupService popupService)
        {
            this.stateController = lavaQuestStateController;
            this.popupService = popupService;
        }


        public bool IsShown { get; private set; }

        public bool StartPopupOpened => startPopup != null && startPopup.IsOpened;
        public bool StartPopupClosed => startPopup == null || !startPopup.IsOpened;

        private bool isInit;


        public void Initialize()
        {
            if (isInit)
                return;

            stateController.OnChangeState += LavaQuestStateController_OnChangeState;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            stateController.OnChangeState -= LavaQuestStateController_OnChangeState;
            DisposePopup();

            isInit = false;
        }


        public async UniTaskVoid OpenPopupAsync(CancellationToken token)
        {
            if (startPopup == null)
            {
                startPopup = await popupService.GetAsync<LavaQuestStartPopup>(token, false, false);
                startPopup.OnStartRequested += LavaQuestStartPopup_OnStartRequested;
                startPopup.Initialize();
            }

            RefreshStartPopupState();
            startPopup.Open();
            IsShown = true;
        }


        public void ClosePopup()
        {
            if(startPopup != null)
                startPopup.Close();
        }


        private void DisposePopup()
        {
            if (startPopup == null)
                return;

            startPopup.OnStartRequested -= LavaQuestStartPopup_OnStartRequested;
            popupService.Dispose(startPopup);
            startPopup = null;
        }


        private void RefreshStartPopupState()
        {
            bool isCooldown = stateController.CurrentState == LQStateType.LooseCooldown;

            string description = isCooldown
                ? LocalizationService.I.Get(LocKeys.LavaQuest.StartPopupFail)
                : LocalizationService.I.Get(LocKeys.LavaQuest.StartPopupDescr, stateController.MaxStepIdx.ToString());

            startPopup.SetTimeRest(isCooldown ? stateController.TimeRest : 0);
            startPopup.SetDescriptionText(description);
            startPopup.SwitchState(isCooldown);
        }


        private void LavaQuestStartPopup_OnStartRequested()
        {
            if (stateController.CurrentState == LQStateType.LooseCooldown)
                return;

            OnStartRequested?.Invoke();
        }


        private void LavaQuestStateController_OnChangeState()
        {
            if (stateController.CurrentState is LQStateType.ReadyStart)
                IsShown = false;

            if (startPopup != null && startPopup.IsOpened)
                RefreshStartPopupState();
        }
    }
}