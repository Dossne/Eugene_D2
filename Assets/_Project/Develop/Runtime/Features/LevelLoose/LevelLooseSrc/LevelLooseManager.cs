using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.Events;
using Features.LevelComplete;
using Features.LevelConfiguration;
using Features.LevelTime;
using Infrastructure.WalletSystem;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using R3;
using UnityEngine;
using Features.Boosters;
using Features.Life;
using Features.Level;
using System;
using Features.ShopUi;
using Infrastructure.PurchaseSystem;
using System.Linq;
using Features.WinStreak;
using Infrastructure.Localization;
using Features.SuperDiscountUi;
using Features.Skin;
using Features.RewardTrack;
using Features.LavaQuest;
using Features.LevelSessionStateControl;
using Features.LifeUi;
using Infrastructure.AudioControl;
using Features.FeatureUnlock;
using Features.SuperSpeedMode;
using Features.LevelTasks;
using Features.Competition;
using Infrastructure.AssetManagement;

namespace Features.LevelLoose
{
    public class LevelLooseManager : ILevelSessionSavable
    {
        public event Action OnResurrectPending;
        public event Action OnResurrectComplete;
        
        private Action OnRedirectToShop;

        private readonly PopupService popupService;
        private readonly InputService inputService;
        private readonly ResurrectConfig resurrectConfig;
        private readonly DeathEvent deathEvent;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly CollectItemEvent collectItemEvent;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly LevelTimeManager levelTimeManager;
        private readonly Wallet wallet;
        private readonly ShopUiController shopUiController;

        private readonly LifeController lifeController;
        private readonly LifeUiController lifeUiController;
        private readonly LevelService levelService;
        private readonly PreBoosterInGamePopupController preBoosterInGamePopupController;

        private readonly LevelCompleteManager levelCompleteManager;
        private readonly WinStreakStateController winStreakStateController;

        private readonly PurchaseOfferContainer purchaseOfferContainer;

        private readonly RewardTrackCollectController rewardTrackCollectController;
        private readonly RewardTrackStateController rewardTrackStateController;
        private readonly LavaQuestStateController lavaQuestStateController;
        private readonly LavaQuestEventPopupController lavaQuestEventPopupController;
        private readonly SuperSpeedController superSpeedController;
        private readonly LevelTaskManager levelTaskManager;
        private readonly CompetitionManager competitionManager;
        private readonly Instantiator instantiator;
        private readonly IPurchaseManager purchaseManager;
        private readonly SuperDiscountUiController superDiscountUiController;
        private readonly ConfigProvider configProvider;

        private readonly LostItemSettings lostItemSettings;
        private readonly FeatureUnlockConfiguration featureUnlockConfiguration;
        private readonly CompositeDisposable disposables;
        private readonly CancellationTokenSource cts;
        private ResurrectPopup resurrectPopup;
        private FirstBombResurrectPopup firstBombResurrectPopup;
        private ResurrectData currentResurrectData;
        private LoseReason currentReason;
        private int loseCounter = 1;
        
        private LoseReason prevSessionLoseReason;
        private int prevSessionLoseCounter;
        private bool isRestoreSession;
        
        private bool isInit;


