using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Pool.Particles;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ExtraCollectableUi
{
    public class ExtraCollectableSlotUi : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private RectTransform root;
        [SerializeField] Image icon;
        [SerializeField] TextMeshProUGUI count;
        [SerializeField] private GameObject checkmark;
        [SerializeField] private float switchToCheckmarkDelay = 0.5f;

        [Header("Bounce")]
        [SerializeField] private AnimationCurve bounceCurve;
        [SerializeField] private float bounceDuration;
        [SerializeField] private ReusableParticleSystemUI countVFX;

        private BroTweenSafe bounceTween;

        public RectTransform IconRect => icon.rectTransform;
        public TextFormatType TextFormatType { get; private set; }


        public void Construct(Sprite value, TextFormatType textFormatType)
        {
            this.icon.sprite = value;
            this.TextFormatType = textFormatType;
        }


        public void Initialize()
        {
            root.localScale = Vector3.one;
            SwitchToText();
            countVFX.Initialize();
        }


        public void Deinitialize()
        {
            countVFX.Stop();
            bounceTween.Kill();
        }


        public void RefreshText(string value, bool switchToCheckmark)
        {
            count.text = value;

            if (switchToCheckmark)
                SwitchToCheckmarkAsync().Forget();
        }


        public BroTweenSafe PlayCountFx()
        {
            PlayCountParticles();

            if (!bounceTween.TryRewind())
            {
                bounceTween = BroTween.ScaleByCurve(root, Vector3.one, bounceDuration, bounceCurve).SetAutoKill(false).ToSafe();
            }

            bounceTween.Play();
            return bounceTween;
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }


        public void SetSiblingIndex(int idx)
        {
            transform.SetSiblingIndex(idx);
        }


        private void SetTextObjectActive(bool value)
        {
            count.gameObject.SetObjectActive(value);
        }


        private void SetCheckmarkObjectActive(bool value)
        {
            checkmark.SetObjectActive(value);
        }


        private async UniTaskVoid SwitchToCheckmarkAsync()
        {
            await UniTask.WaitForSeconds(switchToCheckmarkDelay, true, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            SetTextObjectActive(false);
            SetCheckmarkObjectActive(true);
        }


        private void SwitchToText()
        {
            SetTextObjectActive(true);
            SetCheckmarkObjectActive(false);
        }


        private void PlayCountParticles()
        {
            countVFX.Play();
        }


    }
}