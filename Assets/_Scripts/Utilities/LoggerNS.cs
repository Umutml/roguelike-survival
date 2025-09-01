using UnityEngine;

namespace _Scripts.Utilities
{
    // ReSharper disable once InconsistentNaming
    public static class LoggerNS
    {
        public static bool EnableLog { get; set; } = true;
        public static bool EnableLogWarning { get; set; } = true;
        public static bool EnableLogError { get; set; } = true;

        public static void Log(string message)
        {
            if (EnableLog)
                Debug.Log(message);
        }

        public static void Log(object message)
        {
            if (EnableLog)
                Debug.Log(message);
        }

        public static void Log(object message, Object context)
        {
            if (EnableLog)
                Debug.Log(message, context);
        }

        public static void LogWarning(string message)
        {
            if (EnableLogWarning)
                Debug.LogWarning(message);
        }

        public static void LogWarning(object message)
        {
            if (EnableLogWarning)
                Debug.LogWarning(message);
        }

        public static void LogWarning(object message, Object context)
        {
            if (EnableLogWarning)
                Debug.LogWarning(message, context);
        }

        public static void LogError(string message)
        {
            if (EnableLogError)
                Debug.LogError(message);
        }

        public static void LogError(object message)
        {
            if (EnableLogError)
                Debug.LogError(message);
        }

        public static void LogError(object message, Object context)
        {
            if (EnableLogError)
                Debug.LogError(message, context);
        }
        
        public static void SetLogStatus(bool status)
        {
            EnableLog = status;
            EnableLogWarning = status;
            EnableLogError = status;
        }
    }
}
