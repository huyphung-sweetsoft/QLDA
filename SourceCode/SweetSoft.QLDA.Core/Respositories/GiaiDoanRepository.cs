using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class GiaiDoanRepository : BaseRepository<TblGiaiDoan>
    {
        public GiaiDoanRepository(AuditManager auditManager) : base(auditManager) { }

        public override TblGiaiDoan GetById(Guid id)
        {
            return new Select()
                .From(TblGiaiDoan.Schema)
                .Where(TblGiaiDoan.IdGiaiDoanColumn).IsEqualTo(id)
                .And(TblGiaiDoan.KichHoatColumn).IsEqualTo(true)
                .And(TblGiaiDoan.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblGiaiDoan>();
        }

        public List<TblGiaiDoan> GetAllActive()
        {
            return new Select()
                .From(TblGiaiDoan.Schema)
                .Where(TblGiaiDoan.KichHoatColumn).IsEqualTo(true)
                .And(TblGiaiDoan.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblGiaiDoan>();
        }
    }
}
