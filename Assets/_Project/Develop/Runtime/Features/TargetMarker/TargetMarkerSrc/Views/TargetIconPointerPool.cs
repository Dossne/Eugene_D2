using Infrastructure.Pool;

namespace Features.TargetMarker
{
    public class TargetIconPointerPool : ComponentPoolSingle<PoolableTargetIconPointer>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(mainUIProvider.TargetMarkersRoot.Clamped);
        }
    }
}