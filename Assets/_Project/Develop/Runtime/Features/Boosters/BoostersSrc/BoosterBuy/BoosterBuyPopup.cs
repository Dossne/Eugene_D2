using System;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.Boosters
{
    public class BoosterBuyPopup : PopupBase
    {
        public event Action<BoosterType> OnBuyClick;

        [Header("Components")]
        [SerializeField] private Button buyBtn;
        [SerializeField] private Image boosterIcon;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI boosterNameTxt;
        [SerializeField] private TextMeshProUGUI boosterCountTxt;
        [SerializeField] private TextMeshProUGUI boosterDescrTxt;
        [SerializeField] private TextMeshProUGUI buyBtnTxt;
        [SerializeField] private TextMeshProUGUI priceTxt;

        private SpriteAtlasService spriteAtlasService;

        private BoosterType boosterType;
        private string iconName;
        private string boosterName;
        private string description;
        private int buyCount;
        private int buyPrice;


        [Inject]
        public void Construct(SpriteAtlasService spriteAtlasService)
        {
            this.spriteAtlasService = spriteAtlasService;
        }


        public void Construct(BoosterType boosterType, string iconName, string boosterName, string description, int buyCount, int buyPrice)
        {
            this.boosterType = boosterType;
            this.iconName = iconName;
            this.boosterName = boosterName;
            this.description = description;
            this.buyCount = buyCount;
            this.buyPrice = buyPrice;
        }


        protected override void OnInitialize()
        {
            buyBtn.onClick.AddListener(StartButtonClick);
        }


        protected override void OnDeinitialize()
        {
            buyBtn.onClick.RemoveListener(StartButtonClick);

            OnBuyClick = null;
        }


        protected override void OnBeginOpen()
        {
            boosterIcon.sprite = spriteAtlasService.GetFromMain(iconName);
            boosterNameTxt.text = boosterName;
            boosterCountTxt.text = $"x{buyCount.ToString()}";
            boosterDescrTxt.text = description;
            buyBtnTxt.text = LocalizationService.I.Get(LocKeys.Boosters.BoosterBuy);
            priceTxt.text = buyPrice.ToString();
        }


        private void StartButtonClick()
        {
            BroTween.ClickBounceWithCallBack(buyBtn, buyBtn.transform, this, target => target.StartClickInvoke())
                    .Play();
        }


        private void StartClickInvoke()
        {
            OnBuyClick?.Invoke(boosterType);
        }
    }
}