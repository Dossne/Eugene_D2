using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infrastructure.Utilities
{
    public class Utils
    {

        private static CultureInfo customCulture = null;

        public static bool IsNull<T>(T nullObject)
        {
            return nullObject == null || nullObject.Equals(null);
        }


        public static string GenerateUniqueId()
        {
            return $"{Guid.NewGuid()}";
        }

        public static string GetSpaceSeparatedNumberString(float number)
        {
            if (customCulture == null)
            {
                customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                customCulture.NumberFormat.NumberGroupSeparator = " ";
                customCulture.NumberFormat.NumberGroupSizes = new int[] { 3 };
            }
            
            return number.ToString("#,0", customCulture);
        }


#if UNITY_EDITOR


        public static Sprite GetSpriteFromAtlasMainUi_Editor(string spriteName)
        {
            return SearchItemInAssetdatabase_Editor<Sprite>(spriteName, "Assets/_Project/Art/SpriteAtlases");
        }


        public static T SearchItemInAssetdatabase_Editor<T>(string Name_, string Path_) where T : Object
        {
            string[] guids2 = AssetDatabase.FindAssets("\"" + Name_ + "\"", new[] { Path_ });
            string GUIDTOPATH = "";
            for (int i = 0; i < guids2.Length; ++i)
            {
                GUIDTOPATH = AssetDatabase.GUIDToAssetPath(guids2[i]);
                bool isChinaFolder = GUIDTOPATH.Contains("ChinaLocalizedAssets");
                bool isCorrectFolder = !isChinaFolder;
#if SAYKIT_CHINA_VERSION
            isCorrectFolder = isChinaFolder;
#endif
                if (isCorrectFolder
                    && Path.GetFileNameWithoutExtension(GUIDTOPATH) == Name_)
                {
                    break;
                }
            }

            T ObjectFounded = AssetDatabase.LoadAssetAtPath<T>(GUIDTOPATH);
            if (ObjectFounded == null)
            {
                Debug.LogError($"[CODE] File {Name_} not found in {Path_} folder");
            }

            return ObjectFounded;
        }


        public static T[] LoadAllAssetsOfType_Editor<T>(string path, string filter = null) where T : Object
        {
            string typeFilter = filter ?? $"t:{nameof(GameObject)}";
            string[] guids = AssetDatabase.FindAssets(typeFilter, new[] { path });
            return guids
                   .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                   .Where(asset => asset != null)
                   .ToArray();
        }


#endif

    }
}