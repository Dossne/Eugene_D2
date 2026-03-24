using SayKitInternal;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantSwitchExpressionArms

#endregion

public class SayKitAlertIconPrefab : MonoBehaviour
{
    public GameObject sayKitAlertIconPrefab;

    private void OnEnable()
    {
        if (sayKitAlertIconPrefab)
        {
            sayKitAlertIconPrefab.SetActive(CheckATTStatus(SKBridgeManager.Instance.GetATTStatus()));
        }
    }

    public static bool CheckATTStatus(string status)
    {
        return status switch
        {
            "Authorized" => false,
            "Denied" => true,
            "NotDetermined" => true,
            "Restricted" => true,
            "SystemDenied" => false,
            _ => false
        };
    }
}