using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.ApplicationInterrupt;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Progress = Infrastructure.PersistentProgress.Progress;

namespace Infrastructure.DateTimeControl
{
    public sealed class DateTimeService : ISavable
    {
        private const string Tag = "[DateTimeService]";
        public event Action<TimeChangeReason> OnChange;

        private const int NtpUdpPort = 123;                   // Standard NTP port.
        private const int NtpMessageBytes = 48;               // Standard NTP message size.
        private const byte NtpRequestHeader = 0x1B;           // Standard NTP message header.
        private const int NtpSecondsOffsetByte = 40;          // Standard byte position for current time seconds in NTP message.
        private const int NtpFractionOfSecondOffsetByte = 44; // Standard byte position for current time fraction of seconds in NTP message.

        private readonly AppInterruptObserver appInterruptObserver;
        private readonly DateTimeServiceConfig config;
        private readonly DateTime epochTime = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly CancellationTokenSource cts;

        private readonly Dictionary<string/*host*/, IPAddress[]> cachedDns =  new();
        private (string DomainNameAddress, DateTime NextRequestTime, int FailCount)[] ntpServers;

        private DateTimeState dateTimeState;
        private DateTimeServiceData serviceData;
        private Stopwatch stopwatch;
        private DateTime lastServerNowUtc;
        private DateTime appPauseTime;
        private TimeSpan timeDifferenceUtc;

        private float realtimeSinceStartupCached;
        private float currentCheckTime;
        private bool isSynced;
        private bool syncInProcess;
        private bool isInit;

        private static float defaultTimeScale = 1.0f;


        public DateTimeService(AppInterruptObserver appInterruptObserver, ConfigProvider configProvider)
        {
            this.appInterruptObserver = appInterruptObserver;
            this.config = configProvider.DateTimeServiceConfig;
            cts = new CancellationTokenSource();
        }


        public bool IsSynced => isSynced;
        public bool IsSyncInProcess => syncInProcess;
        public TimeSpan TimeDifferenceUtc => timeDifferenceUtc;
        public float TimeSinceLastSync => Time.realtimeSinceStartup - realtimeSinceStartupCached;

        public static float DefaultTimeScale => defaultTimeScale;


        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return UniTask.CompletedTask;

            this.serviceData = config.ServiceData;
            InitializeServersAddresses();

            appInterruptObserver.Interrupt += AppInterruptObserver_Interrupt;
            appInterruptObserver.Resume += AppInterruptObserver_Resume;

#if PR_CHEAT || UNITY_EDITOR
            InitCheatShift();
#endif
            isInit = true;

            return SyncNetworkTimeAsync(cancellationToken);
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;
            
            cachedDns.Clear();
            appInterruptObserver.Interrupt -= AppInterruptObserver_Interrupt;
            appInterruptObserver.Resume -= AppInterruptObserver_Resume;
            OnChange = null;
            isInit = false;
        }


        public void Load(Progress progress)
        {
            dateTimeState = progress.appState.dateTime;
        }


        public void Save(Progress progress)
        {
        }


        public async UniTask TimeResyncAsync(CancellationToken cancellationToken)
        {
            if (syncInProcess)
            {
                await UniTask.WaitUntil(() => !syncInProcess, cancellationToken: cancellationToken);
                return;
            }

            TryGetServerTime(out DateTime serverTime);
            DateTime systemLikeServerTime = GetSystemTime() - timeDifferenceUtc;
            float diffSec = Mathf.Abs((float)(systemLikeServerTime - serverTime).TotalSeconds);
            bool needSync = diffSec > serviceData.allowedOffSyncSec || !isSynced;

            if (needSync)
            {
                isSynced = false;
                await SyncNetworkTimeAsync(cancellationToken);
            }

            if (serviceData.showLogs)
                Debug.Log($"Server=[{serverTime}]. System=[{GetSystemTime()}]. SystemWDiff=[{systemLikeServerTime}]. Diff=[{diffSec}]. NeedSync=[{needSync}]");
        }


        public void ForceTimeResync()
        {
            isSynced = false;

            if (syncInProcess)
                return;

            SyncNetworkTimeAsync(cts.Token).Forget();
        }


