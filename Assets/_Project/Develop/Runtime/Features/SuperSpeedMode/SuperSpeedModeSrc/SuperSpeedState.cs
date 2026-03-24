using System;

using Newtonsoft.Json;


namespace Features.SuperSpeedMode
{
    [Serializable]
    public class SuperSpeedState
    {
        [JsonProperty("wc")] public int winCounter = 0;
        [JsonProperty("ps")] public bool isPopupShown = false;
    }
}