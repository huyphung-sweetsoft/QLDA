using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlMeet : BaseAdminUserControl
    {
        public EventHandler NewMeetingHandlerCallback;
        public EventHandler EditMeetingHandlerCallback;
        public EventHandler OpenMeetingFilesHandlerCallback;

        public Guid ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    if (this.Page is BaseAdminPage basePage && basePage.CurrentProjectId != Guid.Empty)
                        return basePage.CurrentProjectId;
                    if (Request.QueryString["ProjectId"] != null && Guid.TryParse(Request.QueryString["ProjectId"], out Guid qId))
                        return qId;
                    return Guid.Empty;
                }
                return (Guid)ViewState["ProjectId"];
            }
            set => ViewState["ProjectId"] = value;
        }

        protected bool IsView => this.CURRENT_PAGE.IsView;
        protected bool IsEdit => this.CURRENT_PAGE.IsEdit;
        protected bool IsDelete => this.CURRENT_PAGE.IsDelete;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            if (script != null)
            {
                script.RegisterAsyncPostBackControl(lbtSearchSingle);
                script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
                script.RegisterAsyncPostBackControl(lbtCancel);
            }
        }

        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;

            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);

            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "MaCuocHop";
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlSearch.Update();
            pnlButtons.Update();
        }

        private void AssignSearchColumns()
        {
            txtSearchTenCuocHop.SearchColumn = "TenCuocHop";
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenCuocHop.SearchTagItemText = "Tên cuộc họp";
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);

            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.MEETING_CODE),
                GetResourceText(BackEndResourceKeys.MEETING_NAME),
                "Người tham gia",
                GetResourceText(BackEndResourceKeys.START_TIME),
                GetResourceText(BackEndResourceKeys.END_TIME),
                GetResourceText(BackEndResourceKeys.MEETING_ROOM),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null)
                {
                    this.ShowInvalidDataError();
                    return;
                }

                int totalRows = 0;
                int rowIndex = (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;
                int pageSize = rowIndex + grid.CurrentPageSize;
                DataTable dt = null;

                if (grid.GridSearchType == GridSearchType.Single)
                {
                    dt = MeetManager.Instance.SearchMeeting(this.ProjectId, txtSearchSingle.Text, null, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                else
                {
                    Dictionary<string, object> keyValueSearchs = new ControlHelpers().GetControlValues(pnlSearchPopup);
                    dt = MeetManager.Instance.SearchMeeting(this.ProjectId, "", keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                }
                else
                {
                    ctrlGridviewPaging.Visible = true;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();
                    ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }

                upMain.Update();
                pnlButtons.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "VIEW_MEMBERS":
                    if (!this.CURRENT_PAGE.IsEdit && !this.CURRENT_PAGE.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndexMeet = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndexMeet = ((GridViewRow)((WebControl)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndexMeet = Convert.ToInt32(e.CommandArgument);

                    Guid idCuocHop = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexMeet].Value.ToString(), out idCuocHop))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    TblLichHop meet = TblLichHop.FetchByID(idCuocHop);
                    if (meet == null) return;

                    Guid? hostId = meet.IdNguoiTao;
                    DateTime startDate = meet.ThoiGianBatDau;
                    DateTime endDate = meet.ThoiGianKetThuc;

                    ((CtrlXemNhanVienMeet)CtrlXemNhanVienMeet1).OpenModal(idCuocHop, startDate, endDate, meet.TenCuocHop, hostId);
                    break;

                case "MEETING_FILES":
                    if (!this.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    Guid meetingIdForFiles;
                    if (!Guid.TryParse(
                        Convert.ToString(e.CommandArgument),
                        out meetingIdForFiles)
                        || meetingIdForFiles == Guid.Empty)
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (OpenMeetingFilesHandlerCallback != null)
                    {
                        OpenMeetingFilesHandlerCallback(
                            meetingIdForFiles,
                            EventArgs.Empty);
                    }

                    break;

                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit) { ShowAccessDeniedNotify(); return; }
                    int rowIndex = (e.CommandSource.GetType() != typeof(GridviewExtension)) ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    Guid meetId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out meetId)) { ShowInvalidDataError(); return; }
                    EditMeetingHandlerCallback?.Invoke(meetId, EventArgs.Empty);
                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete) { ShowAccessDeniedNotify(); return; }
                    int rowIndexDel = (e.CommandSource.GetType() != typeof(GridviewExtension)) ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    Guid meetIdDel = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexDel].Value.ToString(), out meetIdDel)) { ShowInvalidDataError(); return; }

                    TblLichHop meetDel = TblLichHop.FetchByID(meetIdDel);
                    if (meetDel == null || meetDel.DaXoa == true) { ShowInvalidNotFoundData(); return; }

                    ConfirmResult result = new ConfirmResult { CommandName = "MEETING_DELETE", Value = meetDel };
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION), $"Bạn có chắc chắn xóa cuộc họp: {meetDel.TenCuocHop}?", MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null && e.CommandName.Contains("MEETING_DELETE"))
            {
                TblLichHop meet = e.Value as TblLichHop;
                if (meet == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                try
                {
                    MeetManager.Instance.DeleteMeet(meet);
                    ShowSuccessDeleteData();
                    grvData.CurrentPageIndex = 1;
                    grvData.Rebind();
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master?.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }

        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                if (master != null)
                {
                    GridSearchType? searchType;
                    master.searchTagBox_TagClosed(searchTagBox, tag, null, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
                }
                upSearchTagBox.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc) { ShowNotify(exc.Message, MSGType.Error); }
        }

        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master?.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            if (pnlSearchPopup != null) new ControlHelpers().ClearControlValues(pnlSearchPopup.Controls);
            pnlSearch.Update();
            MasterTemplate master = Page.Master as MasterTemplate;
            master?.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd) { ShowAccessDeniedNotify(); return; }
            NewMeetingHandlerCallback?.Invoke(Guid.Empty, EventArgs.Empty);
        }
        protected string GetTrangThaiCuocHopText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            TrangThaiCuocHopEnum status = (TrangThaiCuocHopEnum)Convert.ToInt32(value);
            return GetResourceText(MeetManager.Instance.GetValueForTrangThaiCuoHop(status));
        }

        public string GetAssigneeDisplay(object tenNhanVienObj, object avatarsObj)
        {
            string names = tenNhanVienObj?.ToString() ?? "";
            string avatars = avatarsObj?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(names)) return "";

            string[] nameArray = names.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            string[] avatarArray = avatars.Split(new string[] { "," }, StringSplitOptions.None);

            string html = "";
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };

            int maxDisplay = 2;
            int count = nameArray.Length;

            for (int i = 0; i < Math.Min(count, maxDisplay); i++)
            {
                string name = nameArray[i].Trim();
                string avatar = (i < avatarArray.Length) ? avatarArray[i].Trim() : "";
                string color = colors[i % colors.Length];
                bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

                if (!isDefaultAvatar)
                {
                    string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                    string fallbackHtml = $"<div class=\\'avatar-circle\\' style=\\'background-color: {color};\\' title=\\'{name}\\'>{GetInitials(name)}</div>";
                    html += $"<img src='{avatarUrl}' class='avatar-circle' style='object-fit: cover;' title='{name}' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
                }
                else
                {
                    string initials = GetInitials(name);
                    html += $"<div class='avatar-circle' style='background-color: {color};' title='{name}'>{initials}</div>";
                }
            }

            if (count > maxDisplay)
            {
                html += $"<div class='avatar-circle avatar-more' title='Và {count - maxDisplay} người khác'>+{count - maxDisplay}</div>";
            }

            return html;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }
        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
            {
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefaultStatus, grvData, txtSearchSingle);
            }
            upSearchTagBox.Update();
            if (pnlSearchDropdowns != null) pnlSearchDropdowns.Update();
        }
    }
}
