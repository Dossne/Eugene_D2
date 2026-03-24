using Features.BottomPanel;
using Features.TargetMarker;
using Features.Tutorial;
using Infrastructure.CameraControl;
using Infrastructure.LoadScreen;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.MainUICanvasControl
{
    public class MainUIProvider : MonoBehaviour
    {
        [Header("MainScene")]
        [SerializeField] private RectTransform safeAreaTransform;
        [SerializeField] private Canvas mainUICanvas;
        [SerializeField] private CanvasScaler mainUICanvasScaler;
        [SerializeField] private LoadScreenUI loadScreenUI;
        [SerializeField] private HudProvider hudProvider;
        [SerializeField] private GameObject metaSceneBg;
        [SerializeField] private CameraVFX cameraVFX;
        [SerializeField] private RawImage overlay3d;
        [SerializeField] private MaskTutorialPanel maskTutorialPanel;
        [SerializeField] private BottomPanelBase bottomPanel;

        [Header("Roots")]
        [SerializeField] private RectTransform countersRoot;
        [SerializeField] private RectTransform levelUpRoot;
        [SerializeField] private RectTransform popupRoot;
        [SerializeField] private RectTransform screenRoot;
        [SerializeField] private RectTransform flyingCurrencyRoot;
        [SerializeField] private TargetMarkersRoot targetMarkersRoot;
        [SerializeField] private RectTransform fxFlyToUiStartArea;
        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private RectTransform overPopupRoot;

        public RectTransform SafeAreaTransform => safeAreaTransform;
        public Canvas MainUICanvas => mainUICanvas;
        public CanvasScaler MainUICanvasScaler => mainUICanvasScaler;
        public LoadScreenUI LoadScreenUI => loadScreenUI;
        public HudProvider HudProvider => hudProvider;
        public GameObject MetaSceneBg => metaSceneBg;
        public CameraVFX CameraVFX => cameraVFX;
        public RawImage Overlay3d => overlay3d;
        public MaskTutorialPanel MaskTutorialPanel => maskTutorialPanel;
        public BottomPanelBase BottomPanel => bottomPanel;

        //Roots
        public RectTransform CountersRoot => countersRoot;
        public RectTransform LevelUpRoot => levelUpRoot;
        public RectTransform PopupRoot => popupRoot;
        public RectTransform ScreenRoot => screenRoot;
        public RectTransform FlyingCurrencyRoot => flyingCurrencyRoot;
        public TargetMarkersRoot TargetMarkersRoot => targetMarkersRoot;
        public RectTransform FxFlyToUiStartArea => fxFlyToUiStartArea;
        public RectTransform OverPopupRoot => overPopupRoot;
        public RectTransform TooltipRoot => tooltipRoot;
    }
}