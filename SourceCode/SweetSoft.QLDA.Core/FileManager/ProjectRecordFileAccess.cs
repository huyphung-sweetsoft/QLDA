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

        public static bool BelongsToRecord(Guid refId, string refType, Guid fileId)
        {
            if (refId == Guid.Empty || fileId == Guid.Empty || !IsRecordAttachment(refType))
                return false;
            TblUploadFile file = TblUploadFile.FetchByID(fileId);
            return file != null && file.RefId == refId && file.RefType == refType;
        }
    }
}
