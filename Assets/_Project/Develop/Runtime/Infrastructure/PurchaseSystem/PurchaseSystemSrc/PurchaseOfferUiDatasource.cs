using Features.Boosters;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using Infrastructure.Reward;

namespace Infrastructure.PurchaseSystem
{
    public class PurchaseOfferUiDatasource
    {
        public OfferId id;
        public OfferType offerType;
        public PurchaseOfferUiType purchaseOfferUiType;
        public OfferGroup offerGroup;
        public ComplexReward complexReward;
        public string priceString;
        public bool isBestPrice;
        public bool isPopular;
        public bool isEnabled;
        public bool isDecorative = false;
        public int maxLimit = int.MaxValue;
        public int curLimit = 0;

        public List<CurrencyOfferReward> CurrencyOfferRewards => complexReward.currencyOfferRewards;
        public List<BoosterReward> BoosterRewards => complexReward.boosterRewards;
        public List<BoosterReward> PreBoosterRewards => complexReward.preBoosterRewards;
        public List<InfiniteBoosterReward> InfinitePreBoosterRewards => complexReward.infinitePreBoosterRewards;
        public InfiniteLifeReward InfiniteLifeReward => complexReward.infiniteLifeReward;


        public CurrencyOfferReward CurrencyReward(CurrencyType currencyType)
        {
            var currency = complexReward.currencyOfferRewards.Find(x => x.currencyType == currencyType);
            return currency;
        }

        public int CurrencyRewardAmount(CurrencyType currencyType) 
        {
            var currency = complexReward.currencyOfferRewards.Find(x => x.currencyType == currencyType);
            if (currency == null) 
                return 0;
            return currency.amount;
        }

        public int BoosterRewardAmount(BoosterType boosterType)
        {
            var booster = complexReward.boosterRewards.Find(x => x.boosterType == boosterType);
            if (booster == null)
                return 0;
            return booster.amount;
        }

        public int PreBoosterRewardAmount(BoosterType boosterType)
        {
            var booster = complexReward.preBoosterRewards.Find(x => x.boosterType == boosterType);
            if (booster == null)
                return 0;
            return booster.amount;
        }

        public int InfinitePreBoosterRewardTime(BoosterType boosterType)
        {
            var booster = complexReward.infinitePreBoosterRewards.Find(x => x.boosterType == boosterType);
            if (booster == null)
                return 0;
            return booster.timeLengthMinutes;
        }

        public int InfiniteLifeRewardTime()
        {
            var reward = complexReward.infiniteLifeReward;
            if (reward == null)
                return 0;
            return reward.timeLengthMinutes;
        }
    }
}