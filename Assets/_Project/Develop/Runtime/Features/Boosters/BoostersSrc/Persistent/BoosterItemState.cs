using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Features.Boosters
{
    [Serializable]
    public class BoosterItemState
    {
        [JsonProperty("t")] public BoosterType type;
        [JsonProperty("c")] public int count;
        [JsonProperty("i")][JsonConverter(typeof(UnixDateTimeConverter))] public DateTime infiniteExpiresTime = DateTime.UnixEpoch;
    }
}