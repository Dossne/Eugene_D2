using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Features.LevelLoose
{
    public class LostItemIcon : MonoBehaviour
    {
        [SerializeField] private Image loseIcon;
        [SerializeField] private TextMeshProUGUI loseText;
        [SerializeField] private Image loseTextBackIcon;
        [SerializeField] private Image crossIcon;

        public void Setup(LostItemData lostItemData) 
        {
            loseIcon.sprite          = lostItemData.icon;
            loseText.text            = lostItemData.slotText;
            crossIcon.enabled        = lostItemData.needCross;
            loseTextBackIcon.enabled = lostItemData.needTextBack;
            loseTextBackIcon.sprite  = lostItemData.textBack;
        }
    }
}
