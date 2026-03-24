using Infrastructure.Localization;
using TMPro;
using UnityEngine;

namespace Features.Boosters
{
    public class RewardMultiplierView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI multiplierTxt;


        public void Construct(int multiplier, bool isActive)
        {
            SetObjectActive(isActive);

            if(!isActive)
                return;
            
            headerTxt.text = LocalizationService.I.Get(LocKeys.Boosters.PopupReward);
            multiplierTxt.text = $"x{multiplier.ToString()}";

        }


        private void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }
    }
}