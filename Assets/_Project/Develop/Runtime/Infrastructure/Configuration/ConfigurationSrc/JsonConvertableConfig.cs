using System;
using System.Collections.Generic;
using System.Reflection;
using Infrastructure.SystemModules;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace Infrastructure.Configuration
{

    public abstract class JsonConvertableConfig : ScriptableObject
    {
        [HideInInspector] public string fileName;
        [SerializeField] private bool isRemote = true;


        public abstract string GoogleSheetName { get; set; }
        public abstract string GoogleTableName { get; set; }

        protected abstract Type ConfigurationDataType { get; }

        protected abstract object ConfigurationDataObject { get; set; }

        public virtual bool IsRemote => isRemote;

        public virtual bool IsVerifyRequired => false;

        public virtual bool IsLoadRequired => true;

        public virtual bool IsSaveRequired => true;



        public void SetConfigsFromRemoteSource()
        {
            string json = GetRemoteConfigData();
            
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            object deserializedObject = JsonConvert.DeserializeObject(json, ConfigurationDataType, JsonUtils.SerializerSettings);

            if (deserializedObject != null)
            {
                ConfigurationDataObject = deserializedObject;
                AfterSerializable();
            }
        }


        protected virtual void AfterSerializable()
        {

        }

        private string GetRemoteConfigData()
        {
            FieldInfo[] fi = typeof(SayKitGameConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo remoteConfigField = Array.Find(fi, f => f.Name == name);
            if (remoteConfigField != null)
            {
                return remoteConfigField.GetValue(SdkService.GetConfig()).ToString();
            }

            return null;
        }

#if UNITY_EDITOR
        
        protected static bool isVerifying = false;

        public virtual GoogleDocsUtils.MajorDimension SpreadSheetDimension => GoogleDocsUtils.MajorDimension.ROWS;

        public virtual bool Verify_Editor()
        {
            return true;
        }

        public string GetJson_Editor()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(ConfigurationDataObject, JsonUtils.SerializerSettings);
        }

        public virtual void ToGoogleSpreadSheet_Editor()
        {

        }

        public virtual async void FromGoogleSpreadSheet_Editor()
        {
            IList<IList<object>> data = await GoogleDocsUtils.ReadAsync(fileName, GoogleSheetName, SpreadSheetDimension);
            TableDataToConfigData_Editor(data);
        }

        public virtual void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {

        }

#endif

    }
}