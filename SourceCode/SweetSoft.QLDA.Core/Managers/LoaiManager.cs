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
    public class LoaiManager : BaseManager
    {
        private static readonly Lazy<LoaiManager> _instance = new Lazy<LoaiManager>(() => new LoaiManager());

        public static LoaiManager Instance => _instance.Value;
        private readonly LoaiRepository _repository;
        private readonly AuditManager _auditManager;

        public LoaiManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new LoaiRepository(_auditManager);
        }

        public TblLoai GetLoaiById(Guid id)
        {
            return _repository.GetById(id);
        }

        public List<TblLoai> GetAllLoaiDuAn()
        {
            return _repository.GetAllLoaiDuAn();
        }

        public List<TblLoai> GetAllLoaiKhachHang()
        {
            return _repository.GetAllLoaiKhachHang();
        }

        public List<TblLoai> GetAllPhongBan()
        {
            return _repository.GetAllPhongBan();
        }

        public List<TblLoai> GetAllChucDanh()
        {
            return _repository.GetAllChucDanh();
        }


    }
}
