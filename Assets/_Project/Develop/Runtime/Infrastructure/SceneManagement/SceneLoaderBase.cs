using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Infrastructure.SceneManagement
{
    public class SceneLoaderBase
    {
        private Dictionary<string /*sceneName*/, RegisteredScene> loadedScenes = new();
        private bool isBusy;


        /// <summary>
        /// Load scene in single mode
        /// </summary>
        public async UniTask<Scene> LoadScene(string sceneName, LoadSceneMode mode, CancellationToken cancellationToken = default)
        {
            if (isBusy)
                await UniTask.WaitUntil(IsAvailable, cancellationToken: cancellationToken);

            isBusy = true;
            
            var scene = await LoadScene_Inner(sceneName, mode, cancellationToken);
            
            isBusy = false;
            
            return scene;
        }


        /// <summary>
        /// Unloads all active additive scene and load target scene in additive mode
        /// </summary>
        public virtual async UniTask<Scene> SwitchAdditiveScene(string targetSceneName, LoadSceneMode mode, CancellationToken cancellationToken = default,
                                                                Action onCompleteCallback = null)
        {
            if (isBusy)
            {
                await UniTask.WaitUntil(IsAvailable, cancellationToken: cancellationToken);

                if (IsSceneRegistered(targetSceneName, out RegisteredScene registeredScene))
                {
                    return registeredScene.GetScene();
                }
            }

            isBusy = true;
            
            string unloadSceneName = GetActiveSceneName();
            await UnloadAdditiveScene(unloadSceneName, cancellationToken);

            var scene = await LoadScene_Inner(targetSceneName, mode, cancellationToken);
            
            isBusy = false;

            return scene;
        }


        public async UniTask UnloadAllAdditiveScenes(CancellationToken cancellationToken)
        {
            if (SceneManager.sceneCount < 1)
                return;

            if (isBusy)
            {
                await UniTask.WaitUntil(IsAvailable, cancellationToken: cancellationToken);
            }

            isBusy = true;

            Scene singleModeScene = SceneManager.GetSceneAt(0);
            await UnloadScenes(new List<string> { singleModeScene.name }, cancellationToken);

            isBusy = false;
        }


        /// <summary>
        /// Get last loaded in additive mode scene name
        /// </summary>
        public string GetActiveSceneName()
        {
            string sceneName = string.Empty;

            foreach (KeyValuePair<string, RegisteredScene> entry in loadedScenes)
            {
                if (entry.Value.IsLoaded && entry.Value.LoadMode == LoadSceneMode.Additive)
                {
                    sceneName = entry.Key;
                }
            }

            return sceneName;
        }


        public async UniTask DeinitializeAsync(CancellationToken cancellationToken = default)
        {
            if (isBusy)
            {
                await UniTask.WaitUntil(IsAvailable, cancellationToken: cancellationToken);
            }

            isBusy = true;
            
            await UnloadAsync(cancellationToken);
            loadedScenes.Clear();
            
            isBusy = false;
        }


        protected bool TryGetRegisteredScene(string sceneName, out RegisteredScene registeredScene)
        {
            return loadedScenes.TryGetValue(sceneName, out registeredScene);
        }


        private async UniTask UnloadAsync(CancellationToken cancellationToken = default)
        {
            foreach (KeyValuePair<string, RegisteredScene> entry in loadedScenes)
            {
                if (entry.Value.IsLoaded)
                {
                    await entry.Value.UnloadAsync(cancellationToken);
                }
            }

            await UnloadUnused(cancellationToken);
        }


        private async UniTask UnloadScenes(List<string> exceptionNames = null, CancellationToken cancellationToken = default)
        {
            int loadedScenesTemp = SceneManager.sceneCount;

            for (int i = loadedScenesTemp - 1; i >= 0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (exceptionNames != null && exceptionNames.Contains(scene.name))
                {
                    continue;
                }

                await UnloadSceneAsync(scene.name, cancellationToken);
            }

            if (loadedScenesTemp > SceneManager.sceneCount)
            {
                await UnloadUnused(cancellationToken);
            }

            if (exceptionNames == null || exceptionNames.Count == 0)
            {
                await UnloadAsync(cancellationToken);
            }
        }


        private async UniTask UnloadAdditiveScene(string sceneNameToUnload, CancellationToken cancellationToken = default)
        {
            int loadedScenesTemp = SceneManager.sceneCount;

            if (loadedScenesTemp < 1)
            {
                return;
            }

            Scene sceneByName = SceneManager.GetSceneByName(sceneNameToUnload);

            if (!sceneByName.IsValid())
            {
                return;
            }

            await UnloadSceneAsync(sceneNameToUnload, cancellationToken);
            await UnloadUnused(cancellationToken);
        }


        private async UniTask<Scene> LoadScene_Inner(string sceneName, LoadSceneMode loadMode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new Exception("Scene name is null or empty!");
            }

            if (IsSceneRegistered(sceneName, out RegisteredScene registeredScene))
            {
                return registeredScene.GetScene();
            }


            if (loadMode == LoadSceneMode.Single)
            {
                await UnloadAsync(cancellationToken);
            }

            AsyncOperationHandle<SceneInstance>? handle = await LoadSceneAsync_Inner(sceneName, loadMode, cancellationToken);

            SetSceneAsLoaded(sceneName, handle, loadMode);
            SetSceneAsActive(handle.Value.Result.Scene);

#if PR_CHEAT
            DebugLog_Load(sceneName, loadMode, handle.Value.Status);
#endif

            return handle.Value.Result.Scene;
        }


        private void SetSceneAsLoaded(string sceneName, AsyncOperationHandle<SceneInstance>? handle, LoadSceneMode loadMode)
        {
            if (!loadedScenes.ContainsKey(sceneName))
            {
                loadedScenes.Add(sceneName, new RegisteredScene());
            }

            loadedScenes[sceneName].SetAsLoaded(handle, loadMode);
        }


        //For placing new instances of objects on active scene
        private void SetSceneAsActive(Scene scene)
        {
            SceneManager.SetActiveScene(scene);
        }


        private async UniTask UnloadSceneAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            bool isRegistered = IsSceneRegistered(sceneName, out RegisteredScene registeredScene);

            if (isRegistered)
            {
                await registeredScene.UnloadAsync(cancellationToken);
            }
            else
            {
                await SceneManager.UnloadSceneAsync(sceneName).ToUniTask(cancellationToken: cancellationToken);
            }

#if PR_CHEAT
            DebugLog_Unload(sceneName);
#endif
        }


        private async UniTask UnloadUnused(CancellationToken cancellationToken = default)
        {
            await Resources.UnloadUnusedAssets().ToUniTask(cancellationToken: cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                GC.Collect();
        }


        private async UniTask<AsyncOperationHandle<SceneInstance>> LoadSceneAsync_Inner(string sceneName, LoadSceneMode loadSceneMode,
                                                                                        CancellationToken cancellationToken = default)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneName, loadSceneMode);
            await handle.ToUniTask(cancellationToken: cancellationToken);
            return handle;
        }


        private bool IsAvailable()
        {
            return !isBusy;
        }


        private bool IsSceneRegistered(string sceneName, out RegisteredScene registeredScene)
        {
            return loadedScenes.TryGetValue(sceneName, out registeredScene) && registeredScene.IsLoaded;
        }


#if PR_CHEAT
        private bool isDebugLog /*= true*/;


        private void DebugLog_Load(string sceneName, LoadSceneMode mode, AsyncOperationStatus currentStatus)
        {
            if (isDebugLog)
            {
                Debug.Log(currentStatus == AsyncOperationStatus.Succeeded
                              ? $"Scene \"{sceneName}\" loaded successfully in LoadSceneMode: \"{mode}\""
                              : $"Failed to load scene \"{sceneName}\" in LoadSceneMode: \"{mode}\"");
            }
        }


        private void DebugLog_Unload(string sceneName)
        {
            if (isDebugLog)
            {
                Debug.Log($"Scene \"{sceneName}\" unloaded successfully in LoadSceneMode: Additive");
            }
        }
#endif
    }
}