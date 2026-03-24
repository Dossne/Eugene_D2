using System;
using Features.ScrollList;
using Infrastructure.Reward;
using Infrastructure.UiElementFx;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Competition
{
    public class CompetitionSlotView : ScrollElement<CompetitionSlotView>
    {
        [Header("Main")]
        [SerializeField] private Image rootImage;
        [SerializeField] private Image stateImage;
        [SerializeField] private PointButtonScale pointButton;

        [Header("Position")]
        [SerializeField] private Image positionIcon;
        [SerializeField] private TextMeshProUGUI positionTxt;

        [Header("Avatar")]
        [SerializeField] private Image avatarFrame;
        [SerializeField] private Image avatarIcon;
        [SerializeField] private TextMeshProUGUI nicknameTxt;

        [Header("Reward")]
        [SerializeField] private PurchaseRewardItemView rewardItem;

        [Header("State")]
        [SerializeField] private TextMeshProUGUI collectedCountTxt;

        [Header("Debug")]
        [SerializeField] private int idx;

        public int ViewIdx => idx;
        public Vector2 AnchoredPosCurrent => elementRect.anchoredPosition;

        public void SetCharacterData(Sprite icon, /*Sprite frameSprite,*/ string nickname)
        {
            avatarIcon.sprite = icon;
            //avatarFrame.sprite = frameSprite;
            nicknameTxt.text = nickname;
        }

        public void SetParentForRoot(RectTransform parent, bool worldPositionStays)
        {
            elementRect.SetParent(parent, worldPositionStays);
        }

        public void SetPositionText(string value)
        {
            positionTxt.text = value;
        }

        public void SetRootImage(Sprite value)
        {
            rootImage.sprite = value;
        }

        public void SetStateImage(Sprite value)
        {
            stateImage.sprite = value;
        }

        public void SetPositionIcon(Sprite value)
        {
            positionIcon.sprite = value;
        }

        public void SetPositionIconActive(bool value)
        {
            positionIcon.gameObject.SetObjectActive(value);
        }

        public void SetViewIndex(int value)
        {
            this.idx = value;
        }

        public void SetRewardItemActive(bool value)
        {
            rewardItem.SetObjectActive(value);
        }

        public void SetRewardData(RewardContainerVisualData rewardData, Action<PurchaseRewardItemView> callback)
        {
            rewardItem.SetRewardData(rewardData, callback);
        }

        public void SetSiblingIndex(int value)
        {
            elementRect.SetSiblingIndex(value);
        }

        public void SetScoreText(string value)
        {
            collectedCountTxt.text = value;
        }

        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

        public void SetButtonEnabled(bool value)
        {
            pointButton.SetEnabled(value);
        }

        protected override void InitializeImpl()
        {
            pointButton.Construct(HandleClick);
        }

        protected override void DeinitializeImpl()
        {
            OnClicked = null;
        }
    }
}