using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SweetSoft.QLDA.Core.ExceptionHelpers
{
    public class EmailExceptionNotifier : IExceptionNotifier
    {
        public EmailExceptionNotifier()
        {
        }

        public void Notify(BusinessException exception)
        {
            if (!ErrorMailLimiter.CanSendMail()) return;
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
            var systemName = context?.SystemName ?? "System";

            var sb = new StringBuilder();
            sb.AppendLine($"Function: {innerException.Message}<br/>");
            sb.AppendLine("-------------------Detail-------------------<br/>");
            sb.AppendLine($"<b>Server name:</b> {serverName}<br/>");
            sb.AppendLine($"<b>Program:</b> {appName}<br/>");
            sb.AppendLine($"<b>Admin user:</b> {userName}<br/>");
            sb.AppendLine($"<b>IP address:</b> {userIp}<br/>");
            sb.AppendLine($"<b>Reported date:</b> {DateTime.UtcNow}<br/>");
            sb.AppendLine($"<b>Url:</b> <a href=\"{currentUri}\">{currentUri}</a><br/>");
            sb.AppendLine($"<b>Error message:</b> {innerException.Message}<br/>");
            sb.AppendLine($"<b>Field:</b> {exception.FieldName}<br/>");
            sb.AppendLine($"<b>Line number:</b> {lineNum}<br/>");
            sb.AppendLine($"<b>File:</b> {fileName}<br/>");
            sb.AppendLine("<b>STACK TRACE:</b><br/>");
            sb.AppendLine(innerException.StackTrace?.Replace(Environment.NewLine, "<br/>") ?? "No stack trace");

            string fromAddress = SettingManager.Instance.GetSettingValue(SettingKeys.AdministratorEmail);
            string toAddress = SettingManager.Instance.GetSettingValue(SettingKeys.ErrorReceiverEmail);

            if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(toAddress))
                return;

            var emailList = toAddress.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(e => e.Trim()).ToList();

            var toEmail = emailList.FirstOrDefault();
            var ccEmails = emailList.Skip(1).Select(email => new EmailAddress { Email = email, Name = email }).ToList();
            Guid userId = SweetContext.Current.UserId;
            var emailRequest = new EmailRequest
            {
                CustomerId = userId,
                RefType = EmailType.System,
                Sender = EmailManager.BackendSenderName,
                Subject = $"{systemName} - Error: {exception.Message}",
                Content = sb.ToString(),
                FromEmail = fromAddress,
                ToEmail = toEmail,
                CcEmails = ccEmails
            };
            var appContext = SweetContext.Current;
            Task.Run(async () =>
            {
                await new EmailManager(appContext).SendEmailAsync(emailRequest, true);
            });
        }
    }

}
