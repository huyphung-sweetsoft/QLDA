using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailTemplateKeys
    {
        public class AdminTemplate
        {
            public const string TemplateAccountInformation = "TemplateAccountInformation";
            public const string TemplateForgotPassword = "TemplateForgotPassword";
            public const string TemplateChangedPassword = "TemplateChangedPassword";
        }
        public static string GetText(string value)
        {
            switch (value)
            {
                case AdminTemplate.TemplateAccountInformation:
                    return UITextsReader.GetBackEndResourceText(BackEndResourceKeys.TEMPLATE_ACCOUNT_INFORMATION);
                case AdminTemplate.TemplateForgotPassword:
                    return UITextsReader.GetBackEndResourceText(BackEndResourceKeys.TEMPLATE_FORGOT_PASSWORD);
                case AdminTemplate.TemplateChangedPassword:
                    return UITextsReader.GetBackEndResourceText(BackEndResourceKeys.TEMPLATE_CHANGE_PASSWORD);
                default:
                    return value;
            }
        }
        public static List<ListItem> GetListItems()

        {
            return new List<ListItem>
                    {
                        new ListItem(GetText(AdminTemplate.TemplateAccountInformation), AdminTemplate.TemplateAccountInformation),
                        new ListItem(GetText(AdminTemplate.TemplateForgotPassword), AdminTemplate.TemplateForgotPassword),
                        new ListItem(GetText(AdminTemplate.TemplateChangedPassword), AdminTemplate.TemplateChangedPassword)
                    };
        }
    }
}
