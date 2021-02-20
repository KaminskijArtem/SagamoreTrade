using System;
using System.Collections.Generic;
using System.Text;

namespace QuartzScheduler.Logging
{
    public class StaticLogger
    {
        public static string Log = "";
        public static void LogMessage(string message)
        {
            if(string.IsNullOrEmpty(Log))
                Log += message;
            else
            {
                Log += $"\n{message}";
            }
        }
    }
}
