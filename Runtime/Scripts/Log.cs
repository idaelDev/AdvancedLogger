using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace IdaelDev.AdvancedLogger
{
    /// <summary>
    /// Système de logging amélioré pour Unity
    /// </summary>
    public static class Log
    {
        private static readonly List<LogEntry> _logHistory = new List<LogEntry>();
        private static readonly object _lock = new object();
        private static bool _isProduction;

        /// <summary>
        /// Active ou désactive le mode production (désactive les logs sauf erreurs)
        /// </summary>
        public static bool IsProductionMode
        {
            get => _isProduction;
            set => _isProduction = value;
        }

        /// <summary>
        /// Obtient l'historique complet des logs
        /// </summary>
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

        /// <summary>
        /// Événement déclenché quand un nouveau log est ajouté
        /// </summary>
        public static event Action<LogEntry> OnLogAdded;

        /// <summary>
        /// Vide l'historique des logs
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _logHistory.Clear();
            }
        }

        #region Public Logging Methods

        /// <summary>
        /// Log de debug (désactivé en production)
        /// </summary>
        [Conditional("DEBUG")]
        [HideInCallstack]
        public static void Debug(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (_isProduction) return;
            LogInternal(ELogLevel.Debug, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        /// <summary>
        /// Log d'information (désactivé en production)
        /// </summary>
        [HideInCallstack]
        public static void Info(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (_isProduction) return;
            LogInternal(ELogLevel.Info, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        /// <summary>
        /// Log d'avertissement (désactivé en production)
        /// </summary>
        [HideInCallstack]
        public static void Warning(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (_isProduction) return;
            LogInternal(ELogLevel.Warning, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        /// <summary>
        /// Log d'erreur (TOUJOURS actif, même en production)
        /// </summary>
        [HideInCallstack]
        public static void Error(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(ELogLevel.Error, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        /// <summary>
        /// Log d'erreur fatale (TOUJOURS actif, même en production)
        /// </summary>
        [HideInCallstack]
        public static void Fatal(
            object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(ELogLevel.Fatal, message?.ToString() ?? "null", filePath, memberName, lineNumber);
        }

        /// <summary>
        /// Log d'exception (TOUJOURS actif, même en production)
        /// </summary>
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

                // Limiter l'historique à 1000 entrées pour éviter les problèmes de mémoire
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

        #region Conditional Compilation

        /// <summary>
        /// Configure automatiquement le mode production basé sur les symboles de compilation
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            _isProduction = false;
#elif DEVELOPMENT_BUILD
            _isProduction = false;
#else
            _isProduction = true;
#endif
        }

        #endregion
    }
}
