using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.LevelConfiguration;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;



namespace LevelEditor
{
    [CreateAssetMenu(fileName = "LevelAddressableValidator", menuName = "Config/Tools/LevelAddressableValidator")]
    public class LevelAddressableValidator : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private SerializedDictionary<string, string> absentTables = new();
        private HashSet<string> allAssetGuids = new(); 

        private async UniTaskVoid FindAbsentAddressables() 
        {
            allAssetGuids.Clear();
            await Addressables.InitializeAsync().ToUniTask();
            foreach (var locator in Addressables.ResourceLocators)
            {
                foreach (var key in locator.Keys)
                {
                    var locations = await Addressables.LoadResourceLocationsAsync(key);
                    foreach (var loc in locations)
                    {
                        allAssetGuids.Add(loc.PrimaryKey);
                    }
                }
            }

            absentTables.Clear();
            //check level config
            var levelCount = levelConfig.Items.Count;
            for (int i = 0; i < levelCount; i++) 
            {
                var levelData = levelConfig.Items[i];
                string rawJson = StringUtils.DecompressString(levelData.levelJson);
                var currentLevel = JsonConvert.DeserializeObject<Level>(rawJson, JsonUtils.SerializerSettings);
                var currentLevelTable = currentLevel.table.prefabName;
                if (!allAssetGuids.Contains(currentLevelTable))
                    absentTables.Add(levelData.id, currentLevelTable);                
            }

            //check level addressable
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found.");
                return;
            }
            AddressableAssetGroup group = settings.FindGroup(levelConfig.LevelAddressableGroup);
            if (group == null)
            {
                Debug.LogError("AddressableAssetGroup not found.");
                return;
            }

            foreach (var entry in group.entries)
            {
                string path = AssetDatabase.GUIDToAssetPath(entry.guid);
                LevelScriptableObject asset = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(path);
                if (asset == null)
                    continue;

                string rawJson = StringUtils.DecompressString(asset.levelJson);
                var currentLevel = JsonConvert.DeserializeObject<Level>(rawJson, JsonUtils.SerializerSettings);
                var currentLevelTable = currentLevel.table.prefabName;
                if (!allAssetGuids.Contains(currentLevelTable))
                    absentTables.Add(asset.id, currentLevelTable);
            }

            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        [TriInspector.Button]
        private void CheckLevelAddressables()
        {
            FindAbsentAddressables().Forget();
        }
#endif
    }
}