using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.Collections;
using UnityEngine;

namespace Infrastructure.Pool
{
    [Serializable]
    public class ComponentSubPool<V> where V : Component, IPoolableObject<V>
    {
        private readonly Instantiator instantiator;
        private readonly AssetProvider assetProvider;
        private readonly AssetSourceData assetConfig;
        private readonly PoolSettings settings;
        private readonly string containerKey;

        private V prefab;
        private int totalCount;
        private bool isInit;

#if UNITY_EDITOR
        [TriInspector.Title("Debug")]
        [SerializeField, TriInspector.ReadOnly]
#endif
        private Transform parentRoot;

#if UNITY_EDITOR
        [SerializeField, TriInspector.ReadOnly]
#endif
        private IntHashMap<V> activeElements;

#if UNITY_EDITOR
        [SerializeField, TriInspector.ReadOnly]
#endif
        private FastList<V> inactiveElements;


        public ComponentSubPool(
            Instantiator instantiator,
            AssetProvider assetProvider,
            Transform parentRoot,
            AssetSourceData assetConfig,
            PoolSettings settings,
            string poolTag)
        {
            this.instantiator = instantiator;
            this.assetProvider = assetProvider;
            this.parentRoot = parentRoot;
            this.assetConfig = assetConfig;
            this.settings = settings;
            containerKey = string.IsNullOrEmpty(poolTag) ? $"[{typeof(V).Name}]" : $"[{poolTag} - {typeof(V).Name}]";
            activeElements = new IntHashMap<V>(settings.preWarmCount);
            inactiveElements = new FastList<V>(settings.preWarmCount);
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;

            prefab = await assetConfig.GetPrefabAsync<V>(assetProvider, cancellationToken);

            for (int i = 0; i < settings.preWarmCount; i++)
            {
                V element = await CreateItemAsync(cancellationToken);
                inactiveElements.Add(element);
                element.IsInPool = true;
            }

            isInit = true;
        }


        public void ReleaseAllToPool()
        {
            if (!isInit)
                return;
            
            if (activeElements.length == 0)
                return;
            
            foreach (var idx in activeElements)
            {
                V item = activeElements.GetValueByIndex(in idx);

                if (item.IsInPool)
                    continue;

                //Debug.Log($"[PoolService] {item.GetType().Name} Releasing {item.PooledId}. But its index is {idx}");
                inactiveElements.Add(item);
                item.IsInPool = true;
                item.OnPoolRelease();
            }

            activeElements.Clear();
        }


        public void Destroy()
        {
            if (!isInit)
                return;

            prefab = null;
            DecrementAssetRef();

            foreach (var item in inactiveElements)
            {
                OnDestroyItem(item);
            }

            foreach (var idx in activeElements)
            {
                V item = activeElements.GetValueByIndex(in idx);
                OnDestroyItem(item);
            }

            inactiveElements.Clear();
            activeElements.Clear();
            totalCount = 0;
            isInit = false;
        }


        public bool TryGetItem(out V item)
        {
            item = null;

            if (IsMaxReached())
            {
                LogOnMaxReach();

                if (!settings.createNewOnMaxReach)
                    return false;
            }

            if (inactiveElements.length == 0)
            {
                item = CreateItem();
                activeElements.Add(item.PooledId, item, out _);
            }
            else
            {
                item = PopItem();
            }

            OnGetItem(item);
            return true;
        }


        public async UniTask<(bool isSuccess, V item)> TryGetItemAsync(CancellationToken cancellationToken)
        {
            if (IsMaxReached())
            {
                LogOnMaxReach();

                if (!settings.createNewOnMaxReach)
                    return (false, null);
            }

            V item;

            if (inactiveElements.length == 0)
            {
                item = await CreateItemAsync(cancellationToken);
                activeElements.Add(item.PooledId, item, out _);
            }
            else
            {
                item = PopItem();
            }

            OnGetItem(item);

            return (true, item);
        }


        public void SetItemsRoot(Transform customRoot)
        {
            this.parentRoot = customRoot;
        }


