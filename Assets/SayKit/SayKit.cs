#if SAYKIT_PURCHASING
using UnityEngine.Purchasing;
#endif

using System;
using SayKitInternal;
using System.Collections.Generic;
using System.Globalization;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable MethodOverloadWithOptionalParameter

#endregion

public class SayKitConfig
{
    public string appKey = "";
    public string bannerAdUnitId = "";
    public string interstitialAdUnitId = "";
    public string rewardedAdUnitId = "";

    public bool attributionConfigUpdate = false;
    public Action remoteConfigUpdated;
    public Action<int> sayCatalogueRewarded;
    public Action<string, AdRevenueInfo> adRevenuePaid;
    public Action<string, AdInfo> adDisplayed;
    public Action<string> onDeepLinkReceived;
    public Action<bool> onSupportRequestSubmitted;
    public Action nonConfirmedPurchaseReceived;
    public bool disablePurchaseValidation;
    
    /// <summary>
    /// Uses when the user clicks on NoAds button in the interstitial popup.
    /// </summary>
    public Action showNoAdsOffer;

    /// <summary>
    /// Invoked when the user allows the push notification permission. If the user denies the permission, the action will not be called.
    /// Returns notification token.
    /// </summary>
    public Action<string> notificationTokenReceived;

    /// <summary>
    /// Uses only in the Editor.
    /// </summary>
    public SayKitLanguage overrideSystemLanguage = SKLocalizationService.Instance.GetOverrideLanguage();

    public int overrideAnalyticSegment = -1;

    /// <summary>
    /// Disable automatic banner initialization.
    /// If you enable it, you must manage the banner using showBanner/hideBanner methods.
    /// </summary>
    public bool disableAutoBanner = false;

    /// <summary>
    /// Skip banner timeouts on start.
    /// If you enable it, the banner will show on start of application.
    /// </summary>
    public bool disableAutoBannerTimeouts = false;

    /// <summary>
    /// Disable interstitial ads.
    /// </summary>
    public bool disableInterstitial = false;

    /// <summary>
    /// Call Notification request manually. Make sure you are calling notification request every session.
    /// </summary>
    public bool customNotificationRequest = false;

    /// <summary>
    /// Sorting order for all SayKit UI windows (10000 by default). 
    /// </summary>
    public int canvasSortingOrder = 10000;
}

public class SayKit
{
    public static int GetVersion => SKManager.Instance.Version;
    public static string BuildVersion => SKManager.Instance.BuildVersion;
    public static SayKitConfig config => SKManager.Instance.Config;
    public static RuntimeInfo runtimeInfo => SKManager.Instance.RuntimeInfo;
    public static RemoteConfig remoteConfig => SKManager.Instance.RemoteConfig;
    public static SayKitGameConfig gameConfig => SKManager.Instance.GameConfig;
    public static bool isInitialized => SKManager.Instance.IsInitialized;
    public static float initializedProgress => SKManager.Instance.InitializedProgress;
    public static EInitState GetInitState() => SKManager.Instance.GetInitState;

    public static void init(SayKitConfig sayKitConfig)
    {
        SKManager.Instance.Initialize(sayKitConfig: sayKitConfig);
    }

    // Ads
    /// <summary>
    /// Uses only if disableAutoBanner flag is true.
    /// </summary>
    public static void showBanner()
    {
        SKManager.Instance.ShowBanner(isExternalCall: true);
    }

    /// <summary>
    /// Uses only if disableAutoBanner flag is true.
    /// </summary>
    public static void hideBanner()
    {
        SKManager.Instance.HideBanner(isExternalCall: true);
    }

    /// <summary>
    /// Returns background banner size including safe area in pixels.
    /// Important: real banner can be smaller on iPhones.
    /// </summary>
    public static float GetBackgroundBannerSize()
    {
        return SKManager.Instance.GetBackgroundBannerSize();
    }

    public static bool isInterstitialAvailable(string place, int countdown = 0)
    {
        return SKManager.Instance.IsInterstitialAvailable(place: place, countdown: countdown);
    }

    public static bool isRewardedAvailable(string place)
    {
        return SKManager.Instance.IsRewardedAvailable(place: place);
    }

    public static bool isRewardedPlacementAvailable(string place)
    {
        return SKManager.Instance.IsRewardedPlacementAvailable(place: place);
    }

    public static bool showInterstitial(string place, Action onCloseCallback = null)
    {
        return SKManager.Instance.ShowInterstitial(place: place, onCloseCallback: onCloseCallback);
    }
    
    public static bool showInterstitialWithPopup(string place, int countdown, Action onShowCallback = null, Action onCloseCallback = null)
    {
        return SKManager.Instance.ShowInterstitialWithPopup(place: place, countdown: countdown, onShowCallback: onShowCallback, onCloseCallback: onCloseCallback);
    }
    
    public static void showRewarded(string place, Action<bool> onCloseCallback)
    {
        SKManager.Instance.ShowRewarded(place: place, onCloseCallback: onCloseCallback);
    }
    
    public static void trackInterstitialOffer(string place, string extra = "")
    {
        SKManager.Instance.TrackInterstitialOffer(place: place, extra: extra);
    }

    public static void trackRewardedOffer(string place, string extra = "") 
    {
        SKManager.Instance.TrackRewardedOffer(place: place, extra: extra);
    }

    /// <summary>
    /// Open browser with support page.
    /// </summary>
    /// <returns>Returns request GUID id.</returns>
    /// <param name="extra">Extra data.</param>
    public static string OpenSupportPage(string extra = "")
    {
        return SKManager.Instance.OpenSupportPage(extra: extra);
    }

    /// <summary>
    /// Returns the privacy policy link.
    /// </summary>
    /// <returns></returns>
    public static string GetPrivacyPolicyLink()
    {
        return SKManager.Instance.GetPrivacyPolicyLink();
    }

    public static bool isFacebookSDKInitialized()
    {
        return SKManager.Instance.IsFacebookSDKInitialized();
    }

    public static void logFBAppEvent(string logEvent, float? valueToSum = default(float?),
        Dictionary<string, object> parameters = null)
    {
        SKManager.Instance.LogFacebookEvent(logEvent: logEvent, valueToSum: valueToSum.GetValueOrDefault(), parameters: parameters);
    }

    public static void logFirebaseEvent(string eventName, string extraParam = "")
    {
        SKManager.Instance.LogFirebaseEvent(eventName: eventName, extraParam: extraParam);
    }

    public static void logFirebaseEvent(string eventName, string paramName, object paramValue)
    {
        SKManager.Instance.LogFirebaseEvent(eventName: eventName, paramName: paramName, paramValue: paramValue);
    }

    public static void logFirebaseEvent(string eventName, float? valueToSum = default(float?),
        Dictionary<string, object> parameters = null)
    {
        SKManager.Instance.LogFirebaseEvent(eventName: eventName, valueToSum: valueToSum.GetValueOrDefault(), parameters: parameters);
    }

    public static void setCrashlyticsParam(string paramName, string paramValue)
    {
        SKManager.Instance.SetCrashlyticsParam(paramName: paramName, paramValue: paramValue);
    }

