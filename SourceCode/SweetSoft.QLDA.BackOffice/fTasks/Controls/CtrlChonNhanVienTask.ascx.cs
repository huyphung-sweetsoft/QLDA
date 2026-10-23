using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
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
        private Guid? IdCongViecCha
        {
            get => ViewState["IdCongViecCha"] as Guid?;
            set => ViewState["IdCongViecCha"] = value;
        }
        protected string ClientTaskId => IdCongViec.ToString("D");
        protected string ClientTimelineJson => ViewState["TaskMemberTimelineJson"] as string ?? "";
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
            get => ViewState["ViewOnly"] != null && (bool)ViewState["ViewOnly"];
            set => ViewState["ViewOnly"] = value;
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
        private readonly Dictionary<string, string> _memberCalendarCache = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _memberBusyCache = new Dictionary<string, int>();
        private Dictionary<Guid, DataRow> _fullEmployeeRows;
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
        // Timeline uses the SAME SweetSoft controls as the two original search panels.
        // Resolve by ID to avoid introducing new designer.cs fields in an existing WebForms project.
        private static T FindTimelineChild<T>(Control parent, string id) where T : Control
        {
            if (parent == null) return null;
            if (parent.ID == id) return parent as T;
            foreach (Control child in parent.Controls)
            {
                T found = FindTimelineChild<T>(child, id);
                if (found != null) return found;
            }
            return null;
        }
        private T TimelineControl<T>(string id) where T : Control
        {
            return FindTimelineChild<T>(upnlMemberPicker, id);
        }

        // BootstrapDropdown của dự án là WebControl có Items: List<DropdownItem>,
        // không phải asp:DropDownList/ListItemCollection. Dùng API chính thức.
        private static void BindSourceDropdown(SweetSoft.QLDA.Controls.BootstrapDropdown ddl)
        {
            if (ddl == null) return;
            ddl.DataSource = null;
            ddl.ClearItems();
            ddl.AddItem("Tất cả", "all");
            ddl.AddItem("Trong dự án", "project");
            ddl.AddItem("Ngoài dự án", "other");
            ddl.Text = "Tất cả";
            ddl.SelectedValue = "all";
        }
        // Tín hiệu từ server để JavaScript chỉ tự đóng/mở nhóm khi lọc thật sự.
        protected string ClientFilterRevision => Convert.ToString(ViewState["TaskMemberFilterRevision"] ?? 0);
        private string GetSourceFilter()
        {
            var ddl = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlSourceTimeline");
            var val = (ddl == null ? "all" : ddl.SelectedValue ?? "all").ToLowerInvariant();
            return val == "project" || val == "other" ? val : "all";
        }
        protected void ddlSourceTimeline_SelectedValueChanged(object sender, EventArgs e)
        {
            RefreshTimelineNativeSearch(false, false);
        }
        private void InitTimelineNativeSearch()
        {
            var ddl = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlChucDanhTimeline");
            var txt = TimelineControl<SweetSoft.QLDA.Controls.ExtraTextBox>("txtSearchTimeline");
            var tags = TimelineControl<SweetSoft.QLDA.Controls.ExtraSearchBox>("searchTagBoxTimeline");
            if (ddl == null || txt == null || tags == null) return;
            // Chức danh phải được bind độc lập. Nếu nguồn nhân sự lỗi, không được
            // bỏ qua việc nạp toàn bộ chức danh (lỗi dropdown chỉ còn 'Bỏ chọn').
            ddl.SearchTagItemText = GetResourceText(BackEndResourceKeys.CHUC_DANH);
            txt.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txt.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            new ControlHelpers().BindChucDanh(ddl);
            ddl.ClearSelection();
            txt.Text = string.Empty;
            var source = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlSourceTimeline");
            if (source != null)
            {
                source.SearchTagItemText = "Nguồn nhân sự";
                try
                {
                    BindSourceDropdown(source);
                    source.SelectedValue = "all";
                }
                catch (Exception ex)
                {
                    try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex, "Loi nap nguon nhan su - bo loc chuc danh da duoc nap"); } catch { }
                }
            }
            tags.TagItems.Clear();
            tags.Update();
            SyncTimelineNativeSearch();
        }
        private void SyncTimelineNativeSearch()
        {
            var ddl = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlChucDanhTimeline");
            var txt = TimelineControl<SweetSoft.QLDA.Controls.ExtraTextBox>("txtSearchTimeline");
            var tags = TimelineControl<SweetSoft.QLDA.Controls.ExtraSearchBox>("searchTagBoxTimeline");
            if (ddl == null || txt == null || tags == null) return;
            ListCtx a = GetCtx(true), b = GetCtx(false);
            string aKeyword = GetKeyword(a), bKeyword = GetKeyword(b);
            bool commonKeyword = string.Equals(aKeyword, bKeyword, StringComparison.Ordinal);
            string aRole = HasChucDanhFilter(a) ? a.Ddl.SelectedValue : string.Empty;
            string bRole = HasChucDanhFilter(b) ? b.Ddl.SelectedValue : string.Empty;
            bool commonRole = string.Equals(aRole, bRole, StringComparison.OrdinalIgnoreCase);
            txt.Text = commonKeyword ? aKeyword : string.Empty;
            if (commonRole && !string.IsNullOrEmpty(aRole)) ddl.SelectedValue = aRole;
            else ddl.ClearSelection();
            tags.TagItems.Clear();
            var source = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlSourceTimeline");
            if (source != null && GetSourceFilter() != "all" && source.SearchTagItem != null)
                tags.TagItems.Add(source.SearchTagItem);
            // Lấy chính SearchTagItem do bộ lọc gốc sinh ra. Hai Repeater ẩn vẫn có
            // bộ lọc SweetSoft hợp lệ; đừng tự tái tạo tag từ textbox sau postback.
            bool roleAdded = false, keywordAdded = false;
            foreach (var item in a.TagBox.TagItems)
            {
                if (item == null) continue;
                if (commonRole && !string.IsNullOrEmpty(aRole) && item.Key == a.Ddl.ClientID)
                {
                    tags.TagItems.Add(item); roleAdded = true;
                }
                else if (commonKeyword && aKeyword.Length > 0 && item.Key == a.Txt.ClientID)
                {
                    tags.TagItems.Add(item); keywordAdded = true;
                }
            }
            if (commonRole && !string.IsNullOrEmpty(aRole) && !roleAdded && ddl.SearchTagItem != null)
                tags.TagItems.Add(ddl.SearchTagItem);
            if (commonKeyword && aKeyword.Length > 0 && !keywordAdded && txt.SearchTagItem != null)
                tags.TagItems.Add(txt.SearchTagItem);
            tags.Update();
            var roleUp = TimelineControl<UpdatePanel>("upnlTimelineRole");
            var tagsUp = TimelineControl<UpdatePanel>("upnlTimelineTags");
            if (roleUp != null) roleUp.Update();
            if (tagsUp != null) tagsUp.Update();
        }
        private void RefreshTimelineNativeSearch(bool changeKeyword, bool changeRole)
        {
            try
            {
                var ddl = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlChucDanhTimeline");
                var txt = TimelineControl<SweetSoft.QLDA.Controls.ExtraTextBox>("txtSearchTimeline");
                if (ddl == null || txt == null) return;
                SyncPickedFromRepeaters();
                string keyword = (txt.Text ?? string.Empty).Trim();
                if (keyword.Length > 300) keyword = keyword.Substring(0, 300);
                string role = ddl.SelectedValue ?? string.Empty;
                Guid parsedRole;
                bool validRole = Guid.TryParse(role, out parsedRole) && parsedRole != Guid.Empty;
                foreach (ListCtx ctx in new[] { GetCtx(true), GetCtx(false) })
                {
                    if (changeKeyword) ctx.Txt.Text = keyword;
                    if (changeRole)
                    {
                        if (validRole) ctx.Ddl.SelectedValue = parsedRole.ToString("D");
                        else ctx.Ddl.ClearSelection();
                    }
                    ctx.UpSearch.Update();
                    UpdateSearchTags(ctx);
                    BindList(ctx);
                }
                ViewState["TaskMemberFilterRevision"] = Convert.ToInt32(ViewState["TaskMemberFilterRevision"] ?? 0) + 1;
                TryBuildTimelineSnapshot();
                SyncTimelineNativeSearch();
                upnlMemberPicker.Update();
                UpdateAssignmentSummary();
                upnlTaskMemberFooter.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Không thể lọc nhân viên.", MSGType.Error);
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Loi bo loc timeline Task native controls"); } catch { }
            }
        }
        protected void btnSearchTimeline_ServerClick(object sender, EventArgs e)
        {
            RefreshTimelineNativeSearch(true, false);
        }
        protected void ddlChucDanhTimeline_SelectedValueChanged(object sender, EventArgs e)
        {
            RefreshTimelineNativeSearch(false, true);
        }
        protected void searchTagBoxTimeline_TagClosed(object sender, SweetSoft.QLDA.Controls.SearchTagItem tag)
        {
            var txt = TimelineControl<SweetSoft.QLDA.Controls.ExtraTextBox>("txtSearchTimeline");
            var ddl = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlChucDanhTimeline");
            var source = TimelineControl<SweetSoft.QLDA.Controls.BootstrapDropdown>("ddlSourceTimeline");
            if (txt == null || ddl == null) return;
            if (tag != null && source != null && tag.Key == source.ClientID)
            {
                source.SelectedValue = "all";
                RefreshTimelineNativeSearch(false, false);
            }
            else if (tag != null && (tag.Key == txt.ClientID || tag.Key == GetCtx(true).Txt.ClientID || tag.Key == GetCtx(false).Txt.ClientID))
            {
                txt.Text = string.Empty;
                RefreshTimelineNativeSearch(true, false);
            }
            else if (tag != null && (tag.Key == ddl.ClientID || tag.Key == GetCtx(true).Ddl.ClientID || tag.Key == GetCtx(false).Ddl.ClientID))
            {
                ddl.ClearSelection();
                RefreshTimelineNativeSearch(false, true);
            }
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
            var extraSearch = TimelineControl<Control>("lbtSearchTimeline");
            var extraText = TimelineControl<SweetSoft.QLDA.Controls.ExtraTextBox>("txtSearchTimeline");
            if (extraSearch != null && extraText != null)
            {
                script.RegisterAsyncPostBackControl(extraSearch);
                extraText.EnterSubmitClientID = extraSearch.ClientID;
            }
            _searchControlsReady = true;
        }
        public void OpenPicker(Guid idDuAn, Guid idCongViec, DateTime startDate, DateTime endDate, string taskName, Guid? idNhanVienQuanLy = null, Guid? idCongViecCha = null)
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
            this.IdCongViecCha = idCongViecCha;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.IdNhanVienQuanLy = idNhanVienQuanLy;
            ViewState["TaskMemberTaskName"] = taskName ?? string.Empty;
            // Chụp lại trạng thái phân công ban đầu để so sánh khi người dùng thay đổi.
            List<Guid> originalAssignedIds = TaskManager.Instance
                .GetAssignedNhanVienIds(idCongViec)
                .Distinct()
                .ToList();

            ViewState["TaskMemberOriginalSnapshot"] = string.Join(",", originalAssignedIds);
            SetOriginalAssignedIds(originalAssignedIds);
            SetPickedIds(originalAssignedIds);
            UpdateAssignmentSummary();
            lblAssignmentSummary.Visible = !ViewOnly;
            ListCtx proj = GetCtx(true);
            ListCtx company = GetCtx(false);
            InitSearchControls(proj);
            InitSearchControls(company);
            ViewState["TaskMemberFilterRevision"] = 0;
            // The timeline is auxiliary: failure to prepare its filter must not prevent opening the picker.
            try { InitTimelineNativeSearch(); }
            catch (Exception exc)
            {
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Loi khoi tao bo loc timeline nhan su"); } catch { }
            }
            TryBuildTimelineSnapshot();
            BindList(proj);
            BindList(company);
            mdlTaskMemberPicker.Title = GetResourceText(BackEndResourceKeys.ASSIGN_TASK);
            btnConfirmTaskAssign.ToolTip = btnConfirmTaskAssign.Text = GetResourceText(BackEndResourceKeys.SAVE);
            // TÍNH TOÁN SỐ NGÀY THỜI HẠN CỦA TASK
            int totalTaskDays = 0;
            if (startDate != DateTime.MinValue && endDate != DateTime.MinValue && endDate >= startDate)
            {
                totalTaskDays = LichBieuChungManager.Instance.CountWorkingDaysInRange(startDate, endDate);
            }

            string thoiGian = $"<strong>{startDate:dd/MM/yyyy}</strong> - <strong>{endDate:dd/MM/yyyy}</strong>";
            string safeTaskName = HttpUtility.HtmlEncode(taskName);

            // BỔ SUNG BADGE "THỜI HẠN: Y NGÀY" VÀO NGAY CẠNH TÊN CÔNG VIỆC
            ltrTaskInfoNote.Text = $"<div style='margin-bottom: 5px; font-size: 13px;'><i class='fas fa-tasks me-1'></i> {GetResourceText(BackEndResourceKeys.TASK)}: <strong style='color: #b91c1c;'>{safeTaskName}</strong>" +
                                   $"<span class='badge ms-2' style='background-color: #dbeafe; color: #1e40af; border: 1px solid #bfdbfe; font-size: 11px;'><i class='fas fa-hourglass-half me-1'></i>Thời hạn: {totalTaskDays} ngày</span></div>" +
                                   $"<div style='font-size: 12px;'><i class='far fa-clock me-1'></i> {GetResourceText(BackEndResourceKeys.EXECUTION_TIME)}: {thoiGian}</div>";
            btnConfirmTaskAssign.Visible = !ViewOnly;
            mdlTaskMemberPicker.OpenModal(true);
            upnlMemberPicker.Update();
            upnlTaskMemberFooter.Update();
        }
        private static List<Guid> ParseGuidIds(string raw)
        {
            var result = new List<Guid>();

            if (string.IsNullOrWhiteSpace(raw))
                return result;

            foreach (string part in raw.Split(','))
            {
                if (Guid.TryParse(part, out Guid id) && !result.Contains(id))
                    result.Add(id);
            }

            return result;
        }

        private void SetOriginalAssignedIds(IEnumerable<Guid> ids)
        {
            hdfOriginalAssignedIdsClient.Value = ids == null
                ? string.Empty
                : string.Join(",", ids.Distinct().Select(id => id.ToString()));
        }

        private void UpdateAssignmentSummary()
        {
            if (lblAssignmentSummary == null ||
                hdfOriginalAssignedIdsClient == null)
                return;

            var original = new HashSet<Guid>(
                ParseGuidIds(hdfOriginalAssignedIdsClient.Value));

            var picked = new HashSet<Guid>(GetPickedIds());

            int kept = picked.Count(id => original.Contains(id));
            int added = picked.Count - kept;
            int removed = original.Count(id => !picked.Contains(id));

            lblAssignmentSummary.Text =
                $"{picked.Count} nhân viên sau lưu" +
                $"  ·  Giữ {kept}" +
                $"  ·  Thêm {added}" +
                $"  ·  Gỡ {removed}";
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
            Dictionary<string, object> keyValueSearchs = new ControlHelpers().GetControlValues(c.PnlSearch) ?? new Dictionary<string, object>();
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
        private static string GetChucDanh(DataRow row)
        {
            return GetRowString(row, "TenChucDanh").Trim();
        }
        private static string GetRowString(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return string.Empty;
            return Convert.ToString(row[column]);
        }
        private List<Guid> GetPickedIds()
        {
            return ParseGuidIds(
                ViewState["CtrlChonNhanVienTask_Picked"] as string);
        }
        private void SetPickedIds(IEnumerable<Guid> ids)
        {
            string raw = ids == null
                ? string.Empty
                : string.Join(",", ids.Distinct().Select(id => id.ToString()));

            ViewState["CtrlChonNhanVienTask_Picked"] = raw;

            // Đồng bộ danh sách lựa chọn với phía trình duyệt.
            if (hdfPickedIdsClient != null)
                hdfPickedIdsClient.Value = raw;
        }
        private void SyncPickedFromRepeaters()
        {
            if (ViewOnly)
                return;
            List<Guid> picked = hdfPickedIdsClient != null
                ? ParseGuidIds(hdfPickedIdsClient.Value)
                : GetPickedIds();
            SyncOne(rptProjectMembers, picked);
            SyncOne(rptCompanyMembers, picked);

            SetPickedIds(picked);
            UpdateAssignmentSummary();
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
        private bool IsPmOrAdmin()
        {
            return this.CURRENT_PAGE != null && (this.CURRENT_PAGE.IsPM || this.CURRENT_PAGE.IsAdministrator);
        }
        private bool IsValidSelectedUserIds(IEnumerable<Guid> selectedIds, out List<Guid> invalidIds)
        {
            HashSet<Guid> allowedIds;
            if (IsPmOrAdmin())
            {
                allowedIds = new HashSet<Guid>(UserManager.Instance.GetAllActiveNhanVien().Select(x => x.UserId));
                foreach (Guid id in TaskManager.Instance.GetAssignedNhanVienIds(IdCongViec))
                    allowedIds.Add(id);
            }
            else if (IdCongViecCha.HasValue)
            {
                allowedIds = new HashSet<Guid>(TaskManager.Instance.GetAssignedNhanVienIds(IdCongViecCha.Value));
            }
            else
            {
                allowedIds = new HashSet<Guid>(UserManager.Instance.GetAllActiveNhanVien().Select(x => x.UserId));
                foreach (Guid id in TaskManager.Instance.GetAssignedNhanVienIds(IdCongViec))
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
            HashSet<Guid> originalAssignedIds = new HashSet<Guid>(currentAssigned);
            foreach (Guid aId in currentAssigned)
            {
                if (!activeIds.Contains(aId))
                {
                    AspnetUser inactiveUser = UserManager.Instance.GetUserById(aId);
                    if (inactiveUser != null)
                        allUsers.Add(inactiveUser);
                }
            }
            HashSet<Guid> nhomA = new HashSet<Guid>(ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(IdDuAn));
            HashSet<Guid> parentAssignedIds = new HashSet<Guid>();
            if (IdCongViecCha.HasValue)
            {
                parentAssignedIds = new HashSet<Guid>(TaskManager.Instance.GetAssignedNhanVienIds(IdCongViecCha.Value));
                foreach (Guid parentUserId in parentAssignedIds)
                {
                    if (!activeIds.Contains(parentUserId))
                    {
                        AspnetUser parentInactiveUser = UserManager.Instance.GetUserById(parentUserId);
                        if (parentInactiveUser != null && !allUsers.Any(x => x.UserId == parentUserId))
                            allUsers.Add(parentInactiveUser);
                    }
                }
            }
            List<Guid> picked = GetPickedIds();
            SetPickedIds(picked);
            _pickedForBind = picked;
            List<AspnetUser> users;
            if (IsPmOrAdmin())
            {
                users = c.IsProject
                    ? allUsers.Where(u => nhomA.Contains(u.UserId) || (!activeIds.Contains(u.UserId) && picked.Contains(u.UserId))).ToList()
                    : allUsers.Where(u => !nhomA.Contains(u.UserId) && activeIds.Contains(u.UserId)).ToList();
            }
            else if (IdCongViecCha.HasValue)
            {
                users = c.IsProject
                    ? allUsers.Where(u => parentAssignedIds.Contains(u.UserId) && (nhomA.Contains(u.UserId) || !activeIds.Contains(u.UserId))).ToList()
                    : allUsers.Where(u => parentAssignedIds.Contains(u.UserId) && !nhomA.Contains(u.UserId) && activeIds.Contains(u.UserId)).ToList();
            }
            else
            {
                users = c.IsProject
                    ? allUsers.Where(u => nhomA.Contains(u.UserId) || (!activeIds.Contains(u.UserId) && picked.Contains(u.UserId))).ToList()
                    : allUsers.Where(u => !nhomA.Contains(u.UserId) && activeIds.Contains(u.UserId)).ToList();
            }
            // Always render all eligible users as hidden server-owned templates; client filters only applicants.
            // Important: selecting staff must never disappear from the source Repeaters on a filtered postback.
            if (_fullEmployeeRows == null) _fullEmployeeRows = GetAllEmployeeRowsForTimeline();
            Dictionary<Guid, DataRow> employeeRows = _fullEmployeeRows;
            Guid? pmId = IdNhanVienQuanLy;
            // Tính toán lịch và số ngày bận TRƯỚC KHI SORT
            Dictionary<Guid, int> busyDaysDict;
            Dictionary<Guid, string> scheduleJsonDict;
            CalculateSchedules(users, StartDate, EndDate, out busyDaysDict, out scheduleJsonDict);

            // LOGIC SẮP XẾP MỚI: Ai ít ngày bận nhất sẽ đưa lên đầu (Rảnh -> Bận)
            if (c.IsProject)
            {
                users = users.OrderByDescending(u => pmId.HasValue && u.UserId == pmId.Value)
                             .ThenByDescending(u => picked.Contains(u.UserId))
                             .ThenBy(u => busyDaysDict.ContainsKey(u.UserId) ? busyDaysDict[u.UserId] : 0) // Rảnh nhất lên đầu
                             .ThenBy(u => u.DisplayName)
                             .ToList();
            }
            else
            {
                users = users.OrderBy(u => busyDaysDict.ContainsKey(u.UserId) ? busyDaysDict[u.UserId] : 0) // Rảnh nhất lên đầu
                             .ThenBy(u => u.DisplayName)
                             .ToList();
            }

            c.Rpt.DataSource = BuildDisplayList(users, pmId, employeeRows, activeIds, originalAssignedIds, busyDaysDict, scheduleJsonDict);
            c.Rpt.DataBind();
            c.LtrCount.Text = users.Count.ToString();
            c.LtrNoData.Text = GetResourceText(BackEndResourceKeys.NO_DATA);
            c.PnlNoData.Visible = users.Count == 0;
            c.UpList.Update();
            c.UpCount.Update();
        }
        protected void ddlChucDanhProj_SelectedValueChanged(object sender, EventArgs e)
        {
            RefreshList(GetCtx(true));
        }
        protected void btnSearchProj_ServerClick(object sender, EventArgs e)
        {
            RefreshList(GetCtx(true));
        }
        protected void searchTagBoxProj_TagClosed(object sender, SweetSoft.QLDA.Controls.SearchTagItem tag)
        {
            CloseTag(GetCtx(true), tag);
        }
        protected void ddlChucDanhCompany_SelectedValueChanged(object sender, EventArgs e)
        {
            RefreshList(GetCtx(false));
        }
        protected void btnSearchCompany_ServerClick(object sender, EventArgs e)
        {
            RefreshList(GetCtx(false));
        }
        protected void searchTagBoxCompany_TagClosed(object sender, SweetSoft.QLDA.Controls.SearchTagItem tag)
        {
            CloseTag(GetCtx(false), tag);
        }
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
                TryBuildTimelineSnapshot();
                SyncTimelineNativeSearch();
                // Cập nhật cả JSON, hai danh sách và toolbar khi tìm kiếm trong bất kỳ tab nào.
                upnlMemberPicker.Update();
                UpdateAssignmentSummary();
                if (upnlTaskMemberFooter != null)
                    upnlTaskMemberFooter.Update();
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
        private string GetSingleAvatarHtml(string name, string avatar, Guid userId)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            // Stable across both views and sorting/filtering; never derive palette from row index.
            byte[] bytes = userId.ToByteArray();
            int colorSeed = 0;
            foreach (byte part in bytes) colorSeed = (colorSeed + part) % colors.Length;
            string color = colors[colorSeed];
            bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
            string safeName = HttpUtility.HtmlEncode(GetInitials(name));
            string fallbackHtml = $"<div class=\\'single-avatar-circle\\' style=\\'background-color: {color};\\'>{safeName}</div>";
            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                string safeAvatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                return $"<img src='{safeAvatarUrl}' class='single-avatar-circle' style='object-fit: cover;' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
            }
            return $"<div class='single-avatar-circle' style='background-color: {color};'>{safeName}</div>";
        }
        protected void rptMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            HiddenField hdfUserId =
                (HiddenField)e.Item.FindControl("hdfUserId");

            CheckBox chkSelect =
                (CheckBox)e.Item.FindControl("chkSelect");

            if (hdfUserId == null || chkSelect == null)
                return;

            if (Guid.TryParse(hdfUserId.Value, out Guid currentUserId))
            {
                chkSelect.Checked =
                    _pickedForBind.Contains(currentUserId);
            }

            chkSelect.Enabled = !ViewOnly;
        }
        // UX V3: Yêu cầu đóng sau khi có chỉnh sửa chưa lưu; chỉ xác nhận,
        // tuyệt đối không ghi phân công xuống cơ sở dữ liệu tại đây.
        protected void lbtRequestDiscardTaskAssign_Click(object sender, EventArgs e)
        {
            if (IdCongViec == Guid.Empty) return;
            var originalIds = new HashSet<Guid>(ParseGuidIds(ViewState["TaskMemberOriginalSnapshot"] as string));
            var proposedIds = new HashSet<Guid>(ParseGuidIds(hdfPickedIdsClient.Value));
            var added = proposedIds.Except(originalIds).Count();
            var removed = originalIds.Except(proposedIds).Count();
            if (added == 0 && removed == 0)
            {
                CloseTaskMemberPickerAfterDiscard();
                return;
            }

            string safeTaskName = HttpUtility.HtmlEncode(ViewState["TaskMemberTaskName"] as string ?? "Công việc hiện tại");
            string html = "<div style='text-align:left;font-size:13px;line-height:1.6'>" +
                "<div style='color:#475569;margin-bottom:8px'>Công việc: <strong>" + safeTaskName + "</strong></div>" +
                "<div style='padding:10px 12px;background:#fffbeb;border:1px solid #fde68a;border-radius:7px;margin-bottom:10px'>" +
                "<strong style='color:#92400e'>Bạn đang có thay đổi chưa lưu</strong>" +
                "<div style='color:#475569;margin-top:4px'>Thêm <strong>" + added +
                "</strong> · Gỡ <strong>" + removed + "</strong> nhân viên</div></div>" +
                "<div style='font-weight:600;color:#334155'>Đóng cửa sổ và bỏ tất cả thay đổi này?</div></div>";
            var result = new ConfirmResult { CommandName = "CONFIRM_DISCARD_TASK_ASSIGNMENT_CHANGES" };
            CURRENT_PAGE.CurrentConfirmResult = result;
            OpenMessageBox(new MessageBox("Thay đổi phân công chưa lưu", html,
                MSGButton.AcceptCancel, MSGIcon.Warning), result, false, false);
        }

        private void CloseTaskMemberPickerAfterDiscard()
        {
            // Đóng bằng chính ExtraModal như luồng Lưu thành công của control.
            // Không Save, không gọi UpdateAssignments, không ảnh hưởng quyền phân công.
            ScriptManager.RegisterStartupScript(Page, GetType(), "TaskMemberDiscardAccepted",
                "if(window.TaskMemberDiscardGuard)window.TaskMemberDiscardGuard.cancelClose();", true);
            mdlTaskMemberPicker.CloseModal();
        }

        protected void btnConfirmTaskAssign_Click(object sender, EventArgs e)
        {
            if (ViewOnly) return;
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
                IdCongViecCha = task.IdCongViecCha;
                if (AssignmentChangedWhileOpen())
                {
                    ShowNotify("Danh sách phân công đã thay đổi từ nơi khác. Hãy đóng và mở lại cửa sổ để tránh ghi đè.", MSGType.Error);
                    return;
                }
                SyncPickedFromRepeaters();
                List<Guid> selectedIds = GetPickedIds().Distinct().ToList();

                if (!IsValidSelectedUserIds(selectedIds, out List<Guid> invalidIds))
                {
                    ShowInvalidDataError();
                    return;
                }

                // Phân biệt người có trong DB trước khi mở popup với người đang chọn.
                // Một lần bấm Lưu chỉ mở tối đa MỘT MessageBox, dù vừa gỡ vừa thêm người bận.
                var originalIds = new HashSet<Guid>(ParseGuidIds(ViewState["TaskMemberOriginalSnapshot"] as string));
                var selectedSet = new HashSet<Guid>(selectedIds);
                List<Guid> removedIds = originalIds.Except(selectedSet).OrderBy(x => x).ToList();
                List<Guid> addedIds = selectedSet.Except(originalIds).OrderBy(x => x).ToList();
                var busyAdded = new List<KeyValuePair<Guid, int>>();
                foreach (Guid userId in addedIds)
                {
                    var schedule = LichTrinhManager.Instance.LayLichTrinhNhanVien(userId, StartDate, EndDate);
                    int busyDays = schedule == null ? 0 : schedule.Count(day =>
                        day.TrangThaiLich == "busy" &&
                        day.DanhSachCongViec != null &&
                        day.DanhSachCongViec.Any(job => job.IdCongViec != IdCongViec &&
                            job.TrangThai != 2 && job.TrangThai != 3));
                    if (busyDays > 0) busyAdded.Add(new KeyValuePair<Guid, int>(userId, busyDays));
                }
                if (removedIds.Count > 0 || busyAdded.Count > 0)
                {
                    string message = BuildAssignmentChangeMessage(removedIds, addedIds, busyAdded);
                    var result = new ConfirmResult
                    {
                        CommandName = "CONFIRM_TASK_ASSIGNMENT_CHANGES",
                        Value = selectedIds
                    };
                    CURRENT_PAGE.CurrentConfirmResult = result;
                    OpenMessageBox(new MessageBox("Xác nhận thay đổi phân công", message,
                        MSGButton.AcceptCancel, MSGIcon.Warning), result, false, false);
                    return;
                }
                // Không có ai trùng lịch -> Lưu thẳng xuống Database
                TaskManager.Instance.UpdateAssignments(IdDuAn, IdCongViec, selectedIds);
                mdlTaskMemberPicker.CloseModal();
                OnAssignConfirmed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Có lỗi xảy ra khi phân công nhân sự.", MSGType.Error);
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Lỗi khi gán nhân sự Task"); } catch { }
            }
        }
        // Dùng MessageBox của Master: tất cả thay đổi rủi ro được xác nhận trong một bước.
        // Encode tên và tài khoản nhân viên vì MessageBox render nội dung HTML.
        private string BuildAssignmentChangeMessage(List<Guid> removedIds, List<Guid> addedIds,
            List<KeyValuePair<Guid, int>> busyAdded)
        {
            var html = new System.Text.StringBuilder();
            html.Append("<div style='text-align:left;font-size:13px;line-height:1.55;max-height:52vh;overflow-y:auto;padding:1px 2px'>");
            html.Append("<div style='margin-bottom:10px;color:#475569'>Công việc: <strong>")
                .Append(HttpUtility.HtmlEncode(ViewState["TaskMemberTaskName"] as string ?? "Công việc hiện tại"))
                .Append("</strong></div>");
            if (removedIds.Count > 0)
            {
                html.Append("<section style='background:#fff5f5;border:1px solid #fecaca;border-radius:7px;padding:10px 12px;margin-bottom:10px'>")
                    .Append("<div style='color:#b42318;font-weight:700;margin-bottom:6px'>Gỡ phân công · ")
                    .Append(removedIds.Count).Append(" nhân viên</div>");
                foreach (Guid id in removedIds)
                {
                    AppendAssignmentPerson(html, id, null);
                }
                html.Append("<div style='color:#7f1d1d;font-size:12px;margin-top:6px'>Những nhân viên này sẽ không còn được giao công việc trên; không bị gỡ khỏi dự án.</div></section>");
            }
            if (busyAdded.Count > 0)
            {
                html.Append("<section style='background:#fffbeb;border:1px solid #fde68a;border-radius:7px;padding:10px 12px;margin-bottom:10px'>")
                    .Append("<div style='color:#a16207;font-weight:700;margin-bottom:4px'>Thêm nhân viên có lịch bận · ")
                    .Append(busyAdded.Count).Append(" người</div>")
                    .Append("<div style='color:#92400e;font-size:12px;margin-bottom:6px'>Trong thời hạn ")
                    .Append(StartDate.ToString("dd/MM/yyyy")).Append(" – ").Append(EndDate.ToString("dd/MM/yyyy"))
                    .Append("</div>");
                foreach (var item in busyAdded)
                {
                    AppendAssignmentPerson(html, item.Key, "Bận " + item.Value + " ngày");
                }
                html.Append("</section>");
            }
            int freeAddedCount = addedIds.Count - busyAdded.Count;
            if (freeAddedCount > 0)
            {
                html.Append("<div style='margin:7px 0 10px;color:#475569'>Ngoài ra, có ")
                    .Append(freeAddedCount).Append(" nhân viên mới không phát sinh cảnh báo trùng lịch.</div>");
            }
            html.Append("<div style='font-weight:600;color:#334155'>Bạn có chắc chắn muốn áp dụng những thay đổi phân công này không?</div></div>");
            return html.ToString();
        }
        private static void AppendAssignmentPerson(System.Text.StringBuilder html, Guid userId, string note)
        {
            AspnetUser user = UserManager.Instance.GetUserById(userId);
            string name = user == null || string.IsNullOrWhiteSpace(user.DisplayName)
                ? "Nhân viên (" + userId.ToString("D") + ")" : user.DisplayName;
            html.Append("<div style='padding:3px 0;border-bottom:1px solid rgba(100,116,139,.12)'>• <strong>")
                .Append(HttpUtility.HtmlEncode(name)).Append("</strong>");
            if (user != null && !string.IsNullOrWhiteSpace(user.UserName))
            {
                html.Append(" <span style='color:#64748b;font-size:11px'>(")
                    .Append(HttpUtility.HtmlEncode(user.UserName)).Append(")</span>");
            }
            if (!string.IsNullOrEmpty(note))
                html.Append(" <span style='float:right;font-weight:600;color:#a16207'>")
                    .Append(HttpUtility.HtmlEncode(note)).Append("</span>");
            html.Append("</div>");
        }
        private void GetMemberSchedule(Guid userId, DateTime start, DateTime end, out int busyDays, out string scheduleJson)
        {
            string cacheKey = userId.ToString("N") + ":" + start.ToString("yyyyMMdd") + ":" + end.ToString("yyyyMMdd");
            if (_memberCalendarCache.TryGetValue(cacheKey, out scheduleJson))
            {
                busyDays = _memberBusyCache[cacheKey];
                return;
            }
            var lich = LichTrinhManager.Instance.LayLichTrinhNhanVien(userId, start, end);
            // Lịch trạng thái của Manager có thể bỏ qua công việc hiện tại trong một số truy vấn.
            // Với nhân viên đã phân công, khoảng ngày làm việc trong task vẫn có lịch của task này.
            bool alreadyAssigned = ParseGuidIds(ViewState["TaskMemberOriginalSnapshot"] as string).Contains(userId);
            string taskName = ViewState["TaskMemberTaskName"] as string ?? "Công việc hiện tại";
            var days = new Dictionary<string, object>();
            busyDays = 0;
            foreach (var ngay in lich)
            {
                string status = ngay.TrangThaiLich;
                var activeTasks = ngay.DanhSachCongViec == null ? null
                    : ngay.DanhSachCongViec.Where(t => t.TrangThai != 2 && t.TrangThai != 3).ToList();
                var thisTask = activeTasks == null ? null : activeTasks.FirstOrDefault(t => t.IdCongViec == IdCongViec);
                bool inTaskPeriod = ngay.Ngay.Date >= StartDate.Date && ngay.Ngay.Date <= EndDate.Date;
                bool onWorkingDay = status != "holiday" && status != "weekend";
                bool current = thisTask != null || (alreadyAssigned && inTaskPeriod && onWorkingDay);
                var otherTasks = activeTasks == null ? null : activeTasks.Where(t => t.IdCongViec != IdCongViec).ToList();
                int otherCount = otherTasks == null ? 0 : otherTasks.Count;
                if (status == "busy")
                {
                    status = otherCount > 0 ? "busy" : "free";
                    if (otherCount > 0 && ngay.Ngay.Date >= StartDate.Date && ngay.Ngay.Date <= EndDate.Date) busyDays++;
                }
                string text = status == "holiday" ? (GetResourceText(BackEndResourceKeys.HOLIDAY) ?? "Ngày nghỉ")
                    : status == "weekend" ? (GetResourceText(BackEndResourceKeys.WEEKEND) ?? "Ngày nghỉ tuần")
                    : status == "busy" ? otherCount + " công việc khác"
                    : current ? "Đã phân công công việc hiện tại" : (GetResourceText(BackEndResourceKeys.FREE) ?? "Trống lịch");
                days[ngay.Ngay.ToString("yyyy-MM-dd")] = new
                {
                    status = status,
                    text = text,
                    current = current,
                    currentTask = thisTask != null
                        ? new { code = thisTask.MaCongViec, name = thisTask.TenCongViec, projectName = thisTask.TenDuAn }
                        : (current ? new { code = string.Empty, name = taskName, projectName = string.Empty } : null),
                    tasks = otherTasks == null ? null : otherTasks.Select(t => new { code = t.MaCongViec, name = t.TenCongViec, projectName = t.TenDuAn }).ToList()
                };
            }
            scheduleJson = Newtonsoft.Json.JsonConvert.SerializeObject(days);
            _memberCalendarCache[cacheKey] = scheduleJson;
            _memberBusyCache[cacheKey] = busyDays;
        }

        private void CalculateSchedules(List<AspnetUser> users, DateTime start, DateTime end, out Dictionary<Guid, int> busyDaysDict, out Dictionary<Guid, string> scheduleJsonDict)
        {
            busyDaysDict = new Dictionary<Guid, int>();
            scheduleJsonDict = new Dictionary<Guid, string>();
            foreach (var user in users)
            {
                int count;
                string json;
                GetMemberSchedule(user.UserId, start, end, out count, out json);
                busyDaysDict[user.UserId] = count;
                scheduleJsonDict[user.UserId] = json;
            }
        }

        private Dictionary<Guid, DataRow> GetAllEmployeeRowsForTimeline()
        {
            // Lay metadata CHUA loc de Timeline khong mat chuc danh/email khi nguoi dung filter.
            // Neu cau truy van danh muc nay loi, dung ket qua cua hai cot hien tai.
            var result = new Dictionary<Guid, DataRow>();
            try
            {
                int count;
                var parameters = new Dictionary<string, object>();
                parameters[KEY_LA_NHAN_VIEN] = true;
                DataTable dt = UserManager.Instance.SearchUsers("", parameters, "DisplayName ASC", 0, MAX_ROWS, out count);
                if (dt != null && dt.Columns.Contains("UserId"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Guid id;
                        if (Guid.TryParse(Convert.ToString(row["UserId"]), out id)) result[id] = row;
                    }
                }
            }
            catch (Exception ex)
            {
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex, "Khong tai duoc metadata tat ca nhan vien timeline"); } catch { }
            }
            // Phan du lieu da tim thay tu control cung luon duoc uu tien neu cap nhat.
            foreach (var pair in SearchEmployeeRows(GetCtx(true))) result[pair.Key] = pair.Value;
            foreach (var pair in SearchEmployeeRows(GetCtx(false))) result[pair.Key] = pair.Value;
            return result;
        }

        private void TryBuildTimelineSnapshot()
        {
            // Timeline la chuc nang bo sung. Loi o timeline khong duoc chan modal phan cong.
            try
            {
                BuildTimelineSnapshot();
            }
            catch (Exception exc)
            {
                ViewState["TaskMemberTimelineJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    start = StartDate.ToString("yyyy-MM-dd"),
                    end = EndDate.ToString("yyyy-MM-dd"),
                    displayStart = StartDate.ToString("yyyy-MM-dd"),
                    displayEnd = EndDate.ToString("yyyy-MM-dd"),
                    workingDays = 0,
                    readonlyMode = ViewOnly,
                    timelineError = true,
                    filters = new
                    {
                        project = new { keyword = "", role = "" },
                        other = new { keyword = "", role = "" }
                    },
                    members = new object[0]
                });
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Loi tai timeline nhan su (khong chan popup phan cong)"); } catch { }
            }
        }

        private void BuildTimelineSnapshot()
        {
            int taskCalendarDays = (EndDate.Date - StartDate.Date).Days + 1;
            int buffer = taskCalendarDays <= 3 ? 6 : taskCalendarDays <= 10 ? 5 : taskCalendarDays <= 20 ? 3 : 2;
            // Always supply real schedule data for at least 21 calendar days.
            // The allocation period itself is unchanged; X/Y still counts only StartDate..EndDate.
            int standardDays = taskCalendarDays + 2 * buffer;
            int extraDays = Math.Max(0, 21 - standardDays);
            int beforeExtra = extraDays / 2;
            int afterExtra = extraDays - beforeExtra;
            DateTime displayStart = StartDate.Date.AddDays(-buffer - beforeExtra);
            DateTime displayEnd = EndDate.Date.AddDays(buffer + afterExtra);
            var all = UserManager.Instance.GetAllActiveNhanVien();
            var activeIds = new HashSet<Guid>(all.Select(u => u.UserId));
            var assigned = TaskManager.Instance.GetAssignedNhanVienIds(IdCongViec);
            var projectMembers = new HashSet<Guid>(ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(IdDuAn));
            var parentMembers = IdCongViecCha.HasValue
                ? new HashSet<Guid>(TaskManager.Instance.GetAssignedNhanVienIds(IdCongViecCha.Value))
                : new HashSet<Guid>();
            var eligibleIds = new HashSet<Guid>();
            if (!IsPmOrAdmin() && IdCongViecCha.HasValue)
            {
                foreach (Guid id in parentMembers) eligibleIds.Add(id);
            }
            else
            {
                foreach (var u in all) eligibleIds.Add(u.UserId);
                foreach (Guid id in assigned) eligibleIds.Add(id);
            }
            foreach (Guid id in eligibleIds)
            {
                if (!all.Any(u => u.UserId == id))
                {
                    AspnetUser inactive = UserManager.Instance.GetUserById(id);
                    if (inactive != null) all.Add(inactive);
                }
            }
            Dictionary<Guid, DataRow> employeeRows = GetAllEmployeeRowsForTimeline();
            // Dự phòng trường hợp SearchUsers trả về tên chức danh nhưng không có cột IdChucDanh.
            var roleLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // Truy van bo sung nhan chuc danh KHONG duoc phep ngan popup mo.
            try
            {
                var roles = new Select().From(TblChucDanh.Schema).ExecuteTypedList<TblChucDanh>();
                if (roles != null)
                    foreach (TblChucDanh role in roles)
                        if (role != null && !string.IsNullOrWhiteSpace(role.TenChucDanh))
                            roleLookup[role.TenChucDanh.Trim()] = role.IdChucDanh.ToString("D");
            }
            catch (Exception exc)
            {
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Khong tai duoc danh muc chuc danh timeline Task"); } catch { }
            }
            var members = new List<object>();
            foreach (AspnetUser u in all.Where(u => eligibleIds.Contains(u.UserId))
                .OrderBy(u => u.DisplayName))
            {
                bool inactive = !activeIds.Contains(u.UserId);
                bool inProject = projectMembers.Contains(u.UserId);
                DataRow row;
                employeeRows.TryGetValue(u.UserId, out row);
                string roleName = GetChucDanh(row);
                string roleId = GetRowString(row, "IdChucDanh");
                Guid parsedRole;
                if (!Guid.TryParse(roleId, out parsedRole))
                    roleId = roleLookup.ContainsKey(roleName) ? roleLookup[roleName] : string.Empty;
                int busy;
                string scheduleJson;
                GetMemberSchedule(u.UserId, displayStart, displayEnd, out busy, out scheduleJson);
                members.Add(new
                {
                    id = u.UserId.ToString("D"),
                    name = u.DisplayName ?? "Nhân viên",
                    role = roleName,
                    roleId = roleId,
                    email = GetRowString(row, "Email"),
                    avatarHtml = GetSingleAvatarHtml(u.DisplayName, u.Avatar, u.UserId),
                    isPM = IdNhanVienQuanLy.HasValue && u.UserId == IdNhanVienQuanLy.Value,
                    group = inProject ? "project" : "other",
                    inactive = inactive,
                    original = assigned.Contains(u.UserId),
                    busy = busy,
                    schedule = Newtonsoft.Json.Linq.JObject.Parse(scheduleJson)
                });
            }
            ViewState["TaskMemberTimelineJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                start = StartDate.ToString("yyyy-MM-dd"),
                end = EndDate.ToString("yyyy-MM-dd"),
                displayStart = displayStart.ToString("yyyy-MM-dd"),
                displayEnd = displayEnd.ToString("yyyy-MM-dd"),
                workingDays = LichBieuChungManager.Instance.CountWorkingDaysInRange(StartDate, EndDate),
                readonlyMode = ViewOnly,
                filters = new
                {
                    source = GetSourceFilter(),
                    project = new { keyword = GetKeyword(GetCtx(true)), role = HasChucDanhFilter(GetCtx(true)) ? GetCtx(true).Ddl.SelectedValue : string.Empty },
                    other = new { keyword = GetKeyword(GetCtx(false)), role = HasChucDanhFilter(GetCtx(false)) ? GetCtx(false).Ddl.SelectedValue : string.Empty }
                },
                members = members
            });
        }

        private bool AssignmentChangedWhileOpen()
        {
            var originally = new HashSet<Guid>(ParseGuidIds(ViewState["TaskMemberOriginalSnapshot"] as string));
            var current = new HashSet<Guid>(TaskManager.Instance.GetAssignedNhanVienIds(IdCongViec));
            return !originally.SetEquals(current);
        }

        private List<object> BuildDisplayList(List<AspnetUser> users, Guid? pmId, Dictionary<Guid, DataRow> employeeRows, HashSet<Guid> activeIds, HashSet<Guid> originalAssignedIds, Dictionary<Guid, int> busyDaysDict, Dictionary<Guid, string> scheduleJsonDict)
        {
            var list = new List<object>();

            // TÍNH TỔNG SỐ NGÀY (Y) CỦA TASK ĐANG XÉT
            int totalTaskDays = 0;
            if (StartDate != DateTime.MinValue && EndDate != DateTime.MinValue && EndDate >= StartDate)
            {
                totalTaskDays = LichBieuChungManager.Instance.CountWorkingDaysInRange(StartDate, EndDate);
            }

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                string avatarPath = user.Avatar;
                DataRow info;
                employeeRows.TryGetValue(user.UserId, out info);

                int busyDays = busyDaysDict.ContainsKey(user.UserId) ? busyDaysDict[user.UserId] : 0;

                // TẠO CHUỖI HIỂN THỊ "Bận X/Y ngày"
                int freeDays = Math.Max(0, totalTaskDays - busyDays);
                string busyDaysDisplay = totalTaskDays > 0 ? $"{freeDays}/{totalTaskDays}" : "0/0";

                list.Add(new
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    ChucDanh = GetChucDanh(info),
                    Email = GetRowString(info, "Email"),
                    IsPM = pmId.HasValue && user.UserId == pmId.Value,
                    IsInactive = !activeIds.Contains(user.UserId),
                    AvatarHtml = GetSingleAvatarHtml(user.DisplayName, avatarPath, user.UserId),
                    ScheduleJson = scheduleJsonDict.ContainsKey(user.UserId)
         ? scheduleJsonDict[user.UserId]
         : "{}",
                    IsOriginal = originalAssignedIds.Contains(user.UserId),
                    BusyDays = busyDays,
                    FreeDays = freeDays,
                    BusyDaysDisplay = busyDaysDisplay
                });
            }
            return list;
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.CommandName == "CONFIRM_DISCARD_TASK_ASSIGNMENT_CHANGES")
            {
                if (e.Submit) CloseTaskMemberPickerAfterDiscard();
                else ScriptManager.RegisterStartupScript(Page, GetType(), "TaskMemberDiscardCancelled",
                    "if(window.TaskMemberDiscardGuard)window.TaskMemberDiscardGuard.cancelClose();", true);
                return;
            }
            if (e != null && e.Submit && e.CommandName == "CONFIRM_TASK_ASSIGNMENT_CHANGES")
            {
                List<Guid> finalIds = e.Value as List<Guid>;
                if (finalIds == null || ViewOnly || CURRENT_PAGE == null ||
                    !CURRENT_PAGE.IsUserRight(ActionKeys.Update, CURRENT_PAGE.PAGE_FUNCTION_CODE))
                {
                    ShowAccessDeniedNotify();
                    return;
                }
                TblCongViec task = TaskManager.Instance.FetchById(IdCongViec);
                if (task == null || task.IdDuAn != IdDuAn || AssignmentChangedWhileOpen())
                {
                    ShowNotify("Phân công hoặc công việc đã thay đổi. Vui lòng mở lại cửa sổ để kiểm tra.", MSGType.Error);
                    return;
                }
                IdCongViecCha = task.IdCongViecCha;
                List<Guid> invalidIds;
                if (!IsValidSelectedUserIds(finalIds, out invalidIds))
                {
                    ShowInvalidDataError();
                    return;
                }
                try
                {
                    TaskManager.Instance.UpdateAssignments(IdDuAn, IdCongViec, finalIds);
                    mdlTaskMemberPicker.CloseModal();
                    OnAssignConfirmed?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception exc)
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Có lỗi xảy ra khi lưu phân công.", MSGType.Error);
                    try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(exc, "Lỗi xác nhận thay đổi phân công Task"); } catch { }
                }
            }
        }
    }
}