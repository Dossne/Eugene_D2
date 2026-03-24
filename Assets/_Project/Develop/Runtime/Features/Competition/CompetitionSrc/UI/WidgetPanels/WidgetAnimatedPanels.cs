using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Reward;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.Competition
{
    public class WidgetAnimatedPanels : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private GameObject maskRoot;
        [SerializeField] private RewardPanelView rewardPanelView;
        [SerializeField] private MultiplierPanelView multiplierPanelView;

        public void Initialize()
        {
            SetRootActive(true);
        }

        public void ConstructRewardPanel(RewardItemVisualData reward)
        {
            rewardPanelView.Construct(reward);
        }

        public void InitializeRewardPanel()
        {
            rewardPanelView.Initialize();
        }

        public UniTask PlayRewardAnimationAsync(CancellationToken token)
        {
            return rewardPanelView.PlayAsync(token);
        }

        public void ConstructMultiplierPanel(int multiplierCurrent, int multiplierNext)
        {
            multiplierPanelView.Construct(multiplierCurrent, multiplierNext);
        }

        public void InitializeMultiplierPanel()
        {
            multiplierPanelView.Initialize();
        }

        public UniTask PlayMultiplierAnimationAsync(CancellationToken token)
        {
            return multiplierPanelView.PlayAsync(token);
        }

        public void Deinitialize()
        {
            SetRootActive(false);
            rewardPanelView.Deinitialize();
            multiplierPanelView.Deinitialize();
        }

        private void SetRootActive(bool isActive)
        {
            maskRoot.SetObjectActive(isActive);
        }
    }
}