    public static void logCrashlyticsException(Exception exception)
    {
        SKManager.Instance.LogCrashlyticsException(exception: exception);
    }

    public static void logCrashlytics(string message)
    {
        SKManager.Instance.LogCrashlytics(message: message);
    }

    public static void setFirebaseUserProperty(string key, string value = "")
    {
        SKManager.Instance.SetFirebaseUserProperty(key: key, value: value);
    }

    // Events
    public static void trackLevelStarted(int level)
    {
        SKManager.Instance.TrackLevelStarted(level: level);
    }

    public static void trackLevelStarted(string tag, int level)
    {
        SKManager.Instance.TrackLevelStarted(tag: tag, level: level);
    }

    public static void trackLevelStarted(string tag, int level, long score, string extra1)
    {
        SKManager.Instance.TrackLevelStarted(tag: tag, level: level, score: score, extra1: extra1);
    }

    public static void trackLevelStarted(string tag, int level, long score, string extra1, int number, string extra2)
    {
        SKManager.Instance.TrackLevelStarted(tag: tag, level: level, score: score, extra1: extra1, number: number,
            extra2: extra2);
    }

    public static void trackLevelStarted(string tag, int level, long score, string extra1, int number, string extra2, string context)
    {
        SKManager.Instance.TrackLevelStarted(tag: tag, level: level, score: score, extra1: extra1, number: number,
            extra2: extra2, context: context);
    }

    public static void trackLevelCompleted(int level, long score)
    {
        SKManager.Instance.TrackLevelCompleted(level: level, score: score);
    }

    public static void trackLevelCompleted(string tag, int level, long score)
    {
        SKManager.Instance.TrackLevelCompleted(tag: tag, level: level, score: score);
    }

    public static void trackLevelCompleted(string tag, int level, long score, string extra1)
    {
        SKManager.Instance.TrackLevelCompleted(tag: tag, level: level, score: score, extra1: extra1);
    }

    public static void trackLevelCompleted(string tag, int level, long score, string extra1, int number, string extra2)
    {
        SKManager.Instance.TrackLevelCompleted(tag: tag, level: level, score: score, extra1: extra1, number: number,
            extra2: extra2);
    }

    public static void trackLevelCompleted(string tag, int level, long score, string extra1, int number, string extra2, string context)
    {
        SKManager.Instance.TrackLevelCompleted(tag: tag, level: level, score: score, extra1: extra1, number: number,
            extra2: extra2, context: context);
    }
    public static void trackLevelFailed(int level, long score)
    {
        SKManager.Instance.TrackLevelFailed(level: level, score: score);
    }

    public static void trackLevelFailed(string tag, int level, long score)
    {
        SKManager.Instance.TrackLevelFailed(tag: tag, level: level, score: score);
    }

    public static void trackLevelFailed(string tag, int level, long score, string extra1)
    {
        SKManager.Instance.TrackLevelFailed(tag: tag, level: level, score: score, extra1: extra1);
    }

    public static void trackLevelFailed(string tag, int level, long score, string extra1, int number, string extra2)
    {
        SKManager.Instance.TrackLevelFailed(tag: tag, level: level, score: score, extra1: extra1, number: number, extra2: extra2);
    }

    public static void trackLevelFailed(string tag, int level, long score, string extra1, int number, string extra2, string context)
    {
        SKManager.Instance.TrackLevelFailed(tag: tag, level: level, score: score, extra1: extra1, number: number, extra2: extra2,
            context: context);
    }

    public static void trackLevelExtraStarted()
    {
        SKManager.Instance.TrackLevelExtraStarted();
    }

    public static void trackLevelExtraStarted(string tag)
    {
        SKManager.Instance.TrackLevelExtraStarted(tag: tag);
    }

    public static void trackLevelExtraStarted(string tag, string extra1)
    {
        SKManager.Instance.TrackLevelExtraStarted(tag: tag, extra1: extra1);
    }

    public static void trackLevelExtraStarted(string tag, int number, string extra1, string extra2)
    {
        SKManager.Instance.TrackLevelExtraStarted(tag: tag, number: number, extra1: extra1, extra2: extra2);
    }

    public static void trackLevelExtraStarted(string tag, int number, string extra1, string extra2, string context)
    {
        SKManager.Instance.TrackLevelExtraStarted(tag: tag, number: number, extra1: extra1, extra2: extra2, context: context);
    }

    public static void trackLevelExtraCompleted(long score)
    {
        SKManager.Instance.TrackLevelExtraCompleted(score: score);
    }

    public static void trackLevelExtraCompleted(string tag, long score)
    {
        SKManager.Instance.TrackLevelExtraCompleted(tag: tag, score: score);
    }

    public static void trackLevelExtraCompleted(string tag, long score, string extra1)
    {
        SKManager.Instance.TrackLevelExtraCompleted(tag: tag, score: score, extra1: extra1);
    }

    public static void trackLevelExtraCompleted(string tag, long score, int number, string extra1, string extra2)
    {
        SKManager.Instance.TrackLevelExtraCompleted(tag: tag, score: score, number: number, extra1: extra1, extra2: extra2);
    }

    public static void trackLevelExtraCompleted(string tag, long score, int number, string extra1, string extra2, string context)
    {
        SKManager.Instance.TrackLevelExtraCompleted(tag: tag, score: score, number: number, extra1: extra1, extra2: extra2, context: context);
    }

    public static void trackLevelExtraFailed(long score)
    {
        SKManager.Instance.TrackLevelExtraFailed(score: score);
    }

    public static void trackLevelExtraFailed(string tag, long score)
    {
        SKManager.Instance.TrackLevelExtraFailed(tag: tag, score: score);
    }

    public static void trackLevelExtraFailed(string tag, long score, string extra1)
    {
        SKManager.Instance.TrackLevelExtraFailed(tag: tag, score: score, extra1: extra1);
    }

    public static void trackLevelExtraFailed(string tag, long score, int number, string extra1, string extra2)
    {
        SKManager.Instance.TrackLevelExtraFailed(tag: tag, score: score, number: number, extra1: extra1, extra2: extra2);
    }

    public static void trackLevelExtraFailed(string tag, long score, int number, string extra1, string extra2, string context)
    {
        SKManager.Instance.TrackLevelExtraFailed(tag: tag, score: score, number: number, extra1: extra1, extra2: extra2, context: context);
    }

    public static void trackLevelStageStarted(int number)
    {
        SKManager.Instance.TrackLevelStageStarted(number: number);
    }

    public static void trackLevelStageStarted(string tag, int number)
    {
        SKManager.Instance.TrackLevelStageStarted(tag: tag, number: number);
    }

    public static void trackLevelStageStarted(string tag, int number, string extra1, string extra2)
    {
        SKManager.Instance.TrackLevelStageStarted(tag: tag, number: number, extra1: extra1, extra2: extra2);
    }

    public static void trackLevelStageStarted(string tag, int number, string extra1, string extra2, string context)
    {
        SKManager.Instance.TrackLevelStageStarted(tag: tag, number: number, extra1: extra1, extra2: extra2, context: context);
    }

