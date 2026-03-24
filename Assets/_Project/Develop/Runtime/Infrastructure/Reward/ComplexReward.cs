using System;
using System.Collections.Generic;

namespace Infrastructure.Reward
{
    [Serializable]
    public class ComplexReward
    {
        public bool noAdsIncluded = false;
        public List<CurrencyOfferReward> currencyOfferRewards = new();
        public List<BoosterReward> boosterRewards = new();
        public List<BoosterReward> preBoosterRewards = new();
        public List<InfiniteBoosterReward> infinitePreBoosterRewards = new();
        public InfiniteLifeReward infiniteLifeReward = new();
        public string containerId;

        public ComplexReward Clone()
        {
            return (ComplexReward)this.MemberwiseClone();
        }

        //Serialization tuning for Newtonsoft.Json
        public bool ShouldSerializenoAdsIncluded()
        {
            return noAdsIncluded;
        }

        public bool ShouldSerializecurrencyOfferRewards()
        {
            return (currencyOfferRewards != null && currencyOfferRewards.Count > 0);
        }

        public bool ShouldSerializeboosterRewards()
        {
            return (boosterRewards != null && boosterRewards.Count > 0);
        }

        public bool ShouldSerializepreBoosterRewards()
        {
            return (preBoosterRewards != null && preBoosterRewards.Count > 0);
        }

        public bool ShouldSerializeinfinitePreBoosterRewards()
        {
            return (infinitePreBoosterRewards != null && infinitePreBoosterRewards.Count > 0);
        }

        public bool ShouldSerializeinfiniteLifeReward()
        {
            return (infiniteLifeReward != null && infiniteLifeReward.timeLengthMinutes > 0);
        }

        public bool ShouldSerializecontainerId()
        {
            return !string.IsNullOrEmpty(containerId);
        }

        public bool HaveCurrencyRewards()
        {
            return ShouldSerializecurrencyOfferRewards();
        }

        public bool HaveBoosterRewards()
        {
            return ShouldSerializeboosterRewards();
        }

        public bool HavePreBoosterRewards()
        {
            return ShouldSerializepreBoosterRewards();
        }

        public bool HaveInfinitePreBoosterRewards()
        {
            return ShouldSerializeinfinitePreBoosterRewards();
        }

        public bool HaveInfiniteLifeReward()
        {
            return ShouldSerializeinfiniteLifeReward();
        }
    }
}