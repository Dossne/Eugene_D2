using System;

namespace Infrastructure.WalletSystem
{
    [Serializable]
    public class CurrencyAmountSave
    {
        public CurrencyType currencyType;
        public int amount;
    }
}