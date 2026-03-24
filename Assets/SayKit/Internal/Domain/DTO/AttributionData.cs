using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class AttributionData
    {
        [JsonConstructor]
        public AttributionData() { }
        
        [JsonProperty("Attribution")]
        public string Attribution { get; set; } = string.Empty;
        
        [JsonProperty("AttributionToken")]
        public string AttributionToken { get; set; } = string.Empty;
    }
}