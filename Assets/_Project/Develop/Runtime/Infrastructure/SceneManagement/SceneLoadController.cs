using Cysharp.Threading.Tasks;
using Infrastructure.Configs;
using System;
using System.Threading;
using Features.LevelSessionStateControl;
using Features.MainMenuUnlock;
using Infrastructure.LoadScreen;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Infrastructure.SceneManagement
{
    public class SceneLoadController
    {
        public event Action<string, LoadSceneMode> OnProcessBegin;
        public event Action<string, LoadSceneMode> OnProcessFinish;

        private readonly SceneLoaderScoped sceneLoaderScoped;
        private readonly ConfigProvider configProvider;
        private readonly LevelSessionStateService levelSessionStateService;
        private readonly LoadScreenController loadScreenController;
        private readonly MainMenuUnlockInfoService mainMenuUnlockInfoService;
        private readonly CancellationTokenSource cts;
        private bool isRemoteUpdateScheduled = false;


        [Inject]
        public SceneLoadController(LifetimeScope mainSceneScope,
                                   ConfigProvider configProvider,
                                   LevelSessionStateService levelSessionStateService,
                                   LoadScreenController loadScreenController,
                                   MainMenuUnlockInfoService mainMenuUnlockInfoService)
        {
            sceneLoaderScoped = new(mainSceneScope);
            this.configProvider = configProvider;
            this.levelSessionStateService = levelSessionStateService;
            this.loadScreenController = loadScreenController;
            this.mainMenuUnlockInfoService = mainMenuUnlockInfoService;
            cts = new CancellationTokenSource();
        }


        public bool IsGameSceneActive => sceneLoaderScoped != null && sceneLoaderScoped.GetActiveSceneName() == SceneNames.Game;
        public bool IsShowQuitButton => mainMenuUnlockInfoService.IsUnlocked && sceneLoaderScoped != null && sceneLoaderScoped.GetActiveSceneName() == SceneNames.Game;
        public bool IsMetaSceneActive => sceneLoaderScoped != null && sceneLoaderScoped.GetActiveSceneName() == SceneNames.Meta;

        public void ScheduleRemoteUpdate() => isRemoteUpdateScheduled = true;


        public void LoadNextScene()
        {
            string targetSceneName = IsLoadGameLevel() ? SceneNames.Game : SceneNames.Meta;
            SwitchAdditiveScene(targetSceneName, cts.Token).Forget();
        }


        public void LoadGameScene()
        {
            SwitchAdditiveScene(SceneNames.Game, cts.Token).Forget();
        }


        public void LoadMetaScene()
        {
            SwitchAdditiveScene(SceneNames.Meta, cts.Token).Forget();
        }


        public UniTask<Scene> LoadNextSceneAsync(CancellationToken cancellationToken)
        {
            string targetSceneName = IsLoadGameLevel() ? SceneNames.Game : SceneNames.Meta;
            return SwitchAdditiveScene(targetSceneName, cancellationToken);
        }


        public async UniTask LoadMainSceneAsync()
        {
            string sceneName = SceneNames.Main;
            LoadSceneMode single = LoadSceneMode.Single;
            OnProcessBegin?.Invoke(sceneName, single);
            await sceneLoaderScoped.LoadScene(sceneName, single, CancellationToken.None); //no cancellation because of main scope destroy
            OnProcessFinish?.Invoke(sceneName, single);
        }


        public UniTask UnloadAllAdditiveScenes(CancellationToken cancellationToken)
        {
            return sceneLoaderScoped.UnloadAllAdditiveScenes(cancellationToken);
        }


        public UniTask DeinitializeAsync()
        {
            cts.Cancel();
            cts.Dispose();
            return sceneLoaderScoped.UnloadAllAdditiveScenes(CancellationToken.None); //no cancellation because of main scope destroy
        }


        private async UniTask<Scene> SwitchAdditiveScene(string targetSceneName, CancellationToken cancellationToken = default, Action onCompleteCallback = null)
        {
            await loadScreenController.OpenAsync(LoadScreenUI.Reason.LoadScene, false, cancellationToken);
            
            LoadSceneMode additive = LoadSceneMode.Additive;
            OnProcessBegin?.Invoke(targetSceneName, additive);

            //For delayed remote config update for certain players group
            //Hide it under loadscreen of additive scene
            if (isRemoteUpdateScheduled)
            {
                Debug.Log("[CODE] Delayed remote config update.");
                configProvider.SetConfigsFromRemoteSource();
                isRemoteUpdateScheduled = false;
            }

            var scene = await sceneLoaderScoped.SwitchAdditiveScene(targetSceneName, additive, cancellationToken, onCompleteCallback);
            OnProcessFinish?.Invoke(targetSceneName, additive);
            return scene;
        }


        private bool IsLoadGameLevel()
        {
            return levelSessionStateService.NeedLoadGameLevel() || !mainMenuUnlockInfoService.IsUnlocked;
        }
    }
}