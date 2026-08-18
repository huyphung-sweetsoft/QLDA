using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplate> GetTemplateAsync(string templateKey, EmailFormatTypes formatType);
        string ReplacePlaceholders(string content, Dictionary<string, string> placeholders);
    }
}
