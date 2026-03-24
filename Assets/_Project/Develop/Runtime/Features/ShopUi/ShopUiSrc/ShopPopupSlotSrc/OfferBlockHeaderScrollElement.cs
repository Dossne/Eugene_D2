using Infrastructure.Localization;
using Infrastructure.PurchaseSystem;
using Infrastructure.WalletSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ShopUi
{
    public class OfferBlockHeaderScrollElement : ShopOfferScrollElement
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI offerLabel;

        protected override void InitializeImpl()
        {
            icon.sprite = datasource.id == OfferId.rh_no_ads_header
                        ? spriteAtlasService.GetFromMain("No_Ads_Button")
                        : spriteAtlasService.GetCurrencyIcon(CurrencyType.Coins);
            offerLabel.text = LocalizationService.I.Get(datasource.id.ToString());
        }

        protected override void DeinitializeImpl()
        {
        }
    }
}