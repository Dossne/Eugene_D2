using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using Features.LevelConfiguration;
using Features.LevelSequence;
using System.Linq;

namespace LevelEditor
{
    [CreateAssetMenu(fileName = "LevelScriptableObjectManager", menuName = "Config/Game/LevelScriptableObjectManager")]

    public class LevelScriptableObjectManager : ScriptableObject
    {
        [Header("LSO(LevelScriptableObject) - файл данных одного игрового уровня")]
        [Space]
        [Space]
        [Header("Выкладка уровней в репу")]
        [Header("1. Создать LSO новых уровней, не регистрируя их в адрессаблы")]
        [Header("2. Подтянуть свежую ревизию из репы(конфликт адрессаблов разрешить в пользу версии из репы)")]
        [Header("3. Нажать кнопку Find Unregistered LSO")]
        [Header("4. Нажать кнопку Register Selected LSO")]
        [Space]
        [Space]
        [Header("Выгрузка LSO из LevelConfig")]
        [Header("1. Нажать кнопку Create LSO From LevelConfig")]
        [Header("2. Нажать кнопку Register Selected LSO (перед выкладкой)")]
        [Space]
        [Space]
        [Header("Загрузка LSO в LevelConfig")]
        [Header("1. Ввести id уровней в список Selected LSO")]
        [Header("2. Нажать кнопку Add Selected LSO To Config")]
        [Space]
        [Space]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private LevelSequenceConfig levelSequenceConfig;
        [SerializeField] private LevelDifficultyCache levelDifficultyCache;
        [Space]
        [SerializeField] private string LSOFolderPath = "Assets/_Project/Develop/Runtime/Features/LevelConfiguration/LevelDataCfg/Levels";
        [SerializeField] private bool overwriteExistingLSO = true;
        [Space]
        [Space]
        [SerializeField] private List<string> selectedLSO = new();
       

        [TriInspector.Button]
        public void CreateLSOFromLevelConfig()
        {
            selectedLSO.Clear();
            var configData = levelConfig.Items;
            for (var i = 0; i < configData.Count; i++)
            {
                var data = configData[i];
                var assetPath = $"{LSOFolderPath}/{data.id}.asset";
                bool fileExists = File.Exists(assetPath);

                if (fileExists && !overwriteExistingLSO)
                {
                    Debug.LogWarning($"[LevelDesign] Asset already exists at {assetPath}. Skip creation.");
                    continue;
                }

                LevelScriptableObject dataSO = null;
                if (fileExists)
                {
                    dataSO = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(assetPath);
                    if (dataSO == null)
                    {
                        Debug.LogError($"[LevelDesign] LevelScriptableObject {data.id} is null.");
                        continue;
                    }

                    dataSO.FromLevelData(data);
                    EditorUtility.SetDirty(dataSO);
                }
                else 
                {
                    dataSO = ScriptableObject.CreateInstance<LevelScriptableObject>();
                    dataSO.FromLevelData(data);
                    AssetDatabase.CreateAsset(dataSO, assetPath);
                }
                
                selectedLSO.Add(dataSO.id);
                AssetDatabase.SaveAssets();                
            }
            AssetDatabase.Refresh();
        }

        [TriInspector.Button]
        public void ClearSelectedLSO()
        {
            selectedLSO.Clear();
        }

        [TriInspector.Button]
        public void FindUnregisteredLSO()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.FindGroup(levelConfig.LevelAddressableGroup);
            if (group == null)
            {
                Debug.LogError($"[LevelDesign] AddressableAssetGroup {levelConfig.LevelAddressableGroup} not found.");
                return;
            }

            selectedLSO.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(LevelScriptableObject).Name}", new[] { LSOFolderPath });
            List<string> levelnames = new();

            string searchFolder = LSOFolderPath.EndsWith("/") ? LSOFolderPath : LSOFolderPath + "/";

            foreach (string guid in guids)
            {
                AddressableAssetEntry existingEntry = settings.FindAssetEntry(guid);
                if (existingEntry != null)
                {
                    if (existingEntry.parentGroup != group)
                        Debug.LogWarning($"[LevelDesign] Asset found in {existingEntry.parentGroup.Name} group. Address: {existingEntry.address}");
                    continue;
                }

                string path = AssetDatabase.GUIDToAssetPath(guid);
                string directory = Path.GetDirectoryName(path).Replace('\\', '/') + "/";
                if (directory == searchFolder)  // exact match
                    levelnames.Add(Path.GetFileName(path).Replace(".asset", ""));
            }

