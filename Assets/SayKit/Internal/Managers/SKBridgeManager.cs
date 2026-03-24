using System;
using System.Globalization;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable once EmptyRegion
// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    public class SKBridgeManager
    {
        public static SKBridgeManager Instance { get; } = new SKBridgeManager();

        public bool IsInitialized()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.IsInitialized);
        }

        public float GetInitStateProgress()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetInitStateProgress);
        }

        public int GetInitStateNumber()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetInitStateNumber);
        }

        public void InitIfNeeded(string configJson, string environmentParams)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.InitIfNeeded(configJson: SKUtils.CheckNullString(configJson),
                    environmentParams: SKUtils.CheckNullString(environmentParams));
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public float GetBackgroundBannerSize()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetBackgroundBannerSize);
        }

        public void HideBanner()
        {
            SKThreadService.Instance.RunOnMainThread(SayKitBridge.Instance.HideBanner);
        }

        public void ShowBanner()
        {
            SKThreadService.Instance.RunOnMainThread(SayKitBridge.Instance.ShowBanner);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsRewardedPlacementAvailable(string place)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.IsRewardedPlacementAvailable(place: SKUtils.CheckNullString(place)));
        }

        public bool IsInterstitialAvailable(string place, int countdown = 0)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.IsInterstitialAvailable(place: SKUtils.CheckNullString(place), countdown: countdown));
        }

        public bool ShowInterstitial(string place, int countdown = 0)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.ShowInterstitial(place: SKUtils.CheckNullString(place), countdown: countdown));
        }

        public bool IsRewardedAvailable(string place)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.IsRewardedAvailable(place: SKUtils.CheckNullString(place)));
        }

        public bool ShowRewarded(string place)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.ShowRewarded(place: SKUtils.CheckNullString(place)));
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackInterstitialOffer(string place, string extra)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackInterstitialOffer(place: SKUtils.CheckNullString(place),
                    extra: SKUtils.CheckNullString(extra));
            });
        }

        public void TrackRewardedOffer(string place, string extra)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackRewardedOffer(place: SKUtils.CheckNullString(place),
                    extra: SKUtils.CheckNullString(extra));
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetSubscriptionExpirationTimestamp()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(
                SayKitBridge.Instance.GetSubscriptionExpirationTimestamp);
        }

        public bool IsAppStoreAvailable()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.IsAppStoreAvailable);
        }

        public string GetPrivacyPolicyLink()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetPrivacyPolicyLink);
        }

        public bool IsAppInstalled(string packageName)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.IsAppInstalled(packageName: SKUtils.CheckNullString(packageName)));
        }

        public void OpenGooglePlaySubscriptionCenter(string productId)
        {
            SKThreadService.Instance.RunOnMainThread(
                () => { SayKitBridge.Instance.OpenGooglePlaySubscriptionCenter(productId: SKUtils.CheckNullString(productId)); });
        }

        public void OpenStoreProductView(int storeId, string skadData, string storeUrl)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.OpenStoreProductView(storeId: storeId, skadData: SKUtils.CheckNullString(skadData),
                    storeUrl: SKUtils.CheckNullString(storeUrl));
            });
        }

        public string OpenSupportPage(string extra)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.OpenSupportPage(extra: SKUtils.CheckNullString(extra)));
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsFacebookSdkInitialized()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.IsFacebookSdkInitialized);
        }

        public void LogFacebookEvent(string name, float valueToSum, string paramsJson)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.LogFacebookEvent(name: SKUtils.CheckNullString(name), valueToSum: valueToSum,
                    paramsJson: SKUtils.CheckNullString(paramsJson));
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void LogFirebaseEvent(string name, float valueToSum, string paramsJson)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.LogFirebaseEvent(name: SKUtils.CheckNullString(name), valueToSum: valueToSum,
                    paramsJson: SKUtils.CheckNullString(paramsJson));
            });
        }

        public void SetFirebaseUserProperty(string key, string value)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.SetFirebaseUserProperty(key: SKUtils.CheckNullString(key),
                    value: SKUtils.CheckNullString(value));
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void LogCrashlytics(string message)
        {
            SKThreadService.Instance.RunOnMainThread(
                () => { SayKitBridge.Instance.LogCrashlytics(message: SKUtils.CheckNullString(message)); });
        }

        public void LogCrashlyticsException(Exception exception)
        {
            SKThreadService.Instance.RunOnMainThread(
                () => { SayKitBridge.Instance.LogCrashlyticsException(exception: exception); });
        }

        public void SetCrashlyticsParam(string key, string value)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.SetCrashlyticsParam(key: SKUtils.CheckNullString(key),
                    value: SKUtils.CheckNullString(value));
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackApplicationLoaded()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.TrackApplicationLoaded(); });
        }

        public void TrackAvailableMemory()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.TrackAvailableMemory(); });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackEvent(string name, long param1 = 0, long param2 = 0, long param3 = 0, long param4 = 0,
            string extra1 = "", string extra2 = "", string tag = "", string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackEvent(name: SKUtils.CheckNullString(name), param1: param1, param2: param2, param3: param3, param4: param4,
                    extra1: SKUtils.CheckNullString(extra1), extra2: SKUtils.CheckNullString(extra2),
                    tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelStarted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelStarted(level: level, score: score, number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        public void TrackLevelCompleted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelCompleted(level: level, score: score, number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        public void TrackLevelFailed(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelFailed(level: level, score: score, number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelExtraStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelExtraStarted(number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        public void TrackLevelExtraCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelExtraCompleted(score: score, number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        public void TrackLevelExtraFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelExtraFailed(score: score, number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelStageStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelStageStarted(number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        public void TrackLevelStageCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelStageCompleted(score: score, number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        public void TrackLevelStageFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackLevelStageFailed(score: score, number: number, extra1: SKUtils.CheckNullString(extra1),
                    extra2: SKUtils.CheckNullString(extra2), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackChunkStarted(string name, int number, string paramsJson, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackChunkStarted(name: SKUtils.CheckNullString(name), number: number,
                    paramsJson: SKUtils.CheckNullString(paramsJson), tag: SKUtils.CheckNullString(tag), context: context);
            });
        }

        public void TrackChunkCompleted(string paramsJson, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackChunkCompleted(paramsJson: SKUtils.CheckNullString(paramsJson), tag: SKUtils.CheckNullString(tag),
                    context: context);
            });
        }

        public void TrackChunkFailed(string paramsJson, string tag, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackChunkFailed(paramsJson: SKUtils.CheckNullString(paramsJson), tag: SKUtils.CheckNullString(tag),
                    context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackTutorialCompleted(string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.TrackTutorialCompleted(context: context); });
        }

        public void TrackTutorialStep(string name, string step, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackTutorialStep(name: SKUtils.CheckNullString(name),
                    step: SKUtils.CheckNullString(step), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackItem(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackItem(name: SKUtils.CheckNullString(name), ownedItems: ownedItems, sourceType: sourceType,
                    paramsJson: SKUtils.CheckNullString(paramsJson), context: context);
            });
        }

        public void TrackItemLoss(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackItemLoss(name: SKUtils.CheckNullString(name), ownedItems: ownedItems,
                    sourceType: sourceType, paramsJson: SKUtils.CheckNullString(paramsJson), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackClick(string screen, string element, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackClick(screen: SKUtils.CheckNullString(screen),
                    element: SKUtils.CheckNullString(element), context: context);
            });
        }

        public void TrackScreen(string screen, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackScreen(screen: SKUtils.CheckNullString(screen), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackHardIncome(long amount, long total, string place, string extra, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackHardIncome(amount: amount, total: total, place: SKUtils.CheckNullString(place),
                    extra: SKUtils.CheckNullString(extra), context: context);
            });
        }

        public void TrackHardOutcome(long amount, long total, string place, string extra, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackHardOutcome(amount: amount, total: total,
                    place: SKUtils.CheckNullString(place), extra: SKUtils.CheckNullString(extra), context: context);
            });
        }

        public void TrackSoftIncome(long amount, long total, string place, string extra, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackSoftIncome(amount: amount, total: total,
                    place: SKUtils.CheckNullString(place), extra: SKUtils.CheckNullString(extra), context: context);
            });
        }

        public void TrackSoftOutcome(long amount, long total, string place, string extra, string context = null)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackSoftOutcome(amount: amount, total: total,
                    place: SKUtils.CheckNullString(place), extra: SKUtils.CheckNullString(extra), context: context);
            });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void CheckInAppProduct(string json)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.CheckInAppProduct(json: SKUtils.CheckNullString(json)); });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetRequestConfigTimestamp()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetRequestConfigTimestamp);
        }

        public void RequestConfigMigration(string sourceVersion)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.RequestConfigMigration(sourceVersion: SKUtils.CheckNullString(sourceVersion));
            });
        }

        public bool RequestRemoteConfigUpdate()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.RequestRemoteConfigUpdate);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool ShowCustomRateAppPopup(int rate)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() =>
                SayKitBridge.Instance.ShowCustomRateAppPopup(rate: rate));
        }

        public bool ShowRateAppPopup()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.ShowRateAppPopup);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsPremium()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.IsPremium);
        }

        public void DisablePremium()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.DisablePremium(); });
        }

        public void EnablePremium()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.EnablePremium(); });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool GetGdprStatus()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetGdprStatus);
        }

        public bool IsGdprApplicable()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.IsGdprApplicable);
        }

        public void RevokeGdprConsent()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.RevokeGdprConsent(); });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetNotificationToken()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetNotificationToken);
        }

        public void RequestNotificationToken()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.RequestNotificationToken(); });
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////
        
        public string GetFullCurrentLanguage()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetFullCurrentLanguage);
        }

        public (string, CultureInfo) GetFullCurrentLanguageWithCultureInfo()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetFullCurrentLanguageWithCultureInfo);
        }

        public SayKitLanguage GetCurrentLanguage()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetCurrentLanguage);
        }

        public bool OverrideSystemLanguage(string language)
        {
           return SKThreadService.Instance.RunOnMainThreadWithResult(() => SayKitBridge.Instance.OverrideSystemLanguage(language: language));
        }
        
        public string PrioritizedLanguages()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.PrioritizedLanguages);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetFreeMemory()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetFreeMemory);
        }

        public int GetTotalMemory()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetTotalMemory);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetRuntimeInfo()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetRuntimeInfo);
        }

        public string GetAdvertisingId()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetAdvertisingId);
        }

        public string GetAppVersion()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetAppVersion);
        }

        public string GetDeviceId()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetDeviceId);
        }

        public string GetDeviceModel()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetDeviceModel);
        }

        public string GetDeviceOs()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetDeviceOs);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetAppVersionCode()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetAppVersionCode);
        }

        public string GetAppVersionFullName()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetAppVersionFullName);
        }

        public string GetAppVersionOriginalName()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetAppVersionOriginalName);
        }

        public int GetSdkVersionCode()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetSdkVersionCode);
        }

        public string GetSdkVersionName()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetSdkVersionName);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetThermalState()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetThermalState);
        }

        public void DisableLogs()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.DisableLogs(); });
        }

        public void OverrideTrackLevel(int level)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.OverrideTrackLevel(level: level); });
        }

        public void ShowMaxMediationDebug()
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.ShowMaxMediationDebug(); });
        }

        public bool GetRateAppShown()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetRateAppShown);
        }

        public string GetATTStatus()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetATTStatus);
        }

        public void SetSayKitBridgeCallbacks(ISayKitBridgeCallbacks listener)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.SetCallbacks(listener: listener); });
        }

        public void ShowSayCatalogue(string placement, string catalogParams = "")
        {
            SKThreadService.Instance.RunOnMainThread(
                () =>
                {
                    SayKitBridge.Instance.ShowSayCatalogue(placement: SKUtils.CheckNullString(placement),
                        catalogParams: SKUtils.CheckNullString(catalogParams));
                });
        }

        public void TrackSayCatalogueOffer(string sourceType)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackSayCatalogueOffer(sourceType: SKUtils.CheckNullString(sourceType));
            });
        }

        public void TrackInAppOffer(string place, string extra)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.TrackInAppOffer(place: SKUtils.CheckNullString(place),
                    extra: SKUtils.CheckNullString(extra));
            });
        }

        public void SetPlayerId(string playerId)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.SetPlayerId(playerId: SKUtils.CheckNullString(playerId)); });
        }

        public void OpenSystemSettings()
        {
#if UNITY_EDITOR || UNITY_IPHONE
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.OpenSystemSettings(); });
#endif
        }

        public int GetPlayingTime()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetPlayingTime);
        }

        public void RequestLiveServer(string requestName, string requestData, Action<string> onRequestResult)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.RequestLiveServer(requestName: requestName, requestData: requestData, onRequestResult: onRequestResult);
            });
        }

        public void SetExperimentDeviceId(string deviceId)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.SetExperimentDeviceId(deviceId: SKUtils.CheckNullString(deviceId));
            });
        }

        public string GetSessionId()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetSessionId);
        }

        public string GetSystemProxy()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetSystemProxy);
        }

        public AttributionResponseData GetAttributionData()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetAttributionData);
        }

        public void SetGameContext(string gameContext)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.SetGameContext(gameContext: SKUtils.CheckNullString(gameContext));
            });
        }

        public void SetFirebaseUserId(string firebaseUserId)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.SetFirebaseUserId(firebaseUserId: SKUtils.CheckNullString(firebaseUserId));
            });
        }
        
        public void OpenCustomUrl(string url)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.OpenCustomUrl(url: SKUtils.CheckNullString(url));
            });
        }

        #region Billing
        
        public void GetAvailableProducts(SKProductInfo[] productInfos, Action<SKProduct[], SKBillingError> onAvailableProductsFetched)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.GetAvailableProducts(productInfos: productInfos, onAvailableProductsFetched: onAvailableProductsFetched);
            });
        }
        
        public void GetPurchasedProducts(Action<string[], SKInAppSubscription[], SKBillingError> onPurchasedProductsFetched)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.GetPurchasedProducts(onPurchasedProductsFetched: onPurchasedProductsFetched); });
        }
        
        public void GetNonConfirmedProducts(Action<SKPurchasedProduct[], SKBillingError> onNonConfirmedProductsFetched)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.GetNonConfirmedProducts(onNonConfirmedProductsFetched: onNonConfirmedProductsFetched); });
        }
        
        public void PurchaseProduct(SKProductInfo productInfo, string offer, string placement, string extra, Action<bool, SKPurchasedProduct, 
            SKBillingError> onPurchaseProductCompleted, SKPurchaseOptions options = null)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.PurchaseProduct(productInfo: productInfo, options: options, offer: offer, placement: placement, extra: extra, onPurchaseProductCompleted: onPurchaseProductCompleted); });
        }

        public void ConfirmPurchase(SKPurchasedProduct purchasedProduct, Action<bool, SKBillingError> onConfirmProductCompleted)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.ConfirmPurchase(purchasedProduct: purchasedProduct, onConfirmProductCompleted: onConfirmProductCompleted); });
        }

        public void RestorePurchases(Action<string[], SKInAppSubscription[], SKBillingError> onRestorePurchasesCompleted)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.RestorePurchases(onRestorePurchasesCompleted: onRestorePurchasesCompleted); });
        }
        
        public void ShowWebShop(string externalId, string context)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.ShowWebShop(externalId: externalId, context: context); });
            
        }
        
        public void SetWebShopParams(string externalId, string context)
        {
            SKThreadService.Instance.RunOnMainThread(() => { SayKitBridge.Instance.SetWebShopParams(externalId: externalId, context: context); });
            
        }
        
        #endregion

        #region Local notification
        
        public void ScheduleLocalNotification(SKLocalNotification localNotification)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.ScheduleLocalNotification(json: JsonConvert.SerializeObject(localNotification));
            });
        }

        public void RemoveAllDeliveredLocalNotifications()
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.RemoveAllDeliveredLocalNotifications();
            });
        }
        
        public void RemoveDeliveredLocalNotification(string id)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.RemoveDeliveredLocalNotification(id: id);
            });
        }
        
        public void RemoveAllScheduledLocalNotifications()
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.RemoveAllScheduledLocalNotifications();
            });
        }
        
        public void RemoveScheduledLocalNotification(string id)
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.RemoveScheduledLocalNotification(id: id);
            });
        }        
        
        #endregion

        #region InPlay

        public bool IsInPlayAvailable()
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.IsInPlayAvailable);
        }

        public bool ShowInPlay(int x, int y, int width, int height)
        {
            return SKThreadService.Instance.RunOnMainThreadWithResult(() => SayKitBridge.Instance.ShowInPlay(x: x, y: y, width: width, height: height));
        }
    
        public void HideInPlay()
        {
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayKitBridge.Instance.HideInPlay();
            });
        }
        
        public float GetScreenScale()
        {
            return  SKThreadService.Instance.RunOnMainThreadWithResult(SayKitBridge.Instance.GetScreenScale);;
        }        

        #endregion
    }
}