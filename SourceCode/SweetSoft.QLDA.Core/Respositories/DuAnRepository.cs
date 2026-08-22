using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DuAnRepository: BaseRepository<TblDuAn>
    {
        public DuAnRepository(AuditManager auditManager) : base(auditManager) { }

        public override DataTable SearchPaging(string searchTerm,string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber}
                DECLARE @endRow INT = {pageSize}
                select * from (
                    select ROW_NUMBER() OVER (ORDER BY {orderBy} AS RowNum, T.* from (
                        select d.*,
                        nv.TenNhanVien,
                        kh.TenKhachHang,
                        COUNT(1) OVER() AS total_records
                        from TblDuAn d
                        left join TblNhanVien nv on nv.IdNhanVien = d.IdNhanVienQuanLy
                        left join TblKhahcHang kh on kh.IdKhachHang = d.IdKhachHang
                        where d.DaXoa = 0
                    ) as T
                ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }
    }
}
