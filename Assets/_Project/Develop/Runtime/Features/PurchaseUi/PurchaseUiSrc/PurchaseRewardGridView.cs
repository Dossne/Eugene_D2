using Features.Boosters;
using Infrastructure.PurchaseSystem;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Features.PurchaseUi
{
    public class PurchaseRewardGridView : MonoBehaviour
    {
        [SerializeField] private float horizontalLayoutMaxHeight = 300f;
        [SerializeField] private List<PurchaseRewardItemView> purchaseRewardItemViewList = new();
        [SerializeField] private List<PurchaseRewardType> rewardTypeOrder = new();
        [SerializeField] private List<PurchaseRewardType> disabledRewardTypes = new();

        private SpriteAtlasService spriteAtlasService;
        private List<BoosterData> boosterConfig = new();



        public List<PurchaseRewardItemView> PurchaseRewardItemViewList => purchaseRewardItemViewList;



        public void Construct(ComplexReward complexReward, SpriteAtlasService spriteAtlasService, List<BoosterData> boosterConfig) 
        {
            this.spriteAtlasService = spriteAtlasService;
            this.boosterConfig = boosterConfig;
            int startIndex = 0;
            foreach (var item in rewardTypeOrder)
                startIndex = SetRewards(complexReward, item, startIndex);

            for (int i = startIndex; i < purchaseRewardItemViewList.Count; i++)
                purchaseRewardItemViewList[i].gameObject.SetActive(false);

            SetHorizontalLayoutsMaxHeight();
        }


        private int SetRewards(ComplexReward complexReward, PurchaseRewardType purchaseRewardType, int startIndex) 
        {
            if (complexReward == null)
                return startIndex;

            if (disabledRewardTypes.Contains(purchaseRewardType))
                return startIndex;

            return purchaseRewardType switch
            {
                PurchaseRewardType.Currency           => SetCurrencyReward(complexReward, startIndex),
                PurchaseRewardType.Booster            => SetBoosterReward(complexReward, startIndex),
                PurchaseRewardType.PreBooster         => SetPreBoosterReward(complexReward, startIndex),
                PurchaseRewardType.InfinitePreBooster => SetInfinitePreBoosterReward(complexReward, startIndex),
                PurchaseRewardType.InfiniteLife       => SetInfiniteLifeReward(complexReward, startIndex),
                _ => startIndex,
            };
        }

        private int SetInfiniteLifeReward(ComplexReward complexReward, int startIndex)
        {            
            var viewCount = purchaseRewardItemViewList.Count;
            var infiniteLifeReward = complexReward.infiniteLifeReward;
            if (startIndex >= viewCount || infiniteLifeReward.timeLengthMinutes == 0)
                return startIndex;

            var icon = spriteAtlasService.GetFromMain(infiniteLifeReward.IconName);
            var text = infiniteLifeReward.isDisplayTimeText ? TimeUtils.GetTimeString(infiniteLifeReward.timeLengthMinutes * 60f, 100.0f, showMinorZeroValues: false) : null;

            purchaseRewardItemViewList[startIndex].gameObject.SetActive(true);
            purchaseRewardItemViewList[startIndex].Construct(icon, text, null, infiniteLifeReward.isDisplayRibbon);

            startIndex++;
            return startIndex;
        }

        private int SetInfinitePreBoosterReward(ComplexReward complexReward, int startIndex)
        {
            var viewCount = purchaseRewardItemViewList.Count;
            var infinitePreBoosterRewards = complexReward.infinitePreBoosterRewards;

            for (int i = 0; i < infinitePreBoosterRewards.Count; i++)
            {
                if (startIndex >= viewCount)
                    break;

                var icon = spriteAtlasService.GetFromMain(boosterConfig.Find(x => x.type == infinitePreBoosterRewards[i].boosterType).shopIconName);
                var text = TimeUtils.GetTimeString(infinitePreBoosterRewards[i].timeLengthMinutes * 60f, 100.0f, showMinorZeroValues: false);

                purchaseRewardItemViewList[startIndex].gameObject.SetActive(true);
                purchaseRewardItemViewList[startIndex].Construct(icon, text, isDisplayRibbon: infinitePreBoosterRewards[i].isDisplayRibbon);

                startIndex++;
            }
            return startIndex;
        }

        private int SetPreBoosterReward(ComplexReward complexReward, int startIndex)
        {
            var viewCount = purchaseRewardItemViewList.Count;
            var preBoosterRewards = complexReward.preBoosterRewards;

            for (int i = 0; i < preBoosterRewards.Count; i++)
            {
                if (startIndex >= viewCount)
                    break;

                var icon = spriteAtlasService.GetFromMain(boosterConfig.Find(x => x.type == preBoosterRewards[i].boosterType).shopIconName);
                var text = $"<sprite name=\"multiplier_sign\">{preBoosterRewards[i].amount}";

                purchaseRewardItemViewList[startIndex].gameObject.SetActive(true);
                purchaseRewardItemViewList[startIndex].Construct(icon, text, isDisplayRibbon: preBoosterRewards[i].isDisplayRibbon);

                startIndex++;
            }
            return startIndex;
        }

        private int SetBoosterReward(ComplexReward complexReward, int startIndex)
        {
            var viewCount = purchaseRewardItemViewList.Count;
            var boosterRewards = complexReward.boosterRewards;

            for (int i = 0; i < boosterRewards.Count; i++)
            {
                if (startIndex >= viewCount)
                    break;

                var icon = spriteAtlasService.GetFromMain(boosterConfig.Find(x => x.type == boosterRewards[i].boosterType).shopIconName);
                var text = $"<sprite name=\"multiplier_sign\">{boosterRewards[i].amount}";

                purchaseRewardItemViewList[startIndex].gameObject.SetActive(true);
                purchaseRewardItemViewList[startIndex].Construct(icon, text, isDisplayRibbon: boosterRewards[i].isDisplayRibbon);

                startIndex++;
            }
            return startIndex;
        }

        private int SetCurrencyReward(ComplexReward complexReward, int startIndex)
        {
            var viewCount = purchaseRewardItemViewList.Count;
            var currencyRewards = complexReward.currencyOfferRewards;
            for (int i = 0; i < currencyRewards.Count; i++)
            {
                if (startIndex >= viewCount)
                    break;

                var icon = currencyRewards[i].overrideIconId.IsNullOrEmpty() 
                         ? spriteAtlasService.GetCurrencyIcon(currencyRewards[i].currencyType) 
                         : spriteAtlasService.GetFromMain(currencyRewards[i].overrideIconId);
                var text = Utils.GetSpaceSeparatedNumberString(currencyRewards[i].amount);

                purchaseRewardItemViewList[startIndex].gameObject.SetActive(true);
                purchaseRewardItemViewList[startIndex].Construct(icon, text, isDisplayRibbon: currencyRewards[i].isDisplayRibbon);

                startIndex++;
            }
            return startIndex;
        }


        private void SetHorizontalLayoutsMaxHeight()
        {
            foreach (var l in GetComponentsInChildren<LayoutElementMaxHeightLimiter>())
            {
                l.MaxHeight = horizontalLayoutMaxHeight;
            }
        }


        [TriInspector.Button]
        public void CollectRewardItemViews()
        {
            purchaseRewardItemViewList.Clear();
            purchaseRewardItemViewList.AddRange(GetComponentsInChildren<PurchaseRewardItemView>());
        }

        private void OnValidate()
        {
            rewardTypeOrder = rewardTypeOrder.Distinct().ToList();  
            var orderValues = (PurchaseRewardType[])Enum.GetValues(typeof(PurchaseRewardType));
            foreach (var item in orderValues)
                if (!rewardTypeOrder.Contains(item))
                    rewardTypeOrder.Add(item);
            SetHorizontalLayoutsMaxHeight();
        }
    }
}