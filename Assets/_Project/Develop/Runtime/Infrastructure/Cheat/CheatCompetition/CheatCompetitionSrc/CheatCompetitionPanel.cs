#if PR_CHEAT

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Features.Competition;
using Features.ScoringVisualize;
using Features.WinStreak;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat
{
    public class CheatCompetitionPanel : MonoBehaviour
    {
        [SerializeField] private Button addBtn;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button addMaxBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private List<Button> closeBtns;

        private CompetitionManager manager;
        private ScoringVisualizeService scoringVisualizeService;
        private WinStreakStateController winStreakStateController;
        
        private CheatMainMenuPopup parentPopup;

        private bool isInit;

        public bool IsInit => isInit;

        [Inject]
        public void Inject(CompetitionManager manager, WinStreakStateController winStreakStateController, ScoringVisualizeService scoringVisualizeService)
        {
            this.manager = manager;
            this.winStreakStateController = winStreakStateController;
            this.scoringVisualizeService = scoringVisualizeService;
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

            addBtn.onClick.AddListener(ApplyWinWithCount);
            addMaxBtn.onClick.AddListener(ApplyWinWithMax);
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

        private void ApplyWinWithCount()
        {
            ApplyWinWithCountAsync().Forget();
        }

        private async UniTaskVoid ApplyWinWithCountAsync()
        {
            if (Int32.TryParse(inputField.text, out int result) && result > 0)
            {
                Close();
                parentPopup.Close();
                
                await manager.CheatApplyWinAsync(result, winStreakStateController.IsMaxLevel, winStreakStateController.RewardMultiplier, 1);
                scoringVisualizeService.ExecuteScheduledAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();

            }
        }

        private void ApplyWinWithMax()
        {
            ApplyWinWithMaxAsync().Forget();
        }

        
        private async UniTaskVoid ApplyWinWithMaxAsync()
        {
            Close();
            parentPopup.Close();
            
            await manager.CheatApplyWinMaxAsync(winStreakStateController.IsMaxLevel, winStreakStateController.RewardMultiplier, 1);
            scoringVisualizeService.ExecuteScheduledAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
        }
        
        
        private void ResetRT()
        {
            manager.CheatResetAsync().Forget();
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