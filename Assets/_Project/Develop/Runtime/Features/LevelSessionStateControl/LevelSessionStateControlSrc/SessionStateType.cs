namespace Features.LevelSessionStateControl
{
    public enum SessionStateType
    {
        None = 0,
        InProcess = 1,
        PendingResurrect = 2,
        Win = 3,
        Loose = 4,
    }
}