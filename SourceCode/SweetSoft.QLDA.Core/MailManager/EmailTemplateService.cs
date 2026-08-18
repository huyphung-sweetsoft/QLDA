using SweetSoft.QLDA.Core.MailManager.Interfaces;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private string _key;
        private EmailFormatTypes _type;
        private TblEmailTemplate _template;

        public TblEmailTemplate Template => _template;
        public EmailTemplateService()
        {

        }

        public Task<EmailTemplate> GetTemplateAsync(string templateKey, EmailFormatTypes formatType)
        {
            this._key = templateKey;
            this._type = formatType;
            var template = EmailTemplateManager.GetEmailTemplateByTemplateKey(this._key, this._type);
            if (template == null)
                return null;
            return Task.FromResult(new EmailTemplate()
            {
                IsActivated = template.IsActivated,
                Subject = template.Subject,
                Body = template.Body,
                CCEmail = template.CCEmail,
                BCCEmail = template.BCCEmail,
            });
        }
        public EmailTemplateService(string templateKey, EmailFormatTypes formatType)
        {
            this._key = templateKey;
            this._type = formatType;
            this._template = EmailTemplateManager.GetEmailTemplateByTemplateKey(this._key, this._type);
        }

        public string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
        {
            if (placeholders != null && placeholders.Count > 0)
            {
                foreach (var placeholder in placeholders)
                {
                    template = template.Replace(placeholder.Key, placeholder.Value);
                }
            }
            return template;
        }
    }
}
