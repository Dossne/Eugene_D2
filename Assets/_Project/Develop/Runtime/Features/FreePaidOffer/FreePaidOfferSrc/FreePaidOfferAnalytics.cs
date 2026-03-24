using Infrastructure.Ads;
using Infrastructure.PurchaseSystem;
using Infrastructure.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Features.FreePaidOffer
{
    public static class FreePaidOfferAnalytics
    {
        private const string Available = "special_offer_available";
        private const string Received = "special_offer_reward_received";
        
        private static StringBuilder sb = new StringBuilder();


        public static void SendEventOfferAvailable(int offerCounter, int preseId, FreePaidOfferSkinType skinType, string mainContext)
        {
            AnalyticSender.SendEvent(Available, offerCounter, preseId, GetOfferAvailableJson(skinType), mainContext); 
        }


        public static void SendEventRewardReceived(int offerCounter, int preseId, int rewardIdx, FreePaidOfferSkinType skinType, OfferId inAppOfferId, bool isLastFreeReward, bool isLastIapReward, string mainContext)
        {
            AnalyticSender.SendEvent(Received, offerCounter, preseId, rewardIdx + 1, GetRewardReceivedJson(skinType, inAppOfferId, isLastFreeReward, isLastIapReward), mainContext);
        }


        private static string GetOfferAvailableJson(FreePaidOfferSkinType skinType)
        {
            sb.Clear();
            sb.Append('{');
            sb.Append(JsonUtils.CreateValue("skin_id", (int)skinType));
            sb.AppendLast('}');
            return sb.ToString();
        }


        private static string GetRewardReceivedJson(FreePaidOfferSkinType skinType, OfferId inAppOfferId, bool isLastFreeReward, bool isLastIapReward)
        {
            bool isFree = inAppOfferId == OfferId.none;

            sb.Clear();
            sb.Append('{');
            sb.Append(JsonUtils.CreateValue("skin_id", (int)skinType));
            sb.Append(JsonUtils.CreateValue("reward_type", isFree ? "free" : "iap"));
            sb.Append(JsonUtils.CreateValue("is_last_free_reward", isLastFreeReward ? 1 : 0));
            sb.Append(JsonUtils.CreateValue("is_last_iap_reward", isLastIapReward ? 1 : 0));
            sb.AppendLast('}');
            return sb.ToString();
        }
    }
}


