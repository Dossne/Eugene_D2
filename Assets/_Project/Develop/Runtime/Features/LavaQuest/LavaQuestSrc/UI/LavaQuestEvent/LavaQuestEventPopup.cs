using System;
using System.Collections.Generic;
using System.Linq;
using Features.InfoPopup;
using Features.Tutorial;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Pool.Particles;
using Infrastructure.Popups;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.LavaQuest
{
    public class LavaQuestEventPopup : PopupBase
    {
        public event Action OnInfoRequested;

        [Header("Top")]
        [SerializeField] private Button infoButton;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI looseDescriptionTxt;
        [SerializeField] private TextMeshProUGUI timerTxt;
        [SerializeField] private TextMeshProUGUI levelsStateTxt;
        [SerializeField] private TextMeshProUGUI playersStateTxt;
        [SerializeField] private TextMeshProUGUI rewardValueTxt;
        [SerializeField] private Image rewardImage;
        [SerializeField] private RectTransform topRoot;
        [SerializeField] private List<TMPLocalizer> localizeTexts;
        [SerializeField] private GameObject normalStateRoot;
        [SerializeField] private GameObject looseStateRoot;
        [SerializeField] private List<Image> levelImages;
        [SerializeField] private List<Image> playerImages;
        [SerializeField] private List<Image> treasureImage;

        [Header("Animations")]
        [SerializeField] private float minSpawnYDistance = 10;
        [SerializeField] private CharacterIcon iconPf;
        [SerializeField] private RectTransform characterIconRoot;
        [SerializeField] private IPAnimationBehaviour tapToContinueBhvr;
        [SerializeField] private Button tapToContinueBtn;
        [SerializeField] private RectTransform levelsFxRoot;
        [SerializeField] private RectTransform playersFxRoot;
        [SerializeField] private ReusableParticleSystem splashFx;
        [SerializeField] private LavaQuestEventPopupAnimationParams animParams;

        private TutorialTriggerEvent tutorialTriggerEvent;
        private readonly List<CharacterIcon> characterIcons = new();
        private List<LavaQuestEventZone> evtZones;
        private BroTweenSafe topSeq;
        private float timeRest;
        private float refreshTimeCurrent;
        private int playersCount;
        private bool isAnimatedMoveTop;
        
        
        public List<CharacterIcon> CharacterIcons => characterIcons;
        public List<Image> LevelImages => levelImages;
        public List<Image> PlayerImages => playerImages;
        public List<Image> TreasureImage => treasureImage;
        public float MinSpawnYDistance => minSpawnYDistance;
        public LavaQuestEventPopupAnimationParams AnimParams => animParams;

        public bool IsOpenAnimationFinished {  get; private set; }


        private void Update()
        {
            if (timeRest <= 0)
                return;

            refreshTimeCurrent += Time.unscaledDeltaTime;
            timeRest -= Time.unscaledDeltaTime;

            if (refreshTimeCurrent < 1)
                return;

            SetTimeText();
            refreshTimeCurrent = 1;
        }


        [Inject]
        public void Construct(TutorialTriggerEvent tutorialTriggerEvent)
        {
            this.tutorialTriggerEvent = tutorialTriggerEvent;
        }


        public void Refresh(Sprite rewardIcon, string prizeValue, string descriptionText, string levelsTxt, int playersCount, float timeLeft, bool isLoose, bool isAnimatedMoveTop)
        {
            rewardImage.sprite = rewardIcon;
            rewardValueTxt.text = prizeValue;

            if (isLoose)
                looseDescriptionTxt.text = descriptionText;
            else
                descriptionTxt.text = descriptionText;

            normalStateRoot.SetObjectActive(!isLoose);
            looseStateRoot.SetObjectActive(isLoose);

            SetLevelsTxt(levelsTxt);
            playersStateTxt.text = playersCount.ToString();
            this.playersCount = playersCount;

            SetTimeRest(timeLeft);
            this.isAnimatedMoveTop = isAnimatedMoveTop;
        }


        public bool TrySetEventZoneActive(int id, out LavaQuestEventZone activeZone)
        {
            activeZone = null;
            foreach (var zone in evtZones)
            {
                bool isTarget = zone.ID == id;
                if (isTarget)
                {
                    activeZone = zone;
                }

                zone.SetObjectActive(isTarget);
            }

            bool found = activeZone != null;

            if (!found)
                Debug.LogError($"[{nameof(LavaQuestEventPopup)}]. Platform zone with id {id} is not found");

            return found;
        }


        public bool TryGetActiveEventZone(out LavaQuestEventZone activeZone)
        {
            activeZone = null;
            foreach (var zone in evtZones)
            {
                if (zone.gameObject.activeSelf)
                {
                    activeZone = zone;
                }

            }

            bool found = activeZone != null;

            if (!found)
                Debug.LogError($"[{nameof(LavaQuestEventPopup)}]. Active zone is not found");

            return found;
        }

        

        public void DelayedActivateTapToContinue()
        {
            tapToContinueBhvr.Prepare();
            SetTapToContinueActive(true);
            tapToContinueBtn.interactable = false;
            BroTween.DelayedCall(animParams.tapToContinueShowDelay, () =>
            {
                tapToContinueBhvr.Play();
                tapToContinueBtn.interactable = true;
            });
        }


        public void CreateNewIconInstance()
        {
            var icon = Instantiate(iconPf, characterIconRoot);
            icon.Initialize();
            icon.PutOnBottomHierarchy();
            icon.SetObjectActive(false);
            characterIcons.Add(icon);
        }


        public void SetLevelsTxt(string levelsTxt)
        {
            levelsStateTxt.text = levelsTxt;
        }


        public BroTweenBase GetChangeTextByCurveTween(int to)
        {
            int from = playersCount;
            playersCount = to;
            return BroTween.Int(SetPlayersStateText, from, to, animParams.playerChangeTextDuration);
        }


        public BroTweenBase GetScaleByCurveTweenLevels(bool scaleUp)
        {
            return BroTween.ScaleByCurve(levelsStateTxt.transform, animParams.levelTxtScaleDuration, animParams.levelTxScaleCurve).SetPlayBackwards(!scaleUp);
        }


        public BroTweenBase GetScaleByCurveTweenPlayers(bool scaleUp)
        {
            return BroTween.ScaleByCurve(playersStateTxt.transform, animParams.playerScaleDuration, animParams.playerScaleCurve).SetPlayBackwards(!scaleUp);;
        }


        public void PlaySplashFxOnPlayers()
        {
            PlaySplashFx(playersFxRoot);
        }


        public void PlaySplashFxOnLevels()
        {
            PlaySplashFx(levelsFxRoot);
        }

        public void UpdatePlayerIcon(Sprite sprite)
        {
            if (characterIcons.Count > 0)
                characterIcons[0].SetIcon(sprite);
        }

        protected override void OnInitialize()
        {
            foreach (var txt in localizeTexts)
            {
                txt.Localize();
            }
            
            evtZones = GetComponentsInChildren<LavaQuestEventZone>(true).ToList();
            
            foreach (var eventZone in evtZones)
            {
                eventZone.Initialize();
            }
            
            tapToContinueBtn.onClick.AddListener(InstantClose);
            tapToContinueBhvr.Initialize();
            infoButton.onClick.AddListener(InfoPopupRequest);
            
            splashFx.Initialize();
        }


        protected override void OnDeinitialize()
        {
            topSeq.Kill();

            infoButton.onClick.RemoveListener(InfoPopupRequest);
            tapToContinueBtn.onClick.RemoveListener(InstantClose);

            foreach (var icon in characterIcons)
            {
                icon.Dispose();
            }

            tapToContinueBhvr.Deinitialize();
            characterIcons.Clear();
            splashFx.Deinitialize();
            OnInfoRequested = null;
        }


        protected override void OnBeginOpen()
        {
            MoveTopPart();
            SetTapToContinueActive(false);
            IsOpenAnimationFinished = false;
            tutorialTriggerEvent.Execute(TutorialTrigger.LavaQuestEventPopupBeginOpen);
        }


        private void PlaySplashFx(RectTransform root)
        {
            splashFx.transform.SetParent(root);
            splashFx.transform.localPosition = Vector3.zero;
            splashFx.Play();
        }


        private void MoveTopPart()
        {
            if (!isAnimatedMoveTop)
                return;
            
            var seq = BroTween.Sequence().SetUpdate(true);
            seq.Append(BroTween.AnchoredPosition(topRoot,  animParams.topStartOffsetPos, Vector2.zero, animParams.topMoveDuration).SetEase(animParams.topMoveCurve));
            topSeq = seq.ToSafe();
            topSeq.Play();
        }


        private void SetTimeRest(float timeLeft)
        {
            this.timeRest = timeLeft;
            this.refreshTimeCurrent = 1;
        }


        private void SetTimeText()
        {
            string timeStr = TimeUtils.GetTimeString(timeRest);
            timerTxt.text = timeStr;
        }


        private void InfoPopupRequest()
        {
            OnInfoRequested?.Invoke();
        }


        private void SetPlayersStateText(int value)
        {
            playersStateTxt.text = value.ToString();
        }

        private void SetTapToContinueActive(bool value)
        {
            tapToContinueBhvr.gameObject.SetObjectActive(value);
        }

        protected override void AnimationStopHandler() => IsOpenAnimationFinished = IsOpened;
    }
}