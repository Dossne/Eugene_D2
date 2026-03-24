using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ShopUi
{
    public class OneCurrencyOfferScrollElement : ShopOfferScrollElement
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private TextMeshProUGUI offerLabel;

        protected override void InitializeImpl()
        {
            costLabel.text = datasource.priceString;

            var oneCurrencyReward = datasource.complexReward.currencyOfferRewards[0];

            currencyIcon.sprite = oneCurrencyReward.overrideIconId.IsNullOrEmpty() ? spriteAtlasService.GetCurrencyIcon(oneCurrencyReward.currencyType) 
                                                                                   : spriteAtlasService.GetFromMain(oneCurrencyReward.overrideIconId); ;
            offerLabel.text = Utils.GetSpaceSeparatedNumberString(oneCurrencyReward.amount);

            buyButton.onClick.AddListener(HandleClick);
        }

        protected override void DeinitializeImpl()
        {
            buyButton.onClick.RemoveListener(HandleClick);
        }
    }
}