using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SayKitUnityException
    {
        [JsonConstructor]
        public SayKitUnityException() { }
        
        [JsonProperty("scene")] 
        public string Scene { get; set; }
        
        [JsonProperty("exception")] 
        public string Exception { get; set; }
    }
}