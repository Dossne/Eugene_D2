using Infrastructure.Pool;

namespace Features.ScoringVisualize
{
    public class ScoringIconPool : ComponentPool<ScoringFeature, ScoringIconView>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(mainUIProvider.CountersRoot);
        }
    }
}