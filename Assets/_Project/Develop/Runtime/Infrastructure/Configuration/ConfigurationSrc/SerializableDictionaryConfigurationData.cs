using System;

namespace Infrastructure.Configuration
{
    [Serializable]
    public class SerializableDictionaryConfigurationData<K>
    {
        public K keyId = default;
    }
}