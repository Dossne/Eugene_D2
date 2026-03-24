using System;
using Newtonsoft.Json;

namespace Features.LevelSessionStateControl
{
    [Serializable]
    public class LevelSessionState
    {
        [JsonProperty("lastLvl")] public int lastLevelNumber = -1;
        [JsonProperty("state")] public SessionStateType state;
        [JsonProperty("sessionData")] public string sessionDataCompressedJson;

        public void ClearSessionData()
        {
            sessionDataCompressedJson = null;
        }
    }
}