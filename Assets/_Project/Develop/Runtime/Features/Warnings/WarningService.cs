using Infrastructure.Pool.FloatingText;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.Pool;
using Infrastructure.WalletSystem;
using UnityEngine;
using Infrastructure.MainUICanvasControl;

namespace Features.Warnings
{
    public class WarningService
    {
        private readonly CurrencyConfig currencyConfig;
        private FloatingTextPool floatingTextPool;
        private readonly PoolService poolService;

        private bool isInit;


        public WarningService(ConfigProvider configProvider, 
                              PoolService poolService)
        {
            this.poolService = poolService;
            this.currencyConfig = configProvider.CurrencyConfig;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            floatingTextPool = poolService.Get<FloatingTextPool>();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            isInit = false;
        }


        public void ShowNotEnough(CurrencyType currency, Vector3 pos = default, bool isAnchored = true)
        {
            if (!currencyConfig.TryGet(currency, out CurrencyData data))
            {
                return;
            }

            if(!floatingTextPool.TryGetItem(FloatingTextType.NotEnoughCurrency, out PoolableFloatingText flyText))
                return;
            
            var currencyName = LocalizationService.I.Get(data.nameLocKey);
            flyText.Show(LocalizationService.I.Get(LocKeys.Currency.NotEnough, currencyName), pos, Vector3.one, isAnchored);
        }

        public void ShowAlert(string text, FloatingTextType alertType, Vector3 pos = default, bool isAnchored = true)
        {
            if (!floatingTextPool.TryGetItem(alertType, out PoolableFloatingText flyText))
                return;

            flyText.Show(text, pos, Vector3.one, isAnchored);            
        }
    }
}