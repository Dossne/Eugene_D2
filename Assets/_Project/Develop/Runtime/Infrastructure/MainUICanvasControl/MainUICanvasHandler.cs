using Features.Banner;
using Infrastructure.Ads;
using Infrastructure.QualityGraphicsControl;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.MainUICanvasControl
{
    /// <summary>
    /// Apply safe area, bottom banner size, etc
    /// </summary>
    public class MainUICanvasHandler
    {
        private readonly BannerController bannerController;
        private readonly GraphicsQualityHandler graphicsQualityHandler;
        private readonly RectTransform safeAreaTransform;
        private readonly Canvas mainUICanvas;
        private readonly CanvasScaler mainUICanvasScaler;

        private Rect safeAreaDefault;
        private float screenWidthDefault = 1080f;
        private float screenHeightDefault = 1920f;
        private float pixelRectWidthDefault;
        private float pixelRectHeightDefault;

        private bool isInit;


        public MainUICanvasHandler(MainUIProvider uiProvider,
                                   GraphicsQualityHandler graphicsQualityHandler,
                                   BannerController bannerController)
        {
            safeAreaTransform = uiProvider.SafeAreaTransform;
            mainUICanvas = uiProvider.MainUICanvas;
            mainUICanvasScaler = uiProvider.MainUICanvasScaler;

            this.graphicsQualityHandler = graphicsQualityHandler;
            this.bannerController = bannerController;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            CacheDefaultValues();

            bannerController.OnBannerStateChange += RefreshBannerState;
            RefreshBannerState();
            isInit = true;
        }


        public void RefreshBannerState()
        {
            bool isPremium = Advertisement.IsPremium();
            SetSafeAreaAndBanner(isPremium || !bannerController.IsBannerOn);
        }


        /// <summary>
        /// Diff between current and default screen ratio
        /// </summary>
        private float GetDiffScreenRatio()
        {
            var defaultScreenRatio = screenWidthDefault / screenHeightDefault;
            var currentScreenRatio = Screen.width / (float)Screen.height;
            var result = 1 - (defaultScreenRatio - currentScreenRatio);
            return result == 0 ? 1 : result;
        }


        /// <summary>
        /// Get corrective coefficients, which represents current screen width/height relative to main canvas aspect ratio 1080*1920
        /// </summary>
        /// <returns>correctiveCoefW - Screen width corrective coefficient relative to main canvas width 1080</returns>
        /// <returns>correctiveCoefH - Screen height corrective coefficient relative to main canvas height 1920</returns>
        public Vector2 GetCorrectiveCoefficients()
        {
            var screenRatioCoef = GetDiffScreenRatio();
            var correctiveCoefW = Screen.width / screenWidthDefault * screenRatioCoef;
            var correctiveCoefH = Screen.height / screenHeightDefault * screenRatioCoef;
            return new Vector2(correctiveCoefW, correctiveCoefH);
        }


        private void SetSafeAreaAndBanner(bool hasBannerOff)
        {
            float height = hasBannerOff ? 0.0f : GetPlatformDependentBannerSize(bannerController.BannerSize);

            ApplySafeArea(hasBannerOff);
            safeAreaTransform.offsetMin = new Vector2(safeAreaTransform.offsetMin.x, height);
        }


        private float GetPlatformDependentBannerSize(float bannerHeight)
        {
#if UNITY_EDITOR
            return bannerHeight;

#elif UNITY_IOS || UNITY_IPHONE
        return GetBannerSizeIOS(bannerHeight);

#elif UNITY_ANDROID
        return GetBannerSizeAndroid(bannerHeight);

#else
        return bannerHeight;
#endif
        }


        /// <summary>
        /// Recalculate height for IOS devices. See UNITY_IOS regions
        /// </summary>
        private float GetBannerSizeIOS(float bannerHeight)
        {
            if (bannerHeight <= 0)
                return 0;

            var resolutionCoeff = graphicsQualityHandler.GetCurrentResolutionScale();
            var screenCoeff = screenHeightDefault * resolutionCoeff / Screen.currentResolution.height;

            return bannerHeight * screenCoeff + 50;
        }


        /// <summary>
        /// Recalculate height for ANDROID devices. See UNITY_ANDROID region
        /// </summary>
        private float GetBannerSizeAndroid(float bannerHeight)
        {
            if (bannerHeight <= 0)
                return 0;
            
            return bannerHeight + 5f;
        }


        /// <summary>
        /// Sets safe area for device. Do not remove method
        /// </summary>
        private void ApplySafeArea(bool isPremium)
        {
            if (safeAreaTransform == null)
                return;

            SetSafeAreaTop(safeAreaDefault);
            SetSafeAreaBottom(safeAreaDefault, isPremium);
        }


        private void SetSafeAreaTop(Rect safeArea)
        {
            float iosTopExpand = 0.0f;

#if UNITY_IPHONE || UNITY_IOS
            const float topUnsafeAreaFactor = 0.30f;
            float topUnsafeAreaHeight = Mathf.Max(pixelRectHeightDefault - safeArea.y - safeArea.height, 0.0f);
            iosTopExpand = topUnsafeAreaHeight * topUnsafeAreaFactor;
#endif

            Vector2 anchorMax = safeArea.position + safeArea.size + new Vector2(0.0f, iosTopExpand);
            anchorMax.x /= pixelRectWidthDefault;
            anchorMax.y /= pixelRectHeightDefault;
            safeAreaTransform.anchorMax = anchorMax;
        }


        private void SetSafeAreaBottom(Rect safeArea, bool isPremium)
        {
            Vector2 anchorMin = safeArea.position;
            anchorMin.x /= pixelRectWidthDefault;

            if (!isPremium) //bottom safe area is calculated by SayKit.GetBackgroundBannerSize()
                anchorMin.y = 0f;
            else
                anchorMin.y /= pixelRectHeightDefault;

            safeAreaTransform.anchorMin = anchorMin;
        }


        private void CacheDefaultValues()
        {
            safeAreaDefault = Screen.safeArea;
            screenWidthDefault = mainUICanvasScaler.referenceResolution.x;
            screenHeightDefault = mainUICanvasScaler.referenceResolution.y;
            pixelRectWidthDefault = mainUICanvas.pixelRect.width;
            pixelRectHeightDefault = mainUICanvas.pixelRect.height;
        }
    }
}