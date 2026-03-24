using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class InAppResponseData
    {
        [JsonConstructor]
        public InAppResponseData() { }
        
        [JsonProperty("success")]
        public bool Success;
        
        [JsonProperty("seen_before")]
        public bool SeenBefore;
        
        [JsonProperty("message")]
        public string Message;
        
        [JsonProperty("adjust_iap_token")]
        public string AdjustIapToken;
    }
}