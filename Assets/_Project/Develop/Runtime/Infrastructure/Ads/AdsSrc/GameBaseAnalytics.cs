using System;
using System.Collections.Generic;
using System.Text;
using Features.Boosters;
using Features.Events;
using Features.LavaQuest;
using Features.LevelComplete;
using Features.LevelSessionStateControl;
using Features.LevelTime;
using Features.SuperSpeedMode;
using Infrastructure.Utilities;
using R3;

namespace Infrastructure.Ads
{
    /// <summary>
    /// Game scene scope
    /// </summary>
    public class GameBaseAnalytics : ILevelSessionSavable
    {
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly LevelTimeManager timeManager;
        private readonly LevelStartedEvent levelStartedEvent;
        private readonly LavaQuestStateController lavaQuestStateController;
        private readonly SuperSpeedController superSpeedController;
        private readonly CompositeDisposable disposable;
        private readonly StringBuilder sb;
        private readonly StringBuilder boosterSb;

        private List<BoosterType> preBoosters = new();
        private Dictionary<BoosterType, int /*usedCount*/> inGameBoosters = new();
        private Dictionary<BoosterType, int /*usedCount*/> winStreakBoosters = new();

        private string levelId;
        private int levelNumber;
        private int winStreakLevel;

        private List<BoosterType> prevSessionPreBoosters;
        private Dictionary<BoosterType, int /*usedCount*/> prevSessionInGameBoosters;
        private Dictionary<BoosterType, int /*usedCount*/> prevSessionWinStreakBoosters;
        private bool isRestoreSession;

        private bool isInit;


        public GameBaseAnalytics(LevelFinishEvent levelFinishEvent, LevelTimeManager timeManager, LevelStartedEvent levelStartedEvent,
                                 LavaQuestStateController lavaQuestStateController, SuperSpeedController superSpeedController)
        {
            this.levelFinishEvent = levelFinishEvent;
            this.timeManager = timeManager;
            this.levelStartedEvent = levelStartedEvent;
            this.lavaQuestStateController = lavaQuestStateController;
            this.superSpeedController = superSpeedController;
            this.disposable = new CompositeDisposable();
            this.sb = new StringBuilder();
            this.boosterSb = new StringBuilder();
        }


        public void RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionPreBoosters = sessionData.preBoosters;
            prevSessionInGameBoosters = sessionData.inGameBoosters;
            prevSessionWinStreakBoosters = sessionData.winStreakBoosters;
            isRestoreSession = true;
        }


