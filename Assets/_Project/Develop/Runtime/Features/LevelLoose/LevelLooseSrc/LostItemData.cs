using UnityEngine;

namespace Features.LevelLoose
{
    public class LostItemData
    {
        public LostItemType lostItemType;
        public Sprite icon;
        public string name;
        public string slotText;
        public bool needCross;
        public Sprite textBack;
        public bool needTextBack;
        public int priority;

        public LostItemData(LostItemType lostItemType, 
                            Sprite icon, 
                            string name, 
                            string slotText, 
                            bool needCross,
                            Sprite textBack,
                            bool needTextBack, 
                            int priority)
        {
            this.lostItemType = lostItemType;
            this.icon = icon;
            this.name = name;
            this.slotText = slotText;
            this.needCross = needCross;
            this.priority = priority;
            this.textBack = textBack;
            this.needTextBack = needTextBack;
        }
    }
}