using System;
using System.Collections.Generic;
using System.IO;
using Infrastructure.Configuration;
using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    [Serializable]
    public class LevelPrefabsShowHideData
    {
        public string levelId;
    }

    [Serializable]
    public class LevelPrefabsShowHideDataConfig
    {
        public List<LevelPrefabsShowHideData> datas;
    }

    //GUI controlled by LevelPrefabsShowHidEditor
    [CreateAssetMenu(fileName = "LevelPrefabsShowHide", menuName = "Config/System/LevelPrefabsShowHide")]
    public class LevelPrefabsShowHide : JsonConvertableConfig
    {
        [Header("Base")]
        [SerializeField] private string VisibleFolder = "_Project/Develop/Editor/Level/LevelPrefabs/JelLevels/";
        [SerializeField] private string HiddenFolder = "_Project/Develop/Editor/Level/LevelPrefabsHidden~/";
        [SerializeField] private bool showFullLog = true;

        [Header("Ids")]
        [SerializeField] private LevelPrefabsShowHideDataConfig configData;


#region Custom

        //////////////////BUTTONS//////////////////   
        public void SetPrefabsVisible()
        {
            var from = GetHiddenPath();
            var to = GetVisiblePath();
            TransferFilesByIdList(from, to, showFullLog, false);
        }


        public void CopyHiddenPrefabsByList()
        {
            var from = GetHiddenPath();
            var to = GetVisiblePath();
            TransferFilesByIdList(from, to, showFullLog, true);
        }


        public void HidePrefabsByList()
        {
            var from = GetVisiblePath();
            var to = GetHiddenPath();
            TransferFilesByIdList(from, to, showFullLog, false);
        }

        public void ShowAllPrefabs()
        {
            var from = GetHiddenPath();
            var to = GetVisiblePath();
            MoveAllFilesByPath(from, to, showFullLog);
        }
        
        
        public void HideAllPrefabs()
        {
            var from = GetVisiblePath();
            var to = GetHiddenPath();
            MoveAllFilesByPath(from, to, showFullLog);
        }


        public void ClearList()
        {
            configData.datas.Clear();
        }


        public void OpenHiddenFolder()
        {
            EditorUtility.RevealInFinder(GetHiddenPath());
        }
        //////////////////BUTTONS END////////////////// 


        //////////////////HELPERS//////////////////


        private void MoveAllFilesByPath(string fromPath, string toPath, bool isLog)
        {
            var files = GetAllFilesBy(fromPath);

            if (files == null || files.Length == 0)
            {
                Debug.Log("Nothing to hide. Folder is empty.");
                return;
            }

            int prefabCount = 0;
            foreach (var file in files)
            {
                var fileNameInput = Path.GetFileName(file);
                
                if (fileNameInput.EndsWith("prefab"))
                    prefabCount++;
                
                TransferItem(fileNameInput, fromPath, toPath, false);
            }

            if (isLog)
            {
                Debug.Log($"[LevelEditor] MOVED {prefabCount} prefabs to: \"{toPath}\"");
            }

            AssetDatabase.Refresh();
        }


        private void TransferFilesByIdList(string fromPath, string toPath, bool isLog, bool isCopy)
        {
            List<LevelPrefabsShowHideData> files = configData.datas;

            if (files == null || files.Count == 0)
            {
                Debug.Log("[LevelEditor] Nothing to transfer. List is empty.");
                return;
            }

            foreach (var data in configData.datas)
            {
                TransferLevelPrefabWithMetaById(data.levelId, fromPath, toPath, isLog, isCopy);
            }

            AssetDatabase.Refresh();
        }


        private void TransferLevelPrefabWithMetaById(string levelId, string fromPath, string toPath, bool log, bool justCopy)
        {
            string file = $"{levelId}.prefab";
            string metaFile = $"{file}.meta";
            var fromFileWithPath = Path.Combine(fromPath, file);

            if (!File.Exists(fromFileWithPath))
            {
                Debug.Log($"[LevelEditor] File {fromFileWithPath} does not exist");
                return;
            }

            TransferItem(file, fromPath, toPath, justCopy);
            TransferItem(metaFile, fromPath, toPath, justCopy);

            if (log)
            {
                string action = justCopy ? "COPIED" : "MOVED";
                Debug.Log($"[LevelEditor] {action} prefab: \"{levelId}\" to: \"{toPath}\"");
            }
        }


        private void TransferItem(string fileNameInput, string fromPath, string toPath, bool justCopy)
        {
            var fromFileWithPath = Path.Combine(fromPath, fileNameInput);
            var toFileWithPath = Path.Combine(toPath, fileNameInput);

            if (justCopy)
                File.Copy(fromFileWithPath, toFileWithPath, true);
            else
                File.Move(fromFileWithPath, toFileWithPath);
        }


        private string[] GetAllFilesBy(string path)
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        }


        private string GetVisiblePath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, VisibleFolder));
        }


        private string GetHiddenPath()
        {
            var hidden = Path.Combine(Application.dataPath, HiddenFolder);
            Directory.CreateDirectory(hidden);
            return Path.GetFullPath(hidden);
        }
        //////////////////HELPERS END////////////////// 

#endregion


#region Base

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LevelPrefabsShowHideConfig";
        public override bool IsSaveRequired => false;

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LevelPrefabsShowHideDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LevelPrefabsShowHideDataConfig);
        private List<LevelPrefabsShowHideData> Items => configData.datas;


        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<LevelPrefabsShowHideData>(tableData);
        }

#endregion


    }
}