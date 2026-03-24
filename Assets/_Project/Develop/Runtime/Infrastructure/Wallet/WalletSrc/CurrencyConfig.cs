using Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.WalletSystem
{
    [Serializable]
    public class CurrencyData
    {
        [Header("Currency Info")]
        public CurrencyType currencyType;
        public string nameLocKey;
        public string iconName;
        public string packIconName;
        public bool inHud;
        
        [Header("Properties")]
        public bool canGoNegative = false;
        public int maxAmount = int.MaxValue;
        public int welcomeBonus = 0;
    }

    [Serializable]
    public class CurrencyDataConfig
    {
        public List<CurrencyData> datas = new();
    }

    [CreateAssetMenu(fileName = "CurrencyConfig", menuName = "Config/Game/Wallet/CurrencyConfig")]
    public class CurrencyConfig : JsonConvertableConfig
    {
        [SerializeField] private CurrencyDataConfig configData = new();

        private Dictionary<CurrencyType, CurrencyData> cached = new();

        public List<CurrencyData> Items => configData.datas;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "CurrencyConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (CurrencyDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(CurrencyDataConfig);

        public void Initialize()
        {
            cached.Clear();

            foreach (var data in Items)
            {
                if (cached.TryAdd(data.currencyType, data))
                    continue;

                Debug.LogError($"[GAME DESIGN] Currency duplicate {data.currencyType}. Check config {nameof(CurrencyConfig)}", this);
            }
        }


        public bool TryGet(CurrencyType type, out CurrencyData result)
        {
            if (cached.TryGetValue(type, out result))
                return true;

            Debug.LogError($"[GAME DESIGN] Data not found for currency {type}. Check config {nameof(CurrencyConfig)}", this);
            return false;
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
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<CurrencyData>(tableData);
        }
#endif
    }
}