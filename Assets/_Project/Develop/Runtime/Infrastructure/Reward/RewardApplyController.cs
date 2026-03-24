using System.Collections.Generic;
using Features.Boosters;
using Features.Life;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.Reward.Container;
using Infrastructure.WalletSystem;
using UnityEngine;

namespace Infrastructure.Reward
{
    public class RewardApplyController
    {
        private readonly Wallet wallet;
        private readonly BoostersManager boostersManager;
        private readonly LifeController lifeController;
        private readonly RewardContainersConfig rewardContainersConfig;

        public RewardApplyController(Wallet wallet, BoostersManager boostersManager, LifeController lifeController, ConfigProvider configProvider)
        {
            this.wallet = wallet;
            this.boostersManager = boostersManager;
            this.lifeController = lifeController;
            rewardContainersConfig = configProvider.RewardContainersConfig;
        }

        public void ApplyComplexReward(ComplexReward complexReward, string reason, bool updateUi = true, float rewardMultiplier = 1)
        {
            ApplyComplexRewardInternal(complexReward, reason, updateUi, rewardMultiplier);
            ApplyRewardContainer(complexReward.containerId, reason, updateUi, rewardMultiplier);
        }

        /// <summary>
        /// Returns clone of input complexReward with merged into it data from container reward (if it presents in complexReward)
        /// </summary>
        /// <param name="complexReward"></param>
        /// <returns></returns>
        public ComplexReward GetRewardDataIncludeContainer(ComplexReward complexReward)
        {
            ComplexReward result = complexReward.Clone();

            if (!TryGetRewardContainerReward(result.containerId, out (string iconName, ComplexReward complexReward) containerReward))
            {
                return result;
            }

            var cComplex = containerReward.complexReward;

            if (cComplex.HaveCurrencyRewards())
            {
                result.currencyOfferRewards ??= new List<CurrencyOfferReward>();
                result.currencyOfferRewards.AddRange(cComplex.currencyOfferRewards);
            }

            if (cComplex.HaveBoosterRewards())
            {
                result.boosterRewards ??= new List<BoosterReward>();
                result.boosterRewards.AddRange(cComplex.boosterRewards);
            }

            if (cComplex.HavePreBoosterRewards())
            {
                result.preBoosterRewards ??= new List<BoosterReward>();
                result.preBoosterRewards.AddRange(cComplex.preBoosterRewards);
            }

            if (cComplex.HaveInfinitePreBoosterRewards())
            {
                result.infinitePreBoosterRewards ??= new List<InfiniteBoosterReward>();
                result.infinitePreBoosterRewards.AddRange(cComplex.infinitePreBoosterRewards);
            }

            if (cComplex.HaveInfiniteLifeReward())
            {
                if (result.HaveInfiniteLifeReward())
                    result.infiniteLifeReward.timeLengthMinutes += cComplex.infiniteLifeReward.timeLengthMinutes;
                else
                    result.infiniteLifeReward = cComplex.infiniteLifeReward;
            }

            return result;
        }

        private void ApplyComplexRewardInternal(ComplexReward complexReward, string reason, bool updateUi, float rewardMultiplier)
        {
            ApplyNoAdsReward(complexReward);
            ApplyCurrencyRewards(complexReward.currencyOfferRewards, reason, updateUi, rewardMultiplier);
            ApplyBoosterRewards(complexReward.preBoosterRewards);
            ApplyBoosterRewards(complexReward.boosterRewards);
            ApplyInfiniteBoosterRewards(complexReward.infinitePreBoosterRewards);
            ApplyInfiniteLifeReward(complexReward.infiniteLifeReward);
        }

        private void ApplyNoAdsReward(ComplexReward complexReward)
        {
            if (!complexReward.noAdsIncluded)
                return;

            if (Advertisement.IsPremium())
            {
                Debug.LogError("NoAds offer already purchased!");
                return;
            }

            Advertisement.EnablePremium();
        }

        private void ApplyCurrencyRewards(List<CurrencyOfferReward> value, string reason, bool updateUi = true, float rewardMultiplier = 1f)
        {
            for (int i = 0; i < value.Count; i++)
            {
                int giveCount = Mathf.RoundToInt(value[i].amount * rewardMultiplier);
                wallet.Increase(value[i].currencyType, giveCount, reason, updateUi);
            }
        }

        private void ApplyBoosterRewards(List<BoosterReward> value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                boostersManager.AddBooster(value[i].boosterType, value[i].amount);
            }
        }

        private void ApplyInfiniteBoosterRewards(List<InfiniteBoosterReward> value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                boostersManager.AddInfiniteBooster(value[i].boosterType, value[i].timeLengthMinutes);
            }
        }

        private void ApplyInfiniteLifeReward(InfiniteLifeReward value)
        {
            if (value.timeLengthMinutes <= 0)
                return;

            lifeController.AddInfiniteLifeTime(value.timeLengthMinutes);
        }

        private void ApplyRewardContainer(string rewardContainerId, string reason, bool updateUi = true, float rewardMultiplier = 1)
        {
            if (!TryGetRewardContainerReward(rewardContainerId, out var rewardContainerInfo))
            {
                return;
            }

            ApplyComplexRewardInternal(rewardContainerInfo.complexReward, reason, updateUi, rewardMultiplier);
        }

        private bool TryGetRewardContainerReward(string containerId, out (string iconName, ComplexReward complexReward) result)
        {
            return rewardContainersConfig.TryGetRewardContainerReward(containerId, out result);
        }
    }
}