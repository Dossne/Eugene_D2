#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable RedundantUsingDirective
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming

#endregion

#if UNITY_IOS
using System.Runtime.InteropServices;
using AOT;
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;

namespace SayKitInternal
{
    public interface ISayKitBridgeCallbacks
    {
        void OnAdShown(string type, string time, string network, string creativeId);
        void OnAdRevenuePaid(string type, string adRevenueInfo);
        void OnAdDisplayed(string type, string adInfo);
        void OnInAppProductChecked(string json);
        void OnInitStateChanged(int number, float progress);
        void OnNotificationTokenReceived(string token);
        void OnRemoteConfigUpdated(string path);
        void OnInterstitialClosed();
        void OnRewardedClosed(bool rewarded);
        void OnSayCatalogueShown(string placement);
        void OnNonConfirmedPurchaseReceived();
        void OnLocalizationsUpdated(string data);
        void OnDeepLinkReceived(string data);
        void OnSupportRequestSubmitted(bool result);
        void OnOfferwallRewardReceived(string data);
    }
    
    public class SayKitBridgeCallbacks : ISayKitBridgeCallbacks
    {
        #region Const

        private const string TAG = "[SayKitBridgeCallbacks]";

        #endregion

        public static SayKitBridgeCallbacks Instance { get; } = new SayKitBridgeCallbacks();

        public void Initialize()
        {
            SayKitDebug.Log($"{TAG} Initialize");

            SetSayKitBridgeCallbacks(this);
        }


#if UNITY_EDITOR
        private static void SetSayKitBridgeCallbacks(ISayKitBridgeCallbacks listener)
        {
        }

#elif UNITY_IOS
        private static void SetSayKitBridgeCallbacks(ISayKitBridgeCallbacks listener)
        {
            SKBridgeManager.Instance.SetSayKitBridgeCallbacks(listener);
        }

#elif UNITY_ANDROID
        private void SetSayKitBridgeCallbacks(ISayKitBridgeCallbacks listener)
        {
            SKBridgeManager.Instance.SetSayKitBridgeCallbacks(listener);
        }

#endif

        #region SayKitBridgeCallbacks

        public void OnAdShown(string type, string time, string network, string creativeId)
        {
            SayKitDebug.Log($"{TAG} OnAdShown({type}, {time}, {network}, {creativeId})");

            if (SKManager.Instance.RemoteConfig.runtime.debug == 1)
            {
                SKThreadService.Instance.RunOnMainThread(() =>
                {
                    DebugService.Instance.AddAdInfo(
                        time,
                        type,
                        !string.IsNullOrEmpty(network) ? network : "unknown",
                        !string.IsNullOrEmpty(creativeId) ? creativeId : "unknown"
                    );
                });
            }
        }

