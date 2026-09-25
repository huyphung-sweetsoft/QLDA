using SubSonic;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
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

namespace SweetSoft.QLDA.Core.FileManager
{
    public class UploadManager : BaseManager
    {
        private static bool IsDocumentFile(TblUploadFile file)
        {
            return file!=null && (file.RefType=="DocumentVersion" || file.RefType=="DocumentSigningResult");
        }
        private void EnsureDocumentFileWrite(TblUploadFile file)
        {
            if(file==null) return;
            var old=_repository.GetById(file.Id);
            if (old != null && ProjectRecordFileAccess.IsRecordAttachment(old.RefType)
                && (file.RefId != old.RefId || file.RefType != old.RefType
                    || file.FileUrl != old.FileUrl || file.IsDeleted != old.IsDeleted))
                throw new InvalidOperationException("Không được thay liên kết của file chi phí/lịch họp.");
            if (ProjectRecordFileAccess.IsRecordAttachment(file.RefType))
            {
                if (!ProjectRecordFileAccess.CanAccess(
                    SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.UserId,
                    file.RefId, file.RefType, true))
                    throw new UnauthorizedAccessException("Bạn không có quyền cập nhật file chi phí/lịch họp.");
                return;
            }
            if (old != null && ProjectRecordFileAccess.IsRecordAttachment(old.RefType))
                throw new UnauthorizedAccessException("Không được đổi loại file chi phí/lịch họp.");
            if(IsDocumentFile(old) && (file.RefId!=old.RefId || file.RefType!=old.RefType || file.FileUrl!=old.FileUrl || file.IsDeleted!=old.IsDeleted))
                throw new InvalidOperationException("Không được thay liên kết hoặc xóa file lịch sử hồ sơ.");
            if(!IsDocumentFile(file)) return;
            var repository=new DocumentRepository(null);
            Guid currentUserId = SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.UserId;
            bool allowed = file.RefType == "DocumentSigningResult"
                ? repository.CanProcessSigningResult(currentUserId, file.RefId)
                : repository.ResolveUploadDocument(file.RefId,file.RefType).HasValue
                    && repository.CanAccess(currentUserId,file.RefId,
                        DocumentPermissionKeys.ManageFiles);
            if (!allowed)
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật file hồ sơ.");
        }
        private void EnsureDocumentFileDelete(TblUploadFile file)
        {
            if (file != null && ProjectRecordFileAccess.IsRecordAttachment(file.RefType))
            {
                if (!ProjectRecordFileAccess.CanAccess(
                    SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.UserId,
                    file.RefId, file.RefType, true))
                    throw new UnauthorizedAccessException("Bạn không có quyền xóa file chi phí/lịch họp.");
                return;
            }
            if (!IsDocumentFile(file))
                return;

            var repository = new DocumentRepository(null);
            Guid currentUserId = SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.UserId;
            Guid? documentId = repository.ResolveUploadDocument(
                file.RefId,
                file.RefType);
            bool allowed = file.RefType == "DocumentSigningResult"
                ? repository.CanProcessSigningResult(currentUserId, file.RefId)
                : documentId.HasValue && repository.CanAccess(
                    currentUserId, documentId.Value,
                    DocumentPermissionKeys.ManageFiles);
            if (!allowed)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền cập nhật file hồ sơ.");
            }

