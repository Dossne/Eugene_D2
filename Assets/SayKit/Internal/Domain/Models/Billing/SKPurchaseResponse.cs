using Newtonsoft.Json;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKPurchaseResponse
    {
        [JsonConstructor]
        public SKPurchaseResponse() { }
        
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("details")] public SKPurchasedProduct PurchasedProduct { get; set; }
        [JsonProperty("error")] public SKBillingError Error { get; set; }
    }
}