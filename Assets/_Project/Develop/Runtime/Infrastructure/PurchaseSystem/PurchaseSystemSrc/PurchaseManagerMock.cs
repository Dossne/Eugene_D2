using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Configs;

#pragma warning disable CS0067 // the event is never used

namespace Infrastructure.PurchaseSystem
{
    public class PurchaseManagerMock : IPurchaseManager
    {
        public event Action<(bool isLoading, string message, float maxTime)> OnPurchaseLoadingEvent;
        public event Action<OfferId> OnPurchaseCompleted;
        public event Action OnPurchaseFailed;

        private readonly ConfigProvider configProvider;

        public PurchaseManagerMock(ConfigProvider configProvider)
        {
            this.configProvider = configProvider;
        }

        public List<InAppData> ActiveOfferDataList { get; private set; }

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            ActiveOfferDataList = configProvider.InAppConfig.Items.Where(x => x.isEnabled).ToList();
            return UniTask.CompletedTask;
        }

        public void Deinitialize()
        {
            ActiveOfferDataList.Clear();
        }

        public void InitiatePurchase(OfferId id, OpeningMethod yourself, string lastShopOpenSource)
        {
            OnPurchaseCompleted?.Invoke(id);
        }

        public string GetLocalizedPrice(OfferId purchaseOfferId) => "0.00";

        public void RestorePurchases() { }

#if PR_CHEAT || UNITY_EDITOR

        public bool CheatPurchaseEnabled => true;
        public void SetCheatPurchaseEnabled(bool isEnabled) { }

#endif
    }
}