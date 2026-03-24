using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Popups
{
    public abstract class OpenCloseAnimator : MonoBehaviour
    {
        public Action OnOpenComplete { get; set; }
        public Action OnCloseComplete { get; set; }

        public abstract void Initialize();
        public abstract void Deinitialize();
        public abstract void ActivateAsInvisible();
        public abstract void Open();
        public abstract void Close();
        public abstract void InstantClose();
        public abstract void ForceStop();

        public abstract UniTask OpenAsync(CancellationToken token);
        public abstract UniTask CloseAsync(CancellationToken token);


    }
}