using Serilog;
using System;

namespace RedAlertService.Common.Logging
{
    public static class LoggingService
    {
        public static void ConfigureLogger(string basePath)
        {
            string logFileName = "log_.txt";
            string logFolderPath = "logs";

            if (string.IsNullOrWhiteSpace(basePath))
                basePath = IOHelper.GetAppPath();

            logFolderPath = System.IO.Path.Combine(basePath, logFolderPath);

            if (!Directory.Exists(logFolderPath))
                Directory.CreateDirectory(logFolderPath);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(System.IO.Path.Combine(logFolderPath, logFileName), rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void LogInformation(string message)
        {
            Log.Information(message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            if (ex != null)
                Log.Error(ex, message);
            else
                Log.Error(message);
        }
    }
}
