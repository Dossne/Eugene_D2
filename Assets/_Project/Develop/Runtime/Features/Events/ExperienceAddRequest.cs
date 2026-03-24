using Features.LevelUp;
using R3;

namespace Features.Events
{
    public class ExperienceAddRequest : ReactiveCommand<ExperienceArgs>
    {
        public void RequestAdd(Source source, int addValue)
        {
            base.Execute(new ExperienceArgs { source = source, addValue = addValue });
        }
    }
}