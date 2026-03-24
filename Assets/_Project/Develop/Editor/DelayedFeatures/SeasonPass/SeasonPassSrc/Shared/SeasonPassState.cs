using System;
using System.Collections.Generic;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Features.SeasonPass
{
    [Serializable]
    public class SeasonPassState
    {
        [JsonProperty("collectedCount")] public int collectedCount;
        [JsonProperty("scheduledCnt")] public int scheduledCount;
        [JsonProperty("scheduledWs")] public bool scheduledWinStreak;
        [JsonProperty("isPremium")] public bool isPremiumBought;
        [JsonProperty("endDate")] [JsonConverter(typeof(UnixDateTimeConverter))] public DateTime endDate = DateTime.UnixEpoch;
        [JsonProperty("slots")] public List<SlotClaimState> slots = new();
    }
}