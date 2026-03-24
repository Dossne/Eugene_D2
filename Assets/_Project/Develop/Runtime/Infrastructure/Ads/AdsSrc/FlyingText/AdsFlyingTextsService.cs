using Infrastructure.Pool.FloatingText;
using Infrastructure.Localization;
using Infrastructure.Pool;
using R3;
using UnityEngine;

namespace Infrastructure.Ads
{
    public class AdsFlyingTextsService
    {
        private readonly PoolService poolService;
        private readonly AdsFlyTextEvent textEvent;
        private readonly CompositeDisposable disposable;
        
        private FloatingTextPool floatingTextPool;
        private bool isInit;


        public AdsFlyingTextsService(PoolService poolService, AdsFlyTextEvent textEvent)
        {
            this.poolService = poolService;
            this.textEvent = textEvent;
            this.disposable = new CompositeDisposable();
        }


        public void Initialize()
        {
            if (isInit)
                return;
            floatingTextPool = poolService.Get<FloatingTextPool>();
            textEvent.Subscribe(_=> { FlyText(textEvent.localKey, textEvent.position); }).AddTo(disposable);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposable.Dispose();
            isInit = false;
        }


        private void FlyText(string localizationKey, Vector3 position)
        {
            if (floatingTextPool.TryGetItem(FloatingTextType.RewAdsText, out PoolableFloatingText loadTxt))
                loadTxt.Show(LocalizationService.I.Get(localizationKey), position, Vector3.one, false);
        }
    }
}