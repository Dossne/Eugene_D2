using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.PlayerProfile
{
    public class PlayerProfileStatSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI valueText;

        public void Setup(Sprite sprite, string name, string value) 
        {
            icon.sprite = sprite;
            nameText.text = name;
            valueText.text = value;
        }
    }
}