    public static void trackLevelStageCompleted(int number, long score)
    {
        SKManager.Instance.TrackLevelStageCompleted(number: number, score: score);
    }

    public static void trackLevelStageCompleted(string tag, int number, long score)
    {
        SKManager.Instance.TrackLevelStageCompleted(tag: tag, number: number, score: score);
    }

    public static void trackLevelStageCompleted(string tag, int number, long score, string extra1, string extra2)
    {
        SKManager.Instance.TrackLevelStageCompleted(tag: tag, number: number, score: score, extra1: extra1, extra2: extra2);
    }

    public static void trackLevelStageCompleted(string tag, int number, long score, string extra1, string extra2, string context)
    {
        SKManager.Instance.TrackLevelStageCompleted(tag: tag, number: number, score: score, extra1: extra1, extra2: extra2, context: context);
    }

    public static void trackLevelStageFailed(int number, long score)
    {
        SKManager.Instance.TrackLevelStageFailed(number: number, score: score);
    }

    public static void trackLevelStageFailed(string tag, int number, long score)
    {
        SKManager.Instance.TrackLevelStageFailed(tag: tag, number: number, score: score);
    }

    public static void trackLevelStageFailed(string tag, int number, long score, string extra1, string extra2)
    {
        SKManager.Instance.TrackLevelStageFailed(tag: tag, number: number, score: score, extra1: extra1, extra2: extra2);
    }

    public static void trackLevelStageFailed(string tag, int number, long score, string extra1, string extra2, string context)
    {
        SKManager.Instance.TrackLevelStageFailed(tag: tag, number: number, score: score, extra1: extra1, extra2: extra2, context: context);
    }
    
    public static void trackChunkStarted(string name, int sequenceNumber)
    {
        SKManager.Instance.TrackChunkStarted(name: name, sequenceNumber: sequenceNumber);
    }

    public static void trackChunkStarted(string name, int sequenceNumber, string tag)
    {
        SKManager.Instance.TrackChunkStarted(name: name, sequenceNumber: sequenceNumber, tag: tag);
    }
    
    public static void trackChunkStarted(string name, int sequenceNumber, Dictionary<string, object> customData)
    {
        SKManager.Instance.TrackChunkStarted(name: name, sequenceNumber: sequenceNumber, customData: customData);
    }

    public static void trackChunkStarted(string name, int sequenceNumber, Dictionary<string, object> customData, string tag)
    {
        SKManager.Instance.TrackChunkStarted(name: name, sequenceNumber: sequenceNumber, customData: customData, tag: tag);
    }

    public static void trackChunkStarted(string name, int sequenceNumber, Dictionary<string, object> customData, string tag, string context)
    {
        SKManager.Instance.TrackChunkStarted(name: name, sequenceNumber: sequenceNumber, customData: customData, tag: tag, context: context);
    }
    
    public static void trackChunkCompleted()
    {
        SKManager.Instance.TrackChunkCompleted();
    }

    public static void trackChunkCompleted(string tag)
    {
        SKManager.Instance.TrackChunkCompleted(tag: tag);
    }
    
    public static void trackChunkCompleted(Dictionary<string, object> customData)
    {
        SKManager.Instance.TrackChunkCompleted(customData: customData);
    }

    public static void trackChunkCompleted(Dictionary<string, object> customData, string tag)
    {
        SKManager.Instance.TrackChunkCompleted(customData: customData, tag: tag);
    }
    
    public static void trackChunkCompleted(Dictionary<string, object> customData, string tag, string context)
    {
        SKManager.Instance.TrackChunkCompleted(customData: customData, tag: tag, context: context);
    }
    
    public static void trackChunkFailed()
    {
        SKManager.Instance.TrackChunkFailed();
    }

    public static void trackChunkFailed(string tag)
    {
        SKManager.Instance.TrackChunkFailed(tag: tag);
    }
    
    public static void trackChunkFailed(Dictionary<string, object> customData)
    {
        SKManager.Instance.TrackChunkFailed(customData: customData);
    }

    public static void trackChunkFailed(Dictionary<string, object> customData, string tag)
    {
        SKManager.Instance.TrackChunkFailed(customData: customData, tag: tag);
    }
    
    public static void trackChunkFailed(Dictionary<string, object> customData, string tag, string context)
    {
        SKManager.Instance.TrackChunkFailed(customData: customData, tag: tag, context: context);
    }

    public static void trackTutorialCompleted()
    {
        SKManager.Instance.TrackTutorialCompleted();
    }
    
    public static void trackTutorialCompleted(string context)
    {
        SKManager.Instance.TrackTutorialCompleted(context: context);
    }

    public static void trackTutorialStep(string tutorialName, string stepName)
    {
        SKManager.Instance.TrackTutorialStep(tutorialName: tutorialName, stepName: stepName);
    }
    
    public static void trackTutorialStep(string tutorialName, string stepName, string context)
    {
        SKManager.Instance.TrackTutorialStep(tutorialName: tutorialName, stepName: stepName, context: context);
    }

    public static void trackEvent(string eventName)
    {
        SKManager.Instance.TrackEvent(eventName: eventName);
    }

