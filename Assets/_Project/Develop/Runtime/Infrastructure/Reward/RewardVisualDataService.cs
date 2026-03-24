using System;
using System.Collections.Generic;
using Features.Boosters;
using Features.PurchaseUi;
using Infrastructure.Configs;
using Infrastructure.Reward.Container;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;

namespace Infrastructure.Reward
{
    public class RewardVisualDataService
    {
        private readonly BoostersManager boostersManager;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly RewardContainersConfig rewardContainersConfig;

        public RewardVisualDataService(BoostersManager boostersManager, SpriteAtlasService spriteAtlasService, ConfigProvider configProvider)
        {
            this.boostersManager = boostersManager;
            this.spriteAtlasService = spriteAtlasService;
            this.rewardContainersConfig = configProvider.RewardContainersConfig;
        }

        public RewardItemVisualData GetFirstRewardVisualData(string rewardJson)
        {
            ComplexReward reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardJson);
            return GetFirstRewardVisualData(reward);
        }

        public RewardContainerVisualData GetContainerData(string rewardJson)
        {
            ComplexReward reward = RewardUtils.ConvertFromJson<ComplexReward>(rewardJson);
            return GetContainerData(reward);
        }

        public RewardContainerVisualData GetContainerData(ComplexReward reward)
        {
            var result = new RewardContainerVisualData
            {
                rewards = GetRewardViewParams(reward)
            };

            if (rewardContainersConfig.TryGetRewardContainerReward(reward.containerId, out (string iconName, ComplexReward complexReward) containerReward))
            {
                result.haveContainer = true;
                result.containerIcon = spriteAtlasService.GetFromMain(containerReward.iconName);
                result.containerRewards = GetRewardViewParams(containerReward.complexReward);
            }

            return result;
        }

        public RewardItemVisualData GetFirstRewardVisualData(ComplexReward reward)
        {
            if (reward.currencyOfferRewards.Count > 0)
            {
                return GetCurrencyRewardInfo(reward.currencyOfferRewards[0]);
            }

            if (reward.boosterRewards.Count > 0)
            {
                return GetBoosterRewardInfoBase(reward.boosterRewards[0], PurchaseRewardType.Booster);
            }

            if (reward.preBoosterRewards.Count > 0)
            {
                return GetBoosterRewardInfoBase(reward.preBoosterRewards[0], PurchaseRewardType.PreBooster);
            }

            if (reward.infinitePreBoosterRewards.Count > 0)
            {
                return GetInfinitePreBoosterRewardInfo(reward.infinitePreBoosterRewards[0]);
            }

            if (reward.infiniteLifeReward.timeLengthMinutes > 0)
            {
                return GetInfiniteLifeRewardInfo(reward.infiniteLifeReward);
            }

            return default;
        }

        public static void MergeEqualItems(List<RewardItemVisualData> input)
        {
            for (int i = 0; i < input.Count; i++)
            {
                int count = input.Count;
                var left = input[i];

                for (int j = count - 1; j > i; j--)
                {
                    var right = input[j];

                    if (left == null || !left.Equals(right))
                        continue;

                    left.amount += right.amount;
                    left.amountText = GetText(left.rewardType, left.amount);
                    input.RemoveAt(j);
                }
            }
        }

