using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class MeetManager : BaseManager
    {
        private static readonly Lazy<MeetManager> _instance = new Lazy<MeetManager>(() => new MeetManager());
        public static MeetManager Instance => _instance.Value;
        private readonly MeetRepository _repository;
        private readonly AuditManager _auditManager;

        public MeetManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new MeetRepository(_auditManager);
        }
    }
}
