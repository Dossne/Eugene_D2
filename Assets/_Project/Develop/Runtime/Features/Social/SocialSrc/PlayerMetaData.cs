using Features.PlayerProfile;
using Newtonsoft.Json;
using System;


namespace Features.Social
{
    [Serializable]
    public class PlayerStatistics
    {
        [JsonProperty("fwc")] public int firstWinCount = 0;
        [JsonProperty("twc")] public int totalWinCount = 0;
        [JsonProperty("mws")] public int maxWinStreak = 0;
    }


    [Serializable]
    public class PlayerMetaData
    {
        [JsonProperty("pa")] public PlayerProfileAvatarDto playerAvatar;
        [JsonProperty("ps")] public PlayerStatistics playerStatistics;
    }
}


