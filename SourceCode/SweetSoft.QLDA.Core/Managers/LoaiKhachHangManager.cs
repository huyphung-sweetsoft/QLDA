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
    public class LoaiKhachHangManager : BaseManager
    {
        private static readonly Lazy<LoaiKhachHangManager> _instance = new Lazy<LoaiKhachHangManager>(() => new LoaiKhachHangManager());

        public static LoaiKhachHangManager Instance => _instance.Value;
        private readonly LoaiKhachHangRepository _repository;
        private readonly AuditManager _auditManager;

        public LoaiKhachHangManager(IAppContext applicationContect = null) : base(applicationContect)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new LoaiKhachHangRepository(_auditManager);
        }

        public List<TblLoaiKhachHang> GetAllLoaiKhachHang()
        {
            return _repository.GetAllTblLoaiKhachHang();
        }
    }
}
