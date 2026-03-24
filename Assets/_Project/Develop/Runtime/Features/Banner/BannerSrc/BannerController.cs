using Infrastructure.Ads;
using Infrastructure.Configs;
using System;
using VContainer;

namespace Features.Banner
{
    public class BannerController
    {
        public event Action OnBannerStateChange;

        private readonly AdsConfig adsConfig;
        private bool isBannerOn;

        public float BannerSize => Advertisement.GetBackgroundBannerSize();
        public bool IsBannerOn => isBannerOn;


        [Inject]
        public BannerController(ConfigProvider configProvider)
        {
            adsConfig = configProvider.AdsConfig;
        }


        private bool IsInitialized { get; set; }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            HideBanner();

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            HideBanner();
            OnBannerStateChange = null;
            IsInitialized = false;
        }

        public void ShowBanner() 
        {
            if (adsConfig.AdsData.isBannerEnabled && Advertisement.IsRateAppPopupShown() && !Advertisement.IsPremium())
            {
                Advertisement.ShowBanner();
                isBannerOn = true;
            }

            OnBannerStateChange?.Invoke();
        }

        public void HideBanner()
        {
            Advertisement.HideBanner();
            isBannerOn = false;
            OnBannerStateChange?.Invoke();
        }  
    }
}