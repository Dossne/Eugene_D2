using Features.SuperDiscountUi;
using Features.FeatureUnlock;
using Features.Level;
using Features.NoAdsUi;
using Features.ShopUi;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PurchaseSystem;
using Infrastructure.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Infrastructure.Utilities;
using Features.Skin;
using Cysharp.Threading.Tasks;
using System.Threading;
using Features.BottomPanel;

namespace Features.Widgets
{
    public class WidgetManager
    {
        private readonly Dictionary<WidgetId, Widget> widgetViews = new();
        private readonly Dictionary<WidgetId, WidgetData> configDataCached = new();

        private readonly WidgetsConfig widgetsConfig;
        private readonly BottomPanelConfiguration bottomPanelConfig;
        private readonly WidgetUIRoots uiRoots;
        private readonly WidgetFactory widgetFactory;
        private readonly ShopUiController shopUiController;
        private readonly NoAdsUiController noAdsUiController;
        private readonly SuperDiscountUiController superDiscountUiController;
        private readonly FeatureUnlockConfiguration featureUnlockConfiguration;
        private readonly InAppConfig inAppConfig;
        private readonly LevelService levelService;
        private readonly PurchaseOfferContainer purchaseOfferContainer;
        private readonly SceneLoadController sceneLoadController;
        private CancellationTokenSource cts;

        [Inject]
        public WidgetManager(WidgetFactory widgetFactory,
                             MainUIProvider mainUIProvider,
                             ShopUiController shopUiController,
                             NoAdsUiController noAdsUiController,
                             SuperDiscountUiController superDiscountUiController,
                             ConfigProvider configProvider,
                             LevelService levelService,
                             PurchaseOfferContainer purchaseOfferContainer,
                             SceneLoadController sceneLoadController)
        {
            this.widgetFactory = widgetFactory;
            this.shopUiController = shopUiController;
            this.noAdsUiController = noAdsUiController;
            this.superDiscountUiController = superDiscountUiController;
            this.levelService = levelService;
            this.sceneLoadController = sceneLoadController;
            this.purchaseOfferContainer = purchaseOfferContainer;
            featureUnlockConfiguration = configProvider.FeatureUnlockConfiguration;
            inAppConfig = configProvider.InAppConfig;
            widgetsConfig = configProvider.WidgetsConfig;
            bottomPanelConfig = configProvider.BottomPanelConfiguration;
            uiRoots = mainUIProvider.HudProvider.WidgetUIRoots;
        }

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            uiRoots.Initialize();
            
            cts = new();
            FillConfigDatas();

            await CreateShopAsync(cancellationToken);
            await CreateNoAdsAsync(cancellationToken);
            await CreateSuperDiscountAsync(cancellationToken);

            foreach (var item in widgetViews)
                item.Value.Initialize();

            purchaseOfferContainer.OnPurchaseCompleted += PurchaseOfferContainer_OnPurchaseCompleted;
            purchaseOfferContainer.OnOfferSecondTick += PurchaseOfferContainer_OnOfferSecondTick;
            superDiscountUiController.OnSkinChange += SuperDiscountUiController_OnSkinChange;

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;
            
            uiRoots.Deinitialize();
            DeinitializeImpl();
            configDataCached.Clear();
            IsInitialized = false;
        }

        public void ShowWidgets()
        {
            if (!sceneLoadController.IsMetaSceneActive)
                return;

            foreach (var item in widgetViews)
            {
                if (item.Value.IsUnlocked && !WidgetBlocked(item.Key))
                    item.Value.Show();
                else
                    item.Value.Hide();
            }
        }

        public void HideWidgets()
        {
            foreach (var item in widgetViews)
                item.Value.Hide();
        }

        public bool TryGetWidget(WidgetId widgetId, out IWidgetReadable widgetReadable)
        {
            if (widgetViews.TryGetValue(widgetId, out var widget))
            {
                widgetReadable = widget;
                return true;
            }
            
            widgetReadable = null;
            return false;
        }
        
        public bool TryGetWidgetRootSide(WidgetId widgetId, out RootSide rootSide)
        {
            if (configDataCached.TryGetValue(widgetId, out var data))
            {
                rootSide = data.root;
                return true;
            }
            
            rootSide = RootSide.Left;
            return false;
        }
        
