using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    internal interface ICookieManager
    {
        string Get(string key);
        void Set(string key, string value, DateTime expires);
    }
}
