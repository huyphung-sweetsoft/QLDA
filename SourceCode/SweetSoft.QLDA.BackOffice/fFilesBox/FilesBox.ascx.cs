//---------------------- PROGRAMMER LOG ---------------------------------------
//Change 01: Truong, 29 Oct 2024 - Fix bug
using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fFilesBox
{
    public partial class FilesBox : BaseAdminUserControl
    {
        #region Script + Styles
        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>();
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Styles/plugins/lightbox-evolution/theme/default/jquery.lightbox.css"));
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/fFilesBox/FilesBox.css"));
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/fFilesBox/FileBoxViewer.css"));
                List<string> jsLinks = new List<string>();
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Styles/plugins/lightbox-evolution/js/jquery.lightbox.1.8.min.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/fFilesBox/isotope.pkgd.min.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/fFilesBox/Sortable.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/fFilesBox/FilesBox.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/fFilesBox/FileBoxViewer.js"));
                return new RegisterCSSAndJS("cpHeadVendor", "cpVendorScript", cssLinks, jsLinks);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }
        #endregion
        public string SingleFilePath
        {
            get
            {
                return (string)ViewState["SingleFilePath"];
            }
            set
            {
                ViewState["SingleFilePath"] = value;
            }
        }
        public string SingleFilePathType
        {
            get
            {
                string type = (string)ViewState["SingleFilePathType"];
                if (string.IsNullOrEmpty(type))
                    type = FileTypes.External;
                return type;
            }
            set
            {
                ViewState["SingleFilePathType"] = value;
            }
        }
        public string DefaultSingleFilePath
        {
            get
            {
                return (string)ViewState["DefaultSingleFilePath"];
            }
            set
            {
                ViewState["DefaultSingleFilePath"] = value;
            }
        }
        public string DefaultSingleFilePathType
        {
            get
            {
                string type = (string)ViewState["DefaultSingleFilePathType"];
                if (string.IsNullOrEmpty(type))
                    type = FileTypes.External;
                return type;
            }
            set
            {
                ViewState["DefaultSingleFilePathType"] = value;
            }
        }
        public bool IsEnabled
        {
            get
            {
                try
                {
                    if (ViewState["IsEnabled"] != null)
                        return (bool)ViewState["IsEnabled"];
                    return true;
                }
                catch
                {
                    return true;
                }
            }
            set
            {
                ViewState["IsEnabled"] = value;
            }
        }
        public bool IsMultiple
        {
            get
            {
                try
                {
                    if (ViewState["IsMultiple"] != null)
                        return (bool)ViewState["IsMultiple"];
                    return false;
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                ViewState["IsMultiple"] = value;
            }
        }
        public string AcceptType
        {
            get
            {
                string acceptType = (string)ViewState["AcceptType"];
                if (string.IsNullOrEmpty(acceptType))
                {
                    acceptType = string.Join(",",
                "image/png",
                "image/gif",
                "image/jpeg",
                "image/jpg",
                "image/webp",
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );
                }    

                return acceptType;
            }
            set
            {
                ViewState["AcceptType"] = value;
            }
        }
        public bool IsFirstUpload
        {
            get
            {
                foreach (string key in Request.Params.AllKeys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    if (key.StartsWith(this.ClientID + "fileTitle$") || key.StartsWith(this.ClientID + "fileOrder$"))
                        return false;
                }
                return true;
            }
        }
        private Guid? RefId
        {
            get
            {
                try
                {
                    return (Guid)ViewState["RefId"];//'**Change 02
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                ViewState["RefId"] = value;
            }
        }
        private FileUploadTypes? _refType
        {
            get
            {
                if (ViewState["RefType"] == null)
                    return null;

                return (FileUploadTypes)ViewState["RefType"];
            }
            set
            {
                ViewState["RefType"] = value;
            }
        }
        static string[] mediaExtensions = new string[] { ".MP3", ".AVI", ".MP4" };
        static bool IsMediaFile(string path)
        {
            return -1 != Array.IndexOf(mediaExtensions, Path.GetExtension(path).ToUpperInvariant());
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (IsPostBack)
                    return;
                divControls.Visible = IsEnabled;
                btnDiscardFile.Title = btnDiscardFile.InnerText = GetResourceText(BackEndResourceKeys.CANCEL);
                btnApplyFile.Title = btnApplyFile.InnerText = GetResourceText(BackEndResourceKeys.SAVE_CHANGES);
                ScriptManager.RegisterStartupScript(this.Page, GetType(), "HtmlFormatFile"
                    , string.Format("$(document).ready(function () {{ if (FilesBox.HtmlFormatFile === '') FilesBox.HtmlFormatFile = '{0}'; }});"
                        , HttpUtility.JavaScriptStringEncode(htmlFormatFile.InnerHtml))
                    , true);
            }
            catch (Exception exc)
            {
                throw new Exception("FilesBox", exc);
            }
        }
        protected void btnDiscardFile_ServerClick(object sender, EventArgs e)
        {
            try
            {
                if (this.RefId == null || this._refType == null)
                {
                    this.CURRENT_PAGE.ShowInvalidDataError();
                    return;
                }
                LoadFile(this.RefId.Value, this._refType.Value);//'**Change 02
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "FilesBox.DiscardFile", "FilesBox.DiscardFile();", true);
            }
            catch (Exception exc)
            {
                throw new Exception("FilesBox", exc);
            }
        }
        protected void btnApplyFile_ServerClick(object sender, EventArgs e)
        {
            try
            {
                if (this.RefId == null || this._refType == null)
                {
                    this.CURRENT_PAGE.ShowInvalidDataError();
                    return;
                }
                //---------------------------------------------
                #region delete file
                List<Guid> listFileRemoveId = new List<Guid>();
                foreach (string fileRemoveId in txtArFileRemove.Value.Split(','))
                {
                    if (string.IsNullOrEmpty(fileRemoveId))
                        continue;
                    Guid tmpId = Guid.Empty;
                    if(Guid.TryParse(fileRemoveId, out tmpId) && tmpId != Guid.Empty)
                        listFileRemoveId.Add(tmpId);
                }
                if(listFileRemoveId.Count > 0)
                    UploadManager.Instance.RemoveFiles(listFileRemoveId, this._refType.Value);
                #endregion

                #region permissions
                List<FilePermission> filePermissions = new List<FilePermission>();
                try
                {
                    filePermissions = JsonConvert.DeserializeObject<List<FilePermission>>(hdfFilePermission.Value);
                }
                catch
                {
                    filePermissions = new List<FilePermission>();
                }
                #endregion

                #region update title and order
                var fileUpdates = new Dictionary<Guid, (string Title, int? Order, string Path)>();

                foreach (string key in Request.Params.AllKeys)
                {
                    if (string.IsNullOrEmpty(key)) continue;

                    if (key.StartsWith(this.ClientID + "fileTitle$"))
                    {
                        string fileId = key.Replace(this.ClientID + "fileTitle$", "");
                        if (Guid.TryParse(fileId, out var gFileId))
                        {
                            if (!fileUpdates.ContainsKey(gFileId)) fileUpdates[gFileId] = (null, null, null);
                            fileUpdates[gFileId] = (Request.Params[key], fileUpdates[gFileId].Order, fileUpdates[gFileId].Path);
                        }
                    }
                    else if (key.StartsWith(this.ClientID + "fileOrder$"))
                    {
                        string fileId = key.Replace(this.ClientID + "fileOrder$", "");
                        if (Guid.TryParse(fileId, out var gFileId))
                        {
                            if (!fileUpdates.ContainsKey(gFileId)) fileUpdates[gFileId] = (null, null, null);
                            fileUpdates[gFileId] = (fileUpdates[gFileId].Title, int.Parse(Request.Params[key]), fileUpdates[gFileId].Path);
                        }
                    }
                    else if (key.StartsWith(this.ClientID + "filePath$"))
                    {
                        string fileId = key.Replace(this.ClientID + "filePath$", "").Replace("|New", "");
                        if (Guid.TryParse(fileId, out var gFileId))
                        {
                            if (!fileUpdates.ContainsKey(gFileId)) fileUpdates[gFileId] = (null, null, null);
                            fileUpdates[gFileId] = (fileUpdates[gFileId].Title, fileUpdates[gFileId].Order, Request.Params[key]);
                        }
                    }
                }
                bool isNoFile = true;
                var appContext = SweetContext.Current;
                foreach (var kvp in fileUpdates)
                {
                    var gFileId = kvp.Key;
                    var (title, order, path) = kvp.Value;

                    var uploadFile = new UploadManager(appContext, gFileId);
                    if (uploadFile?.File == null)
                    {
                        this.CURRENT_PAGE.ShowInvalidDataError();
                        return;
                    }
                    isNoFile = false;
                    if (!string.IsNullOrEmpty(title))
                        uploadFile.File.Name = title;

                    if (order.HasValue && uploadFile.File.DisplayOrder != order.Value)
                        uploadFile.File.DisplayOrder = order.Value;

                    if (!string.IsNullOrEmpty(path) && uploadFile.File.FileUrl != path)
                    {
                        uploadFile.File.FileUrl = path;
                        uploadFile.File.Ext = Path.GetExtension(uploadFile.File.FileUrl) ?? string.Empty;
                    }

                    // Gắn quyền
                    var filePer = filePermissions.FirstOrDefault(t => t.Id == uploadFile.File.Id);
                    if (filePer != null)
                    {
                        uploadFile.File.IsHost = filePer.IsHost;
                        uploadFile.File.IsSecretary = filePer.IsSecretary;
                        uploadFile.File.IsParticipant = filePer.IsParticipant;
                    }

                    string fieldError, msgError;
                    if (!uploadFile.Save(out fieldError, out msgError))
                    {
                        this.CURRENT_PAGE.ShowInvalidDataError();
                        return;
                    }
                }

                #endregion

                //if (this.IsMultiple)
                this.CURRENT_PAGE.ShowSuccessSaveData();
                if (!this.IsMultiple)
                {
                    if (isNoFile)
                    {
                        this.SingleFilePath = this.DefaultSingleFilePath;
                        this.SingleFilePathType = this.DefaultSingleFilePathType;
                    }
                }
                LoadFile(this.RefId.Value, this._refType.Value);//'**Change 02
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "FilesBox.DiscardFile", "FilesBox.DiscardFile();", true);

                DataCallback(this.SaveDataCallbackKey, null, null);
            }
            catch (Exception exc)
            {
                throw new Exception("FilesBox", exc);
            }
        }
        public string SaveDataCallbackKey
        {
            get
            {
                return (string)ViewState["SaveDataCallbackKey"];
            }
            set
            {
                ViewState["SaveDataCallbackKey"] = value;
            }
        }
        public void LoadFile(Guid refId, FileUploadTypes refType)
        {
            this.RefId = refId;
            this._refType = refType;

            btnApplyFile.Attributes.Add("onclick", string.Format("return FilesBox.SaveFile('{0}','{1}');", refType, refId));

            ltrCurrentFiles.Text = "";

            UploadManager fileManager = new UploadManager(SweetContext.Current, refId, refType);

            divControls.Visible = IsEnabled;
            if (fileManager.TblUploadFiles == null || fileManager.TblUploadFiles.Count == 0)
            {
                if (!this.IsMultiple)
                {
                    fileManager.TblUploadFiles = new List<TblUploadFile>();

                    if (string.IsNullOrEmpty(this.SingleFilePath))
                    {
                        if (string.IsNullOrEmpty(this.DefaultSingleFilePath))
                        {
                            fileManager.TblUploadFiles.Add(new TblUploadFile
                            {
                                FileUrl = "/styles/images/no-file.png",
                                FileType = FileTypes.Internal
                            });
                        }
                        else
                        {
                            fileManager.TblUploadFiles.Add(new TblUploadFile
                            {
                                FileUrl = this.DefaultSingleFilePath,
                                FileType = this.DefaultSingleFilePathType
                            });
                        }
                    }
                    else
                    {
                        fileManager.TblUploadFiles.Add(new TblUploadFile
                        {
                            FileUrl = this.SingleFilePath,
                            FileType = this.SingleFilePathType
                        });
                    }
                }
                else
                {
                    upListFile.Update();
                    return;
                }
            }

            string listCurrentFile = string.Empty;
            int index = 0;
            foreach (TblUploadFile file in fileManager.TblUploadFiles)
            {
                index++;
                if (!this.IsMultiple && index > 1)
                {
                    if (string.IsNullOrEmpty(txtArFileRemove.Value))
                        txtArFileRemove.Value = file.Id.ToString();
                    else
                        txtArFileRemove.Value += "," + file.Id.ToString();
                    continue;
                }

                if (!this.IsMultiple)
                {
                    this.SingleFilePath = file.FileUrl;
                    this.SingleFilePathType = file.FileType;
                }

                string fileTitle;
                if (file.Id == Guid.Empty)
                    fileTitle = "no-file";
                else
                    fileTitle = file.Name;
                string fileSrc = string.Empty;
                //string filePath = file.GetHiddenFileUrl(this.CURRENT_PAGE.GetRelativeClientPath(""));
                if (IsMediaFile(file.FileUrl))
                {
                    fileSrc = this.CURRENT_PAGE.GetRelativeClientPath("/styles/images/video-thumbnail.jpg");
                }
                else if (file.FileUrl.EndsWith(".pdf"))
                {
                    fileSrc = this.CURRENT_PAGE.GetRelativeClientPath("/styles/images/pdf-thumbnail.png");
                }
                else if (file.FileUrl.EndsWith(".doc") || file.FileUrl.EndsWith(".docx"))
                {
                    fileSrc = this.CURRENT_PAGE.GetRelativeClientPath("/styles/images/doc-thumbnail.jpg");
                }
                else if (file.FileUrl.EndsWith(".xlsx") || file.FileUrl.EndsWith(".xls"))
                {
                    fileSrc = this.CURRENT_PAGE.GetRelativeClientPath("/styles/images/excel-thumbnail.jpg");
                }
                else
                    fileSrc = file.FileUrl;

                listCurrentFile += string.Format(htmlFormatFile.InnerHtml
                    , fileSrc
                    , fileTitle
                    , string.Empty
                    , string.Format("{0}fileTitle${1}", this.ClientID, file.Id)
                    , file.DisplayOrder
                    , string.Empty
                    , string.Format("{0}fileOrder${1}", this.ClientID, file.Id)
                    , file.Id
                    , string.Format("{0}filePath${1}", this.ClientID, file.Id)
                    , SecurityUtilities.ProtectUrlParameter(string.Format("/Upload/{0}/{1}"
                        , this._refType, this.RefId))
                    , string.Format("{0}filePath_{1}", this.ClientID, file.Id)
                    , file.FileUrl
                    , IsEnabled ? string.Empty : "d-none hidden"
                    , file.IsHost ? "checked" : ""
                    , file.IsSecretary ? "checked" : ""
                    , file.IsParticipant ? "checked" : "");
            }

            ltrCurrentFiles.Text = listCurrentFile;
            upListFile.Update();
        }
        public void ClearData()
        {
            ltrCurrentFiles.Text = string.Empty;
            upListFile.Update();
        }

        private class FilePermission
        {
            public Guid Id { get; set; }
            public bool IsHost { get; set; }
            public bool IsParticipant { get; set; }
            public bool IsSecretary { get;set; }
        }
    }
}