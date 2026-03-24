
namespace Features.ScoringVisualize
{
    public struct ScoringScheduleTask
    {
        public ScoringFeature feature;
        public int addScore;

        public ScoringScheduleTask(ScoringFeature feature, int count)
        {
            this.feature = feature;
            this.addScore = count;
        }
    }
}