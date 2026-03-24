#if UNITY_EDITOR

using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class SayKitBridgeEditor
    {
        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsInitialized()
        {
            SayKitDebug.Log("SayKitBridgeEditor [IsInitialized]");
            return false;
        }

        public float GetInitStateProgress()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetInitStateProgress]");
            return 0f;
        }

        public int GetInitStateNumber()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetInitStateNumber]");
            return 0;
        }

        public void InitIfNeeded(string configJson, string environmentParams)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [InitIfNeeded] configJson: {configJson}, environmentParams: {environmentParams}");
            UIManager.Instance.StartCoroutine(InitCoroutine());
        }
        
        private IEnumerator InitCoroutine()
        {
            RemoteConfigManager.Instance.InitializeLocalConfig(SKManager.Instance.Config.appKey, Application.version);
            yield return UIManager.Instance.StartCoroutine(RemoteConfigManager.Instance.RequestRemoteConfigUpdate());
            yield return new WaitUntil(() => RemoteConfigManager.Instance.Initialized);

            SKManager.Instance.UpdateInitState(7, 1f);
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public float GetBackgroundBannerSize()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetBackgroundBannerSize]");
            return 50f;
        }

        public void HideBanner()
        {
            SayKitDebug.Log("SayKitBridgeEditor [HideBanner]");
        }

        public void ShowBanner()
        {
            SayKitDebug.Log("SayKitBridgeEditor [ShowBanner]");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsInterstitialAvailable(string place, int countdown)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [IsInterstitialAvailable] place: {place}, countdown: {countdown}");
            return false;
        }

        public bool IsRewardedAvailable(string place)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [IsRewardedAvailable] place: {place}");
            return false;
        }

        public bool IsRewardedPlacementAvailable(string place)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [IsRewardedPlacementAvailable] place: {place}");
            return false;
        }

        public bool ShowInterstitial(string place, int countdown)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ShowInterstitial] place: {place}, countdown: {countdown}");
            return false;
        }

        public bool ShowRewarded(string place)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ShowRewarded] place: {place}");
            return false;
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackInterstitialOffer(string place, string extra)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackInterstitialOffer] place: {place}, extra: {extra}");
        }

        public void TrackRewardedOffer(string place, string extra)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackRewardedOffer] place: {place}, extra: {extra}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetSubscriptionExpirationTimestamp()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetSubscriptionExpirationTimestamp]");
            return 0;
        }

        public bool IsAppStoreAvailable()
        {
            SayKitDebug.Log("SayKitBridgeEditor [IsAppStoreAvailable]");
            return true;
        }

        public string GetPrivacyPolicyLink()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetPrivacyPolicyLink]");
            return string.Empty;
        }

        public bool IsAppInstalled(string packageName)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [IsAppInstalled] packageName: {packageName}");

            // iOS only
            return false;
        }

        public void OpenGooglePlaySubscriptionCenter(string productId)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [OpenGooglePlaySubscriptionCenter] productId: {productId}");

            // Android only
        }

        public void OpenStoreProductView(int storeId, string skadData, string storeUrl)
        {
            SayKitDebug.Log(
                $"SayKitBridgeEditor [OpenStoreProductView] storeId: {storeId}, skadData: {skadData}, storeUrl: {storeUrl}");

            // iOS only
        }

        public string OpenSupportPage(string extra)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [OpenSupportPage] extra: {extra}");
            return string.Empty;
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsFacebookSdkInitialized()
        {
            SayKitDebug.Log("SayKitBridgeEditor [IsFacebookSdkInitialized]");

            // Android only
            return false;
        }

        public void LogFacebookEvent(string name, float valueToSum, string paramsJson)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [LogFacebookEvent] name: {name}, valueToSum: {valueToSum}, paramsJson: {paramsJson}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void LogFirebaseEvent(string name, float valueToSum, string paramsJson)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [LogFirebaseEvent] name: {name}, valueToSum: {valueToSum}, paramsJson: {paramsJson}");
        }

        public void SetFirebaseUserProperty(string key, string value)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [SetFirebaseUserProperty] key: {key}, value: {value}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void LogCrashlytics(string message)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [LogCrashlytics] message: {message}");
        }

        public void LogCrashlyticsException(Exception exception)
        {
            SayKitDebug.Log(
                $"SayKitBridgeEditor [LogCrashlyticsException] Exception message: {exception.Message}, exception stacktrace: {exception.StackTrace}");
        }

        public void SetCrashlyticsParam(string key, string value)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [SetCrashlyticsParam] key: {key}, value: {value}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackApplicationLoaded()
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackApplicationLoaded]");
        }

        public void TrackAvailableMemory()
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackAvailableMemory]");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackEvent(string name, long param1 = 0, long param2 = 0, long param3 = 0, long param4 = 0,
            string extra1 = "", string extra2 = "", string tag = "", string context = null)
        {
            SayKitDebug.Log(
                $"SayKitBridgeEditor [TrackEvent] name: {name}, param1: {param1}, param2: {param2}, param3: {param3}, param4: {param4}, " +
                $"extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelCompleted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelCompleted] " +
                            $"level: {level}, score: {score}, number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackLevelFailed(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelFailed] " +
                            $"level: {level}, score: {score}, number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackLevelStarted(int level, long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelStarted] " +
                            $"level: {level}, score: {score}, number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelExtraCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelExtraCompleted] " +
                            $"score: {score}, number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackLevelExtraFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelExtraFailed] " +
                            $"score: {score}, number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackLevelExtraStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelExtraStarted] " +
                            $"number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackLevelStageCompleted(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelStageCompleted] " +
                            $"score: {score}, number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackLevelStageFailed(long score, int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelStageFailed] " +
                            $"score: {score}, number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackLevelStageStarted(int number, string extra1, string extra2, string tag, string context = null)
        {
            SayKitDebug.Log("SayKitBridgeEditor [TrackLevelStageStarted] " +
                            $"number: {number}, extra1: {extra1}, extra2: {extra2}, tag: {tag}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackChunkCompleted(string paramsJson, string tag, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackChunkCompleted] paramsJson: {paramsJson}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackChunkFailed(string paramsJson, string tag, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackChunkFailed] paramsJson: {paramsJson}, tag: {tag}, context: {context ?? string.Empty}");
        }

        public void TrackChunkStarted(string name, int number, string paramsJson, string tag, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackChunkStarted] name: {name}, number: {number}, paramsJson: {paramsJson}, tag: {tag}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackTutorialCompleted(string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackTutorialCompleted] context: {context ?? string.Empty}");
        }

        public void TrackTutorialStep(string name, string step, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackTutorialStep] name: {name}, step: {step}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackItem(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackItem] ownedItems: {ownedItems}, sourceType: {sourceType}, paramsJson: {paramsJson}, context: {context ?? string.Empty}");
        }

        public void TrackItemLoss(string name, int ownedItems, int sourceType, string paramsJson, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackItemLoss] name: {name}, ownedItems: {ownedItems}, sourceType: {sourceType}, paramsJson: {paramsJson}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackClick(string screen, string element, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackClick] screen: {screen}, element: {element}, context: {context ?? string.Empty}");
        }

        public void TrackScreen(string screen, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackScreen] screen: {screen}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void TrackHardIncome(long amount, long total, string place, string extra, string context = null) 
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackHardIncome] amount: {amount}, total: {total}, place: {place}, extra: {extra}, context: {context ?? string.Empty}");
        }

        public void TrackHardOutcome(long amount, long total, string place, string extra, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackHardOutcome] amount: {amount}, total: {total}, place: {place}, extra: {extra}, context: {context ?? string.Empty}");
        }

        public void TrackSoftIncome(long amount, long total, string place, string extra, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackSoftIncome] amount: {amount}, total: {total}, place: {place}, extra: {extra}, context: {context ?? string.Empty}");
        }

        public void TrackSoftOutcome(long amount, long total, string place, string extra, string context = null)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackSoftOutcome] amount: {amount}, total: {total}, place: {place}, extra: {extra}, context: {context ?? string.Empty}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void CheckInAppProduct(string json)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [CheckInAppProduct] json: {json}");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetRequestConfigTimestamp()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetRequestConfigTimestamp]");
            return 0;
        }

        public void RequestConfigMigration(string sourceVersion)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [RequestConfigMigration] sourceVersion: {sourceVersion}");
        }

        public bool RequestRemoteConfigUpdate()
        {
            SayKitDebug.Log("SayKitBridgeEditor [RequestRemoteConfigUpdate]");
            return false;
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool ShowCustomRateAppPopup(int rate)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ShowCustomRateAppPopup] rate: {rate}");

            if (StorageService.Instance.RateAppPopupWasShowed == false)
            {
                StorageService.Instance.RateAppPopupWasShowed = true;
                StorageService.Instance.Save();

                return true;
            }

            return false;
        }

        public bool ShowRateAppPopup()
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ShowRateAppPopup]");
            return false;
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool IsPremium()
        {
            SayKitDebug.Log("SayKitBridgeEditor [IsPremium]");
            return StorageService.Instance.IsPremium;
        }

        public void DisablePremium()
        {
            SayKitDebug.Log("SayKitBridgeEditor [DisablePremium]");
            StorageService.Instance.IsPremium = false;
            StorageService.Instance.Save();
        }

        public void EnablePremium()
        {
            SayKitDebug.Log("SayKitBridgeEditor [EnablePremium]");
            StorageService.Instance.IsPremium = true;
            StorageService.Instance.Save();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public bool GetGdprStatus()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetGdprStatus]");
            return false;
        }

        public bool IsGdprApplicable()
        {
            SayKitDebug.Log("SayKitBridgeEditor [IsGdprApplicable]");
            return false;
        }

        public void RevokeGdprConsent()
        {
            SayKitDebug.Log("SayKitBridgeEditor [RevokeGdprConsent]");
            SayKitUI.CallShowPopup();
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetNotificationToken()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetNotificationToken]");
            return string.Empty;
        }

        public void RequestNotificationToken()
        {
            SayKitDebug.Log("SayKitBridgeEditor [RequestNotificationToken]");
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////
        
        public SayKitLanguage GetCurrentLanguage()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetCurrentLanguage]");
            
            var overrideSystemLanguage = SKLocalizationService.Instance.GetOverrideLanguage();
            
            if (overrideSystemLanguage != SayKitLanguage.Unknown || string.IsNullOrEmpty(overrideSystemLanguage.ToString()))
            {
                return overrideSystemLanguage;
            }
            
            return SKLocalizationService.currentLanguage;
        }

        public string GetFullCurrentLanguage()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetFullCurrentLanguage]");
            
            var overrideSystemLanguage = SKLocalizationService.Instance.GetOverrideLanguage();
            
            if (overrideSystemLanguage != SayKitLanguage.Unknown || string.IsNullOrEmpty(overrideSystemLanguage.ToString()))
            {
                return SayKitLanguageConverter.ConvertFromSayKitLanguage(overrideSystemLanguage);
            }
            
            return SayKitLanguageConverter.ConvertFromSayKitLanguage(SKLocalizationService.currentLanguage);
        }

        public (string, CultureInfo) GetFullCurrentLanguageWithCultureInfo()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetFullCurrentLanguageWithCultureInfo]");
            
            var overrideSystemLanguage = SKLocalizationService.Instance.GetOverrideLanguage();
            
            if (overrideSystemLanguage != SayKitLanguage.Unknown || string.IsNullOrEmpty(overrideSystemLanguage.ToString()))
            {
                var langCode = SayKitLanguageConverter.ConvertToIETF(SayKitLanguageConverter.ConvertFromSayKitLanguage(overrideSystemLanguage));
                var cultureInfo = SayKitLanguageConverter.GetCultureInfoFromCode(langCode);
                return (langCode, cultureInfo);
            }
            
            var currentLangCode = SayKitLanguageConverter.ConvertToIETF(SayKitLanguageConverter.ConvertFromSayKitLanguage(SKLocalizationService.currentLanguage));
            var currentCultureInfo = SayKitLanguageConverter.GetCultureInfoFromCode(currentLangCode);
            
           return (currentLangCode, currentCultureInfo);
        }

        public bool OverrideSystemLanguage(string language)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [OverrideSystemLanguage] language: {language}");
            
            if (string.IsNullOrEmpty(language))
            {
                SayKitDebug.LogWarning("[SayKitBridgeEditor] empty language code passed to OverrideSystemLanguage method.");
                return false;
            }

            SKLocalizationService.currentLanguage = SayKitLanguageConverter.ConvertToSayKitLanguage(language);
            SKLocalizationService.Instance.SaveOverrideLanguage(language);
            
            return true;
        }
        
        public string PrioritizedLanguages()
        {
            SayKitDebug.Log("SayKitBridgeEditor [PrioritizedLanguages]");
            
            return "[\"en_us\", \"en\"]";
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetFreeMemory()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetFreeMemory]");
            return 0;
        }

        public int GetTotalMemory()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetTotalMemory]");
            return 0;
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetRuntimeInfo()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetRuntimeInfo]");
            return JsonConvert.SerializeObject(new RuntimeInfo());
        }

        public string GetAdvertisingId()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetAdvertisingId]");
            return string.Empty;
        }

        public string GetAppVersion()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetAppVersion]");
            return string.Empty;
        }

        public string GetDeviceId()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetDeviceId]");
            return string.Empty;
        }

        public string GetDeviceModel()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetDeviceModel]");
            return string.Empty;
        }

        public string GetDeviceOs()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetDeviceOs]");
            return string.Empty;
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public int GetAppVersionCode()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetAppVersionCode]");
            return 0;
        }

        public string GetAppVersionFullName()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetAppVersionFullName]");
            return string.Empty;
        }

        public string GetAppVersionOriginalName()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetAppVersionOriginalName]");
            return string.Empty;
        }

        public int GetSdkVersionCode()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetSdkVersionCode]");
            return 0;
        }

        public string GetSdkVersionName()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetSdkVersionName]");
            return string.Empty;
        }

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public string GetThermalState()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetThermalState]");
            return string.Empty;
        }

        public void ChangeLanguage(int language)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ChangeLanguage] language: {language}");
        }

        public void DisableLogs()
        {
            SayKitDebug.Log("SayKitBridgeEditor [DisableLogs]");
        }

        public void OverrideTrackLevel(int level)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [OverrideTrackLevel] level: {level}");
        }

        public void ShowMaxMediationDebug()
        {
            SayKitDebug.Log("SayKitBridgeEditor [ShowMaxMediationDebug]");
        }

        public bool GetRateAppShown()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetRateAppShown]");
            return false;
        }

        public string GetATTStatus()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetATTStatus]");
            return "Denied";
        }

        public void ShowSayCatalogue(string placement, string catalogParams = "")
        {
            SayKitDebug.Log(
                $"SayKitBridgeEditor [ShowSayCatalogue] placement: {placement}, catalogParams: {catalogParams}");
        }

        public void TrackSayCatalogueOffer(string sourceType)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackSayCatalogueOffer] sourceType: {sourceType}");
        }

        public void TrackInAppOffer(string place, string extra)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [TrackInAppOffer] place: {place}, extra: {extra}");
        }

        public void OpenSystemSettings()
        {
            SayKitDebug.Log("SayKitBridgeEditor [OpenSystemSettings]");
        }

        public void SetPlayerId(string playerId)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [SetPlayerId] playerId: {playerId}");
        }

        public int GetPlayingTime()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetPlayingTime]");
            return 0;
        }

        public void RequestLiveServer(string requestName, string requestData, Action<string> onRequestResult)
        {
            SayKitDebug.Log(
                $"SayKitBridgeEditor [RequestLiveServer] requestName: {requestName}, requestData:{requestData}");
            onRequestResult?.Invoke(string.Empty);
        }

        public void SetExperimentDeviceId(string deviceId)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [SetExperimentDeviceId] deviceId: {deviceId}");
        }

        public string GetSessionId()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetSessionId]");
            return string.Empty;
        }

        public string GetSystemProxy()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetSystemProxy]");
            return string.Empty;
        }

        public AttributionResponseData GetAttributionData()
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetAttributionData]");
            return new AttributionResponseData(status: "empty", creative: string.Empty, campaign: string.Empty);
        }
        
        public void SetGameContext(string gameContext)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [SetGameContext] gameContext: {gameContext}");
        }

        public void SetFirebaseUserId(string firebaseUserId)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [SetFirebaseUserId] firebaseUserId: {firebaseUserId}");
        }
        
        public void OpenCustomUrl(string url)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [OpenCustomUrl] url: {url}");
            Application.OpenURL(url);
        }

        #region Billing

        public void GetAvailableProducts(SKProductInfo[] productInfos, Action<SKProduct[], SKBillingError> onAvailableProductsFetched)
        {
            var prodArray = productInfos != null ? JsonConvert.SerializeObject(productInfos) : string.Empty;
            Debug.Log($"SayKitBridgeEditor [GetAvailableProducts] products: {prodArray}");
            onAvailableProductsFetched?.Invoke(Array.Empty<SKProduct>(), new SKBillingError(string.Empty));
        }

        public void GetNonConfirmedProducts(Action<SKPurchasedProduct[], SKBillingError> onNonConfirmedProductsFetched)
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetNonConfirmedProducts]");
            onNonConfirmedProductsFetched?.Invoke(Array.Empty<SKPurchasedProduct>(), new SKBillingError(string.Empty));
        }

        public void GetPurchasedProducts(Action<string[], SKInAppSubscription[], SKBillingError> onPurchasedProductsFetched)
        {
            SayKitDebug.Log("SayKitBridgeEditor [GetPurchasedProducts]");
            onPurchasedProductsFetched?.Invoke(Array.Empty<string>(), Array.Empty<SKInAppSubscription>(), new SKBillingError(string.Empty));
        }

        public void PurchaseProduct(SKProductInfo productInfo, SKPurchaseOptions options, string offer, string placement, string extra, Action<bool, SKPurchasedProduct, SKBillingError> onPurchaseProductCompleted)
        {
            offer ??= string.Empty;
            placement ??= string.Empty;
            extra ??= string.Empty;
            options ??= new SKPurchaseOptions();
            
            SayKitDebug.Log($"SayKitBridgeEditor [PurchaseProduct] productId: {JsonConvert.SerializeObject(productInfo)} offer: {offer}, placement: {placement}, extra: {extra}, options: {JsonConvert.SerializeObject(options)}");
            onPurchaseProductCompleted?.Invoke(true, new SKPurchasedProduct() { Id = productInfo.Id, Type = productInfo.Type}, new SKBillingError(string.Empty));
        }

        public void RestorePurchases(Action<string[], SKInAppSubscription[], SKBillingError> onRestorePurchasesCompleted)
        {
            SayKitDebug.Log("SayKitBridgeEditor [RestorePurchases]");
            onRestorePurchasesCompleted?.Invoke(Array.Empty<string>(), Array.Empty<SKInAppSubscription>(), new SKBillingError(string.Empty));
        }

        public void ConfirmPurchase(SKPurchasedProduct purchasedProduct, Action<bool, SKBillingError> onConfirmProductCompleted)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ConfirmPurchase] product: {JsonConvert.SerializeObject(purchasedProduct)}");
            onConfirmProductCompleted?.Invoke(true, new SKBillingError( string.Empty));
        }        
        
        public void ShowWebShop(string externalId, string context)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ShowWebShop] externalId: {externalId}, context: {context}");
        }
        
        public void SetWebShopParams(string externalId, string context)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [SetWebShopParams] externalId: {externalId}, context: {context}");
        }
        
        #endregion

        #region Local notification

        public void ScheduleLocalNotification(string json)
        {
            SayKitDebug.Log($"SayKitBridgeEditor [ScheduleLocalNotification] {JsonConvert.SerializeObject(json)}");
        }

        public void RemoveAllDeliveredLocalNotifications()
        {
            SayKitDebug.Log("SayKitBridgeEditor [RemoveAllDeliveredLocalNotifications]");
        }
        
        public void RemoveDeliveredLocalNotification(string id)
        {
            SayKitDebug.Log("SayKitBridgeEditor [RemoveDeliveredLocalNotification]");
        }
        
        public void RemoveAllScheduledLocalNotifications()
        {
            SayKitDebug.Log("SayKitBridgeEditor [RemoveAllScheduledLocalNotifications]");
        }
        
        public void RemoveScheduledLocalNotification(string id)
        {
            SayKitDebug.Log("SayKitBridgeEditor [RemoveScheduledLocalNotification]");
        }
        
        #endregion

        #region InPlay

        public bool IsInPlayAvailable()
        {
            SayKitDebug.Log($"SayKitBridgeEditor [IsInPlayAvailable]");
            return true;
        }

        public bool ShowInPlay(int x, int y, int width, int height)
        {
			SayKitDebug.Log("SayKitBridgeEditor [ShowInPlay]");
            return false;
        }

        public void HideInPlay()
        {
             SayKitDebug.Log("SayKitBridgeEditor [HideInPlay]");
        }

        public float GetScreenScale()
        {
            return 1f;
        }

        #endregion

        //////  //////  //////
        //////  //////  //////
        //////  //////  //////

        public void SetCallbacks(ISayKitBridgeCallbacks listener)
        {
            SayKitDebug.Log("SayKitBridgeEditor [SetCallbacks]");
        }
    }
}
#endif