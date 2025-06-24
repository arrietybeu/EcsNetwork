using System;
using System.IO;

namespace arriety.utils
{
    public static class Log
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        private static bool IsLog = true;
        private static bool IsLogWarning = true;
        private static bool IsLogError = true;
        private static bool WriteToFile = false;
        private static string LogFilePath = "game_log.txt";

        public static void Info(string message)
        {
            if (!IsLog) return;
            Console.WriteLine("[INFO] " + message);
            WriteLogToFile("[INFO] " + message);
        }

        public static void Warning(string message)
        {
            if (!IsLogWarning) return;
            Console.WriteLine("[WARN] " + message);
            WriteLogToFile("[WARN] " + message);
        }

        public static void Error(string message)
        {
            if (!IsLogError) return;
            Console.WriteLine("[ERROR] " + message);
            WriteLogToFile("[ERROR] " + message);
        }

        public static void LogWithLevel(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Info: Info(message); break;
                case LogLevel.Warning: Warning(message); break;
                case LogLevel.Error: Error(message); break;
            }
        }

        private static void WriteLogToFile(string message)
        {
            if (!WriteToFile) return;
            try
            {
                File.AppendAllText(LogFilePath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't write log file: " + ex.Message);
            }
        }

        public static void Exception(Exception ex)
        {
            Error($"StackTrace: {ex.StackTrace}\nMessage: {ex.Message}");
        }
    }
}