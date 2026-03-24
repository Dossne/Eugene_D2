namespace Infrastructure.WalletSystem
{
    public class Transaction
    {
        public TransactionType action;
        public TransactionResult result;
        public CurrencyType currency;
        public int amount; //transaction amount
        public int total; //total amount in wallet
        public string reason;
        public bool updateHud;
    }
}