using Newtonsoft.Json;
using SayGames.Services.Model;
using System;


namespace Features.Social
{
    [Serializable]
    public class LeaderboardState
    {
        [JsonProperty("g")] public long generation = -1;
        [JsonProperty("r")] public LeaderboardRunningState runningState = LeaderboardRunningState.Unknown;
        [JsonProperty("nt")] public DateTime nextTick = DateTime.MinValue;
        [JsonProperty("l")] public long league = -1;
        [JsonProperty("d")] public long division = -1;
        [JsonProperty("jg")] public long joinedToGeneration = -1;
        [JsonProperty("us")] public long unsentScore = 0;
        [JsonProperty("iss")] public int isSubscoreSent = 0;



        public bool IsUserJoinedToLeaderboard => generation == joinedToGeneration;


        public bool IsSubscoreSent => isSubscoreSent != 0;



        public override string ToString()
        {
            return $"runningState: {runningState.ToString()}\n" +
                $"nextTick: {nextTick.ToString()}\n" +
                $"league: {league.ToString()}\n" +
                $"division: {division.ToString()}\n" +
                $"unsentScore: {unsentScore.ToString()}\n" +
                $"generation: {generation.ToString()}\n" +
                $"joinedToGeneration: {joinedToGeneration.ToString()}\n";
        }
    }
}