    public static void trackEvent(string eventName, long eventParam1)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1);
    }

    public static void trackEvent(string eventName, long eventParam1, long eventParam2)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1, param2: eventParam2);
    }

    public static void trackEvent(string eventName, long eventParam1, long eventParam2, string eventExtra1)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1, param2: eventParam2, extra1: eventExtra1);
    }
    
    public static void trackEvent(string eventName, long eventParam1, long eventParam2, long eventParam3)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1, param2: eventParam2, param3: eventParam3);
    }

    public static void trackEvent(string eventName, long eventParam1, long eventParam2, long eventParam3, string eventExtra1)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1, param2: eventParam2, param3: eventParam3,
            extra1: eventExtra1);
    }
    
    public static void trackEvent(string eventName, long eventParam1, long eventParam2, long eventParam3, string eventExtra1, string context)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1, param2: eventParam2, param3: eventParam3,
            extra1: eventExtra1, context: context);
    }

    public static void trackEvent(string eventName, string eventExtra1)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, extra1: eventExtra1);
    }

    public static void trackEvent(string eventName, string eventExtra1, string eventExtra2)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, extra1: eventExtra1, extra2: eventExtra2);
    }

    public static void trackEvent(string eventName, long eventParam1, string eventExtra1, string eventExtra2)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1, extra1: eventExtra1, extra2: eventExtra2);
    }

    public static void trackEvent(string eventName, long eventParam1, long eventParam2, string eventExtra1, string eventExtra2)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, param1: eventParam1, param2: eventParam2, extra1: eventExtra1,
            extra2: eventExtra2);
    }

    public static void trackTagEvent(string eventName, string tag, long eventParam1, long eventParam2, string eventExtra1,
        string eventExtra2)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, tag: tag, param1: eventParam1, param2: eventParam2, extra1: eventExtra1,
            extra2: eventExtra2);
    }

    public static void trackTagEvent(string eventName, string tag, long eventParam1, long eventParam2, long eventParam3,
        string eventExtra1, string eventExtra2)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, tag: tag, param1: eventParam1, param2: eventParam2, param3: eventParam3,
            extra1: eventExtra1, extra2: eventExtra2);
    }
    
    public static void trackTagEvent(string eventName, string tag, long eventParam1, long eventParam2, long eventParam3,
        string eventExtra1, string eventExtra2, string context)
    {
        SKManager.Instance.TrackEvent(eventName: eventName, tag: tag, param1: eventParam1, param2: eventParam2, param3: eventParam3,
            extra1: eventExtra1, extra2: eventExtra2, context: context);
    }

    public static void trackItem(string item)
    {
        SKManager.Instance.TrackItem(item: item);
    }
    
    public static void trackItem(string item, string context)
    {
        SKManager.Instance.TrackItem(item: item, context: context);
    }

    public static void trackItem(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData)
    {
        SKManager.Instance.TrackItem(item: item, ownedItems: ownedItems, sourceId: sourceId, customData: customData);
    }
    
    public static void trackItem(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData, string context)
    {
        SKManager.Instance.TrackItem(item: item, ownedItems: ownedItems, sourceId: sourceId, customData: customData, context: context);
    }

    public static void trackItem(string item, int ownedItems, string customData, SourceType sourceId)
    {
        SKManager.Instance.TrackItem(item: item, ownedItems: ownedItems, customData: customData, sourceId: sourceId);
    }
    
    public static void trackItem(string item, int ownedItems, string customData, SourceType sourceId, string context)
    {
        SKManager.Instance.TrackItem(item: item, ownedItems: ownedItems, customData: customData, sourceId: sourceId, context: context);
    }

    public static void trackItemLoss(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData)
    {
        SKManager.Instance.TrackItemLoss(item: item, ownedItems: ownedItems, sourceId: sourceId, customData: customData);
    }
    
    public static void trackItemLoss(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData, string context)
    {
        SKManager.Instance.TrackItemLoss(item: item, ownedItems: ownedItems, sourceId: sourceId, customData: customData, context: context);
    }
    
    public static void trackItemLoss(string item, int ownedItems, string customData, SourceType sourceId)
    {
        SKManager.Instance.TrackItemLoss(item: item, ownedItems: ownedItems, customData: customData, sourceId: sourceId);
    }

    public static void trackItemLoss(string item, int ownedItems, string customData, SourceType sourceId, string context)
    {
        SKManager.Instance.TrackItemLoss(item: item, ownedItems: ownedItems, customData: customData, sourceId: sourceId, context: context);
    }

    public static void trackScreen(string screen)
    {
        SKManager.Instance.TrackScreen(screen: screen);
    }
    
    public static void trackScreen(string screen, string context)
    {
        SKManager.Instance.TrackScreen(screen: screen, context: context);
    }

    public static void trackClick(string screen, string element)
    {
        SKManager.Instance.TrackClick(screen: screen, element: element);
    }
    
    public static void trackClick(string screen, string element, string context)
    {
        SKManager.Instance.TrackClick(screen: screen, element: element, context: context);
    }
    
    public static void trackSoftIncome(long amount, long total)
    {
        SKManager.Instance.TrackSoftIncome(amount: amount, total: total);
    }
    
    public static void trackSoftIncome(long amount, long total, string place)
    {
        SKManager.Instance.TrackSoftIncome(amount: amount, total: total, place: place);
    }
    
    public static void trackSoftIncome(long amount, long total, string place, string extra)
    {
        SKManager.Instance.TrackSoftIncome(amount: amount, total: total, place: place, extra: extra);
    }
    
    public static void trackSoftIncome(long amount, long total, string place, string extra, string context)
    {
        SKManager.Instance.TrackSoftIncome(amount: amount, total: total, place: place, extra: extra, context: context);
    }

    public static void trackSoftOutcome(long amount, long total)
    {
        SKManager.Instance.TrackSoftOutcome(amount: amount, total: total);
    }
    
    public static void trackSoftOutcome(long amount, long total, string place)
    {
        SKManager.Instance.TrackSoftOutcome(amount: amount, total: total, place: place);
    }
    
    public static void trackSoftOutcome(long amount, long total, string place, string extra)
    {
        SKManager.Instance.TrackSoftOutcome(amount: amount, total: total, place: place, extra: extra);
    }
    
    public static void trackSoftOutcome(long amount, long total, string place, string extra, string context)
    {
        SKManager.Instance.TrackSoftOutcome(amount: amount, total: total, place: place, extra: extra, context: context);
    }

    public static void trackHardIncome(long amount, long total)
    {
        SKManager.Instance.TrackHardIncome(amount: amount, total: total);
    }
    
    public static void trackHardIncome(long amount, long total, string place)
    {
        SKManager.Instance.TrackHardIncome(amount: amount, total: total, place: place);
    }
    
    public static void trackHardIncome(long amount, long total, string place, string extra)
    {
        SKManager.Instance.TrackHardIncome(amount: amount, total: total, place: place, extra: extra);
    }
    
    public static void trackHardIncome(long amount, long total, string place, string extra, string context)
    {
        SKManager.Instance.TrackHardIncome(amount: amount, total: total, place: place, extra: extra, context: context);
    }

    public static void trackHardOutcome(long amount, long total)
    {
        SKManager.Instance.TrackHardOutcome(amount: amount, total: total);
    }

    public static void trackHardOutcome(long amount, long total, string place)
    {
        SKManager.Instance.TrackHardOutcome(amount: amount, total: total, place: place);
    }
    
    public static void trackHardOutcome(long amount, long total, string place, string extra)
    {
        SKManager.Instance.TrackHardOutcome(amount: amount, total: total, place: place, extra: extra);
    }
    
    public static void trackHardOutcome(long amount, long total, string place, string extra, string context)
    {
        SKManager.Instance.TrackHardOutcome(amount: amount, total: total, place: place, extra: extra, context: context);
    }

    public static void trackApplicationLoaded()
    {
        SKManager.Instance.TrackApplicationLoaded();
    }

    public static void trackEventWithoutInit(string eventName, string eventExtra1)
    {
        SKManager.Instance.TrackEventWithoutInit(name: eventName, extra1: eventExtra1);
    }

