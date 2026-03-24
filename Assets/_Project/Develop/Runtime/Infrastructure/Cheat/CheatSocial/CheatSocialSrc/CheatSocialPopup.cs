#if PR_CHEAT || UNITY_EDITOR

using Cysharp.Threading.Tasks;
using Features.Competition;
using Features.Social;
using Infrastructure.Configs;
using Infrastructure.HapticControl;
using Infrastructure.Popups;
using Infrastructure.Utilities;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


namespace Infrastructure.Cheat
{
    public class CheatSocialPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI authenticationTmp;
        [SerializeField] private TMP_InputField leaderboardNameInput;

        [Header("Cache State")]
        [SerializeField] private TextMeshProUGUI cacheStateTmp;
        [SerializeField] private Button resetCacheStateButton;

        [Header("Cache Records")]
        [SerializeField] private TextMeshProUGUI cacheRecordsTmp;
        [SerializeField] private Button resetCacheRecordsButton;

        [Header("Join")]
        [SerializeField] private Button joinRequestButton;
        [SerializeField] private TextMeshProUGUI joinStatus;
        [SerializeField] private Toggle joinToggle;

        [Header("Send Score")]
        [SerializeField] private Button sendScoreButton;
        [SerializeField] private TextMeshProUGUI sendScoreStatus;
        [SerializeField] private Toggle sendScoreToggle;

        [Header("Get State")]
        [SerializeField] private Button getStateButton;
        [SerializeField] private TextMeshProUGUI getStateStatus;
        [SerializeField] private Toggle getStateToggle;

        [Header("Get Records")]
        [SerializeField] private Button getRecordsButton;
        [SerializeField] private TextMeshProUGUI getRecordsStatus;
        [SerializeField] private Toggle getRecordsToggle;


        [Header("Service requests")]
        [SerializeField] private Button serviceSendScoreButton;
        [SerializeField] private Button serviceGetStateButton;
        [SerializeField] private Button serviceGetRecordsButton;
        [SerializeField] private TextMeshProUGUI lastServiceRequestResult;


        private SocialService socialService;
        private LeaderboardService leaderboardService;
        private LeaderboardNakamaClientWrapper leaderboardNakamaClientWrapper;
        private CompetitionConfig competitionConfig;
        private UserAuthenticateEvent userAuthenticateEvent;
        private LeaderboardOnGetStateStartEvent leaderboardOnGetStateStartEvent;
        private LeaderboardRequestEvent leaderboardRequestEvent;        
        private CompositeDisposable disposables;

        private ToggleAnimation joinAnim;
        private ToggleAnimation sendScoreAnim;
        private ToggleAnimation getStateAnim;
        private ToggleAnimation getRecordsAnim;
        float updateTimer = 0.0f;



        [Inject]
        public void Construct(SocialService socialService,
            LeaderboardService leaderboardService,
            LeaderboardNakamaClientWrapper leaderboardNakamaClientWrapper,
            ConfigProvider configProvider,
            UserAuthenticateEvent userAuthenticateEvent,
            LeaderboardOnGetStateStartEvent leaderboardOnGetStateStartEvent,
            LeaderboardRequestEvent leaderboardRequestEvent)
        {
            this.socialService = socialService;
            this.leaderboardService = leaderboardService;
            this.leaderboardNakamaClientWrapper = leaderboardNakamaClientWrapper;
            this.competitionConfig = configProvider.CompetitionConfig;
            this.userAuthenticateEvent = userAuthenticateEvent;
            this.leaderboardOnGetStateStartEvent = leaderboardOnGetStateStartEvent;
            this.leaderboardRequestEvent = leaderboardRequestEvent;
        }


        protected override void OnInitialize()
        {
            disposables = new CompositeDisposable();
            userAuthenticateEvent.Subscribe(_ => { OnUserAuthenticated(); }).AddTo(disposables);
            leaderboardOnGetStateStartEvent.Subscribe(OnLeaderboardGetStateStartEvent).AddTo(disposables);
            leaderboardRequestEvent.Subscribe(_ => { OnLeaderboardRequestExecuted(leaderboardRequestEvent); }).AddTo(disposables);

            FillUserAuthenticated(socialService.IsAuthenticated);
            FillLeaderboardName(competitionConfig.Feature.leaderboardName);
            FillCacheState();
            FillCacheRecords();

            joinAnim = joinToggle.GetComponent<ToggleAnimation>();
            sendScoreAnim = sendScoreToggle.GetComponent<ToggleAnimation>();
            getStateAnim = getStateToggle.GetComponent<ToggleAnimation>();
            getRecordsAnim = getRecordsToggle.GetComponent<ToggleAnimation>();

            joinToggle.isOn = true;
            sendScoreToggle.isOn = true;
            getStateToggle.isOn = true;
            getRecordsToggle.isOn = true;

            Subscribe();
        }

