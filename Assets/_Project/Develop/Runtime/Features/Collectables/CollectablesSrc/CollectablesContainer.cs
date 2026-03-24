using System;
using System.Collections.Generic;
using Infrastructure.GameplayContainers;
using TriInspector;

namespace Features.Collectables
{
    public class CollectablesContainer : GameplayContainerBase<CollectableItem>
    {
        [ReadOnly]
        public List<CollectableLevelsItemsData> itemsOnLevel = new();
        
        [Button]
        private void Refresh()
        {
            itemsOnLevel.Clear();
            
            // Получаем все компоненты CollectableItem во всех дочерних объектах (включая вложенные)
            CollectableItem[] allItems = GetComponentsInChildren<CollectableItem>();
            
            foreach (CollectableItem item in allItems)
            {
                // Пропускаем сам контейнер, если на нем есть компонент CollectableItem
                if (item.transform == transform)
                    continue;
                
                var obj = itemsOnLevel.Find(x => x.type == item.CollectableType);
                if (obj != null)
                {
                    obj.count += 1;
                }
                else
                {
                    itemsOnLevel.Add(new CollectableLevelsItemsData
                    {
                        type = item.CollectableType,
                        count = 1
                    });
                }
            }
        }

        [Serializable]
        public class CollectableLevelsItemsData
        {
            [ReadOnly] public CollectableType type;
            [ReadOnly] public int count;
        }
    }
}