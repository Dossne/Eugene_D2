#if PR_CHEAT
using System.Collections.Generic;
using Features.WinStreak;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat
{
    public class WinStreakPanel : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private Button addBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private List<Button> closeBtns;

        private WinStreakStateController winStreakStateController;
        public bool IsInit => isInit;

        private bool isInit;


        [Inject]
        public void Construct(WinStreakStateController winStreakStateController)
        {
            this.winStreakStateController = winStreakStateController;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            addBtn.onClick.AddListener(AddWs);
            resetBtn.onClick.AddListener(ResetWs);

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
            stateText.text = $"Current level: {winStreakStateController.CurrentLevel}";
        }


        private void AddWs()
        {
            winStreakStateController.CheatAdd();
            RefreshState();
        }


        private void ResetWs()
        {
            winStreakStateController.CheatReset();
            RefreshState();
        }


        private void Close()
        {
            SetObjectActive(false);
        }
    }
}
#endif