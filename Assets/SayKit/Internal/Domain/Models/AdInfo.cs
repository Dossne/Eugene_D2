using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class AdInfo
    {
        [JsonConstructor]
        public AdInfo() { }
        
        [JsonProperty("adunit")] 
        public string MaxAdUnit;
        
        [JsonProperty("network_name")] 
        public string NetworkName;
        
        [JsonProperty("creative_id")] 
        public string MaxCreativeId;
        
        [JsonProperty("cpm")] 
        [JsonConverter(typeof(SayKitDoubleConverter))]
        public double Cpm;
    }
}