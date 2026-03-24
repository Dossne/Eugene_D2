using System;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    internal class SayKitAssets : ScriptableObject
    {
        private const string AssetName = "SayKitAssets";

        [SerializeField] private SayKitUI _sayKitUI;
        [SerializeField] private SayKitUI _sayKitUiLandscape;
        [SerializeField] private SayKitInterstitialCirclePopup _sayKitInterstitialCirclePopup;
        [SerializeField] private SayKitAdIdPopup _sayKitAdIdPopup;
        
#if SAYKIT_CHINA_VERSION
        [SerializeField] private SKAgeVerificationService _skAgeVerificationServicePopup;
#endif

        public SayKitUI SayKitUI =>
            Screen.orientation == ScreenOrientation.Portrait ||
            Screen.orientation == ScreenOrientation.PortraitUpsideDown
                ? _sayKitUI
                : _sayKitUiLandscape;

        public SayKitInterstitialCirclePopup SayKitInterstitialCirclePopup => _sayKitInterstitialCirclePopup;
        public SayKitAdIdPopup SayKitAdIdPopupPrefab => _sayKitAdIdPopup;
        
#if SAYKIT_CHINA_VERSION
        public SKAgeVerificationService SkAgeVerificationServicePopup => _skAgeVerificationServicePopup;
#endif
        #region Instancing

        [NonSerialized] // to keep instance out of Unity cache;
        private static SayKitAssets _instance;

        public static SayKitAssets Instance =>
            _instance ? _instance : (_instance = Resources.Load<SayKitAssets>(AssetName));

        #endregion
    }
}