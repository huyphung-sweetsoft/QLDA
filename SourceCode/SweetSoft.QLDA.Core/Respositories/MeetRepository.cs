using SubSonic;
using SweetSoft.QLDA.Core.Infrastructure;
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
    internal class MeetRepository : BaseRepository<TblLichHop>
    {
        public MeetRepository(AuditManager auditManager) : base(auditManager) { }
        public DataTable SearchMeeting(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            totalRecord = 0;

            string tenCuocHop = parameters != null && parameters.ContainsKey(TblLichHop.Columns.TenCuocHop) ? parameters[TblLichHop.Columns.TenCuocHop]?.ToString() : null;

            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "ThoiGianBatDau DESC";
            }

            string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
        
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenCuocHop NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenCuocHop)}%';

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
                        
                                STUFF((
                                    SELECT ', ' + u.DisplayName
                                    FROM [dbo].[TblLichHop_NhanVien] ln
                                    INNER JOIN [dbo].[aspnet_Users] u ON ln.IdNhanVien = u.UserId
                                    WHERE ln.IdLichHop = m.IdLichHop
                                       AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)
                                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS NhanVienThamGia
                            
                            FROM TblLichHop m
                            WHERE m.DaXoa = 0 
                              AND m.IdDuAn = @projectId
                        ) AS Base
                        WHERE (@tenCuocHop IS NULL OR Base.TenCuocHop LIKE N'%' + @tenCuocHop + '%')
                          AND (@singleKeyWord = N'%%'
                                OR Base.MaCuocHop LIKE @singleKeyWord
                                OR Base.TenCuocHop LIKE @singleKeyWord
                                OR Base.DiaDiemHop LIKE @singleKeyWord
                                OR Base.NhanVienThamGia LIKE @singleKeyWord)
                    ) AS Filtered
                ) T1 WHERE RowNum > @startRow AND RowNum <= @endRow;";

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
    }
}
