using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Infrastructure.AssetManagement
{
    /// <summary>
    /// Provides assets by addressables and resource path
    /// </summary>
    public class AssetProvider
    {
        private readonly Dictionary<string /*resourcePath*/, Object> assetRefResources = new();
        private readonly Dictionary<string /*addressableGUID*/, AssetReferenceCounter> assetRefAddressables = new();

        private bool isBusy;
        private bool isDebugLog;


        public T ResourcesLoadAsset<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("[AssetProvider] Path is not valid");
            }

            if (assetRefResources.TryGetValue(path, out Object cached))
            {
                return cached as T;
            }

            T prefab = Resources.Load<T>(path);

            if (prefab == null)
            {
                throw new Exception($"[AssetProvider] Prefab \"{typeof(T)}\" by path \"{path}\" is not found");
            }

            assetRefResources[path] = prefab;
            return prefab;
        }


        public UniTask<T> AddressableLoadAssetAsync<T>(string assetGUID, CancellationToken cancellationToken) where T : Object
        {
            if (string.IsNullOrEmpty(assetGUID))
            {
                return UniTask.FromException<T>(new NullReferenceException("[AssetProvider] Path is not valid"));
            }

            if (assetRefAddressables.TryGetValue(assetGUID, out AssetReferenceCounter cachedCounter) && cachedCounter.IsValid())
            {
                cachedCounter.IncrementCounter();
                return cachedCounter.GetResultAsync<T>(cancellationToken);;
            }
            
            return LoadAndCacheAsset<T>(assetGUID, cancellationToken);
        }


        private async UniTask<T> LoadAndCacheAsset<T>(string assetGUID, CancellationToken cancellationToken) where T : Object
        {
            if (!HasKey(assetGUID))
                throw new InvalidKeyException($"[AssetProvider] Addressable with key {assetGUID} is not found in locators");
            
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetGUID);
            AssetReferenceCounter customCounter = new AssetReferenceCounter(handle);

            assetRefAddressables[assetGUID] = customCounter;
            T newResult = await customCounter.GetResultAsync<T>(cancellationToken);

            DebugLog<T>(assetGUID);
            
            return newResult;
        }
        
        
        private bool HasKey(string assetGUID)
        {
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(assetGUID, typeof(object), out _))
                    return true;
            }
            return false;
        }
        
        public void ReleaseAsset(string assetGUID)
        {
            if (string.IsNullOrEmpty(assetGUID))
            {
                return;
            }

            if (assetRefAddressables.TryGetValue(assetGUID, out AssetReferenceCounter counter) && counter.IsValid())
            {
                counter.DecrementCounter();
                if (counter.GetRefCount() <= 0)
                {
                    assetRefAddressables.Remove(assetGUID);
                }
            }
        }


        public void Deinitialize()
        {
            if (isBusy)
                return;

            isBusy = true;

            foreach (var pair in assetRefAddressables)
            {
                //UnityEngine.Debug.Log($"Begin releaseAll. GUID: {pair.Key}. Count = {pair.Value.GetRefCount()}");
                pair.Value.ReleaseAll();
            }

            assetRefAddressables.Clear();
            assetRefResources.Clear();
            isBusy = false;
        }


        private void DebugLog<T>(string key)
        {
#if PR_CHEAT
            if (isDebugLog)
            {
                Debug.Log($"Added type \"{typeof(T).Name}\" by key \"{key}\"");
            }
#endif
        }
    }
}