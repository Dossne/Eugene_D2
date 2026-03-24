using System;
using Features.Boosters;
using Newtonsoft.Json;

namespace Infrastructure.Reward
{
    [Serializable]
    public class BoosterReward
    {
        public BoosterType boosterType;
        public int amount;
        public bool isDisplayRibbon;
        [JsonProperty("dispAm")] public bool isDisplayAmount;
        
        public bool ShouldSerializeisDisplayAmount()
        {
            return isDisplayAmount;
        }
    }
}