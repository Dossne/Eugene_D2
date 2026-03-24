using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Pool.Particles;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;

namespace Features.Competition
{
    [System.Serializable]
    public class MultiplierPanelAnimations
    {
        [Header("Panel show")]
        public float beginDelay = 0.1f;
        public AnimationCurve showEase;
        public float showDuration = 1f;

        [Header("Text")]
        public float textScaleDelay = 0.35f;
        public AnimationCurve textScaleCurve;
        public float textScaleDuration = 0.25f;

        [Header("PanelStay")]
        public float panelHideDelay = 0.5f;

        [Header("Panel hide")]
        public AnimationCurve hideEase;
        public float hideDuration = 1f;
    }

    public class MultiplierPanelView : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private RectTransform mainRoot;

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Transform labelRoot;

        [SerializeField] private ReusableParticleSystemUI splashFx;
        [SerializeField] private Vector2 showPosition;
        [SerializeField] private Vector2 hidePos;
        [SerializeField] private string prefix = "X";

        [Header("Animation")]
        [SerializeField] private MultiplierPanelAnimations animParams;

        [Header("Debug")]
        [SerializeField] private BroTweenSafe seq;

        private bool isInit;
        private string nextText;

        private void OnDisable()
        {
            seq.Kill();
        }

        public void Construct(int current, int next)
        {
            label.text = $"{prefix}{current.ToString()}";
            nextText = $"{prefix}{next.ToString()}";
        }

        public void Initialize()
        {
            if (isInit)
                return;
            
            CreateSequence();
            mainRoot.anchoredPosition = hidePos;
            splashFx.Initialize();
            SetObjectActive(true);

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            SetObjectActive(false);

            isInit = false;
        }

        public UniTask PlayAsync(CancellationToken token)
        {
            seq.Play();
            return seq.ToUniTask(cancellationToken: token);
        }

        private void CreateSequence()
        {
            var sequence = BroTween.Sequence().SetUpdate(true);

            //panel show
            sequence.AppendInterval(animParams.beginDelay);
            sequence.Append(BroTween.AnchoredPosition(mainRoot, hidePos, showPosition, animParams.showDuration).SetEase(animParams.showEase));

            //text animation
            sequence.AppendInterval(animParams.textScaleDelay);
            sequence.AppendCallback(ChangeText);
            sequence.Append(BroTween.ScaleByCurve(labelRoot, animParams.textScaleDuration, animParams.textScaleCurve));

            //panel hide
            sequence.AppendInterval(animParams.panelHideDelay);
            sequence.Append(BroTween.AnchoredPosition(mainRoot, showPosition, hidePos, animParams.hideDuration).SetEase(animParams.hideEase));

            seq = sequence.ToSafe();
        }

        private void ChangeText()
        {
            label.text = nextText;
            splashFx.Play();
        }

        private void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

#region UNITY_EDITOR

        [TriInspector.Button]
        private void SetShowPos_Editor()
        {
            showPosition = mainRoot.anchoredPosition;
        }

        [TriInspector.Button]
        private void SetHidePos_Editor()
        {
            hidePos = mainRoot.anchoredPosition;
        }

#endregion
    }
}