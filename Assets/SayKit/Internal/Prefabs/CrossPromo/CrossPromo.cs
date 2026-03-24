using SayKitInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable once InconsistentNaming

#endregion

[RequireComponent(typeof(RectTransform))]
public class CrossPromo : MonoBehaviour
{
    public RectTransform promoRect;
    public Image inPlayImage;
    public Text inPlayText;
    public VideoPlayer videoPlayer;
    public Button promoButton;

    private const string Tag = "[SKPromo]";
    private Canvas _rootCanvas;

    private void Awake()
    {
        promoRect.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!InPlayService.Instance.IsInPlay() && videoPlayer != null)
        {
            videoPlayer.errorReceived += CrossPromoService.Instance.OnVideoPlayerError;
        }
    }

    private void OnDisable()
    {
        if (!InPlayService.Instance.IsInPlay() && videoPlayer != null)
        {
            videoPlayer.errorReceived -= CrossPromoService.Instance.OnVideoPlayerError;
        }
    }

    public static void Init()
    {
        if (!InPlayService.Instance.IsInPlay())
        {
            CrossPromoService.Instance.Init();
        }
    }

    public void Show()
    {
        if (!TryResolveRootCanvas(out _rootCanvas))
        {
            SayKitDebug.LogError($"{Tag} Root Canvas not found. " +
                                 "Place CrossPromo prefab under a Screen Space root Canvas.");
            return;
        }
        
        if (InPlayService.Instance.IsInPlay())
        {
            inPlayImage?.gameObject.SetActive(true);
            inPlayText?.gameObject.SetActive(true);

            InPlayService.Instance.Show(promoRect, inPlayImage, _rootCanvas);
        }
        else
        {
            SayKitDebug.LogWarning($"{Tag} InPlay disabled by config.");

            inPlayImage?.gameObject.SetActive(false);
            inPlayText?.gameObject.SetActive(false);

            CrossPromoService.Instance.Show(promoRect.gameObject, promoButton, videoPlayer);
        }
    }

    public void Hide()
    {
        if (InPlayService.Instance.IsInPlay())
        {
            InPlayService.Instance.Hide(promoRect);
        }
        else
        {
            CrossPromoService.Instance.Hide();
        }
    }

    private static bool TryResolveRootCanvas(out Canvas root)
    {
        root = null;

        var canvas = GetComponentInParentStatic<Canvas>();
        if (canvas == null)
        {
            return false;
        }

        root = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;

        if (root.renderMode == RenderMode.WorldSpace)
        {
            root = null;
            return false;
        }

        return true;
    }

    private static T GetComponentInParentStatic<T>() where T : Component
    {
        var any = FindObjectOfType<CrossPromo>();
        return any ? any.GetComponentInParent<T>() : null;
    }
}