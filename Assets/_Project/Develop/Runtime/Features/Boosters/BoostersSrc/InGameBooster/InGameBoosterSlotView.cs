using System;
using Infrastructure.BroTweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Boosters
{
    public class InGameBoosterSlotView : MonoBehaviour
    {
        [Serializable]
        private class AnimArgs
        {
            [Header("Bounce")]
            public float tapScale = 0.8f;
            public AnimationCurve downCurve;
            public float downDuration = 0.2f;
            public AnimationCurve upCurve;
            public float upDuration = 0.1f;

            [Header("Shake")]
            public float shakeDuration = 0.3f;
            public float shakeStrength = 1f;
            public int shakeVibrato = 10;
        }

        public event Action<BoosterType> OnClick;

        [Header("Main")]
        [SerializeField] private Button button;
        [SerializeField] private CounterView counter;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject lockBg;
        [SerializeField] private TextMeshProUGUI lockText;
        [SerializeField] private Image progressBar;

        [SerializeField] private GameObject freeBg;
        [SerializeField] private TextMeshProUGUI freeText;

        [Header("Button click animation")]
        [SerializeField] private Transform iconAnimRoot;
        [SerializeField] private AnimArgs animArgs;

        [Header("Debug")]
        [SerializeField] private BroTweenSafe bounceSeq;
        [SerializeField] private BroTweenSafe shakeLoop;
        private BoosterType boosterType;
        private bool isInit;


        public void Construct(BoosterType boosterType, Sprite icon)
        {
            this.boosterType = boosterType;
            this.icon.sprite = icon;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            button.onClick.AddListener(ButtonClick);
            SetProgressSlider(0);
            SetButtonEnabled(true);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            bounceSeq.Kill();
            shakeLoop.Kill();
            button.onClick.RemoveListener(ButtonClick);
            OnClick = null;

            isInit = false;
        }


        public void Destroy()
        {
            Destroy(gameObject);
        }


        public void RefreshState(BoosterStateType state, string counterText, string lockText, string freeText)
        {
            SetFreeState(freeText, state == BoosterStateType.Free);
            SetLockState(lockText, state == BoosterStateType.Locked);
            counter.SetState(state, counterText);
        }


        public void SetProgressSlider(float progress)
        {
            progressBar.fillAmount = progress;
        }


        public void SetButtonEnabled(bool value)
        {
            button.enabled = value;
        }


        public void StartShakeIcon()
        {
            shakeLoop = BroTween.SequenceLoop(-1)
                                .Append(GetShakePos())
                                .OnComplete(this, target => target.ResetIconPosition())
                                .ToSafe();

            shakeLoop.Play();
        }


        private ShakePositionTween GetShakePos()
        {
            return BroTween.ShakePosition(iconAnimRoot, new Vector3(animArgs.shakeStrength, animArgs.shakeStrength, 0.0f), 
                                                         in animArgs.shakeDuration, 
                                                         true, 
                                                         in animArgs.shakeVibrato);
        }


        public void StopShakeIcon()
        {
            shakeLoop.Kill();
        }


        private void SetLockState(string lockText, bool isLocked)
        {
            if (isLocked)
                this.lockText.text = lockText;

            if (lockBg.activeSelf == isLocked)
                return;

            lockBg.SetActive(isLocked);
        }


        private void SetFreeState(string freeText, bool isFree)
        {
            if (isFree)
                this.freeText.text = freeText;

            if (freeBg.activeSelf == isFree)
                return;

            freeBg.SetActive(isFree);
        }


        private void ButtonClick()
        {
            bounceSeq.Kill();

            Vector3 defaultScale = Vector3.one;
            Vector3 targetScale = Vector3.one * animArgs.tapScale;


            bounceSeq = BroTween.Sequence()
                                .Append(GetScaleDown(in defaultScale, in targetScale))
                                .Append(GetScaleUp(in targetScale, in defaultScale))
                                .SetUpdate(true)
                                .ToSafe();
            bounceSeq.Play();
        }


        private ScaleTween GetScaleUp(in Vector3 targetScale, in Vector3 defaultScale)
        {
            return BroTween.Scale(iconAnimRoot, in targetScale, in defaultScale, animArgs.upDuration)
                           .SetEase(animArgs.upCurve);
        }


        private BroTweenBase GetScaleDown(in Vector3 defaultScale, in Vector3 targetScale)
        {
            return BroTween.Scale(iconAnimRoot, in defaultScale, in targetScale, animArgs.downDuration)
                           .SetEase(animArgs.downCurve)
                           .OnComplete(this, target => target.ClickInvoke());
        }


        private void ClickInvoke()
        {
            OnClick?.Invoke(boosterType);
        }


        private void ResetIconPosition()
        {
            iconAnimRoot.localPosition = Vector3.zero;
        }
    }
}