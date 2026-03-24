using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class AdRevenueInfo
    {
        [JsonConstructor]
        public AdRevenueInfo() { }
        
        [JsonProperty("ID")]
        public string ID { get; set; }
        
        [JsonProperty("SubID")]
        public string SubID { get; set; }
        
        [JsonProperty("Amount")]
        [JsonConverter(typeof(SayKitDoubleConverter))]
        public double Amount { get; set; }
    }
}