        public LevelLooseManager(
            DeathEvent deathEvent,
            LevelFinishEvent levelFinishEvent,
            CollectItemEvent collectItemEvent,
            PopupService popupService,
            InputService inputService,
            ConfigProvider configProvider,
            SpriteAtlasService spriteAtlasService,
            LevelTimeManager levelTimeManager,
            Wallet wallet,
            LifeController lifeController,
            LifeUiController lifeUiController,
            LevelService levelService,
            LevelCompleteManager levelCompleteManager,
            PreBoosterInGamePopupController preBoosterInGamePopupController,
            ShopUiController shopUiController,
            WinStreakStateController winStreakStateController,
            IPurchaseManager purchaseManager,
            SuperDiscountUiController superDiscountUiController,
            PurchaseOfferContainer purchaseOfferContainer,
            RewardTrackCollectController rewardTrackCollectController,
            RewardTrackStateController rewardTrackStateController,
            LavaQuestStateController lavaQuestStateController, 
            LavaQuestEventPopupController lavaQuestEventPopupController,
            SuperSpeedController superSpeedController,
            LevelTaskManager levelTaskManager,
            CompetitionManager competitionManager,
            Instantiator instantiator)
        {
            this.deathEvent = deathEvent;
            this.levelFinishEvent = levelFinishEvent;
            this.collectItemEvent = collectItemEvent;
            this.popupService = popupService;
            this.inputService = inputService;
            this.resurrectConfig = configProvider.ResurrectConfig;
            this.spriteAtlasService = spriteAtlasService;
            this.levelTimeManager = levelTimeManager;
            this.wallet = wallet;
            this.lifeController = lifeController;
            this.lifeUiController = lifeUiController;
            this.levelService = levelService;
            this.preBoosterInGamePopupController = preBoosterInGamePopupController;
            this.levelCompleteManager = levelCompleteManager;
            this.shopUiController = shopUiController;
            this.winStreakStateController = winStreakStateController;
            this.purchaseManager = purchaseManager;
            this.superDiscountUiController = superDiscountUiController;
            this.configProvider = configProvider;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.rewardTrackCollectController = rewardTrackCollectController;
            this.rewardTrackStateController = rewardTrackStateController;
            this.lavaQuestStateController = lavaQuestStateController;
            this.lavaQuestEventPopupController = lavaQuestEventPopupController;
            this.superSpeedController = superSpeedController;
            this.levelTaskManager = levelTaskManager;
            this.competitionManager = competitionManager;
            this.instantiator = instantiator;

            lostItemSettings = configProvider.LostItemSettings;
            featureUnlockConfiguration = configProvider.FeatureUnlockConfiguration;

            this.disposables = new CompositeDisposable();
            this.cts = new CancellationTokenSource();
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionLoseReason = sessionData.loseReason;
            prevSessionLoseCounter = sessionData.loseCounter;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.loseReason = currentReason;
            sessionData.loseCounter = loseCounter;
        }
        
        
        public void Initialize()
        {
            if (isInit)
                return;

            this.deathEvent.Subscribe(_ =>
            {
                currentReason = deathEvent.reason;
                HandleDeath();

            }).AddTo(disposables);

            this.collectItemEvent.Subscribe(ReactOnDamage).AddTo(disposables);

            lifeUiController.RegisterLevelLooseManager(this);

            purchaseManager.OnPurchaseCompleted += PurchaseManager_OnPurchaseCompleted;

            if (isRestoreSession)
            {
                currentReason = prevSessionLoseReason;
                loseCounter = prevSessionLoseCounter;
            }
            
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            purchaseManager.OnPurchaseCompleted -= PurchaseManager_OnPurchaseCompleted;

            lifeUiController.UnregisterLevelLooseManager();

            lavaQuestEventPopupController.OnClosePopup -= LavaQuestPopupController_OnClosePopup;

            disposables.Dispose();
            isInit = false;
        }

        public List<LostItemData> GetLostItems()
        {
            List<LostItemData> result = new();
            bool lifeLostBlocked = false;
            LevelData levelData = levelService.GetCurrentLevelData();

            //Winstreaks
            bool wsCondition = winStreakStateController.IsFeatureEnabled && winStreakStateController.CurrentLevel > 0;
            AddLostItemToList(result,
                              LostItemType.Winstreaks,
                              winStreakStateController.GetWinStreakData(winStreakStateController.CurrentLevel).icon,
                              LocalizationService.I.Get(LocKeys.ResurrectPopup.LostWinstreak),
                              string.Empty,
                              wsCondition);
            lifeLostBlocked |= wsCondition;

            //Reward track item
            var rewardTrackProgressExists = rewardTrackStateController.TryGetCurrentProgress(out var rewardTrackProgress);
            bool rtCondition = rewardTrackProgressExists && rewardTrackStateController.IsEnabledByConfig() && rewardTrackCollectController.CollectedCount > 0;

            int calcMultiplier = configProvider.LevelCompleteConfig.GetMultiplierByDifficulty(levelData.difficulty) * winStreakStateController.RewardMultiplier;
            int multiplier = calcMultiplier < 1 ? 1 : calcMultiplier;

            AddLostItemToList(result,
                              LostItemType.RewardTrackItem,
                              rewardTrackProgress.targetResurrectIcon,
                              LocalizationService.I.Get(LocKeys.ResurrectPopup.LostRewardTrack),
                              $"{rewardTrackCollectController.CollectedCount * multiplier}",
                              rtCondition);
            lifeLostBlocked |= rtCondition;


            //Lava quest item
            bool lqCondition = lavaQuestStateController.IsFeatureEnabled() && lavaQuestStateController.IsStartedState();
            AddLostItemToList(result,
                              LostItemType.LavaQuest,
                              spriteAtlasService.GetFromMain("widget_lava_icon"),
                              LocalizationService.I.Get(LocKeys.ResurrectPopup.LostLavaQuest),
                              $"{lavaQuestStateController.CurrentStepIdx + 1}/{lavaQuestStateController.MaxStepIdx}",
                              lqCondition);
            lifeLostBlocked |= lqCondition;


            //Coins
            string levelCompleteCoins = levelCompleteManager.LevelCompleteReward(levelData.difficulty).ToString();
            AddLostItemToList(result,
                              LostItemType.Coins,
                              spriteAtlasService.GetCurrencyIcon(CurrencyType.Coins),
                              LocalizationService.I.Get(LocKeys.ResurrectPopup.LostCoins),
                              levelCompleteCoins.ToString());


            //Lives
            AddLostItemToList(result,
                              LostItemType.Lives,
                              spriteAtlasService.GetFromMain("heart_cracked_resurect_icon"),
                              LocalizationService.I.Get(LocKeys.ResurrectPopup.LostLives),
                              1.ToString(),
                              !lifeLostBlocked);

            //SuperSpeed
            AddLostItemToList(result,
                              LostItemType.SuperSpeed,
                              spriteAtlasService.GetFromMain("super_speed_icon"),
                              LocalizationService.I.Get(LocKeys.ResurrectPopup.LostSuperSpeed),
                              string.Empty,
                              superSpeedController.IsSuperSpeedActive);


            //CompetitionWinstreak
            AddLostItemToList(result,
                              LostItemType.CompetitionWinstreak,
                              spriteAtlasService.GetFromMain("aerostat_widget"),
                              LocalizationService.I.Get(LocKeys.ResurrectPopup.LostCompetitionWinstreak),
                              $"X{competitionManager.GetMultiplierState().multiplierCurNum}",
                              competitionManager.IsFeatureEnabled() && competitionManager.GetMultiplierState().multiplierCurNum > 1);

            return result.OrderBy(x => x.priority).ToList();
        }
                
