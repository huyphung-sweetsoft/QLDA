using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.SysManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public abstract class BaseManager
    {
        protected readonly IAppContext _applicationContext;

        protected BaseManager(IAppContext applicationContext)
        {
            if (applicationContext != null)
            {
                _applicationContext = applicationContext;
            }
            else
            {
                try
                {
                    _applicationContext = SweetContext.Current;
                }
                catch
                {
                    _applicationContext = SweetContext.CreateBackgroundContext();
                }
            }
        }


        /// <summary>
        /// Lấy thông tin client (IP, UserAgent).
        /// An toàn với NullReference, nếu không có context thì trả về rỗng.
        /// </summary>
        protected ClientInfo GetClientInfo()
        {
            try
            {
                return new ClientInfo
                {
                    UserId = _applicationContext?.UserId,
                    UserName = _applicationContext?.UserName,
                    IpAddress = _applicationContext?.CurrentUserIp ?? string.Empty,
                    UserAgent = _applicationContext?.CurrentUserAgent ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                return new ClientInfo()
                {
                    UserId = Guid.Empty,
                    UserName = "[System]",
                    IpAddress = "127.0.0.1",
                    UserAgent = "IIS"
                };
            }
        }
    }

}
