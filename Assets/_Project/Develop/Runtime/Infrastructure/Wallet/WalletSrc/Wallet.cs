using System;
using System.Collections.Generic;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using R3;

namespace Infrastructure.WalletSystem
{
    public class Wallet : ISavable, IDisposable
    {
        private readonly Dictionary<CurrencyType, Currency> currencies = new();
        public Subject<Transaction> OnChange = new Subject<Transaction>();


        public Wallet(ConfigProvider configProvider)
        {
            foreach (var currencySo in configProvider.CurrencyConfig.Items)
            {
                currencies.Add(currencySo.currencyType, new Currency(currencySo));
            }
        }


        public void Load(Progress progress)
        {
            if (progress.gameState.currencies == null)
            {
                Save(progress);
            }
            else
            {
                foreach (var currencyAmountSave in progress.gameState.currencies)
                {
                    if (currencyAmountSave.currencyType == CurrencyType.None)
                        continue;

                    currencies[currencyAmountSave.currencyType].Set(currencyAmountSave.amount);
                }
            }
        }


        public void Save(Progress progress)
        {
            var currenciesSaves = new List<CurrencyAmountSave>();
            foreach (var currencyKey in currencies.Keys)
            {
                currenciesSaves.Add(new CurrencyAmountSave
                {
                    currencyType = currencyKey,
                    amount = currencies[currencyKey].ReactiveAmount.CurrentValue
                });
            }

            progress.gameState.currencies = currenciesSaves;
        }


        public void Dispose()
        {
            foreach (var currency in currencies.Values)
            {
                currency.ReactiveAmount?.Dispose();
            }

            currencies.Clear();
        }


        public int GetCount(CurrencyType currencyType) =>
            currencies.TryGetValue(currencyType, out var currency) ? currency.ReactiveAmount.CurrentValue : 0;


        public bool IsEnough(CurrencyType currencyType, int amount) => GetCount(currencyType) >= amount;


        public TransactionResult Increase(CurrencyType currencyType, int amount, string reason, bool updateHud = true)
        {
            return ExecuteTransaction(currencyType, amount, TransactionType.Add, reason, updateHud);
        }


        public TransactionResult Decrease(CurrencyType currencyType, int amount, string reason, bool updateHud = true)
        {
            return ExecuteTransaction(currencyType, amount, TransactionType.Subtract, reason, updateHud);
        }


        public TransactionResult Set(CurrencyType currencyType, int amount, string reason, bool updateHud = true)
        {
            return ExecuteTransaction(currencyType, amount, TransactionType.Set, reason, updateHud);
        }


        public ReadOnlyReactiveProperty<int> GetCurrencyObservable(CurrencyType currencySo)
        {
            return currencies.TryGetValue(currencySo, out var currency) ? currency.ReactiveAmount : null;
        }

        public bool IsCurrencyFull(CurrencyType currencyType) 
        {
            if (currencies.TryGetValue(currencyType, out var currency)) 
                return currency.IsFull;
            return false;
        }
        
        private TransactionResult ExecuteTransaction(CurrencyType currencyType, int amount, TransactionType transaction, string reason, bool updateHud)
        {
            if (!IsCurrencyExists(currencyType))
            {
                return TransactionResult.InvalidCurrency;
            }

            var currency = currencies[currencyType];

            TransactionResult result = TransactionResult.Success;

            switch (transaction)
            {
                case TransactionType.Add:
                    result = currency.Add(amount);
                    if (result == TransactionResult.Success)
                    {
                        AnalyticSender.SendSoftIncome(amount, currency.ReactiveAmount.CurrentValue, reason, "");
                    }

                    break;

                case TransactionType.Subtract:
                    if (!currency.TrySpend(amount))
                    {
                        result = TransactionResult.InsufficientFunds;
                    }

                    if (result == TransactionResult.Success)
                    {
                        AnalyticSender.SendSoftOutcome(amount, currency.ReactiveAmount.CurrentValue, reason, "");
                    }

                    break;

                case TransactionType.Set:
                    result = currency.Set(amount);
                    break;
            }

            OnChange.OnNext(new Transaction
            {
                action = transaction,
                result = result,
                currency = currencyType,
                amount = amount,
                total = currency.Amount,
                reason = reason,
                updateHud = updateHud
            });

            return result;
        }


        private bool IsCurrencyExists(CurrencyType currencyType)
        {
            return currencies.TryGetValue(currencyType, out _);
        }

    }
}