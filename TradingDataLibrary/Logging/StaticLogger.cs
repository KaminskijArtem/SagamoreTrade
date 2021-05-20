using System;

namespace TradingDataLibrary.Logging
{
    public class StaticLogger
    {
        public static string Log = "";
        public static void LogMessage(string message)
        {
            message = $"{DateTime.UtcNow.AddHours(3)} {message}";
            if(string.IsNullOrEmpty(Log))
                Log += message;
            else
            {
                Log += $"\n{message}";
            }
        }
    }
}
