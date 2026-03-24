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
    public class SKPurchasedProduct
    {
        [JsonConstructor]
        public SKPurchasedProduct() { }
        
        [JsonProperty("productId")] public string Id { get; set; }
        [JsonProperty("currencyCode")] public string CurrencyCode { get; set; }
        [JsonProperty("localizedPrice")] public string LocalizedPrice { get; set; }
        [JsonProperty("payload")] public string Payload { get; set; }
        [JsonProperty("store")] public SKPurchaseStore Store { get; set; }
        [JsonProperty("signature")] public string Signature { get; set; }
        [JsonProperty("transactionId")] public string TransactionId { get; set; }
        [JsonProperty("productType")] public SKProductType? Type { get; set; }
    }
}
