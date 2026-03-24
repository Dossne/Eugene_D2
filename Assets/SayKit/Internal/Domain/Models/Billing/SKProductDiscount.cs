using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKProductDiscount
    {
        [JsonConstructor]
        public SKProductDiscount() { }
        
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("price")] public string Price { get; set; }
        [JsonProperty("localizedPrice")] public string LocalizedPrice { get; set; }
        [JsonProperty("paymentMode")] public string PaymentMode { get; set; }
        [JsonProperty("currencyCode")] public string CurrencyCode { get; set; }
        [JsonProperty("period")] public string Period { get; set; }
    }
}