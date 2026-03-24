using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using System.Threading;
using UnityEngine;

namespace Features.Widgets
{
    public class WidgetFactory
    {
        private readonly Instantiator instantiator;

        public WidgetFactory(Instantiator instantiator)
        {
            this.instantiator = instantiator;
        }

        public UniTask<T> CreateAsync<T>(
            string assetGUID,
            RectTransform parent,
            CancellationToken cancellationToken,
            bool injectDependency,
            bool isInstantiateAsync
            )
            where T : Widget
        {
            return instantiator.InstantiateAsync<T>(assetGUID, Vector3.zero, Quaternion.identity, parent, false, injectDependency, false, isInstantiateAsync,
                                                    cancellationToken);
        }
    }
}
