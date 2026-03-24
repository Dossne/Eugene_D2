using Infrastructure.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Features.Social
{

    [Serializable]
    public class SocialFeatureData
    {
        public bool isFeatureEnabled;
        public int unlockLevel;
    }


    [Serializable]
    public class LeaderboardData
    {
        public float topUpdateCooldownSec = 60.0f;
    }


    [Serializable]
    public class SocialDataConfig
    {
        public List<SocialFeatureData> feature;
        public List<LeaderboardData> leaderboard;
    }


    [CreateAssetMenu(fileName = "SocialConfig", menuName = "Config/Game/SocialConfig")]
    public class SocialConfig : JsonConvertableConfig
    {
        [SerializeField] private SocialDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "SocialConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (SocialDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(SocialDataConfig);


        public SocialFeatureData Feature => configData.feature[0];
        public LeaderboardData Leaderboard => configData.leaderboard[0];



#if UNITY_EDITOR

        public override GoogleDocsUtils.MajorDimension SpreadSheetDimension => GoogleDocsUtils.MajorDimension.COLUMNS;

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.feature, nameof(Feature), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.leaderboard, nameof(Leaderboard), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.feature = GoogleDocsUtils.SnatchDataRowsToConfig<SocialFeatureData>(tableData);
            configData.leaderboard = GoogleDocsUtils.SnatchDataRowsToConfig<LeaderboardData>(tableData);
        }
#endif
    }
}

