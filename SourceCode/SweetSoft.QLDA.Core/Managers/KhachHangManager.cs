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
    public class KhachHangManager : BaseManager
    {
        private static readonly Lazy<KhachHangManager> _instance = new Lazy<KhachHangManager>(() => new KhachHangManager());

        public static KhachHangManager Instance => _instance.Value;
        private readonly KhachHangRepository _repository;
        private readonly AuditManager _auditManager;

        public KhachHangManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new KhachHangRepository(_auditManager);
        }

        public TblKhachHang GetKhachHangById(Guid id)
        {
            return _repository.GetById(id);
        }
        public List<TblKhachHang> GetAllKhachHang()
        {
            return _repository.GetAllTblKhachHang();
        }
    }
}
