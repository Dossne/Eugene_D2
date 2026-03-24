using System;
using System.Collections.Generic;
using Infrastructure.Utilities;
using Newtonsoft.Json;

namespace Features.LevelConfiguration
{

    [Serializable]
    public class Level
    {
        [JsonProperty("c")] public List<CollectableData> collectables;
        [JsonProperty("t")] public LevelTableData table;
        [JsonProperty("s")] public SerializableVector3 spawnPointPosition;
    }
}