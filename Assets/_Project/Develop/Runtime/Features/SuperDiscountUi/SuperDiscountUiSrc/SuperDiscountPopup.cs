using Features.Boosters;
using Features.PurchaseUi;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;
using System;
using System.Collections.Generic;
using Infrastructure.BroTweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SuperDiscountUi
{
    public class SuperDiscountPopup : PopupBase
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buyButtonText;
        [SerializeField] private Image coinsIcon;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private PurchaseRewardGridView rewardGridView;

        [Header("Skin images")]
        [SerializeField] private Image mainSkinImage;
        [SerializeField] private Image headerSkinImage;

        private SpriteAtlasService spriteAtlasService;
        private List<BoosterData> boosterConfig = new();

        private Action onBuyCallback;

        public void Construct(SpriteAtlasService spriteAtlasService, ConfigProvider configProvider, Action onBuyCallback)
        {
            this.spriteAtlasService = spriteAtlasService;
            this.onBuyCallback = onBuyCallback;
            boosterConfig.Clear();
            boosterConfig.AddRange(configProvider.BoosterConfig.PreBoosters);
            boosterConfig.AddRange(configProvider.BoosterConfig.InGameBoosters);
        }

        public void RefreshPopup(PurchaseOfferUiDatasource ds, Sprite headerSkinSprite, Sprite mainSkinSprite)
        {
            if (ds == null)
                return;

            buyButtonText.text = ds.priceString;
            mainSkinImage.sprite = mainSkinSprite;
            headerSkinImage.sprite = headerSkinSprite;
            this.headerTxt.text = LocalizationService.I.Get(ds.id.ToString());

            SetCoinReward(ds);
            rewardGridView.Construct(ds.complexReward, spriteAtlasService, boosterConfig);
        }

        public void SetRestTime(string restTimeText)
        {
            timerText.text = restTimeText;
        }

        protected override void OnInitialize()
        {
            buyButton.onClick.AddListener(ButtonClick);
        }


        protected override void OnDeinitialize()
        {
            buyButton.onClick.RemoveListener(ButtonClick);
        }

        private void SetCoinReward(PurchaseOfferUiDatasource rewardDs)
        {
            if (rewardDs.complexReward.currencyOfferRewards.Exists(x => x.currencyType == CurrencyType.Coins))
            {
                var reward = rewardDs.CurrencyReward(CurrencyType.Coins);
                coinsIcon.sprite = reward.overrideIconId.IsNullOrEmpty() ? spriteAtlasService.GetCurrencyIcon(CurrencyType.Coins) : spriteAtlasService.GetFromMain(reward.overrideIconId);
                coinsText.text = Utils.GetSpaceSeparatedNumberString(reward.amount);
            }
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