using SayKitInternal;
using UnityEngine;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

public class SayKitAdIdPrefab : MonoBehaviour
{
    #region LocalizationKeys

    private const string AD_ID_KEY = "sk_ad_id_button";

    #endregion

    [SerializeField] private Text adIdButtonText;
    [SerializeField] private GameObject adIdButton;

    private void OnEnable()
    {
        if (adIdButtonText)
        {
            SetLocalizedText(adIdButtonText);
        }

        if (adIdButton)
        {
            if (SayKitAlertIconPrefab.CheckATTStatus(SKBridgeManager.Instance.GetATTStatus()))
            {
                adIdButton.SetActive(true);
                SKBridgeManager.Instance.TrackEvent(name: "sk_adid", extra1: "show");
            }
            else
            {
                adIdButton.SetActive(false);
            }
        }
    }

    private void SetLocalizedText(Text textComponent)
    {
        if (textComponent)
        {
            var locString = SKManager.Instance.GetLocalizedString(AD_ID_KEY);

            if (!string.IsNullOrEmpty(locString))
            {
                textComponent.text = locString;
            }
        }
    }

    public void ShowAdIdPopup()
    {
        SayKitAdIdPopup.GetInstance().ShowPopup();
        SKBridgeManager.Instance.TrackEvent(name: "sk_adid", extra1: "click");
    }
}