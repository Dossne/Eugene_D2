using System;

namespace Infrastructure.HapticControl
{
    [Serializable]
    public class HapticAction
    {
        public HapticType hapticType = HapticType.Selection;
        public float activateDelaysSec;
    }
}