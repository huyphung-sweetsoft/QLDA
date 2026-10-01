using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using SweetSoft.QLDA.Core.Managers;

namespace SweetSoft.QLDA.Core.Respositories
{
    public partial class DocumentRepository
    {
        // Derive per-file events from snapshots so a whole-set restore is also
        // visible in every affected file's timeline, without synthetic versions.
        private const string FileTransitionsSql = @"
            WITH Snapshots AS (
                SELECT * FROM dbo.TblPhienBanTaiLieu WHERE IdTaiLieu=@D AND DaXoa=0
            ), Members AS (
                SELECT p.IdPhienBanTaiLieu, TRY_CONVERT(uniqueidentifier,j.value) IdFile,
                    COALESCE(root.IdChuoiFile,TRY_CONVERT(uniqueidentifier,j.value)) IdChuoiFile
                FROM Snapshots p
                CROSS APPLY OPENJSON(COALESCE(p.DanhSachFileJson,
                    CASE WHEN p.IdFileNoiDung IS NULL THEN N'[]' ELSE N'[""'+CONVERT(nvarchar(36),p.IdFileNoiDung)+N'""]' END)) j
                OUTER APPLY (SELECT TOP 1 e.IdChuoiFile FROM Snapshots e
                    WHERE e.IdFileThayDoi=TRY_CONVERT(uniqueidentifier,j.value) AND e.IdChuoiFile IS NOT NULL
                    ORDER BY e.NgayTao,e.IdPhienBanTaiLieu) root
            ), Roots AS (SELECT DISTINCT IdChuoiFile FROM Members), States AS (
                SELECT p.IdPhienBanTaiLieu,p.NgayTao,p.NguoiTao,p.NguonTao,r.IdChuoiFile,m.IdFile,
                    TRY_CONVERT(decimal(18,4),p.SoPhienBan) ThuTu,
                    LAG(CONVERT(nvarchar(36),m.IdFile)) OVER (PARTITION BY r.IdChuoiFile
                        ORDER BY p.NgayTao,TRY_CONVERT(decimal(18,4),p.SoPhienBan),p.IdPhienBanTaiLieu) PreviousFile
                FROM Snapshots p CROSS JOIN Roots r LEFT JOIN Members m
                    ON m.IdPhienBanTaiLieu=p.IdPhienBanTaiLieu AND m.IdChuoiFile=r.IdChuoiFile
            ), FileChanges AS (
                SELECT *,ROW_NUMBER() OVER(PARTITION BY IdChuoiFile ORDER BY NgayTao,ThuTu,IdPhienBanTaiLieu) FileVersion
                FROM States WHERE ISNULL(CONVERT(nvarchar(36),IdFile),'')<>ISNULL(PreviousFile,'')
            ) ";

        public DocumentFileSet PlanSnapshotRestore(Guid documentId, Guid versionId, Guid? selectedFile)
        {
            if (GetDocumentVersionById(documentId, versionId) == null)
                throw new InvalidOperationException("Mốc lịch sử không tồn tại hoặc không thuộc hồ sơ.");
            var source = GetDocumentVersionFileIds(documentId, versionId);
            var current = GetCurrentDocumentFileSet(documentId);
            var desired = source;
            if (selectedFile.HasValue)
            {
                if (!source.Contains(selectedFile.Value)) throw new InvalidOperationException("File không thuộc mốc lịch sử đã chọn.");
                Guid root = FileRoot(documentId, selectedFile.Value);
                desired = current.FileIds.Where(f => FileRoot(documentId, f) != root).ToList();
                desired.Add(selectedFile.Value);
            }
            if (desired.Select(f => FileRoot(documentId, f)).Distinct().Count() != desired.Count)
                throw new InvalidOperationException("Mốc này có nhiều bản thuộc cùng một dòng file. Hãy khôi phục từng file.");
            // Preview is read-only: pending requests are never recalled implicitly.
            // SaveDocumentFileSet repeats these guards under the dossier lock.
            foreach (Guid removed in current.FileIds.Except(desired))
                ValidateFileRemoval(documentId, removed, false, string.Empty, DateTime.UtcNow);
            return new DocumentFileSet { VersionId = current.VersionId, FileIds = desired };
        }

