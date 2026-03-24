using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using System.Threading;
using UnityEngine;

namespace Infrastructure.TooltipControl
{
    public class TooltipService 
    {
        private readonly Instantiator instantiator;


        public TooltipService(Instantiator instantiator)
        {
            this.instantiator = instantiator;
        }

        public UniTask<T> CreateCustomTooltipAsync<T>(
            string assetGUID,
            RectTransform parent,
            CancellationToken cancellationToken,
            bool injectDependency,
            bool isInstantiateAsync
        )
            where T : CustomTooltip
        {
            return instantiator.InstantiateAsync<T>(assetGUID,
                                                    Vector3.zero,
                                                    Quaternion.identity, 
                                                    parent,
                                                    false,
                                                    injectDependency,
                                                    false,
                                                    isInstantiateAsync,
                                                    cancellationToken);
        }

        public UniTask<T> CreateSimpleTutorialTooltipAsync<T>(
            string assetGUID,
            RectTransform parent,
            CancellationToken cancellationToken,
            bool injectDependency,
            bool isInstantiateAsync
        )
            where T : SimpleTutorialTooltip
        {
            return instantiator.InstantiateAsync<T>(assetGUID,
                                                    Vector3.zero,
                                                    Quaternion.identity,
                                                    parent,
                                                    false,
                                                    injectDependency,
                                                    false,
                                                    isInstantiateAsync,
                                                    cancellationToken);
        }
    }
}