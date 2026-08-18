using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Utils
{
    public class VnUnicodeHelpers
    {
        public VnUnicodeHelpers()
        {
        }

        #region  Methods

        public static string ReplaceVietnameseCharacters(string value)
        {
            string English = "aAeEoOuUiIdDyY";
            string[] Vietnamese = { "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                    "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",
                    "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                    "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ",
                    "íìịỉĩ", "ÍÌỊỈĨ",
                    "đ", "Đ",
                    "ýỳỵỷỹ", "ÝỲỴỶỸ" };
            StringBuilder sb = new StringBuilder();
            foreach (char ch in value.ToCharArray())
            {
                int i;
                for (i = 0; i < Vietnamese.Length; i++)
                    if (Vietnamese[i].Contains(ch)) break;
                if (i < Vietnamese.Length) sb.Append(English[i]);
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        #endregion
    }
}
