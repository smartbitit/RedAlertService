using Serilog;
using Serilog.Events;

using RedAlertService.Common;

namespace RedAlertService.Test
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Common.Logging.LoggingService.ConfigureLogger(string.Empty);
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new TestForm());
            Log.CloseAndFlush();
        }
    }
}
