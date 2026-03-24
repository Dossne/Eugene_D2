using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Features.Social;
using UnityEngine;
using VContainer;

namespace Features.Competition
{
    public class CompetitionDebugger : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private List<LeaderboardRecord> records;

        [SerializeField] private bool swapPlayerIdx;
        [SerializeField] private int playerIdxToSwap;
        [SerializeField] private int serverRewardIdx = -1;

        private CompetitionManager manager;

        [Inject]
        public void Inject(CompetitionManager manager)
        {
            this.manager = manager;
        }

        [TriInspector.Button]
        private void GetRecords()
        {
            GetRecordsAsync().Forget();
        }

        private async UniTask GetRecordsAsync()
        {
            var list = await manager.GetLeaderboardPositionsStateAsync(gameObject.GetCancellationTokenOnDestroy());

            if (swapPlayerIdx)
            {
                LeaderboardRecordsActualizer.Swap(list.records, list.actualIdx, playerIdxToSwap);
            }

            var data = manager.GetActualizedRecords(list.records, serverRewardIdx);
            records = data.records;
        }
#endif
    }
}