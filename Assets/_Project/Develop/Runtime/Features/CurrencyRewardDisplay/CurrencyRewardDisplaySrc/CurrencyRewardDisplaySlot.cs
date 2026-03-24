using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CurrencyRewardDisplaySlot : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI amountTMP;



    public void Construct(Sprite sprite, int amount)
    {
        icon.sprite = sprite;
        amountTMP.text = amount.ToString();
    }
}
