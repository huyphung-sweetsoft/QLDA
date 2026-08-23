using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class LoaiDuAnManager : BaseManager
    {
        private static readonly Lazy<LoaiDuAnManager> _instance = new Lazy<LoaiDuAnManager>(() => new LoaiDuAnManager());

        public static LoaiDuAnManager Instance => _instance.Value;
        private readonly LoaiDuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public LoaiDuAnManager(IAppContext applicationContext) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new LoaiDuAnRepository(_auditManager);
        }
        public TblLoaiDuAn GetLoaiDuAnById(Guid id)
        {
            return _repository.GetById(id);
        }
        public List<TblLoaiDuAn> GetAllLoaiDuAn()
        {
            return _repository.GetAllTblLoaiDuAn();
        }
    }

}
