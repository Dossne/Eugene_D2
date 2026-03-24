#if UNITY_EDITOR && UNITY_ANDROID
using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    public class SKAndroidPostBuild
    {
        [PostProcessBuild(int.MaxValue)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
        {
#if SAYKIT_UPLOAD_ANDROID_SYMB
            UploadFirebaseSymbols(pathToBuildProject);
#endif
        }

        private static void UploadFirebaseSymbols(string pathToBuildProject)
        {
            const string tag = "[PostBuild Android]: ";

            try
            {
                var symbolsPath = string.Empty;

                var firebaseConfig = 
                    JsonConvert.DeserializeObject<FirebaseConfiguration>(SayKitRemoteSettings.Instance.Firebase);

                if (!string.IsNullOrEmpty(firebaseConfig.Client[0].ClientInfo.MobileSdkAppId))
                {
                    var directoryPath = Path.GetDirectoryName(pathToBuildProject);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        var files = Directory.GetFiles(directoryPath);

                        if (files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                if (file.Contains("symbols.zip"))
                                {
                                    symbolsPath = file;
                                }
                            }
                        }
                    }

                    if (!File.Exists(symbolsPath))
                    {
                        SayKitDebug.LogError(tag + "Symbols not found at path: " + symbolsPath);
                        return;
                    }

                    var uploadScriptPath = Path.Combine(Application.dataPath +
                                                        "/SayKit/Internal/Editor/Android/Utils/firebase_upload_symb.py");

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/python3",
                        Arguments =
                            $"\"{uploadScriptPath}\" {firebaseConfig.Client[0].ClientInfo.MobileSdkAppId} \"{symbolsPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    startInfo.Environment["PATH"] += ":/usr/local/bin";

                    var process = new Process();
                    process.StartInfo = startInfo;

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Debug.Log(tag + e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Debug.LogError(tag + e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
                else
                {
                    SayKitDebug.LogError(tag + $"Failed to parse firebase config, json data: {SayKitRemoteSettings.Instance.Firebase}");
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError(tag + $"{e.Message}, {e.StackTrace}");
            }
        }
    }
}
#endif
