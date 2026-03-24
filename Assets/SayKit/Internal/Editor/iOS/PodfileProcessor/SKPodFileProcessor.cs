#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable FieldCanBeMadeReadOnly.Global

#endregion

namespace SayKitInternal
{
    public class SKPodFileProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => int.MaxValue;

        #region Const

        private const string Tag = "[SKPodFileProcessor]";
        private const string SayKitPodSpec = "source 'https://gitlab.saygames.io/saykit/say-podspecs'";
        
        private const string CocoapodsSource = "source 'https://cdn.cocoapods.org/'";
        private const string TargetUnityFramework = "target 'UnityFramework' do";
        private const string TargetUnityiPhone = "target 'Unity-iPhone' do";
        private const string PlatformiOS = "platform :ios";
        private const string LinkageStaticField = "use_frameworks! :linkage => :static";
        private const string Indent = "  ";
        private const string End = "end";
        private const string Name = "name";

        #endregion

        private static readonly string[] PodsTargets = { TargetUnityiPhone, TargetUnityFramework };

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

            var buildPath = report.summary.outputPath;
            var podfilePath = Path.Combine(buildPath, "Podfile");

            if (File.Exists(podfilePath))
            {
                SayKitDebug.Log($"{Tag} Podfile already exists. Updating dependencies...");
                UpdatePodfile(podfilePath);
            }
            else
            {
                SayKitDebug.Log($"{Tag} Podfile not found. Creating a new one...");
                CreatePodfile(podfilePath);
            }

