using System;
using System.Collections.Generic;
using Infrastructure.WalletSystem;
using Newtonsoft.Json;

namespace Infrastructure.Reward
{
    public class AnalyticsRewardJsonCreator
    {
        [Serializable]
        public struct RewardJson
        {
            [JsonProperty("reward_id")] public string rewardId;
            [JsonProperty("amount")] public int amount;
            [JsonProperty("minutes")] public int minutes;

            //Serialization tuning for Newtonsoft.Json
            public bool ShouldSerializeamount()
            {
                return amount > 0;
            }

            public bool ShouldSerializeminutes()
            {
                return minutes > 0;
            }
        }

        /// <summary>
        /// Create json from complex reward for analytics
        /// </summary>
        /// <param name="input">If it should include container reward (containerId != null), do merge data in RewardApplyController.GetRewardDataIncludeContainer</param>
        /// <returns>result string WITH QUOTES</returns>
        public static string CreateJson(ComplexReward input)
        {
            List<RewardJson> rewardJsons = new();

            if (input.HaveCurrencyRewards())
            {
                foreach (var reward in input.currencyOfferRewards)
                {
                    rewardJsons.Add(new RewardJson
                    {
                        rewardId = reward.currencyType.ToString(),
                        amount = reward.amount
                    });
                }
            }

            if (input.HaveBoosterRewards())
            {
                foreach (var reward in input.boosterRewards)
                {
                    rewardJsons.Add(new RewardJson
                    {
                        rewardId = reward.boosterType.ToString(),
                        amount = reward.amount
                    });
                }
            }

            if (input.HavePreBoosterRewards())
            {
                foreach (var reward in input.preBoosterRewards)
                {
                    rewardJsons.Add(new RewardJson
                    {
                        rewardId = reward.boosterType.ToString(),
                        amount = reward.amount
                    });
                }
            }

            if (input.HaveInfinitePreBoosterRewards())
            {
                foreach (var reward in input.infinitePreBoosterRewards)
                {
                    rewardJsons.Add(new RewardJson
                    {
                        rewardId = reward.boosterType.ToString(),
                        minutes = reward.timeLengthMinutes
                    });
                }
            }

            if (input.HaveInfiniteLifeReward())
            {
                rewardJsons.Add(new RewardJson
                {
                    rewardId = nameof(CurrencyType.Life),
                    minutes = input.infiniteLifeReward.timeLengthMinutes
                });
            }

            return JsonConvert.SerializeObject(rewardJsons);
        }
    }
}