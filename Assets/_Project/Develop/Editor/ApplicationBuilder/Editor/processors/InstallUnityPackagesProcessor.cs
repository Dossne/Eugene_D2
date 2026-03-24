#if SAY_BUILDER_INSTALL_UNITY_PACKAGES
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SayBuilder.Editor
{
    public class InstallUnityPackagesProcessor : IPreprocessBuildWithReport
    {

        public void InstallUnityPackage(string path)
        {
            Debug.Log($"CI: begin install following package: {path}");
            AssetDatabase.ImportPackage(path, false);
            Debug.Log($"CI: end install following package: {path}");
        }
        
        public void InstallUnityPackages()
        {
            var pathToPackages = Application.dataPath + "/SayBuilder/Editor/projects/" + CIBuilderDefines.ProjectNameConfigPath + "/unitypackages/";
            foreach (string packagePath in System.IO.Directory.GetFiles(pathToPackages))
            {
                if (packagePath.EndsWith(".unitypackage"))
                {
                    InstallUnityPackage(packagePath);
                }
                                
            }
        }
        
        public void OnPreprocessBuild(BuildReport report)
        {

            InstallUnityPackages();
        }

        public int callbackOrder => 0;
    }
}
#endif