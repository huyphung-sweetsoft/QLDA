using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class VaiTroDuAnRepository : BaseRepository<TblThanhVienDuAn>
    {
        public VaiTroDuAnRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public TblVaiTroDuAn GetActiveByIdVaiTro(string idVaiTro)
        {
            if (string.IsNullOrEmpty(idVaiTro))
                return null;
            return new Select()
                .From(TblVaiTroDuAn.Schema)
                .Where(TblVaiTroDuAn.IdVaiTroColumn).IsEqualTo(idVaiTro.Trim())
                .And(TblVaiTroDuAn.KichHoatColumn).IsEqualTo(true)
                .And(TblVaiTroDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblVaiTroDuAn>();
        }
    }
}
