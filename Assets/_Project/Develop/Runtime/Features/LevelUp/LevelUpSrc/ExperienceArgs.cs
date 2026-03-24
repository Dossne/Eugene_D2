namespace Features.LevelUp
{
    public struct ExperienceArgs
    {
        public Source source;
        public int addValue;


        public ExperienceArgs(Source source, int addValue)
        {
            this.source = source;
            this.addValue = addValue;
        }
    }
}