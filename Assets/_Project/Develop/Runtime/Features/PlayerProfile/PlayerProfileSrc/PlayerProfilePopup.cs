using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using System;
using System.Collections.Generic;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Features.PlayerProfile
{
    public class PlayerProfilePopup : PopupBase
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI levelLabelText;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI statHeaderText;
        [SerializeField] private Button editBtn;
        [SerializeField] private Transform avatarButtonTransform;
        [SerializeField] private List<PlayerProfileStatSlot> playerProfileStatSlots = new();
        [SerializeField] private string firstWinIconId;
        [SerializeField] private string totalWinIconId;
        [SerializeField] private string winStreakIconId;
        [SerializeField] private GameObject editBtnVisual;
        
        private SpriteAtlasService spriteAtlasService;

        private Action editButtonCallback;

        public void Construct(SpriteAtlasService spriteAtlasService, Action editButtonCallback)
        {
            this.spriteAtlasService = spriteAtlasService;
            this.editButtonCallback = editButtonCallback;
        }

        public void SetEditEnabled(bool enabled)
        {
            editBtn.interactable = enabled;
            editBtnVisual.SetObjectActive(enabled);
        }
        
        protected override void OnInitialize()
        {
            statHeaderText.text = LocalizationService.I.Get(LocKeys.PlayerProfilePopup.StatHeader);
            levelLabelText.text = LocalizationService.I.Get(LocKeys.Common.Level);
            editBtn.onClick.AddListener(ButtonClick);
        }


        protected override void OnDeinitialize()
        {
            editBtn.onClick.RemoveListener(ButtonClick);
        }

        
        public void RefreshData(Sprite avatar, string name, int level, int firstWinCount, int totalWinCount, int maxWinStreak)
        {
            avatarImage.sprite = avatar;
            levelNumberText.text = level.ToString();
            playerNameText.text = name;

            playerProfileStatSlots[0].Setup(spriteAtlasService.GetFromMain(firstWinIconId),  LocalizationService.I.Get(LocKeys.PlayerProfilePopup.StatSlotFirstWin ), firstWinCount.ToString());
            playerProfileStatSlots[1].Setup(spriteAtlasService.GetFromMain(totalWinIconId),  LocalizationService.I.Get(LocKeys.PlayerProfilePopup.StatSlotTotalWin ), totalWinCount.ToString());
            playerProfileStatSlots[2].Setup(spriteAtlasService.GetFromMain(winStreakIconId), LocalizationService.I.Get(LocKeys.PlayerProfilePopup.StatSlotWinStreak), maxWinStreak.ToString());
        }

        private void ButtonClick()
        {
            BroTween.ClickBounceWithCallBack(editBtn, avatarButtonTransform, ClickCallback)
                    .Play();
        }

        private void ClickCallback()
        {
            editButtonCallback?.Invoke();
        }
    }
}