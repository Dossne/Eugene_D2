#if UNITY_EDITOR

using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class StorageData
    {
        [JsonConstructor]
        public StorageData() { }
        
        [JsonProperty("rateAppPopupWasShowed")]
        public bool RateAppPopupWasShowed { get; set; }
        
        [JsonProperty("isPremium")]
        public bool IsPremium { get; set; }
    }
}
#endif