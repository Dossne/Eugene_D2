using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitLiveRuntimeResponse
    {
        [JsonConstructor]
        public SayKitLiveRuntimeResponse() { }
        
        [JsonProperty("timestamp")] public long Timestamp;
    }
}