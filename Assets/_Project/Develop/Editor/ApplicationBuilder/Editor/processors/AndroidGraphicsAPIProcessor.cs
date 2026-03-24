#if SAYBUILDER_ANDORID_FIX_GRAPHICS_API
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;  // <- Add this!

namespace SayBuilder.Editor
{
    public class AndroidGraphicsAPIProcessor : IPreprocessBuildWithReport
    {
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("CI: Checking and setting Graphics API for Android build.");

            if (report.summary.platform == BuildTarget.Android)
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);

                GraphicsDeviceType[] graphicsAPIs = { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.Vulkan };
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, graphicsAPIs);

                Debug.Log("CI: Auto Graphics API disabled. Set to OpenGLES3 and Vulkan.");
            }
        }

        public int callbackOrder => 0;
    }
}
#endif
