#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

using Newtonsoft.Json;

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitConfigDTO
    {
        [JsonConstructor]
        public SayKitConfigDTO() { }
        
        [JsonProperty("appKey")]
        public string AppKey { get; set; } = string.Empty;
        
        [JsonProperty("bannerAdUnitId")]
        public string BannerAdUnitId { get; set; } = string.Empty;
        
        [JsonProperty("interstitialAdUnitId")]
        public string InterstitialAdUnitId { get; set; } = string.Empty;
        
        [JsonProperty("rewardedAdUnitId")]
        public string RewardedAdUnitId { get; set; } = string.Empty;

        [JsonProperty("attributionConfigUpdate")]
        public bool AttributionConfigUpdate { get; set; }
      
        /// <summary>
        /// Uses only in the Editor.
        /// </summary>
        [JsonProperty("overrideSystemLanguage")]
        public string OverrideSystemLanguage { get; set; } = "en";
        
        [JsonProperty("overrideAnalyticSegment")]
        public int OverrideAnalyticSegment { get; set; } = -1;

        /// <summary>
        /// Disable automatic banner initialization.
        /// If you enable it, you must manage the banner using showBanner/hideBanner methods.
        /// </summary>
        [JsonProperty("disableAutoBanner")]
        public bool DisableAutoBanner { get; set; }

        /// <summary>
        /// Skip banner timeouts on start.
        /// If you enable it, the banner will show on start of application.
        /// </summary>
        [JsonProperty("disableAutoBannerTimeouts")]
        public bool DisableAutoBannerTimeouts { get; set; }

        /// <summary>
        /// Disable interstitial ads.
        /// </summary>
        [JsonProperty("disableInterstitial")]
        public bool DisableInterstitial { get; set; }

        /// <summary>
        /// Call Notification request manually. Make sure you are calling notification request every session.
        /// </summary>
        [JsonProperty("customNotificationRequest")]
        public bool CustomNotificationRequest { get; set; }
        
#if SAYKIT_SMART_INTER
        /// <summary>
        /// [Android] Enable smart interstitial flow.
        /// </summary>
        [JsonProperty("enableSmartInterstitials")]
        public bool EnableSmartInterstitials { get; set; } = true;
#endif

        /// <summary>
        /// Disable automatic banner initialization by SAYKIT_BANNER_DISABLED define
        /// </summary>
        [JsonProperty("disableBanner")]
        public bool DisableBanner { get; set; }

        /// <summary>
        /// Enable Facebook auto-logging.
        /// </summary>
        [JsonProperty("facebookAutoLoggingEnabled")]
        public bool FacebookAutoLoggingEnabled { get; set; } = true;

        /// <summary>
        /// Disable native billing.
        /// </summary>
        [JsonProperty("disableBilling")]
        public bool DisableBilling { get; set; } = true;

        /// <summary>
        /// Disable purchase validation.
        /// </summary>
        [JsonProperty("disablePurchaseValidation")]
        public bool DisablePurchaseValidation { get; set; }
    }
}