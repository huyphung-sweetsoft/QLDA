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
            string mucDoAnhHuong = parameters != null && parameters.ContainsKey(TblVanDe.Columns.MucDoAnhHuong) ? parameters[TblVanDe.Columns.MucDoAnhHuong]?.ToString() : null;
            string trangThai = parameters != null && parameters.ContainsKey(TblVanDe.Columns.TrangThai) ? parameters[TblVanDe.Columns.TrangThai]?.ToString() : null;
            string nguonGoc = parameters != null && parameters.ContainsKey(TblVanDe.Columns.NguonGocVanDe) ? parameters[TblVanDe.Columns.NguonGocVanDe]?.ToString() : null;
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "MaVanDe ASC";
            }
            string sqlMucDoAnhHuong = string.IsNullOrEmpty(mucDoAnhHuong) ? "NULL" : mucDoAnhHuong;
            string sqlTrangThai = string.IsNullOrEmpty(trangThai) ? "NULL" : trangThai;
            string sqlNguonGoc = string.IsNullOrEmpty(nguonGoc) ? "NULL" : nguonGoc;

             Guid viewerId = SweetContext.Current == null ? Guid.Empty : SweetContext.Current.UserId;
             string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
                DECLARE @viewerId UNIQUEIDENTIFIER = '{viewerId}';
        
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenVanDe NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenVanDe)}%';
        
                DECLARE @mucDoAnhHuong INT = {sqlMucDoAnhHuong};
                DECLARE @trangThai INT = {sqlTrangThai};
                DECLARE @nguonGoc INT = {sqlNguonGoc};

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
                                v.TrangThaiDuyetGio,
                                 CASE WHEN EXISTS(SELECT 1 FROM dbo.TblVanDe_NhanVien n
                                        WHERE n.IdVanDe=v.IdVanDe AND n.IdNhanVien=@viewerId)
                                      THEN 1 ELSE 0 END AS LaNguoiXuLy,
                                 CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                                     WHERE s.IdDuAn=v.IdDuAn AND s.IdVanDe=v.IdVanDe AND s.IdNhanVien=@viewerId
                                       AND s.LaBanHienTai=1 AND s.LoaiSuKien IN(1,3)
                                       AND s.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                                                            WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END))
                                      THEN 1 ELSE 0 END AS DaBaoGioVanDe,
                                 CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p
                                     JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien AND k.SoGiay>0
                                     WHERE p.IdVanDe=v.IdVanDe AND p.IdDuAn=v.IdDuAn AND p.LoaiGio=3)
                                     OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                                     WHERE s.IdVanDe=v.IdVanDe AND s.IdDuAn=v.IdDuAn AND s.LaBanHienTai=1
                                       AND s.LoaiSuKien IN(1,3,4,6))
                                     THEN 1 ELSE 0 END AS CoDuLieuGioVanDe,
                                 CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                                     WHERE s.IdVanDe=v.IdVanDe AND s.IdDuAn=v.IdDuAn AND s.LaBanHienTai=1
                                       AND s.LoaiSuKien IN(4,6)) THEN 1 ELSE 0 END AS DaChotGioVanDe,
                                 CASE WHEN EXISTS(
                                     SELECT 1 FROM dbo.TblKhaiBaoGioPhien p
                                     LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                                     WHERE p.IdVanDe=v.IdVanDe AND p.IdDuAn=v.IdDuAn AND p.LoaiGio=3
                                     GROUP BY p.IdNhanVien
                                     HAVING ISNULL(SUM(k.SoGiay),0)>0 AND NOT EXISTS(
                                         SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                                         WHERE s.IdVanDe=v.IdVanDe AND s.IdDuAn=v.IdDuAn
                                           AND s.IdNhanVien=p.IdNhanVien AND s.LaBanHienTai=1
                                           AND s.LoaiSuKien IN(1,3,4,6)
                                           AND s.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                                               WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END)
                                     )
                                 ) THEN 1 ELSE 0 END AS CoNguoiChuaBaoGio,
                                 v.NguonGocVanDe,
                                v.NguoiTao,
                                v.NgayTao
                            FROM TblVanDe v
                            WHERE v.DaXoa = 0 AND v.IdDuAn = @projectId
                        ) AS Base
                        WHERE (@tenVanDe = N'%%' OR Base.TenVanDe LIKE @tenVanDe)
                          AND (@singleKeyWord = N'%%'
                                OR Base.MaVanDe LIKE @singleKeyWord 
                                OR Base.TenVanDe LIKE @singleKeyWord
                                OR Base.NguoiTao LIKE @singleKeyWord)
                          AND (@mucDoAnhHuong IS NULL OR Base.MucDoAnhHuong = @mucDoAnhHuong)
                          AND (@trangThai IS NULL OR Base.TrangThai = @trangThai)
                          AND (@nguonGoc IS NULL OR Base.NguonGocVanDe = @nguonGoc)
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
        public bool DeleteIssue(TblVanDe item)
        {
            if (item == null) return false;

            var id = item.IdVanDe;

            TblVanDe itemOld = TblVanDe.FetchByID(id); 

            item.DaXoa = true;
            item.NgayCapNhat = DateTime.Now;
            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, item, "TblVanDe", id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log delete for TblVanDe");
                }
            });
            return true;
        }
    }
}
