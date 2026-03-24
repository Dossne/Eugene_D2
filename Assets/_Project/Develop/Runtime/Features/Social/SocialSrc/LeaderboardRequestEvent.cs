using R3;


namespace Features.Social
{
    public class LeaderboardRequestEvent : ReactiveCommand
    {
        public string LeaderboardName { get; private set; }
        public LeaderboardRequestType LeaderboardRequestType { get; private set; }
        public LeaderboardRequestState LeaderboardRequestState { get; private set; }



        public void Execute(string leaderboardName, LeaderboardRequestType leaderboardRequestType, LeaderboardRequestState leaderboardRequestState)
        {
            LeaderboardName = leaderboardName;
            LeaderboardRequestType = leaderboardRequestType;
            LeaderboardRequestState = leaderboardRequestState;
            base.Execute(Unit.Default);
        }
    }
}