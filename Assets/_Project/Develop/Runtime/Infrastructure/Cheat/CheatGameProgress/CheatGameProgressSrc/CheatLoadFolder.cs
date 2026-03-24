#if PR_CHEAT

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Infrastructure.Cheat.CheatGameProgress
{
    public class CheatLoadFolder : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI folderNameTmp;
        private Toggle toggle;
    

        private void OnDestroy()
        {
            if(toggle != null)
                toggle.onValueChanged.RemoveAllListeners();
        }


        public void Init(string folderName, UnityAction<bool> OnChangeCallback)
        {
            folderNameTmp.text = folderName;
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnChangeCallback);
            toggle.isOn = false;
        }

    }
}
#endif