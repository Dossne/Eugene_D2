using System;

namespace Infrastructure.Utilities
{
    public static class SerializableDateTimeExtensions
    {
        public static DateTime ToDateTime(this SerializableDateTime dateTime)
        {
            return new DateTime(dateTime.years, dateTime.month, dateTime.day, dateTime.hour, dateTime.minute, dateTime.second, dateTime.millisecond,
                                dateTime.kind);
        }


        public static SerializableDateTime ToSerializableDateTime(this DateTime dateTime)
        {
            return new SerializableDateTime
            {
                years = dateTime.Year,
                month = dateTime.Month,
                day = dateTime.Day,
                hour = dateTime.Hour,
                minute = dateTime.Minute,
                second = dateTime.Second,
                millisecond = dateTime.Millisecond,
                kind = dateTime.Kind
            };
        }


        public static SerializableDateTime Add(this SerializableDateTime dateTime, TimeSpan timeSpan)
        {
            var result = dateTime.ToDateTime().Add(timeSpan);
            return result.ToSerializableDateTime();
        }
    }
}