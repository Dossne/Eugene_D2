using Cysharp.Threading.Tasks;
using Infrastructure.Configs;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using System;
using UnityEngine;

namespace Features.ScrollList
{
    public abstract class ScrollElement<T> : MonoBehaviour
    {
        [Header("ScrollElement Base")]
        [SerializeField] protected RectTransform elementRect;
        [SerializeField] protected RectTransform rootRect;
        [SerializeField] private bool checkPositionInViewport = true;
        
        protected T datasource = default;
        protected SpriteAtlasService spriteAtlasService;
        protected ConfigProvider configProvider;
        private RectTransform viewportRect;

        public Action<T> OnShownInViewport;
        private bool isInsideScreen = false;
        private bool isLayoutCalculated = false;
        private Vector2 anchoredPosDefault;


        public Action<T, RectTransform> OnClicked;

        public T ElementDatasource => datasource;

        public RectTransform ElementRect => elementRect;
        
        public float RectHeight => elementRect.rect.height;
        public bool IsInitialized { get; private set; }
        public Vector2 AnchoredPosDefault => anchoredPosDefault;

        public void Construct(T datasource, SpriteAtlasService spriteAtlasService, ConfigProvider configProvider) 
        {
            this.datasource = datasource;
            this.spriteAtlasService = spriteAtlasService;
            this.configProvider = configProvider;
            
        }

        public void SetViewportRect(RectTransform viewportRect) 
        {
            this.viewportRect = viewportRect;
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            InitializeImpl();

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            DeinitializeImpl();

            IsInitialized = false;
        }

        public void SetAnchoredPosition(Vector2 value)
        {
            elementRect.anchoredPosition = value;
            SetAnchoredPositionDefault(value);
        }

        public void SetAnchoredPositionDefault(Vector2 value)
        {
            anchoredPosDefault = value;
        }
        
        public void SetCheckPositionInViewPortEnabled(bool value)
        {
            checkPositionInViewport = value;
        }
        
        protected abstract void InitializeImpl();

        protected abstract void DeinitializeImpl();

        protected void HandleClick() 
        {
            OnClicked?.Invoke(datasource, rootRect);
        }

        public void EnableViewportCalculation()
        {
            if(!checkPositionInViewport)
                return;
            
            WaitingForLayoutCalculated().Forget();
        }

        private void LateUpdate()
        {
            CheckViewportPosition();
        }

        private async UniTask WaitingForLayoutCalculated()
        {
            isInsideScreen = false;
            isLayoutCalculated = false;
            await UniTask.NextFrame();
            isLayoutCalculated = true;
        }


        private void CheckViewportPosition()
        {
            if(!checkPositionInViewport)
                return;
            
            if (!isLayoutCalculated)
                return;

            if (isInsideScreen == elementRect.Overlaps(viewportRect))
                return;

            isInsideScreen = !isInsideScreen;

            if (!isInsideScreen)
                return;

            OnShownInViewport?.Invoke(datasource);
        }
    }
}