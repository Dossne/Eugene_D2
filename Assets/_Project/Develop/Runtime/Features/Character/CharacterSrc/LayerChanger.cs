using Features.Collectables;
using Infrastructure.Collections;
using UnityEngine;

namespace Features.Character
{
    public class LayerChanger : MonoBehaviour
    {
        
#if UNITY_EDITOR
        [TriInspector.ShowInInspector]
#endif
        private FastList<int /*persistentId*/> itemIds = new(500);
        
#if UNITY_EDITOR
        [TriInspector.ShowInInspector]
#endif
        private IntHashMap<CollectableItem> itemMap = new(500);

        private readonly float checkTime = 0.25f;
        private readonly int step = 2;
        private float currentTime;
        private int frameOffset;

        private void Update()
        {
            currentTime -= Time.deltaTime;

            if (currentTime > 0)
                return;

            currentTime = checkTime;
            frameOffset++;
            int totalCount = itemIds.length;
            
            for (int i = frameOffset % step; i < totalCount; i += step) //event or odd
            {
                bool itemExists = itemMap.TryGetValue(itemIds[i], out CollectableItem item);

                if (!itemExists)
                {
                    itemIds.RemoveAtSwapBackFast(i);
                    totalCount = itemIds.length;
                    continue;
                }
                
                if (item.IsCollected || !item.IsActive)
                {
                    itemMap.Remove(item.OrderNumber, out _);
                    itemIds.RemoveAtSwapBackFast(i);
                    totalCount = itemIds.length;
                    continue;
                }

                item.RigidbodyWakeUpIfSleeping();
            }
        }


        private void OnTriggerEnter(Collider collider)
        {
            Rigidbody rb = collider.attachedRigidbody;

            if (rb == null || !rb.TryGetComponent(out CollectableItem item) || !itemMap.Add(item.OrderNumber, item, out _))
                return;

            itemIds.Add(item.OrderNumber);
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.WakeUp();
            item.SetCollidersLayerToNonCollab();
        }


        private void OnTriggerExit(Collider collider)
        {
            Rigidbody rb = collider.attachedRigidbody;

            if (rb == null || !rb.TryGetComponent(out CollectableItem item) || !itemMap.Remove(item.OrderNumber, out _))
                return;

            item.SetCollidersLayerToDefault();
        }


        public void Initialize()
        {

        }


        public void Deinitialize()
        {
            itemIds.Clear();
            itemMap.Clear();
        }
    }
}