        public DocumentFileSet RestoreSnapshot(Guid documentId, Guid versionId, Guid? selectedFile,
            Guid? expectedVersion, string user, DateTime now)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(2)))
            {
                ExecuteScalarInt("SELECT COUNT(*) FROM dbo.TblTaiLieu WITH(UPDLOCK,HOLDLOCK) WHERE IdTaiLieu=@D AND DaXoa=0",
                    new Dictionary<string,object>{{"@D",documentId}});
                var plan = PlanSnapshotRestore(documentId, versionId, selectedFile);
                if (plan.VersionId != expectedVersion)
                    throw new InvalidOperationException("Bộ file đã thay đổi sau khi xem xác nhận. Hãy mở lại thao tác khôi phục.");
                var saved = SaveDocumentFileSet(documentId, expectedVersion, plan.FileIds, user, now,
                    (selectedFile.HasValue ? "Khôi phục file" : "Khôi phục bộ file") + " từ mốc " + versionId, "KHOI_PHUC");
                scope.Complete();
                return saved;
            }
        }

        private Guid FileRoot(Guid documentId, Guid fileId)
        {
            object root = ExecuteScalarObject(@"SELECT TOP 1 IdChuoiFile FROM dbo.TblPhienBanTaiLieu
                WHERE IdTaiLieu=@D AND IdFileThayDoi=@F AND IdChuoiFile IS NOT NULL AND DaXoa=0
                ORDER BY NgayTao,IdPhienBanTaiLieu;",
                new Dictionary<string,object>{{"@D",documentId},{"@F",fileId}});
            return root == null || root == DBNull.Value ? fileId : (Guid)root;
        }

        public DataTable GetWorkspaceFiles(Guid documentId)
        {
            return ExecuteDataTable(FileTransitionsSql + @"
                SELECT u.Id AS IdFile,COALESCE(lineage.IdChuoiFile,u.Id) AS IdChuoiFile,
                    COALESCE(NULLIF(u.OriginalFileName,N''),u.Name) AS TenFile,u.FileUrl,u.FileSize,
                    ISNULL(history.VersionCount,1) AS FileVersion,
                    COALESCE(history.LastDate,u.CreatedDate) AS NgayCapNhat,
                    ISNULL(signing.TrangThai,'CHUA_TRINH') AS TrangThai,
                    signing.IdFileSauKy,resultFile.FileUrl AS SignedFileUrl,
                    CASE WHEN signedLock.Found=1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS DaKhoa
                FROM dbo.TblPhienBanTaiLieu p
                CROSS APPLY OPENJSON(COALESCE(p.DanhSachFileJson,
                    CASE WHEN p.IdFileNoiDung IS NULL THEN N'[]' ELSE N'[""' + CONVERT(nvarchar(36),p.IdFileNoiDung) + N'""]' END)) member
                JOIN dbo.TblUploadFile u ON u.Id=TRY_CONVERT(uniqueidentifier,member.value)
                    AND u.RefId=@D AND u.RefType='DocumentVersion' AND u.IsDeleted=0
                OUTER APPLY(SELECT TOP 1 e.IdChuoiFile FROM dbo.TblPhienBanTaiLieu e
                    WHERE e.IdTaiLieu=@D AND e.IdFileThayDoi=u.Id AND e.IdChuoiFile IS NOT NULL AND e.DaXoa=0
                    ORDER BY e.NgayTao,e.IdPhienBanTaiLieu) lineage
                OUTER APPLY(SELECT MAX(e.FileVersion) VersionCount,MAX(e.NgayTao) LastDate FROM FileChanges e
                    WHERE e.IdChuoiFile=COALESCE(lineage.IdChuoiFile,u.Id)) history
                OUTER APPLY(SELECT TOP 1 f.TrangThai,f.IdFileSauKy FROM dbo.TblTrinhKyTaiLieuFile f
                    JOIN dbo.TblTrinhKyTaiLieu s ON s.IdTrinhKyTaiLieu=f.IdTrinhKyTaiLieu AND s.DaXoa=0
                    WHERE f.IdFileNguon=u.Id AND f.DaXoa=0
                    ORDER BY CASE WHEN f.TrangThai='DA_KY' THEN 0 ELSE 1 END,s.NgayTao DESC,f.IdTrinhKyTaiLieuFile DESC) signing
                LEFT JOIN dbo.TblUploadFile resultFile ON resultFile.Id=signing.IdFileSauKy AND resultFile.IsDeleted=0
                OUTER APPLY(SELECT TOP 1 1 AS Found FROM dbo.TblTrinhKyTaiLieuFile f
                    JOIN dbo.TblTrinhKyTaiLieu s ON s.IdTrinhKyTaiLieu=f.IdTrinhKyTaiLieu AND s.DaXoa=0
                    WHERE f.DaXoa=0 AND f.TrangThai='DA_KY' AND (f.IdFileNguon=COALESCE(lineage.IdChuoiFile,u.Id)
                    OR f.IdFileNguon IN(SELECT e.IdFileThayDoi FROM dbo.TblPhienBanTaiLieu e
                        WHERE e.IdTaiLieu=@D AND e.IdChuoiFile=COALESCE(lineage.IdChuoiFile,u.Id)))) signedLock
                WHERE p.IdTaiLieu=@D AND p.DaXoa=0 AND p.LaPhienBanHienTai=1
                ORDER BY COALESCE(history.LastDate,u.CreatedDate) DESC,u.Id;",
                new Dictionary<string,object>{{"@D",documentId}});
        }

        public DataTable GetFileTimeline(Guid documentId, Guid root)
        {
            return ExecuteDataTable(FileTransitionsSql + @"
                SELECT e.*,
                    CASE WHEN e.IdFile IS NULL THEN N'Gỡ file' WHEN e.NguonTao='KHOI_PHUC' THEN N'Khôi phục'
                        WHEN e.FileVersion=1 THEN N'Tải file lần đầu' ELSE N'Tải bản mới' END AS HanhDong,
                    COALESCE(NULLIF(u.OriginalFileName,N''),u.Name) AS TenFile,u.FileUrl,
                    COALESCE(NULLIF(a.DisplayName,N''),e.NguoiTao) AS NguoiThucHien
                FROM FileChanges e LEFT JOIN dbo.TblUploadFile u ON u.Id=e.IdFile AND u.RefId=@D AND u.IsDeleted=0
                LEFT JOIN dbo.aspnet_Users a ON a.UserName=e.NguoiTao
                WHERE e.IdChuoiFile=@Root
                ORDER BY e.NgayTao DESC,e.ThuTu DESC,e.IdPhienBanTaiLieu DESC;",
                new Dictionary<string,object>{{"@D",documentId},{"@Root",root}});
        }

        // Called under the same dossier lock used by signing completion and file-set writes.
        private void ValidateFileRemoval(Guid documentId, Guid fileId, bool recall, string user, DateTime now)
        {
            Guid root = FileRoot(documentId,fileId);
            var args = new Dictionary<string,object>{{"@D",documentId},{"@F",fileId},{"@Root",root},{"@User",user},{"@Now",now}};
            int signed = ExecuteScalarInt(@"SELECT COUNT(*) FROM dbo.TblTrinhKyTaiLieuFile f
                JOIN dbo.TblTrinhKyTaiLieu s ON s.IdTrinhKyTaiLieu=f.IdTrinhKyTaiLieu AND s.DaXoa=0
                WHERE f.DaXoa=0 AND f.TrangThai='DA_KY' AND
                (f.IdFileNguon=@Root OR f.IdFileNguon IN(SELECT IdFileThayDoi FROM dbo.TblPhienBanTaiLieu WHERE IdTaiLieu=@D AND IdChuoiFile=@Root));",args);
            if (signed>0) throw new InvalidOperationException("File đã ký bị khóa; không thể thay, khôi phục hoặc gỡ.");
            var requests=ExecuteDataTable(@"SELECT DISTINCT f.IdTrinhKyTaiLieu FROM dbo.TblTrinhKyTaiLieuFile f
                JOIN dbo.TblTrinhKyTaiLieu s ON s.IdTrinhKyTaiLieu=f.IdTrinhKyTaiLieu AND s.DaXoa=0
                WHERE f.IdFileNguon=@F AND f.DaXoa=0 AND f.TrangThai IN('DANG_TRINH','DANG_TRINH_KY');",args);
            if(requests.Rows.Count>0 && !recall)
                throw new InvalidOperationException("File đang chờ ký. Hãy xác nhận thu hồi yêu cầu ký trước khi thay đổi.");
            if(requests.Rows.Count>0)
            {
                ExecuteNonQuery(@"UPDATE dbo.TblTrinhKyTaiLieuFile SET TrangThai='THU_HOI',
                    NguoiCapNhat=@User,NgayCapNhat=@Now
                    WHERE IdFileNguon=@F AND DaXoa=0 AND TrangThai IN('DANG_TRINH','DANG_TRINH_KY');",args);
                foreach(DataRow request in requests.Rows)
                    RefreshSigningAggregateStatus(documentId,(Guid)request["IdTrinhKyTaiLieu"],user,now);
            }
        }

        public DocumentFileSet ChangeWorkspaceFile(Guid documentId,Guid previous,Guid? next,bool restore,bool recall,string user,DateTime now)
        {
            using(var scope=new TransactionScope(TransactionScopeOption.Required,TimeSpan.FromMinutes(2)))
            {
                if(ExecuteScalarInt("SELECT COUNT(*) FROM dbo.TblTaiLieu WITH(UPDLOCK,HOLDLOCK) WHERE IdTaiLieu=@D AND DaXoa=0",
                    new Dictionary<string,object>{{"@D",documentId}})!=1) throw new InvalidOperationException("Không tìm thấy hồ sơ.");
                var current=GetCurrentDocumentFileSet(documentId);
                if(!current.FileIds.Contains(previous)) throw new InvalidOperationException("File đã có bản mới. Vui lòng tải lại trang.");
                Guid root=FileRoot(documentId,previous);
                if(next==previous) throw new InvalidOperationException("Đây đã là bản hiện tại.");
                if(next.HasValue)
                {
                    if(current.FileIds.Contains(next.Value)) throw new InvalidOperationException("File này đã có trong hồ sơ.");
                    if(restore && !GetFileTimeline(documentId,root).AsEnumerable().Any(r=>r["IdFile"]!=DBNull.Value && (Guid)r["IdFile"]==next.Value))
                        throw new InvalidOperationException("Phiên bản không thuộc lịch sử của file này.");
                    if(!restore && GetDocumentVersionsWithFiles(documentId).AsEnumerable().Any(r=>r["IdFile"]!=DBNull.Value && (Guid)r["IdFile"]==next.Value))
                        throw new InvalidOperationException("Hãy tải lên một file mới.");
                }
                ValidateFileRemoval(documentId,previous,recall,user,now);
                var ids=current.FileIds.Where(id=>id!=previous).ToList();
                if(next.HasValue)ids.Add(next.Value);
                var saved=SaveDocumentFileSet(documentId,current.VersionId,ids,user,now,
                    restore ? "Khôi phục file" : next.HasValue ? "Thay bản mới cho file" : "Gỡ file",
                    restore ? "KHOI_PHUC" : "UPLOAD");
                ExecuteNonQuery(@"UPDATE dbo.TblPhienBanTaiLieu SET IdChuoiFile=@Root,IdFileTruoc=@Previous,IdFileThayDoi=@Next
                    WHERE IdPhienBanTaiLieu=@Version AND IdTaiLieu=@D;",
                    new Dictionary<string,object>{{"@D",documentId},{"@Version",saved.VersionId},{"@Root",root},
                        {"@Previous",previous},{"@Next",(object)next??DBNull.Value}});
                scope.Complete();
                return saved;
            }
        }

        public Guid? GetStorageLocation(Guid documentId)
        {
            var value=ExecuteScalarObject(@"SELECT TOP 1 IdNoiLuuTru FROM dbo.TblLuuTruVatLy
                WHERE IdTaiLieu=@D AND DaXoa=0 AND LaViTriHienTai=1 ORDER BY NgayTao DESC",
                new Dictionary<string,object>{{"@D",documentId}});
            return value==null||value==DBNull.Value?(Guid?)null:(Guid)value;
        }

        public void SetStorageLocation(Guid documentId,Guid? location,Guid userId,string user)
        {
            if(location.HasValue)
            {
                StoreDocumentPhysicalCopy(documentId,location.Value,false,"","","Cập nhật nơi lưu từ thông tin hồ sơ.",userId,user,DateTime.UtcNow);
                return;
            }
            ExecuteNonQuery(@"UPDATE dbo.TblLuuTruVatLy SET LaViTriHienTai=0,NguoiCapNhat=@User,NgayCapNhat=GETUTCDATE()
                WHERE IdTaiLieu=@D AND LaViTriHienTai=1 AND DaXoa=0;
                UPDATE dbo.TblTaiLieu SET TrangThaiLuuTru='CHUA_LUU' WHERE IdTaiLieu=@D;",
                new Dictionary<string,object>{{"@D",documentId},{"@User",user}});
        }
    }
}
