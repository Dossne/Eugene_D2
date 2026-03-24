using System;
using Features.Level;
using Infrastructure.Configs;

namespace Infrastructure.Ads
{
    public class InterstitialService
    {
        private readonly AdsConfig adsConfig;
        private readonly LevelService levelService;


        public InterstitialService(ConfigProvider configProvider,
                                   LevelService levelService)
        {
            this.adsConfig = configProvider.AdsConfig;
            this.levelService = levelService;
        }


        public void ShowOnWinLevel(Action callback)
        {
            ShowInterAds(AdsKeys.Inter.LvlComplete, callback);
        }


        private bool IsLevelReached()
        {
            return levelService.CurrentLevelNumber >= adsConfig.AdsData.interMinLvlNumber;
        }


        private void ShowInterAds(string place, Action callback = null)
        {
            if (!IsLevelReached())
            {
                //UnityEngine.Debug.Log($"Interstitial. Level not reached");
                callback?.Invoke();
                return;
            }

            Advertisement.ShowInterstitial(place, callback);
            //UnityEngine.Debug.Log($"Interstitial. Place: {place}");
        }
    }
}