using Infrastructure.Localization;
using TMPro;
using UnityEngine;

namespace Infrastructure.TooltipControl
{
    public class FinishLevelTutorialTooltip : CustomTooltip
    {
        [SerializeField] private TextMeshProUGUI text;

        protected override void Setup()
        {
            text.text = LocalizationService.I.Get(LocKeys.Tooltips.FinishLevelTutorialTooltip);
        }
    }
}