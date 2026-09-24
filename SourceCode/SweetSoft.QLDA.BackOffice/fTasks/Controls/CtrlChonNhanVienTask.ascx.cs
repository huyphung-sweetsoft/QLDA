using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security; // Kéo thư viện bảo mật vào để dùng ActionKeys
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.ScheduleManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlChonNhanVienTask : BaseAdminUserControl
    {
        public event EventHandler OnAssignConfirmed;

        private const string KEY_LA_NHAN_VIEN = "LaNhanVien";
        private const int MAX_ROWS = 10000;

        private Guid IdDuAn
        {
            get => ViewState["IdDuAn"] != null ? (Guid)ViewState["IdDuAn"] : Guid.Empty;
            set => ViewState["IdDuAn"] = value;
        }

        private Guid IdCongViec
        {
            get => ViewState["IdCongViec"] != null ? (Guid)ViewState["IdCongViec"] : Guid.Empty;
            set => ViewState["IdCongViec"] = value;
        }

        private DateTime StartDate
        {
            get => ViewState["StartDate"] != null ? (DateTime)ViewState["StartDate"] : DateTime.Today;
            set => ViewState["StartDate"] = value;
        }

        private DateTime EndDate
        {
            get => ViewState["EndDate"] != null ? (DateTime)ViewState["EndDate"] : DateTime.Today;
            set => ViewState["EndDate"] = value;
        }

        private Guid? IdNhanVienQuanLy
        {
            get => ViewState["IdNhanVienQuanLy"] as Guid?;
            set => ViewState["IdNhanVienQuanLy"] = value;
        }

        public bool ViewOnly
        {
            get
            {
                return ViewState["ViewOnly"] != null && (bool)ViewState["ViewOnly"];
            }
            set
            {
                ViewState["ViewOnly"] = value;
            }
        }

        private class ListCtx
        {
            public bool IsProject;
            public string Key;
            public Repeater Rpt;
            public Literal LtrCount;
            public Panel PnlNoData;
            public Literal LtrNoData;
            public SweetSoft.QLDA.Controls.BootstrapDropdown Ddl;
            public Panel PnlSearch;
            public SweetSoft.QLDA.Controls.ExtraTextBox Txt;
            public Control BtnSearch;
            public SweetSoft.QLDA.Controls.ExtraSearchBox TagBox;
            public UpdatePanel UpSearch, UpTag, UpList, UpCount;
        }

        private ListCtx GetCtx(bool isProject)
        {
            if (isProject)
            {
                return new ListCtx
                {
                    IsProject = true,
                    Key = "Proj",
                    Rpt = rptProjectMembers,
                    LtrCount = ltrCountProj,
                    PnlNoData = pnlNoDataProj,
                    LtrNoData = ltrNoDataProj,
                    Ddl = ddlChucDanhProj,
                    PnlSearch = pnlSearchProj,
                    Txt = txtSearchProj,
                    BtnSearch = lbtSearchProj,
                    TagBox = searchTagBoxProj,
                    UpSearch = upnlSearchProj,
                    UpTag = upSearchTagProj,
                    UpList = upListProj,
                    UpCount = upCountProj
                };
            }
            return new ListCtx
            {
                IsProject = false,
                Key = "Company",
                Rpt = rptCompanyMembers,
                LtrCount = ltrCountCompany,
                PnlNoData = pnlNoDataCompany,
                LtrNoData = ltrNoDataCompany,
                Ddl = ddlChucDanhCompany,
                PnlSearch = pnlSearchCompany,
                Txt = txtSearchCompany,
                BtnSearch = lbtSearchCompany,
                TagBox = searchTagBoxCompany,
                UpSearch = upnlSearchCompany,
                UpTag = upSearchTagCompany,
                UpList = upListCompany,
                UpCount = upCountCompany
            };
        }

        private bool _searchControlsReady;
        private List<Guid> _pickedForBind = new List<Guid>();

        protected void Page_Load(object sender, EventArgs e)
        {
            PrepareSearchControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            PrepareSearchControls();
            base.OnPreRender(e);
        }

        private void PrepareSearchControls()
        {
            if (_searchControlsReady || lbtSearchProj == null || lbtSearchCompany == null) return;

            ScriptManager script = ScriptManager.GetCurrent(Page);
            if (script == null) return;

            foreach (ListCtx c in new[] { GetCtx(true), GetCtx(false) })
            {
                script.RegisterAsyncPostBackControl(c.BtnSearch);
                c.Txt.EnterSubmitClientID = c.BtnSearch.ClientID;
            }
            _searchControlsReady = true;
        }

        public void OpenPicker(Guid idDuAn, Guid idCongViec, DateTime startDate, DateTime endDate, string taskName, Guid? idNhanVienQuanLy = null)
        {
            if (idDuAn == Guid.Empty || idCongViec == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            if (startDate.Date > endDate.Date)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.INVALID_DATA) ?? "Thời gian thực hiện công việc không hợp lệ.", MSGType.Error);
                return;
            }

            TblCongViec task = new Select().From(TblCongViec.Schema).Where(TblCongViec.Columns.IdCongViec).IsEqualTo(idCongViec).ExecuteSingle<TblCongViec>();
            if (task == null || task.IdDuAn != idDuAn)
            {
                ShowInvalidDataError();
                return;
            }

            this.IdDuAn = idDuAn;
            this.IdCongViec = idCongViec;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.IdNhanVienQuanLy = idNhanVienQuanLy;

            SetPickedIds(TaskManager.Instance.GetAssignedNhanVienIds(idCongViec));

            ListCtx proj = GetCtx(true);
            ListCtx company = GetCtx(false);

            InitSearchControls(proj);
            InitSearchControls(company);

            BindList(proj);
            BindList(company);

            mdlTaskMemberPicker.Title = GetResourceText(BackEndResourceKeys.ASSIGN_TASK);
            btnConfirmTaskAssign.ToolTip = btnConfirmTaskAssign.Text = GetResourceText(BackEndResourceKeys.SAVE);
            string thoiGian = $"<strong>{startDate:dd/MM/yyyy}</strong> - <strong>{endDate:dd/MM/yyyy}</strong>";

            string safeTaskName = HttpUtility.HtmlEncode(taskName);
            ltrTaskInfoNote.Text = $"<div style='margin-bottom: 5px; font-size: 13px;'><i class='fas fa-tasks me-1'></i> {GetResourceText(BackEndResourceKeys.TASK)}: <strong style='color: #b91c1c;'>{safeTaskName}</strong></div>" +
                                   $"<div style='font-size: 12px;'><i class='far fa-clock me-1'></i> {GetResourceText(BackEndResourceKeys.EXECUTION_TIME)}: {thoiGian}</div>";

            btnConfirmTaskAssign.Visible = !ViewOnly;
            mdlTaskMemberPicker.OpenModal(true);

            // Ép cập nhật cả nội dung Modal LẪN Footer chứa nút Lưu
            upnlMemberPicker.Update();
            upnlTaskMemberFooter.Update();
        }

        private void InitSearchControls(ListCtx c)
        {
            c.Txt.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            c.Ddl.SearchTagItemText = GetResourceText(BackEndResourceKeys.CHUC_DANH);
            c.Txt.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);

            new ControlHelpers().BindChucDanh(c.Ddl);
            c.Ddl.ClearSelection();
            c.Txt.Text = string.Empty;
            ClearKeywordClient(c);

            c.TagBox.TagItems.Clear();
            c.TagBox.Update();
            c.UpSearch.Update();
            c.UpTag.Update();
        }

        private void ClearKeywordClient(ListCtx c)
        {
            ScriptManager.RegisterClientScriptBlock(Page, GetType(), "ClearMemberKeyword_" + c.Key,
                string.Format("$('#{0}').val('');", c.Txt.ClientID), true);
        }

        private bool HasChucDanhFilter(ListCtx c)
        {
            string value = c.Ddl.SelectedValue;
            return !string.IsNullOrEmpty(value) && value != Guid.Empty.ToString();
        }

        private string GetKeyword(ListCtx c)
        {
            return (c.Txt.Text ?? string.Empty).Trim();
        }

        private Dictionary<Guid, DataRow> SearchEmployeeRows(ListCtx c)
        {
            var result = new Dictionary<Guid, DataRow>();

            Dictionary<string, object> keyValueSearchs = new ControlHelpers().GetControlValues(c.PnlSearch)
                                                         ?? new Dictionary<string, object>();
            keyValueSearchs[KEY_LA_NHAN_VIEN] = true;

            int totalRows;
            DataTable dt = UserManager.Instance.SearchUsers(GetKeyword(c), keyValueSearchs, "DisplayName ASC", 0, MAX_ROWS, out totalRows);

            if (dt == null || !dt.Columns.Contains("UserId")) return result;

            foreach (DataRow row in dt.Rows)
            {
                Guid id;
                if (Guid.TryParse(Convert.ToString(row["UserId"]), out id))
                    result[id] = row;
            }
            return result;
        }

        private static string GetRowString(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return string.Empty;
            return Convert.ToString(row[column]);
        }

        private List<Guid> GetPickedIds()
        {
            var list = new List<Guid>();
            string raw = ViewState["CtrlChonNhanVienTask_Picked"] as string;
            if (string.IsNullOrEmpty(raw)) return list;

            foreach (string part in raw.Split(','))
            {
                Guid id;
                if (Guid.TryParse(part, out id) && !list.Contains(id))
                    list.Add(id);
            }
            return list;
        }

        private void SetPickedIds(IEnumerable<Guid> ids)
        {
            ViewState["CtrlChonNhanVienTask_Picked"] = ids == null ? string.Empty : string.Join(",", ids.Select(x => x.ToString()));
        }

        private void SyncPickedFromRepeaters()
        {
            if (ViewOnly) return;

            List<Guid> picked = GetPickedIds();
            SyncOne(rptProjectMembers, picked);
            SyncOne(rptCompanyMembers, picked);
            SetPickedIds(picked);
        }

        private void SyncOne(Repeater rpt, List<Guid> picked)
        {
            foreach (RepeaterItem item in rpt.Items)
            {
                if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem) continue;

                CheckBox chkSelect = (CheckBox)item.FindControl("chkSelect");
                HiddenField hdfUserId = (HiddenField)item.FindControl("hdfUserId");
                Guid id;
                if (chkSelect == null || hdfUserId == null || !Guid.TryParse(hdfUserId.Value, out id)) continue;

                if (chkSelect.Checked)
                {
                    if (!picked.Contains(id)) picked.Add(id);
                }
                else
                {
                    picked.Remove(id);
                }
            }
        }

        private bool IsValidSelectedUserIds(IEnumerable<Guid> selectedIds, out List<Guid> invalidIds)
        {
            HashSet<Guid> allowedIds = new HashSet<Guid>(UserManager.Instance.GetAllActiveNhanVien().Select(x => x.UserId));
            foreach (Guid id in TaskManager.Instance.GetAssignedNhanVienIds(IdCongViec))
            {
                allowedIds.Add(id);
            }

            invalidIds = selectedIds.Where(id => !allowedIds.Contains(id)).Distinct().ToList();
            return invalidIds.Count == 0;
        }

        private void BindList(ListCtx c)
        {
            List<AspnetUser> allUsers = UserManager.Instance.GetAllActiveNhanVien();
            HashSet<Guid> activeIds = new HashSet<Guid>(allUsers.Select(u => u.UserId));

            List<Guid> currentAssigned = TaskManager.Instance.GetAssignedNhanVienIds(IdCongViec);
            foreach (Guid aId in currentAssigned)
            {
                if (!activeIds.Contains(aId))
                {
                    AspnetUser inactiveUser = UserManager.Instance.GetUserById(aId);
                    if (inactiveUser != null)
                    {
                        allUsers.Add(inactiveUser);
                    }
                }
            }

            HashSet<Guid> nhomA = new HashSet<Guid>(ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(IdDuAn));

            List<Guid> picked = GetPickedIds();
            SetPickedIds(picked);
            _pickedForBind = picked;

            List<AspnetUser> users = c.IsProject
                ? allUsers.Where(u => nhomA.Contains(u.UserId) || (!activeIds.Contains(u.UserId) && picked.Contains(u.UserId))).ToList()
                : allUsers.Where(u => !nhomA.Contains(u.UserId) && activeIds.Contains(u.UserId)).ToList();

            Dictionary<Guid, DataRow> employeeRows = SearchEmployeeRows(c);
            if (GetKeyword(c).Length > 0 || HasChucDanhFilter(c))
            {
                users = users.Where(u => employeeRows.ContainsKey(u.UserId)).ToList();
            }

            Guid? pmId = IdNhanVienQuanLy;
            if (c.IsProject)
            {
                users = users
                    .OrderByDescending(u => pmId.HasValue && u.UserId == pmId.Value)
                    .ThenByDescending(u => picked.Contains(u.UserId))
                    .ThenBy(u => u.DisplayName)
                    .ToList();
            }
            else
            {
                users = users
                    .OrderBy(u => u.DisplayName)
                    .ToList();
            }

            c.Rpt.DataSource = BuildDisplayList(users, StartDate, EndDate, pmId, employeeRows, activeIds);
            c.Rpt.DataBind();
            c.LtrCount.Text = users.Count.ToString();

            c.LtrNoData.Text = GetResourceText(BackEndResourceKeys.NO_DATA);
            c.PnlNoData.Visible = users.Count == 0;

            c.UpList.Update();
            c.UpCount.Update();
        }

        private List<object> BuildDisplayList(List<AspnetUser> users, DateTime start, DateTime end, Guid? pmId, Dictionary<Guid, DataRow> employeeRows, HashSet<Guid> activeIds)
        {
            var list = new List<object>();
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                string avatarPath = user.Avatar;

                DataRow info;
                employeeRows.TryGetValue(user.UserId, out info);

                list.Add(new
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    Email = GetRowString(info, "Email"),
                    IsPM = pmId.HasValue && user.UserId == pmId.Value,
                    IsInactive = !activeIds.Contains(user.UserId),
                    AvatarHtml = GetSingleAvatarHtml(user.DisplayName, avatarPath, i),
                    ScheduleJson = GenerateScheduleJson(user.UserId, start, end)
                });
            }
            return list;
        }

        protected void ddlChucDanhProj_SelectedValueChanged(object sender, EventArgs e) { RefreshList(GetCtx(true)); }
        protected void btnSearchProj_ServerClick(object sender, EventArgs e) { RefreshList(GetCtx(true)); }
        protected void searchTagBoxProj_TagClosed(object sender, SweetSoft.QLDA.Controls.SearchTagItem tag) { CloseTag(GetCtx(true), tag); }

        protected void ddlChucDanhCompany_SelectedValueChanged(object sender, EventArgs e) { RefreshList(GetCtx(false)); }
        protected void btnSearchCompany_ServerClick(object sender, EventArgs e) { RefreshList(GetCtx(false)); }
        protected void searchTagBoxCompany_TagClosed(object sender, SweetSoft.QLDA.Controls.SearchTagItem tag) { CloseTag(GetCtx(false), tag); }

        private void CloseTag(ListCtx c, SweetSoft.QLDA.Controls.SearchTagItem tag)
        {
            try
            {
                if (tag != null && tag.Key == c.Txt.ClientID)
                {
                    c.Txt.Text = string.Empty;
                    ClearKeywordClient(c);
                }
                else
                {
                    c.Ddl.ClearSelection();
                    c.UpSearch.Update();
                }
                RefreshList(c);
            }
            catch (Exception exc)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Có lỗi xảy ra.", MSGType.Error);
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Lỗi UI khi đóng search tag nhân viên Task"); } catch { }
            }
        }

        private void RefreshList(ListCtx c)
        {
            try
            {
                SyncPickedFromRepeaters();
                UpdateSearchTags(c);
                BindList(c);
            }
            catch (Exception exc)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Có lỗi xảy ra.", MSGType.Error);
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Lỗi RefreshList nhân viên Task"); } catch { }
            }
        }

        private void UpdateSearchTags(ListCtx c)
        {
            c.TagBox.TagItems.Clear();

            if (HasChucDanhFilter(c) && c.Ddl.SearchTagItem != null)
                c.TagBox.TagItems.Add(c.Ddl.SearchTagItem);

            if (GetKeyword(c).Length > 0 && c.Txt.SearchTagItem != null)
                c.TagBox.TagItems.Add(c.Txt.SearchTagItem);

            c.TagBox.Update();
            c.TagBox.Visible = true;
            c.UpTag.Update();
        }

        private string GenerateScheduleJson(Guid userId, DateTime start, DateTime end)
        {
            var lich = LichTrinhManager.Instance.LayLichTrinhNhanVien(userId, start, end);
            var dict = new Dictionary<string, object>();

            foreach (var ngay in lich)
            {
                int dow = (int)ngay.Ngay.DayOfWeek;
                string dayName = dow == 0 ? "CN" : dow == 6 ? "T7" : $"T{dow + 1}";

                string text;
                switch (ngay.TrangThaiLich)
                {

                    case "holiday":
                        text = "🎉 " + GetResourceText(BackEndResourceKeys.HOLIDAY);
                        break;
                    case "weekend":
                        text = "⬜ " + GetResourceText(BackEndResourceKeys.WEEKEND);
                        break;
                    case "busy":
                        text = $"🔴 {ngay.DanhSachCongViec.Count} {GetResourceText(BackEndResourceKeys.TASK).ToLower()}";
                        break;
                    default:
                        text = "🟢 " + GetResourceText(BackEndResourceKeys.FREE);
                        break;
                }

                dict.Add(ngay.Ngay.ToString("yyyy-MM-dd"), new { status = ngay.TrangThaiLich, dayName, text });
            }

            return JsonConvert.SerializeObject(dict);
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string GetSingleAvatarHtml(string name, string avatar, int index)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            string color = colors[index % colors.Length];
            bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            string safeName = HttpUtility.HtmlEncode(GetInitials(name));
            string fallbackHtml = $"<div class=\\'single-avatar-circle\\' style=\\'background-color: {color};\\'>{safeName}</div>";

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                string safeAvatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                return $"<img src='{safeAvatarUrl}' class='single-avatar-circle' style='object-fit: cover;' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
            }
            else
            {
                return $"<div class='single-avatar-circle' style='background-color: {color};'>{safeName}</div>";
            }
        }

        protected void rptMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            HiddenField hdfUserId = (HiddenField)e.Item.FindControl("hdfUserId");
            CheckBox chkSelect = (CheckBox)e.Item.FindControl("chkSelect");
            if (hdfUserId == null || chkSelect == null) return;

            if (Guid.TryParse(hdfUserId.Value, out Guid currentUserId) && _pickedForBind.Contains(currentUserId))
            {
                chkSelect.Checked = true;
            }
            chkSelect.Enabled = !ViewOnly;
        }

        protected void btnConfirmTaskAssign_Click(object sender, EventArgs e)
        {
            if (ViewOnly)
                return;

            // XÁC THỰC BẢO MẬT TẦNG SERVER: Ngăn chặn gửi request thẳng qua Postman 
            // Đảm bảo User hiện tại có quyền chỉnh sửa trên màn hình gốc chứa popup này
            if (!this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, this.CURRENT_PAGE.PAGE_FUNCTION_CODE))
            {
                ShowAccessDeniedNotify();
                return;
            }

            try
            {
                TblCongViec task = new Select().From(TblCongViec.Schema).Where(TblCongViec.Columns.IdCongViec).IsEqualTo(IdCongViec).ExecuteSingle<TblCongViec>();
                if (task == null || task.IdDuAn != IdDuAn)
                {
                    ShowInvalidDataError();
                    return;
                }

                SyncPickedFromRepeaters();
                List<Guid> selectedIds = GetPickedIds().Distinct().ToList();

                if (!IsValidSelectedUserIds(selectedIds, out List<Guid> invalidIds))
                {
                    ShowInvalidDataError();
                    return;
                }

                TaskManager.Instance.UpdateAssignments(this.IdDuAn, this.IdCongViec, selectedIds);

                mdlTaskMemberPicker.CloseModal();
                OnAssignConfirmed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Có lỗi xảy ra khi phân công nhân sự.", MSGType.Error);
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Lỗi khi gán nhân sự Task"); } catch { }
            }
        }
    }
}