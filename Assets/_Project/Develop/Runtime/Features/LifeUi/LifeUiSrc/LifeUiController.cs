using Cysharp.Threading.Tasks;
using Features.Life;
using Infrastructure.Popups;
using System;
using System.Threading;
using Features.Events;
using VContainer;
using Infrastructure.Localization;
using Infrastructure.Utilities;
using Infrastructure.PurchaseSystem;
using Features.WinStreak;
using Features.LevelLoose;
using System.Collections.Generic;
using Features.LavaQuest;
using Features.LevelSessionStateControl;
using Features.SuperSpeedMode;
using Infrastructure.Configs;
using Infrastructure.Ads;

namespace Features.LifeUi
{
    public class LifeUiController
    {
        public Action<string, PurchaseOfferUiType> OnRedirectToShop;

        public Action<bool/*withLoose*/> OnQuitHandled;
        public Action<bool> OnShowCurrencyRequest;

        private readonly LifeController lifeController;
        private readonly PopupService popupService;
        private readonly LavaQuestStateController lavaQuestStateController;
        private readonly LavaQuestEventPopupController lavaQuestEventPopupController;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly LevelSessionStateService levelSessionStateService;
        private readonly SuperSpeedController superSpeedController;
        private readonly LifeUiConfiguration lifeUiConfiguration;
        private QuitLevelPopup quitLevelPopup;
        private LevelLooseManager levelLoseManager;
        private BuyLifePopup buyLifePopup;

        private string fullLifeText = string.Empty;

        private CancellationTokenSource cts;


        [Inject]
        public LifeUiController(LifeController lifeController,
                                PopupService popupService,
                                LevelFinishEvent levelFinishEvent,
                                LavaQuestStateController lavaQuestStateController,
                                LavaQuestEventPopupController lavaQuestEventPopupController,
                                LevelSessionStateService levelSessionStateService,
                                SuperSpeedController superSpeedController,
                                ConfigProvider configProvider)
        {
            this.lifeController = lifeController;
            this.popupService = popupService;
            this.levelFinishEvent = levelFinishEvent;
            this.lavaQuestStateController = lavaQuestStateController;
            this.lavaQuestEventPopupController = lavaQuestEventPopupController;
            this.levelSessionStateService = levelSessionStateService;
            this.superSpeedController = superSpeedController;
            this.lifeUiConfiguration = configProvider.LifeUiConfiguration;
        }


        public bool IsInitialized { get; private set; }


        public void Initialize()
        {
            if (IsInitialized)
                return;

            lifeController.OnTimerUpdate += LifeController_OnTimerUpdate;

            cts = new();

            fullLifeText = LocalizationService.I.Get(LocKeys.Life.FullLife);

            IsInitialized = true;
        }


        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            if (buyLifePopup != null)
            {
                buyLifePopup.OnChangeState -= BuyLifePopup_OnChangeState;
                buyLifePopup.Deinitialize();
            }

            if (quitLevelPopup != null)
                quitLevelPopup.Deinitialize();

            cts.Cancel();
            cts.Dispose();

            lifeController.OnTimerUpdate -= LifeController_OnTimerUpdate;
            lavaQuestEventPopupController.OnClosePopup -= LavaQuestEventPopupController_OnClosePopup;

            IsInitialized = false;
        }

        
        public void RegisterLevelLooseManager(LevelLooseManager levelLooseManager)
        {
            levelLoseManager = levelLooseManager;
        }


        public void UnregisterLevelLooseManager()
        {
            levelLoseManager = null;
        }


        public void ShowQuitLevelPopup()
        {
            ShowQuitLevelPopupAsync(cts.Token).Forget();
        }


        public void ShowBuyLifePopup()
        {
            ShowBuyLifePopupAsync(cts.Token).Forget();
        }


        private async UniTaskVoid ShowQuitLevelPopupAsync(CancellationToken cancellationToken)
        {
            if (quitLevelPopup == null)
            {
                quitLevelPopup = await popupService.GetAsync<QuitLevelPopup>(cancellationToken);
                quitLevelPopup.Construct(QuitHandler);
                quitLevelPopup.Initialize();
            }

            quitLevelPopup.SetState(QuitLevelPopup.State.LoseLife);
            quitLevelPopup.Open();

        }


