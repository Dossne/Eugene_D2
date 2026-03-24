using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Popups
{
    public class PopupService
    {
        public enum PopupMode 
        {
            Popup  = 0,
            Screen = 1,
        }
        public event Action<PopupBase, PopupState> OnChangeState;

        private readonly PopupFactory popupFactory;
        private readonly RectTransform popupsParent;
        private readonly RectTransform screenParent;
        private readonly HashSet<PopupBase> openedList;
        private readonly Dictionary<Type, PopupBase> popupCache;
        private bool isBusy;


        public PopupService(PopupFactory popupFactory, RectTransform popupsParent, RectTransform screenParent)
        {
            this.popupFactory = popupFactory;
            this.popupsParent = popupsParent;
            this.screenParent = screenParent;
            this.openedList = new HashSet<PopupBase>();
            this.popupCache = new Dictionary<Type, PopupBase>();
        }


        public bool IsAnyOpened => openedList.Count > 0;


        public bool IsPopupOpened(Type popupType) 
        {
            if (!popupCache.TryGetValue(popupType, out PopupBase popup))
                return false;

            return popup.IsOpened;
        }


        public void Deinitialize()
        {
            foreach (KeyValuePair<Type, PopupBase> instance in popupCache)
            {
                if (instance.Value == null)
                    continue;

                instance.Value.OnChangeState -= PopupBase_OnChangeState;
                instance.Value.Deinitialize();
                instance.Value.Dispose();
            }

            popupCache.Clear();
            openedList.Clear();
        }


        public async UniTask<T> GetAsync<T>(CancellationToken token,
                                            bool inject = false,
                                            bool isInstantiateAsync = true,
                                            PopupMode popupMode = PopupMode.Popup) where T : PopupBase
        {
            if (isBusy)
                await UniTask.WaitUntil(IsReady, cancellationToken: token);

            isBusy = true;
            Type popupType = typeof(T);

            if (popupCache.TryGetValue(popupType, out PopupBase popup) && popup is T cachedInstance)
            {
                isBusy = false;
                return cachedInstance;
            }

            T instance = await popupFactory.CreateAsync<T>(GetParentTransformByMode(popupMode), token, inject, isInstantiateAsync);

            instance.OnChangeState += PopupBase_OnChangeState;

            popupCache.Add(popupType, instance);
            isBusy = false;

            return instance;
        }


        public async UniTask<T> OpenAsync<T>(CancellationToken token,                                             
                                             bool inject = false,
                                             bool isInstantiateAsync = true,
                                             PopupMode popupMode = PopupMode.Popup) where T : PopupBase
        {
            T popup = await GetAsync<T>(token, inject, isInstantiateAsync, popupMode);

            if (!popup.IsInit)
            {
                popup.Initialize();
            }

            popup.Open();

            return popup;
        }


        public void CloseActivePopups()
        {
            HashSet<PopupBase> closeTemp = new HashSet<PopupBase>(openedList);

            foreach (var popup in closeTemp)
            {
                popup.InstantClose();
            }

            openedList.Clear();
        }


        public void Dispose(PopupBase popup)
        {
            if (popup == null)
                return;

            Type popupType = popup.GetType();

            if (!popupCache.TryGetValue(popupType, out PopupBase instance))
                return;

            if (instance.IsOpened)
            {
                openedList.Remove(instance);
                instance.InstantClose();
            }

            popupFactory.ReleaseAsset(instance);

            instance.OnChangeState -= PopupBase_OnChangeState;
            instance.Deinitialize();
            instance.Dispose();
            popupCache.Remove(popupType);
        }

        private RectTransform GetParentTransformByMode(PopupMode popupMode)
        {
            return popupMode switch
            {
                PopupMode.Popup  => popupsParent,
                PopupMode.Screen => screenParent,
                _                => popupsParent,
            };
        }
        private bool IsReady()
        {
            return !isBusy;
        }


        private void HandleBeginOpenState(PopupBase popup)
        {
            openedList.Add(popup);
            popup.transform.SetAsLastSibling();
            OnChangeState?.Invoke(popup, PopupState.BeginOpen);
        }


        private void HandleOpenedState(PopupBase popup)
        {
            OnChangeState?.Invoke(popup, PopupState.Opened);
        }


        private void HandleBeginCloseState(PopupBase popup)
        {
            OnChangeState?.Invoke(popup, PopupState.BeginClose);
        }


        private void HandleClosedState(PopupBase popup)
        {
            if (!openedList.Remove(popup))
                return;

            OnChangeState?.Invoke(popup, PopupState.Closed);

            if (popup.IsDisposeOnClosed)
            {
                Dispose(popup);
            }
        }


        private void PopupBase_OnChangeState(PopupBase popup, PopupState state)
        {
            switch (state)
            {
                case PopupState.BeginOpen:
                    HandleBeginOpenState(popup);
                    break;
                case PopupState.Opened:
                    HandleOpenedState(popup);
                    break;

                case PopupState.BeginClose:
                    HandleBeginCloseState(popup);
                    break;

                case PopupState.Closed:
                    HandleClosedState(popup);
                    break;
            }

        }
    }
}