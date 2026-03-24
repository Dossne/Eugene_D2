#if !SAYKIT_APPLOVIN_QUALITY_IOS_DISABLE

//
//  MaxIntegrationManager.cs
//  AppLovin MAX Unity Plugin
//
//  Created by Santosh Bagadi on 8/29/19.
//  Copyright © 2019 AppLovin. All rights reserved.
//

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX

#if UNITY_IOS && UNITY_EDITOR || UNITY_IPHONE && UNITY_EDITOR

using System.Diagnostics;
using System.Text;
using UnityEngine.Networking;
using System.IO;
using SayKitInternal;
using UnityEditor;
using UnityEditor.Callbacks;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable UseObjectOrCollectionInitializer

#endregion

/// <summary>
/// Adds AppLovin Quality Service to the iOS project once the project has been exported.
///
/// 1. Downloads the Quality Service ruby script.
/// 2. Runs the script using Ruby which integrates AppLovin Quality Service to the project.
/// </summary>
public class AppLovinPostProcessiOS
{
    private const string OutputFileName = "AppLovinQualityServiceSetup.rb";

    [PostProcessBuild(int.MinValue)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        var outputFilePath = Path.Combine(buildPath, OutputFileName);

        // Check if Quality Service is already installed.
        if (File.Exists(outputFilePath) && Directory.Exists(Path.Combine(buildPath, "AppLovinQualityService")))
        {
            return;
        }

        // Download the ruby script needed to install Quality Service
        var downloadHandler = new DownloadHandlerFile(outputFilePath);
        var postJson = $"{{\"sdk_key\" : \"NDcrL4E6eTcwHnfZbHRNw-0AoxPCMdQSgFOFkOyJ8D1VJrumRobtyvufr93tccBk3mIya4_CTO_SENV-vluony\"}}";
        var bodyRaw = Encoding.UTF8.GetBytes(postJson);
        var uploadHandler = new UploadHandlerRaw(bodyRaw);
        uploadHandler.contentType = "application/json";

        using (var unityWebRequest = new UnityWebRequest("https://api2.safedk.com/v1/build/ios_setup2"))
        {
            unityWebRequest.method = UnityWebRequest.kHttpVerbPOST;
            unityWebRequest.downloadHandler = downloadHandler;
            unityWebRequest.uploadHandler = uploadHandler;
            var operation = unityWebRequest.SendWebRequest();

            // Wait for the download to complete or the request to timeout.
            while (!operation.isDone) { }

#if UNITY_2020_1_OR_NEWER
            if (unityWebRequest.result != UnityWebRequest.Result.Success)
#else
                if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
#endif
            {
                SayKitDebug.Log("AppLovin Quality Service installation failed. Failed to download script with error: " + unityWebRequest.error);
                return;
            }

            // Check if Ruby is installed
            var rubyVersion = AppLovinCommandLine.Run("ruby", "--version", buildPath);
            if (rubyVersion.ExitCode != 0)
            {
                SayKitDebug.Log("AppLovin Quality Service installation requires Ruby. Please install Ruby, export it to your system PATH and re-export the project.");
                return;
            }

            // Ruby is installed, run `ruby AppLovinQualityServiceSetup.rb`
            var result = AppLovinCommandLine.Run("ruby", OutputFileName, buildPath);

            // Check if we have an error.
            if (result.ExitCode != 0) SayKitDebug.Log("Failed to set up AppLovin Quality Service");

            SayKitDebug.Log(result.Message);
        }
    }

    /// <summary>
    /// A helper class to run command line tools.
    ///
    /// TODO: Currently only supports shell (Linux). Add support for Windows machines.
    /// </summary>
    public class AppLovinCommandLine
    {
        /// <summary>
        /// Result obtained by running a command line command.
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Standard output stream from command line.
            /// </summary>
            public string StandardOutput;

            /// <summary>
            /// Standard error stream from command line. 
            /// </summary>
            public string StandardError;

            /// <summary>
            /// Exit code returned from command line.
            /// </summary>
            public int ExitCode;

            /// <summary>
            /// The description of the result that can be used for error logging.
            /// </summary>
            public string Message;
        }

        /// <summary>
        /// Runs a command line tool using the provided <see cref="toolPath"/> and <see cref="arguments"/>.
        /// </summary>
        /// <param name="toolPath">The tool path to run</param>
        /// <param name="arguments">The arguments to be passed to the command line tool</param>
        /// <param name="workingDirectory">The directory from which to run this command.</param>
        /// <returns></returns>
        public static Result Run(string toolPath, string arguments, string workingDirectory)
        {
            var stdoutFileName = Path.GetTempFileName();
            var stderrFileName = Path.GetTempFileName();

            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;

            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.FileName = "bash";
            process.StartInfo.Arguments = $"-l -c '\"{toolPath}\" {arguments} 1> {stdoutFileName} 2> {stderrFileName}'";
            process.Start();

            process.WaitForExit();

            var stdout = File.ReadAllText(stdoutFileName);
            var stderr = File.ReadAllText(stderrFileName);

            File.Delete(stdoutFileName);
            File.Delete(stderrFileName);

            var result = new Result();
            result.StandardOutput = stdout;
            result.StandardError = stderr;
            result.ExitCode = process.ExitCode;

            var messagePrefix = result.ExitCode == 0 ? "Command executed successfully" : "Failed to run command";
            result.Message =
                $"{messagePrefix}: '{toolPath} {arguments}'\nstdout: {stdout}\nstderr: {stderr}\nExit code: {process.ExitCode}";

            return result;
        }
    }
}

#endif
#endif
#endif