        private async UniTaskVoid ShowBuyLifePopupAsync(CancellationToken cancellationToken)
        {
            if (buyLifePopup == null)
            {
                buyLifePopup = await popupService.GetAsync<BuyLifePopup>(cancellationToken, true);
                buyLifePopup.Construct(lifeController.LifePrice, 
                                       BuyLifeHandler, 
                                       lifeUiConfiguration.LifeUiData.rewardedLifeEnabled 
                                    && !lifeController.HasMaxLifeAmount()
                                    && lifeController.RewardedLifeAvailable(),
                                       AdsKeys.Rewarded.BuyLife,
                                       OnRewardedAds);
                buyLifePopup.Initialize();
                buyLifePopup.OnChangeState += BuyLifePopup_OnChangeState;
            }

            BuyLifePopupUpdateData(lifeController.GetUpdateData());
            buyLifePopup.Open();
        }


        private void QuitHandler(QuitLevelPopup.State state)
        {
            if (!levelSessionStateService.IsGameLevelInProcess())
            {
                OnQuitHandled?.Invoke(false);
                return;
            }

            List<LostItemData> lostItems = new();
            lostItems = levelLoseManager?.GetLostItems();

            if (state == QuitLevelPopup.State.LoseLife && lostItems.Exists(x => x.lostItemType != LostItemType.Lives))
            {
                quitLevelPopup.SetState(QuitLevelPopup.State.LoseItems);
                quitLevelPopup.SetLostItems(lostItems);
                return;
            }

            lifeController.LoseLife();
            lavaQuestStateController.ScheduleActionLoose();
            levelFinishEvent.ExecuteWithFail(LevelComplete.LoseReason.LevelExit);
            superSpeedController.HandleLevelFinish(false);

            if (lavaQuestEventPopupController.TryOpenPopupOnScheduledAction())
            {
                lavaQuestEventPopupController.OnClosePopup += LavaQuestEventPopupController_OnClosePopup;
            }
            else
            {
                OnQuitHandled?.Invoke(true);
            }
        }


        private void BuyLifeHandler()
        {
            if (!lifeController.TryBuyLife())
                OnRedirectToShop?.Invoke(ShopOpenSource.BuyLifePopup, PurchaseOfferUiType.BestDealHeader);
            else
                buyLifePopup.Close();
        }


        private void BuyLifePopupUpdateData((bool isLifeFull, int timeRestSec) updateData)
        {
            buyLifePopup.UpdateData(lifeController.LifeAmount,
                                    updateData.isLifeFull
                                        ? fullLifeText
                                        : TimeUtils.GetTimeString(updateData.timeRestSec),
                                    lifeUiConfiguration.LifeUiData.rewardedLifeEnabled
                                 && !lifeController.HasMaxLifeAmount()
                                 && lifeController.RewardedLifeAvailable());
        }


        private void BuyLifePopup_OnChangeState(PopupBase popup, PopupState state)
        {
            switch (state)
            {
                case PopupState.BeginOpen:
                    OnShowCurrencyRequest?.Invoke(true);
                    break;
                case PopupState.BeginClose:
                    OnShowCurrencyRequest?.Invoke(false);
                    break;
            }
        }
        
        
        private void LifeController_OnTimerUpdate((bool isLifeFull, int timeRestSec) updateData)
        {
            if (buyLifePopup == null)
                return;

            if (!buyLifePopup.IsOpened)
                return;

            BuyLifePopupUpdateData(updateData);
        }


        private void LavaQuestEventPopupController_OnClosePopup()
        {
            lavaQuestEventPopupController.OnClosePopup -= LavaQuestEventPopupController_OnClosePopup;
            OnQuitHandled?.Invoke(true);
        }


        private void OnRewardedAds()
        {
            lifeController.AddRewardedLife();
            buyLifePopup.Close();
        }
    }
}