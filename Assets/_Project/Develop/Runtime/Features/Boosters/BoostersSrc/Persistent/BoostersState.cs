using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Features.Boosters
{
    [Serializable]
    public class BoostersState
    {
        [JsonProperty("s")] public List<BoosterItemState> states = new();
    }
}