        public bool TryGetServerTime(out DateTime result)
        {
            if (!isSynced || lastServerNowUtc == DateTime.MinValue)
            {
                if (serviceData.showLogs)
                    Debug.Log($"{Tag} Time is not in sync with server. Returned system time utc");

                result = GetSystemTime();
                return false;
            }

            result = GetCalcedServerTime();
            return true;
        }


        public DateTime GetSystemTime()
        {
            DateTime result = DateTime.UtcNow;

#if PR_CHEAT || UNITY_EDITOR
            result = result.Add(CheatTimeShift);
#endif
            return result;
        }


        public static void SetDefaultTimeScale(float timeScale)
        {
            defaultTimeScale = timeScale;
            Time.timeScale = defaultTimeScale;
        }


        private DateTime GetCalcedServerTime()
        {
            DateTime result = lastServerNowUtc.AddSeconds(TimeSinceLastSync);

#if PR_CHEAT || UNITY_EDITOR
            result = result.Add(CheatTimeShift);
#endif
            return result;

        }


        private void OnPause(bool isPaused)
        {
            if (!isSynced || syncInProcess)
                return;

            if (isPaused)
            {
                appPauseTime = GetSystemTime();
            }
            else
            {
                DateTime systemTime = GetSystemTime();
                double appPauseForSeconds = (systemTime - appPauseTime).TotalSeconds;

                if (appPauseForSeconds < 0 || appPauseForSeconds > serviceData.allowedPauseSec)
                    isSynced = false;
            }
        }


