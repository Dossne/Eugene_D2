using System;

namespace Infrastructure.Configuration
{
    [Serializable]
    public class ExampleDictionaryConfigurationData : SerializableDictionaryConfigurationData<int>
    {
        public string suffix;
    }
}