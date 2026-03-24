using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{   
    [UnityEngine.Scripting.Preserve]
    public class AttributionResponseData
    {
        [JsonConstructor]
        public AttributionResponseData(string status, string creative, string campaign)
        {
            Status = status;
            Creative = creative;
            Campaign = campaign;
        }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("creative")]
        public string Creative { get; set; }

        [JsonProperty("campaign")]
        public string Campaign { get; set; }
        
    }
}