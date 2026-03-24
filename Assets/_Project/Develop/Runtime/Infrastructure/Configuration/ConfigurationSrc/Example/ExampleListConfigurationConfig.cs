using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Configuration
{
    [CreateAssetMenu(fileName = "ExampleListConfigurationConfig", menuName = "Config/Example/ExampleListConfigurationConfig")]
    public class ExampleListConfigurationConfig : ListConfiguration<int, ExampleListConfigurationData>
    {
        public override string GoogleSheetName { get; set; }
        public override string GoogleTableName { get; set; } = "ExampleListConfigurationConfig";

#if UNITY_EDITOR

        protected override void GenerateName_Editor()
        {
            configurationData.ForEach(data => { data.SetName_Editor($"{data.id}_{data.suffix}"); });
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configurationData = GoogleDocsUtils.SnatchDataRowsToConfig<ExampleListConfigurationData>(tableData);
        }

#endif

    }
}