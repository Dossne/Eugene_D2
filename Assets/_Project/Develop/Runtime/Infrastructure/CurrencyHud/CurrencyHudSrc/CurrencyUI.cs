using System;
using Infrastructure.AudioControl;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.CurrencyHud
{
    public class CurrencyUI : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private Image icon;
        [SerializeField] private Image iconPlus;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text additionalLabel;
        [SerializeField] private Button button;
        [SerializeField] private GameObject infinityIcon;
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform iconTarget;
        [SerializeField] private bool isClickable;

        private ParticlesPool particlesPool;
        private int value;
        private Action callback;
        private BroTweenSafe clickBounceSeq;
        private BroTweenSafe bounceTween;
        private bool isInit;

        public int Value => value;
        public RectTransform IconTransform => iconTarget;


        [Inject]
        public void Construct(PoolService poolService)
        {
            this.particlesPool = poolService.Get<ParticlesPool>();
        }


        public void Construct(Sprite icon, int value, string additionalText, Action clickCallback)
        {
            this.icon.sprite = icon;
            this.callback = clickCallback;
            RefreshValue(value, false, ActionType.Set);
            RefreshAdditionalText(additionalText, false);
        }


        public void Initialize()
        {
            if (isInit)
                return;

            isClickable = true;
            InitializeButton();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            isClickable = false;
            DeinitializeButton();

            isInit = false;
        }


        public void DestroyObject()
        {
            Destroy(gameObject);
        }


        public void RefreshValue(int value, bool animated, ActionType actionType)
        {
            if (actionType == ActionType.Set)
                this.value = value;
            if (actionType == ActionType.Add)
                this.value += value;
            if (actionType == ActionType.Remove)
                this.value -= value;

            label.text = this.value.ToString();

            if (animated)
            {
                BounceIcon();
            }
        }


        public void SetClickable(bool isClickable)
        {
            if (isClickable == this.isClickable)
                return;

            this.isClickable = isClickable;

            if (isClickable)
                InitializeButton();
            else
                DeinitializeButton();

        }


        public void RefreshAdditionalText(string text, bool animated)
        {
            additionalLabel.text = text;

            if (animated)
            {
                BounceIcon();
            }
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        public void SetLabelActive(bool isActive)
        {
            GameObject labelGO = label.gameObject;
            if (labelGO.activeSelf != isActive)
            {
                labelGO.SetActive(isActive);
            }
        }


        public void SetInfinityIconActive(bool isActive)
        {
            if (infinityIcon.activeSelf != isActive)
            {
                infinityIcon.SetActive(isActive);
            }
        }


        private void BounceIcon()
        {
            bounceTween.Kill();
            bounceTween = BroTween.Bounce(root)
                                  .SetUpdate(true)
                                  .ToSafe();
            bounceTween.Play();
            PlayCountParticles();
        }


        private void InitializeButton()
        {
            button.enabled = isClickable;
            iconPlus.gameObject.SetActive(isClickable);
            
            if (isClickable)
            {
                button.onClick.RemoveListener(OnButtonClick);
                button.onClick.AddListener(OnButtonClick);
            }
        }


        private void DeinitializeButton()
        {
            button.enabled = false;
            iconPlus.gameObject.SetActive(false);
            
            clickBounceSeq.Kill();
            bounceTween.Kill();
            
            button.onClick.RemoveListener(OnButtonClick);
        }


        private void OnButtonClick()
        {
            if (!clickBounceSeq.TryRewind())
            {
                clickBounceSeq = BroTween.ClickBounceWithCallBack(button, root, in callback).SetAutoKill(false).ToSafe();
            }

            clickBounceSeq.Play();
            AudioService.I.PlaySfx(SfxType.ClickUI);
        }


        private void PlayCountParticles()
        {
            if (particlesPool.TryGetItem(PoolableParticleType.CurrencyCountSparks, out PoolableParticleSystem fx))
            {
                fx.transform.position = iconTarget.position;
            }
        }
    }
}