            selectedLSO.AddRange(levelnames);
            Debug.LogWarning($"[LevelDesign] {selectedLSO.Count} unregistered LSOs was found in {LSOFolderPath}.");            
        }


        [TriInspector.Button]
        public void RegisterSelectedLSO() 
        {            
            for (var i = 0; i < selectedLSO.Count; i++)
            {
                var assetPath = $"{LSOFolderPath}/{selectedLSO[i]}.asset";
                if (!File.Exists(assetPath))
                {
                    Debug.LogError($"[LevelDesign] Asset not exists at {assetPath}. Skip registration.");
                    continue;
                }

                AddToAddressables(selectedLSO[i], assetPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [TriInspector.Button]
        public void AddSelectedLSOToConfig()
        {
            for (var i = 0; i < selectedLSO.Count; i++)
            {
                var assetPath = $"{LSOFolderPath}/{selectedLSO[i]}.asset";
                if (!File.Exists(assetPath))
                {
                    Debug.LogWarning($"[LevelDesign] Asset not exists at {assetPath}. Skip registration.");
                    continue;
                }

                LevelScriptableObject asset = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(assetPath);
                if (asset == null)
                {
                    Debug.LogError($"[LevelDesign] LevelScriptableObject {selectedLSO[i]} is null.");
                    continue;
                }
                levelConfig.Insert_Editor(asset.ToLevelData());
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [TriInspector.Button]
        public void ValidateLevelSequenceConfig()
        {            
            selectedLSO.Clear();
            var lvlIds = levelSequenceConfig.GetAllLevelsIds();
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.FindGroup(levelConfig.LevelAddressableGroup);
            if (group == null)
            {
                Debug.LogError($"[LevelDesign] AddressableAssetGroup {levelConfig.LevelAddressableGroup} not found.");
                return;
            }

            bool levelNotRegistered = false;
            var entryList = group.entries.ToList().Select(x => x.address);
            for (int i = 0; i < lvlIds.Count; i++)
            {
                if (entryList.Contains(lvlIds[i])) 
                    continue;       

                selectedLSO.Add(lvlIds[i]);
                levelNotRegistered = true;
            }

            if (levelNotRegistered)
            {
                Debug.LogWarning($"[LevelDesign] LevelSequenceConfig contains unregistered level ids.");
                Debug.Log($"[LevelDesign] Try to register found unregistered level ids.");
                RegisterSelectedLSO();
            }
            else 
            {
                Debug.Log($"[LevelDesign] LevelSequenceConfig validated.");
            }

            levelDifficultyCache.Clear();
            foreach (var item in group.entries)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(item.guid);
                var soData = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(assetPath);
                if (soData == null)
                {
                    Debug.LogError($"[LevelDesign] LevelScriptableObject with guid {item.guid} is null.");
                    continue;
                }
                Debug.Log($"[LevelDesign] LevelDifficultyCache updated for {soData.id}, difficulty set to {soData.difficulty}");
                levelDifficultyCache.Add(soData.id, soData.difficulty);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        private void AddToAddressables(string id, string assetPath)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.FindGroup(levelConfig.LevelAddressableGroup);
            if (group == null)
            {
                group = settings.CreateGroup(levelConfig.LevelAddressableGroup, false, false, true, null);
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"[LevelDesign] Failed to get GUID for asset at {assetPath}");
                return;
            }


            AddressableAssetEntry existingEntry = settings.FindAssetEntry(guid);
            if (existingEntry != null)
            {
                if (existingEntry.parentGroup == group)
                    Debug.Log($"[LevelDesign] Asset already addressable in the {levelConfig.LevelAddressableGroup} group. Address: {existingEntry.address}");
                else
                    Debug.Log($"[LevelDesign] Asset already addressable in group '{existingEntry.parentGroup.Name}'.");
                return;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            if (entry != null)
            {
                entry.address = id;
                entry.SetLabel(levelConfig.LevelAddressableLabel, true);

                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();

                Debug.Log($"[LevelDesign] Asset added to Addressables with address: {entry.address}");
            }
            else
            {
                Debug.LogError("[LevelDesign] Failed to create Addressable entry.");
            }
        }
    }
}