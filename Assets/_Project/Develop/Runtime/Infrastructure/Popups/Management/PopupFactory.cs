using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Infrastructure.Popups
{
    public class PopupFactory
    {
        private readonly Instantiator instantiator;
        private readonly AssetProvider assetProvider;


        public PopupFactory(Instantiator instantiator, AssetProvider assetProvider)
        {
            this.instantiator = instantiator;
            this.assetProvider = assetProvider;
        }


        public UniTask<T> CreateAsync<T>(
            RectTransform parent,
            CancellationToken cancellationToken,
            bool injectDependency,
            bool isInstantiateAsync
        )
            where T : PopupBase
        {

            string assetGUID = typeof(T).Name;

            return instantiator.InstantiateAsync<T>(assetGUID,
                                                    Vector3.zero,
                                                    Quaternion.identity, parent,
                                                    false,
                                                    injectDependency,
                                                    false,
                                                    isInstantiateAsync,
                                                    cancellationToken);
        }


        public void ReleaseAsset(PopupBase popup)
        {
            string assetGUID = popup.GetType().Name;
            assetProvider.ReleaseAsset(assetGUID);
        }
    }
}