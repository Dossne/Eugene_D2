using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitIAPVersion
    {
        [JsonConstructor]
        public SayKitIAPVersion(string version)
        {
            Version = version;
        }
        
        [JsonProperty("version")] 
        public string Version { get; set; }
    }
}