using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailUtils
    {
        public static string ReplaceKey(string template, Dictionary<string, string> placeholders)
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
