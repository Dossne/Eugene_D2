using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Infrastructure.Settings
{
    [Serializable]
    public class SettingsData
    {
        [Header("Settings")]
        public bool haveMusic;
        public bool haveSound;
        public bool haveHaptic;
        public bool haveJoystickFollow;

        [Header("Graphics")]
        public bool haveGraphicSettings;
        public float lowResolutionScale = 0.5f;
        public float highResolutionScale = 0.66f;
        [Tooltip("Min value for android devices exclusive. Checks of 1st launch")]
        public int lowGraphicsMemorySize = 512;
        [Tooltip("Min value for android devices exclusive. Checks of 1st launch")]
        public int lowSystemMemorySize = 4096;

        [Header("Fps")]
        public bool haveFpsSettings;
        public int lowFps = 30;
        public int highFps = 60;

        
        [Header("Localization")]
        public bool isUsingSayKit;

        [Header("Gameplay")]
        public float gravity = 9.81f;
        public float holeSpeedMultiplier = 1;

        public bool haveFirstLoadProlongation = false;

        [Header("LevelStorage")]
        public bool useLevelFiles = false;
    }

    [Serializable]
    public class SettingsDataConfig
    {
        public List<SettingsData> datas;
    }

    [CreateAssetMenu(fileName = "SystemSettingsConfig", menuName = "Config/System/SystemSettingsConfig")]
    public class SystemSettingsConfig : JsonConvertableConfig
    {

        [SerializeField] private SettingsDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "SystemSettingsConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (SettingsDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(SettingsDataConfig);

        public List<SettingsData> Items => configData.datas;
        public SettingsData SettingsData => configData.datas[0];

#if UNITY_EDITOR


        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<SettingsData>(tableData);
        }
#endif
    }
}