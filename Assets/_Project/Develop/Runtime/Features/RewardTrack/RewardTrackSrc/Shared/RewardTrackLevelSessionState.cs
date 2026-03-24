using System;
using System.Collections.Generic;
using Features.Collectables;
using Features.LevelConfiguration;
using Newtonsoft.Json;

namespace Features.RewardTrack
{
    [Serializable]
    public class RewardTrackLevelSessionState
    {
        [JsonProperty("count")] public int collectedCountRaw;
        [JsonProperty("target")] public CollectableType targetItemType;
        [JsonProperty("spawnData")] public List<CollectableData> spawnData;
    }
}