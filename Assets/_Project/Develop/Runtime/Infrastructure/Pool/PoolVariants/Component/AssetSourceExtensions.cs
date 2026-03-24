using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Infrastructure.Pool
{
    public static class AssetSourceExtensions
    {
        public static async UniTask<T> GetPrefabAsync<T>(this AssetSourceData assetConfig, AssetProvider assetProvider, CancellationToken ct)
        {
            GameObject prefab;

            switch (assetConfig.assetSourceType)
            {
                case AssetSourceType.AddressableByPath:
                    prefab = await assetProvider.AddressableLoadAssetAsync<GameObject>(assetConfig.addressablePath, ct);
                    break;

                case AssetSourceType.AddressableByRef:
                    prefab = await assetProvider.AddressableLoadAssetAsync<GameObject>(assetConfig.addressableRef.AssetGUID, ct);
                    break;

                case AssetSourceType.GameObjectPf:
                    prefab = assetConfig.gameObjectPrefab;
                    break;

                case AssetSourceType.ResourcesByPath:
                    prefab = assetProvider.ResourcesLoadAsset<GameObject>(assetConfig.resourceFullPath);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        $"Behaviour for {nameof(AssetSourceType)} = {assetConfig.assetSourceType} is not implemented");
            }

            return prefab.GetComponent<T>();
        }


        public static async UniTask<T> GetInstanceAsync<T>(this AssetSourceData conf,
                                                           Instantiator instantiator,
                                                           AssetProvider assetProvider,
                                                           Vector3 at,
                                                           Quaternion rot,
                                                           Transform root,
                                                           bool worldSpace,
                                                           bool inject,
                                                           bool isActive,
                                                           bool isInstantiateAsync,
                                                           CancellationToken ct) where T : Component
        {
            switch (conf.assetSourceType)
            {
                case AssetSourceType.AddressableByPath:
                    return await instantiator.InstantiateAsync<T>(conf.addressablePath, at, rot, root, worldSpace, inject, isActive, isInstantiateAsync, ct);

                case AssetSourceType.AddressableByRef:
                    return await instantiator.InstantiateAsync<T>(conf.addressableRef, at, rot, root, worldSpace, inject, isActive, isInstantiateAsync, ct);

                case AssetSourceType.GameObjectPf:
                    return instantiator.Instantiate(conf.gameObjectPrefab.GetComponent<T>(), at, rot, root, worldSpace, inject, isActive);

                case AssetSourceType.ResourcesByPath:
                    T prefab = assetProvider.ResourcesLoadAsset<T>(conf.resourceFullPath);
                    return instantiator.Instantiate(prefab, at, rot, root, worldSpace, inject, isActive);

                default:
                    throw new ArgumentOutOfRangeException(
                        $"Behaviour for {nameof(AssetSourceType)} = {conf.assetSourceType} is not implemented");
            }
        }
    }
}