using System;
using Infrastructure.CurrencyHud;
using Infrastructure.Localization;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.LevelLoose
{
    public class LevelLoosePopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI header;
        [SerializeField] private Button repeatButton;
        [SerializeField] private TextMeshProUGUI repeatBtnTxt;

        private Action repeatCallback;
        private Action closeCallback;
        
        public void Construct(Action repeatCallback, Action closeCallback)
        {
            this.repeatCallback = repeatCallback;
            this.closeCallback = closeCallback;

        }


        protected override void OnInitialize()
        {
            this.header.text = LocalizationService.I.Get(LocKeys.LoosePopup.Header);
            this.repeatBtnTxt.text = LocalizationService.I.Get(LocKeys.LoosePopup.Btn);
            repeatButton.onClick.AddListener(ClickRepeat);
            foreach (var btn in closeButton)
            {
                btn.onClick.AddListener(CloseClicked);
            }
        }


        protected override void OnDeinitialize()
        {
            foreach (var btn in closeButton)
            {
                btn.onClick.RemoveListener(CloseClicked);
            }

            repeatButton.onClick.RemoveListener(ClickRepeat);
        }


        private void ClickRepeat()
        {
            repeatCallback?.Invoke();
        }


        private void CloseClicked()
        {
            closeCallback?.Invoke();
        }
    }
}