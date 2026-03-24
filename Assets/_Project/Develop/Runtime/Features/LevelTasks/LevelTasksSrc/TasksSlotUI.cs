using System;
using Infrastructure.AudioControl;
using Infrastructure.BroTweens;
using Infrastructure.Pool.Particles;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LevelTasks
{
    public class TasksSlotUI : MonoBehaviour
    {
        public Action OnShrinkStep;

        [Header("Main")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform root;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI count;
        [SerializeField] private GameObject checkMark;

        [Header("Bounce")]
        [SerializeField] private AnimationCurve bounceCurve;
        [SerializeField] private float bounceDuration;
        [SerializeField] private ReusableParticleSystemUI countVFX;

        [Header("Scale")]
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private float scaleDuration = 1f;

        [Header("Shrink")]
        [SerializeField] private float shrinkDuration = 0.3f;

        [Header("Debug")]
        [TriInspector.ShowInInspector] private BroTweenSafe bounceTween;
        [TriInspector.ShowInInspector] private BroTweenSafe shrinkSequence;

        private float defaultWidth = 0;

        public RectTransform IconRect => icon.rectTransform;


        private void OnDisable()
        {
            countVFX.Stop();
            KillTweens();
        }


        private void OnDestroy()
        {
            KillTweens();
        }


        public void Construct(Sprite iconValue, int countValue)
        {
            icon.sprite = iconValue;
            count.text = countValue.ToString();
            checkMark.SetActive(false);
            if (defaultWidth == 0)
                defaultWidth = rectTransform.sizeDelta.x;

            rectTransform.sizeDelta = new Vector2(defaultWidth, rectTransform.sizeDelta.y);
            count.gameObject.SetActive(countValue > 0);
            countVFX.Initialize();
        }


        public void Refresh(int value)
        {
            count.text = value.ToString();

            if (value == 0)
                PlayDissapear();
        }


        public void ResetState()
        {
            root.localScale = Vector3.one;
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        public void PlayCountFx()
        {
            PlayCountParticles();

            if (!bounceTween.TryRewind())
            {
                bounceTween = BroTween.ScaleByCurve(root, Vector3.one, bounceDuration, bounceCurve).SetAutoKill(false).ToSafe();
            }

            bounceTween.Play();
        }


        private void PlayDissapear()
        {
           var  seq = BroTween.Sequence().SetUpdate(true);
            seq.AppendCallback(this, target => target.SwitchVisualOnComplete());
            seq.AppendInterval(0.5f);
            seq.Append(GetScaleTween());
            seq.Append(GetSizeDeltaSequence());
            seq.AppendCallback(this, target => target.SetObjectActive(false));
            
            shrinkSequence = seq.ToSafe();
            shrinkSequence.Play();
        }


        private void PlayCountParticles()
        {
            countVFX.Play();
        }


        private void SwitchVisualOnComplete()
        {
            count.gameObject.SetActive(false);
            checkMark.SetActive(true);
            AudioService.I.PlaySfx(SfxType.TaskSlotComplete);
        }


        private ScaleTween GetScaleTween()
        {
            return BroTween.Scale(root, Vector3.zero, scaleDuration).SetEase(scaleCurve);
        }

        
        private BroSequence GetSizeDeltaSequence()
        {
            BroSequence seq = BroTween.Sequence();
            FloatTween floatTween = BroTween.Float(this, (target, _) => target.ObserveShrink(), shrinkDuration).SetEase(scaleCurve);
            seq.Insert(0, floatTween);
            var sizeDelta = BroTween.RectSizeDelta(rectTransform, new Vector2(0, rectTransform.sizeDelta.y), shrinkDuration).SetEase(scaleCurve);
            seq.Insert(0, sizeDelta);
            return seq;
        }


        private void ObserveShrink()
        {
            OnShrinkStep?.Invoke();
        }
        
        private void KillTweens()
        {
            bounceTween.Kill();
            shrinkSequence.Kill();
        }
    }
}