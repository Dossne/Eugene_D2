using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

#if SAYKIT_PURCHASING
using UnityEngine.Purchasing;
#endif

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable RedundantJumpStatement
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
// ReSharper disable ConstantConditionalAccessQualifier

#endregion

namespace SayKitInternal
{
    public class SKManager
    {
        public int Version = 2025111900;

        private static SKManager _instance;
       
        public int InitCount;
        private EInitState _initState = EInitState.None;
        private long _lastGetServerTimestampMs;

        private const string Tag = "[SKManager]";

        public static SKManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SKManager();
                }

                return _instance;
            }
        }
        
        public SayKitConfig Config { get; private set; } = new SayKitConfig();

        public RemoteConfig RemoteConfig => RemoteConfigManager.Instance.Config;

        public RuntimeInfo RuntimeInfo
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<RuntimeInfo>(SKBridgeManager.Instance.GetRuntimeInfo());
                }
                catch (Exception e)
                {
                    SayKitDebug.LogError($"RuntimeInfo message: {e.Message}, stacktrace: {e.StackTrace}");
                }

                return new RuntimeInfo();
            }
        }

        public SayKitGameConfig GameConfig => RemoteConfig.game_settings ?? new SayKitGameConfig();

        public string BuildVersion => SKBridgeManager.Instance.GetAppVersionFullName();
        
        public EInitState GetInitState => _initState;

        public bool IsInitialized => _initState == EInitState.Done;
        public float InitializedProgress { get; private set; }

        internal void UpdateInitState(int number, float progress)
        {
            InitializedProgress = progress;
            switch (number)
            {
                case 1:
                    _initState = EInitState.None;
                    break;
                case 2:
                    _initState = EInitState.Platform;
                    break;
                case 3:
                    _initState = EInitState.RemoteConfig;
                    break;
                case 4:
                    _initState = EInitState.Consent;
                    break;
                case 5:
                    _initState = EInitState.IDFA;
                    break;
                case 6:
                    _initState = EInitState.Gdpr;
                    SKBridgeManager.Instance.TrackEvent(name: "unity_engine", extra1: Application.unityVersion);
                    SKBridgeManager.Instance.TrackEvent(name: "sk_version_unity", param1: Version);
#if SAYKIT_PURCHASING                    
                    SKBridgeManager.Instance.TrackEvent(name: "sk_version_iap", extra1: SKUtils.GetIAPVersion());
#endif
                    
#if SAYKIT_SMART_INTER
                    SKBridgeManager.Instance.TrackEvent(name: "sk_smart_flow");
#endif
                    break;
                case 7:


                    OnSayKitInitialized();

#if SAYKIT_CHINA_VERSION
                    SKAgeVerificationService.GetInstance().StartVerification(() =>
                    {
                        _initState = EInitState.Done;
                    });
#else
                    _initState = EInitState.Done;
#endif


#if !SAYKIT_CROSSPROMO_DISABLED
                    CrossPromo.Init();
#endif
                    break;
            }
        }

        public void Initialize(SayKitConfig sayKitConfig)
        {
            if (InitCount > 0)
            {
                InitCount++;
                Debug.LogError("SayKit Initialize method has already been called.");
            }
            else
            {
                InitCount++;

#if SAYKIT_DEBUG && SAYKIT_LOGLEVEL_DEBUG
                SayKitDebug.InitDebugLogs(true);
#endif

                DebugService.Instance.startInitTimeStamp = SKUtils.currentTimestamp;
                UIManager.Init();
                Config = sayKitConfig;
                SKLocalizationService.Instance.InitializeLocalGameMessages();

                // Connect callbacks
                SayKitBridgeCallbacks.Instance.Initialize();

#if SAYKIT_BANNER_DISABLED
                sayKitConfig.disableAutoBanner = true;
#endif

                // Start init
                var configDto = new SayKitConfigDTO()
                {
                    AppKey = sayKitConfig.appKey,
                    BannerAdUnitId = sayKitConfig.bannerAdUnitId,
                    InterstitialAdUnitId = sayKitConfig.interstitialAdUnitId,
                    RewardedAdUnitId = sayKitConfig.rewardedAdUnitId,
                    AttributionConfigUpdate = sayKitConfig.attributionConfigUpdate,
                    OverrideSystemLanguage = SayKitLanguageConverter.ConvertFromSayKitLanguage(sayKitConfig.overrideSystemLanguage),
                    OverrideAnalyticSegment = sayKitConfig.overrideAnalyticSegment,
                    DisableAutoBanner = sayKitConfig.disableAutoBanner,
                    DisableAutoBannerTimeouts = sayKitConfig.disableAutoBannerTimeouts,
                    DisableInterstitial = sayKitConfig.disableInterstitial,
                    CustomNotificationRequest = sayKitConfig.customNotificationRequest,

#if SAYKIT_BANNER_DISABLED
                    DisableBanner = true,
#else
                    DisableBanner = false,
#endif

#if SAYKIT_DISABLE_FACEBOOK_AUTOLOG
                    FacebookAutoLoggingEnabled = false
#endif
                    
#if SAYKIT_BILLING
                    DisableBilling = false,
                    DisablePurchaseValidation = sayKitConfig.disablePurchaseValidation
#endif
                };
                
                var configJson = JsonConvert.SerializeObject(configDto);
                var environmentParams = SKUtils.GetScriptingDefineSymbols();
                SKBridgeManager.Instance.InitIfNeeded(configJson: configJson, environmentParams: environmentParams);
            }
        }

        private void OnSayKitInitialized()
        {
            UIManager.Instance.StartCoroutine(FpsCounter.ReportFpsRoutine());

            if (RemoteConfig.runtime.fps_tag_enabled == 1)
            {
                UIManager.Instance.StartCoroutine(PerfomanceManager.StartFPSRoutine(
                    RemoteConfig.runtime.fps_tag_enabled, RemoteConfig.runtime.fps_tag_min_rate
                    , RemoteConfig.runtime.fps_tag_min_spikes));
            }

            if (InitCount > 1)
            {
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: $"SayKit.init() method was called {InitCount} times.");
            }
        }

        #region In-App Purchases

        public bool IsPremium => SKBridgeManager.Instance.IsPremium();

        public void EnablePremium()
        {
            SKBridgeManager.Instance.EnablePremium();
        }

        public void DisablePremium()
        {
            SKBridgeManager.Instance.DisablePremium();
        }
        
