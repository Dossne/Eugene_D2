using AYellowpaper.SerializedCollections;
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



namespace LevelEditor
{
    [CreateAssetMenu(fileName = "LevelCollectableCounter", menuName = "Config/Tools/LevelCollectableCounter")]
    public class LevelCollectableCounter : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private string lvl;
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private CollectablesConfig collectableConfig;
        [SerializeField] private SerializedDictionary<string, string> results = new();
        [SerializeField] private SerializedDictionary<CollectableType, int> counts = new();
        Dictionary<int, int> amounts = new();
        Dictionary<CollectableType, int> sizes = new();

        [TriInspector.Button]
        private void CountObjectSizes()
        {
            if (lvl.IsNullOrEmpty())
            {
                Debug.LogWarning($"Level not selected.");
                return;
            }

            var levelData = levelConfig.Items.Find(x => lvl == x.id);
            if (levelData != null)
            {
                Debug.LogWarning($"Level found in LevelConfig. Search in addressables skipped.");
                CollectData(levelData);
                return;
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
                if (entry.address != lvl)
                    continue;

                string path = AssetDatabase.GUIDToAssetPath(entry.guid);
                LevelScriptableObject asset = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(path);
                if (asset == null)
                {
                    Debug.LogError($"LevelScriptableObject with guid {entry.guid} is null.");
                    continue;
                }                    

                CollectData(asset.ToLevelData());
                return;
            }           
        }

        private void CollectData(LevelData levelData)
        {
            collectableConfig.Initialize();

            amounts.Clear();
            results.Clear();
            sizes.Clear();
            counts.Clear();
            float totalAmount = 0;

            string rawJson = StringUtils.DecompressString(levelData.levelJson);
            var currentLevel = JsonConvert.DeserializeObject<Level>(rawJson, JsonUtils.SerializerSettings);

            for (int i = 0; i < currentLevel.collectables.Count; i++)
            {
                var item = currentLevel.collectables[i];

                if (!sizes.TryGetValue(item.collectableType, out int size))
                {
                    if (!collectableConfig.TryGet(item.collectableType, out CollectablesData data))
                    {
                        Debug.LogWarning($"{item.collectableType} not found in CollectableConfig");
                        continue;
                    }

                    sizes.Add(item.collectableType, data.size);
                    counts.TryAdd(item.collectableType, 0);
                }

                if (!amounts.ContainsKey(sizes[item.collectableType]))
                    amounts.Add(sizes[item.collectableType], 0);


                amounts[sizes[item.collectableType]]++;
                counts[item.collectableType]++;
                totalAmount++;
            }
            var keys = amounts.Keys.ToList();
            keys.Sort();

            for (int i = 0; i < keys.Count; i++)
                results[$"s{keys[i]}"] = $"{amounts[keys[i]] / totalAmount * 100f:N1}";
        }
#endif
    }
}