using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Features.LevelComplete;
using Features.ShopUi;
using Features.Skin;
using Features.SuperDiscountUi;
using Infrastructure.Ads;
using Infrastructure.AssetManagement;
using Infrastructure.BroTweens;
using Infrastructure.Configs;
using Infrastructure.CurrencyHud;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SpriteAtlasControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LevelLoose
{
    public class ResurrectPopup : PopupBase
    {
        public Action<OfferId> OnOfferBuyAttempt;
        public Action<OfferId> OnOfferShow;

        [Serializable]
        private class LoseItemRefs 
        {
            [SerializeField] public GameObject loseItemSlot;
            [SerializeField] public LostItemIcon lostItemIcon;
        }

        [SerializeField] private CurrencyUIController coinsUIController;
        
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;

        [Header("Condition")]
        [SerializeField] private TextMeshProUGUI conditionTopTxt;
        [SerializeField] private TextMeshProUGUI conditionBotTxt;
        [SerializeField] private Image conditionIcon;
        [SerializeField] private SerializedDictionary<LoseReason, Sprite> conditionIcons;

        [Header("Result")]
        [SerializeField] private TextMeshProUGUI willLooseTxt;


        [Header("LostItems")]
        [SerializeField] private List<LoseItemRefs> loseItemRefs = new();

        [Header("Buy button")]
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buyBtnTxt;
        [SerializeField] private Image buyBtnIcon;
        [SerializeField] private TextMeshProUGUI buyBtnPriceTxt;
        [SerializeField] private RectTransform buyBtnTarget;

        [Header("Reward button")]
        [SerializeField] private RewardAdsButton rewardAdsButton;
        [SerializeField] private TextMeshProUGUI rewardAdsTxt;
        
        [SerializeField] private RectTransform pageRoot;
        [SerializeField] private AnimationCurve moveCurve;
        [SerializeField] private float moveDuration;
        
        [Header("Offer")]
        [SerializeField] private SerializedDictionary<PurchaseOfferUiType, ShopOfferScrollElement> shopOfferScrollElements = new();
        [SerializeField] private ShopOfferScrollElementList shopOfferScrollElementList;
        [SerializeField] private Image pointPf;
        [SerializeField] private Transform pointParent;
        private List<PurchaseOfferUiDatasource> offerDataList;        

        private Instantiator instantiator;
        private SpriteAtlasService spriteAtlasService;
        private ConfigProvider configProvider;
        private SuperDiscountUiController superDiscountUiController;
        private PurchaseOfferContainer purchaseOfferContainer;

        private CancellationTokenSource cts;

        private Timer scrollTimer = new(5000, TimerUnit.Ms);
        private int scrollDirection = -1;
        private int scrollIndex = -1;
        private const float scrollStep = 1080;
        private List<PurchaseOfferUiType> scrollOrder = new();
        private List<Image> scrollPoints = new();

        private Action buyCallback;
        private Action closeCallback;
        private BroTweenSafe moveTween;
        private int page;


        public void Construct(
            LoseReason looseReason, 
            string addTime,
            string addTimeReward,
            Sprite currencyIcon,
            string priceText,
            Action buyCallback,
            Action closeCallback,
            string rewardPlacement,
            Action rewardCallback,
            bool isAdsBtnVisible,
            Instantiator instantiator,
            SpriteAtlasService spriteAtlasService,
            ConfigProvider configProvider,
            SuperDiscountUiController superDiscountUiController,
            PurchaseOfferContainer purchaseOfferContainer)
        {
            this.cts = new CancellationTokenSource();
            string headerKey = looseReason == LoseReason.Bomb ? LocKeys.ResurrectPopup.HeaderBomb : LocKeys.ResurrectPopup.HeaderTime;

            headerTxt.text = LocalizationService.I.Get(headerKey);
            conditionTopTxt.text = looseReason == LoseReason.Bomb ? LocalizationService.I.Get(LocKeys.ResurrectPopup.BombCondition) : "";
            conditionBotTxt.text = looseReason == LoseReason.Timer
                ? LocalizationService.I.Get(LocKeys.ResurrectPopup.TimeCondition, addTime)
                : "";

            willLooseTxt.text = LocalizationService.I.Get(LocKeys.ResurrectPopup.LooseItem);

            buyBtnIcon.sprite = currencyIcon;
            buyBtnPriceTxt.text = priceText;            
            this.buyCallback = buyCallback;
            this.closeCallback = closeCallback;

            conditionIcon.sprite = conditionIcons[looseReason];
            buyBtnTxt.text = LocalizationService.I.Get(LocKeys.ResurrectPopup.Btn);
            rewardAdsButton.SetObjectActive(isAdsBtnVisible);

            if (isAdsBtnVisible)
            {
                rewardAdsButton.Construct(rewardPlacement, this.name, rewardCallback);
                if (addTime == addTimeReward)
                    rewardAdsTxt.text = LocalizationService.I.Get(LocKeys.ResurrectPopup.AdsButton);
                else
                    rewardAdsTxt.text = LocalizationService.I.Get(LocKeys.ResurrectPopup.AdsButtonTime, addTimeReward);
            }

            this.instantiator = instantiator;
            this.spriteAtlasService = spriteAtlasService;
            this.configProvider = configProvider;
            this.superDiscountUiController = superDiscountUiController;
            this.purchaseOfferContainer = purchaseOfferContainer;
        }

        public void ResetPage()
        {
            page = 0;
            pageRoot.anchoredPosition = new Vector2(0, pageRoot.anchoredPosition.y);
        }

        public void SetLostItems(List<LostItemData> lostItems)
        {
            var lostCount = lostItems.Count;

            if (lostItems.Exists(x => x.lostItemType == LostItemType.Lives))
            {
                string firstItemName = lostCount > 0 ? lostItems[0].name : string.Empty;
                string secondItemName = lostCount > 1 ? lostItems[1].name : string.Empty;
                willLooseTxt.text = LocalizationService.I.Get(LocKeys.ResurrectPopup.LooseItem, firstItemName, secondItemName);
            }                
            else 
            {
                willLooseTxt.text = LocalizationService.I.Get(LocKeys.ResurrectPopup.LoseAllAchievements);
            }       

            for (int i = 0; i < loseItemRefs.Count; i++)
            {
                if (lostItems.Count <= i)
                {
                    loseItemRefs[i].loseItemSlot.SetActive(false);
                    continue;
                }

                loseItemRefs[i].loseItemSlot.SetActive(true);
                loseItemRefs[i].lostItemIcon.Setup(lostItems[i]);
            }
        }

        public async UniTask UpdateOfferList(List<PurchaseOfferUiDatasource> offers)
        {
            shopOfferScrollElementList.OnElementClicked = null;
            shopOfferScrollElementList.OnElementClicked += ShopOfferScrollElementList_OnElementClicked;
            shopOfferScrollElementList.OnElementShown = null;
            shopOfferScrollElementList.OnElementShown += ShopOfferScrollElementList_OnElementShown;

            this.offerDataList = offers;

            var resurrectOffers = configProvider.ResurrectOfferConfig.GetOrderedOffers();

            scrollOrder.Clear();
            int index = 0;
            foreach (var resurrectOffer in resurrectOffers)
            {
                var offerData = offerDataList.Find(x => x.id == resurrectOffer);
                if (offerData == null)
                    continue;

                if (shopOfferScrollElementList.HasOffer(offerData.id))
                {
                    var element = shopOfferScrollElementList.GetScrollElement(offerData.id);
                    element.Construct(offerData, spriteAtlasService, configProvider);
                    if (element is SuperDiscountInPopupOffer superDiscount)
                    {
                        superDiscount.SetPurchaseOfferContainer(purchaseOfferContainer);
                        superDiscount.SetSuperDiscountSkin(superDiscountUiController.CurrentSkin);
                    }

                    element.transform.SetSiblingIndex(index);
                    scrollOrder.Add(offerData.purchaseOfferUiType);
                }
                else
                {
                    var item = await CreateOfferListElement(offerData);
                    if (item != null)
                    {
                        if (item is SuperDiscountInPopupOffer superDiscount)
                        {
                            superDiscount.SetPurchaseOfferContainer(purchaseOfferContainer);
                            superDiscount.SetSuperDiscountSkin(superDiscountUiController.CurrentSkin);
                        }
                        shopOfferScrollElementList.AddElement(item, index);
                        scrollOrder.Add(offerData.purchaseOfferUiType);
                    }
                }
                index++;
            }

            List<OfferId> inAppOfferIds = offerDataList.Select(x => x.id).ToList();
            shopOfferScrollElementList.RemoveOffersIfNotExist(inAppOfferIds);

            int currentPointCount = scrollOrder.Count > 1 ? scrollOrder.Count : 0;

            while (currentPointCount > pointParent.childCount)
                scrollPoints.Add(Instantiate(pointPf, pointParent));

            while (currentPointCount < scrollPoints.Count)
            {
                Destroy(scrollPoints[0].gameObject);
                scrollPoints.RemoveAt(0);
            }

            if (scrollOrder.Count < 1)
                return;

            shopOfferScrollElementList.ScrollToOfferType(scrollOrder[0], false);
            UpdatePoints(0);
        }

        protected override void AnimationStartHandler() => shopOfferScrollElementList.SetScrollEnabled(false);
        protected override void AnimationStopHandler() => shopOfferScrollElementList.SetScrollEnabled(true);

        private async UniTask<ShopOfferScrollElement> CreateOfferListElement(PurchaseOfferUiDatasource offerData)
        {
            if (!shopOfferScrollElements.TryGetValue(offerData.purchaseOfferUiType, out var elementPf))
            {
                if (offerData.purchaseOfferUiType != PurchaseOfferUiType.Empty)
                    Debug.LogWarning($"[CODE] ShopPopup: scroll element prefab for {offerData.purchaseOfferUiType} not found!");
                return null;
            }

            var item = await instantiator.InstantiateAsync(elementPf, Vector3.zero, Quaternion.identity, shopOfferScrollElementList.ElementsRoot, false, true, true, cts.Token);
            item.Construct(offerData, spriteAtlasService, configProvider);
            return item;
        }

        protected override void OnInitialize()
        {
            superDiscountUiController.OnSkinChange += SuperDiscountUiController_OnSkinChange;
            foreach (var btn in closeButton)
            {
                btn.onClick.RemoveListener(Close);
                btn.onClick.AddListener(CloseClicked);
            }

            buyButton.onClick.AddListener(BuyClicked);
            rewardAdsButton.Initialize();
            coinsUIController.Initialize();
        }


        protected override void OnDeinitialize()
        {
            superDiscountUiController.OnSkinChange -= SuperDiscountUiController_OnSkinChange;
            foreach (var btn in closeButton)
            {
                btn.onClick.RemoveListener(CloseClicked);
            }

            buyButton.onClick.RemoveListener(BuyClicked);
            rewardAdsButton.Deinitialize();

            moveTween.Kill();
            coinsUIController.Deinitialize();
        }


        protected override void OnBeginOpen()
        {
            coinsUIController.OnActivate();
        }


        protected override void OnBeginClose()
        {
            coinsUIController.OnDeactivate();
        }

        private void CloseClicked()
        {
            if (page == 0)
            {
                page++;
                MoveRoot();
            }
            else
            {
                Close();
                closeCallback?.Invoke();
            }
        }


        private void BuyClicked()
        {
            BroTween.ClickBounceWithCallBack(buyButton, buyButton.transform, in buyCallback)
                    .Play();
        }


        private void MoveRoot()
        {
            moveTween = BroTween.AnchoredPosition(pageRoot, new Vector2(-pageRoot.rect.width, pageRoot.anchoredPosition.y), moveDuration)
                                .SetEase(moveCurve)
                                .SetUpdate(true)
                                .ToSafe();
            
            moveTween.Play();
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

        private void SuperDiscountUiController_OnSkinChange(SuperDiscountSkinType type, double timeRestSec)
        {
            var elements = shopOfferScrollElementList.Elements;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is SuperDiscountInPopupOffer superDiscount)
                    superDiscount.SetSuperDiscountSkin(type);
            }
        }

        private void Update()
        {
            if (!IsOpened)
                return;

            if (scrollOrder.Count < 2)
                return;

            if (scrollIndex == -1)
                scrollIndex = 1;            

            if (scrollTimer.IsOffAfterUpdate(Time.unscaledDeltaTime))
            {
                shopOfferScrollElementList.ScrollToOfferType(scrollOrder[scrollIndex], true);
                scrollTimer.Reset();
            }
            var scrollPosition = shopOfferScrollElementList.GetContentRootPosition();
            scrollIndex = Mathf.Abs(Mathf.RoundToInt(scrollPosition.x / scrollStep));
            UpdatePoints(scrollIndex);
            UpdateScrollIndex(scrollIndex);
        }

        private void UpdateScrollIndex(int currentIndex) 
        {
            if (currentIndex == 0)
                scrollDirection = 1;
            else if (currentIndex == scrollOrder.Count - 1)
                scrollDirection = -1;

            scrollIndex = currentIndex + scrollDirection;
        }

        private void UpdatePoints(int activeIndex) 
        {
            for (int i = 0; i < scrollPoints.Count; i++)
                scrollPoints[i].color = new Color(1, 1, 1, 0.5f);
            if (activeIndex < scrollPoints.Count)
                scrollPoints[activeIndex].color = new Color(1, 1, 1, 1);
        }
    }
}