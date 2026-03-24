using VContainer;

namespace Infrastructure.PurchaseSystem
{
    public enum PurchaseOfferUiType
    {
        /// <summary>
        /// Indicates that the offer is not displayed in the shop.
        /// </summary>
        Empty = 0, 
        Currency = 1,
        NoAds = 2,
        Complex = 3,
        BestDeal_1 = 5,
        BestDeal_2 = 6,
        BestDeal_3 = 7,

        NoAdsHeader = 8,
        CurrencyHeader = 9,
        BestDealHeader = 10,
        BestDiscountHeader = 11,

        SuperDiscount = 12,

        StarterPack = 13,

        SpecialOfferHeader = 14,

        RewardedCurrency = 15,
    }
}