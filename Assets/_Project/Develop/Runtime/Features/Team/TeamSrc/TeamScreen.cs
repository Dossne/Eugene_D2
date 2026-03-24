using Infrastructure.Popups;
using UnityEngine;
using Infrastructure.Localization;
using TMPro;

namespace Features.Team
{
    public class TeamScreen : PopupBase
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI comingSoonText;

        protected override void OnInitialize() 
        {
            headerText.text     = LocalizationService.I.Get(LocKeys.Team.TeamsText);
            comingSoonText.text = LocalizationService.I.Get(LocKeys.Common.ComingSoon);
        }
    }
}