        protected override void OnDeinitialize()
        {
            disposables.Dispose();
            Unsubscribe();
        }


        protected override void OnBeginOpen()
        {
            joinAnim.PlayAnimation(joinToggle.isOn);
            sendScoreAnim.PlayAnimation(sendScoreToggle.isOn);
            getStateAnim.PlayAnimation(getStateToggle.isOn);
            getRecordsAnim.PlayAnimation(getRecordsToggle.isOn);
        }


        private void Subscribe()
        {
            joinToggle.onValueChanged.AddListener(ChangeJoinValue);
            joinRequestButton.onClick.AddListener(OnJoinRequestButtonClick);

            sendScoreToggle.onValueChanged.AddListener(ChangeSendScoreValue);
            sendScoreButton.onClick.AddListener(OnSendScoreButtonClick);

            getStateToggle.onValueChanged.AddListener(ChangeGetStateValue);
            getStateButton.onClick.AddListener(OnGetStateButtonClick);

            getRecordsToggle.onValueChanged.AddListener(ChangeGetRecordsValue);
            getRecordsButton.onClick.AddListener(OnGetRecordsButtonClick);

            serviceSendScoreButton.onClick.AddListener(() => { OnServiceSendScoreButtonClickAsync().Forget(); });
            serviceGetStateButton.onClick.AddListener(() => { OnServiceGetStateButtonClickAsync().Forget(); });
            serviceGetRecordsButton.onClick.AddListener(() => { OnServiceGetRecordsButtonClickAsync().Forget(); });

            resetCacheStateButton.onClick.AddListener(OnResetCacheStateButtonClick);
            resetCacheRecordsButton.onClick.AddListener(OnResetCacheRecordsButtonClick);
        }


        private void Unsubscribe()
        {
            joinToggle.onValueChanged.RemoveListener(ChangeJoinValue);
            joinRequestButton.onClick.RemoveAllListeners();

            sendScoreToggle.onValueChanged.RemoveListener(ChangeSendScoreValue);
            sendScoreButton.onClick.RemoveAllListeners();

            getStateToggle.onValueChanged.RemoveListener(ChangeGetStateValue);
            getStateButton.onClick.RemoveAllListeners();

            getRecordsToggle.onValueChanged.RemoveListener(ChangeGetRecordsValue);
            getRecordsButton.onClick.RemoveAllListeners();

            serviceSendScoreButton.onClick.RemoveAllListeners();
            serviceGetStateButton.onClick.RemoveAllListeners();
            serviceGetRecordsButton.onClick.RemoveAllListeners();

            resetCacheStateButton.onClick.RemoveAllListeners();
            resetCacheRecordsButton.onClick.RemoveAllListeners();
        }


        private void FillUserAuthenticated(bool isAuthenticated)
        {
            authenticationTmp.text = "Authenticated: " + (isAuthenticated ? "<color=green>Yes</color>" : "<color=red>No</color>");
        }


        private void FillLeaderboardName(string leaderboardName)
        {
            leaderboardNameInput.text = leaderboardName;
        }


        private void ChangeJoinValue(bool isOn)
        {
            ChangeToggleValue(joinAnim, isOn, LeaderboardRequestType.Join);
        }


        private void ChangeSendScoreValue(bool isOn)
        {
            ChangeToggleValue(sendScoreAnim, isOn, LeaderboardRequestType.SendScore);
        }


        private void ChangeGetStateValue(bool isOn)
        {
            ChangeToggleValue(getStateAnim, isOn, LeaderboardRequestType.GetState);
        }


        private void ChangeGetRecordsValue(bool isOn)
        {
            ChangeToggleValue(getRecordsAnim, isOn, LeaderboardRequestType.GetRecords);
        }


        private void ChangeToggleValue(ToggleAnimation toggleAnim, bool isOn, LeaderboardRequestType leaderboardRequestType)
        {
            Vibrate();
            toggleAnim.PlayAnimation(isOn);

            if(isOn)
            {
                leaderboardNakamaClientWrapper.RemoveForcedFailureRequest(leaderboardRequestType);
            }
            else
            {
                leaderboardNakamaClientWrapper.AddForcedFailureRequest(leaderboardRequestType);
            }
        }


        private void OnJoinRequestButtonClick()
        {
            leaderboardNakamaClientWrapper.JoinLeaderboardAsync(leaderboardNameInput.text).Forget();
        }


        private void OnSendScoreButtonClick()
        {
            leaderboardNakamaClientWrapper.WriteRecordAsync(leaderboardNameInput.text, 1, 0).Forget();
        }


        private void OnGetStateButtonClick()
        {
            leaderboardNakamaClientWrapper.GetLeaderboardStateAsync(leaderboardNameInput.text).Forget();
        }


        private void OnGetRecordsButtonClick()
        {
            leaderboardNakamaClientWrapper.ListLeaderboardRecordsAroundOwnerAsync(leaderboardNameInput.text, competitionConfig.Feature.leaderboardCount).Forget();
        }


