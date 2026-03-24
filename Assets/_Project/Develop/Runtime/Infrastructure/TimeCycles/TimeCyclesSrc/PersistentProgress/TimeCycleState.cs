using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Infrastructure.TimeCycles
{
    [Serializable]
    public class TimeCycleState
    {
        [JsonProperty("s")] public List<CycleItemState> states = new();
    }
}