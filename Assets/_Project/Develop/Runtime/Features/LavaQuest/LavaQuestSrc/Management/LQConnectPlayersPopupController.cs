using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.PlayerProfile;
using Infrastructure.Configs;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.LavaQuest
{
    public class LQConnectPlayersPopupController
    {
        public event Action OnRequestClose;

        private readonly LavaQuestStateController stateController;
        private readonly PopupService popupService;
        private readonly LavaQuestConfig lavaQuestConfig;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly PlayerProfileController playerProfileController;
        private LavaQuestConnectPlayersPopup popup;
        private CancellationTokenSource cts;
        private bool isInit;


        public LQConnectPlayersPopupController(LavaQuestStateController stateController,
                                               PopupService popupService,
                                               ConfigProvider configProvider,
                                               SpriteAtlasService spriteAtlasService,
                                               PlayerProfileController playerProfileController)
        {
            this.stateController = stateController;
            this.popupService = popupService;
            this.lavaQuestConfig = configProvider.LavaQuestConfig;
            this.spriteAtlasService = spriteAtlasService;
            this.playerProfileController = playerProfileController;
        }


        public void Initialize()
        {
            if (isInit || !stateController.IsFeatureEnabled())
                return;

            cts = new CancellationTokenSource();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            cts.Cancel();
            cts.Dispose();

            if (popup != null)
                popup.OnTapToContinue -= LavaQuestConnectPlayersPopup_OnTapToContinue;

            popupService.Dispose(popup);

            popup = null;
            isInit = false;
        }


        public async UniTask OpenConnectPlayersInfoPopupAsync()
        {
            if (popup == null)
            {
                popup = await popupService.GetAsync<LavaQuestConnectPlayersPopup>(cts.Token, false, false);
                popup.OnTapToContinue += LavaQuestConnectPlayersPopup_OnTapToContinue;
                popup.Initialize();
            }

            Sprite playerIcon = null;
            Sprite playerBack = null;
            List<(Sprite icon, Sprite back)> enemyIcons = new List<(Sprite icon, Sprite back)>();

            foreach (var iconData in lavaQuestConfig.Icons)
            {
                var spriteIcon = spriteAtlasService.GetFromMain(iconData.name);
                var spriteBack = spriteAtlasService.GetFromMain(iconData.back);

                if (iconData.enemy)
                {
                    enemyIcons.Add((spriteIcon, spriteBack));
                }
                else
                {
                    if (playerProfileController.FeatureEnabled)
                        playerIcon = spriteAtlasService.GetFromMain(playerProfileController.PlayerProfileData.avatarId);
                    else
                        playerIcon = spriteIcon;
                    playerBack = spriteAtlasService.GetFromMain(iconData.back);
                }                    
            }

            GameplayUtils.ShuffleElements(enemyIcons);
            
            Sprite rewardIcon = spriteAtlasService.GetFromMain(stateController.BillboardRewardIcon);
            popup.Construct((playerIcon, playerBack), enemyIcons, rewardIcon, stateController.CurrentRewardVisualInfo.amountText);
            popup.Open(); 
        }

        public void ClosePopup()
        {
            popup.Close();
        }

        private void LavaQuestConnectPlayersPopup_OnTapToContinue()
        {
            OnRequestClose?.Invoke();
        }
    }
}