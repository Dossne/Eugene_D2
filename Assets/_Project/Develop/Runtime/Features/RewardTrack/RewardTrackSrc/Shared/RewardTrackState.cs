using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Features.RewardTrack
{
    [Serializable]
    public class RewardTrackState
    {
        [JsonProperty("cti")] public int collectTargetIdx = -1;
        [JsonProperty("cic")] public int collectedItemCount;
        [JsonProperty("end")] [JsonConverter(typeof(UnixDateTimeConverter))] public DateTime endDate = DateTime.UnixEpoch;
    }
}