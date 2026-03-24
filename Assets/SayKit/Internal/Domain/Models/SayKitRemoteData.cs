using System.Collections.Generic;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitRemoteData
    {
        [JsonConstructor]
        public SayKitRemoteData() { }
        
        [JsonProperty("Version")] 
        public int Version { get; set; }
        
        [JsonProperty("Error")] 
        public string Error { get; set; }
        
        [JsonProperty("Platform")] 
        public string Platform { get; set; }
        
        [JsonProperty("Configuration")] 
        public List<SayKitRemoteConfiguration> Configuration { get; set; }
    }
}