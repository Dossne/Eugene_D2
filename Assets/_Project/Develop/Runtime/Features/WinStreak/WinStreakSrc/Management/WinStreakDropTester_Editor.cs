#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using VContainer;
#endif

using UnityEngine;

namespace Features.WinStreak
{
    public class WinStreakDropTester_Editor : MonoBehaviour
    {
#if UNITY_EDITOR
        private WinStreakBonusApplier winStreakBonusApplier;
        
        [SerializeField, Range(0, 5)] private int level;


        [Inject]
        public void Construct(WinStreakBonusApplier winStreakBonusApplier)
        {
            this.winStreakBonusApplier = winStreakBonusApplier;
        }


        [TriInspector.Button]
        public void DropCollectables()
        {
            winStreakBonusApplier.DropCollectableBoostersAsync(level, gameObject.GetCancellationTokenOnDestroy()).Forget();
        }
#endif
    }
}