#if PR_SAYKIT_ENABLED
using Cysharp.Threading.Tasks;
using Infrastructure.Ads;
using Infrastructure.ApplicationInterrupt;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.PersistentProgress;
using Infrastructure.SystemModules;
using SayKitInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VContainer;

#pragma warning disable CS0067 // The event is never used
namespace Infrastructure.PurchaseSystem
{
    public class PurchaseManagerSaykit : IPurchaseManager, ISavable
    {
        public event Action<(bool isLoading, string message, float maxTime)> OnPurchaseLoadingEvent;
        public event Action OnPurchaseFailed;

        public event Action<OfferId> OnPurchaseCompleted;
        

        private readonly SdkService sdkService;
        private readonly SaveStorage saveStorage;
        private readonly AnalyticsContextCreator analyticsContextCreator;
        private readonly ConfigProvider configProvider;


#if UNITY_IOS || UNITY_IPHONE
        public const float MaxConnectingTime = 30.0f;
        private bool isPurchaseInProgress = false;
#endif

        private bool isRestoreInProgress = false;

        private PurchaseState purchaseSaveState = new();

        private List<SKProduct> skProducts;

        public List<InAppData> ActiveOfferDataList { get; private set; }


        [Inject]
        public PurchaseManagerSaykit(SdkService sdkService,
                               SaveStorage saveStorage,
                               AnalyticsContextCreator analyticsContextCreator,
                               ConfigProvider configProvider) 
        {
            this.sdkService = sdkService;
            this.saveStorage = saveStorage;
            this.analyticsContextCreator = analyticsContextCreator;
            this.configProvider = configProvider;
        }

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;
            await InitializeImplAsync(cancellationToken);
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;
            DeinitializeImpl();
        }


        public void InitiatePurchase(OfferId inAppOfferId, OpeningMethod openingMethod, string source)
        {
            InAppData offer = GetOfferData(inAppOfferId);
            if (offer == null)
                return;

            if (offer.type == OfferType.InApp)
                InitiatePurchaseInApp(offer, openingMethod, source);
            else if (offer.type == OfferType.RewardedAd)
                InitiatePurchaseRewardedAd(offer, openingMethod, source);
        }


        private void InitiatePurchaseInApp(InAppData offer, OpeningMethod openingMethod, string source)
        {
            if (offer.type != OfferType.InApp)
            {
                Debug.LogError($"Offer {offer.id} is not {OfferType.InApp}.");
                return;
            }

            StartPurchase();
            PurchaseAnalytics.TrackInAppAttempt(offer.id, openingMethod, source, analyticsContextCreator.MainContext);
            InAppInProgress inAppInProgress = new InAppInProgress(offer.id, openingMethod, source);
            purchaseSaveState.inAppsInProgress.Add(inAppInProgress);

#if PR_CHEAT || UNITY_EDITOR
            if (CheatPurchaseEnabled)
            {
                CheatPurchaseProcess(inAppInProgress);
                return;
            }
#endif

            SKProductInfo productInfo = CreateSKProductInfo(offer.id.ToString(), offer.type, offer.productType);
            SayKit.PurchaseProduct(productInfo, (bool result, SKPurchasedProduct purchasedProduct, SKBillingError error) =>
            {
                OnPurchaseProductCompleted(inAppInProgress, result, purchasedProduct, error);
            },
            offer: openingMethod.ToString(), placement: source);
        }

        private void InitiatePurchaseRewardedAd(InAppData offer, OpeningMethod openingMethod, string source)
        {
            if (offer.type != OfferType.RewardedAd)
            {
                Debug.LogError($"Offer {offer.id} is not {OfferType.RewardedAd}.");
                return;
            }

            OnPurchaseCompleted?.Invoke(offer.id);
        }


        public void RestorePurchases()
        {
            if (isRestoreInProgress)
            {
                return;
            }

            isRestoreInProgress = true;
            StartPurchase();
            SayKit.RestorePurchases(OnRestorePurchasesCompleted);
        }


        public string GetLocalizedPrice(OfferId inAppOfferId)
        {
            InAppData offer = GetOfferData(inAppOfferId);
#if UNITY_EDITOR

            if (offer != null
             && offer.id == inAppOfferId
             && offer.type == OfferType.InApp)
            {
                return $"{offer.price - 0.01f}$";
            }

            return "None";
#else
            if (!IsInitialized)
                return LocalizationService.I.Get(LocKeys.Purchase.NoInternet);

            if (offer.type != OfferType.InApp)
            {
                Debug.LogError($"Offer {inAppOfferId} is not {OfferType.InApp}.");
                return "None";
            }

            SKProduct product = GetSKProduct(inAppOfferId.ToString());
            if (product != null)
            {
                return product.LocalizedPrice;
            }

            return "None";
#endif
        }


