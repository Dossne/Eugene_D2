using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Infrastructure.DateTimeControl
{
    [Serializable]
    public class DateTimeServiceData
    {
        [Header("Shared")]
        [Tooltip("Show logs of current process")] public bool showLogs;

        [Header("Network communication and sync settings")]
        [Tooltip("Sync with ntp servers max time (all connection attempts will be canceled after this time)")] public float synchronizationTimeMaxSec = 10f;
        [Tooltip("Time to wait before requesting time from same NTP server (64 to 1024)")] public int ntpServerCooldownSec = 64;
        [Tooltip("Time to wait on a NTP server response before canceling")] public int networkTimeoutMs = 2000;
        [Tooltip("Allowed timeouts to a NTP server before excluding it as a request option")] public int ntpRequestMaxFails = 2;
        [Tooltip("Allowed time to be paused before requiring a resync to a NTP server")] public int allowedPauseSec = 5;
        [Tooltip("Minimum time to wait before trying to connect to a NTP " +
                 "server after all connections failed")] public float waitForNetworkMinSec = 1f;
        [Tooltip("Maximum time to wait before trying to connect to a NTP server as each failed attempt " +
                 "increases the wait time by 1 second")] public float waitForNetworkMaxSec = 30f;
        [Tooltip("Maximum times to connect all servers in list in 1 method call")] public float syncMaxTryCount = 1f;
        [Tooltip("Diff between cached server time and calculated system time")] public float allowedOffSyncSec = 1f;
    }

    [Serializable]
    public class DateTimeServerData
    {
        public string address;
    }

    [Serializable]
    public class DateTimeServiceDataConfig
    {
        public List<DateTimeServiceData> datas;
        public List<DateTimeServerData> nptServers;
    }

    [CreateAssetMenu(fileName = "DateTimeServiceConfig", menuName = "Config/System/DateTimeServiceConfig")]
    public class DateTimeServiceConfig : JsonConvertableConfig
    {
        [SerializeField] private DateTimeServiceDataConfig configData;

        public List<DateTimeServerData> NptServers => configData.nptServers;
        public DateTimeServiceData ServiceData => configData.datas[0];

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "DateTimeServiceConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (DateTimeServiceDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(DateTimeServiceDataConfig);
        private List<DateTimeServiceData> ConfigData => configData.datas;

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(ConfigData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.nptServers, nameof(NptServers), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<DateTimeServiceData>(tableData);
        }
#endif
    }
}