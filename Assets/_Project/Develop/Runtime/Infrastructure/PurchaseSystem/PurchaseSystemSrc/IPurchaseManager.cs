using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Infrastructure.PurchaseSystem
{
    public interface IPurchaseManager
    {
        event Action<(bool isLoading, string message, float maxTime)> OnPurchaseLoadingEvent;
        event Action<OfferId> OnPurchaseCompleted;
        event Action OnPurchaseFailed;
        List<InAppData> ActiveOfferDataList { get; }

        void InitiatePurchase(OfferId        id, OpeningMethod yourself, string lastShopOpenSource);
        string GetLocalizedPrice(OfferId     purchaseOfferId);
        UniTask InitializeAsync(CancellationToken cancellationToken);
        void Deinitialize();
        void RestorePurchases();

#if PR_CHEAT || UNITY_EDITOR
        bool CheatPurchaseEnabled { get; }
        void SetCheatPurchaseEnabled(bool isEnabled);
#endif
    }
}