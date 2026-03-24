using Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Features.PlayerProfile
{
    [Serializable]
    public class PlayerProfileFeatureData
    {
        public bool isEnabled;
        public string defaultAvatar;
        public string defaultName;
        public bool isAvatarTabEnabled;
        public bool isFrameTabEnabled;
        public bool isNameTabEnabled;
        public bool isTokenTabEnabled;
        public int tutorUnlockLevel;
    }

    [Serializable]
    public class PlayerProfileAvatarData
    {
        public string avatarId;
    }

    [Serializable]
    public class PlayerProfileConfigurationData
    {
        public List<PlayerProfileFeatureData> featureData;
        public List<PlayerProfileAvatarData> avatarData;
    }

    [CreateAssetMenu(fileName = "PlayerProfileConfiguration", menuName = "Config/Game/PlayerProfileConfiguration")]
    public class PlayerProfileConfiguration : JsonConvertableConfig
    {
        [SerializeField] private PlayerProfileConfigurationData configData;

        public PlayerProfileFeatureData Feature => configData.featureData[0];
        public List<PlayerProfileAvatarData> Avatars => configData.avatarData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "PlayerProfileConfiguration";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (PlayerProfileConfigurationData)value;
        }

        protected override Type ConfigurationDataType => typeof(PlayerProfileConfigurationData);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            Validate();
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.featureData, nameof(Feature), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.avatarData,  nameof(Avatars), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {            
            configData.featureData = GoogleDocsUtils.SnatchDataRowsToConfig<PlayerProfileFeatureData>(tableData);
            configData.avatarData  = GoogleDocsUtils.SnatchDataRowsToConfig<PlayerProfileAvatarData>(tableData);
            Validate();
        }

        private void Validate()
        {
            if (!Avatars.Exists(x => x.avatarId == Feature.defaultAvatar))
                Avatars.Add(new() { avatarId = Feature.defaultAvatar });
        }
#endif
    }
}