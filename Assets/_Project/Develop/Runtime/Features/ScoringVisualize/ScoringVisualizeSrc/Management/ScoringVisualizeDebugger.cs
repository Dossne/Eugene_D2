using UnityEngine;

#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using VContainer;
#endif

namespace Features.ScoringVisualize
{
    public class ScoringVisualizeDebugger : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private int addCount = 10;

        private ScoringVisualizeService scoringVisualizeService;

        [Inject]
        public void Inject(ScoringVisualizeService service)
        {
            this.scoringVisualizeService = service;
        }

        [TriInspector.Button]
        public void TestCompetition()
        {
            scoringVisualizeService.Schedule(ScoringFeature.Competition, addCount);
            scoringVisualizeService.ExecuteScheduledAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
        }

        /*[TriInspector.Button]
        public void TestMany()
        {
            scoringVisualizeService.Schedule(ScoringFeature.Competition, addCount);
            scoringVisualizeService.Schedule(ScoringFeature.SuperDiscount, addCount);
            scoringVisualizeService.Schedule(ScoringFeature.FreePaidOffer, addCount);

            scoringVisualizeService.ExecuteScheduledAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
        }*/

#endif
    }
}