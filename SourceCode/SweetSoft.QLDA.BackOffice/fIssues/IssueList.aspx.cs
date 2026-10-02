using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fIssues
{
    public partial class IssueList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Issue;
        private Guid IssueId
        {
            get
            {
                if (ViewState["IssueId"] != null)
                    return (Guid)ViewState["IssueId"];
                return Guid.Empty;
            }
            set => ViewState["IssueId"] = value;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlIssue1.NewIssueHandlerCallback += NewIssueAction;
            CtrlIssue1.EditIssueHandlerCallback += EditIssueAction;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.ISSUES_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.ISSUES_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.ISSUES_LIST) }
                };
                ApplyControlsText();
                CtrlIssue1.InitControls();

                Guid issueId;
                if (Guid.TryParse(Request.QueryString["issueId"], out issueId))
                {
                    TblVanDe issue = TblVanDe.FetchByID(issueId);
                    if (issue != null && issue.DaXoa != true
                        && issue.IdDuAn == CurrentProjectId)
                    {
                        EditIssueAction(issueId, EventArgs.Empty);
                    }
                }
            }
        }
        private void ApplyControlsText()
        {
            ddlCongViecBiAnhHuong.PlaceHolder =
            ddlMucDoAnhHuong.PlaceHolder =
            ddlCongViecPhatSinh.PlaceHolder =
            ddlNguonGocVanDe.PlaceHolder =
            ddlTrangThaiVanDe.PlaceHolder =
            GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);
            txtTenVanDe.PlaceHolder =
            txtMoTaChiTiet.PlaceHolder =
            txtKeHoachXuLy.PlaceHolder =
            GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
        }
        private void NewIssueAction(object sender, EventArgs e)
        {
            RefreshIssueInfo();
            ddlTrangThaiVanDe.Enabled = this.IsAdd;
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }
        private void EditIssueAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }
            Guid issueId = (Guid)sender;
            if (issueId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            RefreshIssueInfo();
            divTrangThaiVanDe.Visible = true;
            ddlTrangThaiVanDe.Enabled = this.IsEdit;
            lbtSubmit.Visible = this.IsEdit;
            TblVanDe issue = TblVanDe.FetchByID(issueId);
            if (issue == null || issue.DaXoa == true)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }
            this.IssueId = issue.IdVanDe;
            txtTenVanDe.Text = issue.TenVanDe;
            txtMoTaChiTiet.Text = issue.MoTaChiTiet;
            txtKeHoachXuLy.Text = issue.KeHoachXuLy;
            if (issue.IdCongViecPhatSinh != null)
                ddlCongViecPhatSinh.SelectedValue = issue.IdCongViecPhatSinh.ToString();
            if (issue.IdCongViecBiAnhHuong != null)
                ddlCongViecBiAnhHuong.SelectedValue = issue.IdCongViecBiAnhHuong.ToString();
            if (issue.MucDoAnhHuong != null)
                ddlMucDoAnhHuong.SelectedValue = issue.MucDoAnhHuong.ToString();
            if (issue.NguonGocVanDe != null)
                ddlNguonGocVanDe.SelectedValue = issue.NguonGocVanDe.ToString();
            if (ddlTrangThaiVanDe.Items.Count > 0)
                ddlTrangThaiVanDe.SelectedValue = issue.TrangThai.ToString();
            BindNhanVienXuLy(issue.IdCongViecBiAnhHuong);
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.EDIT);
            dlDetail.OpenModal(true, IsPostBack ? 0 : 1000);
        }
        private void RefreshIssueInfo()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindCongViecDuAn(ddlCongViecBiAnhHuong, CtrlIssue1.ProjectId);
            controlHelpers.BindCongViecDuAn(ddlCongViecPhatSinh, CtrlIssue1.ProjectId);
            controlHelpers.BindMucDoAnhHuong(ddlMucDoAnhHuong);
            controlHelpers.BindNguonGocVanDe(ddlNguonGocVanDe);
            controlHelpers.BindTrangThaiVanDe(ddlTrangThaiVanDe);
            lbtSubmit.Visible = false;
            divTrangThaiVanDe.Visible = true;
            ddlTrangThaiVanDe.Enabled = true;
            txtTenVanDe.Text = txtMoTaChiTiet.Text = txtKeHoachXuLy.Text = "";
            if (ddlCongViecBiAnhHuong.Items.Count > 0) ddlCongViecBiAnhHuong.SelectedIndex = 0;
            if (ddlCongViecPhatSinh.Items.Count > 0) ddlCongViecPhatSinh.SelectedIndex = 0;
            BindNhanVienXuLy(GetSelectedTaskId(ddlCongViecPhatSinh));
            if (ddlMucDoAnhHuong.Items.Count > 0) ddlMucDoAnhHuong.SelectedIndex = 0;
            if (ddlNguonGocVanDe.Items.Count > 0) ddlNguonGocVanDe.SelectedIndex = 0;
            if (ddlTrangThaiVanDe.Items.Count > 0) ddlTrangThaiVanDe.SelectedIndex = 0;
            this.IssueId = Guid.Empty;
        }
        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                TblVanDe issueDto = new TblVanDe();
                bool isNew = this.IssueId == Guid.Empty;
                if (!isNew)
                    issueDto.IdVanDe = this.IssueId;

                issueDto.IdDuAn = CtrlIssue1.ProjectId;
                issueDto.TenVanDe = txtTenVanDe.Text.Trim();
                issueDto.MoTaChiTiet = !string.IsNullOrEmpty(txtMoTaChiTiet.Text.Trim()) ? txtMoTaChiTiet.Text.Trim() : null;
                issueDto.KeHoachXuLy = !string.IsNullOrEmpty(txtKeHoachXuLy.Text.Trim()) ? txtKeHoachXuLy.Text.Trim() : null;

                Guid idCongViecBiAnhHuong = Guid.Empty;
                if (GetValue(ddlCongViecBiAnhHuong, out idCongViecBiAnhHuong) && idCongViecBiAnhHuong != Guid.Empty)
                    issueDto.IdCongViecBiAnhHuong = idCongViecBiAnhHuong;

                Guid idCongViecPhatSinh = Guid.Empty;
                if (GetValue(ddlCongViecPhatSinh, out idCongViecPhatSinh) && idCongViecPhatSinh != Guid.Empty)
                    issueDto.IdCongViecPhatSinh = idCongViecPhatSinh;

                int mucDoAnhHuong = 0;
                if (GetValue(ddlMucDoAnhHuong, out mucDoAnhHuong) && mucDoAnhHuong > 0)
                    issueDto.MucDoAnhHuong = mucDoAnhHuong;

                int nguonGoc = 0;
                if (GetValue(ddlNguonGocVanDe, out nguonGoc))
                    issueDto.NguonGocVanDe = nguonGoc;

                if (byte.TryParse(ddlTrangThaiVanDe.SelectedValue, out byte trangThaiVanDe))
                {
                    issueDto.TrangThai = trangThaiVanDe;
                }
                else
                {
                    issueDto.TrangThai = 0;
                }

                TblVanDe savedIssue = IssueManager.Instance.CreateOrUpdate(issueDto);
                if (savedIssue == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                if (!isNew)
                {
                    savedIssue.TrangThai = issueDto.TrangThai;
                    savedIssue.NgayCapNhat = DateTime.Now;
                    savedIssue.Save();
                }

                ShowSuccessSaveData();
                dlDetail.CloseModal();
                CtrlIssue1.Rebind();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlIssue1.ConfirmRequest(e);
        }
        protected void ddlCongViecBiAnhHuong_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindNhanVienXuLy(GetSelectedTaskId(ddlCongViecBiAnhHuong));
        }
        private Guid? GetSelectedTaskId(ExtraDropdown dropdown)
        {
            if (dropdown == null || string.IsNullOrWhiteSpace(dropdown.SelectedValue) || dropdown.SelectedValue == "null")
                return null;
            return Guid.TryParse(dropdown.SelectedValue, out Guid taskId) && taskId != Guid.Empty ? (Guid?)taskId : null;
        }
        private void BindNhanVienXuLy(Guid? taskId)
        {
            List<object> result = new List<object>();
            if (taskId.HasValue && taskId.Value != Guid.Empty)
            {
                List<TblCongViecNhanVien> assignments = new Select()
                    .From(TblCongViecNhanVien.Schema)
                    .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(taskId.Value)
                    .ExecuteTypedList<TblCongViecNhanVien>();
                List<Guid> assignedIds = assignments
                    .Select(x => x.IdNhanVien)
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();
                List<AspnetUser> assignedUsers = new List<AspnetUser>();
                foreach (Guid userId in assignedIds)
                {
                    AspnetUser user = UserManager.Instance.GetUserById(userId);
                    if (user != null)
                        assignedUsers.Add(user);
                }
                assignedUsers = assignedUsers
                    .OrderBy(x => string.IsNullOrWhiteSpace(x.DisplayName) ? x.UserName : x.DisplayName)
                    .ThenBy(x => x.UserName)
                    .ToList();
                for (int i = 0; i < assignedUsers.Count; i++)
                {
                    AspnetUser user = assignedUsers[i];
                    string displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;
                    string email = GetUserEmail(user.UserName);
                    result.Add(new
                    {
                        DisplayName = HttpUtility.HtmlEncode(displayName),
                        Email = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(email) ? "Chưa cập nhật email" : email),
                        AvatarHtml = GetAvatarHtml(displayName, user.Avatar, i)
                    });
                }
            }
            rptNhanVienXuLy.DataSource = result;
            rptNhanVienXuLy.DataBind();
            pnlNoNhanVienXuLy.Visible = result.Count == 0;
        }
        private string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "";
            try
            {
                MembershipUser user = Membership.GetUser(userName);
                return user != null ? user.Email : "";
            }
            catch
            {
                return "";
            }
        }
        private string GetAvatarHtml(string displayName, string avatar, int index)
        {
            string[] colors = { "#7c3aed", "#2563eb", "#059669", "#d97706", "#db2777" };
            string color = colors[index % colors.Length];
            string safeName = HttpUtility.HtmlAttributeEncode(displayName ?? "");
            string initials = HttpUtility.HtmlEncode(GetInitials(displayName));
            bool isDefaultAvatar = string.IsNullOrWhiteSpace(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~", StringComparison.Ordinal) ? Page.ResolveUrl(avatar) : avatar;
                avatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                string fallbackHtml = $"<div class='issue-edit-person-avatar' style='background:{color};'>{initials}</div>";
                return $"<img src='{avatarUrl}' class='issue-edit-person-avatar' alt='{safeName}' title='{safeName}' onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }
            return $"<div class='issue-edit-person-avatar' style='background:{color};' title='{safeName}'>{initials}</div>";
        }
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";
            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }
    }
}
