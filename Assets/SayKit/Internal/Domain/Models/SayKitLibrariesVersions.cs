using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitLibrariesVersions
    {
        [JsonConstructor]
        public SayKitLibrariesVersions(string android, string ios, string unity)
        {
            Android = android;
            iOS = ios;
            Unity = unity;
        }
        
        [JsonProperty("android")] 
        public string Android { get; set; }
        
        [JsonProperty("ios")] 
        public string iOS { get; set; }
        
        [JsonProperty("unity")] 
        public string Unity { get; set; }
    }
}