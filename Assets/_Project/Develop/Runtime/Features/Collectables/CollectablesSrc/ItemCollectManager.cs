using System.Collections.Generic;
using Features.Events;
using Features.LevelConfiguration;
using Infrastructure.Collections;
using R3;
using UnityEngine;

namespace Features.Collectables
{
    public class ItemCollectManager
    {
        private readonly Dictionary<CollectableType, IntHashMap<CollectableItem>> groupedItems = new();

        
        private readonly LevelCreateManager levelCreateManager;
        private readonly CollectItemEvent collectItemEvent;
        private CompositeDisposable disposables;
        private bool isInit;

        public ItemCollectManager(LevelCreateManager levelCreateManager, CollectItemEvent collectItemEvent)
        {
            this.levelCreateManager = levelCreateManager;
            this.collectItemEvent = collectItemEvent;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            GroupItems();
            
            disposables = new();
            collectItemEvent.Subscribe(CollectAndObserve).AddTo(disposables);
            
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            isInit = false;
            disposables.Dispose();
        }


        private void GroupItems()
        {
            IReadOnlyList<CollectableItem> allItems = levelCreateManager.GetItems();

            for (int i = 0; i < allItems.Count; i++)
            {
                CollectableItem item = allItems[i];

                if (!groupedItems.ContainsKey(item.CollectableType))
                    groupedItems.Add(item.CollectableType, new IntHashMap<CollectableItem>());

                groupedItems[item.CollectableType].Add(item.OrderNumber, item, out _);
            }
        }


        public Dictionary<CollectableType, int> GetGroupedCount()
        {
            Dictionary<CollectableType, int> result = new Dictionary<CollectableType, int>();

            foreach (var pair in groupedItems)
            {
                result.Add(pair.Key, pair.Value.length);
            }

            return result;
        }


        public Dictionary<CollectableType, IntHashMap<CollectableItem>> GetItems()
        {
            return groupedItems;
        }
        

        private void CollectAndObserve(CollectableItem item)
        {
            if (item.Group is not ItemGroup.Collectable)
            {
                return;
            }

            if (!groupedItems.TryGetValue(item.CollectableType, out IntHashMap<CollectableItem> map))
            {
                Debug.LogError($"[CODE] Key {item.CollectableType} is not present in groupedItems", item);
                return;
            }

            if (!map.Remove(item.OrderNumber, out CollectableItem lastValue))
            {
                Debug.LogError("[CODE] Item is not present in list", item);
                return;
            }

            lastValue.SetCollidersActive(false);
        }
    }
}