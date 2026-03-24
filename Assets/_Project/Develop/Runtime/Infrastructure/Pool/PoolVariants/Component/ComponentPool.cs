using System;
using System.Collections.Generic;
using System.Threading;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.MainUICanvasControl;
using UnityEngine;
using VContainer;

namespace Infrastructure.Pool
{
    /// <summary>
    /// Multiple pools with items of type V
    /// </summary>
    /// <typeparam name="K">key (string, enum, int and etc)</typeparam>
    /// <typeparam name="V">Prefab type</typeparam>
    public class ComponentPool<K, V> : MonoBehaviour, IPool where V : Component, IPoolableObject<V>
    {
        [Tooltip("Nullable. Set tag for multiple pools with same type")] public string poolTag;

        [SerializeField] private PoolSettings settings;

        [Space(20f)]
#if UNITY_EDITOR
        [TriInspector.ValidateInput(nameof(ValidateSettings))]
#endif
        [TriInspector.ListDrawerSettings(ShowElementLabels = true)]
        [SerializeField] private List<PoolItemData<K>> items = new();

#if UNITY_EDITOR
        [TriInspector.Title("Debug")] [SerializeField, TriInspector.ReadOnly]
#endif
        private SerializedDictionary<K, ComponentSubPool<V>> subPools = new();

        protected MainUIProvider mainUIProvider;
        private Instantiator instantiator;
        private AssetProvider assetProvider;
        public string Tag => poolTag;


        [Inject]
        public void Construct(Instantiator instantiator, AssetProvider assetProvider, MainUIProvider mainUIProvider)
        {
            this.instantiator = instantiator;
            this.assetProvider = assetProvider;
            this.mainUIProvider = mainUIProvider;
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            subPools.Clear();

            foreach (PoolItemData<K> config in items)
            {
                if (subPools.ContainsKey(config.poolItemKey))
                {
                    Debug.LogError($"[PoolService] Duplicate key {config.poolItemKey} in pool {gameObject.name}");
                    continue;
                }

                var currentSettings = config.useSpecificSettings ? config.specificSettings : settings;

                if (!config.useSpecificSettings)
                    config.specificSettings = null;
                
                ComponentSubPool<V> itemsPool = new ComponentSubPool<V>(instantiator, assetProvider, transform, config.assetSourceData, currentSettings, poolTag);
                subPools.Add(config.poolItemKey, itemsPool);
            }

            OnBeforeInitialize();
            
            foreach (var entryPair in subPools)
            {
                await entryPair.Value.InitializeAsync(cancellationToken);
            }
            
            OnAfterInitialize();;
            SetObjectActive(true);
        }


        public void ReleaseAllToPool()
        {
            foreach (KeyValuePair<K, ComponentSubPool<V>> pool in subPools)
            {
                pool.Value.ReleaseAllToPool();
            }
        }


        public void ReleaseAllToPool(K kType)
        {
            foreach (KeyValuePair<K, ComponentSubPool<V>> pool in subPools)
            {
                if(Convert.ToInt64(pool.Key) == Convert.ToInt64(kType))
                {
                    pool.Value.ReleaseAllToPool();
                }
            }
        }


        public void Destroy()
        {
            foreach (KeyValuePair<K, ComponentSubPool<V>> pool in subPools)
            {
                pool.Value.Destroy();
            }

            subPools.Clear();
        }


        public bool TryGetItem(K key, out V item)
        {
            if (subPools.TryGetValue(key, out ComponentSubPool<V> pool))
                return pool.TryGetItem(out item);

            item = null;
            Debug.LogError($"[PoolService] No pool found for key {key.ToString()}");
            return false;
        }


        public UniTask<(bool isSuccess, V item)> TryGetItemAsync(K key, CancellationToken cancellationToken)
        {
            if (subPools.TryGetValue(key, out ComponentSubPool<V> pool))
                return pool.TryGetItemAsync(cancellationToken);

            Debug.LogError($"[PoolService] No pool found for key {key.ToString()}");
            return new UniTask<(bool, V)>((false, null));
        }


        /// <summary>
        /// Custom root for all items
        /// </summary>
        public void SetItemsRoot(Transform value)
        {
            foreach (KeyValuePair<K, ComponentSubPool<V>> pool in subPools)
            {
                pool.Value.SetItemsRoot(value);
            }
        }


        /// <summary>
        /// Custom root for specific type items
        /// </summary>
        public void SetItemsRoot(K key, Transform value)
        {
            if (subPools.TryGetValue(key, out var subPool))
            {
                subPool.SetItemsRoot(value);
            }
        }


        protected virtual void OnBeforeInitialize() { }
        protected virtual void OnAfterInitialize() { }
        
        
        private void SetObjectActive(bool isActive)
        {
            if (gameObject.activeSelf != isActive)
                gameObject.SetActive(isActive);
        }


#if UNITY_EDITOR
        public TriInspector.TriValidationResult ValidateSettings()
        {
            bool hasDuplicate = false;
            K duplicateKey = default;

            for (int i = 0; i < items.Count; i++)
            {
                if (items.FindAll(x => x.poolItemKey.ToString() == items[i].poolItemKey.ToString()).Count > 1)
                {
                    duplicateKey = items[i].poolItemKey;
                    hasDuplicate = true;
                    break;
                }

            }

            return hasDuplicate
                ? TriInspector.TriValidationResult.Error($"ERROR. Duplicate key: {duplicateKey}!")
                : TriInspector.TriValidationResult.Valid;
        }
#endif
    }
}