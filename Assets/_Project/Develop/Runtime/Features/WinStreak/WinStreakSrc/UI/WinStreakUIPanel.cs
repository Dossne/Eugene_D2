using Cysharp.Threading.Tasks;
using Features.ProgressBar;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Pool;
using Infrastructure.TooltipControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.WinStreak
{
    public class WinStreakUIPanel : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image icon;
        [SerializeField] private SegmentedProgressBar progressBar;
        [SerializeField] private Button infoBtn;
        [SerializeField] private Tooltip infoTooltip;
        [SerializeField] private GameObject lockRoot;
        [SerializeField] private GameObject rewardMultiplierRoot;
        [SerializeField] private GameObject rewardMultiplierFxRoot;

        [SerializeField] private RectTransform infoBtnRect;
        [SerializeField] private RectTransform panelRect;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI lockTxt;

        [Header("IconFx")]
        [SerializeField] private float scaleAnimDelaySec = 0.2f;
        [SerializeField] private AnimationCurve iconScaleDownCurve;
        [SerializeField] private AnimationCurve iconScaleUpCurve;
        [SerializeField] private float iconScaleDownDurationSec = 0.1f;
        [SerializeField] private float iconScaleUpDurationSec = 0.1f;
        [SerializeField] private GameObject backParticleFx;

        private PoolService poolService;
        private WinStreakStateController winStreakStateController;
        private int unlockGameLevel;
        private bool isUnlocked;
        private bool isMultiplierUnlocked;
        private bool isInit;

        public RectTransform PanelRect => panelRect;
        public RectTransform InfoButtonRect => infoBtnRect;


        [Inject]
        public void Construct(PoolService poolService, WinStreakStateController winStreakStateController)
        {
            this.poolService = poolService;
            this.winStreakStateController = winStreakStateController;
        }


        public void Construct(int segmentMaxCount, int unlockGameLevel, bool isUnlocked, bool isMultiplierUnlocked)
        {
            this.progressBar.Construct(segmentMaxCount);
            this.unlockGameLevel = unlockGameLevel;
            this.isUnlocked = isUnlocked;
            this.isMultiplierUnlocked = isMultiplierUnlocked;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            headerTxt.text = LocalizationService.I.Get(LocKeys.WinStreak.PanelHeader);
            lockTxt.text = LocalizationService.I.Get(LocKeys.WinStreak.Unlock, unlockGameLevel.ToString());
            
            infoTooltip.Initialize();
            progressBar.Initialize();
            infoBtn.onClick.AddListener(OpenTooltip);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            infoTooltip.Deinitialize();
            infoBtn.onClick.RemoveListener(OpenTooltip);

            isInit = false;
        }


        public void RefreshState(Sprite icon, int level)
        {
            this.icon.sprite = icon;
            progressBar.SetSliderToSegment(level);
            progressBar.SetObjectActive(isUnlocked);
            lockRoot.SetActive(!isUnlocked);
            rewardMultiplierRoot.SetActive(isUnlocked && isMultiplierUnlocked);
            backParticleFx.SetObjectActive(level > 0);
            rewardMultiplierFxRoot.SetActive(winStreakStateController.RewardMultiplier > 1 && isMultiplierUnlocked);
        }


        public void RefreshStateAnimated(Sprite fromIcon, int fromLevel, Sprite toIcon, int toLevel)
        {
            RefreshState(fromIcon, fromLevel);

            if (isUnlocked && fromLevel != toLevel)
                SetToLevelAnimatedAsync(fromLevel, toIcon, toLevel).Forget();
        }

        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }
        
        public void OpenTooltip()
        {
            string text = LocalizationService.I.Get(LocKeys.WinStreak.PanelTooltip);
            infoTooltip.Show(text);
        }


        private async UniTaskVoid SetToLevelAnimatedAsync(int fromLevel, Sprite toIcon, int toLevel)
        {
            var token = gameObject.GetCancellationTokenOnDestroy();

            if (scaleAnimDelaySec > 0)
                await UniTask.WaitForSeconds(scaleAnimDelaySec, ignoreTimeScale: true, cancellationToken: token);

            progressBar.SliderAnimatedMove(fromLevel, toLevel);
            
            await GetScaleDownTween().ToUniTask(cancellationToken: token);
            
            this.icon.sprite = toIcon;
            backParticleFx.SetObjectActive(toLevel > 0);
            PlayParticleFx();
            
            await GetScaleUpTween().ToUniTask(cancellationToken: token);
        }


        private BroTweenSafe GetScaleDownTween()
        {
            return BroTween.ScaleByCurve(icon.transform, iconScaleDownDurationSec, iconScaleDownCurve)
                           .SetUpdate(true)
                           .ToSafe();
        }


        private BroTweenSafe GetScaleUpTween()
        {
            return BroTween.ScaleByCurve(icon.transform, iconScaleUpDurationSec, iconScaleUpCurve)
                           .SetUpdate(true)
                           .ToSafe();
        }


        private void PlayParticleFx()
        {
            ParticlesPool particlesPool = poolService.Get<ParticlesPool>();
            
            if(particlesPool.TryGetItem(PoolableParticleType.WinStreakSplash, out PoolableParticleSystem fx))
                fx.transform.position = icon.transform.position;
        }
        

#if UNITY_EDITOR
        private WinStreakStateController wsStateControllerDebug;
        
        public void SetStateController_Editor(WinStreakStateController wsStateControllerDebug)
        {
            this.wsStateControllerDebug = wsStateControllerDebug;
        }


        [SerializeField] private int fromLevelDebug;
        
        [TriInspector.Button]
        private void TestMoveZeroAnimation()
        {
            (Sprite icon, int level) current = wsStateControllerDebug.GetWinStreakData(fromLevelDebug);
            (Sprite icon, int level) zero = wsStateControllerDebug.GetWinStreakData(0);
            RefreshStateAnimated(current.icon, current.level, zero.icon, zero.level);
        }
#endif
    }
}