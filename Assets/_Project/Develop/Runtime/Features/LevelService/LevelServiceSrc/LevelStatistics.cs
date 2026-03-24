using Newtonsoft.Json;
using System;


namespace Features.Level
{
    [Serializable]
    public class LevelStatistics
    {
        [JsonProperty("pfl")] public int previousFinisedLevel = 0;
        [JsonProperty("fwc")] public int firstWinCount        = 0;
        [JsonProperty("twc")] public int totalWinCount        = 0;
        [JsonProperty("cws")] public int currentWinStreak     = 0;
        [JsonProperty("mws")] public int maxWinStreak         = 0;
    }
}