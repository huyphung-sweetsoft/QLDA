using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    internal interface ISessionContext
    {
        bool HasSession { get; }
        HttpSessionState Session { get; }
        object Get(string key);
        void Set(string key, object value);
        void Remove(string key);
        void ClearAll();
    }

}
