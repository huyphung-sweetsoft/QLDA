    //---------------------- PROGRAMMER LOG ---------------------------------------
using Microsoft.Extensions.DependencyInjection;
using Quartz.Logging;
using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.MailManager.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace SweetSoft.QLDA.BackOffice
{
    public class Global : System.Web.HttpApplication
    {
        public static IServiceProvider ServiceProvider;
        private static string _timeZoneId = ""; // Time zone for Vietnam
        private string TimeZoneId
        {
            get
            {
                if (string.IsNullOrEmpty(_timeZoneId))
                {
                    var systemConfiguration = SettingManager.Instance.GetSettingByName(SettingKeys.MyTimeZone);
                    if (systemConfiguration != null)
                        _timeZoneId = systemConfiguration.SettingValue;
                    else
                        _timeZoneId = "SE Asia Standard Time";
                }
                return _timeZoneId;
            }
        }
        protected void Application_Start(object sender, EventArgs e)
        {
            PreSendRequestHeaders += Application_PreSendRequestHeaders;
            AppDomain.CurrentDomain.UnhandledException += Application_Error;

            //----------------------------------------------------------
            string prefixSession = AppSettingHelpers.GetSetting<string>("SessionAppContext");
            var services = new ServiceCollection();
            services.AddSingleton<AuditManager>(sp => new AuditManager(new Core.SysManager.Models.ClientInfo()
            {
                IpAddress = HttpContext.Current?.Request?.UserHostAddress,
                UserAgent = HttpContext.Current?.Request?.UserAgent,
                UserId = HttpContext.Current?.Session?[prefixSession + "CURRENT_USER_ID"] as Guid?,
                UserName = HttpContext.Current?.Session?[prefixSession + "CURRENT_USER_NAME"] as string
            }));
            services.AddTransient<IEmailManager>(sp =>
                new EmailManager(
                    SweetContext.Current,
                    sp.GetRequiredService<AuditManager>()
                ));


            var provider = services.BuildServiceProvider();

            ServiceProvider = provider;

            ExtraDateTime.DateTimeConverter = new SettingDateTimeConverter(TimeZoneId);
            //-------------------------------------------------------------
        }
        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            if (Response.Headers != null)
            {
                Response.Headers.Remove("Server");
                Response.Headers.Remove("X-AspNet-Version");
                Response.Headers.Remove("Expires");
                Response.Headers.Remove("Cache-Control");
                Response.Headers.Remove("Connection");
                Response.Headers.Remove("access-control-allow-origin");
            }
        }
        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        #region Log Errors
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = null;
            try
            {
                if (e is UnhandledExceptionEventArgs unhandledArgs)
                    ex = unhandledArgs.ExceptionObject as Exception;
                else if (HttpContext.Current != null && HttpContext.Current.Server != null)
                    ex = HttpContext.Current.Server.GetLastError();
            }
            catch
            {
                if (e is UnhandledExceptionEventArgs unhandledArgs)
                    ex = unhandledArgs.ExceptionObject as Exception;
            }

            if (ex != null)
            {
                string url = "Unknown";
                string userAgent = "Unknown";
                string referrer = "Unknown";
                string sessionID = "Unknown";

                try
                {
                    if (HttpContext.Current != null)
                    {
                        var req = HttpContext.Current.Request;
                        url = req?.Url?.ToString() ?? "Unknown";
                        userAgent = req?.UserAgent ?? "Unknown";
                        referrer = req?.UrlReferrer?.ToString() ?? "Unknown";
                        if (HttpContext.Current.Session != null)
                            sessionID = HttpContext.Current.Session.SessionID ?? "Unknown";
                    }
                }
                catch { }

                string errorMessage = $@"
                === UNHANDLED EXCEPTION ===
                Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
                URL: {url}
                User Agent: {userAgent}
                Referrer: {referrer}
                Session ID: {sessionID}
                Exception Type: {ex.GetType().Name}
                Message: {ex.Message}
                Stack Trace: {ex.StackTrace}
                Inner Exception: {ex.InnerException?.Message ?? "None"}
                Inner Stack Trace: {ex.InnerException?.StackTrace ?? "None"}
                ========================
                ";

                // Log to file
                LogErrorToFile(errorMessage);

                // Log to Event Log
                LogErrorToEventLog(errorMessage);

                // Log to Debug Output
                System.Diagnostics.Debug.WriteLine(errorMessage);

                //Server.ClearError();
            }
        }

        private void LogErrorToFile(string errorMessage)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string logPath = Path.Combine(baseDir, "Uploads", "_Logs", $"ErrorLog_{DateTime.UtcNow.ToString("ddMMyyyy")}.txt");
                string directory = Path.GetDirectoryName(logPath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.AppendAllText(logPath, errorMessage + Environment.NewLine);
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log to file: {logEx.Message}");
            }
        }

        private void LogErrorToEventLog(string errorMessage)
        {
            try
            {
                string source = "WebApplication";
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }
                EventLog.WriteEntry(source, errorMessage, EventLogEntryType.Error);
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log to event log: {logEx.Message}");
            }
        }
        #endregion

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            
        }
    }
}