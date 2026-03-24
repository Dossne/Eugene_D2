using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKProductInfo
    {
        [JsonConstructor]
        public SKProductInfo() { }
        
        [JsonProperty("product_type")] public SKProductType Type { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
    }
}