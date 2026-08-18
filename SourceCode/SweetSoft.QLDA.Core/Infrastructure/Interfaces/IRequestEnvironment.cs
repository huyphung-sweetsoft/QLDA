using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    internal interface IRequestEnvironment : IRequestContext
    {
        new HttpContext Context { get; }
        bool HasHttpContext { get; }
        string GetUserIpAddress();
        string GetUserAgent();
    }
}
