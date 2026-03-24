using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.LevelComplete;
using Features.LevelConfiguration;
using Features.LevelSequence;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using VContainer;


namespace Features.Level
{
    public class LevelService : ISavable
    {
        public event Action OnLevelIncremented;

        private readonly LevelConfig levelConfig;
        private readonly SystemSettingsConfig systemConfig;
        private readonly LevelSequenceConfig levelSequenceConfig;
        private readonly LevelDifficultyCache levelDifficultyCache;
        private readonly AssetProvider assetProvider;
        private int scheduledLevelAdd = 0;
        private Dictionary<LevelDifficulty, ShuffledSelector<LevelSequenceData>> shuffledSelectors;
        private string currentLevelScriptableObjectId = string.Empty;
        private LevelData currentLevelData;
        private LevelConfiguration.Level currentLevel;
        private LevelStatistics levelStatistics;

        /// <summary>
        /// CurrentLevelIndex + 1
        /// </summary>
        public int CurrentLevelNumber => CurrentLevelIndex + 1;

        public bool IsSequenceOff => CurrentLevelIndex >= levelSequenceConfig.LevelSequence.Count;

        private int CurrentLevelIndex { get; set; } = 0;
        public string ForcedLevelId { get; set; } = string.Empty;
        private string currentLevelId = string.Empty;
        private bool isInit = false;

        public bool HasLoseOnCurrentLevel { get; private set; } = false;

        public LevelStatistics LevelStatistics => levelStatistics;


        [Inject]
        public LevelService(ConfigProvider configProvider, AssetProvider assetProvider)
        {
            levelConfig = configProvider.LevelConfig;
            systemConfig = configProvider.SystemSettingsConfig;
            levelSequenceConfig = configProvider.LevelSequenceConfig;
            levelDifficultyCache = configProvider.LevelDifficultyCache; 
            this.assetProvider = assetProvider;            
        }


        public async UniTask InitializeAsync(CancellationToken token)
        {
            if (isInit)
                return;

            await CreateShuffledSelectorsAsync(token);

            if (scheduledLevelAdd > 0)
                await ApplyScheduledAsync(token);
            else
                await SetCurrentLevelAsync(CurrentLevelIndex, token);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            if (currentLevelScriptableObjectId != string.Empty)
                assetProvider.ReleaseAsset(currentLevelScriptableObjectId);

            isInit = false;
        }


        void ISavable.Load(Infrastructure.PersistentProgress.Progress progress)
        {
            CurrentLevelIndex = progress.gameState.levelIdx;
            HasLoseOnCurrentLevel = progress.gameState.loseOnCurrentLevel;
            scheduledLevelAdd = progress.gameState.scheduledLevelAdd;
            levelStatistics = progress.gameState.levelStatistics;

#if PR_CHEAT
            ForcedLevelId = progress.cheatState.forceLevelId;
#endif            
        }


        void ISavable.Save(Infrastructure.PersistentProgress.Progress progress)
        {
            progress.gameState.levelIdx = CurrentLevelIndex;
            progress.gameState.loseOnCurrentLevel = HasLoseOnCurrentLevel;
            progress.gameState.scheduledLevelAdd = scheduledLevelAdd;
            progress.gameState.levelStatistics = levelStatistics;
        }


        public LevelData GetCurrentLevelData()
        {
            return currentLevelData;
        }


        public bool CurrentLevelHasCollectable(CollectableType collectableType)
        {
            return currentLevel.collectables.Exists(x => x.collectableType == collectableType);
        }


        public string GetCurrentLevelId()
        {
            string levelId = currentLevelId;
#if PR_CHEAT
            if (!string.IsNullOrEmpty(ForcedLevelId))
            {
                levelId = ForcedLevelId;
            }
#endif
            return levelId;
        }


        public void ScheduleIncrement()
        {
            ApplyWinStatistics();            
            scheduledLevelAdd = 1;
            HasLoseOnCurrentLevel = false;
        }


        public async UniTask ApplyScheduledAsync(CancellationToken cancellationToken)
        {
            if (scheduledLevelAdd < 1)
                return;

            int nextLevel = CurrentLevelIndex + scheduledLevelAdd;
            scheduledLevelAdd = 0;
            await SetCurrentLevelAsync(nextLevel, cancellationToken);
            OnLevelIncremented?.Invoke();
        }


        public void RegisterLose()
        {
            ApplyLoseStatistics();            
            HasLoseOnCurrentLevel = true;
        }

        private void ApplyWinStatistics() 
        {
            if (levelStatistics.previousFinisedLevel < CurrentLevelNumber)
                levelStatistics.firstWinCount++;

            levelStatistics.previousFinisedLevel = CurrentLevelNumber;
            levelStatistics.totalWinCount = CurrentLevelNumber;
            levelStatistics.currentWinStreak++;
            levelStatistics.maxWinStreak = Math.Max(levelStatistics.currentWinStreak, levelStatistics.maxWinStreak);
        }

