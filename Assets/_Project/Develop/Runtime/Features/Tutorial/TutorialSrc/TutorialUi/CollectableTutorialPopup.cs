using Infrastructure.BroTweens;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Tutorial
{
    public class CollectableTutorialPopup : PopupBase
    {
        [Header("Components")]
        [SerializeField] private Button continueBtn;
        [SerializeField] private Image icon;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;
        

        public void Setup(string header, string description, string button, Sprite iconSprite)
        {
            headerTxt.text = header;
            descriptionTxt.text = description;
            buttonTxt.text = button;
            icon.sprite = iconSprite;
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
            BroTween.ClickBounceWithCallBack(continueBtn, continueBtn.transform, Close)
                    .Play();
        }
    }
}