            // A file that is part of a saved version, official file, signing
            // result, customer return, template, or historical snapshot must
            // be removed through the dossier workflow.  Unreferenced files
            // are allowed here so a failed/staged upload can be cleaned up.
            string fileId = file.Id.ToString("D");
            string sql = $@"
                SELECT CASE WHEN EXISTS
                (
                    SELECT 1 FROM dbo.TblPhienBanTaiLieu
                    WHERE IdFileNoiDung = '{fileId}'
                       OR ISNULL(DanhSachFileJson, N'') LIKE '%{fileId}%'
                    UNION ALL
                    SELECT 1 FROM dbo.TblTaiLieu
                    WHERE IdFileBanChinhThuc = '{fileId}'
                    UNION ALL
                    SELECT 1 FROM dbo.TblTrinhKyTaiLieu
                    WHERE IdFileSauKy = '{fileId}'
                    UNION ALL
                    SELECT 1 FROM dbo.TblTrinhKyTaiLieuFile
                    WHERE IdFileSauKy = '{fileId}'
                    UNION ALL
                    SELECT 1 FROM dbo.TblGuiNhanKhachHang
                    WHERE IdFileNhanLai = '{fileId}'
                    UNION ALL
                    SELECT 1 FROM dbo.TblMauTaiLieu
                    WHERE IdFileMau = '{fileId}'
                ) THEN 1 ELSE 0 END;";
            if (new InlineQuery().ExecuteScalar<int>(sql) == 1)
            {
                throw new InvalidOperationException(
                    "Hãy gỡ file trong hồ sơ. Không xóa trực tiếp file lịch sử.");
            }
        }
        private static string Namespace
        {
            get
            {
                return typeof(UploadManager).Namespace;
            }
        }
        private static readonly Lazy<UploadManager> _instance = new Lazy<UploadManager>(() => new UploadManager());
        public static UploadManager Instance => _instance.Value;
        private readonly FileRepository _repository;
        private readonly AuditManager _auditManager;
        private byte? _langId { get; set; } = null;
        public byte LangId
        {
            get
            {
                if (this._langId != null)
                    return this._langId.Value;

                this._langId = LanguageHelpers.GetLanguageCodeByCultureName(CultureInfo.CurrentCulture.Name);
                return this._langId.Value;
            }
            set
            {
                this._langId = value;
            }
        }
        private TblUploadFile _tblUploadFile;
        private Guid? _refId { get; set; } = null;
        private FileUploadTypes? _refType { get; set; } = null;
        private List<TblUploadFile> _tblUploadFiles { get; set; } = null;
        public List<TblUploadFile> TblUploadFiles
        {
            get
            {
                if (_refId == null || _refType == null)
                    return null;
                if (_tblUploadFiles != null)
                    return _tblUploadFiles;
                _tblUploadFiles = _repository.GetListFileByRefId(_refId.Value, _refType.Value);
                return _tblUploadFiles;
            }
            set
            {
                _tblUploadFiles = value;
            }
        }
        public TblUploadFile File
        {
            get
            {
                return _tblUploadFile;
            }
            set
            {
                _tblUploadFile = value;
            }
        }
        public UploadManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new FileRepository(_auditManager);
        }
        public UploadManager(IAppContext applicationContext, Guid fileId) : base (applicationContext) 
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new FileRepository(_auditManager);
            this.File = _repository.GetById(fileId);
        }
        public UploadManager(IAppContext applicationContext, Guid refId, FileUploadTypes refType) : base (applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new FileRepository(_auditManager);
            this._refId = refId;
            this._refType = refType;
        }
        public UploadManager(IAppContext applicationContext, Guid refId, FileUploadTypes refType, string name, string fileUrl, string fileType, string ext, int fileSize, string originalFileName,  int displayOrder,
            Guid ownerId) : base(applicationContext) 
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new FileRepository(_auditManager);
            this.File = _repository.GetFileByParams(refId, refType, name); ;
            if (this.File == null)
            {
                this.File = new TblUploadFile();
                this.File.RefId = refId;
                this.File.RefType = refType.ToString();
                this.File.Name = name;
                this.File.IsHost = true;
                this.File.IsSecretary = true;
                this.File.IsParticipant = true;
            }
            this.File.FileUrl = fileUrl;
            this.File.FileType = fileType;
            this.File.Ext = ext;
            this.File.DisplayOrder = displayOrder;
            this.File.FileSize = fileSize;
            this.File.OriginalFileName = originalFileName;
            this.File.IsDeleted = false;
            this.File.OwnerId = ownerId;
            this.File.CreatedDate = DateTime.UtcNow;
        }
        public bool Save(out string errorField, out string errorMess)
        {
            EnsureDocumentFileWrite(this.File);
            errorField = "";
            errorMess = "";
            #region vadid input
            //-------------------------------------------------
            if (this.File.RefId == Guid.Empty)
            {
                errorField = nameof(this.File.RefId);
                errorMess = LanguageHelpers.GetResourceText(this.LangId, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
                return false;
            }
            //------------------------------------------------
            if (string.IsNullOrEmpty(this.File.RefType))
            {
                errorField = nameof(this.File.RefType);
                errorMess = LanguageHelpers.GetResourceText(this.LangId, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
                return false;
            }
            //------------------------------------------------
            if (string.IsNullOrEmpty(this.File.Name))
            {
                errorField = nameof(this.File.Name);
                errorMess = LanguageHelpers.GetResourceText(this.LangId, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
                return false;
            }
            //------------------------------------------------
            if (string.IsNullOrEmpty(this.File.FileUrl))
            {
                errorField = nameof(this.File.FileUrl);
                errorMess = LanguageHelpers.GetResourceText(this.LangId, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
                return false;
            }
            //------------------------------------------------
            if (string.IsNullOrEmpty(this.File.FileType))
            {
                errorField = nameof(this.File.FileType);
                errorMess = LanguageHelpers.GetResourceText(this.LangId, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
                return false;
            }
            //------------------------------------------------
            //if (string.IsNullOrEmpty(this.File.Ext))
            //{
            //    errorField = nameof(this.File.Ext);
            //    errorMess = LanguageHelpers.GetResourceText(this.LangId, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
            //    return false;
            //}
            #endregion
            if (this.File.IsNew)
                this.File = _repository.Insert(this.File);
            else
                this.File = _repository.Update(this.File);
            return true;
        }
        public TblUploadFile Create(TblUploadFile item)
        {
            if (item == null)
                return null;
            EnsureDocumentFileWrite(item);
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, nameof(TblUploadFile), item.Id).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblUploadFile");
                }
            });
            return item;
        }
        public string RemoveFiles(List<Guid> fileIDs, FileUploadTypes uploadTypes)
        {
            foreach(Guid id in fileIDs)
            {
                // Deletion may complete after a replacement has already
                // marked the previous upload inactive. Validate its original
                // type even when it is no longer shown in active file lists.
                TblUploadFile file = TblUploadFile.FetchByID(id);
                if (file == null || file.RefType != uploadTypes.ToString())
                    throw new InvalidOperationException("File không thuộc mục đang chỉnh sửa.");
                EnsureDocumentFileDelete(file);
            }
            string filePaths = _repository.GetFilePaths(fileIDs, uploadTypes);
            if(!string.IsNullOrEmpty(filePaths))
            {
                string[] filePathsArray = filePaths.Split('|');
                if(filePathsArray.Length > 0)
                {
                    foreach (var item in filePathsArray)
                    {
                        if (string.IsNullOrEmpty(item))
                            continue;
                        try
                        {
                            string fullPath = System.Web.Hosting.HostingEnvironment.MapPath(item);
                            if (System.IO.File.Exists(fullPath))
                                System.IO.File.Delete(fullPath);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            return _repository.RemoveFiles(fileIDs);
        }
        public bool DeleteFile(out string errorField, out string errorMess)
        {
            EnsureDocumentFileDelete(this.File);
            errorField = "";
            errorMess = "";
            if (this.File == null || this.File.IsDeleted)
            {
                errorField = nameof(this.File);
                errorMess = LanguageHelpers.GetResourceText(this.LangId, BackEndResourceKeys.DATA_NOT_FOUND);
                return false;
            }
            string sql = $"DELETE TblUploadFile WHERE Id = '{this.File.Id}'";
            new InlineQuery().Execute(sql);
            // Remove physical file
            try
            {
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath(this.File.FileUrl);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                // Log audit
                Task.Run(async () =>
                {
                    try
                    {
                        await _auditManager.LogActionAsync(LogActions.Actions.DELETE, this.File, nameof(TblUploadFile), this.File.Id).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        SysLogger.LogError(ex, "Failed to log DELETE action for TblUploadFile");
                    }
                });
            }
            catch
            {
            }
            return true;
        }
        public TblUploadFile GetUploadFileByRefIdAndRefType(Guid refId, FileUploadTypes refType)
        {
            return _repository.GetListFileByRefIdAndRefType(refId, refType);
        }
        public DataTable SearchPaging(Guid ownerId, string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(ownerId, searchTerm, orderBy, pageNumber, pageSize, out totalRecord);
        }
    }
}