#if SAYKIT_PURCHASING

    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchaseEventArgs">A purchase that succeeded, including the purchased product along with its purchase receipt.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchase(PurchaseEventArgs purchaseEventArgs, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchaseEventArgs: purchaseEventArgs, validationCallback: validationCallback);
    }

    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchaseEventArgs">A purchase that succeeded, including the purchased product along with its purchase receipt.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchase(PurchaseEventArgs purchaseEventArgs, string placement,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchaseEventArgs: purchaseEventArgs, validationCallback: validationCallback, placement: placement);
    }
    
    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchaseEventArgs">A purchase that succeeded, including the purchased product along with its purchase receipt.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="extra">Extra param.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchase(PurchaseEventArgs purchaseEventArgs, string placement, string extra,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchaseEventArgs: purchaseEventArgs, validationCallback: validationCallback, placement: placement, extra: extra);
    }

    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchase(Product purchasedProduct, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback);
    }
    
    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    /// <param name="receipt">Receipt.</param>
    /// <param name="transactionId">Transaction Id.</param>
    public static void trackOrderPurchase(Product purchasedProduct, string receipt, string transactionId, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, receipt:receipt, transactionId:transactionId);
    }

    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchase(Product purchasedProduct, string placement, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, placement: placement);
    }
    
    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    /// <param name="receipt">Receipt.</param>
    /// <param name="transactionId">Transaction Id.</param>
    public static void trackOrderPurchase(Product purchasedProduct, string placement, string receipt, string transactionId, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, placement: placement, receipt:receipt, transactionId:transactionId);
    }
    
    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="extra">Extra param.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchase(Product purchasedProduct, string placement, string extra, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, placement: placement, extra: extra);
    }
    
    /// <summary>
    /// Tracks and validate in_app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="extra">Extra param.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    /// <param name="receipt">Receipt.</param>
    /// <param name="transactionId">Transaction Id.</param>
    public static void trackOrderPurchase(Product purchasedProduct, string placement, string extra, string receipt, string transactionId, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, placement: placement, extra: extra, receipt:receipt, transactionId:transactionId);
    }

    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchaseEventArgs">A purchase that succeeded, including the purchased product along with its purchase receipt.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchaseOffer(PurchaseEventArgs purchaseEventArgs, string offer,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchaseEventArgs: purchaseEventArgs, validationCallback: validationCallback, offer: offer);
    }
    
    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchaseEventArgs">A purchase that succeeded, including the purchased product along with its purchase receipt.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchaseOffer(PurchaseEventArgs purchaseEventArgs, string offer, string placement,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchaseEventArgs: purchaseEventArgs, validationCallback: validationCallback, offer: offer, placement: placement);
    }
    
    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchaseEventArgs">A purchase that succeeded, including the purchased product along with its purchase receipt.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="extra">Extra param.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchaseOffer(PurchaseEventArgs purchaseEventArgs, string offer, string placement, string extra,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchaseEventArgs: purchaseEventArgs, validationCallback: validationCallback, offer: offer, placement: placement, extra: extra);
    }

    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchaseOffer(Product purchasedProduct, string offer, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, offer: offer);
    }
    
    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    /// <param name="receipt">Receipt.</param>
    /// <param name="transactionId">Transaction Id.</param>
    public static void trackOrderPurchaseOffer(Product purchasedProduct, string offer, string receipt, string transactionId, Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, offer: offer, receipt:receipt, transactionId:transactionId);
    }

    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchaseOffer(Product purchasedProduct, string offer, string placement,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, offer: offer, placement: placement);
    }
    
    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    /// <param name="receipt">Receipt.</param>
    /// <param name="transactionId">Transaction Id.</param>
    public static void trackOrderPurchaseOffer(Product purchasedProduct, string offer, string placement, string receipt, string transactionId,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, offer: offer, placement: placement, receipt:receipt, transactionId:transactionId);
    }
    
    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="extra">Extra param.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    public static void trackPurchaseOffer(Product purchasedProduct, string offer, string placement, string extra,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, offer: offer, placement: placement, extra: extra);
    }
    
    /// <summary>
    /// Tracks and validate in-app purchase.
    /// </summary>
    /// <param name="purchasedProduct">The product which was purchased successfully.</param>
    /// <param name="offer">Offer data sends in in_app event.</param>
    /// <param name="placement">Placement.</param>
    /// <param name="extra">Extra param.</param>
    /// <param name="validationCallback">Callback with validation data.</param>
    /// <param name="receipt">Receipt.</param>
    /// <param name="transactionId">Transaction Id.</param>
    public static void trackOrderPurchaseOffer(Product purchasedProduct, string offer, string placement, string extra, string receipt, string transactionId,
        Action<SayKitInternal.InAppProduct> validationCallback = null)
    {
        SKManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct, validationCallback: validationCallback, offer: offer, placement: placement, extra: extra, receipt:receipt, transactionId:transactionId);
    }

    /// <summary>
    /// Returns the expiration timestamp of the subscription in seconds.(UTC) 
    /// </summary>
    /// <returns></returns>
    public static int GetSubscriptionExpirationTimestamp()
    {
        return SKManager.Instance.GetSubscriptionExpirationTimestamp();
    }

    /// <summary>
    /// iOS: Checks if the local App Store is available.
    /// Android: Not supported.
    /// </summary>
    /// <returns></returns>
    public static bool IsAppStoreAvailable()
    {
        return SKManager.Instance.IsAppStoreAvailable();
    }

