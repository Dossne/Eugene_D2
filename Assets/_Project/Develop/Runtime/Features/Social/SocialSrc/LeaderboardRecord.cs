using Features.PlayerProfile;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using System;
using UnityEngine;


namespace Features.Social
{
    [Serializable]
    public class LeaderboardRecord    
    {
        [JsonProperty("dn")] public string displayName;
        [JsonProperty("md")] public PlayerMetaData metaData;
        [JsonProperty("s")] public long score;
        [JsonProperty("ss")] public long subScore;
        [JsonProperty("p")] public int isPlayer;



        public override string ToString()
        {
            return $"{displayName} : {score} : {metaData.playerAvatar.avatarId}" +
                $" : {metaData.playerStatistics.firstWinCount}" +
                $" : {metaData.playerStatistics.totalWinCount}" +
                $" : {metaData.playerStatistics.maxWinStreak}";

        }


        public bool IsPlayer()
        {
            return isPlayer == 1;
        }
    }
}


