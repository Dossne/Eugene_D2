using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Infrastructure.SystemModules;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

public class ProjectPreBuilder
{
    public static void PrepareBuild_debug()
    {
        ActualizeBuildNumber();
        MarkAddressableCheatGroup(true);
        EditorConfigWindow.GenerateSayGameConfig();
        RemoveSayKitAssets();
        EnableAddressablesDebugReport();
    }


    public static void PrepareBuild_dev()
    {
        ActualizeBuildNumber();
        MarkAddressableCheatGroup(true);
        EditorConfigWindow.GenerateSayGameConfig();
        RemoveSayKitAssets();
        EnableAddressablesDebugReport();
    }


    public static void PrepareBuild_release()
    {
        ActualizeBuildNumber();
        MarkAddressableCheatGroup(true);
        EditorConfigWindow.GenerateSayGameConfig();
        RemoveSayKitAssets();
    }


    public static void PrepareBuild_store()
    {
        ActualizeBuildNumber();
#if PR_CHEAT
        MarkAddressableCheatGroup(true);
#else
        MarkAddressableCheatGroup(false);
        RemoveCheatFolder();
#endif
        EditorConfigWindow.GenerateSayGameConfig();
        RemoveSayKitAssets();
    }


    private static void ActualizeBuildNumber()
    {
        BuildInformationService.Actualize();
    }


    private static void MarkAddressableCheatGroup(bool included)
    {
        var groups = AddressableAssetSettingsDefaultObject.Settings.groups;

        foreach (AddressableAssetGroup group in groups)
        {
            if (group.Name == "Cheat")
            {
                BundledAssetGroupSchema bundledAssetGroupSchema = group.GetSchema<BundledAssetGroupSchema>();
                if (bundledAssetGroupSchema != null)
                {
                    bundledAssetGroupSchema.IncludeInBuild = included;
                    EditorUtility.SetDirty(group);
                }
            }
        }
    }


    private static void RemoveCheatFolder()
    {
        List<string> foldersNames = new List<string>()
        {
            "Cheat",
        };

        string[] definesToRemove = { "PR_CHEAT" };
        RemoveDefines(BuildTargetGroup.Android, definesToRemove);
        RemoveDefines(BuildTargetGroup.iOS, definesToRemove);

        foreach (string folderName in foldersNames)
        {
            string[] derectories = Directory.GetDirectories("Assets", folderName, SearchOption.AllDirectories);
            foreach (string path in derectories)
            {
                RemoveFileOrFolder(path);
            }
        }
    }


    private static void RemoveDefines(BuildTargetGroup btg, string [] definesToRemove)
    {
        if(definesToRemove.Length == 0)
            return;
        
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
        List<string> allDefines = definesString.Split(';').ToList();

        int removedCount = 0;
        
        foreach (string define in definesToRemove)
        {
            if(define == null)
                continue;

            if (allDefines.Remove(define))
                removedCount++;
        }
        if(removedCount > 0)
            PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, string.Join(";", allDefines.ToArray()));
    }


    private static void RemoveSayKitAssets()
    {
#if PR_SAYKIT_REMOVE
        string[] definesToRemove = { "PR_SAYKIT_ENABLED" };
        RemoveDefines(BuildTargetGroup.Android, definesToRemove);
        RemoveDefines(BuildTargetGroup.iOS, definesToRemove);
        RemoveFileOrFolder("Assets/SayKit"); //remove entire folder for clean build (approx -50Mb)
#endif
    
        SayKitHack();
    }


    private static void SayKitHack()
    {
        string uiManagerPath = "Assets/SayKit/Internal/Managers/UIManager.cs";
        
        if(!File.Exists(uiManagerPath))
            return;
        
        string uiManagerText = File.ReadAllText(uiManagerPath);
        uiManagerText = uiManagerText.Replace("SayKitInterstitialSplashPopup.GetInstance()", "//SayKitInterstitialSplashPopup.GetInstance();");
        uiManagerText = uiManagerText.Replace("SayKitInterstitialTimerPopup.GetInstance();", "//SayKitInterstitialTimerPopup.GetInstance();");
        File.WriteAllText(uiManagerPath, uiManagerText);
    }


    private static void RemoveFileOrFolder(string path)
    {
        string meta = path + ".meta";
        FileUtil.DeleteFileOrDirectory(path);
        FileUtil.DeleteFileOrDirectory(meta);
    }


    /// <summary>
    ///For addressable profiling purposes.
    ///After jenkins build ready, replace your local content in folder "Library\com.unity.addressables"
    /// to jenkins/outer builder content from same path,
    /// after that set path to local buildlayout.json to addressable profiler
    /// </summary>
    public static void EnableAddressablesDebugReport()
    {
        ProjectConfigData.GenerateBuildLayout = true;
        SetAutoOpenReport(false);
    }


    public static void DisableAddressablesDebugReport()
    {
        ProjectConfigData.GenerateBuildLayout = false;
    }

    static void SetAutoOpenReport(bool value)
    {
        var type = typeof(ProjectConfigData);
        var prop = type.GetProperty("AutoOpenAddressablesReport",
                                    BindingFlags.NonPublic | BindingFlags.Static);

        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(null, value);
        }
    }
}