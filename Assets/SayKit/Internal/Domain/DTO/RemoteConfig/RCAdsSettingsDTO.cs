using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class RCAdsSettingsDTO
    {
        [JsonConstructor]
        public RCAdsSettingsDTO() { }

        [JsonProperty("maxsdk_enabled")]
        public int maxsdk_enabled { get; set; }
        
        [JsonProperty("maxsdk_key")]
        public string maxsdk_key { get; set; } = string.Empty;
        
        [JsonProperty("banner_disabled")]
        public int banner_disabled { get; set; }
        
        [JsonProperty("banner_bg_padding")]
        public int banner_bg_padding { get; set; }
        
        [JsonProperty("maxsdk_interstitial_id")]
        public string maxsdk_interstitial_id { get; set; } = string.Empty;
        
        [JsonProperty("maxsdk_rewarded_id")]
        public string maxsdk_rewarded_id { get; set; } = string.Empty;
        
        [JsonProperty("sk_interstitial_popup_enabled")]
        public int sk_interstitial_popup_enabled { get; set; } = 1;
        
        [JsonProperty("sk_interstitial_popup_delay")]
        public int sk_interstitial_popup_delay { get; set; } = 5;
        
        [JsonProperty("inplay_id")]
        public string inplay_id { get; set; } = string.Empty;
       
        [JsonProperty("inplay_ads_count")]
        public int inplay_ads_count { get; set; } = 5;
        
        [JsonProperty("inplay_refresh_timeout")]
        public int inplay_refresh_timeout { get; set; } = 15;
    }
}