#endif

    /// <summary>
    /// Returns the timestamp of the configuration request in UTC.
    /// </summary>
    /// <returns></returns>
    public static int GetRequestConfigTimestamp()
    {
        return SKManager.Instance.GetRequestConfigTimestamp();
    }

    // In-App Purchases
    public static bool isPremium => SKManager.Instance.IsPremium;

    public static void enablePremium()
    {
        SKManager.Instance.EnablePremium();
    }

    public static void disablePremium()
    {
        SKManager.Instance.DisablePremium();
    }

    // Rate App
    /// <summary>
    /// Show rate us pop up.
    /// </summary>
    /// <returns>True, if the popup has already been shown.</returns>
    public static bool showRateAppPopup()
    {
        return SKManager.Instance.ShowRateAppPopup();
    }

    /// <summary>
    /// Notify SayKit that the pop-up was shown.
    /// </summary>
    /// <param name="rate">Value of user rating from custom pop up.</param>
    /// <returns>True, if the popup has already been shown.</returns>
    public static bool showCustomRateAppPopup(int rate = 0)
    {
        return SKManager.Instance.ShowCustomRateAppPopup(rate: rate);
    }

    /// <summary>
    ///  Returns rate app popup status.
    /// </summary>
    /// <returns></returns>
    public static bool IsRateAppPopupShown()
    {
        return SKManager.Instance.IsRateAppPopupShown();
    }

    public static void revokeGdprConsent()
    {
        SKManager.Instance.RevokeGdprConsent();
    }

    public static bool? isGdprApplicable()
    {
        return SKManager.Instance.IsGdprApplicable();
    }
    /// <summary>
    /// Returns true if user granted consent after SayKit initialization.
    /// </summary>
    /// <returns></returns>
    public static bool GetGdprStatus()
    {
        return SKManager.Instance.GetGdprStatus();
    }
    
    public static void RequestNotificationToken()
    {
        SKManager.Instance.RequestNotificationToken();
    }

    /// <summary>
    /// Returns remote notification token. 
    /// </summary>
    /// <returns></returns>
    public static string GetNotificationToken()
    {
        return SKManager.Instance.GetNotificationToken();
    }

    /// <summary>
    /// Returns a localized string corresponding to the given key.
    /// Placeholders in the string (e.g., {1}, {2}) will be replaced by the provided values.
    /// </summary>
    /// <param name="key">The key used to retrieve the corresponding localized string.</param>
    /// <param name="val1">Optional placeholder value to replace {1}.</param>
    /// <param name="val2">Optional placeholder value to replace {2}.</param>
    /// <param name="val3">Optional placeholder value to replace {3}.</param>
    /// <param name="val4">Optional placeholder value to replace {4}.</param>
    /// <param name="val5">Optional placeholder value to replace {5}.</param>
    /// <returns>The localized string with placeholders replaced.</returns>
    public static string getLocalizedString(string key, string val1 = null, string val2 = null, string val3 = null,
        string val4 = null, string val5 = null)
    {
        return SKManager.Instance.GetLocalizedString(key: key, val1: val1, val2: val2, val3: val3, val4: val4, val5: val5);
    }

    /// <summary>
    /// Returns a tuple containing a localized string and a boolean flag indicating whether the message is a default value.
    /// Placeholders in the string (e.g., {1}, {2}) will be replaced by the provided values.
    /// </summary>
    /// <param name="key">The key used to retrieve the corresponding localized string.</param>
    /// <param name="val1">Optional placeholder value to replace {1}.</param>
    /// <param name="val2">Optional placeholder value to replace {2}.</param>
    /// <param name="val3">Optional placeholder value to replace {3}.</param>
    /// <param name="val4">Optional placeholder value to replace {4}.</param>
    /// <param name="val5">Optional placeholder value to replace {5}.</param>
    /// <returns>A tuple containing the localized string and a boolean flag.
    /// The flag is true if the message is a default value; otherwise, false.</returns>
    public static (string, bool) getLocalizedTuple(string key, string val1 = null, string val2 = null,
        string val3 = null, string val4 = null, string val5 = null)
    {
        return SKManager.Instance.GetLocalizedTuple(key: key, val1: val1, val2: val2, val3: val3, val4: val4, val5: val5);
    }

    /// <summary>
    /// Checks if a localized message exists for the specified key.
    /// </summary>
    /// <param name="key">The key used to retrieve the corresponding localized string.</param>
    /// <returns>True if the message exists, otherwise false.</returns>
    public static bool hasLocalizedMessage(string key)
    {
        return SKManager.Instance.HasLocalizedMessage(key: key);
    }

    /// <summary>
    /// Returns value can include language code (e.g., "en", "ru", "de") or
    /// the language code and the country/region code (e.g., "en_us", "fr_us").
    /// </summary>
    /// <returns>The current language code (e.g., "en_us", "ru", "de").</returns>
    public static string GetFullCurrentLanguage()
    {
        return SKManager.Instance.GetFullCurrentLanguage();
    }
    
    /// <summary>
    /// Returns tuple with language code in IETF format (e.g., "en-US", "ru-RU")
    /// along with its corresponding <see cref="CultureInfo"/>.
    /// The language code in IETF format (e.g., "en-US", "ru-RU").
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// - The language code in IETF format (e.g., "en-US", "ru-RU").
    /// - The corresponding <see cref="CultureInfo"/> object.
    /// </returns>
    public static (string, CultureInfo) GetFullCurrentLanguageWithCultureInfo()
    {
        return SKManager.Instance.GetFullCurrentLanguageWithCultureInfo();
    }
    
    /// <summary>
    /// Returns the current language.
    /// </summary>
    /// <returns>
    /// The current <see cref="SayKitLanguage"/> being used.
    /// </returns>
    public static SayKitLanguage getCurrentLanguage()
    {
        return SKManager.Instance.GetCurrentSayKitLanguage();
    }
    
    /// <summary>
    /// DEBUG only method!
    /// Changed current language in runtime.
    /// </summary>
    /// <param name="language">The language code (e.g., "en", "ru", "de") or
    /// the language code and the country/region code (e.g., "en_us", "fr_us")
    /// </param>
    /// <param name="OnLanguageChanged">
    /// Optional callback invoked after the operation completes:
    /// <c>true</c> if the language was found and applied; <c>false</c> otherwise
    /// </param>
    public static void changeLanguage(string language, Action<bool> OnLanguageChanged = null)
    {
        SKManager.Instance.ChangeLanguage(language: language, OnLanguageChanged: OnLanguageChanged);
    }
    
    /// <summary>
    /// DEBUG only method!
    /// Changed current language in runtime.
    /// </summary>
    /// <param name="language">The <see cref="SayKitLanguage"/> to switch to.</param>
    /// <param name="OnLanguageChanged">
    /// Optional callback invoked after the operation completes:
    /// <c>true</c> if the language was found and applied; <c>false</c> otherwise
    /// </param>
    public static void changeLanguage(SayKitLanguage language, Action<bool> OnLanguageChanged = null)
    {
        SKManager.Instance.ChangeLanguage(language: language, OnLanguageChanged: OnLanguageChanged);
    }

    /// <summary>
    /// DEBUG only method!
    /// Changes the current language at runtime using the specified <see cref="CultureInfo"/>.
    /// </summary>
    /// <param name="cultureInfo">
    /// The <see cref="CultureInfo"/> representing the desired language and region
    /// (e.g., <c>new CultureInfo("en-US")</c>, <c>new CultureInfo("ru-RU")</c>).
    /// </param>
    /// <param name="OnLanguageChanged">
    /// Optional callback invoked after the operation completes:
    /// <c>true</c> if the language was found and successfully applied; <c>false</c> otherwise.
    /// </param>
    public static void changeLanguage(CultureInfo cultureInfo, Action<bool> OnLanguageChanged = null)
    {
        SKManager.Instance.ChangeLanguage(cultureInfo: cultureInfo, OnLanguageChanged: OnLanguageChanged);
    }

    /// <summary>
    /// Overrides the system language and saves it in future sessions.
    /// </summary>
    /// <param name="language">The <see cref="SayKitLanguage"/> to override to.</param>
    /// <returns>
    /// <c>true</c> if the override was applied successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool overrideSystemLanguage(SayKitLanguage language)
    {
       return SKManager.Instance.OverrideSystemLanguage(language: language);
    }

    /// <summary>
    /// Overrides the system language and saves it in future sessions.
    /// </summary>
    /// <param name="language">The language code (e.g., "en", "ru", "de") or
    /// the language code and the country/region code (e.g., "en_us", "fr_us")</param>
    /// <returns>
    /// <c>true</c> if the override was applied successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool overrideSystemLanguage(string language)
    {
       return SKManager.Instance.OverrideSystemLanguage(language: language);
    }
    
    /// <summary>
    /// Overrides the system language and saves it in future sessions.
    /// </summary>
    /// <param name="cultureInfo">
    /// The <see cref="CultureInfo"/> representing the desired language and region
    /// (e.g., <c>new CultureInfo("en-US")</c>, <c>new CultureInfo("ru-RU")</c>).
    /// </param>
    /// <returns>
    /// <c>true</c> if the override was applied successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool overrideSystemLanguage(CultureInfo cultureInfo)
    {
       return SKManager.Instance.OverrideSystemLanguage(cultureInfo: cultureInfo);
    }
    
    public static void startFPSTag(string tagName)
    {
        SKManager.Instance.StartFPSTag(tagName: tagName);
    }

    public static void endFPSTag(string tagName)
    {
        SKManager.Instance.EndFPSTag(tagName: tagName);
    }

    public static void startTimeFPSTag(string tagName, int seconds)
    {
        SKManager.Instance.StartTimeFPSTag(tagName: tagName, seconds: seconds);
    }
    
    /// <summary>
    /// DEBUG only method!
    /// </summary>
    public static void overrideTrackLevel(int level)
    {
        SKManager.Instance.OverrideTrackLevel(level: level);
    }
    
    public static void requestConfigMigration(string sourceVersion)
    {
        SKManager.Instance.RequestConfigMigration(sourceVersion: sourceVersion);
    }

    /// <summary>
    /// Updates remote config manually.
    /// You need to provide remoteConfigUpdated action in SayKitConfig if you need to get a callback.
    /// Call it no faster than once in 15 seconds!
    /// </summary>
    /// <returns> True if update config starts </returns>
    public static bool RequestRemoteConfigUpdate()
    {
        return SKManager.Instance.RequestRemoteConfigUpdate();
    }
    
    /// <summary>
    /// [Android] Opens a link to Google Play's Subscription Center.
    /// </summary>
    /// <param name="productId"></param>
    public static void OpenGooglePlaySubscriptionCenter(string productId)
    {
       SKManager.Instance.OpenGooglePlaySubscriptionCenter(productId: productId);
    }

    /// <summary>
    /// Returns the total accessible memory of the device. 
    /// </summary>
    /// <returns></returns>
    public static int GetTotalMemory()
    {
        return SKManager.Instance.GetTotalMemory();
    }

    /// <summary>
    /// Returns the available memory of the device.
    /// </summary>
    /// <returns></returns>
    public static int GetFreeMemory()
    {
        return SKManager.Instance.GetFreeMemory();
    }

    /// <summary>
    /// Sets context to fps events.
    /// </summary>
    /// <param name="data">Context</param>
    public static void SetFPSGameContext(string data)
    {
        SKManager.Instance.SetFPSGameContext(data: data);
    }

    /// <summary>
    /// Sets context to unity exception events.
    /// </summary>
    /// <param name="data">Context</param>
    public static void SetUnityExceptionContext(string data)
    {
        SKManager.Instance.SetUnityExceptionContext(data: data);
    }

    public static void DisableSayKitLogs()
    {
        SKManager.Instance.DisableSayKitLogs();
    }

    /// <summary>
    /// Tracks in-app offer event
    /// </summary>
    /// <param name="place">Placement</param>
    /// <param name="extra">Extra data</param>
    public static void TrackInAppOffer(string place, string extra)
    {
        SKManager.Instance.TrackInAppOffer(place: place, extra: extra);
    }

    /// <summary>
    /// Open Max mediation debug window
    /// </summary>
    public static void ShowMaxMediationDebug()
    {
        SKManager.Instance.ShowMaxMediationDebug();
    }

    /// <summary>
    /// Returns user debug mode status.
    /// </summary>
    /// <returns></returns>
    public static bool IsDebugUser()
    {
        return SKManager.Instance.IsDebugUser();
    }

    /// <summary>
    /// Sets player id 
    /// </summary>
    /// <param name="playerId">Player id</param>
    public static void SetPlayerId(string playerId)
    {
        SKManager.Instance.SetPlayerId(playerId: playerId);
    }

    /// <summary>
    /// Returns the user playing time in seconds
    /// </summary>
    public static int GetPlayingTime()
    {
        return SKManager.Instance.GetPlayingTime();
    }

    /// <summary>
    /// Send request to live server
    /// </summary>
    /// <param name="requestName">Request name</param>
    /// <param name="requestData">Request body data</param>
    /// <param name="onRequestResult">Request response callback</param>
    public static void RequestLiveServer(string requestName, string requestData, Action<string> onRequestResult)
    {
        SKManager.Instance.RequestLiveServer(requestName: requestName, requestData: requestData, onRequestResult: onRequestResult);
    }

    /// <summary>
    /// Send payer_prediction request to the live server.
    /// </summary>
    /// <param name="onRequestResult">LiveRequestPayerPrediction data callback</param>
    public static void RequestPayerPrediction(Action<LiveRequestPayerPrediction> onRequestResult)
    {
        SKManager.Instance.RequestPayerPrediction(onRequestResult: onRequestResult);
    }

    /// <summary>
    /// Sets experiment device id 
    /// </summary>
    /// <param name="deviceId">Device id.</param>
    public static void SetExperimentDeviceId(string deviceId)
    {
        SKManager.Instance.SetExperimentDeviceId(deviceId: deviceId);
    }

    /// <summary>
    /// Returns the session id.
    /// </summary>
    public static string GetSessionId()
    {
        return SKManager.Instance.GetSessionId();
    }

    /// <summary>
    /// Retrieves attribution data as a AttributionResponseData object.
    /// </summary>
    /// <returns>A AttributionResponseData containing status, creative, and campaign details.</returns>
    public static AttributionResponseData GetAttributionData()
    {
        return SKManager.Instance.GetAttributionData();
    }

    /// <summary>
    /// Sets the game context.
    /// </summary>
    /// <param name="gameContext">Game context.</param>
    public static void SetGameContext(string gameContext)
    {
        SKManager.Instance.SetGameContext(gameContext: gameContext);
    }
    
    /// <summary>
    /// Sets firebase user id.
    /// </summary>
    /// <param name="firebaseUserId">Firebase user id.</param>
    public static void SetFirebaseUserId(string firebaseUserId)
    {
        SKManager.Instance.SetFirebaseUserId(firebaseUserId: firebaseUserId);
    }
    
    /// <summary>
    /// Opens the specified URL.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public static void OpenCustomUrl(string url)
    {
        SKManager.Instance.OpenCustomUrl(url: url);
    }
    
    /// <summary>
    /// Returns whether the banner ad is enabled.
    /// </summary>
    public static bool IsBannerEnabled()
    {
        return SKManager.Instance.IsBannerEnabled();
    }

