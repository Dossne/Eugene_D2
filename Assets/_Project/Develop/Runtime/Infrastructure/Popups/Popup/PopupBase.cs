using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Popups
{
    public class PopupBase : MonoBehaviour
    {
        public event Action<PopupBase, PopupState> OnChangeState;
        
        [Header("Base")]
        [SerializeField] private OpenCloseAnimator openCloseAnimator;
        [SerializeField] protected Button[] closeButton;

        private GameObject rootGameObject;

        private bool isOpened;
        private bool isInit;

        public bool IsOpened => isOpened && rootGameObject.activeSelf;
        public virtual bool IsDisposeOnClosed => false;
        public bool IsInit => isInit;


        public void Initialize()
        {
            if (isInit)
                return;

            rootGameObject = gameObject;
            SetObjectActive(false);
            SetOpened(false);

            SubscribeButtons();
            openCloseAnimator.Initialize();
            openCloseAnimator.OnOpenComplete += OpenCloseAnimator_OnOpenComplete;
            openCloseAnimator.OnCloseComplete += OpenCloseAnimator_OnCloseComplete;

            OnInitialize();
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            UnsubscribeButtons();

            openCloseAnimator.OnOpenComplete -= OpenCloseAnimator_OnOpenComplete;
            openCloseAnimator.OnCloseComplete -= OpenCloseAnimator_OnCloseComplete;
            openCloseAnimator.Deinitialize();
            OnDeinitialize();
            isInit = false;
        }


        public void Dispose()
        {
            OnChangeState = null;
            OnDispose();
            Destroy(gameObject);
        }


        public virtual void Open()
        {
            if (isOpened)
                return;

            HandleBeginOpenState();
            openCloseAnimator.Open();
        }


        public UniTask OpenAsync(CancellationToken token)
        {
            if (isOpened)
                return UniTask.CompletedTask;

            HandleBeginOpenState();
            return openCloseAnimator.OpenAsync(token);
        }


        public void Close()
        {
            if (!isOpened)
                return;
            
            HandleBeginCloseState();
            openCloseAnimator.Close();
        }


        public UniTask CloseAsync(CancellationToken token)
        {
            if (!isOpened)
                return UniTask.CompletedTask;

            HandleBeginCloseState();
            return openCloseAnimator.CloseAsync(token);
        }


        /// <summary>
        /// Active, but not visible. For correct calculations before Open
        /// </summary>
        public void ActivateAsInvisible()
        {
            openCloseAnimator.ActivateAsInvisible();
        }


        public void InstantClose()
        {
            if (!isOpened)
                return;

            openCloseAnimator.InstantClose();
            HandleBeginCloseState();
            HandleClosedState();
        }


        protected virtual void OnInitialize() { }
        protected virtual void OnDeinitialize() { }
        protected virtual void OnDispose() { }
        protected virtual void OnBeginOpen() { }
        protected virtual void OnFinishOpen() { }
        protected virtual void OnBeginClose() { }
        protected virtual void OnFinishClose() { }
        protected virtual void AnimationStartHandler() { } //OnStartOpen or OnStartClose
        protected virtual void AnimationStopHandler() { } //OnCompleteOpen or OnCompleteClose


        private void SetOpened(bool value)
        {
            isOpened = value;
        }


        private void SetObjectActive(bool value)
        {
            rootGameObject.SetObjectActive(value);
        }


        protected virtual void SubscribeButtons()
        {
            for (int i = 0; i < closeButton.Length; i++)
            {
                closeButton[i].onClick.AddListener(Close);
            }
        }


        protected virtual void UnsubscribeButtons()
        {
            for (int i = 0; i < closeButton.Length; i++)
            {
                closeButton[i].onClick.RemoveListener(Close);
            }
        }


        private void HandleBeginOpenState()
        {
            SetObjectActive(true);
            SetOpened(true);
            OnBeginOpen();
            AnimationStartHandler();
            OnChangeState?.Invoke(this, PopupState.BeginOpen);
        }


        private void HandleOpenedState()
        {
            OnFinishOpen();
            AnimationStopHandler();
            OnChangeState?.Invoke(this, PopupState.Opened);
        }


        private void HandleBeginCloseState()
        {
            OnBeginClose();
            AnimationStartHandler();
            OnChangeState?.Invoke(this, PopupState.BeginClose);
        }


        private void HandleClosedState()
        {
            SetOpened(false);
            OnFinishClose();
            AnimationStopHandler();
            OnChangeState?.Invoke(this, PopupState.Closed);
        }


        private void OpenCloseAnimator_OnOpenComplete()
        {
            HandleOpenedState();
        }


        private void OpenCloseAnimator_OnCloseComplete()
        {
            HandleClosedState();
        }
    }
}