        private void ApplyLoseStatistics()
        {
            levelStatistics.previousFinisedLevel = CurrentLevelNumber;
            levelStatistics.currentWinStreak = 0;
        }

        private async UniTask CreateShuffledSelectorsAsync(CancellationToken cancellationToken)
        {
            Dictionary<LevelDifficulty, List<LevelSequenceData>> randomLevelsByDifficulty = new Dictionary<LevelDifficulty, List<LevelSequenceData>>();
            for (int i = 0; i < levelSequenceConfig.RandomLevelRules.Count; ++i)
            {
                LevelSequenceData levelSequenceData = levelSequenceConfig.RandomLevelRules[i];
                LevelDifficulty resultDifficulty = LevelDifficulty.Default;

                //1. check overrides
                if (levelConfig.TryGetLevelById(levelSequenceData.levelId, out LevelData configData))
                {
                    resultDifficulty = configData.difficulty;
                }
                else
                {
                    //2. check cache
                    if (levelDifficultyCache.TryGet(levelSequenceData.levelId, out var cachedDifficulty))
                    {
                        resultDifficulty = cachedDifficulty;
                    }
                    else
                    {
                        //3. load from file
                        (LevelData levelData, LevelConfiguration.Level level, bool overrideFromConfig) = await GetLevelCfgData(levelSequenceData.levelId, cancellationToken);
                        if (levelData != null)
                            resultDifficulty = levelData.difficulty;
                        if (!overrideFromConfig)
                            assetProvider.ReleaseAsset(levelSequenceData.levelId);
                    }
                }

                if (!randomLevelsByDifficulty.ContainsKey(resultDifficulty))
                {
                    randomLevelsByDifficulty[resultDifficulty] = new List<LevelSequenceData>();
                }
                randomLevelsByDifficulty[resultDifficulty].Add(levelSequenceData);                
            }

            shuffledSelectors = new Dictionary<LevelDifficulty, ShuffledSelector<LevelSequenceData>>();
            foreach (var item in randomLevelsByDifficulty)
            {
                LevelDifficulty levelDifficulty = item.Key;
                List<LevelSequenceData> randomLevelsRules = item.Value;

                shuffledSelectors[levelDifficulty] = new ShuffledSelector<LevelSequenceData>(randomLevelsRules, 12345);
            }
        }

        public async UniTask SetCurrentLevelAsync(int index, CancellationToken cancellationToken)
        {
            CurrentLevelIndex = Math.Max(0, index);
            currentLevelId = GetLevelByIndex(CurrentLevelIndex).levelId;

            if (currentLevelScriptableObjectId != string.Empty)
                assetProvider.ReleaseAsset(currentLevelScriptableObjectId);

            (LevelData levelData, LevelConfiguration.Level level, bool overrideFromConfig) = await GetLevelCfgData(GetCurrentLevelId(), cancellationToken);

            if (!overrideFromConfig)
                currentLevelScriptableObjectId = levelData.id;

            currentLevelData = levelData;
            currentLevel = level;
        }

        private async UniTask<(LevelData levelData, LevelConfiguration.Level level, bool overrideFromConfig)> GetLevelCfgData(string levelId, CancellationToken cancellationToken) 
        {
            if (!systemConfig.SettingsData.useLevelFiles || levelConfig.HasLevel(levelId))
            {
                var configData = levelConfig.GetLevelById(levelId);
                return (configData.levelData, configData.level, true);
            }                

            LevelScriptableObject levelSO = await assetProvider.AddressableLoadAssetAsync<LevelScriptableObject>(levelId, cancellationToken);
            var soData = levelSO.GetLevel();
            return (soData.levelData, soData.level, false);
        }


        private LevelSequenceData GetLevelByIndex(int index)
        {
            if (index < levelSequenceConfig.LevelSequence.Count)
            {
                return levelSequenceConfig.LevelSequence[index];
            }

            int levelDifficultyIndex = index % levelSequenceConfig.RandomLevelsDifficultySequence.Count;
            LevelDifficulty levelDifficulty = levelSequenceConfig.RandomLevelsDifficultySequence[levelDifficultyIndex];
            if (!shuffledSelectors.ContainsKey(levelDifficulty))
            {
                levelDifficulty = LevelDifficulty.Default;
            }

            if (!shuffledSelectors[levelDifficulty].HasItems)
            {
                shuffledSelectors[levelDifficulty].Reset();
            }

            return shuffledSelectors[levelDifficulty].GetRandomItem(index);
        }
    }
}