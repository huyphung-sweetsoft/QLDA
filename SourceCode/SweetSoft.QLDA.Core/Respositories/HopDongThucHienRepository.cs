using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class HopDongThucHienRepository : BaseRepository<TblHopDongThucHien>
    {
        public HopDongThucHienRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        #region Search paging

        public DataTable SearchPaging(string searchTerm, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            parameters = parameters ?? new Dictionary<string, object>();

            Guid idKhachHang = GetGuidParameter(parameters, TblHopDongThucHien.Columns.IdKhachHang);
            string giaTriTu = GetDecimalSqlValue(parameters, "GiaTriHopDongTu");
            string giaTriDen = GetDecimalSqlValue(parameters, "GiaTriHopDongDen");
            string ngayKyTu = GetDateSqlValue(parameters, "NgayKyTu");
            string ngayKyDen = GetDateSqlValue(parameters, "NgayKyDen");
            string keyword = InlineQueryHelpers.SQLEncode(searchTerm ?? string.Empty);

            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @idKhachHang UNIQUEIDENTIFIER = '{idKhachHang}';
                DECLARE @giaTriTu DECIMAL(18, 2) = {giaTriTu};
                DECLARE @giaTriDen DECIMAL(18, 2) = {giaTriDen};
                DECLARE @ngayKyTu DATETIME = {ngayKyTu};
                DECLARE @ngayKyDen DATETIME = {ngayKyDen};
                DECLARE @singleKeyWord NVARCHAR(250) = N'%{keyword}%';

                SELECT *
                FROM
                (
                    SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.*
                    FROM
                    (
                        SELECT
                            hd.*,
                            kh.TenKhachHang,
                            COUNT(1) OVER() AS total_records
                        FROM dbo.TblHopDongThucHien hd
                        INNER JOIN dbo.TblKhachHang kh ON kh.IdKhachHang = hd.IdKhachHang
                        WHERE hd.DaXoa = 0
                          AND (@idKhachHang = '{Guid.Empty}' OR hd.IdKhachHang = @idKhachHang)
                          AND (@giaTriTu IS NULL OR hd.GiaTriHopDong >= @giaTriTu)
                          AND (@giaTriDen IS NULL OR hd.GiaTriHopDong <= @giaTriDen)
                          AND (@ngayKyTu IS NULL OR hd.NgayKy >= @ngayKyTu)
                          AND (@ngayKyDen IS NULL OR hd.NgayKy < DATEADD(DAY, 1, CAST(@ngayKyDen AS DATE)))
                          AND (@singleKeyWord = N'%%' OR hd.SoHopDong LIKE @singleKeyWord OR hd.TenHopDong LIKE @singleKeyWord OR kh.TenKhachHang LIKE @singleKeyWord)
                    ) AS T
                ) AS T1
                WHERE RowNum >= @startRow AND RowNum <= @endRow;";

            IDataReader reader = new InlineQuery().ExecuteReader(sql);

            if (reader == null)
                return null;

            DataTable table = new DataTable();
            table.Load(reader);

            InlineQueryHelpers.GetTotal(ref table, out totalRecord);

            return table;
        }

        public override DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return SearchPaging(string.Empty, parameters, orderBy, pageNumber, pageSize, out totalRecord);
        }

        #endregion

        #region CRUD

        public override TblHopDongThucHien GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.IdHopDongThucHienColumn).IsEqualTo(id)
                .And(TblHopDongThucHien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblHopDongThucHien>();
        }

        public TblHopDongThucHien GetBySoHopDong(string soHopDong)
        {
            if (string.IsNullOrWhiteSpace(soHopDong))
            {
                return null;
            }

            return new Select()
                .From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.SoHopDongColumn).IsEqualTo(soHopDong.Trim())
                // Không lọc DaXoa vì số hợp đồng phải duy nhất toàn hệ thống.
                .ExecuteSingle<TblHopDongThucHien>();
        }

        public bool IsSoHopDongExists(Guid excludedId, string soHopDong)
        {
            if (string.IsNullOrWhiteSpace(soHopDong))
            {
                return false;
            }

            Select select = new Select();
            select.From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.SoHopDongColumn).IsEqualTo(soHopDong.Trim());

            if (excludedId != Guid.Empty)
            {
                select.And(TblHopDongThucHien.IdHopDongThucHienColumn).IsNotEqualTo(excludedId);
            }

            return select.ExecuteSingle<TblHopDongThucHien>() != null;
        }

        public override TblHopDongThucHien Insert(TblHopDongThucHien item)
        {
            item.Save();

            Guid id = item.IdHopDongThucHien;

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, id, item.NguoiTao).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblHopDongThucHien");
                }
            });

            return item;
        }

        public override TblHopDongThucHien Update(TblHopDongThucHien item)
        {
            Guid id = item.IdHopDongThucHien;

            TblHopDongThucHien oldItem = GetById(id);

            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(oldItem, item, _tableName, id, item.NguoiCapNhat).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log UPDATE action for TblHopDongThucHien");
                }
            });

            return item;
        }

        public override bool Delete(TblHopDongThucHien item)
        {
            if (item == null)
                return false;

            Guid id = item.IdHopDongThucHien;

            TblHopDongThucHien oldItem = GetById(id);

            if (oldItem == null)
                return false;

            oldItem.DaXoa = true;
            oldItem.NguoiCapNhat = item.NguoiCapNhat;
            oldItem.NgayCapNhat = item.NgayCapNhat;
            oldItem.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(item, oldItem, _tableName, id, oldItem.NguoiCapNhat).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log DELETE action for TblHopDongThucHien");
                }
            });

            return true;
        }

        #endregion

        #region Parameter helpers

        private Guid GetGuidParameter(Dictionary<string, object> parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key) || parameters[key] == null)
            {
                return Guid.Empty;
            }

            Guid value;
            return Guid.TryParse(Convert.ToString(parameters[key]), out value) ? value : Guid.Empty;
        }

        private string GetDecimalSqlValue(Dictionary<string, object> parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key) || parameters[key] == null || string.IsNullOrWhiteSpace(Convert.ToString(parameters[key])))
            {
                return "NULL";
            }

            decimal value;
            string text = Convert.ToString(parameters[key]);

            bool parsed = decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value) || decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

            return parsed ? value.ToString(CultureInfo.InvariantCulture) : "NULL";
        }

        private string GetDateSqlValue(Dictionary<string, object> parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key) || parameters[key] == null || string.IsNullOrWhiteSpace(Convert.ToString(parameters[key])))
            {
                return "NULL";
            }

            DateTime value;

            if (!DateTime.TryParse(Convert.ToString(parameters[key]), out value))
            {
                return "NULL";
            }

            return "'" + value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "'";
        }

        #endregion
    }
}