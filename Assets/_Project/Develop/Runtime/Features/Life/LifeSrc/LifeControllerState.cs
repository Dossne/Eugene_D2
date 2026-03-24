using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Features.Life
{
    [Serializable]
    public class LifeControllerState
    {
        public bool firstLaunch = true;
        [JsonProperty("rrc")] public int rewardedResurrectCount = 0;
        [JsonProperty("r")][JsonConverter(typeof(UnixDateTimeConverter))] public DateTime lastLifeRestoreDate = DateTime.UnixEpoch;
        [JsonProperty("i")][JsonConverter(typeof(UnixDateTimeConverter))] public DateTime infiniteLifeExpireDate = DateTime.UnixEpoch;
        [JsonProperty("brc")] public int bombResurrectCount = 0;
        [JsonProperty("rlc")] public int rewardedLifeCount;
    }
}