        public void HandleDeath()
        {
            LevelData levelData = levelService.GetCurrentLevelData();
            currentResurrectData = resurrectConfig.GetResurrectData(currentReason, levelData.difficulty, loseCounter);
            AudioService.I.PauseMusic();
            if (currentReason == LoseReason.Bomb && lifeController.BombResurrectAvailable() && featureUnlockConfiguration.firstBombResurrectPopupUnlocked)
            {
                OpenFirstBombResurrectPopupAsync().Forget();
                return;
            }                

            if (currentResurrectData != null)
                OpenResurrectPopupAsync(currentResurrectData).Forget();
            else
                OnDeclineResurrect();
        }

        private void AddLostItemToList(List<LostItemData> list, LostItemType itemType, Sprite icon, string name, string text, bool condition = true)
        {
            if (!condition)
                return;

            var settings = lostItemSettings.GetLostItemSettingData(itemType);
            if (settings == null)
                return;

            list.Add(new(itemType,
                         icon,
                         name,
                         text,
                         settings.needCross,
                         spriteAtlasService.GetFromMain(settings.textBackId),
                         settings.needTextBack,
                         settings.priority));
        }

        private void OnFirstBombResurrect()
        {
            Resurrect(currentResurrectData.coinsResurrectAddedSeconds);            
            lifeController.UseBombResurrect();
            loseCounter--;//Restore resurrect price
            firstBombResurrectPopup.Close();
        }

        private void OnBuyResurrect()
        {
            bool isEnough = wallet.IsEnough(currentResurrectData.currency, currentResurrectData.price);

            if (!isEnough)
            {
                shopUiController.ShowShopPopup(ShopPopup.ShopMode.Popup, OpeningMethod.Forse, ShopOpenSource.ResurrectAttempt, purchaseOfferType: PurchaseOfferUiType.BestDealHeader).Forget(); 
                return;
            }

            wallet.Decrease(currentResurrectData.currency, currentResurrectData.price, Reason.Out.Resurrect);

            Resurrect(currentResurrectData.coinsResurrectAddedSeconds);
            resurrectPopup.Close();
        }


        private void OnDeclineResurrect()
        {
            lavaQuestStateController.ScheduleActionLoose();

            if (lavaQuestEventPopupController.TryOpenPopupOnScheduledAction())
            {
                lavaQuestEventPopupController.OnClosePopup += LavaQuestPopupController_OnClosePopup;
            }
            else
            {
                OpenLoosePopup();
            }
        }


