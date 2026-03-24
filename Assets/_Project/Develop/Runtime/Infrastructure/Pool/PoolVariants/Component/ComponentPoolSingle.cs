using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.MainUICanvasControl;
using UnityEngine;
using VContainer;

namespace Infrastructure.Pool
{
    /// <summary>
    /// Single pool with items of type V
    /// </summary>
    /// <typeparam name="V">prefab type</typeparam>
    public class ComponentPoolSingle<V> : MonoBehaviour, IPool where V : Component, IPoolableObject<V>
    {
        [Tooltip("Nullable. Set tag for multiple pools with same type")] public string poolTag;
        [SerializeField] private PoolSettings settings;
        [SerializeField] private AssetSourceData assetSourceData = new();

#if UNITY_EDITOR
        [TriInspector.Title("Debug")] [SerializeField, TriInspector.ReadOnly]
#endif
        private ComponentSubPool<V> subPool;

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
            subPool = new ComponentSubPool<V>(instantiator, assetProvider, transform, assetSourceData, settings, poolTag);
            OnBeforeInitialize();
            await subPool.InitializeAsync(cancellationToken);
            OnAfterInitialize();
            SetObjectActive(true);
        }


        public void ReleaseAllToPool()
        {
            subPool.ReleaseAllToPool();

        }


        public void Destroy()
        {
            subPool.Destroy();
        }


        public bool TryGetItem(out V item)
        {
            return subPool.TryGetItem(out item);
        }


        public UniTask<(bool isSuccess, V item)> TryGetItemAsync(CancellationToken cancellationToken)
        {
            return subPool.TryGetItemAsync(cancellationToken);
        }


        /// <summary>
        /// Custom root for items
        /// </summary>
        public void SetItemsRoot(Transform value)
        {
            subPool.SetItemsRoot(value);
        }


        protected virtual void OnBeforeInitialize() { }
        protected virtual void OnAfterInitialize() { }


        private void SetObjectActive(bool isActive)
        {
            if (gameObject.activeSelf != isActive)
                gameObject.SetActive(isActive);
        }
    }
}