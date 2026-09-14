using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class ThanhToanRepository : BaseRepository<TblThanhToan>
    {
        public ThanhToanRepository(AuditManager auditManager) : base(auditManager) { }

        public DataTable SearchPaging(Guid projectId, string searchTerm, string sortColumn,
            string sortDirection, int startRow, int endRow, out int totalRecord)
        {
            // Sorting comes from the grid, but only known column names may enter SQL.
            switch (sortColumn)
            {
                case "MaDotThanhToan":
                case "TenDotThanhToan":
                case "GhiChu":
                case "SoTien":
                case "HanThanhToan":
                case "NgayThanhToanThucTe":
                case "TrangThai":
                    break;
                default:
                    sortColumn = "MaDotThanhToan";
                    break;
            }
            string direction = string.Equals(sortDirection, "DESC", StringComparison.OrdinalIgnoreCase)
                ? "DESC" : "ASC";
            string sql = $@"
                SELECT * FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY p.[{sortColumn}] {direction}, p.IdThanhToan) AS RowNum,
                        p.*, COUNT(1) OVER() AS total_records
                    FROM TblThanhToan p
                    INNER JOIN TblDuAn d ON d.IdDuAn = p.IdDuAn AND d.DaXoa = 0
                    WHERE p.DaXoa = 0 AND p.IdDuAn = @projectId
                      AND (p.MaDotThanhToan LIKE @keyword OR p.TenDotThanhToan LIKE @keyword)
                ) T WHERE RowNum BETWEEN @startRow AND @endRow ORDER BY RowNum";
            DataTable table = new DataTable();
            // Explicit Unicode parameters preserve Vietnamese search terms.
            QueryCommand command = new InlineQuery().GetCommand(sql);
            command.Parameters.Add("@projectId", projectId, DbType.Guid);
            command.Parameters.Add("@keyword", "%" + (searchTerm ?? string.Empty).Trim() + "%", DbType.String);
            command.Parameters.Add("@startRow", startRow, DbType.Int32);
            command.Parameters.Add("@endRow", endRow, DbType.Int32);
            using (IDataReader reader = DataService.GetReader(command))
            {
                table.Load(reader);
            }
            InlineQueryHelpers.GetTotal(ref table, out totalRecord);
            return table;
        }

        public TblThanhToan GetByProject(Guid id, Guid projectId)
        {
            return new Select().From(TblThanhToan.Schema)
                .Where(TblThanhToan.IdThanhToanColumn).IsEqualTo(id)
                .And(TblThanhToan.IdDuAnColumn).IsEqualTo(projectId)
                .And(TblThanhToan.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblThanhToan>();
        }

        public TblThanhToan GetByCode(string paymentCode)
        {
            return new Select().From(TblThanhToan.Schema)
                .Where(TblThanhToan.MaDotThanhToanColumn).IsEqualTo(paymentCode)
                .And(TblThanhToan.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblThanhToan>();
        }

        public int GetMaxSequence(Guid projectId, string codePrefix)
        {
            string sql =
                "SELECT ISNULL(MAX(" +
                "    TRY_CAST(SUBSTRING(p.MaDotThanhToan, LEN(@Prefix) + 1, " +
                "        LEN(p.MaDotThanhToan) - LEN(@Prefix)) AS INT)" +
                "), 0) FROM TblThanhToan p" +
                " WHERE p.IdDuAn = @ProjectId" +
                " AND p.MaDotThanhToan LIKE @Prefix + '%'";
            QueryCommand command = new InlineQuery().GetCommand(sql);
            command.Parameters.Add("@ProjectId", projectId, DbType.Guid);
            command.Parameters.Add("@Prefix", codePrefix, DbType.String);
            object result = DataService.ExecuteScalar(command);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public override TblThanhToan Insert(TblThanhToan item)
        {
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item,
                        _tableName, item.IdThanhToan).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblThanhToan");
                }
            });
            return item;
        }

        public override TblThanhToan Update(TblThanhToan item)
        {
            TblThanhToan oldItem = GetByProject(item.IdThanhToan, item.IdDuAn);
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(oldItem, item, _tableName,
                        item.IdThanhToan, item.NguoiCapNhat).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblThanhToan");
                }
            });
            return item;
        }

        public override bool Delete(TblThanhToan item)
        {
            if (item == null)
                return false;
            item.DaXoa = true;
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.DELETE, item,
                        _tableName, item.IdThanhToan).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log DELETE action for TblThanhToan");
                }
            });
            return true;
        }
    }
}
