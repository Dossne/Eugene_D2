using System;
using Infrastructure.DateTimeControl;
using Infrastructure.Settings;
using Infrastructure.TimeCycles;
using Newtonsoft.Json;

namespace Infrastructure.PersistentProgress
{
    [Serializable]
    public class AppState
    {
        public int launchCount;
        public AppSettingsData appSettings = new();
        [JsonProperty("dt")] public DateTimeState dateTime = new();
        [JsonProperty("tc")] public TimeCycleState timeCycle = new();
        public bool rateUsShown = false;
    }
}