        public void SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.preBoosters = new List<BoosterType>(preBoosters);
            sessionData.inGameBoosters = new Dictionary<BoosterType, int>(inGameBoosters);
            sessionData.winStreakBoosters = new Dictionary<BoosterType, int>(winStreakBoosters);
        }


        public void Initialize()
        {
            if (isInit)
                return;

            if (isRestoreSession)
            {
                RestoreFromPrevSession();
            }

            levelStartedEvent.Subscribe(_ => TrackLevelStarted(levelStartedEvent.LevelId, levelStartedEvent.LevelNumber, levelStartedEvent.WinStreakLevel))
                             .AddTo(disposable);

            levelFinishEvent.Subscribe(_ =>
                             {
                                 TrackLevelFinished(levelFinishEvent.isWin,
                                                    levelFinishEvent.reason,
                                                    levelFinishEvent.specialItemsCount,
                                                    levelFinishEvent.specialItemsCountMultiplied);
                             })
                            .AddTo(disposable);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            preBoosters.Clear();
            inGameBoosters.Clear();
            disposable.Dispose();
            isInit = false;
        }


        public void AddSelectedPreBoosters(List<BoosterType> selectedPreBoosters)
        {
            this.preBoosters.AddRange(selectedPreBoosters);
        }


        public void AddUsedInGameBooster(BoosterType boosterType)
        {
            inGameBoosters.TryAdd(boosterType, 0);
            inGameBoosters[boosterType]++;
        }


        public void AddUsedWinStreakBooster(BoosterType boosterType)
        {
            winStreakBoosters.TryAdd(boosterType, 0);
            winStreakBoosters[boosterType]++;
        }


        private void TrackLevelStarted(string levelId, int levelNumber, int winStreakLevel)
        {
            this.levelId = levelId;
            this.levelNumber = levelNumber;
            this.winStreakLevel = winStreakLevel;

            string json = GetLevelStartJson();

            AnalyticSender.TrackLevelStarted(levelId, levelNumber, 0, json);
        }


        private void TrackLevelFinished(bool isWin, LoseReason looseReason, int specialItemsCount, int specialItemsCountMultiplied)
        {
            if (isWin)
            {
                AnalyticSender.TrackLevelCompleted(levelId, levelNumber, 0, GetEndLevelJson(true, null, specialItemsCount, specialItemsCountMultiplied));
            }
            else
            {
                string json = GetEndLevelJson(false, JsonUtils.CreateValue("fail_reason", looseReason.ToString().ToSnakeCase()), specialItemsCount, specialItemsCountMultiplied);
                AnalyticSender.TrackLevelFailed(levelId, levelNumber, 0, json);
            }
        }


        private string GetPreBoostersJson()
        {
            return JsonUtils.CreateValue("pre_game_boosters_used", string.Join(',', preBoosters));
        }


        private string GetInGameBoostersJson()
        {
            boosterSb.Clear();
            boosterSb.Append('{');

            foreach (var entryPair in inGameBoosters)
            {
                boosterSb.Append(JsonUtils.CreateValue(entryPair.Key.ToString(), entryPair.Value));
            }

            boosterSb.AppendLast('}');
            return JsonUtils.CreateValue("ingame_boosters_used", boosterSb);
        }


        private string GetWinStreakBoostersJson()
        {
            boosterSb.Clear();
            boosterSb.Append('{');

            foreach (var entryPair in winStreakBoosters)
            {
                boosterSb.Append(JsonUtils.CreateValue(entryPair.Key.ToString(), entryPair.Value));
            }

            boosterSb.AppendLast('}');
            return JsonUtils.CreateValue("winstreak_boosters_used", boosterSb);
        }


        private string GetLevelStartJson()
        {
            sb.Clear();
            sb.Append('{');
            sb.Append(JsonUtils.CreateValue("time_start", timeManager.ConfigTime));
            sb.Append(JsonUtils.CreateValue("winstreak_level", winStreakLevel));
            sb.Append(GetPreBoostersJson());

            sb.AppendLast('}');
            return sb.ToString();
        }


        private string GetEndLevelJson(bool isWin, string failReasonJson, int specialItemsCount, int specialItemsCountMultiplied)
        {
            sb.Clear();
            sb.Append('{');

            if (!isWin)
                sb.Append(failReasonJson);

            sb.Append(GetInGameBoostersJson());
            sb.Append(GetPreBoostersJson());
            sb.Append(GetWinStreakBoostersJson());

            TimeSpan timeSpan = TimeSpan.FromSeconds(timeManager.SecondsLeft);
            int intLikeHud = timeSpan.Minutes * 60 + timeSpan.Seconds;
            string timeLeftJson = JsonUtils.CreateValue("time_left", intLikeHud);
            sb.Append(timeLeftJson);

            float percent = timeManager.SecondsLeft / timeManager.ConfigTime * 100;
            string timeLeftPercJson = JsonUtils.CreateValue("time_left_percent", percent);
            sb.Append(timeLeftPercJson);

            string timeStartJson = JsonUtils.CreateValue("time_start", timeManager.ConfigTime);
            sb.Append(timeStartJson);

            string timeTotalJson = JsonUtils.CreateValue("time_total", timeManager.TotalTime);
            sb.Append(timeTotalJson);

            string winStreakJson = JsonUtils.CreateValue("winstreak_level", winStreakLevel);
            sb.Append(winStreakJson);

            string lavaQuestJson = JsonUtils.CreateValue("is_lava_quest_active", lavaQuestStateController.IsStartedState() ? 1 : 0);
            sb.Append(lavaQuestJson);

            string superSpeedJson = JsonUtils.CreateValue("is_super_speed_active", superSpeedController.IsSuperSpeedActive ? 1 : 0);
            sb.Append(superSpeedJson);

            if (isWin)
            {
                string specialItemsJson = JsonUtils.CreateValue("special_objects_count", specialItemsCount);
                sb.Append(specialItemsJson);

                string specialItemsMultJson = JsonUtils.CreateValue("special_objects_count_multiplier", specialItemsCountMultiplied);
                sb.Append(specialItemsMultJson);
            }

            sb.AppendLast('}');
            return sb.ToString();
        }


        private void RestoreFromPrevSession()
        {
            if (prevSessionPreBoosters != null)
                preBoosters = new List<BoosterType>(prevSessionPreBoosters);

            if (prevSessionInGameBoosters != null)
                inGameBoosters = new Dictionary<BoosterType, int>(prevSessionInGameBoosters);

            if (prevSessionWinStreakBoosters != null)
                winStreakBoosters = new Dictionary<BoosterType, int>(prevSessionWinStreakBoosters);
        }
    }
}