using SubSonic;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    internal class MeetRepository : BaseRepository<TblLichHop>
    {
        public MeetRepository(AuditManager auditManager) : base(auditManager) { }

        public override TblLichHop GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return new Select()
                .From(TblLichHop.Schema)
                .Where(TblLichHop.IdLichHopColumn).IsEqualTo(id)
                .And(TblLichHop.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblLichHop>();
        }

        /// <summary>
        /// Reads the optional meeting-document link through SQL so this source
        /// remains compilable before SubSonic is regenerated for the additive
        /// IdTaiLieu column. The migration must still be installed before use.
        /// </summary>
        public bool HasDocumentLinkColumn()
        {
            const string sql = @"
                SELECT CASE
                    WHEN COL_LENGTH(N'dbo.TblLichHop', N'IdTaiLieu') IS NULL
                        THEN 0
                    ELSE 1
                END;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
        }

        public Guid? GetLinkedDocumentId(Guid idLichHop)
        {
            return GetLinkedDocumentId(idLichHop, false);
        }

        /// <summary>
        /// Takes an update lock while a canonical meeting document is being
        /// created so concurrent clicks cannot create duplicate documents.
        /// </summary>
        public Guid? GetLinkedDocumentIdForUpdate(Guid idLichHop)
        {
            return GetLinkedDocumentId(idLichHop, true);
        }

        private Guid? GetLinkedDocumentId(Guid idLichHop, bool lockForUpdate)
        {
            if (idLichHop == Guid.Empty)
            {
                return null;
            }

            string tableHint = lockForUpdate
                ? " WITH (UPDLOCK, HOLDLOCK)"
                : string.Empty;
            string sql = $@"
                IF COL_LENGTH(N'dbo.TblLichHop', N'IdTaiLieu') IS NULL
                BEGIN
                    SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS IdTaiLieu;
                    RETURN;
                END;

                DECLARE @sql NVARCHAR(MAX) = N'
                    SELECT IdTaiLieu
                    FROM dbo.TblLichHop{tableHint}
                    WHERE IdLichHop = ''{idLichHop}''
                      AND DaXoa = 0;';

                EXEC sys.sp_executesql @sql;";

            using (IDataReader reader = new InlineQuery().ExecuteReader(sql))
            {
                if (reader == null || !reader.Read()
                    || reader["IdTaiLieu"] == DBNull.Value)
                {
                    return null;
                }

                Guid idTaiLieu;
                return Guid.TryParse(
                    Convert.ToString(reader["IdTaiLieu"]),
                    out idTaiLieu)
                    ? (Guid?)idTaiLieu
                    : null;
            }
        }

        /// <summary>
        /// Links a newly-created document only if the meeting is still
        /// unlinked. The conditional update is the last defence against a
        /// duplicate link after the serializable read above.
        /// </summary>
        public bool TryLinkDocument(
            Guid idLichHop,
            Guid idTaiLieu,
            Guid idNguoiCapNhat,
            DateTime ngayCapNhat)
        {
            if (idLichHop == Guid.Empty || idTaiLieu == Guid.Empty)
            {
                return false;
            }

            string updaterIdSql = idNguoiCapNhat == Guid.Empty
                ? "NULL"
                : "''" + idNguoiCapNhat + "''";
            string safeDate = ngayCapNhat.ToString(
                "yyyy-MM-dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture);
            string sql = $@"
                IF COL_LENGTH(N'dbo.TblLichHop', N'IdTaiLieu') IS NULL
                BEGIN
                    RAISERROR(N'Chưa cài cấu trúc liên kết hồ sơ cho lịch họp.', 16, 1);
                    RETURN;
                END;

                DECLARE @sql NVARCHAR(MAX) = N'
                    UPDATE dbo.TblLichHop
                    SET IdTaiLieu = ''{idTaiLieu}'',
                        IdNguoiCapNhat = {updaterIdSql},
                        NgayCapNhat = ''{safeDate}''
                    WHERE IdLichHop = ''{idLichHop}''
                      AND DaXoa = 0
                      AND IdTaiLieu IS NULL;
                    SELECT @@ROWCOUNT;';

                EXEC sys.sp_executesql @sql;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
        }

        public DataTable SearchMeeting(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            totalRecord = 0;
            string tenCuocHop = parameters != null && parameters.ContainsKey(TblLichHop.Columns.TenCuocHop) ? parameters[TblLichHop.Columns.TenCuocHop]?.ToString() : null;
            string diaDiemHop = parameters != null && parameters.ContainsKey(TblLichHop.Columns.DiaDiemHop) ? parameters[TblLichHop.Columns.DiaDiemHop]?.ToString() : null;
            string tuNgayStr = parameters != null && parameters.ContainsKey("TuNgay") ? parameters["TuNgay"]?.ToString() : null;
            string denNgayStr = parameters != null && parameters.ContainsKey("DenNgay") ? parameters["DenNgay"]?.ToString() : null;
            string trangThaiStr = parameters != null && parameters.ContainsKey(TblLichHop.Columns.TrangThai) ? parameters[TblLichHop.Columns.TrangThai]?.ToString() : null;
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "ThoiGianBatDau DESC";
            }
            string sqlTuNgay = string.IsNullOrEmpty(tuNgayStr) ? "NULL" : $"'{tuNgayStr} 00:00:00'";
            string sqlDenNgay = string.IsNullOrEmpty(denNgayStr) ? "NULL" : $"'{denNgayStr} 23:59:59'";
            string sqlTrangThai = string.IsNullOrEmpty(trangThaiStr) ? "NULL" : trangThaiStr;

            string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
        
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenCuocHop NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenCuocHop)}%';
                DECLARE @diaDiemHop NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(diaDiemHop)}%';
        
                DECLARE @tuNgay DATETIME = {sqlTuNgay};
                DECLARE @denNgay DATETIME = {sqlDenNgay};
                DECLARE @trangThai INT = {sqlTrangThai};

                SELECT * FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, Filtered.* FROM (
                        SELECT 
                            Base.*,
                            COUNT(1) OVER() AS total_records
                        FROM (
                            SELECT 
                                m.IdLichHop,
                                m.MaCuocHop,
                                m.TenCuocHop,
                                m.ThoiGianBatDau,
                                m.ThoiGianKetThuc,
                                m.DiaDiemHop,
                                m.TrangThai,
                        
                                -- CỘT 1: Lấy chuỗi Tên nhân viên
                                STUFF((
                                    SELECT ', ' + u.DisplayName
                                    FROM [dbo].[TblLichHop_NhanVien] ln
                                    INNER JOIN [dbo].[aspnet_Users] u ON ln.IdNhanVien = u.UserId
                                    WHERE ln.IdLichHop = m.IdLichHop
                                       AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)
                                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS TenNhanVien,
                            
                                -- CỘT 2: Lấy chuỗi Avatar
                                STUFF((
                                    SELECT ',' + ISNULL(u.Avatar, '')
                                    FROM [dbo].[TblLichHop_NhanVien] ln
                                    INNER JOIN [dbo].[aspnet_Users] u ON ln.IdNhanVien = u.UserId
                                    WHERE ln.IdLichHop = m.IdLichHop
                                       AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)
                                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Avatars
                            
                            FROM TblLichHop m
                            WHERE m.DaXoa = 0
                                AND m.IdDuAn = @projectId
                        ) AS Base
                        WHERE (@tenCuocHop = N'%%' OR Base.TenCuocHop LIKE @tenCuocHop)
                          AND (@singleKeyWord = N'%%' 
                                 OR Base.MaCuocHop LIKE @singleKeyWord 
                                 OR Base.TenCuocHop LIKE @singleKeyWord 
                                 OR Base.DiaDiemHop LIKE @singleKeyWord 
                                 OR Base.TenNhanVien LIKE @singleKeyWord)
                          AND (@diaDiemHop = N'%%' OR Base.DiaDiemHop LIKE @diaDiemHop)
                          AND (@trangThai IS NULL OR Base.TrangThai = @trangThai)
                          AND (@tuNgay IS NULL OR Base.ThoiGianBatDau >= @tuNgay)
                          AND (@denNgay IS NULL OR Base.ThoiGianBatDau <= @denNgay)
                    ) AS Filtered
                ) T1 WHERE RowNum > @startRow AND RowNum <= @endRow;
            ";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }
        public string GenerateMaCuocHop(Guid projectId)
        {
            string sql = $@"
                DECLARE @projectId VARCHAR(36) = '{projectId}';
        
                SELECT ISNULL(MAX(TRY_CAST(REPLACE(MaCuocHop, 'Meet', '') AS INT)), 0) AS MaxNumber
                FROM TblLichHop WITH (UPDLOCK, HOLDLOCK)
                WHERE IdDuAn = @projectId
                   AND MaCuocHop LIKE 'Meet%';
            ";

            int nextNumber = 1;
            using (IDataReader reader = new InlineQuery().ExecuteReader(sql))
            {
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        if (reader["MaxNumber"] != DBNull.Value)
                        {
                            nextNumber = Convert.ToInt32(reader["MaxNumber"]) + 1;
                        }
                    }
                    reader.Close();
                }
            }
            return "Meet" + nextNumber;
        }
        public void UpdateTblLichHopNhanVien(Guid idLichHop, string lstNhanVienIds)
        {
            new InlineQuery().Execute($"DELETE FROM TblLichHop_NhanVien WHERE IdLichHop = '{idLichHop}'");
            if (!string.IsNullOrEmpty(lstNhanVienIds))
            {
                string[] userIds = lstNhanVienIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string uid in userIds)
                {
                    string insertSql = $"INSERT INTO TblLichHop_NhanVien (IdLichHop, IdNhanVien) VALUES ('{idLichHop}', '{uid.Trim()}')";
                    new InlineQuery().Execute(insertSql);
                }
            }
        }
        public DataTable GetNhanVienThamGia(Guid idLichHop)
        {
            string sql = $@"
                SELECT u.UserId, u.DisplayName 
                FROM TblLichHop_NhanVien ln
                INNER JOIN aspnet_Users u ON ln.IdNhanVien = u.UserId
                WHERE ln.IdLichHop = '{idLichHop}'";

            DataTable dt = new DataTable();
            using (System.Data.IDataReader reader = new SubSonic.InlineQuery().ExecuteReader(sql))
            {
                if (reader != null)
                {
                    dt.Load(reader);
                    reader.Close();
                }
            }
            return dt;
        }
        public bool DeleteMeet(TblLichHop meet)
        {
            if (meet == null) return false;

            meet.DaXoa = true;
            meet.NgayCapNhat = DateTime.Now;
            meet.IdNguoiCapNhat = SweetContext.Current.UserId;
            meet.Save();

            return true;
        }
        public List<Guid> GetNhanVienCuocHop(Guid idCuocHop)
        {
            List<Guid> result = new List<Guid>();

            try
            {
                var records = new Select(TblLichHopNhanVien.Columns.IdNhanVien)
                    .From(TblLichHopNhanVien.Schema)
                    .Where(TblLichHopNhanVien.Columns.IdLichHop).IsEqualTo(idCuocHop)
                    .ExecuteTypedList<Guid>();

                if (records != null && records.Count > 0)
                {
                    result = records;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}
