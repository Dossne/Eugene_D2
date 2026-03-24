using System;
using System.Collections.Generic;
using Features.Boosters;
using Features.PurchaseUi;
using Infrastructure.BroTweens;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SpriteAtlasControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.NoAdsUi
{
    public class NoAdsPurchasePopup : PopupBase
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;

        [Header("Components")]
        [SerializeField] private Button buyButton;
        [SerializeField] private Image boosterBgImage;

        [Header("Rewards")]
        [SerializeField] private PurchaseRewardGridView rewardGridView;


        private SpriteAtlasService spriteAtlasService;
        private List<BoosterData> boosterConfig = new();

        private Action onBuyCallback;
        private Action onAnimationStartCallback;

        [Inject]
        public void Construct(SpriteAtlasService spriteAtlasService,
            ConfigProvider configProvider, 
            Action onBuyCallback)
        {
            this.spriteAtlasService = spriteAtlasService;
            this.onBuyCallback = onBuyCallback;
            boosterConfig.Clear();
            boosterConfig.AddRange(configProvider.BoosterConfig.PreBoosters);
            boosterConfig.AddRange(configProvider.BoosterConfig.InGameBoosters);
        }

        protected override void OnInitialize()
        {
            this.headerTxt.text = LocalizationService.I.Get(LocKeys.NoAds.NoAdsText);            
            buyButton.onClick.AddListener(ButtonClick);            
        }


        protected override void OnDeinitialize()
        {
            buyButton.onClick.RemoveListener(ButtonClick);
        }


        protected override void AnimationStartHandler()
        {
            base.AnimationStartHandler();
            onAnimationStartCallback?.Invoke();
        }


        public void SetAnimationStartCallback(Action onAnimationStartCallback)
        {
            this.onAnimationStartCallback = onAnimationStartCallback;
        }


        public void RefreshPopup(PurchaseOfferUiDatasource ds)
        {
            if (ds == null)
                return;

            buttonTxt.text = ds.priceString;

            rewardGridView.Construct(ds.complexReward, spriteAtlasService,boosterConfig);
        }

        private void ButtonClick()
        {
            BroTween.ClickBounceWithCallBack(buyButton, buyButton.transform, ClickInvoke)
                    .Play();
        }


        private void ClickInvoke()
        {
            onBuyCallback?.Invoke();
        }


    }
}