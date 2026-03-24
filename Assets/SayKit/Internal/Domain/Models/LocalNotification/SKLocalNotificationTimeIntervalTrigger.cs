using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKLocalNotificationTimeIntervalTrigger
    {
        [JsonConstructor]
        public SKLocalNotificationTimeIntervalTrigger() { }
        
        [JsonProperty("timeInterval")]
        public double TimeInterval { get; set; }
    }
}