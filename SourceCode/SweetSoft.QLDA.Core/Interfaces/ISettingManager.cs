using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Interfaces
{
    public interface ISettingManager
    {
        string GetSettingValue(string key);
        int GetSettingValueInt(string key, int defaultVal);
        bool GetSettingValueBoolean(string key, bool defaultVal = false);
        string GetSettingValueDecryptAES(string key);
    }
}
