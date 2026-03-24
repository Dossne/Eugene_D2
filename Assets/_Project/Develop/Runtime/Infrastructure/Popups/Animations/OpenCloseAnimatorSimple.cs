using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Utilities;
using UnityEngine;

namespace Infrastructure.Popups
{
    public class OpenCloseAnimatorSimple : OpenCloseAnimator
    {
        private GameObject rootGameObject;
        
        
        public override void Initialize()
        {
            rootGameObject = gameObject;
        }


        public override void Deinitialize()
        {
            OnOpenComplete = null;
            OnCloseComplete = null;
        }


        public override void ActivateAsInvisible()
        {
            //there are no components that can be set invisible
        }


        public override void Open()
        {
            rootGameObject.SetObjectActive(true);
            OnOpenComplete?.Invoke();
        }


        public override void Close()
        {
            rootGameObject.SetObjectActive(false);
            OnCloseComplete?.Invoke();
        }

        public override void InstantClose()
        {
            rootGameObject.SetObjectActive(false);
        }

        public override void ForceStop() { }


        public override UniTask OpenAsync(CancellationToken token)
        {
            Open();
            return UniTask.CompletedTask;
        }


        public override UniTask CloseAsync(CancellationToken token)
        {
            Close();
            return UniTask.CompletedTask;
        }
    }
}