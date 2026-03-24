using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class RCRuntimeDTO
    {
        [JsonConstructor]
        public RCRuntimeDTO() { }
        
        [JsonProperty("timestamp")]
        public int timestamp { get; set; }
        
        [JsonProperty("country")]
        public string country { get; set; } = string.Empty;

        [JsonProperty("debug")]
        public int debug { get; set; }
        
        [JsonProperty("segment")]
        public int segment { get; set; }
        
        [JsonProperty("hash")]
        public string hash { get; set; } = string.Empty;
        
        [JsonProperty("fps_tag_min_rate")]
        public int fps_tag_min_rate { get; set; } = 20;
        
        [JsonProperty("fps_tag_min_spikes")]
        public int fps_tag_min_spikes { get; set; } = 4;

        [JsonProperty("fps_tag_enabled")] 
        public int fps_tag_enabled { get; set; } = 1;

        [JsonProperty("fps_optimisation_enable")] 
        public int fps_optimisation_enable { get; set; }

        [JsonProperty("disable_rc_v2")] 
        public int disable_rc_v2 { get; set; }
        
        [JsonProperty("disable_new_interstitial_ads")] 
        public int disable_new_interstitial_ads { get; set; }

        [JsonProperty("disable_live_request")] 
        public int disable_live_request { get; set; }
        
        [JsonProperty("sk_performance_service_enabled")] 
        public int sk_performance_service_enabled { get; set; }

        [JsonProperty("sk_age_verification_enabled")] 
        public int sk_age_verification_enabled { get; set; } = 1;
        
        [JsonProperty("disable_local_notifications")] 
        public int disable_local_notifications { get; set; }
    }
}