using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.WalletSystem;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using Infrastructure.MainUICanvasControl;
using Infrastructure.SpriteAtlasControl;
using R3;
using UnityEngine;
using Features.ShopUi;
using Features.Life;
using System;
using Infrastructure.Utilities;
using Infrastructure.Localization;
using Features.LifeUi;
using Infrastructure.PurchaseSystem;


namespace Infrastructure.CurrencyHud
{
    public enum CurrencyHudMode 
    {
        Meta = 0,
        Game = 1,
        BuyLifePopup = 2,
        ResurrectPopup = 3,
        ShowAll = 9998,
        HideAll = 9999,
    }

    public class CurrencyHudService
    {
        private readonly Instantiator instantiator;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly ShopUiController shopUiController;
        private readonly Wallet wallet;
        private readonly LifeController lifeController;
        private readonly LifeUiController lifeUiController;
        private readonly Transform currencyUIRoot;
        private readonly CurrencyConfig currencyConfig;
        private readonly Dictionary<CurrencyType, CurrencyUI> hudElements;
        private readonly CompositeDisposable disposable;
        private readonly string currencyPrefabGuid;
        private readonly string lifePrefabGuid;

        private string fullLifeText = string.Empty;

        private CurrencyHudMode baseMode = CurrencyHudMode.Meta;

        private readonly Dictionary<CurrencyHudMode, List<CurrencyType>> currencyHudModeMap = new() 
        {
            { CurrencyHudMode.Meta,           new() { CurrencyType.Coins, CurrencyType.Life } },
            { CurrencyHudMode.Game,           new() { } },
            { CurrencyHudMode.BuyLifePopup,   new() { CurrencyType.Coins } },
            { CurrencyHudMode.ResurrectPopup, new() { CurrencyType.Coins } },
            { CurrencyHudMode.ShowAll,        new() { CurrencyType.Coins, CurrencyType.Life } },
            { CurrencyHudMode.HideAll,        new() { } }
        };

        private bool isInit;


        public CurrencyHudService(
            Instantiator instantiator,
            MainUIProvider mainUIProvider,
            ConfigProvider configProvider,
            SpriteAtlasService spriteAtlasService,
            ShopUiController shopUiController,
            Wallet wallet,
            LifeController lifeController,
            LifeUiController lifeUiController)
        {
            this.instantiator = instantiator;
            this.shopUiController = shopUiController;
            this.currencyUIRoot = mainUIProvider.HudProvider.CurrencyUIRoot;
            currencyPrefabGuid = configProvider.AssetMappingConfig.CurrencyUI.AssetGUID;
            lifePrefabGuid = configProvider.AssetMappingConfig.LifeUI.AssetGUID;
            this.currencyConfig = configProvider.CurrencyConfig;
            this.spriteAtlasService = spriteAtlasService;
            this.wallet = wallet;
            this.lifeController = lifeController;
            this.lifeUiController = lifeUiController;
            hudElements = new Dictionary<CurrencyType, CurrencyUI>();
            disposable = new CompositeDisposable();
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;
            
            foreach (CurrencyData dataItem in currencyConfig.Items)
            {
                if(!dataItem.inHud)
                    continue;
                var guid = dataItem.currencyType == CurrencyType.Life ? lifePrefabGuid : currencyPrefabGuid;
                CurrencyUI currencyUI = await instantiator.InstantiateAsync<CurrencyUI>(guid, parent: currencyUIRoot, worldSpace: false, inject: true, cancellationToken: cancellationToken);
                Sprite icon = spriteAtlasService.GetCurrencyIcon(dataItem.currencyType);

                Action callback = dataItem.currencyType == CurrencyType.Life ? lifeUiController.ShowBuyLifePopup : ShopOpenCallback;
                currencyUI.Construct(icon, wallet.GetCount(dataItem.currencyType), "", callback);
                currencyUI.Initialize();
                hudElements.Add(dataItem.currencyType, currencyUI);
            }
            
            wallet.OnChange.Subscribe(HandleTransaction).AddTo(disposable);

            lifeController.OnTimerUpdate += LifeController_OnTimerUpdate;
            lifeUiController.OnShowCurrencyRequest += LifeUiController_OnShowCurrencyRequest;

            fullLifeText = LocalizationService.I.Get(LocKeys.Life.FullLife);

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;
            lifeUiController.OnShowCurrencyRequest -= LifeUiController_OnShowCurrencyRequest;
            lifeController.OnTimerUpdate -= LifeController_OnTimerUpdate;

            foreach (var entryPair in hudElements)
            {
                entryPair.Value.Deinitialize();
                entryPair.Value.DestroyObject();
            }

            hudElements.Clear();
            disposable.Dispose();

            isInit = false;
        }