        public UniTask<Widget> GetOrAddWidgetAsync(WidgetId widgetId, string assetGUID, string widgetText, Action handler, bool isUnlocked, CancellationToken ct)
        {
            return GetOrAddWidgetAsync<Widget>(widgetId, assetGUID, widgetText, handler, isUnlocked, ct);
        }

        public async UniTask<T> GetOrAddWidgetAsync<T>(WidgetId widgetId, string assetGUID, string widgetText, Action handler, bool isUnlocked, CancellationToken ct) where T : Widget
        {
            if (widgetViews.TryGetValue(widgetId, out var view))
            {
                return (T)view;
            }

            if (!configDataCached.TryGetValue(widgetId, out var configData))
            {
                Debug.LogError($"Widget config data for \"{widgetId}\" not found. Check {nameof(WidgetsConfig)}");
                return null;
            }

            if (!uiRoots.TryGetRoot(configData.root, out var widgetRoot))
            {
                Debug.LogError($"Widget root for \"{widgetId}\" not found. Check {nameof(WidgetUIRoots)}");
                return null;
            }
            
            var widget = await widgetFactory.CreateAsync<T>(assetGUID, widgetRoot.Root, ct, true, true);
            AddWidget(widgetId, widget, widgetText, configData.priority, handler, isUnlocked);
            widgetRoot.Add(widget);
            return widget;
        }

        public void SetWidgetText(WidgetId widgetId, string text)
        {
            if (!widgetViews.TryGetValue(widgetId, out var view))
                return;

            if (view == null)
                return;

            view.SetText(text);
        }

        public bool RemoveWidget(WidgetId key)
        {
            if (!widgetViews.TryGetValue(key, out var widget))
                return false;
            
            if (widget == null)
                return false;

            widget.Deinitialize();
            UnityEngine.Object.Destroy(widget.gameObject);
            widgetViews.Remove(key);
            return true;
        }

        public bool IsWidgetExist(WidgetId key)
        {
            return widgetViews.ContainsKey(key);
        }

        public async UniTask RecreateWidgetAsync(WidgetId widgetId, string assetGuid, string widgetText, Action handler, bool isUnlocked, CancellationToken ct)
        {
            RemoveWidget(widgetId);

            var widget = await GetOrAddWidgetAsync(widgetId, assetGuid, widgetText, handler, isUnlocked, ct);
            widget.Initialize();

            if (!sceneLoadController.IsMetaSceneActive)
                return;

            if (!WidgetBlocked(widgetId))
            {
                widget.Show();
            }
        }

        public void RefreshWidgetNotifier(WidgetId widgetId, bool isActive)
        {
            if (!widgetViews.TryGetValue(widgetId, out var widget))
                return;

            widget.SetNotifierActive(isActive);
        }

        private void DeinitializeImpl()
        {
            cts.Cancel();
            cts.Dispose();

            superDiscountUiController.OnSkinChange -= SuperDiscountUiController_OnSkinChange;
            purchaseOfferContainer.OnOfferSecondTick -= PurchaseOfferContainer_OnOfferSecondTick;
            purchaseOfferContainer.OnPurchaseCompleted -= PurchaseOfferContainer_OnPurchaseCompleted;

            DeinitializeWidgets();
        }

        private void DeinitializeWidgets()
        {
            foreach (var item in widgetViews)
            {
                item.Value.Deinitialize();
                item.Value.Dispose();
            }

            widgetViews.Clear();
        }

        private void AddWidget(WidgetId widgetId, Widget widget, string widgetText, int priority, Action handler, bool isUnlocked)
        {
            widget.SetText(widgetText);
            widget.SetUnlocked(isUnlocked);
            widget.SetPriority(priority);
            
            widgetViews.Add(widgetId, widget);
            widgetViews[widgetId].SetAction(handler);
        }
        
        
        private bool WidgetBlocked(WidgetId key)
        {
            if (key == WidgetId.NoAds && (inAppConfig.GetOfferData(OfferId.rh_no_ads_plus_v2).unlockLevel > levelService.CurrentLevelNumber || Advertisement.IsPremium()))
                return true;

            if (key == WidgetId.SuperDiscount)
                if (superDiscountUiController.CurrentInAppOfferId == OfferId.none)
                    return true;

            if (key == WidgetId.Shop)
                if (bottomPanelConfig.Feature.isEnabled)
                    return true;

            return false;
        }

