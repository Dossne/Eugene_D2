using Infrastructure.Localization;
using Infrastructure.Reward;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Competition
{
    public sealed class CompetitionFinishPopup : CompetitionStatePopup
    {
        [TriInspector.Title("Finish State")]
        [Header("Rank")]
        [SerializeField] private TextMeshProUGUI rankHeaderTxt;
        [SerializeField] private TextMeshProUGUI rankTxt;
        [SerializeField] private string rankPrefix = "#";
        [Header("Reward")]
        [SerializeField] private TextMeshProUGUI rewardHeaderTxt;
        [SerializeField] private TextMeshProUGUI noRewardTxt;
        [SerializeField] private string noRewardTextValue = "—";
        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [Header("Continue")]
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI continueBtnTxt;

        public void SetRankText(int leaderboardPosition)
        {
            rankTxt.text = $"{rankPrefix}{leaderboardPosition.ToString()}";
        }

        public void SetResultReward(bool have, RewardContainerVisualData rewardInfo)
        {
            rewardItemView.SetObjectActive(have);
            noRewardTxt.gameObject.SetObjectActive(!have);

            if (have)
            {
                rewardItemView.SetRewardData(rewardInfo, ShowRewardContainerToolTip);
            }
            else
            {
                noRewardTxt.text = noRewardTextValue;
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            rankHeaderTxt.text = LocalizationService.I.Get(LocKeys.Competition.RankHeader);
            rewardHeaderTxt.text = LocalizationService.I.Get(LocKeys.Competition.RewardHeader);
            continueBtnTxt.text = LocalizationService.I.Get(LocKeys.Competition.Continue);
        }
    }
}