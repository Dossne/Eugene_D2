#if PR_CHEAT
using System.Collections.Generic;
using Features.SuperSpeedMode;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat
{
    public class CheatSuperSpeedPanel : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private Button addBtn;
        [SerializeField] private Button maxBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private List<Button> closeBtns;

        private SuperSpeedController superSpeedController;
        public bool IsInit => isInit;

        private bool isInit;


        [Inject]
        public void Construct(SuperSpeedController superSpeedController)
        {
            this.superSpeedController = superSpeedController;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            addBtn.onClick.AddListener(AddCount);
            maxBtn.onClick.AddListener(MaxCount);
            resetBtn.onClick.AddListener(ResetCount);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.RemoveListener(Close);
            }

            addBtn.onClick.RemoveAllListeners();
            maxBtn.onClick.RemoveAllListeners();
            resetBtn.onClick.RemoveAllListeners();
            isInit = false;
        }


        public void SetObjectActive(bool value)
        {
            RefreshState();
            gameObject.SetObjectActive(value);
        }


        private void RefreshState()
        {
            stateText.text = $"Current level: {superSpeedController.WinCount}";
        }


        private void AddCount()
        {
            superSpeedController.CheatAdd();
            RefreshState();
        }

        private void MaxCount()
        {
            superSpeedController.CheatMax();
            RefreshState();
        }

        private void ResetCount()
        {
            superSpeedController.CheatReset();
            RefreshState();
        }


        private void Close()
        {
            SetObjectActive(false);
        }
    }
}
#endif