using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitVersionDTO
    {
        [JsonConstructor]
        public SayKitVersionDTO() { }
        
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonProperty("comment")]
        public string Comment { get; set; } = string.Empty;
        
        [JsonProperty("enabled")]
        public int Enabled { get; set; }
        
        [JsonProperty("binded")]
        public string Binded  { get; set; } = string.Empty;
    }
}