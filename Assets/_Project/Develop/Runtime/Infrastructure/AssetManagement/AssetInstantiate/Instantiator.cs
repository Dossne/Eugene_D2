using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Infrastructure.AssetManagement
{
    /// <summary>
    /// Creates instances and injects dependencies.
    /// Can return not active gameObject instance (gameObject.activeSelf == false)
    /// for dependency injection BEFORE Unity's Awake/OnEnable/Start callbacks are called
    /// </summary>
    public class Instantiator
    {
        private readonly IObjectResolver resolver;
        private readonly AssetProvider assetProvider;
        private readonly Transform inactiveTempRoot;


        public Instantiator(IObjectResolver resolver, AssetProvider assetProvider, Transform inactiveTempRoot)
        {
            this.resolver = resolver;
            this.assetProvider = assetProvider;
            this.inactiveTempRoot = inactiveTempRoot;

            if (inactiveTempRoot.gameObject.activeSelf)
                inactiveTempRoot.gameObject.SetActive(false);
        }


        #region Inject

        // Inject all relevant dependencies into foo. Injecting
        // an object twice will overwrite any property or
        // field (or call the method) which is marked with [Inject].
        public void InjectObject(object item)
        {
            resolver.Inject(item);
        }


        // Inject dependencies into the MonoBehaviours of this
        // GameObject and its descendents, regardless of whether
        // the targeted GameObjects and MonoBehaviours are enabled.
        public void InjectGameObject(GameObject item)
        {
            resolver.InjectGameObject(item);
        }

        #endregion

        #region C# class. Instantiate

        /// <summary>
        /// Creates an instance of a class with dependency resolving. Important: Only the first constructor of the class is used.
        /// </summary>
        public T InstantiateClass<T>(Lifetime lifetime = Lifetime.Singleton) where T : class
        {
            return resolver.Instantiate<T>(lifetime);
        }


        /// <summary>
        /// Creates an instance of a class with dependency resolving. 
        /// The order of elements in args must be the same as in the constructor.
        /// Container dependencies are resolved automatically.
        /// </summary>
        public T InstantiateClass<T>(Lifetime lifetime = Lifetime.Singleton, params object[] args) where T : class
        {
            return resolver.Instantiate<T>(lifetime, args);
        }

        #endregion

        #region Unity. Instantiate

        public T Instantiate<T>(
            T prefab,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true) where T : Component
        {
            if (prefab == null)
                throw new NullReferenceException("No object to instantiate");

            T instance = Object.Instantiate(prefab, inject || !isActive? inactiveTempRoot: parent);

            PrepareInstance(instance.gameObject, at, rotation, parent, worldSpace, inject, isActive);

            return instance;
        }


        public GameObject Instantiate(
            GameObject prefab,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true)
        {
            if (prefab == null)
                throw new NullReferenceException("No object to instantiate");

            GameObject instance = Object.Instantiate(prefab, inactiveTempRoot);
            PrepareInstance(instance, at, rotation, parent, worldSpace, inject, isActive);
            return instance;
        }

        #endregion

        #region Unity. Instantiate Async

        public async UniTask<T> InstantiateAsync<T>(
            T prefab,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true,
            CancellationToken cancellationToken = default) where T : Component
        {
            if (prefab == null)
                throw new NullReferenceException("No object to instantiate");

            AsyncInstantiateOperation<T> operationAsync = Object.InstantiateAsync(prefab, 1, inject || !isActive ? inactiveTempRoot : parent);
            await operationAsync.ToUniTask(cancellationToken: cancellationToken);
            T instance = operationAsync.Result[0];
            PrepareInstance(instance.gameObject, at, rotation, parent, worldSpace, inject, isActive);
            return instance;
        }


        public async UniTask<GameObject> InstantiateAsync(
            GameObject prefab,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true,
            CancellationToken cancellationToken = default)
        {
            if (prefab == null)
                throw new NullReferenceException("No object to instantiate");

            AsyncInstantiateOperation<GameObject> operationAsync = Object.InstantiateAsync(prefab, 1, inject || !isActive ? inactiveTempRoot : parent);
            await operationAsync.ToUniTask(cancellationToken: cancellationToken);
            GameObject instance = operationAsync.Result[0];
            PrepareInstance(instance.gameObject, at, rotation, parent, worldSpace, inject, isActive);
            return instance;
        }

        #endregion

        #region Unity Addressable. Instantiate Async

        public async UniTask<T> InstantiateAsync<T>(
            string assetGUID,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true,
            bool isInstantiateAsync = true,
            CancellationToken cancellationToken = default) where T : Component
        {
            GameObject prefabGo = await assetProvider.AddressableLoadAssetAsync<GameObject>(assetGUID, cancellationToken);

            if (prefabGo == null)
                throw new NullReferenceException("No object to instantiate");

            T prefab = prefabGo.GetComponent<T>();
            
            if (isInstantiateAsync)
                return await InstantiateAsync(prefab, at, rotation, parent, worldSpace, inject, isActive, cancellationToken);
            
            return Instantiate(prefab, at, rotation, parent, worldSpace, inject, isActive);

        }


        public async UniTask<GameObject> InstantiateAsync(
            string assetGUID,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true,
            bool isInstantiateAsync = true,
            CancellationToken cancellationToken = default)
        {
            GameObject prefabGo = await assetProvider.AddressableLoadAssetAsync<GameObject>(assetGUID, cancellationToken);
            
            if (prefabGo == null)
                throw new NullReferenceException("No object to instantiate");
            
            if (isInstantiateAsync)
                return await InstantiateAsync(prefabGo, at, rotation, parent, worldSpace, inject, isActive, cancellationToken);
            
            return Instantiate(prefabGo, at, rotation, parent, worldSpace, inject, isActive);
        }


        public async UniTask<T> InstantiateAsync<T>(
            AssetReference assetReference,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true,
            bool isInstantiateAsync = true,
            CancellationToken cancellationToken = default) where T : Component
        {
            return await InstantiateAsync<T>(assetReference.AssetGUID, at, rotation, parent, worldSpace, inject, isActive, isInstantiateAsync, cancellationToken);
        }


        public async UniTask<GameObject> InstantiateAsync(
            AssetReference assetReference,
            Vector3 at = default,
            Quaternion rotation = default,
            Transform parent = null,
            bool worldSpace = true,
            bool inject = false,
            bool isActive = true,
            bool isInstantiateAsync = true,
            CancellationToken cancellationToken = default)
        {
            return await InstantiateAsync(assetReference.AssetGUID, at, rotation, parent, worldSpace, inject, isActive, isInstantiateAsync, cancellationToken);
        }

        #endregion


        private void PrepareInstance(GameObject instance, Vector3 at, Quaternion rot, Transform parent, bool worldSpace, bool inject, bool isActive)
        {
            if (inject)
                InjectGameObject(instance);

            if (instance.activeSelf != isActive)
                instance.SetActive(isActive);

            if(instance.transform.parent != parent)
                instance.transform.SetParent(parent, worldSpace);

            if (worldSpace)
                instance.transform.SetPositionAndRotation(at, rot);
            else
                instance.transform.SetLocalPositionAndRotation(at, rot);
        }
    }
}