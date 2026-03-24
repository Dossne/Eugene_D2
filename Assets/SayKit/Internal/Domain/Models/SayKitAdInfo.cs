using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitAdInfo
    {
        [JsonConstructor]
        public SayKitAdInfo(string adTime, string adType, string adNetwork, string creativeId)
        {
            AdTime = adTime;
            AdType = adType;
            AdNetwork = adNetwork;
            CreativeId = creativeId;
        }
        
        [JsonProperty("adTime")] 
        public string AdTime { get; set; }
        
        [JsonProperty("adType")]
        public string AdType { get; set; }
        
        [JsonProperty("adNetwork")]
        public string AdNetwork { get; set; }
        
        [JsonProperty("creativeId")]
        public string CreativeId { get; set; }
    }
}