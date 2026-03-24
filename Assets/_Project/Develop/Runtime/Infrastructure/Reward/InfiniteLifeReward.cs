using System;
using Newtonsoft.Json;

namespace Infrastructure.Reward
{
    [Serializable]
    public class InfiniteLifeReward
    {
        public int timeLengthMinutes;
        public bool isDisplayTimeText;
        public bool isDisplayRibbon;

        [JsonIgnore] public string IconName => "heart_infinity_icon";
    }
}

