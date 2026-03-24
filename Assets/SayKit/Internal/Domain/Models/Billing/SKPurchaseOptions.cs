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
    public class SKPurchaseOptions
    {
        [JsonConstructor]
        public SKPurchaseOptions() { }
        
        [JsonProperty("store")] 
        public SKPurchaseStore? Store { get; set; }
#if UNITY_ANDROID
        public bool IsOfferPersonalized { get; set; }
#endif
    }
}