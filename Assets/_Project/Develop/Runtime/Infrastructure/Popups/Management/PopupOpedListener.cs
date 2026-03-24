using Features.LevelComplete;
using Features.LevelLoose;
using Features.LifeUi;
using Infrastructure.CurrencyHud;
using Infrastructure.InputControl;
using Infrastructure.PauseControl;

namespace Infrastructure.Popups
{
    public class PopupOpedListener
    {
        private PopupService popupService;
        private InputService inputService;
        private PauseService pauseService;
        private readonly CurrencyHudService currencyHudService;
        private bool isInit;


        public PopupOpedListener(PopupService popupService, InputService inputService, PauseService pauseService, CurrencyHudService currencyHudService)
        {
            this.popupService = popupService;
            this.inputService = inputService;
            this.pauseService = pauseService;
            this.currencyHudService = currencyHudService;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            popupService.OnChangeState += PopupService_OnChangeState;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            popupService.OnChangeState -= PopupService_OnChangeState;

            isInit = false;
        }


        private void OnBeginOpen(PopupBase popup)
        {

            if (pauseService.Pause(popup))
            {
                inputService.DisableInput();
            }

            if (popup is LevelCompletePopup)
                currencyHudService.SetMode(CurrencyHudMode.HideAll, false);
        }


        private void OnBeginClose(PopupBase popup)
        {
            if (pauseService.Resume(popup))
            {
                inputService.EnableInput();
            }

            if (popup is LevelCompletePopup)
                currencyHudService.ResetMode();
        }


        private void PopupService_OnChangeState(PopupBase popup, PopupState state)
        {
            switch (state)
            {
                case PopupState.BeginOpen:
                    OnBeginOpen(popup);
                    break;
                case PopupState.BeginClose:
                    OnBeginClose(popup);
                    break;
            }
        }
    }
}