        private void Release(V item)
        {
            if (!activeElements.Remove(item.PooledId, out _) || item.IsInPool)
                return;

            //Debug.Log($"[PoolService] Release: {element.GetType().Name}. ID = {element.PooledId}. Should be last");

            item.OnPoolRelease();
            inactiveElements.Add(item);
            item.IsInPool = true;
        }


        private V PopItem()
        {
            int lastIdx = inactiveElements.length - 1;
            V item = inactiveElements[lastIdx];

            inactiveElements.RemoveAtSwapBackFast(lastIdx);
            activeElements.Add(item.PooledId, item, out _);
            item.IsInPool = false;
            //Debug.Log($"[PoolService] PopItem: {item.GetType().Name}. ID = {item.PooledId}. Should be last");
            return item;
        }


        private bool IsMaxReached()
        {
            return activeElements.length >= settings.maxPoolSize;
        }


        private void LogOnMaxReach()
        {
            if (settings.logOnMaxReach == LogType.None)
                return;

            string msg = $"[PoolService] {containerKey} pool limit is reached! ActiveCount = {activeElements.length}. Limit = {settings.maxPoolSize}";

            switch (settings.logOnMaxReach)
            {
                case LogType.Log:
                    Debug.Log(msg);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(msg);
                    break;
                case LogType.Error:
                    Debug.LogError(msg);
                    break;
            }
        }


        private V CreateItem()
        {
            V item = instantiator.Instantiate(prefab, Vector3.zero, Quaternion.identity, parentRoot, settings.itemWorldSpace,
                                              settings.itemInject == ItemInjectType.OnCreate,
                                              settings.instantiateActiveGo);

            InitializeItem(item);
            return item;
        }


        private async UniTask<V> CreateItemAsync(CancellationToken cancellationToken)
        {
            bool inject = settings.itemInject == ItemInjectType.OnCreate;

            V item = await assetConfig.GetInstanceAsync<V>(instantiator, assetProvider, Vector3.zero, Quaternion.identity, parentRoot,
                                                           settings.itemWorldSpace, inject,
                                                           settings.instantiateActiveGo,
                                                           settings.instantiateAsync,
                                                           cancellationToken);
            InitializeItem(item);
            return item;
        }


        private void OnGetItem(V item)
        {
            if (settings.itemInject == ItemInjectType.OnEachGet)
            {
                instantiator.InjectObject(item);
            }

            item.OnPoolGet();
        }


        private void OnDestroyItem(V item)
        {
            item.RequestReleaseToPool -= IPoolableObject_RequestReleaseToPool;
            item.ObserveDestroy -= IPoolableObject_ObserveDestroy;
            item.Destroy();
        }


        private void InitializeItem(V item)
        {
            item.PooledId = GetNextId();
            item.OnCreate();
            item.OnPoolRelease();
            item.RequestReleaseToPool += IPoolableObject_RequestReleaseToPool;
            item.ObserveDestroy += IPoolableObject_ObserveDestroy;
        }


        private void DecrementAssetRef()
        {
            switch (assetConfig.assetSourceType)
            {
                case AssetSourceType.AddressableByPath:
                    assetProvider.ReleaseAsset(assetConfig.addressablePath);
                    break;
                case AssetSourceType.AddressableByRef:
                    assetProvider.ReleaseAsset(assetConfig.addressableRef.AssetGUID);
                    break;
            }
        }


        private int GetNextId()
        {
            return ++totalCount;
        }


        private void IPoolableObject_RequestReleaseToPool(V element)
        {
            Release(element);

            if (settings.resetParentOnRelease)
            {
                element.SetParent(parentRoot);
            }
        }


        private void IPoolableObject_ObserveDestroy(V item)
        {
            item.RequestReleaseToPool -= IPoolableObject_RequestReleaseToPool;
            item.ObserveDestroy -= IPoolableObject_ObserveDestroy;
            inactiveElements.Remove(item);
            activeElements.Remove(item.PooledId, out _);
        }
    }
}