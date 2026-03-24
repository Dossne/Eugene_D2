namespace Infrastructure.BroTweens
{
    public enum RotateMode
    {
        /// <summary>Never rotates beyond 360°: 750° => 30°</summary>
        Shortest = 0,
        /// <summary>Rotates beyond 360°: 750° rotates 2 times + 30°</summary>
        Full = 1,
    }
}