#if SAYKIT_BILLING

    /// <summary>
    /// Fetches a list of available products for the specified product IDs.
    /// </summary>
    /// <param name="productInfos">Array of product infos.</param>
    /// <param name="onAvailableProductsFetched">
    /// Callback invoked with two parameters:
    /// 1) <see cref="SKProduct"/>[] — the fetched products; can be <c>null</c> (on failure) or an empty array (no products found).<br/>
    /// 2) <see cref="SKBillingError"/> — error details; <c>null</c> if no error occurred.
    /// </param>
    public static void GetAvailableProducts(SKProductInfo[] productInfos, Action<SKProduct[], SKBillingError> onAvailableProductsFetched)
    {
        SKManager.Instance.GetAvailableProducts(productInfos, onAvailableProductsFetched);
    }

    /// <summary>
    /// Retrieves purchased products that have not been confirmed yet.
    /// </summary>
    /// <param name="onNonConfirmedProductsFetched">
    /// Callback invoked with two parameters:
    /// 1) <see cref="SKPurchasedProduct"/>[] — non-confirmed purchases; can be <c>null</c> or empty.<br/>
    /// 2) <see cref="SKBillingError"/> — error details; <c>null</c> if no error occurred.
    /// </param>
    public static void GetNonConfirmedProducts(Action<SKPurchasedProduct[], SKBillingError> onNonConfirmedProductsFetched)
    {
        SKManager.Instance.GetNonConfirmedProducts(onNonConfirmedProductsFetched);
    }

    /// <summary>
    /// Retrieves products that have already been purchased.
    /// </summary>
    /// <param name="onPurchasedProductsFetched">
    /// Callback invoked with three parameters:
    /// 1) <see cref="string"/>[] — purchased product IDs; can be <c>null</c> or empty if none are available.<br/>
    /// 2) <see cref="SKInAppSubscription"/>[] — active subscriptions; can be <c>null</c> or empty if none are available.<br/>
    /// 3) <see cref="SKBillingError"/> — error details; <c>null</c> if no error occurred.
    /// </param>
    public static void GetPurchasedProducts(Action<string[], SKInAppSubscription[], SKBillingError> onPurchasedProductsFetched)
    {
        SKManager.Instance.GetPurchasedProducts(onPurchasedProductsFetched);
    }

    /// <summary>
    /// Initiates a purchase product flow.
    /// </summary>
    /// <param name="productInfo">The product to purchase.</param>
    /// <param name="onPurchaseProductCompleted">
    /// Callback invoked with three parameters:
    /// 1) <see cref="bool"/> — indicates whether the purchase succeeded.<br/>
    /// 2) <see cref="SKPurchasedProduct"/> — the purchased product data; can be <c>null</c> on failure or cancellation.<br/>
    /// 3) <see cref="SKBillingError"/> — error details; <c>null</c> if no error occurred.
    /// </param>
    /// <param name="options">Optional purchase options (can be <c>null</c>).</param>
    /// <param name="offer">Optional offer identifier (can be <c>null</c>).</param>
    /// <param name="placement">Optional placement (can be <c>null</c>).</param>
    /// <param name="extra">Optional extra param (can be <c>null</c>).</param>
    public static void PurchaseProduct(SKProductInfo productInfo, Action<bool, SKPurchasedProduct, SKBillingError> onPurchaseProductCompleted,
        SKPurchaseOptions options = null, string offer = null, string placement = null, string extra = null)
    {
        SKManager.Instance.PurchaseProduct(productInfo: productInfo, offer: offer, placement: placement, 
            onPurchaseProductCompleted: onPurchaseProductCompleted, options: options, extra: extra);
    }

    /// <summary>
    /// Confirms previously completed purchase.
    /// </summary>
    /// <param name="purchasedProduct">The purchased product to confirm.</param>
    /// <param name="onConfirmProductCompleted">
    /// Callback invoked with two parameters:
    /// 1) <see cref="bool"/> — indicates whether confirmation succeeded.<br/>
    /// 2) <see cref="SKBillingError"/> — error details; <c>null</c> if no error occurred.
    /// </param>
    public static void ConfirmPurchase(SKPurchasedProduct purchasedProduct, Action<bool, SKBillingError> onConfirmProductCompleted)
    {
        SKManager.Instance.ConfirmPurchase(purchasedProduct: purchasedProduct, onConfirmProductCompleted: onConfirmProductCompleted);
    }

    /// <summary>
    /// Restores previous purchases.
    /// </summary>
    /// <param name="onRestorePurchasesCompleted">
    /// Callback invoked with three parameters:
    /// 1) <see cref="string"/>[] — restored product IDs; can be <c>null</c> or empty.<br/>
    /// 2) <see cref="SKInAppSubscription"/>[] — restored subscriptions; can be <c>null</c> or empty.<br/>
    /// 3) <see cref="SKBillingError"/> — error details; <c>null</c> if no error occurred.
    /// </param>
    /// <remarks>
    /// The user may want to restore all their non-consumable and subscription purchases at any time.
    /// However, you shouldn't automatically do it every time
    /// an app loads; instead, provide a "Restore" button that the user clicks when they want to restore purchases.
    /// </remarks>
    public static void RestorePurchases(Action<string[], SKInAppSubscription[], SKBillingError> onRestorePurchasesCompleted)
    {
        SKManager.Instance.RestorePurchases(onRestorePurchasesCompleted: onRestorePurchasesCompleted);
    }
    
    /// <summary>
    /// Opens the Web Shop.
    /// </summary>
    /// <param name="externalId">
    /// The unique identifier of the offer, campaign, or user.
    /// </param>
    /// <param name="context">
    /// Additional param describing the context.
    /// </param>
    public static void ShowWebShop(string externalId, string context)
    {
        SKManager.Instance.ShowWebShop(externalId: externalId, context: context);
    }

    /// <summary>
    /// Sets the web shop parameters. Should use before SayKit initialization. 
    /// </summary>
    /// <param name="externalId">
    /// The unique identifier of the offer, campaign, or user.
    /// </param>
    /// <param name="context">
    /// Additional param describing the context.
    /// </param>
    public static void SetWebShopParams(string externalId, string context)
    {
        SKManager.Instance.SetWebShopParams(externalId: externalId, context: context);
    }
    
