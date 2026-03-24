using Infrastructure.Localization;
using TMPro;
using UnityEngine;

namespace Infrastructure.TooltipControl
{
    public class SuperSpeedTutorialTooltip : CustomTooltip
    {
        [SerializeField] private TextMeshProUGUI text;

        protected override void Setup()
        {
            text.text = LocalizationService.I.Get(LocKeys.Tooltips.SuperSpeedTutorialTooltip);
        }
    }
}