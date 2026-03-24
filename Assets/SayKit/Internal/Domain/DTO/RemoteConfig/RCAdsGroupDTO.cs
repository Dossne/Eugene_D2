using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class RCAdsGroupDTO
    {
        [JsonConstructor]
        public RCAdsGroupDTO() { }
        
        [JsonProperty("group")] 
        public string group { get; set; } = string.Empty;
        
        [JsonProperty("skip_after_start")]
        public int skip_after_start { get; set; }
        
        [JsonProperty("skip_after_first_app_start")]
        public int skip_after_first_app_start { get; set; }
        
        [JsonProperty("skip_after_interstitial")]
        public int skip_after_interstitial { get; set; }
        
        [JsonProperty("skip_after_rewarded")]
        public int skip_after_rewarded { get; set; }
        
        [JsonProperty("skip_period_duration")]
        public int skip_period_duration { get; set; }
        
        [JsonProperty("skip_period_limit")]
        public int skip_period_limit { get; set; }
        
        [JsonProperty("force_impression_every")]
        public int force_impression_every { get; set; }
        
        public RCAdsGroupDTO Clone()
        {
            return new RCAdsGroupDTO
            {
                group = string.Copy(this.group),
                skip_after_start = this.skip_after_start,
                skip_after_first_app_start = this.skip_after_first_app_start,
                skip_after_interstitial = this.skip_after_interstitial,
                skip_after_rewarded = this.skip_after_rewarded,
                skip_period_duration = this.skip_period_duration,
                skip_period_limit = this.skip_period_limit,
                force_impression_every = this.force_impression_every,
            };
        }
    }
}