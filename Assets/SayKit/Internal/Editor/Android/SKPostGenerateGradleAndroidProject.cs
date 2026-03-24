#if UNITY_EDITOR && UNITY_ANDROID
using System;
using System.IO;
using UnityEngine;
using UnityEditor.Android;
using SayKitInternal;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#endregion

public class SKPostGenerateGradleAndroidProject : IPostGenerateGradleAndroidProject
{
#if SAYKIT_SHIFT_ANDROID_POSTPROCESS_ORDER
    public int callbackOrder => int.MaxValue - 1;
#else
    public int callbackOrder => int.MaxValue;
#endif

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        SKManifestInjector.Instance.AddDependencyLinesToManifestFile(path);
        SKGradleInjector.Instance.UpdateProjectGradle(path);
        SKGradleInjector.Instance.UpdateLauncherGradle(path);
        SKGradleInjector.Instance.UpdateUnityLibraryGradle(path);
        SKGradleInjector.Instance.UpdateGradleProperties(path);
        SKGradleInjector.Instance.UpdateGradleWrapperFile(path);
        SKGradleInjector.Instance.UpdateSettingsGradleFile(path);
        SKGradleInjector.Instance.ExcludePlayCoreGroupForSplitBinary(path);
        SKGradleInjector.Instance.UpdateGPGSGradleFile(path);
        SKGradleInjector.Instance.UpdateEasyMobileGradleFile(path);
        SKGradleInjector.Instance.UpdateMobileNotificationGradleFile(path);
        SKGradleInjector.Instance.UpdateSayKitGradleFile(path);

        var assetsPath = Path.Combine(path, "src/main/assets");
        var resPath = Path.Combine(path, "src/main/res");
        var skLibPath = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty, SayKitPreBuild.SayKitLibPath);

        CopyFile(Path.Combine(Application.dataPath, "Plugins/Android/google-services.json"),
            Path.Combine(path.Replace("unityLibrary", "launcher"), "google-services.json"));
        CopyFile(Path.Combine(Application.dataPath, "Resources/SayKit/saykit_attribution_settings.json"),
            Path.Combine(assetsPath, "saykit_attribution_settings.json"));
        CopyFile(Path.Combine(Application.dataPath, "Resources/SayKit/saykit_" + SayKitApp.APP_KEY_ANDROID + "_" + Application.version + ".json"),
            Path.Combine(assetsPath, "saykit_" + SayKitApp.APP_KEY_ANDROID + "_" + Application.version + ".json"));
        CopyFile(Path.Combine(skLibPath, "res/values/strings.xml"),
            Path.Combine(resPath, "values/strings.xml"));
        CopyFile(Path.Combine(skLibPath, "res/xml/network_security_config.xml"),
            Path.Combine(resPath, "xml/network_security_config.xml"));
        CopyFile(Path.Combine(Application.dataPath, "Resources/SayKit/saykit_versions.json"),
            Path.Combine(assetsPath, "saykit_versions.json"));
        CopyFile(Path.Combine(Application.dataPath, "Resources/SayKit/saykit_platform_defines.txt"),
            Path.Combine(assetsPath, "saykit_platform_defines.txt"));
        CopyFile(Path.Combine(Application.dataPath, "Plugins/Android/saykit/res/drawable/notification_icon.xml"),
            Path.Combine(resPath, "drawable/notification_icon.xml"));

        CopyLocalizationFiles(assetsPath);
    }

    private void CopyLocalizationFiles(string destinationDirectoryPath)
    {
        var sourceDirectoryPath = Path.Combine(Application.dataPath, "Resources/SayKit/Localizations/");

        if (Directory.Exists(sourceDirectoryPath))
        {
            var localizationFiles = Directory.GetFiles(sourceDirectoryPath, "*.json", SearchOption.TopDirectoryOnly);

            if (localizationFiles.Length > 0)
            {
                foreach (var sourceFilePath in localizationFiles)
                {
                    var fileName = Path.GetFileName(sourceFilePath);
                    var destinationFilePath = Path.Combine(destinationDirectoryPath, fileName);

                    CopyFile(sourceFilePath, destinationFilePath);
                }
            }
        }
    }

    private void CopyFile(string sourcePath, string destPath)
    {
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destPath))
        {
            Debug.LogError("[SKPostGenerateGradleAndroidProject][CopyFile] Invalid path arguments");
            return;
        }

        if (File.Exists(sourcePath))
        {
            var targetDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            try
            {
                File.Copy(sourcePath, destPath, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SKPostGenerateGradleAndroidProject][CopyFile] {e}");
            }
        }
    }
}

#endif
