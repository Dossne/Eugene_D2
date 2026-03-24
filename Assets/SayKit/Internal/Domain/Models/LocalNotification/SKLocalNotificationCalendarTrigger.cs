using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKLocalNotificationCalendarTrigger
    { 
        [JsonConstructor]
        public SKLocalNotificationCalendarTrigger() { }
        
        [JsonProperty("year")] 
        public int? Year { get; set; }
        [JsonProperty("month")] 
        public int? Month { get; set; }
        [JsonProperty("day")] 
        public int? Day { get; set; }
        [JsonProperty("hour")] 
        public int? Hour { get; set; }
        [JsonProperty("minute")] 
        public int? Minute { get; set; }
        [JsonProperty("second")] 
        public int? Second { get; set; }
        
        /// A weekday or count of weekdays.
        /// - note: This value is interpreted in the context of the calendar in which it is used.
        [JsonProperty("weekday")]
        public int? Weekday { get; set; } 
        
        /// A week of the month or a count of weeks of the month.
        /// - note: This value is interpreted in the context of the calendar in which it is used.
        [JsonProperty("weekOfMonth")]
        public int? WeekOfMonth { get; set; }
        
        /// A weekday ordinal or count of weekday ordinals.
        /// Weekday ordinal units represent the position of the weekday within the next larger calendar unit, such as the month. For example, 2 is the weekday ordinal unit for the second Friday of the month.
        /// - note: This value is interpreted in the context of the calendar in which it is used.
        [JsonProperty("weekdayOrdinal")]
        public int? WeekdayOrdinal { get; set; }
        
        /// A week of the year or count of the weeks of the year.
        /// - note: This value is interpreted in the context of the calendar in which it is used.
        [JsonProperty("weekOfYear")]
        public int? WeekOfYear { get; set; }
        
    }
}