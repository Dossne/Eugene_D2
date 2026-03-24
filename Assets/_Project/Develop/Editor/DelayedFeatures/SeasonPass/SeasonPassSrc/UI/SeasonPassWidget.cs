using Features.ProgressBar;
using Features.Widgets;
using Infrastructure.TooltipControl;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SeasonPass
{
    public sealed class SeasonPassWidget : Widget
    {
        [SerializeField] private NormalizedProgressBarImage bar;

        [Header("LockState")]
        [SerializeField] private GameObject lockObject;
        [SerializeField] private Color lockColor;
        [SerializeField] private Color unlockColor;
        [SerializeField] private Image[] elementsForLockColor;
        [SerializeField] private Tooltip tooltip;


        public void SetUnlockedState(bool isUnlocked)
        {
            lockObject.SetObjectActive(!isUnlocked);

            foreach (Image elementForLockColor in elementsForLockColor)
            {
                elementForLockColor.color = isUnlocked ? unlockColor : lockColor;
            }
        }


        public void SetProgress(float progress01)
        {
            bar.SetProgress(progress01);
        }


        public void ShowTooltip(string text)
        {
            tooltip.Show(text);
        }


        protected override void OnInitialize()
        {
            tooltip.Initialize();
        }


        protected override void OnDeinitialize()
        {
            tooltip.Deinitialize();
            SetObjectActive(false);
        }
    }
}