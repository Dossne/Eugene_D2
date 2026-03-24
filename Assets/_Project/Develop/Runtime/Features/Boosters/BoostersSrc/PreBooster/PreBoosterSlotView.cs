using System;
using Infrastructure.BroTweens;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Boosters
{
    public class PreBoosterSlotView : MonoBehaviour
    {
        public event Action<BoosterType, bool /*isSelected*/> OnClick;

        [Serializable]
        private class AnimArgs
        {
            public float tapScale = 1.1f;
            public AnimationCurve downCurve;
            public float downDuration = 0.15f;
            public AnimationCurve upCurve;
            public float upDuration = 0.25f;
        }

        [Header("Main")]
        [SerializeField] private Button button;
        [SerializeField] private CounterView counter;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private Sprite selectedBgSprite;
        [SerializeField] private Sprite nonSelectedBgSprite;
        [SerializeField] private Sprite infiniteBgSprite;
        [SerializeField] private GameObject lockBg;
        [SerializeField] private TextMeshProUGUI lockText;
        [SerializeField] private GameObject timerBg;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject freeBg;
        [SerializeField] private TextMeshProUGUI freeText;

        [Header("Animation")]
        [SerializeField] private Transform scaleRoot;
        [SerializeField] private AnimArgs animArgs;

        private BoosterType boosterType;
        private BroTweenSafe clickTween;
        private bool isSelected;
        private bool isSelectable;
        private Timer timer = null;
        private bool isInit;



        private void Update()
        {
            if (!timerBg.activeSelf
                || timer.IsOff())
            {
                return;
            }

            if (!timer.IsOffAfterUpdate(Time.unscaledDeltaTime))
            {
                timerText.text = GetTimeString();
            }
        }


        public void Construct(BoosterType boosterType, Sprite icon)
        {
            this.boosterType = boosterType;
            this.icon.sprite = icon;

            timer = new Timer(0);
        }

        public void Initialize()
        {
            if (isInit)
                return;
            button.onClick.AddListener(ButtonClick);
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            button.onClick.RemoveListener(ButtonClick);
            OnClick = null;
            clickTween.Kill();
            
            isInit = false;
        }


        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void RefreshState(BoosterStateType state, string counterText, string lockText, string freeText, bool isSelectable, float time)
        {
            this.isSelectable = isSelectable;
            SetFreeState(freeText, state == BoosterStateType.Free);
            SetLockState(lockText, state == BoosterStateType.Locked);
            SetTimerView(time, state == BoosterStateType.Locked, state == BoosterStateType.Free);
            counter.SetState(state, counterText);
            isSelected = state == BoosterStateType.Selected;
            SetBgState(state);
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

        private void SetTimerView(float time, bool isLocked, bool isFree)
        {
            timer.Reset(time);
            timerBg.SetActive(!timer.IsOff() && !isLocked && !isFree);
            timerText.text = TimeUtils.GetTimeString(timer.TimeRest());
        }


        private string GetTimeString()
        {
            if(timer.TimeRest() < 3600)
            {
                return TimeUtils.GetTimeString(timer.TimeRest());
            }

            return TimeUtils.GetTimeString(timer.TimeRest(), lettersSizePerc: 100.0f);
        }


        private void SetState()
        {
            if (isSelectable)
            {
                isSelected = !isSelected;
            }

            OnClick?.Invoke(boosterType, isSelected);
        }

        private void ButtonClick()
        {
            Vector3 tapScale = Vector3.one * animArgs.tapScale;
            clickTween.Kill();
            clickTween = BroTween.Sequence()
                                 .SetUpdate(true)
                                 .Append(BroTween.Scale(scaleRoot, Vector3.one, tapScale, animArgs.downDuration).SetEase(animArgs.downCurve).OnComplete(SetState))
                                 .Append(BroTween.Scale(scaleRoot, tapScale, Vector3.one, animArgs.upDuration).SetEase(animArgs.upCurve))
                                 .ToSafe();
            clickTween.Play();
        }

        private void SetBgState(BoosterStateType state)
        {
            switch (state)
            {
                case BoosterStateType.Infinite:
                {
                    background.sprite = infiniteBgSprite;
                }
                break;
                case BoosterStateType.Selected:
                {
                    background.sprite = selectedBgSprite;
                }
                break;
                default:
                {
                    background.sprite = nonSelectedBgSprite;
                }
                break;
            }
        }
    }
}