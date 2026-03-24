using System;
using Features.Collectables;
using Infrastructure.Utilities;
using Newtonsoft.Json;

namespace Features.LevelConfiguration
{
    [Serializable]
    public class CollectableData
    {
        [JsonProperty("t")] public CollectableType collectableType;
        [JsonProperty("p")] public SerializableVector3 position;
        [JsonProperty("r")] public SerializableVector3 rotation;
        [JsonProperty("s")] public SerializableVector3 scale;
        
        
        //Newtonsoft hooks
        public bool ShouldSerializeposition() => position.HasValue();
        public bool ShouldSerializerotation() => rotation.HasValue();
        public bool ShouldSerializescale() => scale.HasValue();
    }
}