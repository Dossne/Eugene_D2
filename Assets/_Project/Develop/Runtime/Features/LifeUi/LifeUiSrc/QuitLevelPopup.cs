using System;
using System.Collections.Generic;
using System.Linq;
using Features.LevelLoose;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LifeUi
{
    public class QuitLevelPopup : PopupBase
    {
        [Serializable]
        private class LoseItemRefs
        {
            [SerializeField] public GameObject loseItemSlot;
            [SerializeField] public LostItemIcon lostItemIcon;
        }

        public enum State
        {
            LoseLife = 0,
            LoseItems = 1,
        }

        [Header("Components")]
        [SerializeField] private Button quitBtn;
        [SerializeField] private Image icon;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;

        [Header("LostItems")]
        [SerializeField] private List<LoseItemRefs> loseItemRefs = new();
        [SerializeField] private GameObject loseStreakLayout;


        private Action<State> onQuitCallback;
        private State currentState = State.LoseLife;

        private Sprite defaultIcon = null; 


        public void Construct(Action<State> onQuitCallback)
        {
            this.onQuitCallback = onQuitCallback;
            defaultIcon = icon.sprite;
        }

        protected override void OnInitialize()
        {
            headerTxt.text = LocalizationService.I.Get(LocKeys.QuitLevelPopup.Header);
            buttonTxt.text = LocalizationService.I.Get(LocKeys.QuitLevelPopup.QuitButton);
            quitBtn.onClick.AddListener(StartButtonClick);
        }


        protected override void OnDeinitialize()
        {
            quitBtn.onClick.RemoveListener(StartButtonClick);
        }

        private void StartButtonClick()
        {
            BroTween.ClickBounceWithCallBack(quitBtn, quitBtn.transform, this, target => target.StartClickInvoke(), callbackOnEnd: true)
                    .Play();
        }


        private void StartClickInvoke()
        {
            onQuitCallback?.Invoke(currentState);
        }

        public void SetState(State loseStreak)
        {
            currentState = loseStreak;
            switch (currentState)
            {
                case State.LoseLife:
                    {
                        icon.enabled = true;
                        icon.sprite = defaultIcon;
                        descriptionTxt.text = LocalizationService.I.Get(LocKeys.QuitLevelPopup.LoseLifeText);
                        loseStreakLayout.SetActive(false);
                    }
                    break;
                case State.LoseItems:
                    {
                        icon.enabled = false;
                        loseStreakLayout.SetActive(true);
                    }
                    break;
            }
        }

        public void SetLostItems(List<LostItemData> lostItems)
        {
            var filtered = lostItems.Where(x => x.lostItemType != LostItemType.Lives).ToList();
            var lostCount = filtered.Count;

            if (filtered.Exists(x => x.lostItemType == LostItemType.Coins) && filtered.Count == 1)
            {
                string coinItemName = lostCount > 0 ? filtered[0].name : string.Empty;
                descriptionTxt.text = LocalizationService.I.Get(LocKeys.QuitLevelPopup.LoseCoins, coinItemName);
            }
            else
            {
                descriptionTxt.text = LocalizationService.I.Get(LocKeys.QuitLevelPopup.LoseAllItems);
            }

            for (int i = 0; i < loseItemRefs.Count; i++)
            {
                if (filtered.Count <= i)
                {
                    loseItemRefs[i].loseItemSlot.SetActive(false);
                    continue;
                }

                loseItemRefs[i].loseItemSlot.SetActive(true);
                loseItemRefs[i].lostItemIcon.Setup(filtered[i]);
            }
        }
    }
}