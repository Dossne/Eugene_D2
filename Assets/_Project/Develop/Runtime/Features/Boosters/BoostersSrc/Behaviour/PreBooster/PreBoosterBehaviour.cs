using System.Threading;
using Features.Boosters.Model;

namespace Features.Boosters.Behaviour.PreBooster
{
    public abstract class PreBoosterBehaviour
    {
        protected readonly BoosterModel model;


        protected PreBoosterBehaviour(BoosterModel model)
        {
            this.model = model;
        }


        public abstract void ApplyAsync(CancellationToken cancellationToken);


        public void DecreaseCount()
        {
            model.Decrease();
        }
    }
}