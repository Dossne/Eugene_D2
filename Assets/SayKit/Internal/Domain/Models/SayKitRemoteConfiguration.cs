using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitRemoteConfiguration
    {
        [JsonConstructor]
        public SayKitRemoteConfiguration() { }
        
        [JsonProperty("Name")] 
        public string Name { get; set; }
        
        [JsonProperty("Data")] 
        public string Data { get; set; }
    }
}