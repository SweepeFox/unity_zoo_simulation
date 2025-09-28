using UnityEngine;

namespace Utils.Logger
{
    internal class UnityLogger : ILogger
    {
        [HideInCallstack]
        public void Log(string message)
        {
            Debug.Log($"{message}");
        }

        [HideInCallstack]
        public void LogWarning(string message)
        {
            Debug.LogWarning($"{message}");
        }

        [HideInCallstack]
        public void LogError(string message)
        {
            Debug.LogError($"{message}");
        }
    }
}