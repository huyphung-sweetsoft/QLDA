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
    public class NhanVienManager : BaseManager
    {
        private static readonly Lazy<NhanVienManager> _instance = new Lazy<NhanVienManager>(() => new NhanVienManager());

        public static NhanVienManager Instance => _instance.Value;
        private readonly NhanVienRepository _repository;
        private readonly AuditManager _auditManager;

        public NhanVienManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new NhanVienRepository(_auditManager);
        }
        public AspnetUser GetNhanVienById(Guid id)
        {
            return _repository.GetById(id);
        }
        public List<AspnetUser> GetAllNhanVien()
        {
            return _repository.GetAllTblNhanVien();
        }
    }
}