        public string GetLocalizedFakePrice(OfferId inAppOfferId, int fakeDiscountInPercent)
        {
            if (!IsInitialized)
                return LocalizationService.I.Get(LocKeys.Purchase.NoInternet);

            SKProduct product = GetSKProduct(inAppOfferId.ToString());
            if (product == null)
            {
                return "None";
            }

            string localizedPriceString = product.LocalizedPrice;
            string localizedSeparator = localizedPriceString.Contains(",") ? "," : ".";

            float price = product.Price;

            float fakePrice;

            if (fakeDiscountInPercent >= 100)
                fakePrice = price + (price * (fakeDiscountInPercent / 100));
            else
                fakePrice = price / (100 - fakeDiscountInPercent) * 100;

            string fakePriceString = Math.Round(fakePrice, 2).ToString("F2");
            string separator = fakePriceString.Contains(",") ? "," : ".";

            string localizedFakePriceString = localizedPriceString;
            int lastRemoveIndex = 0;
            int i = 0;
            while (i < localizedFakePriceString.Length)
            {
                char character = localizedFakePriceString[i];
                if (char.IsDigit(character)
                    || character.ToString() == localizedSeparator)
                {
                    lastRemoveIndex = i;
                    localizedFakePriceString = localizedFakePriceString.Remove(i, 1);
                }
                else
                {
                    ++i;
                }
            }

            return localizedFakePriceString.Insert(lastRemoveIndex, fakePriceString.Replace(separator, localizedSeparator));
        }


