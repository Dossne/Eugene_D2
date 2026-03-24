using Infrastructure.Ads;
using System.Text;
using UnityEngine;


namespace Infrastructure.PurchaseSystem
{
    public class PurchaseAnalytics
    {
        #region Members

        const string SHOP_TAG           = "shop";
        const string IN_APP_OFFER_TAG   = "iap_offer";
        const string IN_APP_ATTEMPT_TAG = "iap_attempt";
        const string IN_APP_RESULT_TAG  = "iap_result";

        #endregion

        public static void TrackInAppResult(OfferId inAppOfferId, OpeningMethod openingMethod, string source, PurchaseResult purchaseResult, string mainContext)
        {
            AnalyticSender.SendTagEvent(IN_APP_RESULT_TAG, inAppOfferId.ToString(), (int)openingMethod, (int)purchaseResult, source, mainContext);
        }

        public static void TrackInAppAttempt(OfferId inAppOfferId, OpeningMethod openingMethod, string source, string mainContext)
        {
            AnalyticSender.SendTagEvent(IN_APP_ATTEMPT_TAG, inAppOfferId.ToString(), (int)openingMethod, 0, source, mainContext);
        }

        public static void TrackShopOpen(OpeningMethod openingMethod, string source, string mainContext)
        {
            AnalyticSender.SendEvent(SHOP_TAG, (int)openingMethod, source, mainContext);
        }

        public static void TrackInAppOffer(OfferId inAppOfferId, OpeningMethod openingMethod, string source, string mainContext)
        {
            AnalyticSender.SendTagEvent(IN_APP_OFFER_TAG, inAppOfferId.ToString(), (int)openingMethod, 0, source, mainContext);
        }
    }
}