        public void ShowAll()
        {
            foreach (var entryPair in hudElements)
            {
                entryPair.Value.SetObjectActive(true);
            }
        }


        public void HideAll()
        {
            foreach (var entryPair in hudElements)
            {
                entryPair.Value.SetObjectActive(false);
            }
        }


        public void Show(CurrencyType currencyType)
        {
            if (!hudElements.TryGetValue(currencyType, out CurrencyUI currencyUI))
                return;

            currencyUI.SetObjectActive(true);
        }


        public void Hide(CurrencyType currencyType)
        {
            if (!hudElements.TryGetValue(currencyType, out CurrencyUI currencyUI))
                return;

            currencyUI.SetObjectActive(false);
        }

        public void SetMode(CurrencyHudMode currencyHudMode, bool isBase = true) 
        {
            if (isBase)
                baseMode = currencyHudMode;

            foreach (var item in hudElements)
            {
                if (currencyHudModeMap[currencyHudMode].Contains(item.Key))
                    Show(item.Key);
                else
                    Hide(item.Key);
            }
        }

        public void ResetMode() 
        {
            SetMode(baseMode);
        }

        private void LifeUiController_OnShowCurrencyRequest(bool needShow)
        {
            if (needShow)
                SetMode(CurrencyHudMode.BuyLifePopup, false);
            else
                ResetMode();
        }

        public bool TryGetIconTarget(CurrencyType currencyType, out RectTransform result)
        {
            result = null;

            if (!hudElements.TryGetValue(currencyType, out CurrencyUI currencyUI))
                return false;

            result = currencyUI.IconTransform;
            return true;
        }


        public void RefreshOnIncrease(CurrencyType currency, int value)
        {
            if (!hudElements.TryGetValue(currency, out CurrencyUI currencyUI))
                return;

            if (value > currencyUI.Value)
            {
                currencyUI.RefreshValue(value, true, ActionType.Set);
            }
        }


        public void Refresh(CurrencyType currency, int value, bool isAnimated, ActionType actionType)
        {
            if (!hudElements.TryGetValue(currency, out CurrencyUI currencyUI))
                return;

            currencyUI.RefreshValue(value, isAnimated, actionType);
        }


        public int GetCurrentState(CurrencyType currency)
        {
            if (!hudElements.TryGetValue(currency, out CurrencyUI currencyUI))
                return -1;

            return currencyUI.Value;
        }
        
        
        private void HandleTransaction(Transaction tr)
        {
            if (!tr.updateHud)
            {
                return;
            }

            Refresh(tr.currency, tr.total, true, ActionType.Set);
        }

        private void LifeController_OnTimerUpdate((bool isLifeFull, int timeRestSec) updateData)
        {
            if (!hudElements.TryGetValue(CurrencyType.Life, out CurrencyUI currencyUI))
                return;

            bool isInfinite = lifeController.IsInfiniteLife();

            string addText;
            if (isInfinite)
            {
                addText = TimeUtils.GetTimeString(lifeController.GetInfiniteExpiresSeconds(), 100.0f);
            }
            else if(updateData.isLifeFull)
            {
                addText = fullLifeText;
            }
            else
            {
                addText = TimeUtils.GetTimeString(updateData.timeRestSec);
            }

            currencyUI.SetLabelActive(!isInfinite);
            currencyUI.SetInfinityIconActive(isInfinite);
            currencyUI.RefreshAdditionalText(addText, false);
            currencyUI.SetClickable(!isInfinite && !updateData.isLifeFull);
        }

        private void ShopOpenCallback() 
        {
            shopUiController.ShowShopPopup(ShopPopup.ShopMode.Screen, OpeningMethod.Forse, ShopOpenSource.CurrencyHud).Forget();
        }
    }
}