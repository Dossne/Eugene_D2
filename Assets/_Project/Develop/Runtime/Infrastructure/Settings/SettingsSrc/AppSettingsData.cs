using System;

namespace Infrastructure.Settings
{
    [Serializable]
    public class AppSettingsData
    {
        public bool isMusicOn;
        public bool isSoundOn;
        public bool isHapticOn;
        public int graphicsLevel;
        public int fps;
    }
}