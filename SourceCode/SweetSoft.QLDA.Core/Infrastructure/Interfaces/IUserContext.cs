using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    public interface IUserContext
    {
        Guid UserId { get; set; }
        string UserName { get; set; }
        AspnetUser User { get; set; }
        List<string> CurrentUserFunctions { get; set; }
        List<string> CurrentFunctions { get; set; }
        bool IsAdministrator { get; }
        string CurrentUserIp { get; }
        string CurrentUserAgent { get; set; }
        bool CheckFunctionPermission(Guid userId, ModuleKeys module);
    }
}
