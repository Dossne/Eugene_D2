using SayKitInternal;
using UnityEngine;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

public class SayKitAdIdPopup : MonoBehaviour
{
    #region LocalizationdKeys

    private const string HEADER_KEY = "sk_ad_id_header_popup";
    private const string SHARE_BUTTON_KEY = "sk_ad_id_share_button_popup";
    private const string CLOSE_BUTTON_KEY = "sk_ad_id_close_button_popup";

    #endregion

    private static SayKitAdIdPopup _instance;

    [SerializeField] private GameObject sayKitAdIdPopupPrefab;
    [SerializeField] private Button buttonShare;
    [SerializeField] private Button buttonClose;
    [SerializeField] private Text headerText;
    [SerializeField] private Text buttonShareText;
    [SerializeField] private Text buttonCloseText;

    public static SayKitAdIdPopup GetInstance()
    {
        if (!_instance)
        {
            _instance =
                SKUtils.FindComponentOnRootObjects<SayKitAdIdPopup>() ??
                Instantiate(SayKitAssets.Instance.SayKitAdIdPopupPrefab);

            var canvas = _instance.GetComponent<Canvas>();
            SKUtils.SetupCanvas(canvas);

            DontDestroyOnLoad(_instance.gameObject);
            _instance.name = "[SayKitAdIdPopup]";
        }

        return _instance;
    }

    public void ShowPopup()
    {
        var instance = GetInstance();
        if (instance)
        {
            if (sayKitAdIdPopupPrefab != null)
            {
                if (buttonShare)
                {
                    buttonShare.onClick.AddListener(() => { SKBridgeManager.Instance.OpenSystemSettings(); });
                }

                if (buttonClose)
                {
                    buttonClose.onClick.AddListener(ClosePopup);
                }

                SetLocalizedText(headerText, HEADER_KEY);
                SetLocalizedText(buttonShareText, SHARE_BUTTON_KEY);
                SetLocalizedText(buttonCloseText, CLOSE_BUTTON_KEY);

                sayKitAdIdPopupPrefab.SetActive(true);
            }
        }
    }

    private void ClosePopup()
    {
        var instance = GetInstance();
        if (instance)
        {
            if (sayKitAdIdPopupPrefab != null)
            {
                sayKitAdIdPopupPrefab.SetActive(false);
            }
        }
    }

    private void SetLocalizedText(Text text, string key)
    {
        if (!text) return;

        var locString = SKManager.Instance.GetLocalizedString(key);
        if (!string.IsNullOrEmpty(locString))
        {
            text.text = locString;
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            if (sayKitAdIdPopupPrefab)
            {
                sayKitAdIdPopupPrefab.gameObject.SetActive(SayKitAlertIconPrefab.CheckATTStatus(SKBridgeManager.Instance.GetATTStatus()));
            }
        }
    }
}
