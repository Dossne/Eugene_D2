using Lofelt.NiceVibrations;

namespace Infrastructure.HapticControl
{
    public class BakedHapticData
    {
        public float amplitude;
        public float frequency;
        public byte[] bytes;
        public GamepadRumble gamepadRumble;
        public bool isClip;
    }
}