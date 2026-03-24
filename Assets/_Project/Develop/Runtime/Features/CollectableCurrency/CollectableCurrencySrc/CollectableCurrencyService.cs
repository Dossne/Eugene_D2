using Features.Collectables;
using Infrastructure.Configs;
using Infrastructure.WalletSystem;
using R3;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Events;
using Features.ExtraCollectableUi;
using Features.ExtraItemSpawn;
using Features.LevelSessionStateControl;
using Infrastructure.AudioControl;
using Infrastructure.HapticControl;

namespace Features.CollectableCurrency
{
    public class CollectableCurrencyService : ILevelSessionSavable
    {
        private readonly ExtraItemsSpawner extraItemsSpawner;
        private readonly CollectItemEvent collectItemEvent;
        private readonly ExtraCollectableUIService extraCollectableUIService;

        private readonly CollectableCurrencyConfig collectableCurrencyConfig;
        private readonly CompositeDisposable disposables = new();

        private Dictionary<CollectableType, (int collected, int target)> items = new();

        private Dictionary<CollectableType, (int collected, int target)> prevSessionItems;
        private bool isRestoreSession;
        private bool isInit;


        public CollectableCurrencyService(
            ConfigProvider configProvider,
            ExtraItemsSpawner extraItemsSpawner,
            CollectItemEvent collectItemEvent,
            ExtraCollectableUIService extraCollectableUIService)
        {
            this.extraItemsSpawner = extraItemsSpawner;
            this.collectItemEvent = collectItemEvent;
            this.extraCollectableUIService = extraCollectableUIService;

            this.collectableCurrencyConfig = configProvider.CollectableCurrencyConfig;
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionItems = sessionData.collectedCurrency;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.collectedCurrency = new Dictionary<CollectableType, (int collected, int target)>(items);
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;

            items.Clear();

            if (CanRestorePrevSession())
                InitializeItemsFromPrevSession();
            else
                InitializeItemsByDefaultState();

            foreach (var entryPair in items)
            {
                await extraCollectableUIService.AddSlotAsync(entryPair.Key,entryPair.Value.collected, entryPair.Value.target, cancellationToken);
            }
            
            collectItemEvent.Subscribe(CollectItemEventHandle).AddTo(disposables);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            items.Clear();
            disposables.Dispose();

            isInit = false;
        }


        public bool IsCollectableCurrency(CollectableType collectableType)
        {
            return items.ContainsKey(collectableType);
        }


        public int GetCollectedCount(CurrencyType currencyType)
        {
            if (collectableCurrencyConfig.TryGet(currencyType, out CollectableCurrencyData collectableCurrencyData)
             && items.TryGetValue(collectableCurrencyData.collectableType, out (int collected, int target) value))
            {
                return value.collected;
            }

            return 0;
        }


        private void CollectCurrency(CollectableItem item)
        {
            var collectableType = item.CollectableType;
            if (!items.TryGetValue(collectableType, out (int collected, int target) value))
                return;

            (int collected, int target) newValue = (value.collected + 1, value.target);
            items[collectableType] = newValue;
            item.MarkAsCollected();
            
            extraCollectableUIService.FloatToUi(collectableType, newValue.collected, newValue.target);
        }


        private void InitializeItemsFromPrevSession()
        {
            items = prevSessionItems;
        }


        private void InitializeItemsByDefaultState()
        {
            List<CollectableItem> extraCollectables = extraItemsSpawner.GetItems();

            foreach (CollectableItem item in extraCollectables)
            {
                CollectableType collectableType = item.CollectableType;

                if (items.TryGetValue(collectableType, out (int collected, int target) value))
                {
                    int totalCount = value.target;
                    totalCount++;
                    items[collectableType] = (0, totalCount);
                    continue;
                }

                if (!collectableCurrencyConfig.TryGet(item.CollectableType, out CollectableCurrencyData _))
                    continue;

                items.Add(item.CollectableType, (0, 1));
            }
        }


        private void CollectItemEventHandle(CollectableItem cItem)
        {
            if (cItem.Group is not ItemGroup.Extra)
                return;

            CollectableType itemType = cItem.CollectableType;

            if (!IsCollectableCurrency(itemType))
                return;

            CollectCurrency(cItem);

            AudioService.I.PlaySfx(SfxType.PopCurrency);
            HapticService.I.HapticSelection();
        }


        private bool CanRestorePrevSession()
        {
            return isRestoreSession && prevSessionItems != null;
        }
    }
}