using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier

#endregion

namespace SayKitInternal
{
    public static class SayKitStackTraceParser
    {
        private const string FrameArgsRegex = "\\s?\\(.*\\)";
        private const string FrameRegexWithoutFileInfo = "(?<class>[^\\s]+)\\.(?<method>[^\\s\\.]+)" + FrameArgsRegex;
        private const string FrameRegexWithFileInfo = FrameRegexWithoutFileInfo + " .*[/|\\\\](?<file>.+):(?<line>\\d+)";
        private const string MonoFilenameUnknownString = "<filename unknown>";
        private static readonly string[] StringDelimiters = { System.Environment.NewLine };

        public static string ParseStackTraceString(Exception exception)
        {
            var dictionaryList = new List<Dictionary<string, string>>();

            try
            {
                var currentException = exception;
                while (currentException != null)
                {
                    var stackTrace = currentException.StackTrace;
                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        var parsedStackTrace = ParseStackTrace(stackTrace);
                        dictionaryList.AddRange(parsedStackTrace);
                    }

                    currentException = currentException.InnerException;
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"[SayKitStackTraceParser] exception: {e}");
            }

            return JsonConvert.SerializeObject(dictionaryList);
        }

        private static IEnumerable<Dictionary<string, string>> ParseStackTrace(string stackTrace)
        {
            var parsedStackTrace = new List<Dictionary<string, string>>();
            
            try
            {
                var stackTraceLines = stackTrace.Split(StringDelimiters, StringSplitOptions.None);

                foreach (var stackTraceLine in stackTraceLines)
                {
                    if (ParseFrame(stackTraceLine, out var frameData))
                    {
                        parsedStackTrace.Add(frameData);
                    }
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"[SayKitStackTraceParser] exception: {e}");
            }

            return parsedStackTrace;
        }

        private static bool ParseFrame(string stackTraceLine, out Dictionary<string, string> frameData)
        {
            frameData = null;
            
            string frameRegex;

            if (Regex.Matches(stackTraceLine, FrameRegexWithFileInfo).Count == 1)
            {
                frameRegex = FrameRegexWithFileInfo;
            }
            else if (Regex.Matches(stackTraceLine, FrameRegexWithoutFileInfo).Count == 1)
            {
                frameRegex = FrameRegexWithoutFileInfo;
            }
            else
            {
                return false;
            }

            var matchCollection = Regex.Matches(stackTraceLine, frameRegex);

            if (matchCollection.Count < 1)
            {
                return false;
            }

            var match = matchCollection[0];
            if (!match.Groups["class"].Success || !match.Groups["method"].Success)
            {
                return false;
            }

            var fileName = match.Groups["file"].Success ? match.Groups["file"].Value : match.Groups["class"].Value;
            var lineNumber = match.Groups["line"].Success ? match.Groups["line"].Value : "0";

            if (fileName == MonoFilenameUnknownString)
            {
                fileName = match.Groups["class"].Value;
                lineNumber = "0";
            }

            frameData = new Dictionary<string, string>
            {
                { "class", match.Groups["class"].Value },
                { "method", match.Groups["method"].Value },
                { "file", fileName },
                { "line", lineNumber }
            };

            return true;
        }
        
    }
}