using Features.Boosters;
using Features.Life;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.Reward;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Infrastructure.PurchaseSystem
{
    public class PurchaseOfferBase
    {
        private InAppData inAppData;
        private readonly RewardApplyController rewardApplyController;
        private LifeController lifeController;        
        private BoostersManager boostersManager;
        private Wallet wallet;
        private ComplexReward complexReward;
        private int boughtThisCycleCount = 0;

        public ComplexReward ComplexReward => complexReward;        
        public PurchaseOfferUiType OfferUiType => inAppData.purchaseOfferUiType;
        public OfferGroup OfferGroup => inAppData.group; 
        public int DefaultOrder => inAppData.defaultOrder;
        public float Price => inAppData.price;
        public OfferId Id => inAppData.id;
        public OfferType Type => inAppData.type;
        public bool IsBestPrice => inAppData.isBestPrice;
        public bool IsPopular => inAppData.isPopular;
        public bool IsDecorative => inAppData.isDecorative;
        public bool IsEnabled => inAppData.isEnabled;
        public bool IsLimitedPerCycle => inAppData.limitedPurchasePerCycle;
        public bool LimitReached => IsLimitedPerCycle && boughtThisCycleCount >= inAppData.purchasePerCycleLimit;
        public int BoughtThisCycleCount => boughtThisCycleCount;
        public int MaxLimitPerCycle => inAppData.purchasePerCycleLimit;


        [Inject]
        public PurchaseOfferBase(InAppData inAppData, RewardApplyController rewardApplyController, ConfigProvider configProvider) 
        {
            this.inAppData = inAppData;
            this.rewardApplyController = rewardApplyController;

            string rewardJson = inAppData.rewardJson;
            if (inAppData.group == OfferGroup.FreePaid)
            {
                rewardJson = configProvider.FreePaidOfferConfig.GetRewardJson(inAppData.id);
            }

            complexReward = RewardUtils.ConvertFromJson<ComplexReward>(rewardJson);
        }

        public void IncreaseBoughtThisCycle(int value = 1) 
        {
            boughtThisCycleCount += value;
        }

        public void ResetBoughtThisCycle()
        {
            boughtThisCycleCount = 0;
        }

        public bool IsExcluded(int filterLevel, float cumulativeSpendedUsd, PurchaseOfferTimeCycle purchaseOfferTimeCycle, List<PurchasedInAppData> purchasedInAppDataList)
        {
            //Exclude locked
            if (inAppData.unlockLevel > filterLevel)
                return true;

            //Exclude bought one time
            if (inAppData.oneTimeOffer && purchasedInAppDataList.Exists(x => x.inAppOfferId == inAppData.id))
                return true;

            //Exclude locked by other offer
            if (inAppData.unlockOfferId != OfferId.none && !purchasedInAppDataList.Exists(x => x.inAppOfferId == inAppData.unlockOfferId))
                return true;

            //Exclude by cumulative spent
            if (!(inAppData.cumulativeSpentMin <= cumulativeSpendedUsd && cumulativeSpendedUsd < inAppData.cumulativeSpentMax))
                return true;

            //Exclude by time cycles
            if (purchaseOfferTimeCycle != null && !purchaseOfferTimeCycle.IsOfferActive())
                return true;

            if (purchaseOfferTimeCycle != null 
             && LimitReached
             && inAppData.hideOnPurchaseLimitReached)
                return true;

            //Exclude bought no ads
            if (Advertisement.IsPremium() &&
                inAppData.id is OfferId.rh_no_ads_header
                             or OfferId.rh_no_ads
                             or OfferId.rh_no_ads_v2
                             or OfferId.rh_no_ads_plus
                             or OfferId.rh_no_ads_plus_v2)
                return true;

            return false;
        }

        public void CompletePurchase(bool isFirst)
        {
            string reasonPostfix = string.Empty;
            float rewardMultiplier = 1;
            if (isFirst && inAppData.firstRewardMultiplierPercent > 100)
            {
                rewardMultiplier = inAppData.firstRewardMultiplierPercent * 0.01f;
                reasonPostfix = "first_purchased_bonus";
            }
            
            rewardApplyController.ApplyComplexReward(complexReward, inAppData.id + reasonPostfix, true, rewardMultiplier);
        }
    }
}