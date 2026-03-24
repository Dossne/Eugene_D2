using Newtonsoft.Json;
using System.Collections.Generic;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKLanguageLocalization
    {
        [JsonConstructor]
        public SKLanguageLocalization() { }
        
        [JsonProperty("hash")] 
        public string Hash { get; set; }
        [JsonProperty("language")] 
        public string Language { get; set; }
        [JsonProperty("strings")] 
        public Dictionary<string, string> Strings { get; set; }
    }
}