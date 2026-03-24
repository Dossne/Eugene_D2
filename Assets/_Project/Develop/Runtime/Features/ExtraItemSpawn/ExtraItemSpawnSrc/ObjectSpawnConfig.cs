using Features.Collectables;
using Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.ExtraItemSpawn
{
    [Serializable]
    public class ObjectSpawnData
    {
        public CollectableType collectableType;
        public int unlockLevel;
        public int count;
        public int yPosition;
        public float minScale;
        public float maxScale;
    }

    [CreateAssetMenu(fileName = "ObjectSpawnConfig", menuName = "Config/Game/ObjectSpawnConfig")]
    public class ObjectSpawnConfig : JsonConvertableConfig
    {
        [SerializeField] private List<ObjectSpawnData> objectSpawnDatas = new List<ObjectSpawnData>();

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "ObjectSpawnConfig";

        protected override object ConfigurationDataObject
        {
            get => objectSpawnDatas;
            set => objectSpawnDatas = (List<ObjectSpawnData>)value;
        }

        protected override Type ConfigurationDataType => typeof(ObjectSpawnData);

        public List<ObjectSpawnData> ObjectSpawnDatas => objectSpawnDatas;


        public bool TryGet(CollectableType collectableType, out ObjectSpawnData result)
        {
            foreach (ObjectSpawnData objectSpawnData in objectSpawnDatas)
            {
                if (objectSpawnData.collectableType == collectableType)
                {
                    result = objectSpawnData;
                    return true;
                }
            }

            result = null;
            return false;
        }


#if UNITY_EDITOR


        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(objectSpawnDatas, nameof(ObjectSpawnDatas), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            objectSpawnDatas = GoogleDocsUtils.SnatchDataRowsToConfig<ObjectSpawnData>(tableData);
        }
#endif
    }
}