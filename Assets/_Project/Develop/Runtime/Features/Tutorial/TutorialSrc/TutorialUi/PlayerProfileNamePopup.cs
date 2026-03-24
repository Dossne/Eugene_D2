using AYellowpaper.SerializedCollections;
using Features.PlayerProfile;
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


namespace Features.Tutorial
{
    public class PlayerProfileNamePopup : PopupBase
    {        
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI changeNameTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;
        [SerializeField] private TMP_InputField playerNameEdit;

        [SerializeField] private Button saveBtn;

        private ConfigProvider configProvider;
        private Action<string> saveButtonCallback;
        private Action OnWrongSymbolInput;
        private string currentName = string.Empty;
        

        public void Construct(ConfigProvider configProvider, 
                              Action<string> saveButtonCallback,
                              Action wrongSymbolInputCallback) 
        {
            this.configProvider = configProvider;
            this.saveButtonCallback = saveButtonCallback;
            this.OnWrongSymbolInput = wrongSymbolInputCallback;
            headerTxt.text     = LocalizationService.I.Get(LocKeys.PlayerProfileNamePopup.Header);
            buttonTxt.text     = LocalizationService.I.Get(LocKeys.PlayerProfileNamePopup.SaveButton);
            changeNameTxt.text = LocalizationService.I.Get(LocKeys.PlayerProfileNamePopup.Change);
        }


        public void RefreshData(string displayName)
        {
            SetName(displayName);
        }


        protected override void OnInitialize()
        {
            var config = configProvider.PlayerProfileConfiguration;

            saveBtn.onClick.AddListener(SaveButtonClick);

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
        }


        private void SaveButtonClick()
        {
            BroTween.ClickBounceWithCallBack(saveBtn, saveBtn.transform, SaveButtonClickCallback)
                    .Play();
        }


        private void SaveButtonClickCallback()
        {
            SetName(playerNameEdit.text);
            saveButtonCallback?.Invoke(currentName);
        }


        private void SetName(string name)
        {
            var normalizedName = Regex.Replace(name.Trim(), @"\s+", " ");
            currentName = normalizedName;
            playerNameEdit.text = normalizedName;
        }
    }
}