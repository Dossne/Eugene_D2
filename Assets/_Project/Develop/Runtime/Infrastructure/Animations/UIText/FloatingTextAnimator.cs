using System;
using UnityEngine;

namespace Infrastructure.Animations
{
    [Serializable]
    public class ChangeHpAnimation
    {
        [Header("Show. Scale")]
        public AnimationCurve showScaleCurve;
        public float showScaleDuration;

        [Header("Show. Move")]
        public AnimationCurve showMoveSpeedCurve;
        public Vector3 showPosOffset;
        public float showMoveDuration;

        [Header("Stay")]
        public float stayDuration;

        [Header("Hide. Move")]
        public AnimationCurve hideMoveSpeedCurve;
        public Vector3 hidePosOffset;
        public float hideMoveDuration;

        [Header("Hide.FadeOut")]
        public AnimationCurve fadeoutCurve;
        public float fadeoutAlpha;
        public float fadeoutDuration;
    }

    public class FloatingTextAnimator : FloatingTextAnimatorBase
    {
        [Serializable]
        private enum State
        {
            None = 0,
            Show = 1,
            Stay = 2,
            Hide = 3
        }

        [SerializeField] private ChangeHpAnimation config;
        [SerializeField] private RectTransform animatingRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Debug. Do not change")]
        [SerializeField] private float showScaleTime;
        [SerializeField] private float moveTime;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private float stayTime;
        [SerializeField] private float fadeTime;
        [SerializeField] private State state;


        public override float TotalTime => config.showScaleDuration + config.showMoveDuration + config.stayDuration + config.hideMoveDuration + config.fadeoutDuration;
        
        private void Update()
        {
            switch (state)
            {
                case State.Show:
                    DoShow(Time.unscaledDeltaTime);
                    break;
                case State.Stay:
                    DoStay(Time.unscaledDeltaTime);
                    break;
                case State.Hide:
                    DoHide(Time.unscaledDeltaTime);
                    break;
            }
        }


        public override void StartPlay()
        {
            ResetState();
            EnterState(State.Show);
            SetUpdateEnabled(true);
        }


        public override void ForceStop()
        {
            StopPlay();
        }

        public override void ResetState()
        {
            EnterState(State.None);
            animatingRoot.localScale = Vector3.one;
            animatingRoot.localPosition = Vector3.zero;
            canvasGroup.alpha = 1;
        }

        private void EnterState(State newState)
        {
            if (newState == state)
            {
                return;
            }

            state = newState;

            switch (state)
            {
                case State.Show:
                    InitShowState();
                    break;
                case State.Stay:
                    InitStayState();
                    break;
                case State.Hide:
                    InitHideState();
                    break;

            }
        }

#region Show

        private void DoShow(float deltaTime)
        {
            bool isScaling = Show_TryScale(deltaTime);
            bool isMoving = Show_TryMove(deltaTime);

            if (!isScaling && !isMoving)
            {
                EnterState(State.Stay);
            }
        }


        private void InitShowState()
        {
            animatingRoot.localScale = Vector3.one * config.showScaleCurve.Evaluate(0f);
            showScaleTime = 0f;

            moveTime = 0f;
            startPosition = animatingRoot.localPosition;
            targetPosition = animatingRoot.localPosition + config.showPosOffset;
        }


        private bool Show_TryScale(float deltaTime)
        {
            if (showScaleTime >= config.showScaleDuration)
            {
                return false;
            }

            showScaleTime += deltaTime;
            float normalized = Mathf.Clamp01(showScaleTime / config.showScaleDuration);
            Vector3 curveValue = config.showScaleCurve.Evaluate(normalized) * Vector3.one;
            animatingRoot.localScale = Vector3.Lerp(animatingRoot.localScale, curveValue, normalized);
            return true;
        }


        private bool Show_TryMove(float deltaTime)
        {
            if (moveTime >= config.showMoveDuration)
            {
                return false;
            }

            moveTime += deltaTime;
            float normalized = Mathf.Clamp01(moveTime / config.showMoveDuration);
            float curveValue = config.showMoveSpeedCurve.Evaluate(normalized);
            animatingRoot.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            return true;
        }

        #endregion Show

        #region Stay

        private void DoStay(float deltaTime)
        {
            bool isWaiting = Stay_TryWait(deltaTime);

            if (!isWaiting)
            {
                EnterState(State.Hide);
            }
        }


        private void InitStayState()
        {
            stayTime = 0;
        }


        private bool Stay_TryWait(float deltaTime)
        {
            if (stayTime >= config.stayDuration)
            {
                return false;
            }

            stayTime += deltaTime;
            return true;
        }

        #endregion Stay

        #region Hide

        private void DoHide(float deltaTime)
        {
            bool isMoving = Hide_TryMove(deltaTime);
            bool isFading = Hide_TryFadeout(deltaTime);

            if (!isMoving && !isFading)
            {
                StopPlay();
            }
        }


        private void InitHideState()
        {
            moveTime = 0f;
            startPosition = animatingRoot.localPosition;
            targetPosition = animatingRoot.localPosition + config.hidePosOffset;
            fadeTime = 0f;
        }


        private bool Hide_TryMove(float deltaTime)
        {
            if (moveTime >= config.hideMoveDuration)
            {
                return false;
            }

            moveTime += deltaTime;
            float normalized = Mathf.Clamp01(moveTime / config.hideMoveDuration);
            float curveValue = config.hideMoveSpeedCurve.Evaluate(normalized);
            animatingRoot.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            return true;
        }


        private bool Hide_TryFadeout(float deltaTime)
        {
            if (fadeTime >= config.fadeoutDuration)
            {
                return false;
            }

            fadeTime += deltaTime;
            float normalized = Mathf.Clamp01(fadeTime / config.fadeoutDuration);
            float curveValue = config.fadeoutCurve.Evaluate(normalized);
            canvasGroup.alpha = Mathf.Lerp(1, config.fadeoutAlpha, curveValue);
            return true;
        }

        #endregion

#if UNITY_EDITOR
        [TriInspector.Button, TriInspector.ShowInPlayMode]
        private void TestPlay()
        {
            StartPlay();
        }
#endif
    }
}
