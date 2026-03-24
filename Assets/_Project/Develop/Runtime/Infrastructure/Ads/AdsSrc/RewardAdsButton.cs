using System;
using System.Collections;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Ads
{
    public class RewardAdsButton : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform bounceRect;
        [SerializeField] private Button button;
        [SerializeField] private GameObject loading;
        [SerializeField] private float checkCd = 0.3f;

        private float currentCd;

        private AdsFlyTextEvent flyTextEvent;
        private string placement;
        private string parentName;
        private Action click;
        private Action skip;
        private Action finish;

        private bool isAnalyticsSended;
        private bool isInit;


        private void OnEnable()
        {
            isAnalyticsSended = false;
        }


        private void Update()
        {
            if (!isInit)
                return;

            currentCd -= Time.unscaledDeltaTime;

            if (currentCd > 0)
                return;

            currentCd = checkCd;

            bool isAvailable = IsAvailable();

            if (loading.activeSelf != !isAvailable)
                loading.SetActive(!isAvailable);

            SendAnalytics(isAvailable);

        }


        [Inject]
        public void Construct(AdsFlyTextEvent flyTextEvent)
        {
            this.flyTextEvent = flyTextEvent;
        }


        public void Construct(string place, string parentName, Action finish)
        {
            this.placement = place;
            this.parentName = parentName;
            this.finish = finish;
        }


        public void SetSkipDelegate(Action skip)
        {
            this.skip = skip;
        }
        
        
        public void SetOnClickDelegate(Action click)
        {
            this.click = click;
        }
        
        
        public void Initialize()
        {
            if (isInit)
                return;

            button.onClick.AddListener(Button_OnClick);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            button.onClick.RemoveListener(Button_OnClick);
            isInit = false;
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        public void SetButtonsInteractable(bool value)
        {
            button.interactable = value;
        }

        private void SendAnalytics(bool isAvailable)
        {
            if (isAvailable && !isAnalyticsSended)
            {
                AnalyticSender.SendRewardedOfferEvent(placement);
                isAnalyticsSended = true;
            }
        }


        private void ShowRewardedAds()
        {
            if (!HavePlacement())
            {
                Debug.LogError("[CODE] Reward placement not set", this);
                skip?.Invoke();
                return;
            }

            if (IsAvailable())
            {
                Advertisement.ShowRewarded(placement, isFinished =>
                {
#if UNITY_EDITOR
                    fakeRewardedAds = StartCoroutine(ShowFakeReward(isFinished));
#else
                OnRewardedFinish(isFinished);
#endif
                });
            }
            else
            {
                ShowWarning(LocKeys.Ads.Loading);
                skip?.Invoke();
            }
        }


        private void OnRewardedFinish(bool isFinished)
        {
            if (isFinished)
            {
                finish?.Invoke();
            }
            else
            {
                skip?.Invoke();
                ShowWarning(LocKeys.Ads.NotFinish);
            }
        }

        
        private void ShowWarning(string locKey)
        {
            if (root == null)
                return;

            flyTextEvent?.Request(locKey, root.position);
        }


        private bool HavePlacement()
        {
            return isInit && !string.IsNullOrEmpty(placement);
        }


        private bool IsAvailable()
        {
            return Advertisement.IsRewardedAvailable(placement);
        }


        private void Button_OnClick()
        {
            AnalyticSender.TrackClick(parentName, this.name);
            click?.Invoke();
            BroTween.ClickBounceWithCallBack(button, bounceRect, this, target => target.ShowRewardedAds(), true)
                    .Play();
        }


#if UNITY_EDITOR
        private Coroutine fakeRewardedAds;


        private void OnDisable()
        {
            if (fakeRewardedAds != null)
            {
                StopCoroutine(fakeRewardedAds);
                fakeRewardedAds = null;
                SetButtonsInteractable(true);
            }
        }


        private IEnumerator ShowFakeReward(bool isFinished)
        {
            SetButtonsInteractable(false);
            yield return new WaitForSecondsRealtime(1.0f);
            SetButtonsInteractable(true);

            OnRewardedFinish(isFinished);
            fakeRewardedAds = null;
        }
#endif
    }
}