        private static string GetText(PurchaseRewardType rewardType, long amount)
        {
            switch (rewardType)
            {
                case PurchaseRewardType.Currency:
                    return Utils.GetSpaceSeparatedNumberString(amount);
                case PurchaseRewardType.Booster:
                    return amount.ToString();
                case PurchaseRewardType.PreBooster:
                    return amount.ToString();
                case PurchaseRewardType.InfinitePreBooster:
                    return TimeUtils.GetTimeString(amount * 60f, 100.0f, showMinorZeroValues: false);
                case PurchaseRewardType.InfiniteLife:
                    return TimeUtils.GetTimeString(amount * 60f, 100.0f, showMinorZeroValues: false);
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(rewardType), rewardType, null);
            }
        }

        private List<RewardItemVisualData> GetRewardViewParams(ComplexReward reward)
        {
            var result = new List<RewardItemVisualData>();

            for (int i = 0; i < reward.currencyOfferRewards.Count; i++)
            {
                result.Add(GetCurrencyRewardInfo(reward.currencyOfferRewards[i]));
            }

            for (int i = 0; i < reward.boosterRewards.Count; i++)
            {
                result.Add(GetBoosterRewardInfoBase(reward.boosterRewards[i], PurchaseRewardType.Booster));
            }

            for (int i = 0; i < reward.preBoosterRewards.Count; i++)
            {
                result.Add(GetBoosterRewardInfoBase(reward.preBoosterRewards[i], PurchaseRewardType.PreBooster));
            }

            for (int i = 0; i < reward.infinitePreBoosterRewards.Count; i++)
            {
                result.Add(GetInfinitePreBoosterRewardInfo(reward.infinitePreBoosterRewards[i]));
            }

            if (reward.infiniteLifeReward.timeLengthMinutes > 0)
            {
                result.Add(GetInfiniteLifeRewardInfo(reward.infiniteLifeReward));
            }

            return result;
        }

        private RewardItemVisualData GetCurrencyRewardInfo(CurrencyOfferReward reward)
        {
            return new RewardItemVisualData
            {
                icon = reward.overrideIconId.IsNullOrEmpty()
                    ? spriteAtlasService.GetCurrencyIcon(reward.currencyType)
                    : spriteAtlasService.GetFromMain(reward.overrideIconId),
                amountText = GetText(PurchaseRewardType.Currency, reward.amount),
                isDisplayRibbon = reward.isDisplayRibbon,
                amount = reward.amount,
                rewardType = PurchaseRewardType.Currency,
                currencyType = reward.currencyType,
                boosterType = BoosterType.None
            };
        }

        private RewardItemVisualData GetBoosterRewardInfoBase(BoosterReward reward, PurchaseRewardType rewardType)
        {
            boostersManager.TryGetBoosterIconName(reward.boosterType, out string boosterIconName);

            return new RewardItemVisualData
            {
                icon = spriteAtlasService.GetFromMain(boosterIconName),
                isDisplayRibbon = reward.isDisplayRibbon,
                amountText = reward.isDisplayAmount ? GetText(rewardType, reward.amount) : null,
                amount = reward.amount,
                rewardType = rewardType,
                currencyType = CurrencyType.None,
                boosterType = reward.boosterType
            };
        }

        private RewardItemVisualData GetInfinitePreBoosterRewardInfo(InfiniteBoosterReward reward)
        {
            boostersManager.TryGetBoosterInfiniteIconName(reward.boosterType, out string boosterIconName);

            return new RewardItemVisualData
            {
                icon = spriteAtlasService.GetFromMain(boosterIconName),
                amount = reward.timeLengthMinutes,
                amountText = reward.isDisplayTimeText ? GetText(PurchaseRewardType.InfinitePreBooster, reward.timeLengthMinutes) : null,
                isDisplayRibbon = reward.isDisplayRibbon,
                isDisplayInfinityIcon = reward.isDisplayInfinityIcon,
                rewardType = PurchaseRewardType.InfinitePreBooster,
                currencyType = CurrencyType.None,
                boosterType = reward.boosterType
            };
        }

        private RewardItemVisualData GetInfiniteLifeRewardInfo(InfiniteLifeReward reward)
        {
            return new RewardItemVisualData
            {
                icon = spriteAtlasService.GetFromMain(reward.IconName),
                amount = reward.timeLengthMinutes,
                amountText = reward.isDisplayTimeText ? GetText(PurchaseRewardType.InfiniteLife, reward.timeLengthMinutes) : null,
                isDisplayRibbon = reward.isDisplayRibbon,
                rewardType = PurchaseRewardType.InfiniteLife,
                currencyType = CurrencyType.Life,
                boosterType = BoosterType.None
            };
        }
    }
}