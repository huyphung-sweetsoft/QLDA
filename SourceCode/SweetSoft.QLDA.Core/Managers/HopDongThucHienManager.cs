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
    public class HopDongThucHienManager : BaseManager
    {
        private static readonly Lazy<HopDongThucHienManager> _instance = new Lazy<HopDongThucHienManager>(() => new HopDongThucHienManager());

        public static HopDongThucHienManager Instance => _instance.Value;
        private readonly HopDongThucHienRepository _repository;
        private readonly AuditManager _auditManager;

        public HopDongThucHienManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new HopDongThucHienRepository(_auditManager);
        }

        public TblHopDongThucHien GetHopDongById(Guid id)
        {
            return _repository.GetById(id);
        }
        public TblHopDongThucHien GetBySoHopDong(string soHopDong)
        {
            return _repository.GetBySoHopDong(soHopDong);
        }
    }
}
