namespace Infrastructure.Popups
{
    public enum PopupState : byte
    {
        None = 0,
        BeginOpen = 1,
        Opened = 2,
        BeginClose = 3,
        Closed = 4,
    }
}