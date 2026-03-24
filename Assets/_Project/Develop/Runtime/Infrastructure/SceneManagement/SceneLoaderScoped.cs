using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Infrastructure.SceneManagement
{
    /// <summary>
    /// For load additive scenes when project uses VContainer
    /// </summary>
    public class SceneLoaderScoped : SceneLoaderBase
    {
        private readonly LifetimeScope mainSceneScope;
        private bool isEnqueueInProcess;
    
        public SceneLoaderScoped(LifetimeScope mainSceneScope)
        {
            this.mainSceneScope = mainSceneScope;
        }


        public override async UniTask<Scene> SwitchAdditiveScene(string targetSceneName, LoadSceneMode mode, CancellationToken cancellationToken = default, Action onCompleteCallback = null)
        {
            if (isEnqueueInProcess)
            {
                await UniTask.WaitUntil(IsEnqueueComplete, cancellationToken: cancellationToken);
                
                if(TryGetRegisteredScene(targetSceneName, out  var registeredScene))
                    return registeredScene.GetScene();
            }
            
            isEnqueueInProcess = true;

            using (LifetimeScope.EnqueueParent(mainSceneScope))
            {
                Scene newScene = await base.SwitchAdditiveScene(targetSceneName, mode, cancellationToken, onCompleteCallback);
                LifetimeScope sceneLts = LifetimeScope.Find<LifetimeScope>(newScene);
                sceneLts.Build();
                onCompleteCallback?.Invoke();
                isEnqueueInProcess = false;

                return newScene;
            }
        }


        private bool IsEnqueueComplete()
        {
            return !isEnqueueInProcess;
        }
    }
}