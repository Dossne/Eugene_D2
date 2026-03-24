namespace Infrastructure.WalletSystem
{
    public enum TransactionResult
    {
        Success = 0,
        InsufficientFunds = 1, //недостаточно средств
        InvalidCurrency = 2,
        ExceedsMaxAmount = 3,
        Failed = 4
    }
}