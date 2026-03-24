namespace Infrastructure.GameplayContainers
{
    public interface IRenameComponentEditor
    {
#if UNITY_EDITOR


        /// <summary>
        /// Editor only
        /// </summary>
        void RenameChildren();


#endif

    }
}