            AddAdditionalParams(buildPath);
            CheckCocoaPodsCredentials();
            InstallPods(buildPath);
        }
        
        private static void CheckCocoaPodsCredentials()
        {
            var cocoaPodsScriptPath = Path.Combine(Application.dataPath,
                "SayKit/Internal/Editor/iOS/PodfileProcessor/CocoaPodsCredentials.sh");

            if (File.Exists(cocoaPodsScriptPath))
            {
                var podPath = GetPodPath();
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x '{cocoaPodsScriptPath}' && PODPATH={podPath} '{cocoaPodsScriptPath}'\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                RunCommand(processStartInfo, "CheckCocoaPodsCredentials");
            }
        }

        private static void CreatePodfile(string podfilePath)
        {
            if (string.IsNullOrEmpty(podfilePath))
            {
                SayKitDebug.LogError($"{Tag} [CreatePodfile] invalid podfilePath: {podfilePath}");
                return;
            }

            try
            {
                var podfileContent = new StringBuilder();

                podfileContent.AppendLine(CocoapodsSource);
                podfileContent.AppendLine(SayKitPodSpec);
                podfileContent.AppendLine();
                podfileContent.AppendLine($"{PlatformiOS}, '{PlayerSettings.iOS.targetOSVersionString}'");
                podfileContent.AppendLine();

                foreach (var target in PodsTargets)
                {
                    podfileContent.AppendLine(target);

                    foreach (var pod in SKPodDependency.GetRequiredPods())
                    {
                        if (pod.BothTarget || target == TargetUnityFramework)
                        {
                            podfileContent.AppendLine($"{Indent}{pod}");
                        }
                    }

                    podfileContent.AppendLine(End);
                }

                podfileContent.AppendLine(LinkageStaticField);
                podfileContent.AppendLine();

                File.WriteAllText(podfilePath, podfileContent.ToString());

                SayKitDebug.Log($"{Tag} Podfile created at: {podfilePath}");
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{Tag} [CreatePodfile] {e.Message}, {e.StackTrace}");
            }
        }

        private static void UpdatePodfile(string podfilePath)
        {
            try
            {
                var lines = File.ReadAllLines(podfilePath).ToList();
                var updatedPodfile = new StringBuilder();

                updatedPodfile.AppendLine(SayKitPodSpec);

                if (!lines.Any(l => l.Contains("https://cdn.cocoapods.org/")))
                {
                    updatedPodfile.AppendLine(CocoapodsSource);
                }

                if (!lines.Any(l => l.Contains(PlatformiOS)))
                {
                    updatedPodfile.AppendLine($"{PlatformiOS}, '{PlayerSettings.iOS.targetOSVersionString}'");
                }
                
                var lineRegex = new Regex(@"^\s*pod\s+'(?<name>[^']+)'", RegexOptions.Compiled);
               
                var insideTargetBlock = false;
                string currentTarget = null;

                var requiredPods = SKPodDependency.GetRequiredPods();

                foreach (var rawLine in lines)
                {
                    var trimmedStart = rawLine.TrimStart();

                    if (PodsTargets.Any(t => trimmedStart.StartsWith(t)))
                    {
                        insideTargetBlock = true;
                        currentTarget = PodsTargets.First(t => trimmedStart.StartsWith(t));
                        updatedPodfile.AppendLine(rawLine);
                        continue;
                    }

                    if (insideTargetBlock && trimmedStart == End)
                    {
                        foreach (var pod in requiredPods)
                        {
                            var needed = pod.BothTarget || currentTarget == TargetUnityFramework;
                            if (needed)
                            {
                                updatedPodfile.AppendLine($"{Indent}{pod}");
                            }
                        }

                        updatedPodfile.AppendLine(End);
                        insideTargetBlock = false;
                        currentTarget = null;
                        continue;
                    }

                    var match = lineRegex.Match(rawLine);
                    if (insideTargetBlock && match.Success)
                    {
                        var podName = match.Groups[Name].Value;
                        if (requiredPods.All(pod => pod.Name != podName))
                        {
                            updatedPodfile.AppendLine(rawLine);
                        }

                        continue;
                    }

                    updatedPodfile.AppendLine(rawLine);
                }

                if (!updatedPodfile.ToString().Contains(LinkageStaticField))
                {
                    updatedPodfile.AppendLine();
                    updatedPodfile.AppendLine(LinkageStaticField);
                }

                File.WriteAllText(podfilePath, updatedPodfile.ToString());
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{Tag} [UpdatePodfile] {e.Message}, {e.StackTrace}");
            }
        }

        private static void AddAdditionalParams(string buildPath)
        {
            try
            {
                var podfilePath = Path.Combine(buildPath, "Podfile");

                using (var writer = File.AppendText(podfilePath))
                {
                    writer.WriteLine();
                    writer.WriteLine("post_install do |installer|");
                    writer.WriteLine("  installer.pods_project.targets.each do |target|");
                    writer.WriteLine("    target.build_configurations.each do |config|");
                    writer.WriteLine("      config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '15.0'");
                    writer.WriteLine("    end");
                    writer.WriteLine("  end");

                    if (SKUtils.CheckXcodeVersion16())
                    {
                        writer.WriteLine();
                        writer.WriteLine("  bitcode_strip_path = `xcrun --find bitcode_strip`.chop!");
                        writer.WriteLine("  def strip_bitcode_from_framework(bitcode_strip_path, framework_relative_path)");
                        writer.WriteLine("    framework_path = File.join(Dir.pwd, framework_relative_path)");
                        writer.WriteLine("    puts \"Checking framework: #{framework_path}\"");
                        writer.WriteLine("    if File.exist?(framework_path)");
                        writer.WriteLine("      puts \"Stripping bitcode from: #{framework_path}\"");
                        writer.WriteLine("      command = \"#{bitcode_strip_path} \\\"#{framework_path}\\\" -r -o \\\"#{framework_path}\\\"\"");
                        writer.WriteLine("      system(command)");
                        writer.WriteLine("    else");
                        writer.WriteLine("      puts \"Framework not found: #{framework_path}\"");
                        writer.WriteLine("    end");
                        writer.WriteLine("  end");
                        writer.WriteLine();
                        writer.WriteLine("  framework_paths = Dir.glob([");
                        writer.WriteLine("    \"./**/Pods/OMSDK_Appodeal/OMSDK_Appodeal.xcframework/ios-arm64/OMSDK_Appodeal.framework/OMSDK_Appodeal\",");
                        writer.WriteLine("    \"./**/Pods/OguryAds/OMSDK_Ogury.xcframework/ios-arm64/OMSDK_Ogury.framework/OMSDK_Ogury\",");
                        writer.WriteLine("    \"./**/Pods/smaato-ios-sdk/vendor/OMSDK_Smaato.xcframework/ios-arm64_armv7/OMSDK_Smaato.framework/OMSDK_Smaato\",");
                        writer.WriteLine("    \"./**/Pods/**/OMSDK_Pubmatic.framework/OMSDK_Pubmatic\"");
                        writer.WriteLine("]);");
                        writer.WriteLine();
                        writer.WriteLine("  framework_paths.each do |framework_relative_path|");
                        writer.WriteLine("    strip_bitcode_from_framework(bitcode_strip_path, framework_relative_path)");
                        writer.WriteLine("  end");
                    }

                    writer.WriteLine(End);
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{Tag} [AddAdditionalParams] {e.Message}, {e.StackTrace}");
            }
        }

        private static void RunPodCommand(string workingDir, string podPath, string command, string description)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"export LANG=en_US.UTF-8; export LC_ALL=en_US.UTF-8; {podPath} {command}\"",
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            RunCommand(processStartInfo, description);
        }
        
        private static void RunCommand(ProcessStartInfo processStartInfo, string description)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
