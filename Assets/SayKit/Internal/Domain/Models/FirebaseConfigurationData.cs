using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class FirebaseConfiguration
    {
        [JsonConstructor]
        public FirebaseConfiguration() { }
        
        [JsonProperty("client")]
        public Client[] Client { get; set; }
    }
    
    [UnityEngine.Scripting.Preserve]
    public class Client
    {
        [JsonConstructor]
        public Client() { }
        
        [JsonProperty("client_info")]
        public ClientInfo ClientInfo { get; set; }
    }
    
    [UnityEngine.Scripting.Preserve]
    public class ClientInfo
    {
        [JsonConstructor]
        public ClientInfo() { }
        
        [JsonProperty("mobilesdk_app_id")]
        public string MobileSdkAppId { get; set; }
    }
}