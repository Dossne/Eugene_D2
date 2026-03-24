using System;

namespace Infrastructure.Configuration
{
    [Serializable]
    public class ExampleListConfigurationData : ListConfigurationData<int>
    {
        public string suffix;
    }
}