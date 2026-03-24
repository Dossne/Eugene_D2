using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SayKitInternal;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable RedundantNameQualifier
// ReSharper disable FieldCanBeMadeReadOnly.Local

#endregion

public class SayKitUI : MonoBehaviour
{
    public static SayKitUI instance;

    public bool IsPopupShowed;
    private bool _debugMenuShown;

    public Sprite checkedOn;
    public Sprite checkedOff;
    public Text textTitle;
    public Text textTop;
    public Text textBottom;
    public Button buttonAccept;
    public Button buttonOk;

    public GameObject gdprPanel;
    public GameObject debugMenuPanel;

    bool wasAccepted;

    static Color colorAccepted = new Color(94f / 255, 135f / 255, 101f / 255, 1);
    static Color colorNotAccepted = new Color(243f / 255, 202f / 255, 82f / 255, 1);
    
    static string gdpr_popup_title = "gdpr_popup_title";
    static string gdpr_popup_text_top = "gdpr_popup_text_top";
    static string gdpr_popup_text_bottom = "gdpr_popup_text_bottom";
    static string gdpr_popup_button_accept = "gdpr_popup_button_accept";
    static string gdpr_popup_button_ok = "gdpr_popup_button_ok";
    static string gdpr_privacy_url = "gdpr_privacy_url";
    
    public static SayKitUI getInstance()
    {
        if (!instance)
        {
            instance =
                SayKitInternal.SKUtils.FindComponentOnRootObjects<SayKitUI>() ??
                Instantiate(SayKitAssets.Instance.SayKitUI);

            DontDestroyOnLoad(instance.gameObject);
            instance.name = "[SayKitUI]";

            var canvas = instance.GetComponent<Canvas>();
            SKUtils.SetupCanvas(canvas);

            instance.buttonAccept.onClick.AddListener(instance.ClickAccept);
            instance.buttonOk.onClick.AddListener(instance.ClickOk);
            instance.textBottom.GetComponent<Button>().onClick.AddListener(instance.ClickTerms);
        }

        return instance;
    }

    public static void CallShowPopup()
    {
        UIManager.Instance.StartCoroutine(ShowPopupOnMain());
    }

    private static IEnumerator ShowPopupOnMain()
    {
        yield return new WaitUntil(() => RemoteConfigManager.Instance.Initialized);
        ShowPopup();
    }

    private static void ShowPopup()
    {
        getInstance().ShowPopupInternal();
    }

    private void ClickAccept()
    {
        wasAccepted = !wasAccepted;
        Refresh();
    }

    private void ClickOk()
    {
        IsPopupShowed = false;

        gdprPanel.SetActive(false);
        HideSayKitUI();
        
        SayKitDebug.Log("SayKitBridgeEditor [GrantGdprConsent]");
    }

    private void ClickTerms()
    {
        SKBridgeManager.Instance.TrackEvent(name: "gdpr_privacy_click");
        Application.OpenURL(SKManager.Instance.GetLocalizedString(gdpr_privacy_url));
    }

    void Refresh()
    {
        buttonOk.interactable = wasAccepted;
        buttonAccept.GetComponentsInChildren<Image>()[1].sprite = wasAccepted ? checkedOn : checkedOff;
        buttonAccept.GetComponentsInChildren<Image>()[0].color = wasAccepted ? colorAccepted : colorNotAccepted;
    }

    void ShowPopupInternal()
    {
        IsPopupShowed = true;

        ShowSayKitUI();
        gdprPanel.SetActive(true);
        wasAccepted = false;

        textTitle.text = SKManager.Instance.GetLocalizedString(gdpr_popup_title);
        textTop.text = SKManager.Instance.GetLocalizedString(gdpr_popup_text_top);
        textBottom.text = SKManager.Instance.GetLocalizedString(gdpr_popup_text_bottom);

        buttonOk.GetComponentInChildren<Text>().text = SKManager.Instance.GetLocalizedString(gdpr_popup_button_ok);
        buttonAccept.GetComponentInChildren<Text>().text = SKManager.Instance.GetLocalizedString(gdpr_popup_button_accept);

        Refresh();
    }

    private void ShowSayKitUI() { gameObject.SetActive(true); }

    private void HideSayKitUI()
    {
#if UNITY_IOS
        if (!IsPopupShowed)
        {
            gameObject.SetActive(false);
        }
#else
        gameObject.SetActive(false);
#endif
    }

    public void ShowDebugMenu()
    {
        if (_debugMenuShown) { return; }
        _debugMenuShown = true;
        
        SKBridgeManager.Instance.TrackEvent(name: "show_debug_menu");
        
        gameObject.SetActive(true);
        
        instance.StartCoroutine(DebugService.Instance.DownloadVersionList());
        instance.StartCoroutine(WaitUntilDownloadVersionList());
    }

    private IEnumerator WaitUntilDownloadVersionList()
    {
        yield return new WaitUntil(() => DebugService.Instance.VersionList.Count > 0);
        
        UISayKitDebugMenu.GetInstance().ShowPopup();
    }

    public void HideDebugMenu()
    {
        _debugMenuShown = false;
        
        gameObject.SetActive(false);
        
        UISayKitDebugMenu.GetInstance().HidePopup();
    }

}