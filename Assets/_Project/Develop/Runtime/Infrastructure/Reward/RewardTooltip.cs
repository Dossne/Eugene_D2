using System.Collections;
using System.Collections.Generic;
using Infrastructure.TooltipControl;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Reward
{
    public class RewardTooltip : Tooltip
    {
        [Header("Layouts")]
        [SerializeField] private RectTransform rootRectTransform;
        [SerializeField] private List<MonoBehaviour> layouts;
        [Header("Content")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform delimeterPf;
        [SerializeField] private PurchaseRewardItemView rewardItemPf;
        [Header("Other")]
        [SerializeField] private int maxRewardsInRow = 3;
        [Header("Debug")]
        [SerializeField] private List<PurchaseRewardItemView> rewardItems;
        [SerializeField] private List<RectTransform> delimeters;

        public void Show(RewardContainerVisualData rewardcontainerVisualData, Vector2 position)
        {
            var itemsToShow = new List<RewardItemVisualData>();

            if (rewardcontainerVisualData.rewards != null)
                itemsToShow.AddRange(rewardcontainerVisualData.rewards);

            if (rewardcontainerVisualData.containerRewards != null)
                itemsToShow.AddRange(rewardcontainerVisualData.containerRewards);

            if (itemsToShow.Count == 0)
            {
                Debug.LogWarning("Reward container rewards are empty");
                return;
            }

            SetElementsDisabled();
            int delimIdx = 0;
            int currentRewardInRow = 0;
            int siblingIdx = 0;
            for (var i = 0; i < itemsToShow.Count; i++)
            {
                var visualInfo = itemsToShow[i];

                if (rewardItems.Count <= i)
                {
                    rewardItems.Add(Instantiate(rewardItemPf, contentRoot));
                }

                rewardItems[i].Construct(visualInfo.icon, visualInfo.amountText, null, visualInfo.isDisplayRibbon, visualInfo.isDisplayInfinityIcon);
                (rewardItems[i].transform as RectTransform)?.SetSiblingIndex(siblingIdx);
                siblingIdx++;

                rewardItems[i].SetObjectActive(true);
                currentRewardInRow++;

                if (currentRewardInRow == maxRewardsInRow)
                {
                    currentRewardInRow = 0;
                    continue;
                }

                if (delimeters.Count <= delimIdx)
                {
                    delimeters.Add(Instantiate(delimeterPf, contentRoot));
                }

                if (i != itemsToShow.Count - 1)
                {
                    delimeters[delimIdx].SetSiblingIndex(siblingIdx);
                    siblingIdx++;
                    delimeters[delimIdx].gameObject.SetObjectActive(true);
                    delimIdx++;
                }
            }

            rootTransform.position = position;
            foreach (var layout in layouts)
            {
                layout.enabled = true;
            }

            openCloseAnimator.Open();
            StartCoroutine(ForceRecalculateLayoutsRoutine());
        }
        
        private void SetElementsDisabled()
        {
            foreach (var rewardItem in rewardItems)
                rewardItem.SetObjectActive(false);

            foreach (var delimeter in delimeters)
                delimeter.gameObject.SetObjectActive(false);
        }

        private IEnumerator ForceRecalculateLayoutsRoutine()
        {
            SetLayoutsEnabled(true);
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRectTransform);
            SetLayoutsEnabled(false);
        }

        private void SetLayoutsEnabled(bool isEnabled)
        {
            foreach (var layout in layouts)
            {
                layout.enabled = isEnabled;
            }
        }
    }
}