using Newtonsoft.Json;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class LichSuDuAnManager : BaseManager
    {
        private static readonly Lazy<LichSuDuAnManager> _instance = new Lazy<LichSuDuAnManager>(() => new LichSuDuAnManager());
        public static LichSuDuAnManager Instance => _instance.Value;
        private readonly LichSuDuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public LichSuDuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new LichSuDuAnRepository(_auditManager);
        }
    }
}
