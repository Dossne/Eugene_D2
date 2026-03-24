using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.ProgressBar;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using TMPro;
using UnityEngine;

namespace Features.WinStreak
{
    public class WinStreakUIBanner : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform mainRoot;
        [SerializeField] private SegmentedProgressBar progressBar;
        [SerializeField] private GameObject lockRoot;
        [SerializeField] private List<WinStreakBannerIcon> icons;
        [SerializeField] private GameObject rewardMultiplierRoot;
        [SerializeField] private GameObject rewardMultiplierFxRoot;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI progressTxt;
        [SerializeField] private TextMeshProUGUI lockTxt;

        [Header("Animation")]
        [SerializeField] private WinStreakUIBannerParams animParams;

        private int winStreakCurrentLevel;
        private int winStreakMaxLevel;
        private int unlockGameLevel;
        private bool isUnlocked;
        private bool isMultiplierUnlocked;
        private int winStreakMultiplier;
        private BroTweenSafe moveTween;


        public void Construct(int winStreakCurrentLevel, 
                              int winStreakMaxLevel, 
                              int unlockGameLevel, 
                              bool isUnlocked, 
                              int winStreakMultiplier,
                              bool isMultiplierUnlocked)
        {
            this.winStreakCurrentLevel = winStreakCurrentLevel;
            this.winStreakMaxLevel = winStreakMaxLevel;
            this.unlockGameLevel = unlockGameLevel;
            this.isUnlocked = isUnlocked;
            this.isMultiplierUnlocked = isMultiplierUnlocked;
            this.winStreakMultiplier = winStreakMultiplier;
            progressBar.Construct(winStreakMaxLevel);
        }


        public void Initialize()
        {
            progressBar.SetObjectActive(isUnlocked);
            lockRoot.SetActive(!isUnlocked);
            rewardMultiplierRoot.SetActive(isUnlocked && isMultiplierUnlocked);
            rewardMultiplierFxRoot.SetActive(winStreakMultiplier > 1 && isMultiplierUnlocked);
            headerTxt.text = LocalizationService.I.Get(LocKeys.WinStreak.BannerHeader);

            if (!isUnlocked)
                lockTxt.text = LocalizationService.I.Get(LocKeys.WinStreak.Unlock, unlockGameLevel.ToString());
            else
                progressBar.Initialize();

            var currIdx = Mathf.Max(0, winStreakCurrentLevel - 1);
            var maxIdx = Mathf.Max(0, winStreakMaxLevel - 1);

            for (int i = 0; i < icons.Count; i++)
            {
                bool maxReached = i > maxIdx;

                icons[i].SetObjectActive(!maxReached);

                if (maxReached)
                    continue;

                if (i < currIdx || !isUnlocked)
                    icons[i].SetClosed();
                else
                    icons[i].SetOpened();
            }
        }


        public async UniTaskVoid PlayOpenAsync(bool isIncremented, CancellationToken cancellationToken)
        {
            if (!isUnlocked)
                return;

            SetToStartMovePosition();
            int currIdx = Mathf.Max(0, winStreakCurrentLevel - 1);
            int sliderLevel = isIncremented ? currIdx : winStreakCurrentLevel; //do not play fx on max level many times
            progressTxt.text = $"{sliderLevel.ToString()} / {winStreakMaxLevel.ToString()}";
            progressBar.SetSliderToSegment(sliderLevel);
            rewardMultiplierFxRoot.SetActive(false);

            await UniTask.WaitForSeconds(animParams.delayBeforeAnimations, ignoreTimeScale: true, cancellationToken: cancellationToken);

            PlayMoveTween();

            if (isIncremented)
            {
                await UniTask.WaitForSeconds(animParams.moveDuration, ignoreTimeScale: true, cancellationToken: cancellationToken);
                icons[currIdx].PlayOpen();
                progressBar.SliderAnimatedMove(currIdx, winStreakCurrentLevel);
                await UniTask.WaitForSeconds(progressBar.SliderMoveDuration, ignoreTimeScale: true, cancellationToken: cancellationToken);
                rewardMultiplierFxRoot.SetActive(winStreakMultiplier > 1);
            }
            else
            {
                icons[currIdx].ActivateBgFx();
                rewardMultiplierFxRoot.SetActive(winStreakMultiplier > 1);
            }

            SetLevelText();
        }


        private void PlayMoveTween()
        {
            moveTween.Kill();
            moveTween = BroTween.AnchoredPosition(mainRoot, animParams.moveFromPosition, Vector2.zero, animParams.moveDuration)
                                .SetEase(animParams.moveCurve)
                                .SetUpdate(true)
                                .ToSafe();
            moveTween.Play();
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        private void SetLevelText()
        {
            progressTxt.text = $"{winStreakCurrentLevel.ToString()} / {winStreakMaxLevel.ToString()}";
        }


        private void SetToStartMovePosition()
        {
            mainRoot.anchoredPosition = animParams.moveFromPosition;
        }

        

#if UNITY_EDITOR
        [TriInspector.Title("Editor.Debug")]
        [SerializeField] private int winStreakCurrentLevelDebug;
        [SerializeField] private int winStreakMaxLevelDebug;
        [SerializeField] private int unlockGameLevelDebug;
        [SerializeField] private bool isUnlockedDebug;
        [SerializeField] private bool isMultiplierDebug;
        [SerializeField] private bool isLevelIncrementedDebug;

        [TriInspector.Button]
        private void PlayOpen_Editor()
        {
            Construct(winStreakCurrentLevelDebug, winStreakMaxLevelDebug, unlockGameLevelDebug, isUnlockedDebug, 2, isMultiplierDebug);
            Initialize();
            PlayOpenAsync(isLevelIncrementedDebug, gameObject.GetCancellationTokenOnDestroy()).Forget();
        }
#endif
    }
}