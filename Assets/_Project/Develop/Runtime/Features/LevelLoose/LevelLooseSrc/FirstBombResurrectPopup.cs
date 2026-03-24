using Infrastructure.BroTweens;
using Infrastructure.Popups;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LevelLoose
{
    public class FirstBombResurrectPopup : PopupBase
    {
        [Header("Components")]
        [SerializeField] private Button continueBtn;
        [SerializeField] private Image icon;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;
        public Action callback;

        public void Setup(string header, string description, string button, Sprite iconSprite, Action callback)
        {
            headerTxt.text = header;
            descriptionTxt.text = description;
            buttonTxt.text = button;
            icon.sprite = iconSprite;
            this.callback = callback;
        }

        protected override void OnInitialize()
        {
            continueBtn.onClick.AddListener(ContinueButtonClick);
        }


        protected override void OnDeinitialize()
        {
            continueBtn.onClick.RemoveListener(ContinueButtonClick);
        }


        private void ContinueButtonClick()
        {
            BroTween.ClickBounceWithCallBack(continueBtn, continueBtn.transform, callback)
                    .Play();
        }
    }
}