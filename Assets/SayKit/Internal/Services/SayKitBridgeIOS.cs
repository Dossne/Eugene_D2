#if UNITY_IOS

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using AOT;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable RedundantCast
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    public class SayKitBridgeIOS
    {
        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern bool Native_isInitialized();

        public bool IsInitialized()
        {
            return Native_isInitialized();
        }

        [DllImport("__Internal")]
        private static extern float Native_getInitStateProgress();

        public float GetInitStateProgress()
        {
            return Native_getInitStateProgress();
        }

        [DllImport("__Internal")]
        private static extern int Native_getInitStateNumber();

        public int GetInitStateNumber()
        {
            return Native_getInitStateNumber();
        }

        [DllImport("__Internal")]
        private static extern void Native_initIfNeeded(string configJson, string environmentParams);

        public void InitIfNeeded(string configJson, string environmentParams)
        {
            Native_initIfNeeded(configJson, environmentParams);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern float Native_getBackgroundBannerSize();

        public float GetBackgroundBannerSize()
        {
            return Native_getBackgroundBannerSize();
        }

        [DllImport("__Internal")]
        private static extern void Native_hideBanner();

        public void HideBanner()
        {
            Native_hideBanner();
        }

        [DllImport("__Internal")]
        private static extern void Native_showBanner();

        public void ShowBanner()
        {
            Native_showBanner();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern bool Native_isInterstitialAvailable(string place, int countdown);

        public bool IsInterstitialAvailable(string place, int countdown)
        {
            return Native_isInterstitialAvailable(place, countdown);
        }
        
        [DllImport("__Internal")]
        private static extern bool Native_isRewardedAvailable(string place);

        public bool IsRewardedAvailable(string place)
        {
            return Native_isRewardedAvailable(place);
        }

        [DllImport("__Internal")]
        private static extern bool Native_isRewardedPlacementAvailable(string place);

        public bool IsRewardedPlacementAvailable(string place)
        {
            return Native_isRewardedPlacementAvailable(place);
        }

        [DllImport("__Internal")]
        private static extern bool Native_showInterstitial(string place, int countdown);

        public bool ShowInterstitial(string place, int countdown)
        {
            return Native_showInterstitial(place, countdown);
        }

        [DllImport("__Internal")]
        private static extern bool Native_showRewarded(string place);

        public bool ShowRewarded(string place)
        {
            return Native_showRewarded(place);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackInterstitialOffer(string place, string extra);

        public void TrackInterstitialOffer(string place, string extra)
        {
            Native_trackInterstitialOffer(place, extra);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackRewardedOffer(string place, string extra);

        public void TrackRewardedOffer(string place, string extra)
        {
            Native_trackRewardedOffer(place, extra);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern int Native_getSubscriptionExpirationTimestamp();

        public int GetSubscriptionExpirationTimestamp()
        {
            return Native_getSubscriptionExpirationTimestamp();
        }

        [DllImport("__Internal")]
        private static extern bool Native_isAppStoreAvailable();

        public bool IsAppStoreAvailable()
        {
            return Native_isAppStoreAvailable();
        }

        [DllImport("__Internal")]
        private static extern string Native_getPrivacyPolicyLink();

        public string GetPrivacyPolicyLink()
        {
            return Native_getPrivacyPolicyLink();
        }

        [DllImport("__Internal")]
        private static extern bool Native_isAppInstalled(string packageName);

        public bool IsAppInstalled(string packageName)
        {
            return Native_isAppInstalled(packageName);
        }

        public void OpenGooglePlaySubscriptionCenter(string productId)
        {
            // Android only
        }

        [DllImport("__Internal")]
        private static extern void Native_openStoreProductView(int storeId, string skadData, string storeUrl);

        public void OpenStoreProductView(int storeId, string skadData, string storeUrl)
        {
            Native_openStoreProductView(storeId, skadData, storeUrl);
        }

        [DllImport("__Internal")]
        private static extern string Native_openSupportPage(string extra);

        public string OpenSupportPage(string extra)
        {
            return Native_openSupportPage(extra);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsFacebookSdkInitialized()
        {
            // Android only
            return true;
        }

        [DllImport("__Internal")]
        private static extern void Native_logFacebookEvent(string name, float valueToSum, string paramsJson);

        public void LogFacebookEvent(string name, float valueToSum, string paramsJson)
        {
            Native_logFacebookEvent(name, valueToSum, paramsJson);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_logFirebaseEvent(string name, float valueToSum, string paramsJson);

        public void LogFirebaseEvent(string name, float valueToSum, string paramsJson)
        {
            Native_logFirebaseEvent(name, valueToSum, paramsJson);
        }

        [DllImport("__Internal")]
        private static extern void Native_setFirebaseUserProperty(string key, string value);

        public void SetFirebaseUserProperty(string key, string value)
        {
            Native_setFirebaseUserProperty(key, value);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_logCrashlytics(string message);

        public void LogCrashlytics(string message)
        {
            Native_logCrashlytics(message);
        }

        [DllImport("__Internal")]
        private static extern void Native_logCrashlyticsException(string error);

        public void LogCrashlyticsException(Exception exception)
        {
            var exceptionMessage = "";

            if (exception.Message.Length > 0)
            {
                exceptionMessage += "Exception message: " + exception.Message;
            }

            if (exception.StackTrace.Length > 0)
            {
                exceptionMessage += "\n" + "Exception stacktrace: " + exception.StackTrace;
            }

            Native_logCrashlyticsException(exceptionMessage);
        }

        [DllImport("__Internal")]
        private static extern void Native_setCrashlyticsParam(string key, string value);

        public void SetCrashlyticsParam(string key, string value)
        {
            Native_setCrashlyticsParam(key, value);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackApplicationLoaded();

        public void TrackApplicationLoaded()
        {
            Native_trackApplicationLoaded();
        }

        [DllImport("__Internal")]
        private static extern void Native_trackAvailableMemory();

        public void TrackAvailableMemory()
        {
            Native_trackAvailableMemory();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackEvent(string name, long param1, long param2, long param3, long param4, string extra1, string extra2,
            string tag, string context);

        public void TrackEvent(string name, long param1 = 0, long param2 = 0, long param3 = 0, long param4 = 0, string extra1 = "",
            string extra2 = "", string tag = "", string context = null)
        {
            Native_trackEvent(name, param1, param2, param3, param4, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackLevelCompleted(int level, long score, int number, string extra1, string extra2, string tag,
            string context);

        public void TrackLevelCompleted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelCompleted(level, score, number, extra1, extra2, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackLevelFailed(int level, long score, int number, string extra1, string extra2, string tag,
            string context);

        public void TrackLevelFailed(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelFailed(level, score, number, extra1, extra2, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackLevelStarted(int level, long score, int number, string extra1, string extra2, string tag,
            string context);

        public void TrackLevelStarted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelStarted(level, score, number, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackLevelExtraCompleted(long score, int number, string extra1, string extra2, string tag, string context);

        public void TrackLevelExtraCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelExtraCompleted(score, number, extra1, extra2, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackLevelExtraFailed(long score, int number, string extra1, string extra2, string tag, string context);

        public void TrackLevelExtraFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelExtraFailed(score, number, extra1, extra2, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackLevelExtraStarted(int number, string extra1, string extra2, string tag, string context);

        public void TrackLevelExtraStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelExtraStarted(number, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackLevelStageCompleted(long score, int number, string extra1, string extra2, string tag, string context);

        public void TrackLevelStageCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelStageCompleted(score, number, extra1, extra2, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackLevelStageFailed(long score, int number, string extra1, string extra2, string tag, string context);

        public void TrackLevelStageFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelStageFailed(score, number, extra1, extra2, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackLevelStageStarted(int number, string extra1, string extra2, string tag, string context);

        public void TrackLevelStageStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            Native_trackLevelStageStarted(number, extra1, extra2, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackChunkCompleted(string paramsJson, string tag, string context);

        public void TrackChunkCompleted(string paramsJson, string tag, string context = null)
        {
            Native_trackChunkCompleted(paramsJson, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackChunkFailed(string paramsJson, string tag, string context);

        public void TrackChunkFailed(string paramsJson, string tag, string context = null)
        {
            Native_trackChunkFailed(paramsJson, tag, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackChunkStarted(string name, int number, string paramsJson, string tag, string context);

        public void TrackChunkStarted(string name, int number, string paramsJson, string tag, string context = null)
        {
            Native_trackChunkStarted(name, number, paramsJson, tag, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackTutorialCompleted(string context);

        public void TrackTutorialCompleted(string context = null)
        {
            Native_trackTutorialCompleted(context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackTutorialStep(string name, string step, string context);

        public void TrackTutorialStep(string name, string step, string context = null)
        {
            Native_trackTutorialStep(name, step, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackItem(string name, int ownedItems, int sourceType, string paramsJson, string context);

        public void TrackItem(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            Native_trackItem(name, ownedItems, sourceType, paramsJson, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackItemLoss(string name, int ownedItems, int sourceType, string paramsJson, string context);

        public void TrackItemLoss(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            Native_trackItemLoss(name, ownedItems, sourceType, paramsJson, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackClick(string screen, string element, string context);

        public void TrackClick(string screen, string element, string context = null)
        {
            Native_trackClick(screen, element, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackScreen(string screen, string context);

        public void TrackScreen(string screen, string context = null)
        {
            Native_trackScreen(screen, context);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_trackHardIncome(long amount, long total, string place, string extra, string context);

        public void TrackHardIncome(long amount, long total, string place, string extra, string context = null)
        {
            Native_trackHardIncome(amount, total, place, extra, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackHardOutcome(long amount, long total, string place, string extra, string context);

        public void TrackHardOutcome(long amount, long total, string place, string extra, string context = null)
        {
            Native_trackHardOutcome(amount, total, place, extra, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackSoftIncome(long amount, long total, string place, string extra, string context);

        public void TrackSoftIncome(long amount, long total, string place, string extra, string context = null)
        {
            Native_trackSoftIncome(amount, total, place, extra, context);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackSoftOutcome(long amount, long total, string place, string extra, string context);

        public void TrackSoftOutcome(long amount, long total, string place, string extra, string context = null)
        {
            Native_trackSoftOutcome(amount, total, place, extra, context);
        }


        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern void Native_checkInAppProduct(string json);

        public void CheckInAppProduct(string json)
        {
            Native_checkInAppProduct(json);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern int Native_getRequestConfigTimestamp();

        public int GetRequestConfigTimestamp()
        {
            return Native_getRequestConfigTimestamp();
        }

        [DllImport("__Internal")]
        private static extern void Native_requestConfigMigration(string sourceVersion);

        public void RequestConfigMigration(string sourceVersion)
        {
            Native_requestConfigMigration(sourceVersion);
        }

        [DllImport("__Internal")]
        private static extern bool Native_requestRemoteConfigUpdate();

        public bool RequestRemoteConfigUpdate()
        {
            return Native_requestRemoteConfigUpdate();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern bool Native_showCustomRateAppPopup(int rate);

        public bool ShowCustomRateAppPopup(int rate)
        {
            return Native_showCustomRateAppPopup(rate);
        }

        [DllImport("__Internal")]
        private static extern bool Native_showRateAppPopup();

        public bool ShowRateAppPopup()
        {
            return Native_showRateAppPopup();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern bool Native_isPremium();

        public bool IsPremium()
        {
            return Native_isPremium();
        }

        [DllImport("__Internal")]
        private static extern void Native_disablePremium();

        public void DisablePremium()
        {
            Native_disablePremium();
        }

        [DllImport("__Internal")]
        private static extern void Native_enablePremium();

        public void EnablePremium()
        {
            Native_enablePremium();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern bool Native_getGdprStatus();

        public bool GetGdprStatus()
        {
            return Native_getGdprStatus();
        }

        [DllImport("__Internal")]
        private static extern bool Native_isGdprApplicable();

        public bool IsGdprApplicable()
        {
            return Native_isGdprApplicable();
        }

        [DllImport("__Internal")]
        private static extern void Native_revokeGdprConsent();

        public void RevokeGdprConsent()
        {
            Native_revokeGdprConsent();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern string Native_getNotificationToken();

        public string GetNotificationToken()
        {
            return Native_getNotificationToken();
        }

        [DllImport("__Internal")]
        private static extern void Native_requestNotificationToken();

        public void RequestNotificationToken()
        {
            Native_requestNotificationToken();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern string Native_getCurrentLanguage();

        public SayKitLanguage GetCurrentLanguage()
        {
            return SayKitLanguageConverter.ConvertToSayKitLanguage(Native_getCurrentLanguage());
        }

        public string GetFullCurrentLanguage()
        {
            return Native_getCurrentLanguage();
        }

        public (string, CultureInfo) GetFullCurrentLanguageWithCultureInfo()
        {
            var lang = SayKitLanguageConverter.ConvertToIETF(Native_getCurrentLanguage());
            var cultureInfo = SayKitLanguageConverter.GetCultureInfoFromCode(lang);

            return (lang, cultureInfo);
        }

        [DllImport("__Internal")]
        private static extern bool Native_overrideSystemLanguage(string language);

        public bool OverrideSystemLanguage(string language)
        {
            return Native_overrideSystemLanguage(language);
        }

        [DllImport("__Internal")]
        private static extern string Native_prioritizedLanguages();

        public string PrioritizedLanguages()
        {
            return Native_prioritizedLanguages();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern int Native_getFreeMemory();

        public int GetFreeMemory()
        {
            return Native_getFreeMemory();
        }

        [DllImport("__Internal")]
        private static extern int Native_getTotalMemory();

        public int GetTotalMemory()
        {
            return Native_getTotalMemory();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern string Native_getRuntimeInfo();

        public string GetRuntimeInfo()
        {
            return Native_getRuntimeInfo();
        }

        [DllImport("__Internal")]
        private static extern string Native_getAdvertisingId();

        public string GetAdvertisingId()
        {
            return Native_getAdvertisingId();
        }

        [DllImport("__Internal")]
        private static extern string Native_getAppVersion();

        public string GetAppVersion()
        {
            return Native_getAppVersion();
        }

        [DllImport("__Internal")]
        private static extern string Native_getDeviceId();

        public string GetDeviceId()
        {
            return Native_getDeviceId();
        }

        [DllImport("__Internal")]
        private static extern string Native_getDeviceModel();

        public string GetDeviceModel()
        {
            return Native_getDeviceModel();
        }

        [DllImport("__Internal")]
        private static extern string Native_getDeviceOs();

        public string GetDeviceOs()
        {
            return Native_getDeviceOs();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern int Native_getAppVersionCode();

        public int GetAppVersionCode()
        {
            return Native_getAppVersionCode();
        }

        [DllImport("__Internal")]
        private static extern string Native_getAppVersionFullName();

        public string GetAppVersionFullName()
        {
            return Native_getAppVersionFullName();
        }

        [DllImport("__Internal")]
        private static extern string Native_getAppVersionOriginalName();

        public string GetAppVersionOriginalName()
        {
            return Native_getAppVersionOriginalName();
        }

        [DllImport("__Internal")]
        private static extern int Native_getSdkVersionCode();

        public int GetSdkVersionCode()
        {
            return Native_getSdkVersionCode();
        }

        [DllImport("__Internal")]
        private static extern string Native_getSdkVersionName();

        public string GetSdkVersionName()
        {
            return Native_getSdkVersionName();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        [DllImport("__Internal")]
        private static extern string Native_getThermalState();

        public string GetThermalState()
        {
            return Native_getThermalState();
        }

        [DllImport("__Internal")]
        private static extern void Native_disableLogs();

        public void DisableLogs()
        {
            Native_disableLogs();
        }

        [DllImport("__Internal")]
        private static extern void Native_overrideTrackLevel(int level);

        public void OverrideTrackLevel(int level)
        {
            Native_overrideTrackLevel(level);
        }

        [DllImport("__Internal")]
        private static extern void Native_showMediationDebugger();

        public void ShowMaxMediationDebug()
        {
            Native_showMediationDebugger();
        }

        [DllImport("__Internal")]
        private static extern void Native_showSayCatalogue(string placement, string catalogParams);

        public void ShowSayCatalogue(string placement, string catalogParams = "")
        {
            Native_showSayCatalogue(placement, catalogParams);
        }

        [DllImport("__Internal")]
        private static extern void Native_trackSayCatalogueOffer(string sourceType);

        public void TrackSayCatalogueOffer(string sourceType)
        {
            Native_trackSayCatalogueOffer(sourceType);
        }

        [DllImport("__Internal")]
        private static extern string Native_getATTStatus();

        public string GetATTStatus()
        {
            return Native_getATTStatus();
        }

        [DllImport("__Internal")]
        private static extern bool Native_rateAppShown();

        public bool GetRateAppShown()
        {
            return Native_rateAppShown();
        }

        [DllImport("__Internal")]
        private static extern void Native_trackInAppOffer(string place, string extra);

        public void TrackInAppOffer(string place, string extra)
        {
            Native_trackInAppOffer(place, extra);
        }

        [DllImport("__Internal")]
        private static extern bool Native_openSystemSettings();

        public void OpenSystemSettings()
        {
            Native_openSystemSettings();
        }

        [DllImport("__Internal")]
        private static extern void Native_setPlayerId(string playerId);

        public void SetPlayerId(string playerId)
        {
            Native_setPlayerId(playerId);
        }

        [DllImport("__Internal")]
        private static extern int Native_getPlayingTime();

        public int GetPlayingTime()
        {
            return Native_getPlayingTime();
        }

        private static List<(string requestName, Action<string> action)> _requestList =
            new List<(string requestName, Action<string> action)>();


        [DllImport("__Internal")]
        private static extern void Native_requestLiveServer(string requestName, string requestData,
            OnLiveRequestResponse onLiveRequestResponse);

        private delegate void OnLiveRequestResponse(string requestName, string json);

        [MonoPInvokeCallback(typeof(OnLiveRequestResponse))]
        private static void _OnLiveRequestResponse(string requestName, string json)
        {
            try
            {
                foreach (var requestItem in _requestList)
                {
                    if (requestItem.requestName == requestName)
                    {
                        requestItem.action.Invoke(json);
                        _requestList.Remove(requestItem);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                var errorMessage =
                    $"[SayKitBridgeIOS][RequestLiveServer] message: {e.Message}, stacktrace: {e.StackTrace}";
                SayKitDebug.LogError(errorMessage);
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: errorMessage);
            }
        }

        public void RequestLiveServer(string requestName, string requestData,
            Action<string> onRequestResult)
        {
            try
            {
                _requestList.Add((requestName, onRequestResult));
                Native_requestLiveServer(requestName, requestData, _OnLiveRequestResponse);
            }
            catch (Exception e)
            {
                var errorMessage =
                    $"[SayKitBridgeIOS][RequestLiveServer] message: {e.Message}, stacktrace: {e.StackTrace}";
                SayKitDebug.LogError(errorMessage);
                TrackEvent("sk_unity_exception", extra1: errorMessage);
            }
        }

        [DllImport("__Internal")]
        private static extern void Native_setExperimentDeviceId(string deviceId);

        public void SetExperimentDeviceId(string deviceId)
        {
            Native_setExperimentDeviceId(deviceId);
        }

        [DllImport("__Internal")]
        private static extern string Native_getSessionId();

        public string GetSessionId()
        {
            return Native_getSessionId();
        }

        [DllImport("__Internal")]
        private static extern string Native_getSystemProxy();

        public string GetSystemProxy()
        {
            return Native_getSystemProxy();
        }

        [DllImport("__Internal")]
        private static extern string Native_getAttributionData();

        public AttributionResponseData GetAttributionData()
        {
            AttributionResponseData attributionResponseData = null;

            try
            {
                attributionResponseData = JsonConvert.DeserializeObject<AttributionResponseData>(Native_getAttributionData());
            }
            catch (Exception e)
            {
                var errorMessage = $"[SayKitBridgeIOS][GetAttributionData] message: {e.Message}, stacktrace: {e.StackTrace}";
                SayKitDebug.LogError(errorMessage);
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: errorMessage);
            }

            return attributionResponseData ?? new AttributionResponseData("error", string.Empty, string.Empty);
        }

        [DllImport("__Internal")]
        private static extern void Native_setGameContext(string gameContext);

        public void SetGameContext(string gameContext)
        {
            Native_setGameContext(gameContext);
        }

        [DllImport("__Internal")]
        private static extern void Native_setFirebaseUserId(string firebaseUserId);

        public void SetFirebaseUserId(string firebaseUserId)
        {
            Native_setFirebaseUserId(firebaseUserId);
        }

        [DllImport("__Internal")]
        private static extern void CustomOpenURL(string url);

        public void OpenCustomUrl(string url)
        {
            CustomOpenURL(url);
        }

        #region Billing
        
        private static Dictionary<string, Action<SKProduct[], SKBillingError>>
            _fetchProductDictionary = new Dictionary<string, Action<SKProduct[], SKBillingError>>();

        [DllImport("__Internal")]
        private static extern void Native_fetchProducts(string timeStamp, string products, OnFetchProduct onProductsFetch);

        private delegate void OnFetchProduct(string timeStamp, string json);

        [MonoPInvokeCallback(typeof(OnFetchProduct))]
        private static void _OnFetchProduct(string timeStamp, string json)
        {
            if (!_fetchProductDictionary.TryGetValue(timeStamp, out var fetchProductItem))
            {
                SKUtils.HandleError($"[SayKitBridgeiOS][OnFetchProduct][Error] Key '{timeStamp}' not found in dictionary.");
                return;
            }

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(json))
                {
                    var fetchProduct = JsonConvert.DeserializeObject<SKFetchProduct>(json);
                    if (fetchProduct != null)
                    {
                        fetchProductItem(fetchProduct.Products, fetchProduct.Error);
                    }
                    else
                    {
                        fetchProductItem(Array.Empty<SKProduct>(),
                            new SKBillingError("[SayKitBridgeiOS][OnFetchProduct][Error] FetchProduct is null."));
                    }
                }
                else
                {
                    fetchProductItem(Array.Empty<SKProduct>(), new SKBillingError("[SayKitBridgeiOS][OnFetchProduct][Error] Invalid json data."));
                }
            }, e =>
            {
                var errorMessage = $"[SayKitBridgeiOS][OnFetchProduct] message: {e.Message}, stacktrace: {e.StackTrace}";
                SKUtils.HandleError(errorMessage);
                fetchProductItem(Array.Empty<SKProduct>(), new SKBillingError(errorMessage));
            }, () => { _fetchProductDictionary?.Remove(timeStamp); });
        }

        public void GetAvailableProducts(SKProductInfo[] productInfos, Action<SKProduct[], SKBillingError> onAvailableProductsFetched)
        {
            var timeStamp = SKUtils.currentTimestamp.ToString();
            _fetchProductDictionary?.Add(timeStamp, onAvailableProductsFetched);
            Native_fetchProducts(timeStamp, JsonConvert.SerializeObject(productInfos), _OnFetchProduct);
        }

        private static Dictionary<string, Action<SKPurchasedProduct[], SKBillingError>> _nonConfirmedProductsDictionary =
            new Dictionary<string, Action<SKPurchasedProduct[], SKBillingError>>();

        [DllImport("__Internal")]
        private static extern void Native_getNonConfirmedProducts(string timeStamp, OnNonConfirmedProducts onNonConfirmedProducts);

        private delegate void OnNonConfirmedProducts(string timeStamp, string json);

        [MonoPInvokeCallback(typeof(OnNonConfirmedProducts))]
        private static void _OnNonConfirmedProducts(string timeStamp, string json)
        {
            if (!_nonConfirmedProductsDictionary.TryGetValue(timeStamp, out var nonConfirmedProductsItem))
            {
                SKUtils.HandleError($"[SayKitBridgeiOS][OnNonConfirmedProducts][Error] Key '{timeStamp}' not found in dictionary.");
                return;
            }

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(json))
                {
                    var purchaseProductsInfo = JsonConvert.DeserializeObject<SKPurchasedProduct[]>(json);

                    if (purchaseProductsInfo != null)
                    {
                        nonConfirmedProductsItem(purchaseProductsInfo, null);
                    }
                    else
                    {
                        nonConfirmedProductsItem(null,
                            new SKBillingError("[SayKitBridgeIOS][OnNonConfirmedProducts][Error] PurchaseProductsInfo is null."));
                    }
                }
                else
                {
                    nonConfirmedProductsItem(null,
                        new SKBillingError("[SayKitBridgeIOS][OnNonConfirmedProducts][Error] Invalid json data."));
                }
            }, e =>
            {
                var errorMessage = $"[SayKitBridgeIOS][OnNonConfirmedProducts] message: {e.Message}, stacktrace: {e.StackTrace}";
                SKUtils.HandleError(errorMessage);
                nonConfirmedProductsItem(null, new SKBillingError(errorMessage));
            }, () => { _nonConfirmedProductsDictionary?.Remove(timeStamp); });
        }

        public void GetNonConfirmedProducts(Action<SKPurchasedProduct[], SKBillingError> onNonConfirmedProductsFetched)
        {
            var timeStamp = SKUtils.currentTimestamp.ToString();
            _nonConfirmedProductsDictionary?.Add(timeStamp, onNonConfirmedProductsFetched);
            Native_getNonConfirmedProducts(timeStamp, _OnNonConfirmedProducts);
        }

        private static Dictionary<string, Action<string[], SKInAppSubscription[], SKBillingError>> _purchasedProductsDictionary =
            new Dictionary<string, Action<string[], SKInAppSubscription[], SKBillingError>>();

        [DllImport("__Internal")]
        private static extern void Native_getPurchasedProducts(string timeStamp, OnPurchasedProducts onPurchasedProducts);

        private delegate void OnPurchasedProducts(string timeStamp, string json);

        [MonoPInvokeCallback(typeof(OnPurchasedProducts))]
        private static void _OnGetPurchasedProducts(string timeStamp, string json)
        {
            if (!_purchasedProductsDictionary.TryGetValue(timeStamp, out var purchasedProductsItem))
            {
                SKUtils.HandleError($"[SayKitBridgeiOS][OnGetPurchasedProducts][Error] Key '{timeStamp}' not found in dictionary.");
                return;
            }

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(json))
                {
                    var purchaseProductsInfo = JsonConvert.DeserializeObject<SKPurchasedProductsInfo>(json);
                    if (purchaseProductsInfo != null)
                    {
                        purchasedProductsItem(purchaseProductsInfo.ProductIds, purchaseProductsInfo.ActiveSubscriptions, purchaseProductsInfo.Error);
                    }
                    else
                    {
                        purchasedProductsItem(Array.Empty<string>(), null,
                            new SKBillingError("[SayKitBridgeIOS][OnGetPurchasedProducts][Error] PurchaseProductsInfo is null."));
                    }
                }
                else
                {
                    purchasedProductsItem(Array.Empty<string>(), null,
                        new SKBillingError("[SayKitBridgeIOS][OnGetPurchasedProducts][Error] Invalid json data."));
                }
            }, e =>
            {
                var errorMessage = $"[SayKitBridgeIOS][OnGetPurchasedProducts][Error] message: {e.Message}, stacktrace: {e.StackTrace}";
                SKUtils.HandleError(errorMessage);
                purchasedProductsItem(Array.Empty<string>(), null, new SKBillingError(errorMessage));
            }, () => { _purchasedProductsDictionary?.Remove(timeStamp); });
        }

        public void GetPurchasedProducts(Action<string[], SKInAppSubscription[], SKBillingError> onPurchasedProductsFetched)
        {
            var timeStamp = SKUtils.currentTimestamp.ToString();
            _purchasedProductsDictionary?.Add(timeStamp, onPurchasedProductsFetched);
            Native_getPurchasedProducts(timeStamp, _OnGetPurchasedProducts);
        }

        private static Dictionary<string, Action<bool, SKPurchasedProduct, SKBillingError>> _purchaseProductDictionary =
            new Dictionary<string, Action<bool, SKPurchasedProduct, SKBillingError>>();

        [DllImport("__Internal")]
        private static extern void Native_purchaseProduct(string timeStamp, string productInfo, string options, string offer, string placement, string extra,
            OnPurchaseProduct onPurchaseProduct);

        private delegate void OnPurchaseProduct(string timeStamp, string json);

        [MonoPInvokeCallback(typeof(OnPurchaseProduct))]
        private static void _OnPurchaseProduct(string timeStamp, string json)
        {
            if (!_purchaseProductDictionary.TryGetValue(timeStamp, out var purchaseProductItem))
            {
                SKUtils.HandleError(
                    $"[SayKitBridgeiOS][OnPurchaseProduct][Error] Key '{timeStamp}' not found in dictionary.");
                return;
            }

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(json))
                {
                    var purchaseResponse = JsonConvert.DeserializeObject<SKPurchaseResponse>(json);
                    if (purchaseResponse != null)
                    {
                        purchaseProductItem(purchaseResponse.Success, purchaseResponse.PurchasedProduct, purchaseResponse.Error);
                    }
                    else
                    {
                        purchaseProductItem(false, null,
                            new SKBillingError("[SayKitBridgeiOS][OnPurchaseProduct][Error] PurchaseResponse is null."));
                    }
                }
                else
                {
                    purchaseProductItem(false, null, new SKBillingError("[SayKitBridgeiOS][OnPurchaseProduct][Error] Invalid json data."));
                }
            }, e =>
            {
                var errorMessage = $"[SayKitBridgeiOS][OnPurchaseProduct][Error] {e.Message}, {e.StackTrace}";
                SKUtils.HandleError(errorMessage);
                purchaseProductItem(false, null, new SKBillingError(errorMessage));
            }, () => { _purchaseProductDictionary?.Remove(timeStamp); });
        }

        public void PurchaseProduct(SKProductInfo productInfo, SKPurchaseOptions options, string offer, string placement, string extra,
            Action<bool, SKPurchasedProduct, SKBillingError> onPurchaseProductCompleted)
        {
            var timeStamp = SKUtils.currentTimestamp.ToString();
            _purchaseProductDictionary?.Add(timeStamp, onPurchaseProductCompleted);

            offer ??= string.Empty;
            placement ??= string.Empty;
            extra ??= string.Empty;
            options ??= new SKPurchaseOptions();

            Native_purchaseProduct(timeStamp, JsonConvert.SerializeObject(productInfo), JsonConvert.SerializeObject(options), offer, placement, extra,
                _OnPurchaseProduct);
        }

        private static Dictionary<string, Action<string[], SKInAppSubscription[], SKBillingError>> _restoreProductsDictionary =
            new Dictionary<string, Action<string[], SKInAppSubscription[], SKBillingError>>();

        [DllImport("__Internal")]
        private static extern void Native_restoreProducts(string timeStamp, OnRestorePurchases onRestorePurchases);

        private delegate void OnRestorePurchases(string timeStamp, string json);

        [MonoPInvokeCallback(typeof(OnRestorePurchases))]
        private static void _OnRestorePurchases(string timeStamp, string json)
        {
            if (!_restoreProductsDictionary.TryGetValue(timeStamp, out var restoreProductsItem))
            {
                SKUtils.HandleError($"[SayKitBridgeiOS][OnRestorePurchases][Error] Key '{timeStamp}' not found in dictionary.");
                return;
            }

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(json))
                {
                    var restoreProductsInfo = JsonConvert.DeserializeObject<SKPurchasedProductsInfo>(json);

                    if (restoreProductsInfo != null)
                    {
                        restoreProductsItem(restoreProductsInfo.ProductIds, restoreProductsInfo.ActiveSubscriptions, restoreProductsInfo.Error);
                    }
                    else
                    {
                        restoreProductsItem(Array.Empty<string>(), null,
                            new SKBillingError("[SayKitBridgeiOS][OnRestorePurchases][Error] RestoreProductsInfo is null."));
                    }
                }
                else
                {
                    restoreProductsItem(Array.Empty<string>(), null, 
                        new SKBillingError("[SayKitBridgeiOS][OnRestorePurchases][Error] Invalid json data."));
                }
            }, e =>
            {
                var errorMessage = $"[SayKitBridgeiOS][OnRestorePurchases][Error] message: {e.Message}, stacktrace: {e.StackTrace}";
                SKUtils.HandleError(errorMessage);
                restoreProductsItem(Array.Empty<string>(), null, new SKBillingError(errorMessage));
            }, () => _restoreProductsDictionary?.Remove(timeStamp));
        }

        public void RestorePurchases(Action<string[], SKInAppSubscription[], SKBillingError> onRestorePurchasesCompleted)
        {
            var timeStamp = SKUtils.currentTimestamp.ToString();
            _restoreProductsDictionary?.Add(timeStamp, onRestorePurchasesCompleted);
            Native_restoreProducts(timeStamp, _OnRestorePurchases);
        }

        [DllImport("__Internal")]
        private static extern void Native_acceptProduct(string purchasedProduct);

        public void ConfirmPurchase(SKPurchasedProduct purchasedProduct, Action<bool, SKBillingError> onConfirmProductCompleted)
        {
            Native_acceptProduct(JsonConvert.SerializeObject(purchasedProduct));
            onConfirmProductCompleted?.Invoke(true, null);
        }
        
        [DllImport("__Internal")]
        private static extern void Native_showWebShop(string externalId, string context);
        
        public void ShowWebShop(string externalId, string context)
        {
            Native_showWebShop(externalId, context);
        }
        
        [DllImport("__Internal")]
        private static extern void Native_setWebShopParams(string externalId, string context);
        
        public void SetWebShopParams(string externalId, string context)
        {
            Native_setWebShopParams(externalId, context);
        }
        
        #endregion

        #region Local notification

        [DllImport("__Internal")]
        private static extern void Native_scheduleNotification(string json);
        
        public void ScheduleLocalNotification(string json)
        {
            Native_scheduleNotification(json);
        }

        [DllImport("__Internal")]
        private static extern void Native_removeAllDeliveredNotifications();
        
        public void RemoveAllDeliveredLocalNotifications()
        {
            Native_removeAllDeliveredNotifications();
        }
        
        [DllImport("__Internal")]
        private static extern void Native_removeDeliveredNotification(string id);
        
        public void RemoveDeliveredLocalNotification(string id)
        {
            Native_removeDeliveredNotification(id);
        }
        
        [DllImport("__Internal")]
        private static extern void Native_removeAllScheduledNotifications();
        
        public void RemoveAllScheduledLocalNotifications()
        {
            Native_removeAllScheduledNotifications();
        }
        
        [DllImport("__Internal")]
        private static extern void Native_removeScheduledNotification(string id);
        
        public void RemoveScheduledLocalNotification(string id)
        {
            Native_removeScheduledNotification(id);
        }

        #endregion

        #region InPlay

        [DllImport("__Internal")]
        private static extern bool Native_isInPlayAvailable();
        
        public bool IsInPlayAvailable()
        {
            return Native_isInPlayAvailable();
        }
        
        [DllImport("__Internal")]
        private static extern bool Native_showInPlay(int x, int y, int width, int height);

        public bool ShowInPlay(int x, int y, int width, int height)
        {
            return Native_showInPlay(x, y, width, height);
        }
        
        [DllImport("__Internal")]
        private static extern void Native_hideInPlay();
    
        public void HideInPlay()
        {
            Native_hideInPlay();
        }

        [DllImport("__Internal")]
        private static extern float GetScaleScreen();

        public float GetScreenScale()
        {
            return GetScaleScreen();
        }

        #endregion

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        private static ISayKitBridgeCallbacks _listener;

        private delegate void InAppProductCheckedDelegate(string json);
        private delegate void InitStateChangedDelegate(int number, float progress);
        private delegate void NotificationTokenReceivedDelegate(string token);
        private delegate void RemoteConfigUpdatedDelegate(string path);
        private delegate void OnLocalizationsUpdated(string data);
        private delegate void InterstitialClosedDelegate();
        private delegate void RewardedClosedDelegate(bool rewarded);
        private delegate void AdViewShownDelegate(string type, string time, string network, string creativeId);
        private delegate void SayCatalogueShownDelegate(string placement);
        private delegate void AdRevenuePaidDelegate(string type, string revenueInfo);
        private delegate void AdDisplayedDelegate(string type, string adInfo);
        private delegate void OnDeepLinkReceived(string data);
        private delegate void OnSupportRequestSubmitted(bool result);
        private delegate void OnNonConfirmedPurchaseReceived();
        private delegate void OnOfferwallRewardReceived(string data);

        public void SetCallbacks(ISayKitBridgeCallbacks listener)
        {
            _listener = listener;

            Native_setSayKitBridgeDelegate(
                _InAppProductCheckedDelegate,
                _InitStateChangedDelegate,
                _NotificationTokenReceivedDelegate,
                _RemoteConfigUpdatedDelegate,
                _OnLocalizationsUpdated,
                _InterstitialClosedDelegate,
                _RewardedClosedDelegate,
                _AdViewShownDelegate,
                _SayCatalogueShownDelegate,
                _AdRevenuePaidDelegate,
                _AdDisplayedDelegate,
                _OnDeepLinkReceived,
                _OnNonConfirmedPurchaseReceived,
                _OnSupportRequestSubmitted,
                _OnOfferwallRewardReceived
            );
        }

        [DllImport("__Internal")]
        private static extern void Native_setSayKitBridgeDelegate(
            InAppProductCheckedDelegate inAppProductCheckedDelegate,
            InitStateChangedDelegate initStateChangedDelegate,
            NotificationTokenReceivedDelegate notificationTokenReceivedDelegate,
            RemoteConfigUpdatedDelegate remoteConfigUpdatedDelegate,
            OnLocalizationsUpdated onLocalizationsUpdated,
            InterstitialClosedDelegate interstitialClosedDelegate,
            RewardedClosedDelegate rewardedClosedDelegate,
            AdViewShownDelegate onAdViewShownDelegate,
            SayCatalogueShownDelegate sayCatalogueShownDelegate,
            AdRevenuePaidDelegate adRevenuePaidDelegate,
            AdDisplayedDelegate adDisplayedDelegate,
            OnDeepLinkReceived onDeepLinkReceived,
            OnNonConfirmedPurchaseReceived onNonConfirmedPurchaseReceived,
            OnSupportRequestSubmitted onSupportRequestSubmitted,
            OnOfferwallRewardReceived onOfferwallRewardReceived
        );

        [MonoPInvokeCallback(typeof(InAppProductCheckedDelegate))]
        private static void _InAppProductCheckedDelegate(string json)
        {
            _listener?.OnInAppProductChecked(json);
        }

        [MonoPInvokeCallback(typeof(InitStateChangedDelegate))]
        private static void _InitStateChangedDelegate(int number, float progress)
        {
            _listener?.OnInitStateChanged(number, progress);
        }

        [MonoPInvokeCallback(typeof(NotificationTokenReceivedDelegate))]
        private static void _NotificationTokenReceivedDelegate(string token)
        {
            _listener?.OnNotificationTokenReceived(token);
        }

        [MonoPInvokeCallback(typeof(RemoteConfigUpdatedDelegate))]
        private static void _RemoteConfigUpdatedDelegate(string path)
        {
            _listener?.OnRemoteConfigUpdated(path);
        }

        [MonoPInvokeCallback(typeof(InterstitialClosedDelegate))]
        private static void _InterstitialClosedDelegate()
        {
            _listener?.OnInterstitialClosed();
        }

        [MonoPInvokeCallback(typeof(RewardedClosedDelegate))]
        private static void _RewardedClosedDelegate(bool rewarded)
        {
            _listener?.OnRewardedClosed(rewarded);
        }

        [MonoPInvokeCallback(typeof(AdViewShownDelegate))]
        private static void _AdViewShownDelegate(string type, string time, string network, string creativeId)
        {
            _listener?.OnAdShown(type, time, network, creativeId);
        }

        [MonoPInvokeCallback(typeof(SayCatalogueShownDelegate))]
        private static void _SayCatalogueShownDelegate(string placement)
        {
            _listener?.OnSayCatalogueShown(placement);
        }

        [MonoPInvokeCallback(typeof(AdRevenuePaidDelegate))]
        private static void _AdRevenuePaidDelegate(string type, string adRevenueInfo)
        {
            _listener?.OnAdRevenuePaid(type, adRevenueInfo);
        }

        [MonoPInvokeCallback(typeof(AdDisplayedDelegate))]
        private static void _AdDisplayedDelegate(string type, string adInfo)
        {
            _listener?.OnAdDisplayed(type, adInfo);
        }

        [MonoPInvokeCallback(typeof(OnLocalizationsUpdated))]
        private static void _OnLocalizationsUpdated(string data)
        {
            _listener?.OnLocalizationsUpdated(data);
        }

        [MonoPInvokeCallback(typeof(OnDeepLinkReceived))]
        private static void _OnDeepLinkReceived(string data)
        {
            _listener?.OnDeepLinkReceived(data);
        }

        [MonoPInvokeCallback(typeof(OnSupportRequestSubmitted))]
        private static void _OnSupportRequestSubmitted(bool result)
        {
            _listener?.OnSupportRequestSubmitted(result);
        }

        [MonoPInvokeCallback(typeof(OnNonConfirmedPurchaseReceived))]
        private static void _OnNonConfirmedPurchaseReceived()
        {
            _listener?.OnNonConfirmedPurchaseReceived();
        }
        
        [MonoPInvokeCallback(typeof(OnOfferwallRewardReceived))]
        private static void _OnOfferwallRewardReceived(string data) { }
    }
}
#endif