using Infrastructure.Pool;

namespace Features.TargetMarker
{
    public class TargetMarkerPool : ComponentPoolSingle<PoolableTargetMarker>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(mainUIProvider.TargetMarkersRoot.Main);
        }
    }
}