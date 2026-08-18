using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetCMS.Controls.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime MinValueSQL = DateTime.Parse("1900-01-01 00:00:00.000");
        public static bool IsEnglish
        {
            get
            {
                return CultureInfo.CurrentCulture.Name == "en-US";
            }
        }
        public static string DateFormat
        {
            get
            {
                if (DateTimeHelper.IsEnglish)
                    return "MM/dd/yyyy";
                else
                    return "dd/MM/yyyy";
            }
        }
        public static string ConvertDateTime(object dateTime, bool hasTime)
        {
            DateTime dtDateTime = DateTime.MinValue;
            if (dateTime == null)
                return string.Empty;
            try
            {
                dtDateTime = DateTime.Parse(dateTime.ToString());
            }
            catch
            {
                dateTime = null;
            }
            if (dtDateTime != null && dtDateTime != DateTime.MinValue && dtDateTime != MinValueSQL)
            {
                if (hasTime)
                    return dtDateTime.ToString($"{DateFormat} HH:mm:ss");
                else
                    return dtDateTime.ToString(DateFormat);
            }
            return string.Empty;
        }
    }
}
