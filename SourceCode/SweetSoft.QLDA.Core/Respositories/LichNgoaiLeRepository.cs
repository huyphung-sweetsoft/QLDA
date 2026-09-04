using SubSonic;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class LichNgoaiLeRepository : BaseRepository<TblLichNgoaiLe>
    {
        public LichNgoaiLeRepository(AuditManager auditManager) : base(auditManager) { }

        public TblLichNgoaiLe GetById(Guid id)
        {
            return new Select().From(TblLichNgoaiLe.Schema)
                .Where(TblLichNgoaiLe.IdNgoaiLeColumn).IsEqualTo(id)
                .ExecuteSingle<TblLichNgoaiLe>();
        }

        public TblLichNgoaiLe Insert(TblLichNgoaiLe item)
        {
            if (item == null) return null;

            item.IsNew = true;
            item.DaXoa = false; // Bổ sung: Ép cứng giá trị mặc định cho an toàn

            // Nếu bảng bạn có cột NgayTao, hãy mở comment dòng dưới
            // item.NgayTao = DateTime.Now; 

            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(null, item, _tableName, item.IdNgoaiLe, "INSERT");
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log insert for TblLichNgoaiLe");
                }
            });
            return item;
        }

        public TblLichNgoaiLe Update(TblLichNgoaiLe item)
        {
            if (item == null) return null;
            var id = item.IdNgoaiLe;
            TblLichNgoaiLe old = GetById(id);
            item.NgayCapNhat = DateTime.Now;
            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(old, item, _tableName, id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblLichNgoaiLe");
                }
            });
            return item;
        }

        public override bool Delete(TblLichNgoaiLe item)
        {
            if (item == null) return false;
            var id = item.IdNgoaiLe;
            TblLichNgoaiLe old = GetById(id);
            item.DaXoa = true;
            item.NgayCapNhat = DateTime.Now;
            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(old, item, _tableName, id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblLichNgoaiLe");
                }
            });
            return true;
        }

        // ĐÃ NÂNG CẤP: Dùng InlineQuery, nhận chuỗi orderBy và trả về DataTable chuẩn kiến trúc
        public DataTable SearchPaging(string searchTerm, bool? isWorkingDay, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            // Ép kiểu orderBy mặc định nếu UI không truyền xuống
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "NgayBatDau DESC";
            }

            string sql = $@"
            DECLARE @startRow INT = {pageNumber};
            DECLARE @endRow INT = {pageSize};
            
            -- KIỂU BIT: Dùng logic giống UserRepository để xử lý Null an toàn
            DECLARE @isWorkingDay BIT = {(isWorkingDay.HasValue ? $"'{InlineQueryHelpers.SQLEncode(isWorkingDay.Value.ToString())}'" : "NULL")};
            
            DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';

            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                    SELECT f.*
                    , COUNT(1) OVER() AS total_records
                    FROM TblLichNgoaiLe f
                    WHERE f.DaXoa = 0 
                    AND (@isWorkingDay IS NULL OR f.LaNgayLamViec = @isWorkingDay)
                    AND (@singleKeyWord = N'%%' 
                        OR f.TenNgoaiLe LIKE @singleKeyWord 
                        OR f.MoTa LIKE @singleKeyWord)
                ) AS T
            ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow;";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            System.Diagnostics.Debug.WriteLine($"TRƯỚC GetTotal: {dt.Rows.Count} dòng"); // thêm dòng này
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            System.Diagnostics.Debug.WriteLine($"SAU GetTotal: {dt.Rows.Count} dòng");   // thêm dòng này
            return dt;
        }

        public List<TblLichNgoaiLe> GetExceptionsInRange(DateTime startDate, DateTime endDate)
        {
            return new Select()
                .From(TblLichNgoaiLe.Schema)
                .Where(TblLichNgoaiLe.DaXoaColumn.ColumnName).IsEqualTo(false)
                .And(TblLichNgoaiLe.NgayBatDauColumn.ColumnName).IsLessThanOrEqualTo(endDate)
                .And(TblLichNgoaiLe.NgayKetThucColumn.ColumnName).IsGreaterThanOrEqualTo(startDate)
                .ExecuteTypedList<TblLichNgoaiLe>();
        }
    }
}