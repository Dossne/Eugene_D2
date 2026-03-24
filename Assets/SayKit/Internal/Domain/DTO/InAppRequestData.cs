using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class InAppRequestData
    {
        [JsonConstructor]
        public InAppRequestData() { }
        
        [JsonProperty("product_id")]
        public string ProductId;
        
        [JsonProperty("store_product_id")]
        public string StoreProductId;
        
        [JsonProperty("currency")]
        public string Currency;
        
        [JsonProperty("price")]
        public float Price;
        
        [JsonProperty("transaction_id")]
        public string TransactionId;

#if UNITY_IOS
        [JsonProperty("receipt")]
        public string Receipt;
#elif UNITY_ANDROID
        [JsonProperty("json")]
        public string Json;

        [JsonProperty("signature")]
        public string Signature;
#endif
    }
}