        public void OnAdRevenuePaid(string type, string adRevenueInfo)
        {
            SayKitDebug.Log($"{TAG} OnAdRevenuePaid type: {type}, adRevenueInfo: {adRevenueInfo}");

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(adRevenueInfo))
                {
                    try
                    {
                        var advertisingRevenueInfo = JsonConvert.DeserializeObject<AdRevenueInfo>(adRevenueInfo);
                        if (advertisingRevenueInfo != null)
                        {
                            SKManager.Instance.Config.adRevenuePaid?.Invoke(type, advertisingRevenueInfo);
                        }
                    }
                    catch (Exception e)
                    {
                        SKUtils.HandleError($"{TAG} OnAdRevenuePaid. " +
                                            $"Failed to parse AdRevenueInfo {adRevenueInfo} with exception: {e}");
                    }
                }
            });
        }

        public void OnAdDisplayed(string type, string adInfo)
        {
            SayKitDebug.Log($"{TAG} OnAdDisplayed type: {type}, adInfo: {adInfo}");

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(adInfo))
                {
                    try
                    {
                        var advertisingInfo = JsonConvert.DeserializeObject<AdInfo>(adInfo);
                        if (advertisingInfo != null)
                        {
                            SKManager.Instance.Config.adDisplayed?.Invoke(type, advertisingInfo);
                        }
                    }
                    catch (Exception e)
                    {
                        SKUtils.HandleError($"{TAG} OnAdDisplayed. " +
                                            $"Failed to parse AdInfo {adInfo} with exception: {e}");
                    }
                }
            });
        }

        public void OnInAppProductChecked(string json)
        {
            SayKitDebug.Log($"{TAG} OnInAppProductChecked({json})");

#if SAYKIT_PURCHASING
            SKThreadService.Instance.RunOnMainThread(() => { InAppManager.Instance.OnInAppProductChecked(json); });
#endif
        }

        public void OnInitStateChanged(int number, float progress)
        {
            SayKitDebug.Log($"{TAG} OnInitStateChanged({number}, {progress})");

            SKThreadService.Instance.RunOnMainThread(() => { SKManager.Instance.UpdateInitState(number, progress); });
        }

        public void OnNotificationTokenReceived(string token)
        {
            SayKitDebug.Log($"{TAG} OnNotificationTokenReceived({token})");

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    SKManager.Instance.Config.notificationTokenReceived?.Invoke(token);
                }
            });
        }

        public void OnRemoteConfigUpdated(string path)
        {
            SayKitDebug.Log($"{TAG} OnRemoteConfigUpdated({path})");

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (File.Exists(path))
                    {
                        RemoteConfigManager.Instance.ConfigInitialized(File.ReadAllText(path));
                    }
                    else
                    {
                        SKUtils.HandleError($"[OnRemoteConfigUpdated] file not found {path}");
                    }
                }
            });
        }

        public void OnInterstitialClosed()
        {
            SayKitDebug.Log($"{TAG} OnInterstitialClosed()");

            SKThreadService.Instance.RunOnMainThread(() => { AdsManager.Instance.OnInterstitialClosed(); });
        }

        public void OnRewardedClosed(bool rewarded)
        {
            SayKitDebug.Log($"{TAG} OnRewardedClosed({rewarded})");

            SKThreadService.Instance.RunOnMainThread(() => { AdsManager.Instance.OnRewardedClosed(rewarded); });
        }

        public void OnSayCatalogueShown(string placement)
        {
            SayKitDebug.Log($"{TAG} onSayCatalogueShown({placement})");

            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SayCatalogueRewardPrefab.CatalogueRewardUser?.Invoke(placement);
                SayCatalogueRewardPrefab.CatalogueRewardUser = null;
            });
        }

        public void OnLocalizationsUpdated(string data)
        {
            SayKitDebug.Log($"{TAG} OnLocalizationsUpdated({data})");

            if (!string.IsNullOrEmpty(data))
            {
                SKThreadService.Instance.RunOnMainThread(() => { SKLocalizationService.Instance.SetupRemoteMessages(data); });
            }
        }

        public void OnDeepLinkReceived(string data)
        {
            SayKitDebug.Log($"{TAG} OnDeepLinkReceived({data})");

            if (!string.IsNullOrEmpty(data))
            {
                SKThreadService.Instance.RunOnMainThread(() =>
                {
                    SKManager.Instance.Config.onDeepLinkReceived?.Invoke(data);
                });
            }
        }

        public void OnSupportRequestSubmitted(bool result)
        {
            SayKitDebug.Log($"{TAG} OnSupportRequestSubmitted({result})");
            
            SKThreadService.Instance.RunOnMainThread(() =>
            {
                SKManager.Instance.Config.onSupportRequestSubmitted?.Invoke(result);
            });
        }

        public void OnNonConfirmedPurchaseReceived()
        {
            SayKitDebug.Log("[SayKitBridgeCallbacks] OnNonConfirmedPurchaseReceived()");

            SKThreadService.Instance.RunOnMainThread(() => { SKManager.Instance.Config.nonConfirmedPurchaseReceived?.Invoke(); });
        }

        public void OnOfferwallRewardReceived(string data) { }

        #endregion
    }
}