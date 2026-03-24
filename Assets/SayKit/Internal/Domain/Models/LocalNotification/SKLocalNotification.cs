using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKLocalNotification
    {
        [JsonConstructor]
        public SKLocalNotification() { }
        
        [JsonProperty("id")] 
        public string Id { get; set; }
        [JsonProperty("title")] 
        public string Title { get; set; }
        [JsonProperty("message")] 
        public string Message { get; set; }
        [JsonProperty("deepLink")] 
        public string DeepLink { get; set; }
        [JsonProperty("calendarTrigger")] 
        public SKLocalNotificationCalendarTrigger CalendarTrigger { get; set; }
        [JsonProperty("timeIntervalTrigger")] 
        public SKLocalNotificationTimeIntervalTrigger TimeIntervalTrigger { get; set; }
        [JsonProperty("repeats")] 
        public bool Repeats { get; set; }
    }
}