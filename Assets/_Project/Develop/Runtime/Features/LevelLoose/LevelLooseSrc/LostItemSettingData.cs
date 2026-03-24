using System;

namespace Features.LevelLoose
{
    [Serializable]
    public class LostItemSettingData
    {        
        public int priority;
        public bool needCross;
        public bool needTextBack;
        public string textBackId;
    }
}