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
    public class HopDongThucHienRepository : BaseRepository<TblHopDongThucHien>
    {
        public HopDongThucHienRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public TblHopDongThucHien GetById(Guid id)
        {
            return new Select()
                .From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.IdHopDongThucHienColumn).IsEqualTo(id)
                .And(TblHopDongThucHien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblHopDongThucHien>();
        }

        public TblHopDongThucHien GetBySoHopDong(string soHopDong)
        {
            if (string.IsNullOrWhiteSpace(soHopDong))
                return null;
            return new Select()
                .From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.SoHopDongColumn).IsEqualTo(soHopDong.Trim())
                .And(TblHopDongThucHien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblHopDongThucHien>();
        }
    }
}
