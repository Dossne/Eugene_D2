using System;
using Features.PurchaseUi;
using Infrastructure.BroTweens;
using Infrastructure.HapticControl;
using Infrastructure.Reward;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SeasonPass
{
    public class SeasonPassSlotView : MonoBehaviour
    {
        public event Action<SlotViewData> OnClicked;
        [SerializeField] private Button slotBtn;
        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [SerializeField] private Transform tooltipTarget;
        
        private SlotViewData viewData;
        private BroTweenSafe clickTween;
        private bool isInit;

        public void Construct(SlotViewData viewData)
        {
            this.viewData = viewData;
            var rewardVisualData = viewData.rewardInfo;
            rewardItemView.Construct(rewardVisualData.icon, rewardVisualData.amountText, null, rewardVisualData.isDisplayRibbon, rewardVisualData.isDisplayInfinityIcon);

            viewData.tooltipTarget = tooltipTarget;
        }

        public void Initialize()
        {
            if (isInit)
                return;

            slotBtn.onClick.AddListener(Click);
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            clickTween.Kill();
            slotBtn.onClick.RemoveListener(Click);

            isInit = false;
        }

        public void Destroy()
        {
            OnClicked = null;
        }

        private void Click()
        {
            HapticService.I.HapticSelection();

            clickTween = BroTween.ClickBounceWithCallBack(slotBtn, rewardItemView.transform, () => OnClicked?.Invoke(viewData)).ToSafe();
            clickTween.Play();
        }
    }
}