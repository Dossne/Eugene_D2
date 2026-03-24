#if SAY_BUILDER_AMAZON_STORE && SAYKIT_PURCHASING
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing;


namespace SayBuilder.Editor
{
    public class AmazonStoreProcessor : IPreprocessBuildWithReport
    {
        
        public void OnPreprocessBuild(BuildReport report)
        {

            Debug.Log($"CI: setup UnityPurchasingEditor.TargetAndroidStore to Amazon Store: {AppStore.AmazonAppStore}");
            UnityPurchasingEditor.TargetAndroidStore(AppStore.AmazonAppStore);

        }

        public int callbackOrder => 0;
    }
}

#endif