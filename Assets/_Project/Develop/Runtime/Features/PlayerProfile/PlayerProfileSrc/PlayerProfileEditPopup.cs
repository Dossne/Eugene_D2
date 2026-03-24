using AYellowpaper.SerializedCollections;
using Infrastructure.BroTweens;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.TooltipControl;
using Infrastructure.Utilities;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace Features.PlayerProfile
{
    public class PlayerProfileEditPopup : PopupBase
    {
        private enum Tab 
        {
            Avatar = 0,
            Frame  = 1,
            Name   = 2,
            Token  = 3,           
        }

        [Serializable]
        private class TabData 
        {
            public Image image;
            public Button button;
            public GameObject content;
            public TextMeshProUGUI tabText;
        }

        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;

        [SerializeField] private Image avatarImage;
        [SerializeField] private TMP_InputField playerNameEdit;
        [SerializeField] private PlayerProfileAvatarList avatarList;
        [SerializeField] private PlayerProfileAvatarListElement avatarListElementPf;

        [SerializeField] private Button saveBtn;

        [Header("Tabs")]
        [SerializedDictionary][SerializeField] private SerializedDictionary<Tab, TabData> tabs = new();
        [SerializeField] private Sprite pressedTab;
        [SerializeField] private Sprite unpressedTab;


        private SpriteAtlasService spriteAtlasService;
        private ConfigProvider configProvider;
        private Action<PlayerProfileData> saveButtonCallback;
        private Action OnWrongSymbolInput;
        private PlayerProfileData currentData = new();
        private BroTweenSafe bounceTween;


        public void Construct(SpriteAtlasService spriteAtlasSevice, 
                              ConfigProvider configProvider, 
                              Action<PlayerProfileData> saveButtonCallback,
                              Action wrongSymbolInputCallback) 
        {
            this.spriteAtlasService = spriteAtlasSevice;
            this.configProvider = configProvider;
            this.saveButtonCallback = saveButtonCallback;
            this.OnWrongSymbolInput = wrongSymbolInputCallback;
            headerTxt.text = LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.Header);
            buttonTxt.text = LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.SaveButton);
        }


        public void RefreshData(PlayerProfileData playerProfileData)
        {
            SetAvatar(playerProfileData.avatarId);
            SaveAvatar(playerProfileData.avatarId);
            SetName(playerProfileData.displayName);
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

            InitializeTab(Tab.Avatar, LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.TabAvatar), AvatarTabClick, config.Feature.isAvatarTabEnabled);
            InitializeTab(Tab.Frame , LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.TabFrame ), FrameTabClick , config.Feature.isFrameTabEnabled );
            InitializeTab(Tab.Name  , LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.TabName  ), NameTabClick  , config.Feature.isNameTabEnabled  );
            InitializeTab(Tab.Token , LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.TabToken ), TokenTabClick , config.Feature.isTokenTabEnabled );

            if (config.Feature.isAvatarTabEnabled)
               HandleTabClick(Tab.Avatar);

            playerNameEdit.onValidateInput = NameValidator;
        }

        private char NameValidator(string text, int charIndex, char addedChar)
        {
            if (Regex.IsMatch(addedChar.ToString(), "^[A-Za-z0-9 ]$"))
                return addedChar;

            OnWrongSymbolInput?.Invoke();
            return '\0';
        }

        protected override void OnDeinitialize()
        {
            playerNameEdit.onValidateInput -= NameValidator;

            saveBtn.onClick.RemoveListener(SaveButtonClick);

            DeinitializeTab(Tab.Avatar, AvatarTabClick);
            DeinitializeTab(Tab.Frame , FrameTabClick );
            DeinitializeTab(Tab.Name  , NameTabClick  );
            DeinitializeTab(Tab.Token , TokenTabClick );
        }

        private void InitializeTab(Tab tab, string tabName, UnityAction callback, bool isEnabled) 
        {
            tabs[tab].button.gameObject.SetObjectActive(isEnabled);
            if (tabs[tab].content != null)
                tabs[tab].content.SetObjectActive(isEnabled);

            if (!isEnabled)
                return;

            tabs[tab].tabText.text = tabName;
            tabs[tab].button.onClick.AddListener(callback);
        }

        private void DeinitializeTab(Tab tab, UnityAction callback)
        {
            tabs[tab].button.onClick.RemoveListener(callback);
        }

        private void SaveButtonClick()
        {
            BroTween.ClickBounceWithCallBack(saveBtn, saveBtn.transform, SaveButtonClickCallback)
                    .Play();
        }


        private void SaveButtonClickCallback()
        {
            SetName(playerNameEdit.text);
            saveButtonCallback?.Invoke(new() { avatarId = currentData.avatarId, displayName = currentData.displayName });
        }


        private void AvatarList_OnElementClicked((string id, bool isSelected, bool isSaved) avatarData, RectTransform rectTransform)
        {
            SetAvatar(avatarData.id);
            bounceTween = BroTween.Bounce(rectTransform).SetAutoKill(true).SetUpdate(true).ToSafe();
            bounceTween.Play();
        }

        private void SetAvatar(string avatarId)
        {
            currentData.avatarId = avatarId;
            avatarList.SelectAvatar(avatarId);
            avatarImage.sprite = spriteAtlasService.GetFromMain(currentData.avatarId);
        }

        private void SaveAvatar(string avatarId)
        {
            avatarList.SaveAvatar(avatarId);
        }

        private void SetName(string name)
        {
            var normalizedName = Regex.Replace(name.Trim(), @"\s+", " ");
            currentData.displayName = normalizedName;
            playerNameEdit.text = normalizedName;
        }

        private void AvatarTabClick()
        {
            HandleTabClick(Tab.Avatar);
        }

        private void FrameTabClick()
        {
            HandleTabClick(Tab.Frame);
        }

        private void NameTabClick()
        {
            HandleTabClick(Tab.Name);
        }

        private void TokenTabClick()
        {
            HandleTabClick(Tab.Token);
        }

        private void HandleTabClick(Tab tab)
        {
            foreach (var item in tabs)
            {
                var isPressedTab = item.Key == tab;
                item.Value.image.sprite = isPressedTab ? pressedTab : unpressedTab;
                if (item.Value.content != null)
                    item.Value.content.SetObjectActive(isPressedTab);
            }
        }        
    }
}