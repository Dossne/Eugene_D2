#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections.Generic;
#endif
using UnityEngine;

namespace Infrastructure.Utilities
{
    public static class AddressableUtils
    {

#if UNITY_EDITOR


        public static GameObject GetSourcePrefab_Editor(GameObject instance)
        {
            return PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance);
        }


        public static string GetAddressableAssetGuid_Editor(GameObject asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                return "not_found_key";
            }

            AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));
            return entry?.address;
        }


        public static void FindDuplicateKeys()
        {
            HashSet<string> unique = new HashSet<string>();
            List<string> duplicateKeys = new List<string>();
            foreach (var locator in Addressables.ResourceLocators)
            {
                foreach (var key in locator.Keys)
                {
                    string strKey = key.ToString();

                    if (!unique.Add(strKey))
                    {
                        duplicateKeys.Add(strKey);
                    }
                }
            }

            foreach (var item in duplicateKeys)
            {
                Debug.Log($"Duplicate key '{item}'");
            }
        }
        
        
        public static void FindKey(string key)
        {
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations))
                {
                    Debug.Log($"Key '{key}' found in locator {locator.GetType().Name}, locations count: {locations.Count}");

                    foreach (var loc in locations)
                    {
                        Debug.Log($" - {loc.PrimaryKey} | {loc.InternalId} | {loc.ResourceType}");
                    }
                }
            }
        }
#endif
    }
}