namespace Infrastructure.PersistentProgress
{
    public interface ISavable 
    {
        void Load(Progress progress);
        void Save(Progress progress);
    }
}