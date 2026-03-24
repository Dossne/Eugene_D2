using Cysharp.Threading.Tasks;
using Features.Banner;
using Features.FreePaidOffer;
using Features.Level;
using Features.NoAdsUi;
using Features.RateUs;
using Features.SuperDiscountUi;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using System;
using System.Threading;
using VContainer;


namespace Features.RecommendationUi
{
    public class RecommendationUiManager : ISavable
    {
        private InputUiService inputUiService;
        private PopupService popupService;
        private NoAdsUiController noAdsUiController;

        private NoAdsRecommendationPopup noAdsRecommendationPopup;

        private RateUsController rateUsController;
        private BannerController bannerController;

        private SuperDiscountUiController superDiscountUiController;
        private FreePaidOfferUiController freePaidOfferUiController;

        private RecommendationState recommendationState = new();

        

        private bool noAdsShownThisSession = false;
        private bool superDiscountShownThisSession = false;
        private bool freePaidOfferShownThisSession = false;

        [Inject]
        public RecommendationUiManager(InputUiService inputUiService,
                                       PopupService popupService, 
                                       LevelService levelService, 
                                       ConfigProvider configProvider,
                                       NoAdsUiController noAdsUiController,
                                       RateUsController rateUsController,
                                       BannerController bannerController,
                                       SuperDiscountUiController superDiscountUiController,
                                       FreePaidOfferUiController freePaidOfferUiController)
        {
            this.inputUiService = inputUiService;
            this.popupService = popupService;
            this.noAdsUiController = noAdsUiController;
            this.rateUsController = rateUsController;
            this.bannerController = bannerController;
            this.superDiscountUiController = superDiscountUiController;
            this.freePaidOfferUiController = freePaidOfferUiController;
        }

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            noAdsRecommendationPopup = await popupService.GetAsync<NoAdsRecommendationPopup>(cancellationToken, true);
            noAdsRecommendationPopup.Initialize();

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            noAdsRecommendationPopup.Deinitialize();

            IsInitialized = false;
        }

        public async UniTask ExecuteScheduledAsync(CancellationToken cancellationToken)
        {
            try
            {
                await RateUsRecommendation(cancellationToken);
                await NoAdsRecommendation(cancellationToken);
                await SuperDiscountRecommendation(cancellationToken);
                await FreePaidOfferRecommendation(cancellationToken);
                //require await for all related popups close in recommendation method
                //https://www.notion.so/whalergames/2abb9933364c803199c3d2369b284c2f
                //Disable-enable input only for related popups open waiting in recommendation method
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                inputUiService.EnableInput();
            }
        }

        public void Load(Infrastructure.PersistentProgress.Progress progress)
        {
            recommendationState = progress.recommendationState;
        }

        public void Save(Infrastructure.PersistentProgress.Progress progress)
        {
            progress.recommendationState = recommendationState;
        }

        private async UniTask<bool> NoAdsRecommendation(CancellationToken cancellationToken) 
        {
            if (noAdsUiController.NoAdsShowBlocked)
                return false;

            await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
            inputUiService.DisableInput();
            await UniTask.WaitForSeconds(0.4f, true, cancellationToken: cancellationToken);

            if (recommendationState.noAdsRecommendationShown)
            {
                if (noAdsShownThisSession)
                    return false;

                
                noAdsUiController?.ShowPurchasePopup(OpeningMethod.Forse, PurchaseOfferShowSource.Recommendation, inputUiService.EnableInput);
                noAdsShownThisSession = true;

                inputUiService.EnableInput();
                await UniTask.WaitUntil(() => noAdsUiController.PurchasePopupClosed, cancellationToken: cancellationToken);
            }
            else
            {
                noAdsRecommendationPopup.Open();
                recommendationState.noAdsRecommendationShown = true;

                inputUiService.EnableInput();
                await UniTask.WaitUntil(() => !noAdsRecommendationPopup.IsOpened 
                                           && noAdsUiController.PurchasePopupClosed, cancellationToken: cancellationToken);
            }
            
            return true;
        }

        private async UniTask<bool> RateUsRecommendation(CancellationToken cancellationToken)
        {
            if (rateUsController.RateUsBlocked)
                return false;

            await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
            inputUiService.DisableInput();
            await UniTask.WaitForSeconds(0.4f, true, cancellationToken: cancellationToken);

            rateUsController.ShowRateUsPopup();
            inputUiService.EnableInput();
            bannerController.HideBanner();

            return true;
        }

        private async UniTask<bool> SuperDiscountRecommendation(CancellationToken cancellationToken)
        {
            if (superDiscountUiController.CurrentInAppOfferId == OfferId.none)
                return false;

            if (superDiscountShownThisSession)
                return false;
            await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
            inputUiService.DisableInput();
            await UniTask.WaitForSeconds(0.4f, true, cancellationToken: cancellationToken);

            superDiscountUiController.ShowSuperDiscountPopup(OpeningMethod.Forse, PurchaseOfferShowSource.Recommendation);
            superDiscountShownThisSession = true;
            inputUiService.EnableInput();
            await UniTask.WaitUntil(() => superDiscountUiController.SuperDiscountPopupClosed, cancellationToken: cancellationToken);

            return true;
        }

        private async UniTask<bool> FreePaidOfferRecommendation(CancellationToken cancellationToken)
        {
            if (!freePaidOfferUiController.IsOfferActive())
                return false;

            if (freePaidOfferShownThisSession)
                return false;
            await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
            inputUiService.DisableInput();
            await UniTask.WaitForSeconds(0.4f, true, cancellationToken: cancellationToken);

            freePaidOfferUiController.ShowFreePaidOfferPopup(OpeningMethod.Forse, PurchaseOfferShowSource.Recommendation);
            freePaidOfferShownThisSession = true;
            inputUiService.EnableInput();
            await UniTask.WaitUntil(() => freePaidOfferUiController.FreePaidOfferPopupClosed 
                                       && freePaidOfferUiController.FreePaidOfferBoughtPopupClosed, cancellationToken: cancellationToken);
            return true;
        }
    }
}