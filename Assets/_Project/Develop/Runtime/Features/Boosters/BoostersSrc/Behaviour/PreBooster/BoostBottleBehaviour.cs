using System.Threading;
using Features.Boosters.Model;
using Features.Events;
using Features.LevelUp;

namespace Features.Boosters.Behaviour.PreBooster
{
    public class BoostBottleBehaviour : PreBoosterBehaviour
    {
        private readonly ExperienceAddRequest experienceAddRequest;


        public BoostBottleBehaviour(BoosterModel model, ExperienceAddRequest experienceAddRequest) : base(model)
        {
            this.experienceAddRequest = experienceAddRequest;
        }


        public override void ApplyAsync(CancellationToken cancellationToken)
        {
            experienceAddRequest.RequestAdd(Source.BoostBottle, (int)model.BoostedValue);
        }
    }
}