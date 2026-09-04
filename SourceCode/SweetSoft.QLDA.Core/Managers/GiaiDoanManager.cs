using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Managers
{
    public class GiaiDoanManager : BaseManager
    {
        private static readonly Lazy<GiaiDoanManager> _instance = new Lazy<GiaiDoanManager>(() => new GiaiDoanManager());
        public static GiaiDoanManager Instance => _instance.Value;

        private readonly GiaiDoanRepository _repository;
        private readonly AuditManager _auditManager;

        public GiaiDoanManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new GiaiDoanRepository(_auditManager);
        }

        public TblGiaiDoan GetById(Guid idGiaiDoan)
        {
            return _repository.GetById(idGiaiDoan);
        }

        public List<TblGiaiDoan> GetAllActive()
        {
            return _repository.GetAllActive();
        }
    }
}
