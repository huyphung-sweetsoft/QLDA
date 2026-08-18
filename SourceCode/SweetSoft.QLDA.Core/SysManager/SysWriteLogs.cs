using System;
using System.IO;
using System.Linq;

namespace SweetSoft.QLDA.Core.SysManager
{
    public static class SysLogger
    {
        private static readonly string BaseLogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                $"uploads/_logs/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}");

        public static void LogInfo(string message, params object[] args)
        {
            WriteLog("Info", null, message, args);
        }

        public static void LogError(Exception ex, string message, params object[] args)
        {
            WriteLog("Error", ex, message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            WriteLog("Error", null, message, args);
        }

        public static void LogDebug(string message, params object[] args)
        {
            WriteLog("Debug", null, message, args);
        }

        private static void WriteLog(string level, Exception ex, string message, params object[] args)
        {
            try
            {
                if (!Directory.Exists(BaseLogPath))
                {
                    Directory.CreateDirectory(BaseLogPath);
                }

                string logFilePath = Path.Combine(BaseLogPath, $"log_{DateTime.UtcNow:yyyyMMdd}.txt");

                string formattedMessage;
                try
                {
                    // Thay {Name} => {0}, {1}, ...
                    formattedMessage = FormatMessage(message, args);
                }
                catch
                {
                    formattedMessage = message;
                }

                string logMessage =
                    $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] - {formattedMessage}";

                if (ex != null)
                {
                    logMessage += Environment.NewLine + ex;
                }

                logMessage += Environment.NewLine;

                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception logEx)
            {
                Console.WriteLine("Logging failed: " + logEx.Message);
            }
        }

        private static string FormatMessage(string template, object[] args)
        {
            if (args == null || args.Length == 0) return template;

            // Chuyển {CustomerId} => {0}, {ToEmail} => {1}, ...
            var placeholders = System.Text.RegularExpressions.Regex.Matches(template, @"\{[^\}]+\}");
            string formatted = template;
            int i = 0;
            foreach (var match in placeholders.Cast<System.Text.RegularExpressions.Match>())
            {
                if (i >= args.Length) break;
                formatted = formatted.Replace(match.Value, args[i]?.ToString() ?? "null");
                i++;
            }
            return formatted;
        }
    }

}
