using System;
using System.Collections.Generic;
using Features.InfoPopup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LavaQuest
{
    public sealed class LavaQuestConnectPlayersPopup : InfoPopupBase
    {
        public event Action OnTapToContinue;

        [SerializeField] private Image rewardImage;
        [SerializeField] private TextMeshProUGUI rewardCountTxt;
        [SerializeField] private CharacterIcon playerIcon;
        [SerializeField] private List<CharacterIcon> enemyIcons;
        [SerializeField] private Button tapToContinueButton;

        public override bool IsDisposeOnClosed => true;


        public void Construct((Sprite icon, Sprite back) playerIcon, List<(Sprite icon, Sprite back)> enemiesIcons, Sprite rewardIcon, string rewardCount)
        {
            SetIconSprites(playerIcon, enemiesIcons);
            rewardCountTxt.text = rewardCount;
            rewardImage.sprite = rewardIcon;
        }


        protected override void OnInitialize()
        {
            base.OnInitialize();
            tapToContinueButton.onClick.AddListener(CloseRequest);
        }


        protected override void OnDeinitialize()
        {
            base.OnDeinitialize();
            tapToContinueButton.onClick.RemoveListener(CloseRequest);
            OnTapToContinue = null;
        }


        private void SetIconSprites((Sprite icon, Sprite back) player, List<(Sprite icon, Sprite back)> enemies)
        {
            playerIcon.SetIcon(player.icon, player.back);

            int min = Mathf.Min(enemies.Count, enemyIcons.Count);

            for (var i = 0; i < min; i++)
            {
                enemyIcons[i].SetIcon(enemies[i].icon, enemies[i].back);
            }
        }


        private void CloseRequest()
        {
            OnTapToContinue?.Invoke();
        }
    }
}