using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.Level;
using Features.LifeUi;
using Features.PurchaseUi;
using Infrastructure.Configs;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using System.Threading;
using Infrastructure.Reward;
using VContainer;
using Infrastructure.Ads;
using Infrastructure.SceneManagement;
using UnityEngine;


namespace Features.ShopUi
{
    public class ShopUiController
    {
        private IPurchaseManager purchaseManager;
        private PopupService popupService;
        private SpriteAtlasService spriteAtlasService;
        private LevelService levelService;
        private BoostersManager boostersManager;
        private LifeUiController lifeUiController;
        private PurchaseOfferContainer purchaseOfferContainer;
        private AnalyticsContextCreator analyticsContextCreator;
        private SceneLoadController sceneLoadController;
        private ShopPopup shopPopup;
        private ShopScreen shopScreen;

        private ShopPopup currentPopup; 

        private ShopRewardPopup shopRewardPopup;
        private List<PurchaseOfferUiDatasource> purchaseOfferUiDatasourceList;
        private List<BoosterData> boosterConfig = new();

        private string lastShopOpenSource = "";
        

        [Inject]
        public ShopUiController(SpriteAtlasService      spriteAtlasService,
                                ConfigProvider          configProvider,
                                IPurchaseManager        purchaseManager,
                                PopupService            popupService, 
                                LevelService            levelService,
                                BoostersManager         boostersManager,
                                LifeUiController        lifeUiController,
                                PurchaseOfferContainer  purchaseOfferContainer,
                                AnalyticsContextCreator analyticsContextCreator,
                                SceneLoadController     sceneLoadController) 
        {
            this.purchaseManager = purchaseManager;
            this.popupService = popupService;   
            this.spriteAtlasService = spriteAtlasService;
            this.levelService = levelService;
            this.boostersManager = boostersManager;
            this.lifeUiController = lifeUiController;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.analyticsContextCreator = analyticsContextCreator;
            this.sceneLoadController = sceneLoadController;

            boosterConfig.Clear();
            boosterConfig.AddRange(configProvider.BoosterConfig.PreBoosters);
            boosterConfig.AddRange(configProvider.BoosterConfig.InGameBoosters);
        }

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            await InitializeShopPopup(cancellationToken);
            await InitializeShopScreen(cancellationToken);

            shopRewardPopup = await popupService.GetAsync<ShopRewardPopup>(cancellationToken);
            shopRewardPopup.Construct(ShopRewardPopup_OnCollectReward, Infrastructure.Localization.LocKeys.Purchase.SuccessPurchase);
            shopRewardPopup.Initialize();

            purchaseOfferContainer.OnPurchaseCompleted += PurchaseOfferContainer_OnPurchaseCompleted;

            boostersManager.OnRedirectToShop  += HandleRedirectToShop;
            lifeUiController.OnRedirectToShop += HandleRedirectToShop;

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            boostersManager.OnRedirectToShop  -= HandleRedirectToShop;
            lifeUiController.OnRedirectToShop -= HandleRedirectToShop;

            shopRewardPopup.Deinitialize();

            purchaseOfferContainer.OnPurchaseCompleted -= PurchaseOfferContainer_OnPurchaseCompleted;

            DeinitializeShopPopup();
            DeinitializeShopScreen();

            IsInitialized = false;
        }

        public async UniTaskVoid ShowShopPopup(ShopPopup.ShopMode mode, OpeningMethod openingMethod, string source, PurchaseOfferUiType purchaseOfferType = PurchaseOfferUiType.Empty)
        {
            CloseShopPopup();

            currentPopup = sceneLoadController.IsMetaSceneActive && mode == ShopPopup.ShopMode.Screen ? shopScreen : shopPopup;

            purchaseOfferUiDatasourceList = purchaseOfferContainer.GetOfferUiDatasources(levelService.CurrentLevelNumber);
            await currentPopup.UpdateOfferList(purchaseOfferUiDatasourceList);
            currentPopup.ScrollToOfferType(purchaseOfferType);
            currentPopup.Open();

            PurchaseAnalytics.TrackShopOpen(openingMethod, source, analyticsContextCreator.MainContext);
            lastShopOpenSource = source;
        }

        public void CloseShopPopup()
        {         
            if (currentPopup != null)
                currentPopup.Close();
        }

        private async UniTask InitializeShopPopup(CancellationToken cancellationToken)
        {
            shopPopup = await popupService.GetAsync<ShopPopup>(cancellationToken, true, popupMode: PopupService.PopupMode.Popup);
            shopPopup.Initialize();
            shopPopup.OnOfferBuyAttempt += ShopPopup_OnOfferBuyAttempt;
            shopPopup.OnOfferShow += ShopPopup_OnOfferShow;
            purchaseOfferUiDatasourceList = purchaseOfferContainer.GetOfferUiDatasources(levelService.CurrentLevelNumber);
            await shopPopup.UpdateOfferList(purchaseOfferUiDatasourceList);
            shopPopup.gameObject.SetActive(true);
            await UniTask.WaitUntil(() => shopPopup.gameObject.activeInHierarchy);            
            shopPopup.gameObject.SetActive(false);
        }

        private void DeinitializeShopPopup()
        {
            shopPopup.OnOfferShow -= ShopPopup_OnOfferShow;
            shopPopup.OnOfferBuyAttempt -= ShopPopup_OnOfferBuyAttempt;
            shopPopup.Deinitialize();
        }

