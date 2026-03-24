using Infrastructure.Localization;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;

namespace Features.SeasonPass
{
    public class SeasonPassScrollLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI freeText;
        [SerializeField] private TextMeshProUGUI premiumText;

        public void Initialize()
        {
            freeText.text    = LocalizationService.I.Get(LocKeys.SeasonPass.Free);
            premiumText.text = LocalizationService.I.Get(LocKeys.SeasonPass.Prem);

            SetObjectActive(true);
        }

        private void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }
    }
}