        private async UniTask SyncNetworkTimeAsync(CancellationToken externalToken)
        {
            if (!HaveInternetConnection())
                return;

            using var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(serviceData.synchronizationTimeMaxSec));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, timeoutCts.Token);

            CancellationToken ct = linkedCts.Token;

            try
            {
                await SyncNetworkTimeInternalAsync(ct);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                Debug.Log($"{Tag} Internet connection problems. Sync timed out ({serviceData.synchronizationTimeMaxSec} sec).");
            }
            finally
            {
               syncInProcess = false;
            }
        }


        private async UniTask SyncNetworkTimeInternalAsync(CancellationToken cancellationToken)
        {
            syncInProcess = true;
            bool isSuccess = false;
            int syncTryCount = 0;
            float waitSecondsBetweenFails = serviceData.waitForNetworkMinSec;

            if (serviceData.showLogs)
                stopwatch = Stopwatch.StartNew();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                for (int i = 0; i < ntpServers.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!HaveInternetConnection())
                        break;

                    DateTime systemTimeUtc = DateTime.UtcNow;

                    if (systemTimeUtc < ntpServers[i].NextRequestTime || ntpServers[i].FailCount > serviceData.ntpRequestMaxFails)
                        continue;

                    ntpServers[i].NextRequestTime = systemTimeUtc.AddSeconds(serviceData.ntpServerCooldownSec);

                    DateTime ntpTime = await GetTimeFromNtpServerAsync(ntpServers[i].DomainNameAddress, cancellationToken);

                    if (ntpTime == DateTime.MinValue) //Fail
                    {
                        ntpServers[i].FailCount++;
                        DebugLogFail(i);
                    }
                    else // Success
                    {
                        systemTimeUtc = GetSystemTime();
                        timeDifferenceUtc = systemTimeUtc - ntpTime;
                        realtimeSinceStartupCached = Time.realtimeSinceStartup;
                        ntpServers[i].FailCount = 0;

                        lastServerNowUtc = ntpTime;
                        isSynced = true;
                        syncInProcess = false;
                        isSuccess = true;
                        OnChange?.Invoke(TimeChangeReason.ServerSync);
                        DebugLogSuccess(ntpTime, i);
                        break;
                    }
                }

                if (isSuccess)
                    break;

                syncTryCount++;

                if (syncTryCount >= serviceData.syncMaxTryCount)
                {
                    syncInProcess = false;
                    break;
                }

                // Assume no internet, so prepare for next request attempt.
                for (int i = 0; i < ntpServers.Length; i++)
                    ntpServers[i].FailCount = 0;

                if (waitSecondsBetweenFails < serviceData.waitForNetworkMaxSec)
                    waitSecondsBetweenFails = Math.Min(waitSecondsBetweenFails + 1f, serviceData.waitForNetworkMaxSec);

                DebugLogNoConnection(in waitSecondsBetweenFails);

                await UniTask.WaitForSeconds(waitSecondsBetweenFails, ignoreTimeScale: true, cancellationToken: cancellationToken);
            }
        }


        private async UniTask<DateTime> GetTimeFromNtpServerAsync(string ntpServerDnsAddress, CancellationToken cancellationToken)
        {
            try
            {
                // Prepare NTP message data.
                byte[] ntpData = new byte[NtpMessageBytes];
                ntpData[0] = NtpRequestHeader;

                // Get NTP server IP addresses.

                IPAddress[] ntpServerIpAddresses = await ResolveDnsAsync(ntpServerDnsAddress, serviceData.networkTimeoutMs, cancellationToken);

                // Validate there is an IP address to use.
                if (ntpServerIpAddresses.Length == 0)
                    throw new InvalidOperationException($"No IP address found for \"{ntpServerDnsAddress}\".");

                // Connect and send the NTP request to the first resolved IP address.
                using UdpClient thisDeviceUdpClient = new();
                thisDeviceUdpClient.Client.ReceiveTimeout = serviceData.networkTimeoutMs;

                thisDeviceUdpClient.Connect(new IPEndPoint(ntpServerIpAddresses[0], NtpUdpPort));

                DateTime sendTime = DateTime.UtcNow;

                await thisDeviceUdpClient.SendAsync(ntpData, ntpData.Length).AsUniTask().AttachExternalCancellation(cancellationToken);

                UniTask<UdpReceiveResult> receiveTask = thisDeviceUdpClient.ReceiveAsync().AsUniTask();
                UniTask timeoutTask = UniTask.Delay(serviceData.networkTimeoutMs, ignoreTimeScale: true, cancellationToken: cancellationToken);

                (bool hasResultLeft, UdpReceiveResult result) completed = await UniTask.WhenAny(receiveTask, timeoutTask);

                if (!completed.hasResultLeft)
                {
                    throw new TimeoutException("NTP receive timeout.");
                }

                UdpReceiveResult udpReceiveResult = completed.result;

                DateTime receiveTime = DateTime.UtcNow;
                ntpData = udpReceiveResult.Buffer;
                IPEndPoint ntpServerEndPoint = udpReceiveResult.RemoteEndPoint;

                // Validate the response source IP address.
                if (!ntpServerIpAddresses.Any(ipAddress => ipAddress.Equals(ntpServerEndPoint.Address)))
                    throw new InvalidOperationException($"Received message IP address does not match \"{ntpServerDnsAddress}\".");

                // Validate message is correct size, type, and version for the expected NTP response.
                if (ntpData.Length != NtpMessageBytes
                 || (ntpData[0] & 0x07) != 4
                 || ((ntpData[0] >> 3) & 0x07) != 3 && ((ntpData[0] >> 3) & 0x07) != 4)
                    throw new InvalidOperationException("Received message is not a valid size, type, or version for the expected NTP data.");

                // Validate NTP server is synchronized by checking leap indicator.
                if (((ntpData[0] >> 6) & 0x03) == 3)
                    throw new InvalidOperationException($"NTP server \"{ntpServerDnsAddress}\" is not synchronized.");

                // Extract the time from the message.
                ulong ntpSeconds = BitConverter.ToUInt32(ntpData, NtpSecondsOffsetByte);
                ulong ntpFractionOfSecond = BitConverter.ToUInt32(ntpData, NtpFractionOfSecondOffsetByte);

                // Swap the endianness if current system does not match NTP big-endian format.
                if (BitConverter.IsLittleEndian)
                {
                    ntpSeconds = SwapEndianness(ntpSeconds);
                    ntpFractionOfSecond = SwapEndianness(ntpFractionOfSecond);
                }

                // Validate round-trip time.
                TimeSpan roundTripTime = receiveTime - sendTime;
                if (roundTripTime.TotalMilliseconds > serviceData.networkTimeoutMs || roundTripTime.TotalMilliseconds < 0)
                    throw new InvalidOperationException("The system time changed too much while waiting on a NTP response.");

                // Calculate and return the NTP time.
                double ntpMilliseconds = (ntpSeconds * 1000) + (ntpFractionOfSecond * 1000 / 0x100000000L);
                DateTime ntpTime = epochTime.AddMilliseconds(ntpMilliseconds);
                return ntpTime.AddMilliseconds(-(roundTripTime.TotalMilliseconds / 2));
            }

            // Log error.
            catch (Exception e)
            {
                Debug.Log($"{Tag}. ERROR: {e}");
                return DateTime.MinValue;
            }
        }


        private uint SwapEndianness(ulong dataToConvert)
        {
            return (uint)(((dataToConvert & 0x000000ff) << 24)
                        + ((dataToConvert & 0x0000ff00) << 8)
                        + ((dataToConvert & 0x00ff0000) >> 8)
                        + ((dataToConvert & 0xff000000) >> 24));
        }


        private void InitializeServersAddresses()
        {
            var configList = config.NptServers;

            if (configList.Count == 0)
                Debug.LogError($"{Tag}: No NTP servers configured.");

            ntpServers = new (string DomainNameAddress, DateTime NextRequestTime, int FailCount)[configList.Count];

            for (var i = 0; i < configList.Count; i++)
            {
                var item = configList[i];
                ntpServers[i] = (item.address, DateTime.MinValue, 0);
            }
        }


        private bool HaveInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        
        private async UniTask<IPAddress[]> ResolveDnsAsync(string host, int timeoutMs, CancellationToken ct)
        {
            if(cachedDns.TryGetValue(host, out var dns))
                return dns;
            
            var dnsTask = Dns.GetHostAddressesAsync(host).AsUniTask();
            var timeoutTask = UniTask.Delay(timeoutMs, cancellationToken: ct);

            var completed = await UniTask.WhenAny(dnsTask, timeoutTask);

            if (!completed.hasResultLeft)
            {
                throw new TimeoutException("DNS resolve timeout");
            }

            cachedDns.TryAdd(host, completed.result);
            return completed.result;
        }


        private void DebugLogFail(int i)
        {
            if (!serviceData.showLogs)
                return;

            Debug.Log($"{Tag}: NTP request to \"{ntpServers[i].DomainNameAddress}\" failed.");
        }


        private void DebugLogSuccess(DateTime ntpTime, int i)
        {
            if (!serviceData.showLogs)
                return;

            stopwatch?.Stop();
            Debug.Log($"{Tag}: NtpTime = {ntpTime} from \"{ntpServers[i].DomainNameAddress}\". " +
                      $"TimeDifferenceUtc: {timeDifferenceUtc.Milliseconds} ms. Request duration: {stopwatch?.ElapsedMilliseconds} ms");
        }


        private void DebugLogNoConnection(in float waitSecondsBetweenFails)
        {
            if (!serviceData.showLogs)
                return;

            Debug.Log($"{Tag}: Failed to update network time from any NTP servers. "
                    + $"Retry in {waitSecondsBetweenFails} {(Mathf.Approximately(waitSecondsBetweenFails, 1f) ? "second" : "seconds")}.");
        }


        private void AppInterruptObserver_Interrupt()
        {
            OnPause(true);
        }


        private void AppInterruptObserver_Resume()
        {
            OnPause(false);
        }


#region Cheats

#if PR_CHEAT || UNITY_EDITOR

        public TimeSpan CheatTimeShift { get; private set; }


        public void CheatSetTimeShift(TimeSpan value)
        {
            CheatTimeShift = value;
            dateTimeState.cheatShiftTicks = CheatTimeShift.Ticks;
            OnChange?.Invoke(TimeChangeReason.TimeShift);
        }


        private void InitCheatShift()
        {
            CheatTimeShift = new TimeSpan(dateTimeState.cheatShiftTicks);
        }


#endif

#endregion


    }
}