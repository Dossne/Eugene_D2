using System.Threading;

namespace Features.Boosters.Behaviour
{
    public abstract class WinStreakBehaviour
    {
        protected readonly WinStreakBoosterData data;


        protected WinStreakBehaviour(WinStreakBoosterData data)
        {
            this.data = data;
        }


        public abstract void ApplyAsync(CancellationToken cancellationToken);
    }
}