using Infrastructure.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ShopUi
{
    public class NoAdsOfferScrollElement : ShopOfferScrollElement
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image noAdsIcon;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private TextMeshProUGUI offerLabel;

        protected override void InitializeImpl()
        {
            costLabel.text = datasource.priceString;
            noAdsIcon.sprite = spriteAtlasService.GetFromMain("No_Ads_Button");
            offerLabel.text = LocalizationService.I.Get(LocKeys.NoAds.NoAdsText);

            buyButton.onClick.AddListener(HandleClick);
        }

        protected override void DeinitializeImpl()
        {
            buyButton.onClick.RemoveListener(HandleClick);
        }
    }
}