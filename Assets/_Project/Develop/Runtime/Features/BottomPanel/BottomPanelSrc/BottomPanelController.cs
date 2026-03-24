using VContainer;

using Infrastructure.Configs;
using System;
using Features.ShopUi;
using Infrastructure.PurchaseSystem;
using Infrastructure.MainUICanvasControl;
using System.Collections.Generic;
using Infrastructure.Popups;
using Features.Boosters;
using Features.Team;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Ads;


namespace Features.BottomPanel
{
    public class BottomPanelController
    {
        private const string BUTTON_PLAY = "button_play";
        private const string BUTTON_SHOP = "button_shop";
        private const string BUTTON_TEAM = "button_team";
        private readonly PopupService popupService;
        private readonly ConfigProvider configProvider;
        private readonly ShopUiController shopUiController;
        private readonly TeamUiController teamUiController;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly PurchaseOfferContainer purchaseOfferContainer;
        private readonly BottomPanelBase bottomPanel;
        private readonly Dictionary<string, Action> actionMap = new();

        [Inject]
        public BottomPanelController(PopupService popupService,
                                     ConfigProvider configProvider, 
                                     MainUIProvider mainUIProvider, 
                                     ShopUiController shopUiController,
                                     TeamUiController teamUiController,
                                     SpriteAtlasService spriteAtlasService,
                                     PurchaseOfferContainer purchaseOfferContainer) 
        {
            this.popupService = popupService;
            this.configProvider = configProvider;
            this.shopUiController = shopUiController;
            this.teamUiController = teamUiController;
            this.spriteAtlasService = spriteAtlasService;
            this.purchaseOfferContainer = purchaseOfferContainer;

            bottomPanel = mainUIProvider.BottomPanel;
            actionMap.Add(BUTTON_PLAY, GoToPlay);
            actionMap.Add(BUTTON_SHOP, GoToShop);
            actionMap.Add(BUTTON_TEAM, GoToTeam);
        }

        private bool IsInitialized { get; set; }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            bottomPanel.SetObjectActive(configProvider.BottomPanelConfiguration.Feature.isEnabled);
            bottomPanel.Clear();

            var buttonData = configProvider.BottomPanelConfiguration.Buttons;

            for (int i = 0; i < buttonData.Count; i++)
            {
                actionMap.TryGetValue(buttonData[i].id, out Action callback);
                bottomPanel.CreateButton(buttonData[i], spriteAtlasService.GetFromMain(buttonData[i].iconName), callback);
            }

            GoToPlay();

            popupService.OnChangeState += PopupService_OnChangeState;
            UpdateNotifiers();

            purchaseOfferContainer.OnPurchaseCompleted += PurchaseOfferContainer_OnPurchaseCompleted;
            purchaseOfferContainer.OnOfferCycleReset += PurchaseOfferContainer_OnOfferCycleReset;
            IsInitialized = true;
        }


        public void Deinitilize()
        {
            if (!IsInitialized)
                return;

            purchaseOfferContainer.OnPurchaseCompleted -= PurchaseOfferContainer_OnPurchaseCompleted;
            purchaseOfferContainer.OnOfferCycleReset   -= PurchaseOfferContainer_OnOfferCycleReset;

            popupService.OnChangeState -= PopupService_OnChangeState;

            bottomPanel.Clear();
            bottomPanel.SetObjectActive(false);
            IsInitialized = false;
        }

        private void GoToShop()
        {
            bottomPanel.ToggleButton(BUTTON_SHOP);
            shopUiController.ShowShopPopup(ShopPopup.ShopMode.Screen, OpeningMethod.Yourself, ShopOpenSource.BottomPanel).Forget();
            teamUiController.CloseTeamPopup();
            BottomPanelAnalytics.SendShopOpened();
        }

        private void GoToPlay()
        {
            bottomPanel.ToggleButton(BUTTON_PLAY);
            shopUiController.CloseShopPopup();
            teamUiController.CloseTeamPopup();
            BottomPanelAnalytics.SendMainOpened();
        }

        private void GoToTeam()
        {
            bottomPanel.ToggleButton(BUTTON_TEAM);
            teamUiController.ShowTeamPopup();
            shopUiController.CloseShopPopup();
            BottomPanelAnalytics.SendTeamOpened();
        }


        private void UpdateNotifiers()
        {
            bottomPanel.SetButtonNotifierActive(BUTTON_SHOP, purchaseOfferContainer.HasAvailableRewarded());
        }

        private void PopupService_OnChangeState(PopupBase popup, PopupState state)
        {
            if (popup is ShopScreen) 
            {
                if (state == PopupState.BeginOpen && !bottomPanel.IsToggled(BUTTON_SHOP))
                    bottomPanel.ToggleButton(BUTTON_SHOP);

                if (state == PopupState.BeginClose && bottomPanel.IsToggled(BUTTON_SHOP))
                    bottomPanel.ToggleButton(BUTTON_PLAY);
            }

            if (popup is TeamScreen)
            {
                if (state == PopupState.BeginOpen && !bottomPanel.IsToggled(BUTTON_TEAM))
                    bottomPanel.ToggleButton(BUTTON_TEAM);

                if (state == PopupState.BeginClose && bottomPanel.IsToggled(BUTTON_TEAM))
                    bottomPanel.ToggleButton(BUTTON_PLAY);
            }

            if (popup is PreBoostersPopup)
            {
                if (state == PopupState.BeginOpen)
                    GoToPlay();
            }
        }

        private void PurchaseOfferContainer_OnOfferCycleReset((OfferId offerId, PurchaseOfferUiDatasource offerUiDatasource, double timeRestSec) onResetTick)
        {
            UpdateNotifiers();
        }

        private void PurchaseOfferContainer_OnPurchaseCompleted(OfferId id, bool isFirst)
        {
            UpdateNotifiers();
        }
    }
}