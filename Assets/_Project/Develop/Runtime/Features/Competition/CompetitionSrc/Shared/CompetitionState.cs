using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Features.Competition
{
    [Serializable]
    public class CompetitionState
    {
        [JsonProperty("state")] public CStateType currentState;
        [JsonProperty("stct")] public int startCountTimes; //how many times players saw new competition
        [JsonProperty("winRow")] public int winCountInRow;

        [JsonProperty("collectedCount")] public int collectedCount;
        [JsonProperty("scheduledCnt")] public int scheduledCount;
        [JsonProperty("scheduledWs")] public bool scheduledWinStreak;
        [JsonProperty("endDate")] [JsonConverter(typeof(UnixDateTimeConverter))] public DateTime endDate = DateTime.UnixEpoch;
        
        //popup state
        [JsonProperty("gRewardIdx")] public int givenRewardIdx = -1;
        [JsonProperty("nRewardCount")] public int notifierRewardCount;
        [JsonProperty("nRewardIdx")] public int notifierRewardIdx = -1;
        [JsonProperty("popupPosIdx")] public int lastLeaderboardIdx;

        //analytics
        [JsonProperty("startpst")] public int startPopupShownTimes = 0;
        [JsonProperty("rewardtna")] public int rewardTrackNumberAvailable = 0;
        [JsonProperty("rewardsna")] public int rewardServerNumberAvailable = 0;
        [JsonProperty("finishst")] public int finishSendAnalyticTimes = 0;
        
        //other
        [JsonProperty("lbname")] public string leaderboardName;

        
        public bool IsResetTime(DateTime time)
        {
            return time > endDate;
        }
    }
}