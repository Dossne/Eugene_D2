using System.Threading;
using Features.Events;
using Features.LevelUp;

namespace Features.Boosters.Behaviour
{
    public class BoostBottleWinStreakBehaviour : WinStreakBehaviour
    {
        private readonly ExperienceAddRequest experienceAddRequest;


        public BoostBottleWinStreakBehaviour(WinStreakBoosterData data, ExperienceAddRequest experienceAddRequest) : base(data)
        {
            this.experienceAddRequest = experienceAddRequest;
        }


        public override void ApplyAsync(CancellationToken cancellationToken)
        {
            experienceAddRequest.RequestAdd(Source.BoostBottle, (int)data.boostedValue);
        }
    }
}