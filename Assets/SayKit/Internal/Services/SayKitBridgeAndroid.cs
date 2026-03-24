#if UNITY_ANDROID

using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

#endregion

namespace SayKitInternal
{
    public class SayKitBridgeAndroid
    {
        private static readonly AndroidJavaClass sayKitUnityBridgeJava =
            new AndroidJavaClass("saygames.bridge.unity.SayKitBridge");

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsInitialized()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isInitialized");
        }

        public float GetInitStateProgress()
        {
            return sayKitUnityBridgeJava.CallStatic<float>("getInitStateProgress");
        }

        public int GetInitStateNumber()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getInitStateNumber");
        }

        public void InitIfNeeded(string configJson, string environmentParams)
        {
            sayKitUnityBridgeJava.CallStatic("initIfNeeded", configJson, environmentParams);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public float GetBackgroundBannerSize()
        {
            return (float)sayKitUnityBridgeJava.CallStatic<int>("getBackgroundBannerSize");
        }

        public void HideBanner()
        {
            sayKitUnityBridgeJava.CallStatic("hideBanner");
        }

        public void ShowBanner()
        {
            sayKitUnityBridgeJava.CallStatic("showBanner");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsInterstitialAvailable(string place, int countdown)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isInterstitialAvailable", place, countdown);
        }

        public bool IsRewardedAvailable(string place)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isRewardedAvailable", place);
        }

        public bool IsRewardedPlacementAvailable(string place)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isRewardedPlacementAvailable", place);
        }

        public bool ShowInterstitial(string place, int countdown)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("showInterstitial", place, countdown);
        }

        public bool ShowRewarded(string place)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("showRewarded", place);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackInterstitialOffer(string place, string extra)
        {
            sayKitUnityBridgeJava.CallStatic("trackInterstitialOffer", place, extra);
        }

        public void TrackRewardedOffer(string place, string extra)
        {
            sayKitUnityBridgeJava.CallStatic("trackRewardedOffer", place, extra);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetSubscriptionExpirationTimestamp()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getSubscriptionExpirationTimestamp");
        }

        public bool IsAppStoreAvailable()
        {
            return true;
        }

        public string GetPrivacyPolicyLink()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getPrivacyPolicyLink");
        }

        public bool IsAppInstalled(string packageName)
        {
            // iOS only
            return false;
        }

        public void OpenGooglePlaySubscriptionCenter(string productId)
        {
            sayKitUnityBridgeJava.CallStatic("openGooglePlaySubscriptionCenter", productId);
        }

        public void OpenStoreProductView(int storeId, string skadData, string storeUrl)
        {
            // iOS only
        }

        public string OpenSupportPage(string extra)
        {
            return sayKitUnityBridgeJava.CallStatic<string>("openSupportPage", extra);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsFacebookSdkInitialized()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isFacebookSdkInitialized");
        }

        public void LogFacebookEvent(string name, float valueToSum, string paramsJson)
        {
            sayKitUnityBridgeJava.CallStatic("logFacebookEvent", name, valueToSum, paramsJson);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void LogFirebaseEvent(string name, float valueToSum, string paramsJson)
        {
            sayKitUnityBridgeJava.CallStatic("logFirebaseEvent", name, valueToSum, paramsJson);
        }

        public void SetFirebaseUserProperty(string key, string value)
        {
            sayKitUnityBridgeJava.CallStatic("setFirebaseUserProperty", key, value);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void LogCrashlytics(string message)
        {
            sayKitUnityBridgeJava.CallStatic("logCrashlytics", message);
        }

        public void LogCrashlyticsException(Exception exception)
        {
            var stackTrace = SayKitStackTraceParser.ParseStackTraceString(exception);
            sayKitUnityBridgeJava.CallStatic("logCrashlyticsException", exception.Message, stackTrace);
        }

        public void SetCrashlyticsParam(string key, string value)
        {
            sayKitUnityBridgeJava.CallStatic("setCrashlyticsParam", key, value);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackApplicationLoaded()
        {
            sayKitUnityBridgeJava.CallStatic("trackApplicationLoaded");
        }

        public void TrackAvailableMemory()
        {
            sayKitUnityBridgeJava.CallStatic("trackAvailableMemory");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackEvent(string name, long param1 = 0, long param2 = 0, long param3 = 0, long param4 = 0,
            string extra1 = "", string extra2 = "", string tag = "", string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackEvent", name, param1, param2, param3, param4, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelCompleted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelCompleted", level, score, number, extra1, extra2, tag, context);
        }

        public void TrackLevelFailed(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelFailed", level, score, number, extra1, extra2, tag, context);
        }

        public void TrackLevelStarted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelStarted", level, score, number, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelExtraCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelExtraCompleted", score, number, extra1, extra2, tag, context);
        }

        public void TrackLevelExtraFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelExtraFailed", score, number, extra1, extra2, tag, context);
        }

        public void TrackLevelExtraStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelExtraStarted", number, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelStageCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelStageCompleted", score, number, extra1, extra2, tag, context);
        }

        public void TrackLevelStageFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelStageFailed", score, number, extra1, extra2, tag, context);
        }

        public void TrackLevelStageStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackLevelStageStarted", number, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackChunkCompleted(string paramsJson, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackChunkCompleted", paramsJson, tag, context);
        }

        public void TrackChunkFailed(string paramsJson, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackChunkFailed", paramsJson, tag, context);
        }

        public void TrackChunkStarted(string name, int number, string paramsJson, string tag, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackChunkStarted", name, number, paramsJson, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackTutorialCompleted(string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackTutorialCompleted", context);
        }

        public void TrackTutorialStep(string name, string step, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackTutorialStep", name, step, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackItem(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackItem", name, ownedItems, sourceType, paramsJson, context);
        }

        public void TrackItemLoss(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackItemLoss", name, ownedItems, sourceType, paramsJson, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackClick(string screen, string element, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackClick", screen, element, context);
        }

        public void TrackScreen(string screen, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackScreen", screen, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackHardIncome(long amount, long total, string place, string extra, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackHardIncome", amount, total, place, extra, context);
        }

        public void TrackHardOutcome(long amount, long total, string place, string extra, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackHardOutcome", amount, total, place, extra, context);
        }

        public void TrackSoftIncome(long amount, long total, string place, string extra, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackSoftIncome", amount, total, place, extra, context);
        }

        public void TrackSoftOutcome(long amount, long total, string place, string extra, string context = null)
        {
            sayKitUnityBridgeJava.CallStatic("trackSoftOutcome", amount, total, place, extra, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void CheckInAppProduct(string json)
        {
            sayKitUnityBridgeJava.CallStatic("checkInAppProduct", json);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetRequestConfigTimestamp()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getRequestConfigTimestamp");
        }

        public void RequestConfigMigration(string sourceVersion)
        {
            sayKitUnityBridgeJava.CallStatic("requestConfigMigration", sourceVersion);
        }

        public bool RequestRemoteConfigUpdate()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("requestRemoteConfigUpdate");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool ShowCustomRateAppPopup(int rate)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("showCustomRateAppPopup", rate);
        }

        public bool ShowRateAppPopup()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("showRateAppPopup");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsPremium()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isPremium");
        }

        public void DisablePremium()
        {
            sayKitUnityBridgeJava.CallStatic("disablePremium");
        }

        public void EnablePremium()
        {
            sayKitUnityBridgeJava.CallStatic("enablePremium");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool GetGdprStatus()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("getGdprStatus");
        }

        public bool IsGdprApplicable()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isGdprApplicable");
        }

        public void RevokeGdprConsent()
        {
            sayKitUnityBridgeJava.CallStatic("revokeGdprConsent");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetNotificationToken()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getNotificationToken");
        }

        public void RequestNotificationToken()
        {
            sayKitUnityBridgeJava.CallStatic("requestNotificationToken");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetLocalizedString(string key, string val1, string val2, string val3, string val4, string val5)
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getLocalizedString", key, val1, val2, val3, val4, val5);
        }

        public string GetLocalizedTuple(string key, string val1, string val2, string val3, string val4, string val5)
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getLocalizedTuple", key, val1, val2, val3, val4, val5);
        }

        public bool HasLocalizedMessage(string key)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("hasLocalizedMessage", key);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public SayKitLanguage GetCurrentLanguage()
        {
            return SayKitLanguageConverter.ConvertToSayKitLanguage(sayKitUnityBridgeJava.CallStatic<string>("getCurrentLanguage"));
        }

        public string GetFullCurrentLanguage()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getCurrentLanguage");
        }
        
        public (string, CultureInfo) GetFullCurrentLanguageWithCultureInfo()
        {
            var lang = SayKitLanguageConverter.ConvertToIETF(sayKitUnityBridgeJava.CallStatic<string>("getCurrentLanguage"));
            var cultureInfo = SayKitLanguageConverter.GetCultureInfoFromCode(lang);
            
            return (lang, cultureInfo);
        }

        public bool OverrideSystemLanguage(string language)
        {
           return sayKitUnityBridgeJava.CallStatic<bool>("overrideSystemLanguage", language);
        }
        
        public string PrioritizedLanguages()
        {
           return sayKitUnityBridgeJava.CallStatic<string>("getPrioritizedLanguages");
        }
        
        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetFreeMemory()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getFreeMemory");
        }

        public int GetTotalMemory()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getTotalMemory");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetRuntimeInfo()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getRuntimeInfo");
        }

        public string GetAdvertisingId()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getRuntimeInfoAdvertisingId");
        }

        public string GetAppVersion()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getRuntimeInfoAppVersion");
        }

        public string GetDeviceId()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getRuntimeInfoDeviceId");
        }

        public string GetDeviceModel()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getRuntimeInfoDeviceModel");
        }

        public string GetDeviceOs()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getRuntimeInfoDeviceOs");
        }

        public string GetLanguage()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getRuntimeInfoLanguage");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetAppVersionCode()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getAppVersionCode");
        }

        public string GetAppVersionFullName()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getAppVersionFullName");
        }

        public string GetAppVersionOriginalName()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getAppVersionOriginalName");
        }

        public int GetSdkVersionCode()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getSdkVersionCode");
        }

        public string GetSdkVersionName()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getSdkVersionName");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetThermalState()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getThermalState");
        }

        public void ChangeLanguage(int language)
        {
            sayKitUnityBridgeJava.CallStatic("changeLanguage", language);
        }

        public void DisableLogs()
        {
            sayKitUnityBridgeJava.CallStatic("disableLogs");
        }

        public void OverrideTrackLevel(int level)
        {
            sayKitUnityBridgeJava.CallStatic("overrideTrackLevel", level);
        }

        public void ShowMaxMediationDebug()
        {
            sayKitUnityBridgeJava.CallStatic("showMediationDebugger");
        }

        public void ShowSayCatalogue(string placement, string catalogParams = "")
        {
            sayKitUnityBridgeJava.CallStatic("showSayCatalogue", placement, catalogParams);
        }

        public void TrackSayCatalogueOffer(string sourceType)
        {
            sayKitUnityBridgeJava.CallStatic("trackSayCatalogueOffer", sourceType);
        }

        public bool GetRateAppShown()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("getRateAppStatus");
        }

        public string GetATTStatus()
        {
            return string.Empty;
        }

        public void TrackInAppOffer(string place, string extra)
        {
            sayKitUnityBridgeJava.CallStatic("trackInAppOffer", place, extra);
        }

        public void SetPlayerId(string playerId)
        {
            sayKitUnityBridgeJava.CallStatic("setPlayerId", playerId);
        }

        public int GetPlayingTime()
        {
            return sayKitUnityBridgeJava.CallStatic<int>("getPlayingTime");
        }

        public void RequestLiveServer(string requestName, string requestData,
            Action<string> onRequestResult)
        {
            var requestLiveServerCallback = new SayKitBridgeRequestLiveServerCallback
            {
                OnRequestResult = onRequestResult
            };

            sayKitUnityBridgeJava.CallStatic("requestLiveServer",
                requestName, requestData, requestLiveServerCallback);
        }

        public void SetExperimentDeviceId(string deviceId)
        {
            sayKitUnityBridgeJava.CallStatic("setExperimentDeviceId", deviceId);
        }

        public string GetSessionId()
        {
            return sayKitUnityBridgeJava.CallStatic<string>("getSessionId");
        }        
        
        public string GetSystemProxy()
        {
            using (var proxySelector = new AndroidJavaClass("android.net.Proxy"))
            {
                var proxyHost = proxySelector.CallStatic<string>("getDefaultHost");
                var proxyPort = proxySelector.CallStatic<int>("getDefaultPort");

                if (!string.IsNullOrEmpty(proxyHost) && proxyPort > 0)
                {
                    return $"{proxyHost}:{proxyPort}";
                }
            }

            return string.Empty;
        }
        
        public AttributionResponseData GetAttributionData()
        {
            AttributionResponseData attributionResponseData = null;
            
            try
            {
                attributionResponseData = JsonConvert.DeserializeObject<AttributionResponseData>(sayKitUnityBridgeJava.CallStatic<string>("getAttributionData"));
            }
            catch (Exception e)
            {
                var errorMessage = $"[SayKitBridgeIOS][GetAttributionData] message: {e.Message}, stacktrace: {e.StackTrace}";
                SayKitDebug.LogError(errorMessage);
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: errorMessage);
            }

            return attributionResponseData ?? new AttributionResponseData("error", string.Empty, string.Empty);

        }
        
        public void SetGameContext(string gameContext)
        {
            sayKitUnityBridgeJava.CallStatic("setGameContext", gameContext);
        }
        
        public void SetFirebaseUserId(string firebaseUserId)
        {
            sayKitUnityBridgeJava.CallStatic("setFirebaseUID", firebaseUserId);
        }

        public void OpenCustomUrl(string url)
        {
            Application.OpenURL(url);
        }

        #region Billing

        public void GetAvailableProducts(SKProductInfo[] productInfos, Action<SKProduct[], SKBillingError> onAvailableProductsFetched)
        {
            var callback = new SayKitBridgeProductFetchCallback
            {
                OnProductsFetch = onAvailableProductsFetched
            };

            sayKitUnityBridgeJava.CallStatic("fetchProducts", JsonConvert.SerializeObject(productInfos), callback);
        }

        public void GetPurchasedProducts(Action<string[], SKInAppSubscription[], SKBillingError> onPurchasedProductsFetched)
        {
            var callback = new SayKitBridgePurchasedProductsCallback
            {
                OnPurchasedProductsResponse = onPurchasedProductsFetched
            };

            sayKitUnityBridgeJava.CallStatic("getPurchasedProducts", callback);
        }

        public void GetNonConfirmedProducts(Action<SKPurchasedProduct[], SKBillingError> onNonConfirmedProductsFetched)
        {
            var callback = new SayKitBridgeNonConfirmProductsCallback
            {
                OnNonConfirmedProductsFetched = onNonConfirmedProductsFetched
            };

            sayKitUnityBridgeJava.CallStatic("getNonConfirmedProducts", callback);
        }

        public void PurchaseProduct(SKProductInfo productInfo, string offer, string placement, string extra,
            Action<bool, SKPurchasedProduct, SKBillingError> onPurchaseProductCompleted, SKPurchaseOptions options = null)
        {
            var callback = new SayKitBridgePurchasingProductCallback
            {
                OnPurchaseProductCompleted = onPurchaseProductCompleted
            };

            offer ??= string.Empty;
            placement ??= string.Empty;
            extra ??= string.Empty;
            options ??= new SKPurchaseOptions();

            sayKitUnityBridgeJava.CallStatic("purchaseProduct",
                JsonConvert.SerializeObject(productInfo), JsonConvert.SerializeObject(options), offer, placement, extra, callback);
        }

        public void ConfirmPurchase(SKPurchasedProduct purchasedProduct, Action<bool, SKBillingError> onConfirmProductCompleted)
        {
            sayKitUnityBridgeJava.CallStatic("acceptProduct", JsonConvert.SerializeObject(purchasedProduct));
            onConfirmProductCompleted?.Invoke(true, null);
        }

        public void RestorePurchases(Action<string[], SKInAppSubscription[], SKBillingError> onRestorePurchasesCompleted)
        {
            var callback = new SayKitBridgePurchasedProductsCallback
            {
                OnPurchasedProductsResponse = onRestorePurchasesCompleted
            };

            sayKitUnityBridgeJava.CallStatic("restoreProducts", callback);
        }

        public void ShowWebShop(string externalId, string context)
        {
            // not supported
            // sayKitUnityBridgeJava.CallStatic("showWebShop", externalId, context);
        }
        
        public void SetWebShopParams(string externalId, string context)
        {
            // not supported
            // sayKitUnityBridgeJava.CallStatic("setWebShopParams", externalId, context);
        }

        #endregion

        #region Local notification

        public void ScheduleLocalNotification(string json)
        {
            sayKitUnityBridgeJava.CallStatic("scheduleNotification", json);
        }

        public void RemoveAllDeliveredLocalNotifications()
        {
            sayKitUnityBridgeJava.CallStatic("removeAllDeliveredNotifications");
        }
        
        public void RemoveDeliveredLocalNotification(string id)
        {
            sayKitUnityBridgeJava.CallStatic("removeDeliveredNotification", id);
        }
        
        public void RemoveAllScheduledLocalNotifications()
        {
            sayKitUnityBridgeJava.CallStatic("removeAllScheduledNotifications");
        }
        
        public void RemoveScheduledLocalNotification(string id)
        {
            sayKitUnityBridgeJava.CallStatic("removeScheduledNotification", id);
        }

        #endregion
        
        #region InPlay
        
        public bool IsInPlayAvailable()
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("isInPlayAvailable");
        }

        public bool ShowInPlay(int x, int y, int width, int height)
        {
            return sayKitUnityBridgeJava.CallStatic<bool>("showInPlay", x, y, width, height);
        }
    
        public void HideInPlay()
        {
            sayKitUnityBridgeJava.CallStatic("hideInPlay");
        }
        
        public float GetScreenScale()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var resources = activity.Call<AndroidJavaObject>("getResources"))
            using (var dm = resources.Call<AndroidJavaObject>("getDisplayMetrics"))
            {
                return dm.Get<float>("density");
            }
        }
        
        #endregion

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void SetCallbacks(ISayKitBridgeCallbacks listener)
        {
            var callback = new Callback(listener);
            sayKitUnityBridgeJava.CallStatic("setCallback", callback);
        }

        #region Callbacks

        private class SayKitBridgeRequestLiveServerCallback : AndroidJavaProxy
        {
            public Action<string> OnRequestResult { get; set; }

            public SayKitBridgeRequestLiveServerCallback() :
                base("saygames.bridge.unity.SayKitBridgeRequestLiveServerCallback")
            {
            }

            public void invoke(string json)
            {
                SKThreadService.Instance.RunOnMainThread(() => { OnRequestResult?.Invoke(json); });
            }
        }

        private class SayKitBridgePurchasedProductsCallback : AndroidJavaProxy
        {
            public Action<string[], SKInAppSubscription[], SKBillingError> OnPurchasedProductsResponse { get; set; }

            public SayKitBridgePurchasedProductsCallback() : base("saygames.bridge.unity.SayKitStringCallback")
            {
            }

            public void invoke(string json)
            {
                SKThreadService.Instance.RunOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        var purchasedProductsResponse = JsonConvert.DeserializeObject<SKPurchasedProductsInfo>(json);

                        if (purchasedProductsResponse != null)
                        {
                            OnPurchasedProductsResponse?.Invoke(
                                purchasedProductsResponse.ProductIds,
                                purchasedProductsResponse.ActiveSubscriptions,
                                purchasedProductsResponse.Error);
                        }
                        else
                        {
                            OnPurchasedProductsResponse?.Invoke(Array.Empty<string>(), Array.Empty<SKInAppSubscription>(),
                                new SKBillingError("[SayKitBridgePurchaseProductCallback][Error] PurchaseResponse is null."));
                        }
                    }
                    else
                    {
                        OnPurchasedProductsResponse?.Invoke(Array.Empty<string>(), Array.Empty<SKInAppSubscription>(),
                            new SKBillingError("[SayKitBridgePurchaseProductCallback][Error] Invalid json data."));
                    }
                }, e =>
                {
                    var errorMessage = $"[SayKitBridgePurchaseProductCallback][Error] {e.Message}, {e.StackTrace}";
                    SKUtils.HandleError(errorMessage);
                    OnPurchasedProductsResponse?.Invoke(Array.Empty<string>(), Array.Empty<SKInAppSubscription>(), new SKBillingError(errorMessage));
                });
            }
        }

        private class SayKitBridgePurchasingProductCallback : AndroidJavaProxy
        {
            public Action<bool, SKPurchasedProduct, SKBillingError> OnPurchaseProductCompleted { get; set; }

            public SayKitBridgePurchasingProductCallback() : base("saygames.bridge.unity.SayKitStringCallback")
            {
            }

            public void invoke(string json)
            {
                SKThreadService.Instance.RunOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        var purchaseResponse = JsonConvert.DeserializeObject<SKPurchaseResponse>(json);
                        if (purchaseResponse != null)
                        {
                            OnPurchaseProductCompleted(
                                purchaseResponse.Success,
                                purchaseResponse.PurchasedProduct,
                                purchaseResponse.Error);
                        }
                        else
                        {
                            OnPurchaseProductCompleted(false, null,
                                new SKBillingError("[SayKitBridgePurchasingProductCallback][Error] PurchaseResponse is null."));
                        }
                    }
                    else
                    {
                        OnPurchaseProductCompleted(false, null,
                            new SKBillingError("[SayKitBridgePurchasingProductCallback][Error] Invalid json data."));
                    }
                }, e =>
                {
                    var errorMessage = $"[SayKitBridgePurchasingProductCallback][Error] {e.Message}, {e.StackTrace}";
                    SKUtils.HandleError(errorMessage);
                    OnPurchaseProductCompleted(false, null, new SKBillingError(errorMessage));
                });
            }
        }

        private class SayKitBridgeNonConfirmProductsCallback : AndroidJavaProxy
        {
            public Action<SKPurchasedProduct[], SKBillingError> OnNonConfirmedProductsFetched { get; set; }

            public SayKitBridgeNonConfirmProductsCallback() : base("saygames.bridge.unity.SayKitStringCallback")
            {
            }

            public void invoke(string json)
            {
                SKThreadService.Instance.RunOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        var purchaseProductsInfo = JsonConvert.DeserializeObject<SKPurchasedProduct[]>(json);

                        if (purchaseProductsInfo != null)
                        {
                            OnNonConfirmedProductsFetched?.Invoke(purchaseProductsInfo, null);
                        }
                        else
                        {
                            OnNonConfirmedProductsFetched?.Invoke(null,
                                new SKBillingError("[SayKitBridgeProductFetchCallback][Error] FetchProduct is null."));
                        }
                    }
                    else
                    {
                        OnNonConfirmedProductsFetched?.Invoke(null,
                            new SKBillingError("[SayKitBridgeProductFetchCallback][Error] Invalid json data."));
                    }
                }, e =>
                {
                    var errorMessage = $"[SayKitBridgeProductFetchCallback][invoke] {e.Message}, {e.StackTrace}";
                    SKUtils.HandleError(errorMessage);
                    OnNonConfirmedProductsFetched?.Invoke(null, new SKBillingError(errorMessage));
                });
            }
        }

        private class SayKitBridgeProductFetchCallback : AndroidJavaProxy
        {
            public Action<SKProduct[], SKBillingError> OnProductsFetch { get; set; }

            public SayKitBridgeProductFetchCallback() : base("saygames.bridge.unity.SayKitStringCallback")
            {
            }

            public void invoke(string json)
            {
                SKThreadService.Instance.RunOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        var fetchProduct = JsonConvert.DeserializeObject<SKFetchProduct>(json);
                        if (fetchProduct != null)
                        {
                            OnProductsFetch?.Invoke(fetchProduct.Products, fetchProduct.Error);
                        }
                        else
                        {
                            OnProductsFetch?.Invoke(Array.Empty<SKProduct>(),
                                new SKBillingError("[SayKitBridgeProductFetchCallback][Error] FetchProduct is null."));
                        }
                    }
                    else
                    {
                        OnProductsFetch?.Invoke(Array.Empty<SKProduct>(),
                            new SKBillingError("[SayKitBridgeProductFetchCallback][Error] Invalid json data."));
                    }
                }, e =>
                {
                    var errorMessage = $"[SayKitBridgeProductFetchCallback][Error] {e.Message}, {e.StackTrace}";
                    SKUtils.HandleError(errorMessage);
                    OnProductsFetch?.Invoke(Array.Empty<SKProduct>(), new SKBillingError(errorMessage));
                });
            }
        }

        private class Callback : AndroidJavaProxy
        {
            private readonly ISayKitBridgeCallbacks listener;

            internal Callback(ISayKitBridgeCallbacks listener) : base("saygames.bridge.unity.SayKitBridgeCallback")
            {
                this.listener = listener;
            }

            private void onAdShown(string type, string time, string network, string creativeId)
            {
                listener?.OnAdShown(type, time, network, creativeId);
            }

            private void onAdDisplayed(string type, string adInfo)
            {
                listener?.OnAdDisplayed(type, adInfo);
            }

            private void onAdRevenuePaid(string type, string adRevenueInfo)
            {
                listener?.OnAdRevenuePaid(type, adRevenueInfo);
            }

            private void onInAppProductChecked(string json)
            {
                listener?.OnInAppProductChecked(json);
            }

            private void onInitStateChanged(int number, float progress)
            {
                listener?.OnInitStateChanged(number, progress);
            }

            private void onInterstitialClosed()
            {
                listener?.OnInterstitialClosed();
            }

            private void onNotificationTokenReceived(string token)
            {
                listener?.OnNotificationTokenReceived(token);
            }

            private void onRemoteConfigUpdated(string path)
            {
                listener?.OnRemoteConfigUpdated(path);
            }

            private void onRewardedClosed(bool rewarded)
            {
                listener?.OnRewardedClosed(rewarded);
            }

            private void onSayCatalogueShown(string placement)
            {
                listener?.OnSayCatalogueShown(placement);
            }

            private void onLocalizationsUpdated(string data)
            {
                listener?.OnLocalizationsUpdated(data);
            }
            
            private void onDeepLinkReceived(string data)
            {
                listener?.OnDeepLinkReceived(data);
            }
            
            private void onSupportResult(bool result)
            {
                listener?.OnSupportRequestSubmitted(result);
            }

            private void onOfferwallRewardReceived(string json)
            {
                listener?.OnOfferwallRewardReceived(json);
            }

            private void onNonConfirmedPurchaseReceived()
            {
                listener?.OnNonConfirmedPurchaseReceived();
            }
        }

        #endregion
    }
}
#endif