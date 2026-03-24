using System;
using System.Collections.Generic;
using Features.RewardTrack;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

#if PR_CHEAT
namespace Infrastructure.Cheat
{
    public class CheatRewardTrackPanel : MonoBehaviour
    {
        [SerializeField] private Button addBtn;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button addMaxBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private List<Button> closeBtns;

        private RewardTrackRequestEvent request;
        private CheatMainMenuPopup parentPopup;

        private bool isInit;

        public bool IsInit => isInit;


        [Inject]
        public void Inject(RewardTrackRequestEvent rewardTrackEvent)
        {
            this.request = rewardTrackEvent;
        }


        public void Construct(CheatMainMenuPopup parentPopup)
        {
            this.parentPopup = parentPopup;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            addBtn.onClick.AddListener(AddRT);
            addMaxBtn.onClick.AddListener(AddMaxRT);
            resetBtn.onClick.AddListener(ResetRT);

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
            addMaxBtn.onClick.RemoveAllListeners();
            resetBtn.onClick.RemoveAllListeners();

            isInit = false;
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }


        private void AddRT()
        {
            if (Int32.TryParse(inputField.text, out int result) && result > 0)
            {
                request.Request(new RTRequest
                {
                    action = RTRequestAction.Add,
                    addCount = result
                });
                
                Close();
                parentPopup.Close();
            }
        }


        private void AddMaxRT()
        {
            request.Request(new RTRequest
            {
                action = RTRequestAction.AddMax
            });

            Close();
            parentPopup.Close();
        }


        private void ResetRT()
        {
            request.Request(new RTRequest
            {
                action = RTRequestAction.Reset
            });

            Close();
            parentPopup.Close();
        }


        private void Close()
        {
            SetObjectActive(false);
        }

    }
}
#endif