        private async UniTask InitializeShopScreen(CancellationToken cancellationToken)
        {
            shopScreen = await popupService.GetAsync<ShopScreen>(cancellationToken, true, popupMode: PopupService.PopupMode.Screen);
            shopScreen.Initialize();
            shopScreen.OnOfferBuyAttempt += ShopPopup_OnOfferBuyAttempt;
            shopScreen.OnOfferShow += ShopPopup_OnOfferShow;
            purchaseOfferUiDatasourceList = purchaseOfferContainer.GetOfferUiDatasources(levelService.CurrentLevelNumber);
            await shopScreen.UpdateOfferList(purchaseOfferUiDatasourceList);
            shopScreen.gameObject.SetActive(true);
            await UniTask.WaitUntil(() => shopScreen.gameObject.activeInHierarchy);
            shopScreen.gameObject.SetActive(false);
        }

        private void DeinitializeShopScreen()
        {
            shopScreen.OnOfferShow -= ShopPopup_OnOfferShow;
            shopScreen.OnOfferBuyAttempt -= ShopPopup_OnOfferBuyAttempt;
            shopScreen.Deinitialize();
        }

        private void ShopPopup_OnOfferBuyAttempt(OfferId id)
        {
            if (purchaseOfferContainer.IsOfferLimitReached(id))
            {
                Debug.Log($"Limited offer {id} reached cycle limit. Purchase initiation aborted.");
                return;
            }
                
            purchaseManager.InitiatePurchase(id, OpeningMethod.Yourself, lastShopOpenSource);
        }

        private void ShopPopup_OnOfferShow(OfferId id)
        {
            PurchaseAnalytics.TrackInAppOffer(id, OpeningMethod.Yourself, lastShopOpenSource, analyticsContextCreator.MainContext);
        }

        private void PurchaseOfferContainer_OnPurchaseCompleted(OfferId id, bool arg2)
        {
            CloseShopPopup();
            var rewardDs = purchaseOfferContainer.GetOfferUiDatasource(id);
            if (rewardDs == null
                || rewardDs.offerGroup == OfferGroup.FreePaid)
                return;

            RewardItemVisualData coins = null;
            if (rewardDs.complexReward.currencyOfferRewards.Exists(x => x.currencyType == CurrencyType.Coins))
                coins = new()
                {
                    icon = spriteAtlasService.GetFromMain("shop_coins_icon"),
                    amountText = Utils.GetSpaceSeparatedNumberString(rewardDs.CurrencyRewardAmount(CurrencyType.Coins)).ToString()
                };

            RewardItemVisualData noAds = null;
            if (rewardDs.complexReward.noAdsIncluded)
                noAds = new()
                {
                    icon = spriteAtlasService.GetFromMain("No_Ads_Button"),
                    amountText = ""
                };
            
            List<RewardItemVisualData> boosters = new List<RewardItemVisualData>();
            var boosterRewards = rewardDs.complexReward.boosterRewards;
            for (int i = 0; i < boosterRewards.Count; i++)
            {
                boosters.Add(new()
                {
                    icon = spriteAtlasService.GetFromMain(GetBoosterData(boosterRewards[i].boosterType).iconName),
                    amountText = rewardDs.BoosterRewardAmount(boosterRewards[i].boosterType).ToString()
                });
            }

            List<RewardItemVisualData> preBoosters = new List<RewardItemVisualData>();
            var preBoosterRewards = rewardDs.complexReward.preBoosterRewards;
            for (int i = 0; i < preBoosterRewards.Count; i++)
            {
                preBoosters.Add(new()
                {
                    icon = spriteAtlasService.GetFromMain(GetBoosterData(preBoosterRewards[i].boosterType).iconName),
                    amountText = rewardDs.PreBoosterRewardAmount(preBoosterRewards[i].boosterType).ToString()
                });
            }

            List<RewardItemVisualData> infinitePreBoosters = new List<RewardItemVisualData>();
            var infPreBoosterRewards = rewardDs.complexReward.infinitePreBoosterRewards;
            for (int i = 0; i < infPreBoosterRewards.Count; i++)
            {
                InfiniteBoosterReward infPreBooster = infPreBoosterRewards[i];
                infinitePreBoosters.Add(new()
                {
                    icon = spriteAtlasService.GetFromMain(GetBoosterData(infPreBoosterRewards[i].boosterType).infiniteIconName),
                    amountText = $"{infPreBooster.timeLengthMinutes / 60f}{TimeUtils.hour}",
                    isDisplayRibbon = false,
                    isDisplayInfinityIcon = false
                });
            }

            RewardItemVisualData infiniteLifeTime = null;
            InfiniteLifeReward infiniteLifeReward = rewardDs.complexReward.infiniteLifeReward;
            
            if (infiniteLifeReward.timeLengthMinutes > 0)
                infiniteLifeTime = new()
                {
                    icon = spriteAtlasService.GetFromMain(infiniteLifeReward.IconName),
                    amountText = infiniteLifeReward.timeLengthMinutes >= 60 ? $"{infiniteLifeReward.timeLengthMinutes / 60f}{TimeUtils.hour}" 
                                                                      : $"{infiniteLifeReward.timeLengthMinutes}{TimeUtils.minute}"
                };

            shopRewardPopup.SetRewards(coins, noAds, boosters, preBoosters, infinitePreBoosters, infiniteLifeTime);
            shopRewardPopup.Open();
        }

        private BoosterData GetBoosterData(BoosterType boosterType) 
        {
            return boosterConfig.Find(x => x.type == boosterType);
        }

        private void ShopRewardPopup_OnCollectReward() 
        {
            shopRewardPopup.Close();            
        }

        private void HandleRedirectToShop(string source, PurchaseOfferUiType purchaseOfferType)
        {
            ShowShopPopup(ShopPopup.ShopMode.Popup, OpeningMethod.Forse, source, purchaseOfferType: purchaseOfferType).Forget();
        }
    }
}