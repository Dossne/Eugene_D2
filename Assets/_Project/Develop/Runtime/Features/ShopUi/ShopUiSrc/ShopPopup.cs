using System.Collections.Generic;
using System.Threading;
using Infrastructure.AssetManagement;
using Infrastructure.Popups;
using UnityEngine;
using Infrastructure.PurchaseSystem;
using System;
using Cysharp.Threading.Tasks;
using VContainer;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Configs;
using Infrastructure.CurrencyHud;
using R3;
using Infrastructure.WalletSystem;
using AYellowpaper.SerializedCollections;
using Features.SuperDiscountUi;
using Features.Skin;
using Infrastructure.Utilities;
using Infrastructure.Localization;
using TMPro;
using System.Linq;

namespace Features.ShopUi
{
    public class ShopPopup : PopupBase
    {
        public enum ShopMode 
        {
            Screen = 0,
            Popup = 1,
        }

        public Action<OfferId> OnOfferBuyAttempt;
        public Action<OfferId> OnOfferShow;

        [SerializeField] private Transform root;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private CurrencyUI coinsUI;
        [SerializeField] private SerializedDictionary<PurchaseOfferUiType, ShopOfferScrollElement> shopOfferScrollElements = new();

        [SerializeField] private ShopOfferScrollElementList shopOfferScrollElementList;

        private List<PurchaseOfferUiDatasource> offerDataList;
        private Instantiator instantiator;
        private SpriteAtlasService spriteAtlasService;
        private ConfigProvider configProvider;
        private SuperDiscountUiController superDiscountUiController;
        private PurchaseOfferContainer purchaseOfferContainer;
        private Wallet wallet;
        private CancellationTokenSource cts;

        private CompositeDisposable disposable;

        [Inject]
        public void Construct(Instantiator instantiator, 
                              SpriteAtlasService spriteAtlasService, 
                              ConfigProvider configProvider, 
                              SuperDiscountUiController superDiscountUiController,
                              PurchaseOfferContainer purchaseOfferContainer,
                              Wallet wallet)
        {
            this.instantiator = instantiator;
            this.spriteAtlasService = spriteAtlasService;
            this.configProvider = configProvider;
            this.superDiscountUiController = superDiscountUiController;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.wallet = wallet;
            this.cts = new CancellationTokenSource();
            disposable = new CompositeDisposable();
        }

        public async UniTask UpdateOfferList(List<PurchaseOfferUiDatasource> offers)
        {
            shopOfferScrollElementList.OnElementClicked = null;
            shopOfferScrollElementList.OnElementClicked += ShopOfferScrollElementList_OnElementClicked;
            shopOfferScrollElementList.OnElementShown = null;
            shopOfferScrollElementList.OnElementShown += ShopOfferScrollElementList_OnElementShown;

            this.offerDataList = offers;

            int index = 0;
            foreach (var offerData in offerDataList)
            {
                if (shopOfferScrollElementList.HasOffer(offerData.id))
                {
                    var element = shopOfferScrollElementList.GetScrollElement(offerData.id);
                    element.Construct(offerData, spriteAtlasService, configProvider);
                    if (element is SuperDiscountScrollElement superDiscount)
                    {
                        superDiscount.SetPurchaseOfferContainer(purchaseOfferContainer);
                        superDiscount.SetSuperDiscountSkin(superDiscountUiController.CurrentSkin);
                    }

                    if (element is OneCurrencyRewardOfferScrollElement rewardOffer)
                    {
                        rewardOffer.SetPurchaseOfferContainer(purchaseOfferContainer);
                    }

                    element.transform.SetSiblingIndex(index);
                }
                else 
                {
                    var item = await CreateOfferListElement(offerData);
                    if (item != null)
                    {
                        if (item is SuperDiscountScrollElement superDiscount)
                        {
                            superDiscount.SetPurchaseOfferContainer(purchaseOfferContainer);
                            superDiscount.SetSuperDiscountSkin(superDiscountUiController.CurrentSkin);
                        }

                        if (item is OneCurrencyRewardOfferScrollElement rewardOffer)
                        {
                            rewardOffer.SetPurchaseOfferContainer(purchaseOfferContainer);
                        }
                        shopOfferScrollElementList.AddElement(item, index);
                    }
                }
                index++;
            }

            List<OfferId> inAppOfferIds = offerDataList.Select(x => x.id).ToList();
            shopOfferScrollElementList.RemoveOffersIfNotExist(inAppOfferIds);
        }