        private void NoAdsWidgetHandler()
        {
            noAdsUiController?.ShowPurchasePopup(OpeningMethod.Yourself, PurchaseOfferShowSource.NoAdsWidget);
        }

        private void ShopWidgetHandler()
        {
            shopUiController?.ShowShopPopup(ShopPopup.ShopMode.Popup, OpeningMethod.Yourself, ShopOpenSource.ShopWidget).Forget();
        }

        private void SuperDiscountWidgetHandler()
        {
            superDiscountUiController?.ShowSuperDiscountPopup(OpeningMethod.Yourself, PurchaseOfferShowSource.SuperDiscount);
        }

        private async UniTaskVoid ChangeSuperDiscountSkin(double timeRestSec, CancellationToken cancellationToken)
        {
            if (!RemoveWidget(WidgetId.SuperDiscount))
                return;

            var newWidget = await CreateSuperDiscountAsync(cancellationToken);
            newWidget.Initialize();

            if (!sceneLoadController.IsMetaSceneActive)
                return;

            var time = timeRestSec < 86400 ? TimeUtils.GetTimeString(timeRestSec) : TimeUtils.GetTimeString(timeRestSec, 100f);
            newWidget.SetText(time);

            if (!WidgetBlocked(WidgetId.SuperDiscount))
                newWidget.Show();
        }

        private void FillConfigDatas()
        {
            foreach (var dataItem in widgetsConfig.Items)
            {
                configDataCached.Add(dataItem.id, dataItem);
            }
        }

        private UniTask<Widget> CreateNoAdsAsync(CancellationToken cancellationToken)
        {
            return GetOrAddWidgetAsync(WidgetId.NoAds, "NoAdsWidget", string.Empty, NoAdsWidgetHandler, featureUnlockConfiguration.noAdsWidgetUnlocked,
                            cancellationToken);
        }

        private UniTask<Widget> CreateShopAsync(CancellationToken cancellationToken)
        {
            return GetOrAddWidgetAsync(WidgetId.Shop, "ShopWidget", LocalizationService.I.Get(LocKeys.Shop.ShopText), ShopWidgetHandler,
                            featureUnlockConfiguration.shopWidgetUnlocked, cancellationToken);
        }

        private UniTask<Widget> CreateSuperDiscountAsync(CancellationToken cancellationToken)
        {
            return GetOrAddWidgetAsync(WidgetId.SuperDiscount, superDiscountUiController.CurrentSkinData.widgetAssetId, "10d 10h", SuperDiscountWidgetHandler,
                            featureUnlockConfiguration.superDiscountWidgetUnlocked, cancellationToken);
        }

        private void PurchaseOfferContainer_OnPurchaseCompleted(OfferId id, bool arg2)
        {
            ShowWidgets();
        }

        private void PurchaseOfferContainer_OnOfferSecondTick((OfferId offerId, double timeRestSec) offerSecondTick)
        {
            if (!widgetViews.TryGetValue(WidgetId.SuperDiscount, out var widgetView))
                return;

            if (widgetView == null)
                return;

            if (superDiscountUiController.CurrentInAppOfferId == OfferId.none)
            {
                widgetView.Hide();
                return;
            }

            if (offerSecondTick.offerId != superDiscountUiController.CurrentInAppOfferId)
                return;

            if (!sceneLoadController.IsMetaSceneActive)
                return;

            var time = offerSecondTick.timeRestSec < 86400 ? TimeUtils.GetTimeString(offerSecondTick.timeRestSec) : TimeUtils.GetTimeString(offerSecondTick.timeRestSec, 100f);
            widgetView.SetText(time);
            if (offerSecondTick.timeRestSec <= 0)
                widgetView.Hide();
        }

        private void SuperDiscountUiController_OnSkinChange(SuperDiscountSkinType type, double timeRestSec)
        {
            ChangeSuperDiscountSkin(timeRestSec, cts.Token).Forget();
        }
    }
}