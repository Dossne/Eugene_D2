using Cysharp.Threading.Tasks;
using Infrastructure.AudioControl;
using Infrastructure.Popups;
using System;
using System.Threading;
using UnityEngine;

namespace Features.JellyHoleUi
{
    public class JellyHolePopup : PopupBase
    {
        [SerializeField] private Animator jellyHoleAnimator;
        [SerializeField] private float afterAnimationDelaySec;
        private CancellationTokenSource cts;

        public override void Open()
        {          
            cts = new CancellationTokenSource();
            base.Open();
            jellyHoleAnimator.Play("Play");
            AwaitAnimationStop(jellyHoleAnimator.GetCurrentAnimatorStateInfo(0).length + afterAnimationDelaySec, cts.Token).Forget();
            AudioService.I.PlaySfx(SfxType.JellyHolePopupOpen);
        }

        private async UniTaskVoid AwaitAnimationStop(float delay, CancellationToken cancellationToken)
        {
            await UniTask.WaitForSeconds(delay, true, cancellationToken: cancellationToken); 
            Close();
        }

        protected override void OnBeginClose()
        {
            cts?.Cancel();
            cts?.Dispose();
            base.OnBeginClose();
        }
    }
}
