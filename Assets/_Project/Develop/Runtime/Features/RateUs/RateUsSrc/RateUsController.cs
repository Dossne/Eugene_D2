using Features.Level;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using VContainer;

namespace Features.RateUs
{
    public class RateUsController : ISavable
    {
        private readonly LevelService levelService;
        private readonly AdsConfig adsConfig;
        private bool rateUsShown;


        public bool RateUsBlocked => rateUsShown 
                                  || Advertisement.IsRateAppPopupShown()
                                  || levelService.CurrentLevelNumber < adsConfig.AdsData.rateUsMinLvlNumber;
        

        [Inject]
        public RateUsController(LevelService levelService,  ConfigProvider configProvider) 
        {
            adsConfig = configProvider.AdsConfig;
            this.levelService = levelService;
        }

        public void Load(Progress progress)
        {
            rateUsShown = progress.appState.rateUsShown;
        }

        public void Save(Progress progress)
        {
            progress.appState.rateUsShown = rateUsShown;
        }

        public void ShowRateUsPopup()
        {
            if (!RateUsBlocked)
                Advertisement.ShowRateAppPopup();
        }
    }
}