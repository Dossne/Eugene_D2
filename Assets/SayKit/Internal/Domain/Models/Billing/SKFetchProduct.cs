using Newtonsoft.Json;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKFetchProduct
    {
        [JsonConstructor]
        public SKFetchProduct() { }
        
        [JsonProperty("products")] public SKProduct[] Products { get; set; }
        [JsonProperty("error")] public SKBillingError Error { get; set; }

    }
}