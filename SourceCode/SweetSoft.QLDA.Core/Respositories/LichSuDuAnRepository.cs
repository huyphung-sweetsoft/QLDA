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
    public class LichSuDuAnRepository : BaseRepository<TblLichSuDuAn>
    {
        public LichSuDuAnRepository(AuditManager auditManager) : base(auditManager)
        {

        }

        public override TblLichSuDuAn Insert(
            TblLichSuDuAn item)
        {
            if (item == null)
                return null;

            item.Save();
            return item;
        }

        public override TblLichSuDuAn GetById(
            Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblLichSuDuAn.Schema)
                .Where(
                    TblLichSuDuAn
                        .IdLichSuDuAnColumn)
                .IsEqualTo(id)
                .ExecuteSingle<TblLichSuDuAn>();
        }


    }
}
