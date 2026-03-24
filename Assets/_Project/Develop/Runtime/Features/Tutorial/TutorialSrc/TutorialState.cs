using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Features.Tutorial
{
    [Serializable]
    public class TutorialState
    {
        [JsonProperty("ct")] public List<string> completedTutorials = new();
    }
}