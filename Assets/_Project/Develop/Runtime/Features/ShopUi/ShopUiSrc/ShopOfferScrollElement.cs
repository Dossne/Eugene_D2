using Features.ScrollList;
using Infrastructure.Localization;
using Infrastructure.PurchaseSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ShopUi
{
    public abstract class ShopOfferScrollElement : ScrollElement<PurchaseOfferUiDatasource>
    {
        [SerializeField] private Image specialMark;
        [SerializeField] private TextMeshProUGUI specialText;

        protected override void InitializeImpl()
        {
            SetSpecialOfferMark(datasource);
        }

        private void SetSpecialOfferMark(PurchaseOfferUiDatasource rewardDs)
        {
            if (specialMark != null)
                specialMark.gameObject.SetActive(rewardDs.isPopular || rewardDs.isBestPrice);

            if (specialText != null)
            {
                specialText.gameObject.SetActive(rewardDs.isPopular || rewardDs.isBestPrice);
                if (rewardDs.isBestPrice)
                    specialText.text = LocalizationService.I.Get(LocKeys.Purchase.BestPrice);
                else if (rewardDs.isPopular)
                    specialText.text = LocalizationService.I.Get(LocKeys.Purchase.PopularOffer);
                else
                    specialText.text = string.Empty;
            }
        }
    }
}