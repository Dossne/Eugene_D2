using Cysharp.Threading.Tasks;
using Features.Level;
using Features.Skin;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.TimeCycles;
using Infrastructure.Utilities;
using System;
using System.Threading;
using UnityEngine;
using VContainer;

namespace Features.SuperDiscountUi
{
    public class SuperDiscountUiController : ISavable
    {
        public Action<SuperDiscountSkinType, double> OnSkinChange;

        private PopupService popupService;
        private IPurchaseManager purchaseManager;
        private LevelService levelService;
        private ConfigProvider configProvider;
        private SpriteAtlasService spriteAtlasService;
        private TimeCyclesService timeCyclesService;
        private PurchaseOfferContainer purchaseOfferContainer;
        private AnalyticsContextCreator analyticsContextCreator;

        private SuperDiscountPopup superDiscountPopup;

        private CancellationTokenSource cancellationTokenSource;
        private OpeningMethod lastOfferOpeningMethod = OpeningMethod.Yourself;
        private string lastOfferSource = "";

        private SuperDiscountState superDiscountState = new();

        private OfferId pendingOfferId = OfferId.none;

        
        public OfferId CurrentInAppOfferId => purchaseOfferContainer.GetFirstActiveOfferOfType(OfferGroup.SuperDiscount, levelService.CurrentLevelNumber);
        public SuperDiscountSkinData CurrentSkinData => configProvider.SkinConfiguration.GetSuperDiscountSkin(CurrentSkin);
        public SuperDiscountSkinType CurrentSkin => superDiscountState.superDiscountSkinType;

        public bool SuperDiscountPopupClosed => !superDiscountPopup.IsOpened;

        [Inject]
        public SuperDiscountUiController(PopupService            popupService,
                                         SpriteAtlasService      spriteAtlasService,
                                         IPurchaseManager        purchaseManager,
                                         PurchaseOfferContainer  purchaseOfferContainer,
                                         ConfigProvider          configProvider,
                                         TimeCyclesService       timeCyclesService,
                                         LevelService            levelService,
                                         AnalyticsContextCreator analyticsContextCreator) 
        { 
            this.popupService = popupService;
            this.spriteAtlasService = spriteAtlasService;
            this.configProvider = configProvider;
            this.purchaseManager = purchaseManager;
            this.levelService = levelService;
            this.timeCyclesService = timeCyclesService;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.analyticsContextCreator = analyticsContextCreator;
        }

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
                return;
            cancellationTokenSource = new CancellationTokenSource();

            purchaseOfferContainer.OnOfferSecondTick += PurchaseOfferContainer_OnOfferSecondTick;
            purchaseOfferContainer.OnOfferCycleReset += PurchaseOfferContainer_OnOfferCycleReset;
            purchaseManager.OnPurchaseCompleted += PurchaseManager_OnPurchaseCompleted;
            InitializePopupAsync().Forget();

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            purchaseManager.OnPurchaseCompleted -= PurchaseManager_OnPurchaseCompleted;
            purchaseOfferContainer.OnOfferSecondTick -= PurchaseOfferContainer_OnOfferSecondTick;
            purchaseOfferContainer.OnOfferCycleReset -= PurchaseOfferContainer_OnOfferCycleReset;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            if (superDiscountPopup != null)
            {
                superDiscountPopup.Deinitialize();
            }

            IsInitialized = false;
        }

        public void Load(Infrastructure.PersistentProgress.Progress progress)
        {
            superDiscountState = progress.superDiscountState;
        }

        public void Save(Infrastructure.PersistentProgress.Progress progress)
        {
            progress.superDiscountState = superDiscountState;
        }

        public void ShowSuperDiscountPopup(OpeningMethod openingMethod, string source)
        {
            var offerId = CurrentInAppOfferId;
            if (offerId == OfferId.none)
                return;

            var ds = purchaseOfferContainer.GetOfferUiDatasource(offerId);
            var skinData = CurrentSkinData;
            superDiscountPopup.RefreshPopup(ds,
                                            spriteAtlasService.GetFromPopupSuperSale(skinData.popupHeaderSpriteId),
                                            spriteAtlasService.GetFromPopupSuperSale(skinData.popupBackgroundSpriteId));
            PurchaseAnalytics.TrackInAppOffer(offerId, openingMethod, source, analyticsContextCreator.MainContext);
            lastOfferSource = source;
            lastOfferOpeningMethod = openingMethod;

            superDiscountPopup.Open();
        }


        private async UniTaskVoid InitializePopupAsync()
        {
            if (superDiscountPopup == null)
            {
                superDiscountPopup = await popupService.GetAsync<SuperDiscountPopup>(cancellationTokenSource.Token, true);
                superDiscountPopup.Construct(spriteAtlasService, configProvider, BuyOffer);
                superDiscountPopup.Initialize();
            }
        }

        private void BuyOffer()
        {
            var offerId = CurrentInAppOfferId;
            if (offerId == OfferId.none)
                return;

            pendingOfferId = offerId;
            purchaseManager.InitiatePurchase(offerId, lastOfferOpeningMethod, lastOfferSource);            
        }

        private void PurchaseOfferContainer_OnOfferCycleReset((OfferId offerId, PurchaseOfferUiDatasource offerUiDatasource, double timeRestSec) offerCycleReset)
        {
            if (offerCycleReset.offerId != CurrentInAppOfferId)
                return;

            superDiscountState.superDiscountSkinType = superDiscountState.superDiscountSkinType == SuperDiscountSkinType.SkinType_2 ? SuperDiscountSkinType.SkinType_1 : SuperDiscountSkinType.SkinType_2;
            OnSkinChange?.Invoke(superDiscountState.superDiscountSkinType, offerCycleReset.timeRestSec);
        }


        private void PurchaseOfferContainer_OnOfferSecondTick((OfferId offerId, double timeRestSec) offerSecondTick)
        {
            if (offerSecondTick.offerId != CurrentInAppOfferId)
                return;

            if (superDiscountPopup != null && superDiscountPopup.IsOpened)
            {
                var time = offerSecondTick.timeRestSec < 86400 ? TimeUtils.GetTimeString(offerSecondTick.timeRestSec) : TimeUtils.GetTimeString(offerSecondTick.timeRestSec, 100f);
                superDiscountPopup.SetRestTime(time);
            }            

            if (superDiscountPopup.IsOpened && offerSecondTick.timeRestSec == 0)
                superDiscountPopup.Close();
        }


        private void PurchaseManager_OnPurchaseCompleted(OfferId id)
        {
            if (id != pendingOfferId)
                return;

            superDiscountPopup.Close();
            pendingOfferId = OfferId.none;
        }
    }
}