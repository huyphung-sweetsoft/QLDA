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

        public DataTable SearchPaging(string searchTerm, Dictionary<string, object> keyValueSearchs, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            string yearParam = "NULL";
            if (keyValueSearchs.ContainsKey("Nam") && !string.IsNullOrEmpty(keyValueSearchs["Nam"]?.ToString()))
            {
                if (int.TryParse(keyValueSearchs["Nam"].ToString(), out int nam))
                    yearParam = nam.ToString();
            }

            string monthParam = "NULL";
            if (keyValueSearchs.ContainsKey("Thang") && !string.IsNullOrEmpty(keyValueSearchs["Thang"]?.ToString()))
            {
                if (int.TryParse(keyValueSearchs["Thang"].ToString(), out int thang))
                    monthParam = thang.ToString();
            }

            string khoangGiaTri = keyValueSearchs.ContainsKey("KhoangGiaTri") ? keyValueSearchs["KhoangGiaTri"]?.ToString() : null;
            string khoangGiaTriCondition = "1 = 1";

            if (khoangGiaTri == "DUOI_10")
                khoangGiaTriCondition = "hd.GiaTriHopDong < 10000000";
            else if (khoangGiaTri == "10_100")
                khoangGiaTriCondition = "hd.GiaTriHopDong >= 10000000 AND hd.GiaTriHopDong < 100000000";
            else if (khoangGiaTri == "100_500")
                khoangGiaTriCondition = "hd.GiaTriHopDong >= 100000000 AND hd.GiaTriHopDong <= 500000000";
            else if (khoangGiaTri == "TREN_500")
                khoangGiaTriCondition = "hd.GiaTriHopDong > 500000000";

            string sql = $@"
        DECLARE @startRow INT = {pageNumber};
        DECLARE @endRow INT = {pageSize};
        DECLARE @idKhachHang VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(keyValueSearchs[TblHopDongThucHien.Columns.IdKhachHang])}';
        DECLARE @year INT = {yearParam};
        DECLARE @month INT = {monthParam};
        DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm ?? "")}%';

        SELECT *
        FROM (
            SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.*
            FROM (
                SELECT hd.*, kh.TenKhachHang, COUNT(1) OVER() AS total_records
                FROM TblHopDongThucHien hd
                INNER JOIN TblKhachHang kh ON kh.IdKhachHang = hd.IdKhachHang
                WHERE hd.DaXoa = 0
                AND (@idKhachHang = '{Guid.Empty}' OR hd.IdKhachHang = @idKhachHang)
                AND (@year IS NULL OR YEAR(hd.NgayKy) = @year)
                AND (@month IS NULL OR MONTH(hd.NgayKy) = @month)
                AND ({khoangGiaTriCondition})
                AND (
                    @singleKeyWord = N'%%'
                    OR hd.SoHopDong LIKE @singleKeyWord
                    OR hd.TenHopDong LIKE @singleKeyWord
                    OR kh.TenKhachHang LIKE @singleKeyWord
                )
            ) AS T
        ) T1
        WHERE RowNum >= @startRow AND RowNum <= @endRow";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
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

        public DataTable GetSortInfoBySoHopDong(string soHopDong)
        {
            string sql = $@"
                DECLARE @soHopDong NVARCHAR(500) = N'{InlineQueryHelpers.SQLEncode(soHopDong)}';
                SELECT TOP 1
                    hd.*,
                    kh.TenKhachHang
                FROM TblHopDongThucHien hd
                LEFT JOIN TblKhachHang kh ON kh.IdKhachHang = hd.IdKhachHang
                WHERE hd.SoHopDong = @soHopDong;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
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

        public bool IsUsedByAnotherProject(Guid idHopDongThucHien, Guid idDuAn)
        {
            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.Columns.IdHopDongThucHien).IsEqualTo(idHopDongThucHien)
                .And(TblDuAn.Columns.IdDuAn).IsNotEqualTo(idDuAn)
                .ExecuteSingle<TblDuAn>() != null;
        }

        public bool IsUsedByAnotherProject(Guid idHopDongThucHien)
        {
            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.Columns.IdHopDongThucHien).IsEqualTo(idHopDongThucHien)
                .ExecuteSingle<TblDuAn>() != null;
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
