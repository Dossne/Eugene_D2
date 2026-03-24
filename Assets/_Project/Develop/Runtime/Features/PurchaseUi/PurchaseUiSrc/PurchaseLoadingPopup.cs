using Infrastructure.Popups;
using System;
using TMPro;
using UnityEngine;

namespace Features.PurchaseUi
{
    public class PurchaseLoadingPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI descriptionText;

        public void Construct(string text) 
        {
            descriptionText.text = text;
        }
    }
}