#if SAYKIT_DEBUG
                    var defaultOutput = new StringBuilder();
                    var errorOutput = new StringBuilder();

                    process.OutputDataReceived += (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            defaultOutput.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorOutput.AppendLine(e.Data);
                        }
                    };
#endif

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Debug.Log($"{Tag} {description} completed successfully.");
#if SAYKIT_DEBUG
                        SayKitDebug.Log($"{Tag} {description} completed successfully. Output:\n{defaultOutput}");
#endif
                    }
                    else
                    {
#if SAYKIT_DEBUG
                        var combined = new StringBuilder();
                        combined.AppendLine($"[Exit {process.ExitCode}] {description} failed.");
                        
                        if (defaultOutput.Length > 0)
                        {
                            combined.AppendLine("=== STDOUT ===").AppendLine(defaultOutput.ToString());
                        }

                        if (errorOutput.Length > 0)
                        {
                            combined.AppendLine("=== STDERR ===").AppendLine(errorOutput.ToString());
                        }
                        
                        SayKitDebug.Log($"{Tag} {combined}");
#endif                       
                        Debug.LogError($"{Tag} {description} failed with exit code: {process.ExitCode}");
                    }
                }
            }
            catch (Exception e)
            {
                SayKitDebug.Log($"{Tag} [RunPodCommand] {e.Message}, {e.StackTrace}");
            }
        }

        private static void InstallPods(string projectPath)
        {
            var podPath = GetPodPath();

            if (!string.IsNullOrEmpty(podPath))
            {
                RunPodCommand(projectPath, podPath, "repo update", "Updating CocoaPods repositories");
                RunPodCommand(projectPath, podPath, "install", "Installing pods");
            }
        }

        private static string GetPodPath()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"which pod\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var defaultOutput = new StringBuilder();
            var errorOutput = new StringBuilder();

            using (var process = new Process())
            {
                process.StartInfo = psi;
                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        defaultOutput.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorOutput.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }

            var podPath = defaultOutput.ToString().Trim();
            
            if (string.IsNullOrEmpty(podPath) || !File.Exists(podPath))
            {
                string[] possiblePaths =
                {
                    "/usr/local/bin/pod",
                    "/opt/homebrew/bin/pod",
                    "/usr/bin/pod"
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        Debug.Log($"{Tag} Pod path: {path}");
                        return path;
                    }
                }

                Debug.Log($"{Tag} Pod path not found!");
            }

            return podPath;
        }
    }
}
#endif