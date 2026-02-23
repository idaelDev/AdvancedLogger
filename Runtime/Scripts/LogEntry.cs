using System;

namespace IdaelDev.AdvancedLogger
{
    public class LogEntry
    {
        public ELogLevel Level { get; set; }
        public string Message { get; set; }
        public string CallerClass { get; set; }
        public string CallerMethod { get; set; }
        public int LineNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string StackTrace { get; set; }

        public LogEntry(ELogLevel level, string message, string callerClass, string callerMethod, int lineNumber)
        {
            Level = level;
            Message = message;
            CallerClass = callerClass;
            CallerMethod = callerMethod;
            LineNumber = lineNumber;
            Timestamp = DateTime.Now;
        }

        public string GetFormattedMessage()
        {
            return $"[{CallerClass}] {Message}";
        }

        public string GetDetailedMessage()
        {
            return $"[{Timestamp:HH:mm:ss}] [{Level}] [{CallerClass}.{CallerMethod}:{LineNumber}] {Message}";
        }
        
        public UnityEngine.Color GetColor()
        {
            switch (Level)
            {
                case ELogLevel.Debug:
                    return new UnityEngine.Color(0.7f, 0.7f, 0.7f); // Gris
                case ELogLevel.Info:
                    return UnityEngine.Color.white;
                case ELogLevel.Warning:
                    return UnityEngine.Color.yellow;
                case ELogLevel.Error:
                    return new UnityEngine.Color(1f, 0.4f, 0.4f); // Rouge clair
                case ELogLevel.Fatal:
                    return UnityEngine.Color.red;
                default:
                    return UnityEngine.Color.white;
            }
        }
    }
}
