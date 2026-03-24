using UnityEngine;

namespace ApplicationBuilder.processors
{
    class BuildAssetBundlesProcessor
    {
        /// <summary>
        /// Run a clean build before export.
        /// </summary>
        public static void PreExport()
        {
#if ENABLE_BUILD_ASSETBUNDLES
            Debug.Log("CI: BuildAssetBundlesProcessor.PreExport start");

            string assetBundlesDirectory = "Assets/StreamingAssets";
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(assetBundlesDirectory);
            }
            BuildPipeline.BuildAssetBundles(assetBundlesDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            Debug.Log("CI: BuildAssetBundlesProcessor.PreExport done");
#else
            Debug.Log("CI: ENABLE_BUILD_ASSETBUNDLES not defined, so skip build asset bundles");
#endif
        }
    }
}