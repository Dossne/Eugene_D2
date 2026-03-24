namespace Infrastructure.Configuration
{
    public interface IListConfiguration
    {
#if UNITY_EDITOR
        public void SortAndRename_Editor();
#endif
    }
}