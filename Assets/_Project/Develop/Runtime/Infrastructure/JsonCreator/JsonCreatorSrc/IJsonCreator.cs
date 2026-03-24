#if UNITY_EDITOR

namespace Infrastructure.JsonCreator
{
    public interface IJsonCreator
    {
        void ToJson();

        void FromJson();
    }
}

#endif

