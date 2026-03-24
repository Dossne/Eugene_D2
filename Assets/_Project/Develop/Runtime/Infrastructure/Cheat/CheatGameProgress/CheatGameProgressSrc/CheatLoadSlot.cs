#if PR_CHEAT

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Infrastructure.Cheat.CheatGameProgress
{
    public class CheatLoadSlot : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI saveNameTmp;

        private Toggle toggle;
        private ToggleGroup toggleGroup;


        private void OnDestroy()
        {
            if(toggle != null)
                toggle.onValueChanged.RemoveAllListeners();
        }


        public void Init(string saveName, UnityAction<bool> OnChangeCallback)
        {
            toggle = GetComponent<Toggle>();
            toggleGroup = GetComponentInParent<ToggleGroup>();

            toggle.group = toggleGroup;
            saveNameTmp.text = saveName;

            toggle.onValueChanged.AddListener(OnChangeCallback);
        }

    }
}
#endif