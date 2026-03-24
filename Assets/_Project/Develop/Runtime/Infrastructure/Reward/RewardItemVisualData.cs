using System;
using Features.Boosters;
using Features.PurchaseUi;
using Infrastructure.WalletSystem;
using UnityEngine;

namespace Infrastructure.Reward
{
    [Serializable]
    public class RewardItemVisualData : IEquatable<RewardItemVisualData>
    {
        public Sprite icon;
        public Sprite backgroundSprite;
        public int amount;
        public string amountText;
        public bool isDisplayRibbon;
        public bool isDisplayInfinityIcon;
        public PurchaseRewardType rewardType;

        public CurrencyType currencyType;
        public BoosterType boosterType;

        public bool Equals(RewardItemVisualData other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return isDisplayRibbon       == other.isDisplayRibbon
                && isDisplayInfinityIcon == other.isDisplayInfinityIcon
                && rewardType            == other.rewardType
                && currencyType          == other.currencyType
                && boosterType           == other.boosterType;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((RewardItemVisualData)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)currencyType, (int)boosterType);
        }
    }
}