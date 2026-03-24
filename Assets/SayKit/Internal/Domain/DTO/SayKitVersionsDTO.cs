using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitVersionsDTO
    {
        [JsonConstructor]
        public SayKitVersionsDTO() { }
        
        [JsonProperty("versions")]
        public SayKitVersionDTO[] Versions { get; set; }
    }
}