using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class RuntimeInfo
    {
        [JsonConstructor]
        public RuntimeInfo() { }
        
        [JsonProperty("version")] 
        public string version { get; set; } = string.Empty;
        
        [JsonProperty("idfa")] 
        public string idfa { get; set; } = string.Empty;
        
        [JsonProperty("idfv")] 
        public string idfv { get; set; } = string.Empty;
        
        [JsonProperty("deviceOs")] 
        public string deviceOs { get; set; } = string.Empty;
        
        [JsonProperty("deviceModel")] 
        public string deviceModel { get; set; } = string.Empty;
        
        [JsonProperty("language")] 
        public string language { get; set; } = string.Empty;
    };

}