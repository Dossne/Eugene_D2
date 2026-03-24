using System;
using Infrastructure.Utilities;
using Newtonsoft.Json;

namespace Features.LevelConfiguration
{
    [Serializable]
    public class LevelTableData
    {
        [JsonProperty("a")] public string prefabName;
        [JsonProperty("p")] public SerializableVector3 position;
        [JsonProperty("r")] public SerializableVector3 rotation;
        [JsonProperty("s")] public SerializableVector3 scale;
    }
}