        private void OnRewardedAdsResurrect()
        {
            Resurrect(currentResurrectData.rewardResurrectAddedSeconds);
            lifeController.UseRewardedResurrect();
            resurrectPopup.Close();
        }

        
        private void ConstructResurrectPopup(ResurrectData resurrectData)
        {
            Sprite currencyIcon = spriteAtlasService.GetCurrencyIcon(resurrectData.currency);
            resurrectPopup.Construct(
                currentReason,
                resurrectData.coinsResurrectAddedSeconds.ToString(),
                resurrectData.rewardResurrectAddedSeconds.ToString(),
                currencyIcon,
                resurrectData.price.ToString(),
                OnBuyResurrect,
                OnDeclineResurrect,
                AdsKeys.Rewarded.Resurrect,
                OnRewardedAdsResurrect,
                resurrectData.rewardResurrectAllowed && lifeController.RewardedResurrectAvailable(),
                instantiator,
                spriteAtlasService,
                configProvider,
                superDiscountUiController,
                purchaseOfferContainer);
                        
            resurrectPopup.ResetPage();

            resurrectPopup.SetLostItems(GetLostItems());            
        }

        private async UniTaskVoid OpenFirstBombResurrectPopupAsync()
        {
            firstBombResurrectPopup = await popupService.GetAsync<FirstBombResurrectPopup>(cts.Token);
            firstBombResurrectPopup.Setup(LocalizationService.I.Get(LocKeys.FirstBombResurrectPopup.Header),
                                          LocalizationService.I.Get(LocKeys.FirstBombResurrectPopup.Description),
                                          LocalizationService.I.Get(LocKeys.FirstBombResurrectPopup.Button),
                                          spriteAtlasService.GetFromMain("heart_cracked_resurect_icon"),
                                          OnFirstBombResurrect);
            firstBombResurrectPopup.Initialize();
            firstBombResurrectPopup.Open();
            AudioService.I.PlaySfx(SfxType.ResurrectOpen);
            OnResurrectPending?.Invoke();
        }

        private async UniTaskVoid OpenResurrectPopupAsync(ResurrectData resurrectData)
        {
            resurrectPopup = await popupService.GetAsync<ResurrectPopup>(cts.Token, true, false);
            ConstructResurrectPopup(resurrectData);
            resurrectPopup.Initialize();
            resurrectPopup.OnOfferBuyAttempt -= ShopPopup_OnOfferBuyAttempt;
            resurrectPopup.OnOfferShow       -= ShopPopup_OnOfferShow;
            resurrectPopup.OnOfferBuyAttempt += ShopPopup_OnOfferBuyAttempt;
            resurrectPopup.OnOfferShow       += ShopPopup_OnOfferShow;
            await resurrectPopup.UpdateOfferList(purchaseOfferContainer.GetOfferUiDatasources(levelService.CurrentLevelNumber));
            resurrectPopup.Open();
            AudioService.I.PlaySfx(SfxType.ResurrectOpen);
            OnResurrectPending?.Invoke();
        }


        private void OpenLoosePopup()
        {
            levelFinishEvent.ExecuteWithFail(currentReason);
            superSpeedController.HandleLevelFinish(false);
            lifeController.LoseLife();
            preBoosterInGamePopupController.ShowPopup();
            AudioService.I.PlaySfx(SfxType.LooseOpen);
        }


        private void Resurrect(int addSecOnZero)
        {
            if (currentReason == LoseReason.Timer)
                levelTimeManager.AddTimeOnResurrect(addSecOnZero);

            loseCounter++;
            inputService.EnableInput();            
            OnResurrectComplete?.Invoke();

            levelTaskManager.ExecuteScheduled();

            AudioService.I.UnpauseMusic();
        }

        
        private void ReactOnDamage(CollectableItem item)
        {
            if (item.Group is not ItemGroup.Damage) 
                return;
            
            inputService.DisableInput();
            if(item.CollectableType == CollectableType.Bomb)
                AudioService.I.PlaySfx(SfxType.BombExplode);
        }
        
        private void LavaQuestPopupController_OnClosePopup()
        {
            OpenLoosePopup();
        }

        private void ShopPopup_OnOfferBuyAttempt(OfferId id)
        {
            if (purchaseOfferContainer.IsOfferLimitReached(id))
            {
                Debug.Log($"Limited offer {id} reached cycle limit. Purchase initiation aborted.");
                return;
            }

            purchaseManager.InitiatePurchase(id, OpeningMethod.Forse, PurchaseOfferShowSource.ResurrectPopup);
        }

        private void ShopPopup_OnOfferShow(OfferId id)
        {
            PurchaseAnalytics.TrackInAppOffer(id, OpeningMethod.Forse, PurchaseOfferShowSource.ResurrectPopup, string.Empty);
        }

        private void PurchaseManager_OnPurchaseCompleted(OfferId id)
        {
            if (resurrectPopup == null)
                return;

            resurrectPopup.UpdateOfferList(purchaseOfferContainer.GetOfferUiDatasources(levelService.CurrentLevelNumber)).Forget();
        }
    }
}