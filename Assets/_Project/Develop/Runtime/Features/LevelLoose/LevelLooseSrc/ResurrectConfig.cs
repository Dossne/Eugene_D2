using System;
using System.Collections.Generic;
using Infrastructure.WalletSystem;
using Infrastructure.Configuration;
using UnityEngine;
using Features.LevelComplete;
using System.Linq;

namespace Features.LevelLoose
{
    [Serializable]
    public class ResurrectData
    {
        public LoseReason loseReason;
        public LevelDifficulty levelDifficulty;
        public int loseCount;
        public CurrencyType currency;
        public int price;
        public bool rewardResurrectAllowed;
        public int coinsResurrectAddedSeconds;
        public int rewardResurrectAddedSeconds;
    }

    [Serializable]
    public class ResurrectDataConfig
    {
        public List<ResurrectData> datas;
    }

    [CreateAssetMenu(fileName = "ResurrectConfig", menuName = "Config/Game/ResurrectConfig")]
    public class ResurrectConfig : JsonConvertableConfig
    {

        [SerializeField] private ResurrectDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "ResurrectConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (ResurrectDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(ResurrectDataConfig);

        public List<ResurrectData> Items => configData.datas;

        public ResurrectData GetResurrectData(LoseReason loseReason, LevelDifficulty difficulty, int loseCount) 
        {
            ResurrectData resurrectData = configData.datas.Find(x => x.loseReason == loseReason
                                                                  && x.levelDifficulty == difficulty
                                                                  && x.loseCount == loseCount);
            return resurrectData;
        }

#if UNITY_EDITOR
        
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<ResurrectData>(tableData);
        }
#endif
    }
}