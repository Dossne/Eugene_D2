using Infrastructure.Pool;

namespace Features.RewardTrack
{
    public class FloatingIconRewardTrackPool : ComponentPoolSingle<PoolableFloatingIconRewardTrack>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(mainUIProvider.OverPopupRoot);
        }
    }
}