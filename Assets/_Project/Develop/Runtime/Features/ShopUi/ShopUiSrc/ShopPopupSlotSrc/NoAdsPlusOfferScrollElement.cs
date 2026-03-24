using Features.Boosters;
using Features.PurchaseUi;
using Infrastructure.Localization;
using Infrastructure.PurchaseSystem;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ShopUi
{
    public class NoAdsPlusOfferScrollElement : ShopOfferScrollElement
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image noAdsIcon;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private TextMeshProUGUI offerLabel;
        [SerializeField] private PurchaseRewardGridView rewardGridView;
        private List<BoosterData> boosterConfig = new();

        protected override void InitializeImpl()
        {
            boosterConfig.Clear();
            boosterConfig.AddRange(configProvider.BoosterConfig.PreBoosters);
            boosterConfig.AddRange(configProvider.BoosterConfig.InGameBoosters);

            costLabel.text = datasource.priceString;
            noAdsIcon.sprite = spriteAtlasService.GetFromMain("No_Ads_Button");
            offerLabel.text = LocalizationService.I.Get(LocKeys.NoAds.NoAdsPlusText);

            RefreshReward(datasource);

            buyButton.onClick.AddListener(HandleClick);
        }

        protected override void DeinitializeImpl()
        {
            buyButton.onClick.RemoveListener(HandleClick);
        }

        private void RefreshReward(PurchaseOfferUiDatasource ds)
        {
            if (ds == null)
                return;
            rewardGridView.Construct(ds.complexReward, spriteAtlasService, boosterConfig);
        }
    }
}