        public void ScrollToOfferType(PurchaseOfferUiType purchaseOfferType)
        {
            shopOfferScrollElementList.ScrollToOfferType(purchaseOfferType, false);
        }

        protected override void AnimationStartHandler() => shopOfferScrollElementList.SetScrollEnabled(false);
        protected override void AnimationStopHandler() => shopOfferScrollElementList.SetScrollEnabled(true);

        protected override void OnInitialize() 
        {

            Sprite icon = spriteAtlasService.GetCurrencyIcon(CurrencyType.Coins);
            coinsUI.Construct(icon, wallet.GetCount(CurrencyType.Coins), "", null);
            coinsUI.Initialize();
            superDiscountUiController.OnSkinChange += SuperDiscountUiController_OnSkinChange;
            headerText.text = LocalizationService.I.Get(LocKeys.Shop.ShopText);
        }

        protected override void OnDeinitialize() 
        {
            superDiscountUiController.OnSkinChange -= SuperDiscountUiController_OnSkinChange;
            coinsUI.Deinitialize();
            disposable?.Dispose();
        }


        protected override void OnBeginOpen()
        {
            RefreshPopupCoinHud(wallet.GetCount(CurrencyType.Coins), false);
            disposable = new();
            wallet.OnChange.Subscribe(HandleTransaction).AddTo(disposable);
        }


        protected override void OnBeginClose()
        {
            disposable?.Dispose();
        }


        private void ShopOfferScrollElementList_OnElementClicked(PurchaseOfferUiDatasource elementDatasource, RectTransform rectTransform)
        {
            OnOfferBuyAttempt?.Invoke(elementDatasource.id);
        }

        private void ShopOfferScrollElementList_OnElementShown(PurchaseOfferUiDatasource elementDatasource)
        {
            if (!elementDatasource.isDecorative)
                OnOfferShow?.Invoke(elementDatasource.id);
        }

        private async UniTask<ShopOfferScrollElement> CreateOfferListElement(PurchaseOfferUiDatasource offerData) 
        {
            if (!shopOfferScrollElements.TryGetValue(offerData.purchaseOfferUiType, out var elementPf))
            {
                if (offerData.purchaseOfferUiType != PurchaseOfferUiType.Empty)
                    Debug.LogWarning($"[CODE] ShopPopup: scroll element prefab for {offerData.purchaseOfferUiType} not found!");
                return null;
            }
                

            var item = await instantiator.InstantiateAsync(elementPf, Vector3.zero, Quaternion.identity, root, false, true, true, cts.Token);
            item.Construct(offerData, spriteAtlasService, configProvider);
            return item;
        }

        private void HandleTransaction(Transaction tr)
        {
            if (tr.currency == CurrencyType.Coins)
                RefreshPopupCoinHud(tr.total, true);
        }


        private void RefreshPopupCoinHud(int value, bool isAnimated)
        {
            coinsUI.RefreshValue(value, isAnimated, ActionType.Set);
        }


        private void SuperDiscountUiController_OnSkinChange(SuperDiscountSkinType type, double timeRestSec)
        {
            var elements = shopOfferScrollElementList.Elements;
            for (int i = 0; i < elements.Count; i++) 
            {
                if (elements[i] is SuperDiscountScrollElement superDiscount)
                    superDiscount.SetSuperDiscountSkin(type);
            }
        }
    }
}
