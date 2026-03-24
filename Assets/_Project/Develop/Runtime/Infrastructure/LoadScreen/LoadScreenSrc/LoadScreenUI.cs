using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.LoadScreen
{
    /// <summary>
    /// Load screen UI, should always be on scene
    /// </summary>
    public class LoadScreenUI : MonoBehaviour
    {

#region NestedTypes

        public enum Reason
        {
            LoadScene,
        }

#endregion

#region Members

        [SerializeField] private CanvasGroup rootCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressBarText;

        [SerializeField] private Button supportBtn;
        [SerializeField] private TextMeshProUGUI supportTxt;
        private Action supportCallback;

        private List<string> loadingBarTextKeys = new() { LocKeys.LoadScreen.Loading };

        private HashSet<Reason> reasons = new();
        private Timer fakeProgressTimer = new(1);
        [SerializeField] private BroTweenSafe fadeInTween;
        private bool fakeProgressStarted;
        private int fakeProgressLowPercent = 0;
        private int fakeProgressHighPercent = 0;

        public static int Stage01 = 5;
        public static int Stage02 = 20;
        public static int Stage03 = 30;
        public static int Stage04 = 40;
        public static int Stage05 = 50;
        public static int Stage06 = 60;
        public static int Stage07 = 70;
        public static int Stage08 = 80;
        public static int Stage09 = 90;
        public static int Stage10 = 100;

        public bool IsOpened => gameObject.activeSelf;

#endregion

#region Public Methods

        private void Update()
        {
            if (!fakeProgressStarted)
                return;

            if (!fakeProgressTimer.IsOffAfterUpdate(Time.unscaledDeltaTime))
                FakeProgressPercent(fakeProgressLowPercent, fakeProgressHighPercent);
            else
                fakeProgressStarted = false;
        }


        public void Construct(Action supportCallback)
        {
            this.supportCallback = supportCallback;
        }


        public void Initialize()
        {
            supportBtn.onClick.RemoveListener(SupportButtonHandler);
            supportBtn.onClick.AddListener(SupportButtonHandler);

            supportTxt.text = LocalizationService.I.Get(LocKeys.Support.SupportButton);
            supportBtn.gameObject.SetActive(true);

            progressSlider.maxValue = 100.0f;
            ResetProgress();
        }


        public void Open(Reason reason)
        {
            reasons.Add(reason);
            SetLoadScreenActive(true);
        }


        public UniTask OpenAsync(Reason reason, CancellationToken cancellationToken)
        {
            ResetProgress();
            reasons.Add(reason);

            rootCanvasGroup.alpha = 0.0f;
            SetLoadScreenActive(true);

            fadeInTween.Kill();
            var tween = BroTween.FadeCanvasGroup(rootCanvasGroup, 0.0f, 1.0f, fadeDuration)
                                .SetUpdate(isIndependentUpdate: true)
                                .SetLiveBetweenScenes(true);
            
            fadeInTween = tween.ToSafe();
            fadeInTween.Play();
            return fadeInTween.ToUniTask(cancellationToken: cancellationToken);
        }


        public void Close(Reason reason)
        {
            reasons.Remove(reason);

            if (reasons.Count == 0)
            {
                SetLoadScreenActive(false);
            }
        }

        
        public void SetProgressPercent(int percent, bool fakeProgress = false, float fakeTime = 0)
        {
            if (loadingBarTextKeys.Count > 0)
            {
                progressBarText.text = LocalizationService.I.Get(loadingBarTextKeys[UnityEngine.Random.Range(0, loadingBarTextKeys.Count)]);
            }

            if (fakeProgress)
            {
                fakeProgressStarted = true;
                fakeProgressTimer.Reset(fakeTime);
                fakeProgressLowPercent = Mathf.RoundToInt(progressSlider.value);
                fakeProgressHighPercent = percent;
                return;
            }
            else
            {
                fakeProgressStarted = false;
                fakeProgressTimer.SetOff();
            }

            progressSlider.value = percent > progressSlider.maxValue ? progressSlider.maxValue : percent;
        }


        public void ShowProgressSlider(bool value)
        {
            if (progressSlider.gameObject.activeSelf != value)
                progressSlider.gameObject.SetActive(value);
        }


        private void FakeProgressPercent(float lowPercent, float highPercent)
        {
            var value = highPercent - (highPercent - lowPercent) * fakeProgressTimer.TimeRestPercent() * 0.01f;
            progressSlider.value = value > progressSlider.maxValue ? progressSlider.maxValue : value;
        }


        private void ResetProgress()
        {
            progressSlider.value = 0.0f;
        }


        private void SupportButtonHandler()
        {
            supportCallback?.Invoke();
        }


        private void SetLoadScreenActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

#endregion

    }
}