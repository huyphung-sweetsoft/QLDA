using SubSonic;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.ResourceTexts;
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
    public class IssueRepository:BaseRepository<TblVanDe>
    {
        public IssueRepository(AuditManager auditManager) : base(auditManager) { }
        public DataTable SearchIssue(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            totalRecord = 0;
            string tenVanDe = parameters != null && parameters.ContainsKey(TblVanDe.Columns.TenVanDe) ? parameters[TblVanDe.Columns.TenVanDe]?.ToString() : null;
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "MaVanDe ASC";
            }
            string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
        
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenVanDe NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenVanDe)}%';
        
                SELECT * FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, Filtered.* FROM (
                        SELECT 
                            Base.*, 
                            COUNT(1) OVER() AS total_records 
                        FROM (
                            SELECT 
                                v.IdVanDe,
                                v.MaVanDe,
                                v.TenVanDe,
                                v.MucDoAnhHuong,
                                v.TrangThai,
                                v.NguonGocVanDe,
                                v.NguoiTao
                            FROM TblVanDe v
                            WHERE v.DaXoa = 0 AND v.IdDuAn = @projectId
                        ) AS Base
                        WHERE (@tenVanDe IS NULL OR Base.TenVanDe LIKE N'%' + @tenVanDe + '%')
                          AND (@singleKeyWord = N'%%'
                                OR Base.MaVanDe LIKE @singleKeyWord 
                                OR Base.TenVanDe LIKE @singleKeyWord
                                OR Base.NguoiTao LIKE @singleKeyWord)
                    ) AS Filtered
                ) AS T1 
                WHERE RowNum > @startRow AND RowNum <= @endRow;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }
        public TblVanDe GetById(int issueId)
        {
            return TblVanDe.FetchByID(issueId);
        }

        public void UpdateVande(TblVanDe vande)
        {
            vande.Save();
        }
        public void SyncNhanVienXuLyVanDe(Guid idVanDe, Guid idCongViec)
        {
            string sql = $@"
                    DECLARE @idVanDe VARCHAR(36) = '{idVanDe}';
                    DECLARE @idCongViec VARCHAR(36) = '{idCongViec}';

                    DELETE FROM TblVanDe_NhanVien WHERE IdVanDe = @idVanDe;

                    IF (@idCongViec <> '00000000-0000-0000-0000-000000000000')
                    BEGIN
                        INSERT INTO TblVanDe_NhanVien (IdVanDe, IdNhanVien)
                        SELECT @idVanDe, IdNhanVien 
                        FROM TblCongViec_NhanVien 
                        WHERE IdCongViec = @idCongViec
                    END
                ";

            new InlineQuery().Execute(sql);
        }
        public string GenerateMaVanDe(Guid projectId)
        {
            string sql = $@"
                DECLARE @projectId VARCHAR(36) = '{projectId}';
        
                -- Dùng UPDLOCK và HOLDLOCK để khóa bảng với những giao dịch khác đang cố gắng đọc max ID
                SELECT ISNULL(MAX(TRY_CAST(REPLACE(MaVanDe, 'Iss', '') AS INT)), 0) AS MaxNumber
                FROM TblVanDe WITH (UPDLOCK, HOLDLOCK)
                WHERE IdDuAn = @projectId 
                  AND MaVanDe LIKE 'Iss%';
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
            return "Iss" + nextNumber;
        }
        public void DeleteIssue(TblVanDe issue)
        {
            BusinessValidator.ThrowIfNull(issue, BackEndResourceKeys.INVALID_DATA);

            string sql = $@"
                    UPDATE TblVanDe 
                    SET DaXoa = 1, 
                        NguoiCapNhat = N'{SweetContext.Current.UserName}', 
                        NgayCapNhat = GETDATE()
                    WHERE IdVanDe = '{issue.IdVanDe}'
                ";
            new InlineQuery().Execute(sql);
        }
    }
}
