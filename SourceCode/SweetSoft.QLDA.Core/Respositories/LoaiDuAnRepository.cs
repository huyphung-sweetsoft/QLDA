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
    public class LoaiDuAnRepository : BaseRepository<TblLoaiDuAn>
    {
        public LoaiDuAnRepository(AuditManager auditManager) : base(auditManager) { }

        public override TblLoaiDuAn GetById(Guid id)
        {
            return new Select()
                .From(TblLoaiDuAn.Schema)
                .Where(TblLoaiDuAn.IdLoaiDuAnColumn).IsEqualTo(id)
                .And(TblLoaiDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblLoaiDuAn>();
        }

        public List<TblLoaiDuAn> GetAllTblLoaiDuAn()
        {
            Select select = new Select();
            select.From(TblLoaiDuAn.Schema);
            select.And(TblLoaiDuAn.DaXoaColumn).IsEqualTo(false);
            return select.ExecuteTypedList<TblLoaiDuAn>();
        }

        public TblLoaiDuAn GetLoaiDuAnById(object IdLoaiDuAn)
        {
            return new Select().From(TblLoaiDuAn.Schema)
                .Where(TblLoaiDuAn.IdLoaiDuAnColumn).IsEqualTo(IdLoaiDuAn)
                .And(TblLoaiDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblLoaiDuAn>();
        }
    }
}
