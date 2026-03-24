/// <summary>
/// The script gives you choice to whether to build addressable bundles when clicking the build button.
/// For custom build script, call PreExport method yourself.
/// For cloud build, put BuildAddressablesProcessor.PreExport as PreExport command.
/// Discussion: https://forum.unity.com/threads/how-to-trigger-build-player-content-when-build-unity-project.689602/
/// </summary>
using UnityEngine;
#if ENABLE_BUILD_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace ApplicationBuilder.processors
{
    class BuildAddressablesProcessor
    {
        /// <summary>
        /// Run a clean build before export.
        /// </summary>
        public static void PreExport()
        {
#if ENABLE_BUILD_ADDRESSABLES
            Debug.Log("CI: BuildAddressablesProcessor.PreExport start");

            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent();

            Debug.Log("CI: BuildAddressablesProcessor.PreExport done");
#else
            Debug.Log("CI: ENABLE_BUILD_ADDRESSABLES not defined, so skip build addressable");
#endif
        }

    }
}