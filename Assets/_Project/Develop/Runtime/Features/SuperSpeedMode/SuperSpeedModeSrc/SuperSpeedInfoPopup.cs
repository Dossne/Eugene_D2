using Features.InfoPopup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SuperSpeedMode
{
    public sealed class SuperSpeedInfoPopup : InfoPopupBase
    {
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI levelsTxt;
        [SerializeField] private TextMeshProUGUI activateTxt;
        [SerializeField] private TextMeshProUGUI boostTxt;


        public void SetTexts(string headerTxt, string levelsTxt, string activateTxt, string boostTxt)
        {
            this.headerTxt.text = headerTxt;
            this.levelsTxt.text = levelsTxt;
            this.activateTxt.text = activateTxt;
            this.boostTxt.text = boostTxt;
        }
    }
}