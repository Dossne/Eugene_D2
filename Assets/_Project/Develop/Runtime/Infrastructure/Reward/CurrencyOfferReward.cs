using System;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;

namespace Infrastructure.Reward
{
    [Serializable]
    public class CurrencyOfferReward
    {
        public CurrencyType currencyType;
        public int amount;
        public string overrideIconId;
        public bool isDisplayRibbon;

        //Serialization tuning for Newtonsoft.Json

        public bool ShouldSerializeoverrideIconId()
        {
            return !overrideIconId.IsNullOrEmpty();
        }

        public bool ShouldSerializeisDisplayRibbon()
        {
            return isDisplayRibbon;
        }
    }
}