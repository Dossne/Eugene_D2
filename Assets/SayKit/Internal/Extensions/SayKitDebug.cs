using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Local
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    internal enum LogLevel
    {
        Verbose,
        Warning,
        Error
    }

    public class SayKitDebug
    {
        private static bool _debugFlag;
        private static LogLevel _logLevel = LogLevel.Verbose;
        public static bool disableSayKitLogs = false;
        private const string LogTag = "[SayKit Unity]: ";
        private const string LogTagError = "[SayKit Unity][Error]: ";

        public static void InitDebugLogs(bool debugFlag)
        {
            
#if SAYKIT_LOGLEVEL_VERBOSE
            _logLevel = LogLevel.Verbose;
#elif SAYKIT_LOGLEVEL_WARNING
             _logLevel = LogLevel.Warning;
#elif SAYKIT_LOGLEVEL_ERROR
            _logLevel = LogLevel.Error;
#endif

            if (disableSayKitLogs)
            {
                _debugFlag = false;
            }
            else
            {
#if SAYKIT_DEBUG
		        _debugFlag = true;
#else
                _debugFlag = debugFlag;
#endif
            }
        }

        public static void Log(string logMessage)
        {
#if SAYKIT_LOGLEVEL_DEBUG
            _logLevel = LogLevel.Verbose;
#endif
            LogMessage(LogTag + logMessage);
        }

        public static void LogError(string message)
        {
#if SAYKIT_LOGLEVEL_DEBUG
            _logLevel = LogLevel.Error;
#endif
            LogMessage($"{LogTagError}{message}");
        }

        public static void LogFormat(string format, params object[] args)
        {
            if (disableSayKitLogs)
            {
                return;
            }

            if (_debugFlag)
            {
                Debug.LogFormat(format, args);
            }
        }

        public static void LogWarning(string message)
        {
#if SAYKIT_LOGLEVEL_DEBUG
            Debug.LogWarning($"{LogTag}{message}");
#endif
        }

        private static void LogMessage(string message)
        {
#if !SAYKIT_LOGLEVEL_DEBUG
            if (disableSayKitLogs)
            {
                return;
            }

            if (!_debugFlag)
            {
                return;
            }
#endif
            switch (_logLevel)
            {
                case LogLevel.Verbose:
                    Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
    }
}