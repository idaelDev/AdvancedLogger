using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace IdaelDev.AdvancedLogger
{
    /// <summary>
    /// Unity Debug.Log Wrapper
    /// </summary>
    public static class Log
    {
        private static readonly List<LogEntry> _logHistory = new List<LogEntry>();
        private static readonly object _lock = new object();

        public static bool IsProduction
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            get => false;
            #else
            get => true;
            #endif
        }

        public static IReadOnlyList<LogEntry> History
        {
            get
            {
                lock (_lock)
                {
                    return _logHistory.AsReadOnly();
                }
            }
        }

        public static event Action<LogEntry> OnLogAdded;

        public static void Clear()
        {
            lock (_lock)
            {
                _logHistory.Clear();
            }
        }

        #region Public Logging Methods

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void Debug(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (IsProduction) return;
            LogInternal(ELogLevel.Debug, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void Info(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (IsProduction) return;
            LogInternal(ELogLevel.Info, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void Warning(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (IsProduction) return;
            LogInternal(ELogLevel.Warning, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        [HideInCallstack]
        public static void Error(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(ELogLevel.Error, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }


        [HideInCallstack]
        public static void Fatal(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(ELogLevel.Fatal, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        [HideInCallstack]
        public static void Exception(
            Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var message = $"{exception.GetType().Name}: {exception.Message}";
            var entry = CreateLogEntry(ELogLevel.Error, message, filePath, memberName, lineNumber);
            entry.StackTrace = exception.StackTrace;
            AddToHistory(entry);
            UnityEngine.Debug.LogException(exception);
        }

        #endregion

        #region Internal Methods

        [HideInCallstack]
        private static void LogInternal(ELogLevel level, string message, string filePath, string memberName, int lineNumber)
        {
            var entry = CreateLogEntry(level, message, filePath, memberName, lineNumber);
            AddToHistory(entry);
            OutputToUnityConsole(entry);
        }

        private static LogEntry CreateLogEntry(ELogLevel level, string message, string filePath, string memberName, int lineNumber)
        {
            var className = ExtractClassNameFromPath(filePath);
            return new LogEntry(level, message, className, memberName, lineNumber);
        }

        private static void AddToHistory(LogEntry entry)
        {
            lock (_lock)
            {
                _logHistory.Add(entry);

                if (_logHistory.Count > 1000)
                {
                    _logHistory.RemoveAt(0);
                }
            }

            OnLogAdded?.Invoke(entry);
        }

        [HideInCallstack]
        private static void OutputToUnityConsole(LogEntry entry)
        {
            var formattedMessage = entry.GetFormattedMessage();

            switch (entry.Level)
            {
                case ELogLevel.Debug:
                case ELogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case ELogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case ELogLevel.Error:
                case ELogLevel.Fatal:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }

        private static string ExtractClassNameFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "Unknown";

            var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            return fileName;
        }

        #endregion


    }
}
