using R3;

namespace Infrastructure.WalletSystem
{
    public class Currency
    {
        private CurrencyData currencyConfig;
        private ReactiveProperty<int> amount = new ();

        public ReadOnlyReactiveProperty<int> ReactiveAmount => amount.ToReadOnlyReactiveProperty();
        public int Amount => amount.Value;
        public bool IsFull => amount.Value == currencyConfig.maxAmount;

        public Currency(CurrencyData currencyConfig, int amount = 0)
        {
            this.currencyConfig = currencyConfig;
            this.amount.Value = amount;
        }
        
        public bool CanAfford(int cost) => amount.Value >= cost;
    
        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost)) return false;
            amount.Value -= cost;
            return true;
        }
        
        public TransactionResult Add(int value)
        {
            if(value < 0)
                return TransactionResult.Failed;
            var newAmount = amount.Value + value;
            amount.Value = newAmount > currencyConfig.maxAmount ? currencyConfig.maxAmount : newAmount;
            return TransactionResult.Success;
        }

        public TransactionResult Set(int value)
        {
            if (value < 0 && !currencyConfig.canGoNegative)
            {
                return TransactionResult.Failed;
            }
            if (value > currencyConfig.maxAmount)
            {
                return TransactionResult.ExceedsMaxAmount;
            }
            amount.Value = value;
            return TransactionResult.Success;
        }
    }
}