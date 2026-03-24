#if SAY_BUILDER_ADAPTIVE_ICON
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SayBuilder.Editor
{
    public class AdaptiveIconProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public static List<List<Texture2D>> LoadIcons()
        {

            var iconFiles = new string[][]
            {
                new string[] {
                    "bg-xxxhdpi-432x432.png",  
                    "fg-xxxhdpi-432x432.png",
                }, 
                
                new string[] {
                    "bg-xxhdpi-324x324.png",  
                    "fg-xxhdpi-324x324.png",
                },
                
                new string[] {
                    "bg-xhdpi-216x216.png",  
                    "fg-xhdpi-216x216.png",
                },                
                
                new string[] {
                    "bg-hdpi-162x162.png",  
                    "fg-hdpi-162x162.png",
                }, 
                
                new string[] {
                    "bg-mdpi-108x108.png",  
                    "fg-mdpi-108x108.png",
                },

                new string[] {
                    "bg-ldpi-81x81.png",  
                    "fg-ldpi-81x81.png",
                },
               
            };

            List<List<Texture2D>> result = new List<List<Texture2D>>();

            foreach (var pair in iconFiles)
            {
                var icons = LoadIconSet(pair);
                result.Add(icons);
            }
            return result;
        }

        private static List<Texture2D> LoadIconSet(string[] iconFiles)
        {
            var rootPath = CIBuilderDefines.PathToAdaptiveIconFolder;
            var result = new List<Texture2D>();
            foreach (var iconFile in iconFiles)
            {
                var iconPath = Path.Combine(rootPath, iconFile);
                Debug.Log($"CI: Adaptive Icon: Trying loading icon by path {iconPath}");
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                
                if (icon == null || icon.width == 0 || icon.height == 0)
                {
                    Debug.LogError("CI: AdaptiveIcon: We need setup adaptive icons, but can`t load it");
                    EditorApplication.Exit(1);
                }
                else
                {
                    Debug.Log($"CI: AdaptiveIcon: Icon size: {icon.width}x{icon.height}");
                }
                
                result.Add(icon);
            }
            return result;
        }

        [MenuItem("SayKit/Build Server/Set Adaptive Icon")]
        public static void LoadAndSetAdaptiveIcons()
        {
#if UNITY_ANDROID

            var supportedIconKinds = PlayerSettings.GetSupportedIconKindsForPlatform(BuildTargetGroup.Android);
            bool supportAdaptiveIconKind = false;
        
            foreach (var iconKind in supportedIconKinds)
                if (Equals(iconKind, AndroidPlatformIconKind.Adaptive))
                    supportAdaptiveIconKind = true;

            if (supportAdaptiveIconKind)
            {
                Debug.Log("Adaptive Icon: This version Unity support adaptive icons");

                
            
            
                var platform = BuildTargetGroup.Android;
                var kind = AndroidPlatformIconKind.Adaptive;
                var currentIcons = PlayerSettings.GetPlatformIcons(platform, kind);
            
                var newIcons= LoadIcons();
                
                var sets = currentIcons.Zip(newIcons, (n, w) => new { Current = n, New = w });
                
                foreach (var icon in sets)
                {
                    var textures = icon.Current.GetTextures();

                    if (textures.Length >= 2)
                    {   
                        Debug.Log("AdaptiveIcon: Project already have adaptive icons icons, skip");
                    }
                    else
                    {
                        icon.Current.SetTextures(icon.New.ToArray());
                    }
                
                }
                PlayerSettings.SetPlatformIcons(platform, kind, currentIcons);
            }
            else
            {
                Debug.LogError("This version Unity NOT support adaptive icons");
                EditorApplication.Exit(1);
            }

#endif        
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("AdaptiveIcon: AdaptiveIconProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
            LoadAndSetAdaptiveIcons();
        }
    }
}
#endif
