using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    public interface IAppContext : IRequestContext, IUserContext, ILocalizationContext, IPaginationContext, ISystemContext
    {
        HttpSessionState Session { get; }
    }
}
