using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.ExceptionHelpers
{
    public class FileExceptionNotifier : IExceptionNotifier
    {
        public void Notify(BusinessException exception)
        {
            try
            {
                var innerException = exception.InnerException ?? exception;

                var stackTrace = new StackTrace(innerException, true);
                var frame = stackTrace.GetFrames()?.FirstOrDefault(f => f.GetFileLineNumber() > 0);
                var lineNum = frame?.GetFileLineNumber() ?? 0;
                var fileName = frame?.GetFileName() ?? "N/A";

                var context = SweetContext.Current;
                var serverName = Environment.MachineName;
                var appName = SecurityUtilities.ApplicationName;
                var userName = context?.UserName ?? "Unknown";
                var userIp = context?.CurrentUserIp ?? "Unknown";
                var currentUri = context?.CurrentUri?.AbsoluteUri ?? "N/A";

                var sb = new StringBuilder();
                sb.AppendLine("=========== EXCEPTION LOG ===========");
                sb.AppendLine($"Date: {DateTime.UtcNow}");
                sb.AppendLine($"Server: {serverName}");
                sb.AppendLine($"Application: {appName}");
                sb.AppendLine($"User: {userName}");
                sb.AppendLine($"IP: {userIp}");
                sb.AppendLine($"URL: {currentUri}");
                sb.AppendLine($"Error message: {innerException.Message}");
                sb.AppendLine($"Field: {exception.FieldName}");
                sb.AppendLine($"Line number: {lineNum}");
                sb.AppendLine($"File: {fileName}");
                sb.AppendLine("STACK TRACE:");
                sb.AppendLine(innerException.StackTrace ?? "No stack trace");
                sb.AppendLine("=====================================");
                sb.AppendLine();

                string subPath = string.Format("~/_Logs/{0}/{1}/{2}",
                    DateTime.UtcNow.Year,
                    DateTime.UtcNow.Month.ToString("D2"),
                    DateTime.UtcNow.Day.ToString("D2"));

                string physicalPath = HttpContext.Current.Server.MapPath(subPath);
                if (!Directory.Exists(physicalPath))
                    Directory.CreateDirectory(physicalPath);

                string filePath = Path.Combine(physicalPath, "error.log");
                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Logging thất bại, nhưng không được làm sập hệ thống chính
                // Có thể log vào EventLog hoặc bỏ qua
            }
        }
    }

}