#if SAYKIT_PURCHASING
        public void TrackPurchase(PurchaseEventArgs purchaseEventArgs, Action<InAppProduct> validationCallback,
            string offer = null, string placement = null, string extra = null)
        {
            InAppManager.Instance.TrackPurchase(purchaseEventArgs: purchaseEventArgs, validationCallback: validationCallback, offer: offer,
                placement: placement, extra: extra);
        }

        public void TrackPurchase(Product purchasedProduct, Action<InAppProduct> validationCallback,
            string offer = null, string placement = null, string extra = null, string receipt = null,
            string transactionId = null)
        {
            InAppManager.Instance.TrackPurchase(purchasedProduct: purchasedProduct,
                validationCallback: validationCallback, offer: offer, placement: placement, extra: extra,
                orderReceipt: receipt, orderTransactionId: transactionId);
        }

        public int GetSubscriptionExpirationTimestamp()
        {
            return SKBridgeManager.Instance.GetSubscriptionExpirationTimestamp();
        }

        public bool IsAppStoreAvailable()
        {
            return SKBridgeManager.Instance.IsAppStoreAvailable();
        }
#endif

        #endregion

        #region Ads

        public void ShowBanner(bool isExternalCall = false)
        {
            if (!IsInitialized)
            {
                return;
            }

#if UNITY_EDITOR
            return;
#else
            if (DebugService.Instance.BannerDisabled)
            {
                return;
            }
            else
            {
                AdsManager.Instance.ShowBanner();
            }
#endif
        }

        public void HideBanner(bool isExternalCall = false)
        {
            if (!IsInitialized)
            {
                return;
            }

#if UNITY_EDITOR
            return;
#else
            AdsManager.Instance.HideBanner();
#endif
        }

        public float GetBackgroundBannerSize()
        {
            return SKBridgeManager.Instance.GetBackgroundBannerSize();
        }

        public bool IsInterstitialAvailable(string place, int countdown = 0)
        {
#if UNITY_EDITOR
            return true;
#else
            if (DebugService.Instance.InterstitialDisabled)
            {
                return true;
            }

            return AdsManager.Instance.IsInterstitialAvailable(place: place, countdown: countdown);
#endif
        }

        public bool IsRewardedAvailable(string place)
        {
#if UNITY_EDITOR
            return true;
#else
            if (DebugService.Instance.RewardedDisabled)
            {
                return true;
            }
            else
            {
                return AdsManager.Instance.IsRewardedAvailable(place: place);
            }
#endif
        }
        
        public bool IsRewardedPlacementAvailable(string place)
        {
#if UNITY_EDITOR
            return true;
#else
            if (DebugService.Instance.RewardedDisabled)
            {
                return true;
            }
            else
            {
                return AdsManager.Instance.IsRewardedPlacementAvailable(place: place);
            }
#endif
        }
        
        public bool ShowInterstitial(string place, Action onCloseCallback = null)
        {
#if UNITY_EDITOR
            onCloseCallback?.Invoke();
            return true;
#else
            if (DebugService.Instance.InterstitialDisabled)
            {
                onCloseCallback?.Invoke();
                return true;
            }

            return AdsManager.Instance.ShowInterstitial(place: place, onCloseCallback: onCloseCallback);
#endif
        }

        public bool ShowInterstitialWithPopup(string place, int countdown, Action onShowCallback = null, Action onCloseCallback = null)
        {
#if UNITY_EDITOR
            onShowCallback?.Invoke();
            onCloseCallback?.Invoke();
            return true;
#else
            if (DebugService.Instance.InterstitialDisabled)
            {
                onShowCallback?.Invoke();
                onCloseCallback?.Invoke();
                return true;
            }

            if (RemoteConfig.ads_settings.sk_interstitial_popup_enabled == 0)
            {
                return AdsManager.Instance.ShowInterstitial(place: place, onCloseCallback: onCloseCallback, onShowCallback: onShowCallback);
            }

            return AdsManager.Instance.ShowInterstitialWithTimerPopUp(place: place, countdown: countdown, onShowCallback: onShowCallback, onCloseCallback: onCloseCallback);
#endif
        }

        public void ShowRewarded(string place, Action<bool> onCloseCallback)
        {
#if UNITY_EDITOR
            onCloseCallback?.Invoke(true);
#else
            if (DebugService.Instance.RewardedDisabled)
            {
                onCloseCallback?.Invoke(true);
            }
            else
            {
                AdsManager.Instance.ShowRewarded(place: place, onCloseCallback: onCloseCallback);
            }
#endif
        }

        public void TrackInterstitialOffer(string place, string extra)
        {
            SKUtils.CheckInit(() =>
            {
                SKBridgeManager.Instance.TrackInterstitialOffer(place: place, extra: extra);
            });
        }

        public void TrackRewardedOffer(string place, string extra)
        {
           SKUtils.CheckInit(() =>
           {
               SKBridgeManager.Instance.TrackRewardedOffer(place: place, extra: extra);
           });
        }

        #endregion

        #region Facebook, Firebase

        public bool IsFacebookSDKInitialized()
        {
            return SKBridgeManager.Instance.IsFacebookSdkInitialized();
        }

        public void LogFacebookEvent(string logEvent, float valueToSum, Dictionary<string, object> parameters = null)
        {
            var json = "";
            if (parameters != null)
            {
                json = JsonConvert.SerializeObject(parameters, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            }

            SKBridgeManager.Instance.LogFacebookEvent(name: logEvent, valueToSum: valueToSum, paramsJson: json);
        }

        public void LogFirebaseEvent(string eventName, string paramName, object paramValue)
        {
            if (paramName?.Length > 0)
            {
                LogFirebaseEvent(
                    eventName: eventName,
                    valueToSum: 0,
                    parameters: new Dictionary<string, object>
                    {
                        { paramName, paramValue }
                    }
                );
            }
            else
            {
                LogFirebaseEvent(eventName: eventName, valueToSum: 0);
            }
        }

        public void LogFirebaseEvent(string eventName, float valueToSum, Dictionary<string, object> parameters = null)
        {
            var json = "";
            if (parameters != null)
            {
                json = JsonConvert.SerializeObject(parameters, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            }

            SKBridgeManager.Instance.LogFirebaseEvent(name: eventName, valueToSum: valueToSum, paramsJson: json);
        }

        public void LogFirebaseEvent(string eventName, string extraParam = "")
        {
            if (extraParam?.Length > 0)
            {
                LogFirebaseEvent(
                    eventName: eventName,
                    valueToSum: 0,
                    parameters: new Dictionary<string, object>
                    {
                        { "extra", extraParam }
                    }
                );
            }
            else
            {
                LogFirebaseEvent(eventName: eventName, valueToSum: 0);
            }
        }

        public void SetCrashlyticsParam(string paramName, string paramValue)
        {
            SKBridgeManager.Instance.SetCrashlyticsParam(key: paramName, value: paramValue);
        }

        public void LogCrashlyticsException(Exception exception)
        {
            SKBridgeManager.Instance.LogCrashlyticsException(exception: exception);
        }

        public void LogCrashlytics(string message)
        {
            SKBridgeManager.Instance.LogCrashlytics(message: message);
        }

        public void SetFirebaseUserProperty(string key, string value = "")
        {
            SKBridgeManager.Instance.SetFirebaseUserProperty(key: key, value: value);
        }

        #endregion

        #region Events

        public void TrackLevelStarted(string tag = "", int level = 0, long score = 0, string extra1 = "", int number = 0,
            string extra2 = "", string context = null)
        {
            SKBridgeManager.Instance.TrackLevelStarted(level: level, score: score, number: number, extra1: extra1, extra2: extra2, tag: tag,
                context: context);
            PerfomanceManager.StartTag("level", level.ToString());
            SKPerformanceTrackerService.Start("level", "level_started");
        }

        public void TrackLevelCompleted(string tag = "", int level = 0, long score = 0, string extra1 = "", int number = 0,
            string extra2 = "", string context = null)
        {
            SKBridgeManager.Instance.TrackLevelCompleted(level: level, score: score, number: number, extra1: extra1, extra2: extra2, tag: tag,
                context: context);
            PerfomanceManager.EndTag("level");
            SKPerformanceTrackerService.End("level", "level_completed");
        }

        public void TrackLevelFailed(string tag = "", int level = 0, long score = 0, string extra1 = "", int number = 0,
            string extra2 = "", string context = null)
        {
            SKBridgeManager.Instance.TrackLevelFailed(level: level, score: score, number: number, extra1: extra1, extra2: extra2, tag: tag,
                context: context);
            PerfomanceManager.EndTag("level");
            SKPerformanceTrackerService.End("level", "level_failed");
        }

        public void TrackLevelExtraStarted(string tag = "", int number = 0, string extra1 = "", string extra2 = "", string context = null)
        {
            SKBridgeManager.Instance.TrackLevelExtraStarted(number: number, extra1: extra1, extra2: extra2, tag: tag, context: context);
            PerfomanceManager.StartTag("level", "extraLevel");
            SKPerformanceTrackerService.Start("extra", "extra_started");
        }

        public void TrackLevelExtraCompleted(string tag = "", long score = 0, int number = 0, string extra1 = "",
            string extra2 = "", string context = null)
        {
            SKBridgeManager.Instance.TrackLevelExtraCompleted(score: score, number: number, extra1: extra1, extra2: extra2, tag: tag,
                context: context);
            PerfomanceManager.EndTag("level");
            SKPerformanceTrackerService.End("extra", "extra_completed");
        }

        public void TrackLevelExtraFailed(string tag = "", long score = 0, int number = 0, string extra1 = "", string extra2 = "",
            string context = null)
        {
            SKBridgeManager.Instance.TrackLevelExtraFailed(score: score, number: number, extra1: extra1, extra2: extra2, tag: tag, context: context);
            PerfomanceManager.EndTag("level");
            SKPerformanceTrackerService.End("extra", "extra_failed");
        }

        public void TrackLevelStageStarted(string tag = "", int number = 0, string extra1 = "", string extra2 = "", string context = null)
        {
            SKBridgeManager.Instance.TrackLevelStageStarted(number: number, extra1: extra1, extra2: extra2, tag: tag, context: context);
            PerfomanceManager.StartTag("level", "stage_" + number);
            SKPerformanceTrackerService.Start("stage", "stage_started");
        }

        public void TrackLevelStageCompleted(string tag = "", int number = 0, long score = 0, string extra1 = "", string extra2 = "",
            string context = null)
        {
            SKBridgeManager.Instance.TrackLevelStageCompleted(score: score, number: number, extra1: extra1, extra2: extra2, tag: tag,
                context: context);
            PerfomanceManager.EndTag("level");
            SKPerformanceTrackerService.End("stage", "stage_completed");
        }

        public void TrackLevelStageFailed(string tag = "", int number = 0, long score = 0, string extra1 = "", string extra2 = "",
            string context = null)
        {
            SKBridgeManager.Instance.TrackLevelStageFailed(score: score, number: number, extra1: extra1, extra2: extra2, tag: tag, context: context);
            PerfomanceManager.EndTag("level");
            SKPerformanceTrackerService.End("stage", "stage_failed");
        }

        public void TrackChunkStarted(string name, int sequenceNumber, Dictionary<string, object> customData = null, string tag = "", 
            string context = null)
        {
            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConvert.SerializeObject(customData, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            }

            SKBridgeManager.Instance.TrackChunkStarted(name: name, number: sequenceNumber, paramsJson: extra2, tag: tag, context: context);
            PerfomanceManager.StartTag("chunk", name + "|" + sequenceNumber);
            SKPerformanceTrackerService.Start("chunk", "chunk_started");
        }

        public void TrackChunkCompleted(Dictionary<string, object> customData = null, string tag = "", string context = null)
        {
            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConvert.SerializeObject(customData, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            }

            SKBridgeManager.Instance.TrackChunkCompleted(paramsJson: extra2, tag: tag, context: context);
            PerfomanceManager.EndTag("chunk");
            SKPerformanceTrackerService.End("chunk", "chunk_completed");
        }

        public void TrackChunkFailed(Dictionary<string, object> customData = null, string tag = "", string context = null)
        {
            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConvert.SerializeObject(customData, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            }

            SKBridgeManager.Instance.TrackChunkFailed(paramsJson: extra2, tag: tag, context: context);
            PerfomanceManager.EndTag("chunk");
            SKPerformanceTrackerService.End("chunk", "chunk_failed");
        }
        
        public void TrackTutorialCompleted()
        {
            SKBridgeManager.Instance.TrackTutorialCompleted();
        }
        
        public void TrackTutorialCompleted(string context)
        {
            SKBridgeManager.Instance.TrackTutorialCompleted(context: context);
        }

        public void TrackTutorialStep(string tutorialName, string stepName, string context = null)
        {
            SKBridgeManager.Instance.TrackTutorialStep(name: tutorialName, step: stepName, context: context);
        }

        public void TrackEvent(string eventName = "", string tag = "", long param1 = 0, long param2 = 0, long param3 = 0, string extra1 = "", string extra2 = "", string context = null)
        {
            SKBridgeManager.Instance.TrackEvent(name: eventName, param1: param1, param2: param2, param3: param3, param4: 0,
                extra1: extra1, extra2: extra2, tag: tag, context: context);
        }

        public void TrackItem(string item, string context = null)
        {
            SKBridgeManager.Instance.TrackItem(name: item, ownedItems: 0, sourceType: 0, paramsJson: "", context: context);
        }

        public void TrackItem(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData, string context = null)
        {
            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConvert.SerializeObject(customData, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            }

            SKBridgeManager.Instance.TrackItem(name: item, ownedItems: ownedItems, sourceType: (int)sourceId, paramsJson: extra2, context: context);
        }

        public void TrackItem(string item, int ownedItems, string customData, SourceType sourceId, string context = null)
        {
            SKBridgeManager.Instance.TrackItem(name: item, ownedItems: ownedItems, sourceType: (int)sourceId, paramsJson: customData,
                context: context);
        }

        public void TrackItemLoss(string item, int ownedItems, SourceType sourceId,
            Dictionary<string, object> customData, string context = null)
        {
            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConvert.SerializeObject(customData, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            }

            SKBridgeManager.Instance.TrackItemLoss(name: item, ownedItems: ownedItems, sourceType: (int)sourceId, paramsJson: extra2,
                context: context);
        }

        public void TrackItemLoss(string item, int ownedItems, string customData, SourceType sourceId, string context = null)
        {
            SKBridgeManager.Instance.TrackItemLoss(name: item, ownedItems: ownedItems, sourceType: (int)sourceId, paramsJson: customData,
                context: context);
        }

        public void TrackScreen(string screen, string context = null)
        {
            SKBridgeManager.Instance.TrackScreen(screen: screen, context: context);
            PerfomanceManager.UpdateScreen(screenName: screen);
        }

        public void TrackClick(string screen, string element, string context = null)
        {
            SKBridgeManager.Instance.TrackClick(screen: screen, element: element, context: context);
        }

        public void TrackSoftIncome(long amount, long total, string place = "", string extra = "", string context = null)
        {
            SKBridgeManager.Instance.TrackSoftIncome(amount: amount, total: total, place: place, extra: extra, context: context);
        }

        public void TrackSoftOutcome(long amount, long total, string place = "", string extra = "", string context = null)
        {
            SKBridgeManager.Instance.TrackSoftOutcome(amount: amount, total: total, place: place, extra: extra, context: context);
        }

        public void TrackHardIncome(long amount, long total, string place = "", string extra = "", string context = null)
        {
            SKBridgeManager.Instance.TrackHardIncome(amount: amount, total: total, place: place, extra: extra, context: context);
        }

        public void TrackHardOutcome(long amount, long total, string place = "", string extra = "", string context = null)
        {
            SKBridgeManager.Instance.TrackHardOutcome(amount: amount, total: total, place: place, extra: extra, context: context);
        }

        public void TrackApplicationLoaded()
        {
            SKBridgeManager.Instance.TrackApplicationLoaded();
        }

        public void TrackEventWithoutInit(string name, string extra1)
        {
            SKBridgeManager.Instance.TrackEvent(name: name, extra1: extra1);
        }

        #endregion
        
        public int GetRequestConfigTimestamp()
        {
            return RemoteConfig.runtime.timestamp;
        }

        public string OpenSupportPage(string extra)
        {
            return SKBridgeManager.Instance.OpenSupportPage(extra: extra);
        }

        public string GetPrivacyPolicyLink()
        {
            return SKBridgeManager.Instance.GetPrivacyPolicyLink();
        }
        
        public bool ShowRateAppPopup()
        {
            return SKBridgeManager.Instance.ShowRateAppPopup();
        }

        /// <summary>
        /// Notify SayKit that the pop-up was shown.
        /// </summary>
        /// <param name="rate">Value of user rating from custom pop up.</param>
        /// <returns>True, if the popup has already been shown.</returns>
        public bool ShowCustomRateAppPopup(int rate)
        {
            return SKBridgeManager.Instance.ShowCustomRateAppPopup(rate: rate);
        }

        public bool IsRateAppPopupShown()
        {
            return SKBridgeManager.Instance.GetRateAppShown();
        }

        public void RevokeGdprConsent()
        {
            SKBridgeManager.Instance.RevokeGdprConsent();
        }

        public bool IsGdprApplicable()
        {
            return SKBridgeManager.Instance.IsGdprApplicable();
        }
        
        public bool GetGdprStatus()
        {
            return SKBridgeManager.Instance.GetGdprStatus();
        }

        public void RequestNotificationToken()
        {
#if SAYKIT_NOTIFICATIONS && !SAYKIT_CLOUD_BUILD
            SKBridgeManager.Instance.RequestNotificationToken();
#endif
        }

        public string GetNotificationToken()
        {
#if SAYKIT_NOTIFICATIONS && !SAYKIT_CLOUD_BUILD
            return SKBridgeManager.Instance.GetNotificationToken();
#else
            return "";
#endif
        }

        public string GetLocalizedString(string key, string val1 = null, string val2 = null, string val3 = null,
            string val4 = null, string val5 = null)
        {
            var tuple = SKLocalizationService.Instance.GetLocalizedMessage(key, val1, val2, val3, val4, val5);
            return tuple.Item1;
        }

        public (string, bool) GetLocalizedTuple(string key, string val1 = null, string val2 = null, string val3 = null,
            string val4 = null, string val5 = null)
        {
            return SKLocalizationService.Instance.GetLocalizedMessage(key, val1, val2, val3, val4, val5);
        }

        public bool HasLocalizedMessage(string key)
        {
            return SKLocalizationService.Instance.HasLocalizedMessage(key);
        }

        public string GetFullCurrentLanguage()
        {
            return SKBridgeManager.Instance.GetFullCurrentLanguage();
        }

        public (string, CultureInfo) GetFullCurrentLanguageWithCultureInfo()
        {
            
            return SKBridgeManager.Instance.GetFullCurrentLanguageWithCultureInfo();
        }
        
        public SayKitLanguage GetCurrentSayKitLanguage()
        {
            return SKBridgeManager.Instance.GetCurrentLanguage();
        }

        public void StartFPSTag(string tagName)
        {
            PerfomanceManager.StartTag(tagName: tagName, extra: "manual");
        }

        public void EndFPSTag(string tagName)
        {
            PerfomanceManager.EndTag(tagName: tagName);
        }

        public void StartTimeFPSTag(string tagName, int seconds)
        {
            PerfomanceManager.StartTimeTag(tagName: tagName, seconds: seconds, extra: "manual");
        }

        public void ChangeLanguage(string language, Action<bool> OnLanguageChanged = null)
        {
            SKLocalizationService.Instance.ChangeLanguage(language: language, OnLanguageChanged: OnLanguageChanged);
        }

        public void ChangeLanguage(SayKitLanguage language, Action<bool> OnLanguageChanged = null)
        {
            SKLocalizationService.Instance.ChangeLanguage(language: SayKitLanguageConverter.ConvertFromSayKitLanguage(language), 
                OnLanguageChanged: OnLanguageChanged);
        }
        
        public void ChangeLanguage(CultureInfo cultureInfo, Action<bool> OnLanguageChanged = null)
        {
            SKLocalizationService.Instance.ChangeLanguage(language: SayKitLanguageConverter.ConvertFromIETF(cultureInfo.Name), 
                OnLanguageChanged: OnLanguageChanged);
        }
        
        public bool OverrideSystemLanguage(string language)
        {
           return SKBridgeManager.Instance.OverrideSystemLanguage(language: SayKitLanguageConverter.ConvertFromIETF(language));
        }
        
        public bool OverrideSystemLanguage(SayKitLanguage language)
        {
           return SKBridgeManager.Instance.OverrideSystemLanguage(language: SayKitLanguageConverter.ConvertFromSayKitLanguage(language));
        }
        
        public bool OverrideSystemLanguage(CultureInfo cultureInfo)
        {
           return SKBridgeManager.Instance.OverrideSystemLanguage(language: SayKitLanguageConverter.ConvertFromIETF(cultureInfo.Name));
        }
        
        /// <summary>
        /// DEBUG only method!
        /// </summary>
        public void OverrideTrackLevel(int level)
        {
            SKBridgeManager.Instance.OverrideTrackLevel(level: level);
        }

        public void RequestConfigMigration(string sourceVersion)
        {
            SKBridgeManager.Instance.RequestConfigMigration(sourceVersion: sourceVersion);
        }

        public bool RequestRemoteConfigUpdate()
        {
            return SKBridgeManager.Instance.RequestRemoteConfigUpdate();
        }

        public void OpenGooglePlaySubscriptionCenter(string productId)
        {
            SKBridgeManager.Instance.OpenGooglePlaySubscriptionCenter(productId: productId);
        }

        public int GetTotalMemory()
        {
            return SKBridgeManager.Instance.GetTotalMemory();
        }

        public int GetFreeMemory()
        {
            return SKBridgeManager.Instance.GetFreeMemory();
        }

        public void SetFPSGameContext(string data)
        {
            PerfomanceManager.GameContext = data;
        }

        public void DisableSayKitLogs()
        {
            SayKitDebug.disableSayKitLogs = true;
            SKBridgeManager.Instance.DisableLogs();
        }

        public void SetUnityExceptionContext(string data)
        {
            DebugService.UnityExceptionContext = data;
        }

        public void TrackInAppOffer(string place, string extra)
        {
            SKBridgeManager.Instance.TrackInAppOffer(place: place, extra: extra);
        }

        public void ShowMaxMediationDebug()
        {
            SKBridgeManager.Instance.ShowMaxMediationDebug();
        }

        public bool IsDebugUser()
        {
            return RemoteConfig.runtime.debug == 1;
        }

        public void SetPlayerId(string playerId)
        {
            SKBridgeManager.Instance.SetPlayerId(playerId: playerId);
        }

        public int GetPlayingTime()
        {
            return SKBridgeManager.Instance.GetPlayingTime();
        }

        public void RequestLiveServer(string requestName, string requestData, Action<string> onRequestResult)
        {
            LiveRequestManager.Instance.RequestLiveServer(requestName: requestName, requestData: requestData,
                onRequestResult: onRequestResult);
        }

        public void RequestPayerPrediction(Action<LiveRequestPayerPrediction> onRequestResult)
        {
            LiveRequestManager.Instance.RequestPayerPrediction(onRequestResult: onRequestResult);
        }

        public void SetExperimentDeviceId(string deviceId)
        {
            SKBridgeManager.Instance.SetExperimentDeviceId(deviceId: deviceId);
        }

        public string GetSessionId()
        {
            return SKBridgeManager.Instance.GetSessionId();
        }

        public AttributionResponseData GetAttributionData()
        {
            return SKBridgeManager.Instance.GetAttributionData();
        }

        public void SetGameContext(string gameContext)
        {
            SKBridgeManager.Instance.SetGameContext(gameContext: gameContext);
        }

        public void SetFirebaseUserId(string firebaseUserId)
        {
            SKBridgeManager.Instance.SetFirebaseUserId(firebaseUserId: firebaseUserId);
        }
        
        public void OpenCustomUrl(string url)
        {
            SKBridgeManager.Instance.OpenCustomUrl(url: url);
        }
        
        public bool IsBannerEnabled()
        {
#if SAYKIT_BANNER_DISABLED
            return false;
#endif
            if (IsPremium)
            {
                return false;
            }
            
            if (RemoteConfig.ads_settings.banner_disabled == 1)
            {
                return false;
            }

            return true;
        }

        #region Billing

        public void GetAvailableProducts(SKProductInfo[] productInfos, Action<SKProduct[], SKBillingError> onAvailableProductsFetched)
        {
            SKUtils.CheckInit(() =>
            {
                if (productInfos?.Length == 0)
                {
                    onAvailableProductsFetched?.Invoke(Array.Empty<SKProduct>(), new SKBillingError(
                        "Invalid argument. Product ids array is empty."));
                    return;
                }

                SKBridgeManager.Instance.GetAvailableProducts(productInfos: productInfos, onAvailableProductsFetched: onAvailableProductsFetched);
            });
        }

        public void GetNonConfirmedProducts(Action<SKPurchasedProduct[], SKBillingError> onNonConfirmedProductsFetched)
        {
            SKUtils.CheckInit(() => { SKBridgeManager.Instance.GetNonConfirmedProducts(onNonConfirmedProductsFetched: onNonConfirmedProductsFetched); });
        }

        public void GetPurchasedProducts(Action<string[], SKInAppSubscription[], SKBillingError> onPurchasedProductsFetched)
        {
            SKUtils.CheckInit(() => { SKBridgeManager.Instance.GetPurchasedProducts(onPurchasedProductsFetched: onPurchasedProductsFetched); });
        }

        public void PurchaseProduct(SKProductInfo productInfo, string offer, string placement, string extra,
            Action<bool, SKPurchasedProduct, SKBillingError> onPurchaseProductCompleted, SKPurchaseOptions options = null)
        {
            SKUtils.CheckInit(() =>
            {
                if (productInfo == null)
                {
                    onPurchaseProductCompleted?.Invoke(false, null, new SKBillingError("Invalid argument. ProductInfo is null or empty."));
                    return;
                }

                SKBridgeManager.Instance.PurchaseProduct(productInfo: productInfo, offer: offer, placement: placement,
                    onPurchaseProductCompleted: onPurchaseProductCompleted, options: options, extra: extra );
            });
        }

        public void ConfirmPurchase(SKPurchasedProduct purchasedProduct, Action<bool, SKBillingError> onConfirmProductCompleted)
        {
            SKUtils.CheckInit(() =>
            {
                if (purchasedProduct?.Type == null && !string.IsNullOrEmpty(purchasedProduct?.Id)
                                                   && !string.IsNullOrEmpty(purchasedProduct?.TransactionId))
                {
                    onConfirmProductCompleted?.Invoke(false, new SKBillingError("Invalid argument. PurchasedProduct or Type is null or empty."));
                    return;
                }

                SKBridgeManager.Instance.ConfirmPurchase(purchasedProduct: purchasedProduct, onConfirmProductCompleted: onConfirmProductCompleted);
            });
        }

        public void RestorePurchases(Action<string[], SKInAppSubscription[], SKBillingError> onRestorePurchasesCompleted)
        {
            SKUtils.CheckInit(() => { SKBridgeManager.Instance.RestorePurchases(onRestorePurchasesCompleted: onRestorePurchasesCompleted); });
        }
        
        public void ShowWebShop(string externalId, string context)
        {
            SKUtils.CheckInit(() => { SKBridgeManager.Instance.ShowWebShop(externalId: externalId, context: context); });
        }
        
        public void SetWebShopParams(string externalId, string context)
        {
            SKBridgeManager.Instance.SetWebShopParams(externalId: externalId, context: context);
        }

        #endregion

        public void GetServerTimestamp(Action<bool, long> onTimestampReceived)
        {
            if (SKUtils.currentTimestampMs - _lastGetServerTimestampMs < 5000)
            {
                Debug.LogError($"{Tag} Method GetServerTimestamp is called more than once within 5000 ms.");
                onTimestampReceived?.Invoke(false, 0);
                return;
            }

            _lastGetServerTimestampMs = SKUtils.currentTimestampMs;
            
            SKUtils.CheckInit(() =>
            {
                UIManager.Instance.StartCoroutine(SKUtils.FetchServerTimestamp(onTimestampReceived));
            });
        }
        
       #region Local notification
        
        public void ScheduleLocalNotification(SKLocalNotification localNotification)
        {
            SKBridgeManager.Instance.ScheduleLocalNotification(localNotification: localNotification);
        }

        public void RemoveAllDeliveredLocalNotifications()
        {
            SKBridgeManager.Instance.RemoveAllDeliveredLocalNotifications();
        }

        public void RemoveDeliveredLocalNotification(string id)
        {
            SKBridgeManager.Instance.RemoveDeliveredLocalNotification(id: id);
        }

        public void RemoveAllScheduledLocalNotifications()
        {
            SKBridgeManager.Instance.RemoveAllScheduledLocalNotifications();
        }

        public void RemoveScheduledLocalNotification(string id)
        {
            SKBridgeManager.Instance.RemoveScheduledLocalNotification(id: id);
        }
        
        #endregion        
    }
}