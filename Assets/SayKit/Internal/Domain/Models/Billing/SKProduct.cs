using JetBrains.Annotations;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKProduct
    {
        [JsonConstructor]
        public SKProduct() { }
        
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("type")] public SKProductType Type { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("price")] public float Price { get; set; }
        [JsonProperty("currencyCode")] public string CurrencyCode { get; set; }

        [JsonProperty("isSayPayAvailable")] public bool IsSayPayAvailable { get; set; }
        [JsonProperty("localizedPrice")] public string LocalizedPrice { get; set; }
        [CanBeNull][JsonProperty("subscriptionPeriod")] public string SubscriptionPeriod { get; set; }
        [CanBeNull][JsonProperty("subscriptionDiscounts")] public SKProductDiscount[] SubscriptionDiscounts { get; set; }
    }
}