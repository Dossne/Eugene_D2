using System.Collections.Generic;
using Features.InfoPopup;
using Infrastructure.Reward;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LavaQuest
{

    public sealed class LavaQuestCongratulationPopup : InfoPopupBase
    {
        [SerializeField] private TextMeshProUGUI rewardCountOnBoardTxt;
        [SerializeField] private TextMeshProUGUI sharingTxt;
        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [SerializeField] private Image rewardImageBillboard;

        
        [SerializeField] private IPChangeTextByIntBhvr rewardChangeAnimator;
        [SerializeField] private List<CharacterIcon> icons;

        public override bool IsDisposeOnClosed => true;


        public void Construct(
            List<(Sprite icon, Sprite back)> characterSprites,
            Sprite rewardIconSprite,
            Sprite rewardIconSpriteBillboard,
            int givenCount,
            string rewardTotalCountTxt,
            bool isDisplayRibbon,
            bool isDisplayInfinityIcon,
            string sharingTxt)
        {
            for (var i = 0; i < icons.Count; i++)
            {
                var icon = icons[i];
                icon.SetIcon(characterSprites[i].icon, characterSprites[i].back);
            }

            this.rewardCountOnBoardTxt.text = rewardTotalCountTxt;
            rewardChangeAnimator.Construct(0, givenCount);
            rewardItemView.Construct(rewardIconSprite, 0.ToString(), null, isDisplayRibbon, isDisplayInfinityIcon);
            rewardImageBillboard.sprite = rewardIconSpriteBillboard;
            this.sharingTxt.text = sharingTxt;
        }

    }
}