using Features.PlayerProfile;
using Infrastructure.BroTweens;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Features.Tutorial
{
    public class PlayerProfileAvatarPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;

        [SerializeField] private PlayerProfileAvatarList avatarList;
        [SerializeField] private PlayerProfileAvatarListElement avatarListElementPf;

        [SerializeField] private Button saveBtn;

        private SpriteAtlasService spriteAtlasService;
        private ConfigProvider configProvider;
        private Action<string> saveButtonCallback;
        private string currentAvatarId = string.Empty;
        private BroTweenSafe bounceTween;


        public void Construct(SpriteAtlasService spriteAtlasSevice, 
                              ConfigProvider configProvider, 
                              Action<string> saveButtonCallback) 
        {
            this.spriteAtlasService = spriteAtlasSevice;
            this.configProvider = configProvider;
            this.saveButtonCallback = saveButtonCallback;
            headerTxt.text = LocalizationService.I.Get(LocKeys.PlayerProfileAvatarPopup.Header);
            buttonTxt.text = LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.SaveButton);
        }


        public void RefreshData(string avatarId)
        {
            SetAvatar(avatarId);
            SaveAvatar(avatarId);
        }

        protected override void AnimationStartHandler() => avatarList.SetScrollEnabled(false);


        protected override void AnimationStopHandler() => avatarList.SetScrollEnabled(true);


        protected override void OnInitialize()
        {
            var config = configProvider.PlayerProfileConfiguration;

            avatarList.Clear();
            avatarList.OnElementClicked = null;
            avatarList.OnElementClicked += AvatarList_OnElementClicked;

            var avatarDataList = config.Avatars.Where(x => x.avatarId != config.Feature.defaultAvatar).ToList();
            for (int i = 0; i < avatarDataList.Count; i++)
            {
                var avatar = Instantiate(avatarListElementPf, Vector3.zero, Quaternion.identity);
                avatar.Construct((avatarDataList[i].avatarId, false, false), spriteAtlasService, configProvider);
                avatarList.AddElement(avatar);
            }

            saveBtn.onClick.AddListener(SaveButtonClick);
            saveBtn.interactable = false;
        }


        protected override void OnDeinitialize()
        {
            saveBtn.onClick.RemoveListener(SaveButtonClick);
        }

        private void SaveButtonClick()
        {
            BroTween.ClickBounceWithCallBack(saveBtn, saveBtn.transform, SaveButtonClickCallback)
                    .Play();
        }


        private void SaveButtonClickCallback()
        {
            saveButtonCallback?.Invoke(currentAvatarId);
        }


        private void AvatarList_OnElementClicked((string id, bool isSelected, bool isSaved) avatarData, RectTransform rectTransform)
        {
            saveBtn.interactable = true;
            SetAvatar(avatarData.id);

            bounceTween = BroTween.Bounce(rectTransform).SetAutoKill(true).SetUpdate(true).ToSafe();
            bounceTween.Play();
        }

        private void SetAvatar(string avatarId)
        {
            currentAvatarId = avatarId;
            avatarList.SelectAvatar(avatarId);
        }

        private void SaveAvatar(string avatarId)
        {
            avatarList.SaveAvatar(avatarId);
        }
    }
}