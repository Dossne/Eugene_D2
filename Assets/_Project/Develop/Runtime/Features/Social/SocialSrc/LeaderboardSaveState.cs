using Newtonsoft.Json;
using SayGames.Services.Model;
using System;
using System.Collections.Generic;


namespace Features.Social
{
    [Serializable]
    public class LeaderboardSaveState
    {
        [JsonProperty("n")] public string name = string.Empty;
        [JsonProperty("ls")] public LeaderboardState leaderboardState;
        [JsonProperty("lr")] public LeaderboardRecords leaderboardRecords;
    }
}


