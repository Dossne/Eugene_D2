using System;
using Infrastructure.Localization;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;

namespace Features.Competition
{
    public class CompetitionBeginPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;

        private Action onCloseCallback;
        public override bool IsDisposeOnClosed => true;

        public void Construct(Action onCloseCallback)
        {
            this.onCloseCallback = onCloseCallback;
        }

        protected override void OnInitialize()
        {
            this.headerTxt.text = LocalizationService.I.Get(LocKeys.Competition.BeginPopupHeader);
            this.descriptionTxt.text = LocalizationService.I.Get(LocKeys.Competition.BeginPopupDescr);
            this.buttonTxt.text = LocalizationService.I.Get(LocKeys.Competition.BeginPopupButton);
        }

        protected override void OnBeginClose()
        {
            onCloseCallback?.Invoke();
        }
    }
}