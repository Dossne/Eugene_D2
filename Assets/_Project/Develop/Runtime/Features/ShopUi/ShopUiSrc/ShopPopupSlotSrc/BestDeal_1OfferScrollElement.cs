using AYellowpaper.SerializedCollections;
using Features.Boosters;
using Features.PurchaseUi;
using Infrastructure.Localization;
using Infrastructure.PurchaseSystem;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using Infrastructure.Reward;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ShopUi
{
    public class BestDeal_1OfferScrollElement : ShopOfferScrollElement
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image coinsIcon;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private TextMeshProUGUI offerLabel;
        [SerializeField][SerializedDictionary] private SerializedDictionary<BoosterType, PurchaseRewardItemView> preBoosterViewDictionary = new();
        [SerializeField] private TextMeshProUGUI preBoosterViewText;

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

            buyButton.onClick.AddListener(HandleClick);
        }

        protected override void DeinitializeImpl()
        {
            buyButton.onClick.RemoveListener(HandleClick);
        }


        public void SetRewards(PurchaseOfferUiDatasource rewardDs)
        {
            if (rewardDs == null)
                return;
            SetCoinReward(rewardDs);
            SetPreBoosterReward(rewardDs);
        }

        private void SetPreBoosterReward(PurchaseOfferUiDatasource rewardDs)
        {
            int boosterAmount = 0;
            foreach (var item in preBoosterViewDictionary)
            {
                var booster = rewardDs.complexReward.preBoosterRewards.Find(x => x.boosterType == item.Key);

                if (booster == null)
                {
                    item.Value.gameObject.SetActive(false);
                    continue;
                }
                boosterAmount = Mathf.Max(boosterAmount, rewardDs.PreBoosterRewardAmount(booster.boosterType));
                item.Value.gameObject.SetActive(true);
                item.Value.Construct(new()
                {
                    icon = spriteAtlasService.GetFromMain(boosterConfig.Find(x => x.type == booster.boosterType).shopIconName),
                    amountText = ""
                });
            }
            if (preBoosterViewText != null)
                preBoosterViewText.text = $"<sprite name=\"multiplier_sign\">{boosterAmount}";
        }

        private void SetCoinReward(PurchaseOfferUiDatasource rewardDs)
        {
            if (rewardDs.complexReward.currencyOfferRewards.Exists(x => x.currencyType == CurrencyType.Coins))
            {
                var reward = rewardDs.CurrencyReward(CurrencyType.Coins);
                coinsIcon.sprite = reward.overrideIconId.IsNullOrEmpty() ? spriteAtlasService.GetCurrencyIcon(CurrencyType.Coins) : spriteAtlasService.GetFromMain(reward.overrideIconId);
                coinsText.text = Utils.GetSpaceSeparatedNumberString(rewardDs.CurrencyRewardAmount(CurrencyType.Coins));
            }
        }
    }
}