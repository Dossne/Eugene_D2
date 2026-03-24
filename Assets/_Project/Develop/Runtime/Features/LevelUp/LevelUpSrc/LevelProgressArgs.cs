using Infrastructure.Utilities;

namespace Features.LevelUp
{
    public enum Source
    {
        None = 0,
        ExpLevelUp = 1, //level up manager (by change exp.level)
        SizeBooster = 2, //temp by booster system
        BoostBottle = 3, //const by booster system
    }

    public struct LevelProgressArgs
    {
        public Source source;
        public ActionType type;
        public int levelByCurrentXp;
        public int diff;


        public LevelProgressArgs(Source source, ActionType type, int levelByCurrentXp, int diff)
        {
            this.source = source;
            this.type = type;
            this.levelByCurrentXp = levelByCurrentXp;
            this.diff = diff;
        }
    }
}