        private async UniTaskVoid OnServiceSendScoreButtonClickAsync()
        {
            bool isScoreSent = await leaderboardService.TryWriteRecordAsync(leaderboardNameInput.text, 1);
            lastServiceRequestResult.text = isScoreSent.ToString();
        }


        private async UniTaskVoid OnServiceGetStateButtonClickAsync()
        {
            LeaderboardState leaderboardState = await leaderboardService.GetLeaderboardStateAsync(leaderboardNameInput.text);
            lastServiceRequestResult.text = leaderboardState == null ? "null" : leaderboardState.ToString();
        }


        private async UniTaskVoid OnServiceGetRecordsButtonClickAsync()
        {
            LeaderboardRecords leaderboardRecords = await leaderboardService.ListLeaderboardRecordsAroundOwnerAsync(leaderboardNameInput.text, competitionConfig.Feature.leaderboardCount);
            lastServiceRequestResult.text = leaderboardRecords == null ? "null" : leaderboardRecords.ToString();
        }


        private void OnResetCacheStateButtonClick()
        {
            leaderboardService.RemoveCacheState(leaderboardNameInput.text);
        }


        private void OnResetCacheRecordsButtonClick()
        {
            leaderboardService.RemoveCacheRecords(leaderboardNameInput.text);
        }


        private void Vibrate()
        {
            HapticService.I.HapticLight();
        }


        private void OnUserAuthenticated()
        {
            FillUserAuthenticated(socialService.IsAuthenticated);
        }


        private void OnLeaderboardGetStateStartEvent(string leaderboardName)
        {
            if (leaderboardName != leaderboardNameInput.text)
            {
                return;
            }

            ClearStatusTmp();
        }


        private void OnLeaderboardRequestExecuted(LeaderboardRequestEvent leaderboardRequestEvent)
        {
            if(leaderboardRequestEvent.LeaderboardName != leaderboardNameInput.text)
            {
                return;
            }

            TextMeshProUGUI tmp = GetStatusTmp(leaderboardRequestEvent.LeaderboardRequestType);
            if(tmp == null)
            {
                return;
            }

            string status = GetStatusString(leaderboardRequestEvent.LeaderboardRequestState);
            tmp.text = status;
        }


        private TextMeshProUGUI GetStatusTmp(LeaderboardRequestType leaderboardRequestType)
        {
            switch (leaderboardRequestType)
            {
                case LeaderboardRequestType.Join:
                {
                    return joinStatus;
                }
                case LeaderboardRequestType.SendScore:
                { 
                    return sendScoreStatus; 
                }
                case LeaderboardRequestType.GetState:
                {
                    return getStateStatus;
                }
                case LeaderboardRequestType.GetRecords:
                {
                    return getRecordsStatus;
                }
            }

            return null;
        }


        private string GetStatusString(LeaderboardRequestState leaderboardRequestState)
        {
            switch(leaderboardRequestState)
            {
                case LeaderboardRequestState.InProgress:
                {
                    return "In Progress";
                }
                case LeaderboardRequestState.Success:
                {
                    return "<color=green>Success</color>";
                }
                case LeaderboardRequestState.Failure:
                {
                    return "<color=red>Failure</color>";
                }
            }

            return string.Empty;
        }


        private void ClearStatusTmp()
        {
            string notSent = "Not Sent";
            joinStatus.text = notSent;
            sendScoreStatus.text = notSent;
            getStateStatus.text = notSent;
            getRecordsStatus.text = notSent;

            lastServiceRequestResult.text = string.Empty;
        }


        private void FillCacheState()
        {
            string result = $"Cache State: {GetResultString(leaderboardService.IsCacheState(leaderboardNameInput.text))}\n";
            result += $"Cache State Actual: {GetResultString(!leaderboardService.IsNeedUpdateState(leaderboardNameInput.text))}\n";

            cacheStateTmp.text = result;
        }


        private void FillCacheRecords()
        {
            string result = $"Records Cache: {GetResultString(leaderboardService.IsCacheRecords(leaderboardNameInput.text))}\n";
            result += $"Records Cache Need Upd: {GetResultString(leaderboardService.IsNeedUpdateRecords(leaderboardNameInput.text))}\n";

            cacheRecordsTmp.text = result;
        }


        private string GetResultString(bool isResult)
        {
            return isResult ? "<color=green>Yes</color>" : "<color=red>No</color>";
        }


        private void Update()
        {
            if(leaderboardNameInput == null
                || leaderboardNameInput.text.IsNullOrEmpty())
            {
                return;
            }

            updateTimer -= Time.unscaledDeltaTime;

            if(updateTimer <= 0.0f)
            {
                updateTimer = 1.0f;
                FillCacheState();
                FillCacheRecords();
            }
        }
    }
}

#endif
