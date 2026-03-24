using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Features.LevelComplete;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Features.HudLevelButtons
{
    public class StartLevelButtonView : MonoBehaviour
    {
        [Serializable]
        public class ButtonView
        {
            public Button button;
            public TextMeshProUGUI label;
            public TextMeshProUGUI difficultyLabel;
            public Transform rewardMultiplierTarget;
            public List<GameObject> shineBgFx;

            public Transform Transform => button == null ? null : button.transform;


            public void SetObjectActive(bool value)
            {
                button.gameObject.SetObjectActive(value);
            }


            public void AddListener(in UnityAction action)
            {
                button.onClick.AddListener(action);
            }


            public void RemoveListener(in UnityAction action)
            {
                button.onClick.RemoveListener(action);
            }


            public void SetShineFxActive(bool value)
            {
                foreach (GameObject fx in shineBgFx)
                {
                    fx.SetObjectActive(value);
                }
            }
        }

        public event Action OnClicked;
        [SerializeField] private SerializedDictionary<LevelDifficulty, ButtonView> buttons;
        [SerializeField] private GameObject rewardMultiplierRoot;

        private ButtonView currentView;
        private LevelDifficulty difficulty;

        private int nextLevel;
        private bool isWinStreakMaxLevel;
        private bool isInit;


        public void Construct(int nextLevel, LevelDifficulty difficulty, int rewardMultiplier, bool isWinStreakMaxLevel)
        {
            this.nextLevel = nextLevel;
            this.difficulty = difficulty;
            this.isWinStreakMaxLevel = isWinStreakMaxLevel;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            InitializeCurrentButton();
            SetObjectActive(true);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            currentView.RemoveListener(ButtonClick);

            OnClicked = null;
            SetObjectActive(false);
            isInit = false;
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }


        private void ButtonClick()
        {
            BroTween.ClickBounceWithCallBack(currentView.button, currentView.Transform, this, target => target.StartClickInvoke())
                    .Play();
        }


        private void InitializeCurrentButton()
        {
            
            foreach (var entryPair in buttons)
            {
                if (difficulty == entryPair.Key)
                {
                    currentView = entryPair.Value;
                    string dText = string.Empty;
                    switch (difficulty)
                    {
                        case LevelDifficulty.Hard:
                            dText = LocalizationService.I.Get(LocKeys.MetaHud.DifficultyHard);
                            break;
                        case LevelDifficulty.VeryHard:
                            dText = LocalizationService.I.Get(LocKeys.MetaHud.DifficultyVeryHard);
                            break;
                        case LevelDifficulty.Insane:
                            dText = LocalizationService.I.Get(LocKeys.MetaHud.DifficultyInsane);
                            break;
                    }
                    if (currentView.difficultyLabel != null)
                        currentView.difficultyLabel.text = dText;
                }                    
                else
                    entryPair.Value.SetObjectActive(false);
            }

            SetRewardMultiplier();
            currentView.label.text = LocalizationService.I.Get(LocKeys.MetaHud.StartGame, nextLevel.ToString());            
            currentView.AddListener(ButtonClick);
            currentView.SetObjectActive(true);
        }


        private void SetRewardMultiplier()
        {
            currentView.SetShineFxActive(isWinStreakMaxLevel);
            rewardMultiplierRoot.SetObjectActive(isWinStreakMaxLevel);

            if (isWinStreakMaxLevel)
                rewardMultiplierRoot.transform.position = currentView.rewardMultiplierTarget.position;
        }


        private void StartClickInvoke()
        {
            OnClicked?.Invoke();
        }
    }
}