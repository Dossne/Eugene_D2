using Features.InfoPopup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LavaQuest
{
    public sealed class LavaQuestInfoPopup : InfoPopupBase
    {
        [SerializeField] private TextMeshProUGUI playersTxt;
        [SerializeField] private TextMeshProUGUI levelsTxt;
        [SerializeField] private Image rewardImage;
        [SerializeField] private TextMeshProUGUI rewardCountTxt;
        [SerializeField] private TextMeshProUGUI shareCoinsTxt;

        public override bool IsDisposeOnClosed => true;

        public void SetTexts(string playersTxt, string levelsTxt, Sprite rewardIcon, string rewardCountTxt, string shareCoinsTxt)
        {
            this.playersTxt.text = playersTxt;
            this.levelsTxt.text = levelsTxt;
            this.rewardCountTxt.text = rewardCountTxt;
            this.rewardImage.sprite = rewardIcon;
            this.shareCoinsTxt.text = shareCoinsTxt;
        }
    }
}