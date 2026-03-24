using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class CrossPromoResponse
    {
        [JsonConstructor]
        public CrossPromoResponse() { }
        
        [JsonProperty("lines")]
        public CrossPromoResponseLine[] Lines { get; set; }
    }
}