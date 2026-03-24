using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Utilities;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [CreateAssetMenu(fileName = "Example_AsyncTweenCancel", menuName = "Config/System/BroTween/Example_AsyncTweenCancel")]
    public class Example_AsyncTweenCancel : ScriptableObject
    {
        [SerializeField] private BroTweenSafe tween1;
        [SerializeField] private BroTweenSafe tween2;


        /// <summary>
        /// Do not forget set debug = true in BroTweenUniTask
        /// Check if cancel token of one tween task do not break another one with same id.
        /// Situation:
        /// 1. Tween 1 (id1) started as UniTask;
        /// 2. Tween 1 (id1) was stopped by another call (between scenes complete), but the UniTask is still awaiting somewhere;
        /// 3. Tween 1 (id1) returned to the pool and then started as Tween2 (same id1, but gen is next);
        /// 4. Cancellation token for tween 1 should not break Tween2. Tween 2 should play to the end (FinishTween in console)
        /// </summary>
        [TriInspector.Button]
        private void Test1()
        {
            PlayStopAndPlayTweenAsync().Forget();
        }

        
        private async UniTask PlayStopAndPlayTweenAsync()
        {
            tween1.Kill();
            tween2.Kill();
            
            CancellationTokenSource cts = new CancellationTokenSource();
            SomeLogicForTween1Async(cts.Token).Forget();
            
            await UniTask.DelayFrame(10, cancellationToken: cts.Token);
            tween1.Stop();
            
            CancellationTokenSource cts2 = new CancellationTokenSource();
            SomeLogicForTween2Async(cts2.Token).Forget();

            cts.Cancel();
            cts.Dispose();
        }


        /// <summary>
        /// Do not forget set debug = true in BroTweenUniTask
        /// Check if cancel token of one tween task do not break another one with same id.
        /// Situation:
        /// 1. Tween 1 (id1) started as UniTask;
        /// 2. Tween 1 (id1) was cancelled;
        /// 3. Tween 1 (id1) returned to the pool and then started as Tween2 (same id1, but gen is next);
        /// 4. Cancellation token for tween 1 should not break Tween2. Tween 2 should play to the end (FinishTween in console)
        /// </summary>
        
        [TriInspector.Button]
        private void Test2()
        {
            PlayCancelAndPlayAgainTweenAsync().Forget();
        }
        
        
        private async UniTask PlayCancelAndPlayAgainTweenAsync()
        {
            tween1.Kill();
            tween2.Kill();
            CancellationTokenSource cts = new CancellationTokenSource();
            SomeLogicForTween1Async(cts.Token).Forget();
            await UniTask.DelayFrame(10, cancellationToken: cts.Token);
            cts.Cancel();
            cts.Dispose();
            CancellationTokenSource cts2 = new CancellationTokenSource();
            SomeLogicForTween2Async(cts2.Token).Forget();
        }


        private async UniTask SomeLogicForTween1Async(CancellationToken token)
        {
            var tween = GetBroTween();
            Debug.Log($"****StartTween 1. Id: {tween.UniqueId}. Gen: {tween.Generation}. Frame: {Time.frameCount}");

            tween1 = tween.ToSafe();
            tween1.Play();
            await tween1.ToUniTask(cancellationToken: token);
            
            string ok = "TEST OK".ToColor(ColorHex.Green);
            Debug.Log($"********{ok}. FinishTween 1. Id: {tween.UniqueId}. Gen: {tween.Generation}. Frame: {Time.frameCount}");
        }


        private async UniTask SomeLogicForTween2Async(CancellationToken token)
        {
            var tween = GetBroTween();
            Debug.Log($"****StartTween 2. Id: {tween.UniqueId}. Gen: {tween.Generation}. Frame: {Time.frameCount}");

            tween2 = tween.ToSafe();
            tween2.Play();

            await tween2.ToUniTask(cancellationToken: token);
            
            string ok = "TEST OK".ToColor(ColorHex.Green);
            Debug.Log($"****{ok}. FinishTween 2. Id: {tween.UniqueId}. Gen: {tween.Generation}. Frame: {Time.frameCount}");
        }
        
        
        private BroTweenBase GetBroTween()
        {
            return BroTween.Int(IntTest, 0, 3, 3).SetUpdate(true);
        }


        private void IntTest(int progress)
        {
            Debug.Log($"IntTest. Progress {progress}. Frame: {Time.frameCount}");
        }
    }
}