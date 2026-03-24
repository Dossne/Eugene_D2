using Features.Boosters;
using Features.PurchaseUi;
using Features.ShopUi;
using Features.Skin;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.PurchaseSystem;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SuperDiscountUi
{
    public class SuperDiscountInPopupOffer : ShopOfferScrollElement
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image coinsIcon;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private TextMeshProUGUI offerLabel;
        [SerializeField] private TextMeshProUGUI timeRestText;
        [SerializeField] private Image mainBackground;
        [SerializeField] private Image headerBackground;
        [SerializeField] private Image themeBackground;
        [SerializeField] private PurchaseRewardGridView rewardGridView;

        private PurchaseOfferContainer purchaseOfferContainer;

        private List<BoosterData> boosterConfig = new();               
        
        protected override void InitializeImpl()
        {
            base.InitializeImpl();

            boosterConfig.Clear();
            boosterConfig.AddRange(configProvider.BoosterConfig.PreBoosters);
            boosterConfig.AddRange(configProvider.BoosterConfig.InGameBoosters);

            costLabel.text = datasource.priceString;
            //noAdsIcon.sprite = spriteAtlasService.GetFromMain("No_Ads_Button");
            offerLabel.text = LocalizationService.I.Get(datasource.id.ToString());

            SetRewards(datasource);
            purchaseOfferContainer.OnOfferSecondTick += PurchaseOfferContainer_OnOfferSecondTick;

            buyButton.onClick.AddListener(HandleClick);
        }

        protected override void DeinitializeImpl()
        {
            purchaseOfferContainer.OnOfferSecondTick -= PurchaseOfferContainer_OnOfferSecondTick;

            buyButton.onClick.RemoveListener(HandleClick);
        }

        private void SetRewards(PurchaseOfferUiDatasource rewardDs)
        {
            if (rewardDs == null)
                return;
            SetCoinReward(rewardDs);
            rewardGridView.Construct(rewardDs.complexReward, spriteAtlasService, boosterConfig);
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

        private void PurchaseOfferContainer_OnOfferSecondTick((OfferId offerId, double timeRestSec) offerSecondTick)
        {
            if (offerSecondTick.offerId != datasource.id)
                return;

            if (!gameObject.activeSelf)
                return;

            var time = offerSecondTick.timeRestSec < 86400 ? TimeUtils.GetTimeString(offerSecondTick.timeRestSec) : TimeUtils.GetTimeString(offerSecondTick.timeRestSec, 100f);
            timeRestText.text = time;
            if (gameObject.activeSelf && offerSecondTick.timeRestSec == 0)
                gameObject.SetActive(false);
        }

        public void SetSuperDiscountSkin(SuperDiscountSkinType superDiscountSkinType)
        {
            var skinData = configProvider.SkinConfiguration.GetSuperDiscountSkin(superDiscountSkinType);
            mainBackground.sprite = spriteAtlasService.GetFromMain(skinData.shopOfferBackgroundSpriteId);
            headerBackground.sprite = spriteAtlasService.GetFromMain(skinData.shopOfferHeaderSpriteId);
            themeBackground.sprite = spriteAtlasService.GetFromMain(skinData.shopOfferThemeSpriteId);

        }

        public void SetPurchaseOfferContainer(PurchaseOfferContainer purchaseOfferContainer)
        {
            this.purchaseOfferContainer = purchaseOfferContainer;
        }
    }
}


