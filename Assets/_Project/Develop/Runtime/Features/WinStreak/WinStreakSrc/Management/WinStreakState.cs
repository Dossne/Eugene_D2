using System;
using Newtonsoft.Json;

namespace Features.WinStreak
{
    [Serializable]
    public class WinStreakState
    {
        [JsonProperty("wl")] public int winStreakLevel;
        [JsonProperty("sln")] public int startedLevelNumber;
        [JsonProperty("lbc")] public int levelBeginCount;
    }
}