#endif
    
    /// <summary>
    /// Requests the current server timestamp.
    /// </summary>
    /// <remarks>
    /// This method can be called no more than once every <b>5 seconds</b>.
    /// </remarks>
    /// <param name="onTimestampReceived">
    /// Callback invoked when the operation completes.  
    /// The first argument indicates whether the request succeeded. 
    /// The second argument contains the server timestamp or <c>0</c> on error.
    /// </param>
    public static void GetServerTimestamp(Action<bool, long> onTimestampReceived)
    {
        SKManager.Instance.GetServerTimestamp(onTimestampReceived);
    }

    /// <summary>
    /// Schedules a local notification by adding it to the notification dispatcher.
    /// </summary>
    /// <param name="localNotification">The local notification to be scheduled.</param>
    public static void ScheduleLocalNotification(SKLocalNotification localNotification)
    {
        SKManager.Instance.ScheduleLocalNotification(localNotification: localNotification);
    }

    /// <summary>
    /// Removes all delivered notifications from the notification dispatcher.
    /// </summary>
    public static void RemoveAllDeliveredLocalNotifications()
    {
        SKManager.Instance.RemoveAllDeliveredLocalNotifications();
    }

    /// <summary>
    /// Removes a specific delivered notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the delivered notification to be removed.</param>
    public static void RemoveDeliveredLocalNotification(string id)
    {
        SKManager.Instance.RemoveDeliveredLocalNotification(id: id);
    }

    /// <summary>
    /// Removes all scheduled notifications from the notification dispatcher.
    /// </summary>
    public static void RemoveAllScheduledLocalNotifications()
    {
        SKManager.Instance.RemoveAllScheduledLocalNotifications();
    }

    /// <summary>
    /// Removes a specific scheduled notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the scheduled notification to be removed.</param>
    public static void RemoveScheduledLocalNotification(string id)
    {
        SKManager.Instance.RemoveScheduledLocalNotification(id: id);
    }
}