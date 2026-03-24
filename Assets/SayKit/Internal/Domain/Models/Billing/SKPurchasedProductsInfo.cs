using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKPurchasedProductsInfo
    {
        [JsonConstructor]
        public SKPurchasedProductsInfo() { }
        
        [JsonProperty("inApps")] public string[] ProductIds { get; set; }
        [JsonProperty("activeSubscriptions")] public SKInAppSubscription[] ActiveSubscriptions { get; set; }
        [JsonProperty("expiredSubscriptions")] public string[] ExpiredSubscriptions { get; set; }
        [JsonProperty("error")] public SKBillingError Error { get; set; }
    }
}