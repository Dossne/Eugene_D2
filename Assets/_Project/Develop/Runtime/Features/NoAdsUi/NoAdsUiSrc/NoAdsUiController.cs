using Cysharp.Threading.Tasks;
using Features.Level;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SpriteAtlasControl;
using System;
using System.Threading;
using VContainer;

namespace Features.NoAdsUi
{
    public class NoAdsUiController
    {
        private const OfferId inAppOfferId = OfferId.rh_no_ads_plus_v2;
        private SpriteAtlasService spriteAtlasService;
        private PopupService popupService;
        private IPurchaseManager purchaseManager;
        private ConfigProvider configProvider;
        private PurchaseOfferContainer purchaseOfferContainer;

        private LevelService levelService;
        private InAppConfig inAppConfig;
        private InputUiService inputUiService;
        private AnalyticsContextCreator analyticsContextCreator;

        private NoAdsPurchasePopup noAdsPurchasePopup;

        private OpeningMethod lastNoAdsOpeningMethod = OpeningMethod.Yourself;
        private string lastNoAdsSource = "";

        public bool NoAdsShowBlocked => Advertisement.IsPremium() 
                                     || inAppConfig.GetOfferData(OfferId.rh_no_ads_plus_v2).unlockLevel > levelService.CurrentLevelNumber;
        public bool PurchasePopupClosed => !noAdsPurchasePopup.IsOpened;


        [Inject]
        public NoAdsUiController(SpriteAtlasService spriteAtlasService,
                                 ConfigProvider configProvider,
                                 PopupService popupService,
                                 LevelService levelService,
                                 IPurchaseManager purchaseManager,
                                 PurchaseOfferContainer purchaseOfferContainer,
                                 InputUiService inputUiService,
                                 AnalyticsContextCreator analyticsContextCreator)
        {
            this.spriteAtlasService = spriteAtlasService;
            this.popupService = popupService;
            this.purchaseManager = purchaseManager;
            this.configProvider = configProvider;
            this.levelService = levelService;
            this.inputUiService = inputUiService;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.analyticsContextCreator = analyticsContextCreator;
            inAppConfig = configProvider.InAppConfig;
        }

        public bool IsInitialized { get; private set; } = false;

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            noAdsPurchasePopup = await popupService.GetAsync<NoAdsPurchasePopup>(cancellationToken);
            noAdsPurchasePopup.Construct(spriteAtlasService, configProvider, BuyOffer);
            noAdsPurchasePopup.Initialize();

            purchaseOfferContainer.OnPurchaseCompleted += PurchaseOfferContainer_OnPurchaseCompleted;

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            noAdsPurchasePopup.Deinitialize();

            purchaseOfferContainer.OnPurchaseCompleted -= PurchaseOfferContainer_OnPurchaseCompleted;            

            IsInitialized = false;
        }

        public void ShowPurchasePopup(OpeningMethod openingMethod, string source, Action onAnimationStartCallback = null)
        {
            var ds = purchaseOfferContainer.GetOfferUiDatasource(inAppOfferId);

            noAdsPurchasePopup.SetAnimationStartCallback(onAnimationStartCallback);
            noAdsPurchasePopup.RefreshPopup(ds);
            noAdsPurchasePopup.Open();
            PurchaseAnalytics.TrackInAppOffer(inAppOfferId, openingMethod, source, analyticsContextCreator.MainContext);
            lastNoAdsSource = source;
            lastNoAdsOpeningMethod = openingMethod;
        }

        private void BuyOffer() 
        {
            purchaseManager.InitiatePurchase(inAppOfferId, lastNoAdsOpeningMethod, lastNoAdsSource);
        }

        private void PurchaseOfferContainer_OnPurchaseCompleted(OfferId id, bool arg2)
        {
            if (id != inAppOfferId)
                return;

            noAdsPurchasePopup.Close();
            //shopRewardPopup.Open();
        }
    }
}