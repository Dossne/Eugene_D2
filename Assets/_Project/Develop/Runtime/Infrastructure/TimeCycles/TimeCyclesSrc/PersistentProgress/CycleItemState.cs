using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Infrastructure.TimeCycles
{
    [Serializable]
    public class CycleItemState
    {
        [JsonProperty("nrd")][JsonConverter(typeof(UnixDateTimeConverter))] public DateTime nextResetDate;
        [JsonProperty("ct")] public TimeCycleType cycleType;
    }
}