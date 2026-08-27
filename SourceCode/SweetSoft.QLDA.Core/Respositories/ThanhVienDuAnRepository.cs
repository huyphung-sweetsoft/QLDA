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
    public class ThanhVienDuAnRepository : BaseRepository<TblThanhVienDuAn>
    {
        public ThanhVienDuAnRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public TblThanhVienDuAn Save(TblThanhVienDuAn item)
        {
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, Guid.Parse(item.GetColumnValue("IdDuAn").ToString())).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblDuAn");
                }
            });
            return item;
        }


        public List<TblThanhVienDuAn> GetByIdDuAn(Guid idDuAn)
        {
            Select select = new Select();
            select.From(TblThanhVienDuAn.Schema);
            select.Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn);
            return select.ExecuteTypedList<TblThanhVienDuAn>();
        }

        public TblThanhVienDuAn GetNhanVienIsActiveInDuAn(Guid idNhanVien, Guid idDuAn)
        {
            return new Select()
                .From(TblThanhVienDuAn.Schema)
                .Where(TblThanhVienDuAn.IdNhanVienColumn).IsEqualTo(idNhanVien)
                .And(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn)
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblThanhVienDuAn>();
        }

        
    }
}
