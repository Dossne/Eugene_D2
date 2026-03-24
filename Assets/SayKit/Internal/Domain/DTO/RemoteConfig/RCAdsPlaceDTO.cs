using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class RCAdsPlaceDTO
    {
        [JsonConstructor]
        public RCAdsPlaceDTO() { }
        
        [JsonProperty("place")]
        public string place { get; set; } = string.Empty;
        
        [JsonProperty("group")]
        public string group { get; set; } = string.Empty;
        
        [JsonProperty("status")]
        public string status { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string type { get; set; } = string.Empty;
        
        public RCAdsPlaceDTO Clone()
        {
            return new RCAdsPlaceDTO
            {
                place = string.Copy(this.place),
                group = string.Copy(this.group),
                status = string.Copy(this.status),
                type = string.Copy(this.type)
            };
        }
    }
}