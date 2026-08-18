using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Controls.Interfaces
{
    public interface IDateTimeConverter
    {
        DateTime ConvertSettingTimeToUtc(DateTime settingTime);
        DateTime ConvertUTCToSettingTime(DateTime utcTime);
    }
}
