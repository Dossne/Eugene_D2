using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Infrastructure.Pool
{
    /// <summary>
    /// Provide pools:
    /// default (created from start app)
    /// specific (features register and destroy by request)
    /// </summary>
    public class PoolService : MonoBehaviour
    {
        private readonly Dictionary<(string tag, Type), IPool> pools = new();
        private Instantiator instantiator;
        private bool isInit;


        [Inject]
        public void Construct(Instantiator scopedInstantiator)
        {
            this.instantiator = scopedInstantiator;
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;

            foreach (Transform childTransform in transform)
            {
                if (!childTransform.TryGetComponent(out IPool pool))
                {
                    continue;
                }

                await Register_Inner(instantiator, pool, cancellationToken, transform);
            }

            isInit = true;
        }


        public void ReleaseAllToPool()
        {
            foreach (KeyValuePair<(string, Type), IPool> item in pools)
            {
                item.Value.ReleaseAllToPool();
            }
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (KeyValuePair<(string, Type), IPool> item in pools)
            {
                item.Value.Destroy();
            }

            pools.Clear();
            isInit = false;
        }


        /// <summary>
        /// Register speciefic pool in service and get instance.
        /// IMPORTANT. In order to receive dependencies on inject from current scene, set scopedInstantiator to method param.
        /// Otherwise, will be used default from main scope
        /// </summary>
        public async UniTask<T> Register<T>(AssetReference assetRef, CancellationToken cancellationToken, Instantiator scopedInstantiator = null)
            where T : MonoBehaviour, IPool
        {
            var currentIntantiator = scopedInstantiator ?? instantiator;
            T instance = await currentIntantiator.InstantiateAsync<T>(assetRef,
                                                                      at: Vector3.zero,
                                                                      rotation: Quaternion.identity,
                                                                      parent: null,
                                                                      worldSpace: true,
                                                                      inject: false,
                                                                      isActive: true,
                                                                      isInstantiateAsync: false,
                                                                      cancellationToken: cancellationToken);
            
            await Register_Inner(currentIntantiator, instance, cancellationToken, transform);
            return instance;
        }


        public void UnregisterAndDestroy(IPool poolInstance)
        {
            (string tag, Type poolType) poolKey = GetKey(poolInstance);
            pools.Remove(poolKey);

            poolInstance.ReleaseAllToPool();
            poolInstance.Destroy();

            if (poolInstance is MonoBehaviour monoBehaviourPool && monoBehaviourPool != null)
            {
                Destroy(monoBehaviourPool.gameObject);
            }
        }


        /// <summary>
        /// Get registered pool
        /// </summary>
        public T Get<T>(string poolTag = null) where T : IPool
        {
            (string tag, Type) key = (poolTag, typeof(T));

            if (!pools.TryGetValue(key, out IPool pool))
            {
                throw new Exception($"Item with key {key.ToString()} is not registered");
            }

            return (T)pool;
        }


        private async UniTask Register_Inner(Instantiator scopedInstantiator, IPool poolInstance, CancellationToken cancellationToken, Transform parent)
        {
            (string tag, Type) key = GetKey(poolInstance);

            if (pools.ContainsKey(key))
            {
                throw new Exception($"Duplicate key: {key.ToString()}. Try set another {nameof(poolInstance.Tag)} of {poolInstance.GetType().Name}.");
            }

            scopedInstantiator.InjectObject(poolInstance);
            await poolInstance.InitializeAsync(cancellationToken);
            pools.Add(key, poolInstance);

            if (parent != null && poolInstance is MonoBehaviour monoBehaviourPool)
            {
                monoBehaviourPool.transform.SetParent(parent);
                monoBehaviourPool.transform.position = Vector3.zero;
            }
        }


        private (string tag, Type poolType) GetKey(IPool poolInstance)
        {
            string keyTag = poolInstance.Tag?.Trim();

            keyTag = string.IsNullOrEmpty(keyTag) ? null : keyTag;
            return (keyTag, poolInstance.GetType());
        }
    }
}