using Infrastructure.Localization;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;


namespace Features.FreePaidOffer
{
    public class FreePaidOfferBoughtPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI buttonText;



        public void Construct()
        {
            headerText.text = LocalizationService.I.Get(FreePaidOfferLocalization.boughtPopupHeader);
            descriptionText.text = LocalizationService.I.Get(FreePaidOfferLocalization.boughtPopupDescription);
            buttonText.text = LocalizationService.I.Get(FreePaidOfferLocalization.boughtPopupButton);
        }
    }
}
