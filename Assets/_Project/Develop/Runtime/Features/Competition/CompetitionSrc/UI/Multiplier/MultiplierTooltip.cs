using Infrastructure.Localization;
using Infrastructure.TooltipControl;
using TMPro;
using UnityEngine;

namespace Features.Competition
{
    public sealed class MultiplierTooltip : Tooltip
    {
        [Header("Multiplier")]
        [SerializeField] private TextMeshProUGUI bottomText;

        public void Show()
        {
            string topText = LocalizationService.I.Get(LocKeys.Competition.MultiTooltip1);
            bottomText.text = LocalizationService.I.Get(LocKeys.Competition.MultiTooltip2);
            Show(topText);
        }
    }
}