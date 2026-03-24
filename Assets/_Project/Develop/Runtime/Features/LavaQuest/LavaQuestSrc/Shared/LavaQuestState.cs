using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Features.LavaQuest
{
    [Serializable]
    public class LavaQuestState
    {
        [JsonProperty("chainIdx")] public int chainIdx = -1;
        [JsonProperty("stepIndex")] public int stepIndex;
        [JsonProperty("currentState")] public LQStateType currentState;
        [JsonProperty("scheduledAction")] public LQScheduledAction scheduledAction;
        [JsonProperty("startedLevelNumber")] public int startedLevelNumber;
        [JsonProperty("levelBeginCount")] public int levelBeginCount;
        [JsonProperty("nextDailyReset")] [JsonConverter(typeof(UnixDateTimeConverter))] public DateTime nextDailyReset = DateTime.UnixEpoch;
        [JsonProperty("endDate")] [JsonConverter(typeof(UnixDateTimeConverter))] public DateTime endDate = DateTime.UnixEpoch;
        [JsonProperty("playersFakeCounts")] public int[] playersFakeCounts;
        [JsonProperty("readyStartTimes")] public int readyStartTimes;
    }
}