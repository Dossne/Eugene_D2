using Infrastructure.Pool;

namespace Features.ScoringVisualize
{
    public class ScoringTextPool : ComponentPoolSingle<ScoringTextView>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(mainUIProvider.CountersRoot);
        }
    }
}