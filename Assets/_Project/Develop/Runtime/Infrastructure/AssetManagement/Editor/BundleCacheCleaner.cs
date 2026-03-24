using UnityEditor;
using UnityEngine;

namespace AddressablesTools
{
  public class BundleCacheCleaner
  {
    [MenuItem("Tools/Clear Addressable bundles cache")]
    private static void ClearBundleCache()
    {
      Caching.ClearCache();
    }
  }
}