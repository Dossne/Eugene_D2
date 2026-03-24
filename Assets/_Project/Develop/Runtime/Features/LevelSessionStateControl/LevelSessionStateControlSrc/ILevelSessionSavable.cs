namespace Features.LevelSessionStateControl
{
    public interface ILevelSessionSavable
    {
        void RestoreSessionState(LevelSessionData sessionData);

        void SaveSessionState(LevelSessionData sessionData);
    }
}