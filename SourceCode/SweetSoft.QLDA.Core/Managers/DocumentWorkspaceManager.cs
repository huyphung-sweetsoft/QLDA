using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Respositories;

namespace SweetSoft.QLDA.Core.Managers
{
    public partial class DocumentManager
    {
        public DocumentFileSet PlanSnapshotRestore(Guid documentId, Guid versionId, Guid? fileId)
        {
            EnsureDocumentAccess(documentId, DocumentPermissionKeys.ManageFiles);
            var plan = _repository.PlanSnapshotRestore(documentId, versionId, fileId);
            foreach (Guid id in plan.FileIds)
                if (!IsDocumentFileAvailable(_repository.GetDocumentVersionFileById(documentId, id)))
                    throw new InvalidOperationException("Không thể khôi phục: có file không còn tồn tại. Hãy kiểm tra danh sách file ở mốc này.");
            return plan;
        }

        public DocumentFileSet RestoreSnapshot(Guid documentId, Guid versionId, Guid? fileId, Guid? expectedVersion)
        {
            PlanSnapshotRestore(documentId, versionId, fileId);
            var saved = _repository.RestoreSnapshot(documentId, versionId, fileId, expectedVersion, GetCurrentUserName(), DateTime.UtcNow);
            if (saved.Created)
                WriteDocumentAudit(documentId, DocumentActivityTypeKeys.RestoreVersion,
                    DocumentActivityReferenceKeys.DocumentVersion, saved.VersionId,
                    "Mốc nguồn: " + versionId + "; File: " + fileId,
                    fileId.HasValue ? "Khôi phục một dòng file từ lịch sử bộ file." : "Khôi phục toàn bộ danh sách file từ lịch sử.");
            return saved;
        }

        public DataTable GetWorkspaceFiles(Guid documentId)
        {
            EnsureDocumentAccess(documentId, ActionKeys.View);
            return _repository.GetWorkspaceFiles(documentId);
        }

        public DataTable GetFileTimeline(Guid documentId, Guid rootFileId)
        {
            EnsureDocumentAccess(documentId, ActionKeys.View);
            return _repository.GetFileTimeline(documentId, rootFileId);
        }

        public void ChangeWorkspaceFile(Guid documentId, Guid currentFileId, Guid? newFileId,
            bool restoring, bool recallPending)
        {
            EnsureDocumentAccess(documentId, DocumentPermissionKeys.ManageFiles);
            var before = _repository.GetWorkspaceFiles(documentId).AsEnumerable()
                .FirstOrDefault(row => (Guid)row["IdFile"] == currentFileId);
            if (before == null) throw new InvalidOperationException("File đã thay đổi. Vui lòng tải lại trang.");
            if (newFileId.HasValue)
            {
                var file = _repository.GetDocumentVersionFileById(documentId, newFileId.Value);
                if (!IsDocumentFileAvailable(file))
                    throw new InvalidOperationException("File không còn tồn tại hoặc không thuộc hồ sơ.");
            }
            var saved = _repository.ChangeWorkspaceFile(documentId, currentFileId, newFileId,
                restoring, recallPending, GetCurrentUserName(), DateTime.UtcNow);
            if (saved.Created)
                WriteDocumentAudit(documentId, restoring ? DocumentActivityTypeKeys.RestoreVersion : DocumentActivityTypeKeys.UploadVersion,
                    DocumentActivityReferenceKeys.DocumentVersion, saved.VersionId,
                    "File cũ: " + currentFileId + "; file mới: " + newFileId,
                    restoring ? "Khôi phục một file từ lịch sử." : newFileId.HasValue ? "Tải bản mới cho một file." : "Gỡ một file khỏi hồ sơ.");
        }

        public Guid? GetStorageLocation(Guid documentId)
        {
            EnsureDocumentAccess(documentId, ActionKeys.View);
            return _repository.GetStorageLocation(documentId);
        }

        public void SaveStorageLocation(Guid documentId, Guid? locationId, bool creating)
        {
            var doc = GetAccessibleDocument(documentId);
            if (_repository.GetStorageLocation(documentId) == locationId) return;
            if (creating)
            {
                bool allowed = doc.IdDuAn.HasValue
                    ? CanAccessProjectDocument(doc.IdDuAn.Value, ActionKeys.Create)
                    : CanCreateCompanyDocument();
                if (!allowed) throw new UnauthorizedAccessException();
            }
            else EnsureDocumentAccess(documentId, DocumentPermissionKeys.UpdateInfo);
            _repository.SetStorageLocation(documentId, locationId, GetCurrentUserId(), GetCurrentUserName());
            WriteDocumentAudit(documentId, DocumentActivityTypeKeys.StorePhysicalCopy,
                DocumentActivityReferenceKeys.Document, documentId,
                locationId.HasValue ? locationId.Value.ToString() : "",
                locationId.HasValue ? "Cập nhật nơi lưu trữ từ thông tin hồ sơ." : "Bỏ chọn nơi lưu trữ; giữ nguyên lịch sử.");
        }
    }
}
