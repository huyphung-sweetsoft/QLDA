using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubSonic;
using OfficeOpenXml.Interfaces.Drawing.Text;
namespace SweetSoft.QLDA.Core.Respositories
{
    public class CauHinhTuanLamViecRepository : BaseRepository<TblCauHinhTuanLamViec>
    {
        public CauHinhTuanLamViecRepository(AuditManager auditManager) : base(auditManager) { }
        public List<TblCauHinhTuanLamViec> GetAll()
        {
            return new Select().From(TblCauHinhTuanLamViec.Schema).OrderAsc(TblCauHinhTuanLamViec.NgayTrongTuanColumn.ColumnName).ExecuteTypedList<TblCauHinhTuanLamViec>();
        }
        public TblCauHinhTuanLamViec GetByDayOfWeek(int dayOfWeek)
        {
            return new Select().From(TblCauHinhTuanLamViec.Schema).Where(TblCauHinhTuanLamViec.NgayTrongTuanColumn.ColumnName).IsEqualTo(dayOfWeek).ExecuteSingle<TblCauHinhTuanLamViec>();
        }
        public TblCauHinhTuanLamViec Update(TblCauHinhTuanLamViec item)
        {
            if (item == null) return null;
            var dayOfWeek = item.NgayTrongTuan;
            TblCauHinhTuanLamViec old = GetByDayOfWeek(dayOfWeek);
            item.NgayCapNhat = DateTime.Now;
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(old, item, _tableName,item.IdCauHinh, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblCauHinhTuanLamViec");
                }
            });
            return item;
        }
    }
}
