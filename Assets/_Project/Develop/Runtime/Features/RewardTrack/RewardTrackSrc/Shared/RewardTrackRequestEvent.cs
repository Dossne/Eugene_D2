using R3;

namespace Features.RewardTrack
{
    public enum RTRequestAction
    {
        Add,
        AddMax,
        Reset
    }

    public struct RTRequest
    {
        public RTRequestAction action;
        public int addCount;
    }

    public class RewardTrackRequestEvent : ReactiveCommand<RTRequest>
    {
        public void Request(RTRequest evt)
        {
            Execute(evt);
        }
    }
}