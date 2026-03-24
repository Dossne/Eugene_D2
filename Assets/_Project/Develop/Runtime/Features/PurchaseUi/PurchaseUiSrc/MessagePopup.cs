using Infrastructure.Popups;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Features.PurchaseUi
{
    public class MessagePopup : PopupBase
    {
        [SerializeField] TextMeshProUGUI headerTmp;
        [SerializeField] TextMeshProUGUI descriptionTmp;
        [SerializeField] TextMeshProUGUI buttonOkTmp;

        public void Construct(string headerText, string descriptionText, string buttonText)
        {
            this.headerTmp.text = headerText;
            this.descriptionTmp.text = descriptionText;
            this.buttonOkTmp.text = buttonText;
        }
    }
}
