using UnityEngine;

namespace Features.FeatureUnlock
{
    [CreateAssetMenu(fileName = "FeatureUnlockConfiguration", menuName = "Config/Game/FeatureUnlockConfiguration")]
    public class FeatureUnlockConfiguration : ScriptableObject
    {
        [Header("NoAds")]
        public bool noAdsWidgetUnlocked;

        [Header("NoAds")]
        public bool shopWidgetUnlocked;

        [Header("SuperDiscount")]
        public bool superDiscountWidgetUnlocked;

        [Header("Resurrect")]
        public bool firstBombResurrectPopupUnlocked;
    }
}