        protected async UniTask InitializeImplAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => sdkService.IsInitialized(), cancellationToken: cancellationToken);

            ActiveOfferDataList = configProvider.InAppConfig.Items.Where(x => x.isEnabled).ToList();
            SayKit.GetAvailableProducts(CreateSKProductInfos(), OnInitializeFinish);
        }


        protected void OnInitializeFinish(SKProduct[] availableProducts, SKBillingError error)
        {
            if(error != null)
            {
                SayKit.trackEvent("c_iap_error", availableProducts?.Length ?? 0, 0, 0, error.ToString());
                return;
            }

            SayKitApp.OnNonConfirmedPurchaseReceived += SayKitApp_OnNonConfirmedPurchaseReceived;
            skProducts = availableProducts.ToList();
            IsInitialized = true;

            ProcessNonConfirmedProducts();
        }


        protected void DeinitializeImpl()
        {
            SayKitApp.OnNonConfirmedPurchaseReceived -= SayKitApp_OnNonConfirmedPurchaseReceived;
            IsInitialized = false;
        }


        void ISavable.Load(PersistentProgress.Progress progress)
        {
            purchaseSaveState = progress.purchaseState;            
        }


        void ISavable.Save(PersistentProgress.Progress progress)
        {
            progress.purchaseState = purchaseSaveState;            
        }


        private void OnPurchaseProductCompleted(InAppInProgress inAppInProgress, bool result, SKPurchasedProduct purchasedProduct, SKBillingError error)
        {
            if (result)
            {
                ProcessPurchase(inAppInProgress, purchasedProduct);
                return;
            }

            if(error.Type == SKBillingErrorType.PurchaseIsPending)
            {
                SayKit.trackEvent(eventName: "c_iap_purchase_pending", error.ToString());
                return;
            }

            if (error.Type == SKBillingErrorType.PurchaseInProgress)
            {
                SayKit.trackEvent(eventName: "c_iap_purchase_in_progress", error.ToString());
                return;
            }

            SayKit.trackEvent(eventName: "c_iap_purchase_error", error.ToString(), inAppInProgress.inAppOfferId.ToString());
            OnPurchaseProductCompletedFailure(inAppInProgress, error);
        }


        private void ProcessPurchase(InAppInProgress inAppInProgress, SKPurchasedProduct purchasedProduct)
        {
            if (inAppInProgress != null
                && inAppInProgress.progressState != InAppInProgress.ProgressState.completed)
            {
                PurchaseComplete(inAppInProgress);
            }

#if PR_CHEAT || UNITY_EDITOR
            if (CheatPurchaseEnabled)
            {
                purchaseSaveState.inAppsInProgress.Remove(inAppInProgress);
                return;
            }
#endif

            SayKit.ConfirmPurchase(purchasedProduct, (bool result, SKBillingError error) =>
            {
                OnConfirmProductCompleted(inAppInProgress, result, error);
            });
        }


        private void PurchaseComplete(InAppInProgress inAppInProgress)
        {
            FinishPurchase();

            PurchaseAnalytics.TrackInAppResult(inAppInProgress.inAppOfferId,
                                                inAppInProgress.openingMethod,
                                                inAppInProgress.source,
                                                PurchaseResult.Success,
                                                analyticsContextCreator.MainContext);

            inAppInProgress.progressState = InAppInProgress.ProgressState.completed;
            saveStorage.Save();
            OnPurchaseCompleted?.Invoke(inAppInProgress.inAppOfferId);
        }


        void OnPurchaseProductCompletedFailure(InAppInProgress inAppInProgress, SKBillingError error)
        {
            FinishPurchase();
            PurchaseAnalytics.TrackInAppResult(inAppInProgress.inAppOfferId,
                                                inAppInProgress.openingMethod,
                                                inAppInProgress.source,
                                                GetPurchaseResult(error.Type),
                                                analyticsContextCreator.MainContext);

            purchaseSaveState.inAppsInProgress.Remove(inAppInProgress);

#if UNITY_IPHONE || UNITY_IOS
             OnPurchaseFailed?.Invoke();
#endif
        }


        private void OnRestorePurchasesCompleted(string[] restoredProductIds, SKInAppSubscription[] subscriptions, SKBillingError error)
        {
            isRestoreInProgress = false;
            FinishPurchase();

            if (error != null)
            {
                SayKit.trackEvent("c_iap_restore_error", restoredProductIds?.Length ?? 0, subscriptions?.Length ?? 0, 0, error.ToString());
                return;
            }

            for(int i = 0; i < restoredProductIds.Length; ++i)
            {
                OfferId inAppOfferId = GetInAppOfferId(restoredProductIds[i]);
                InAppData offer = GetOfferData(inAppOfferId);
                if (offer == null)
                {
                    continue;
                }

                if (offer.productType == PurchaseProductType.NonConsumable
                    && !IsInAppPurchased(inAppOfferId))
                {
                    OnPurchaseCompleted?.Invoke(inAppOfferId);
                    PurchaseAnalytics.TrackInAppResult(inAppOfferId, 
                        OpeningMethod.Yourself, 
                        PurchaseOfferShowSource.SettingsPopup, 
                        PurchaseResult.Restore,
                        analyticsContextCreator.MainContext);
                }
            }
        }


        private bool IsInAppPurchased(OfferId inAppOfferId)
        {
            for(int i = 0; i < purchaseSaveState.purchasedInAppDataList.Count; ++i)
            {
                if (purchaseSaveState.purchasedInAppDataList[i].inAppOfferId == inAppOfferId)
                {
                    return true;
                }
            }

            return false;
        }


        private InAppInProgress GetInAppInProgress(OfferId inAppOfferId)
        {
            for (int i = 0; i < purchaseSaveState.inAppsInProgress.Count; ++i)
            {
                if (purchaseSaveState.inAppsInProgress[i].inAppOfferId == inAppOfferId)
                {
                    return purchaseSaveState.inAppsInProgress[i];
                }
            }

            return null;
        }


        private PurchaseResult GetPurchaseResult(SKBillingErrorType sKBillingErrorType)
        {
            PurchaseResult purchaseResult = PurchaseResult.Unknown;
            switch (sKBillingErrorType)
            {
                case SKBillingErrorType.UserCancelled:
                {
                    purchaseResult = PurchaseResult.UserCancelled;
                }
                break;
                case SKBillingErrorType.NoInternet:
                {
                    purchaseResult = PurchaseResult.NoInternetConnection;
                }
                break;
            }

            return purchaseResult;
        }


        private SKProductInfo[] CreateSKProductInfos()
        {
            SKProductInfo[] productInfos = new SKProductInfo[ActiveOfferDataList.Count];
            for (int i = 0; i < ActiveOfferDataList.Count; i++)
            {
                InAppData offer = ActiveOfferDataList[i];
                if (offer.type != OfferType.InApp)
                    continue;
                productInfos[i] = CreateSKProductInfo(offer.id.ToString(), offer.type, offer.productType);
            }

            return productInfos;
        }


        private SKProductInfo CreateSKProductInfo(string id, OfferType offerType, PurchaseProductType purchaseProductType)
        {
            if (offerType != OfferType.InApp)
            {
                Debug.LogError($"Offer {id} is not {OfferType.InApp}.");
                return null;
            }

            return new SKProductInfo()
            {
                Id = id,
                Type = GetSKProductType(purchaseProductType),
            };
        }

        private SKProductType GetSKProductType(PurchaseProductType purchaseProductType)
        {
            return purchaseProductType switch
            {
                PurchaseProductType.Consumable    => SKProductType.Consumable,
                PurchaseProductType.NonConsumable => SKProductType.NonConsumable,
                PurchaseProductType.Subscription  => SKProductType.Subscription,
                _                                 => throw new ArgumentOutOfRangeException(nameof(purchaseProductType), purchaseProductType, null)
            };
        }

        private InAppData GetOfferData(OfferId inAppOfferId)
        {
            for (int i = 0; i < ActiveOfferDataList.Count; i++)
            {
                InAppData offer = ActiveOfferDataList[i];
                if (offer.id == inAppOfferId)
                {
                    return offer;
                }
            }

            Debug.LogError($"No offer {inAppOfferId} in InAppConfig");
            return null;
        }


        private SKProduct GetSKProduct(string id)
        {
            for (int i = 0; i < skProducts.Count; ++i)
            {
                if (skProducts[i].Id == id)
                {
                    return skProducts[i];
                }
            }

            Debug.LogError($"[CODE] Product \"{id}\" not found. Check id is added to: 1.{nameof(CreateSKProductInfos)}, 2.Launcher Inapps, 3.Google console, 4.Apple appstoreconnect");
            return null;
        }


        private OfferId GetInAppOfferId(string productId)
        {
            return Enum.Parse<OfferId>(productId, true);
        }


        private void OnConfirmProductCompleted(InAppInProgress inAppInProgress, bool result, SKBillingError error)
        {
            if (!result)
            {
                SayKit.trackEvent("c_iap_confirm_error", error.ToString());
                return;
            }

            purchaseSaveState.inAppsInProgress.Remove(inAppInProgress);
        }


        private void StartPurchase()
        {
#if UNITY_IPHONE || UNITY_IOS
            isPurchaseInProgress = true;
            OnPurchaseLoadingEvent?.Invoke((isPurchaseInProgress, LocKeys.Purchase.LoadingPopupConnecting, MaxConnectingTime));
#endif
        }

        private void FinishPurchase()
        {
#if UNITY_IPHONE || UNITY_IOS
            isPurchaseInProgress = false;
            OnPurchaseLoadingEvent?.Invoke((isPurchaseInProgress, string.Empty, MaxConnectingTime));
#endif
        }

        private void ProcessNonConfirmedProducts()
        {
            SayKit.GetNonConfirmedProducts(ApplyNonConfirmedProducts);
        }


        private void ApplyNonConfirmedProducts(SKPurchasedProduct[] purchasedProducts, SKBillingError error)
        {
            if (error != null)
            {
                SayKit.trackEvent(eventName: "c_iap_get_nonconfirmed_error", error.ToString());
                return;
            }

            foreach (SKPurchasedProduct unconfirmedProduct in purchasedProducts)
            {
                OfferId inAppOfferId = GetInAppOfferId(unconfirmedProduct.Id);
                InAppInProgress inAppInProgress = GetInAppInProgress(inAppOfferId);
                
                InAppData offer = GetOfferData(inAppOfferId);
                if (offer == null)
                {
                    continue;
                }

                if (offer.productType == PurchaseProductType.Consumable
                    || !IsInAppPurchased(inAppOfferId))
                {
                    ProcessPurchase(inAppInProgress, unconfirmedProduct);
                }
            }
        }


        void SayKitApp_OnNonConfirmedPurchaseReceived()
        {
            ProcessNonConfirmedProducts();
        }


        #region Cheat

#if PR_CHEAT || UNITY_EDITOR

        public bool CheatPurchaseEnabled { get; private set; }
#if UNITY_EDITOR
        = true;
#endif

        public void SetCheatPurchaseEnabled(bool value)
        {
            CheatPurchaseEnabled = value;
        }


        private void CheatPurchaseProcess(InAppInProgress inAppInProgress)
        {
            ProcessPurchase(inAppInProgress, null);
        }        
#endif

        #endregion
    }
}

#endif