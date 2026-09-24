using SubSonic;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;

namespace SweetSoft.QLDA.Core.FileManager
{
    /// <summary>Authorization for files attached directly to a cost or meeting row.</summary>
    public static class ProjectRecordFileAccess
    {
        public static bool IsRecordAttachment(string refType)
        {
            return refType == FileUploadTypes.CostAttachment.ToString()
                || refType == FileUploadTypes.MeetingAttachment.ToString();
        }

        public static bool CanAccess(Guid userId, Guid refId, string refType, bool write)
        {
            if (userId == Guid.Empty || refId == Guid.Empty || !IsRecordAttachment(refType))
                return false;

            ModuleKeys module;
            if (refType == FileUploadTypes.CostAttachment.ToString())
            {
                TblChiPhi cost = TblChiPhi.FetchByID(refId);
                if (cost == null || cost.DaXoa == true || cost.IdDuAn == Guid.Empty)
                    return false;
                module = ModuleKeys.Cost;
            }
            else
            {
                TblLichHop meeting = TblLichHop.FetchByID(refId);
                if (meeting == null || meeting.DaXoa == true || meeting.IdDuAn == Guid.Empty)
                    return false;
                module = ModuleKeys.Meet;
            }

            FunctionManager functions = FunctionManager.Instance;
            if (write)
                return functions.IsActionKeyExisted(userId, module, ActionKeys.Update);

            // Matches BaseAdminPage.IsView for the two list pages.
            return functions.IsActionKeyExisted(userId, module, ActionKeys.View)
                || functions.IsActionKeyExisted(userId, module, ActionKeys.Create)
                || functions.IsActionKeyExisted(userId, module, ActionKeys.Update);
        }

        public static Guid? GetLinkedFileId(Guid refId, string refType)
        {
            if (refId == Guid.Empty || !IsRecordAttachment(refType))
                return null;

            string table = refType == FileUploadTypes.CostAttachment.ToString()
                ? "dbo.TblChiPhi" : "dbo.TblLichHop";
            string key = refType == FileUploadTypes.CostAttachment.ToString()
                ? "IdChiPhi" : "IdLichHop";
            QueryCommand command = new QueryCommand(
                "SELECT IdUploadFile FROM " + table +
                " WHERE " + key + " = @RecordId AND DaXoa = 0;",
                TblUploadFile.Schema.Provider.Name);
            command.Parameters.Add("@RecordId", refId, DbType.Guid);
            object value = DataService.ExecuteScalar(command);
            return value == null || value == DBNull.Value
                ? (Guid?)null : (Guid)value;
        }

        public static bool IsLinkedFile(Guid refId, string refType, Guid fileId)
        {
            Guid? linkedId = GetLinkedFileId(refId, refType);
            return linkedId.HasValue && linkedId.Value == fileId;
        }

        public static bool BelongsToRecord(Guid refId, string refType, Guid fileId)
        {
            if (refId == Guid.Empty || fileId == Guid.Empty || !IsRecordAttachment(refType))
                return false;

            TblUploadFile file = TblUploadFile.FetchByID(fileId);
            // A replaced file can already be soft-deleted by LinkFile before
            // FilesBox posts its pending removal. It still belongs to this row.
            return file != null && file.RefId == refId && file.RefType == refType;
        }

        public static void LinkFile(Guid userId, Guid refId, string refType, Guid fileId)
        {
            if (fileId == Guid.Empty || !CanAccess(userId, refId, refType, true))
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền cập nhật file chi phí/lịch họp.");

            string table = refType == FileUploadTypes.CostAttachment.ToString()
                ? "dbo.TblChiPhi" : "dbo.TblLichHop";
            string key = refType == FileUploadTypes.CostAttachment.ToString()
                ? "IdChiPhi" : "IdLichHop";
            QueryCommand command = new QueryCommand(
                "SET XACT_ABORT ON; SET NOCOUNT ON;" +
                " BEGIN TRY BEGIN TRANSACTION;" +
                " DECLARE @PreviousFileId UNIQUEIDENTIFIER;" +
                " SELECT @PreviousFileId = IdUploadFile FROM " + table +
                " WITH (UPDLOCK, HOLDLOCK) WHERE " + key +
                " = @RecordId AND DaXoa = 0;" +
                " IF @@ROWCOUNT <> 1 THROW 52020, 'Record not found.', 1;" +
                " IF NOT EXISTS (SELECT 1 FROM dbo.TblUploadFile" +
                " WHERE Id = @FileId AND RefId = @RecordId" +
                " AND RefType = @RefType AND IsDeleted = 0)" +
                " THROW 52021, 'Upload does not belong to record.', 1;" +
                " UPDATE " + table + " SET IdUploadFile = @FileId" +
                " WHERE " + key + " = @RecordId;" +
                " IF @PreviousFileId IS NOT NULL AND @PreviousFileId <> @FileId" +
                " UPDATE dbo.TblUploadFile SET IsDeleted = 1" +
                " WHERE Id = @PreviousFileId AND RefId = @RecordId" +
                " AND RefType = @RefType;" +
                " COMMIT TRANSACTION; SELECT CAST(1 AS INT);" +
                " END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                TblUploadFile.Schema.Provider.Name);
            command.Parameters.Add("@FileId", fileId, DbType.Guid);
            command.Parameters.Add("@RecordId", refId, DbType.Guid);
            command.Parameters.Add("@RefType", refType, DbType.AnsiString);
            if (Convert.ToInt32(DataService.ExecuteScalar(command)) != 1)
                throw new InvalidOperationException(
                    "File không thuộc khoản chi phí/cuộc họp đang chỉnh sửa.");
        }
    }
}
