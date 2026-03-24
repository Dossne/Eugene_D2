using System;
using System.Collections.Generic;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Reward
{
    public class PurchaseRewardItemView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private List<GameObject> ribbon;
        [SerializeField] private GameObject infinityIcon;
        [SerializeField] private Button btn;
        [SerializeField] private RectTransform tooltipTarget;
        
        private Action<PurchaseRewardItemView> callback;
        private RewardContainerVisualData rewardData;
        public Sprite BackGroundSprite => background.sprite;
        public Sprite IconSprite => icon.sprite;
        public string LabelText => label.text;
        public bool IsDisplayRibbon => ribbon[0].activeSelf;
        public bool IsDisplayInfinityIcon => infinityIcon.activeSelf;
        public RectTransform TooltipTarget => tooltipTarget;
        public RewardContainerVisualData RewardData => rewardData;

        private void OnDestroy()
        {
            rewardData = null;
            callback = null;

            if (btn != null)
                btn.onClick.RemoveAllListeners();
        }

        public void Construct(RewardItemVisualData ds)
        {
            Construct(ds.icon, ds.amountText, ds.backgroundSprite, ds.isDisplayRibbon, ds.isDisplayInfinityIcon);
        }

        public void Construct(
            Sprite iconSprite,
            string text,
            Sprite backgroundSprite = null,
            bool isDisplayRibbon = false,
            bool isDisplayInfinityIcon = false)
        {
            label.gameObject.SetActive(!text.IsNullOrEmpty());
            if (!text.IsNullOrEmpty())
            {
                label.text = text;
            }

            icon.gameObject.SetActive(iconSprite != null);
            if (iconSprite != null)
                icon.sprite = iconSprite;

            background.gameObject.SetActive(backgroundSprite != null);
            if (backgroundSprite != null)
                background.sprite = backgroundSprite;

            for (int i = 0; i < ribbon.Count; i++)
            {
                ribbon[i].SetActive(isDisplayRibbon);
            }

            infinityIcon.SetActive(isDisplayInfinityIcon);
        }

        public void SetRewardData(RewardContainerVisualData rewardData, Action<PurchaseRewardItemView> callback)
        {
            var rewards = rewardData.haveContainer ? rewardData.containerRewards : rewardData.rewards;
            var icon = rewardData.haveContainer ? rewardData.containerIcon : rewards[0].icon;
            var text = rewardData.haveContainer ? string.Empty : rewards[0].amountText;
            var isDisplayRibbon = !rewardData.haveContainer       && rewards[0].isDisplayRibbon;
            var isDisplayInfinityIcon = !rewardData.haveContainer && rewards[0].isDisplayInfinityIcon;
            this.rewardData = rewardData;
            this.callback = callback;
            
            Construct(icon, text, null, isDisplayRibbon, isDisplayInfinityIcon);
            
            if (btn != null && rewardData is { haveContainer: true })
            {
                btn.gameObject.SetObjectActive(true);
                btn.onClick.AddListener(OnClick);
            }
        }

        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

        public void SetText(string text)
        {
            label.text = text;
        }

        private void OnClick()
        {
            callback?.Invoke(this);
        }
    }
}