using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.ScheduleManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlChonNhanVien : BaseAdminUserControl
    {
        // 3 thuộc tính này phải nằm trong ViewState: popup có lọc/tìm kiếm nên sẽ có postback riêng,
        // lúc đó trang cha không set lại giá trị nữa (auto-property sẽ bị null)
        //Bốc 2 cái datetime này từ ngày bắt đấu và ngày kết thúc
        public DateTime? StartDate
        {
            get { return ViewState["CtrlChonNhanVien_StartDate"] as DateTime?; }
            set { ViewState["CtrlChonNhanVien_StartDate"] = value; }
        }
        public DateTime? EndDate
        {
            get { return ViewState["CtrlChonNhanVien_EndDate"] as DateTime?; }
            set { ViewState["CtrlChonNhanVien_EndDate"] = value; }
        }
        public Guid? IdNhanVienQuanLy
        {
            get { return ViewState["CtrlChonNhanVien_IdNhanVienQuanLy"] as Guid?; }
            set { ViewState["CtrlChonNhanVien_IdNhanVienQuanLy"] = value; }
        }
        public List<Guid> SelectedUserIds { get; set; }//Danh sách id nhân viên đang chọn (chỉ dùng lúc mở popup)
        public Guid? CurrentIdDuAn
        {
            get { return ViewState["CtrlChonNhanVien_CurrentIdDuAn"] as Guid?; }
            set { ViewState["CtrlChonNhanVien_CurrentIdDuAn"] = value; }
        }
        public List<Guid> OriginalUserIds
        {
            get { return ViewState["CtrlChonNhanVien_OriginalIds"] as List<Guid> ?? new List<Guid>(); }
            set { ViewState["CtrlChonNhanVien_OriginalIds"] = value; }
        }
        public delegate void ConfirmSelectionHandler(List<Guid> selectedIds);
        public event ConfirmSelectionHandler OnConfirmSelection;

        private const string KEY_LA_NHAN_VIEN = "LaNhanVien";
        private const int MAX_ROWS = 10000; // đủ lớn để lấy hết nhân viên (không phân trang trong popup)

        private bool _searchControlsReady;
        private List<Guid> _pickedForBind = new List<Guid>();

        // FindControl cho các ID bổ sung: tương thích WebForms kể cả khi chưa regenerate .designer.cs.
        private static T FindProjectChild<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            if (root.ID == id) return root as T;
            foreach (Control child in root.Controls)
            {
                T found = FindProjectChild<T>(child, id);
                if (found != null) return found;
            }
            return null;
        }
        private T FindPicker<T>(string id) where T : Control { return FindProjectChild<T>(upnlMemberPicker, id); }
        private HiddenField PickedField { get { return FindPicker<HiddenField>("hfPickedClient"); } }
        private HiddenField OriginalIdsField { get { return FindPicker<HiddenField>("hfOriginalClient"); } }
        private HiddenField MiniUserField { get { return FindPicker<HiddenField>("hfMiniUser"); } }
        private HiddenField MiniMonthField { get { return FindPicker<HiddenField>("hfMiniMonth"); } }
        private HiddenField MiniPayloadField { get { return FindPicker<HiddenField>("hfMiniPayload"); } }

        protected void btnLoadMiniMonth_Click(object sender, EventArgs e)
        {
            if (!StartDate.HasValue || !EndDate.HasValue) return;
            SyncPickedFromRepeater();
            Guid userId; DateTime month;
            if (!Guid.TryParse(MiniUserField.Value, out userId) || !DateTime.TryParseExact(MiniMonthField.Value + "-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out month)) return;
            // Chỉ truy vấn một lần cho tháng được yêu cầu và hai tháng lân cận của đúng nhân viên này.
            // Không giới hạn ngày theo thời hạn dự án: lịch mini là lịch thực tế của nhân sự.
            DateTime previousMonth = month.AddMonths(-1);
            DateTime nextMonth = month.AddMonths(1);
            DateTime first = previousMonth.AddDays(-((int)previousMonth.DayOfWeek + 6) % 7);
            DateTime lastMonth = nextMonth.AddMonths(1).AddDays(-1);
            DateTime last = lastMonth.AddDays(6 - ((int)lastMonth.DayOfWeek + 6) % 7);
            AspnetUser user = UserManager.Instance.GetAllActiveNhanVien().FirstOrDefault(x => x.UserId == userId);
            if (user == null || (IdNhanVienQuanLy.HasValue && userId == IdNhanVienQuanLy.Value)) return;
            MiniPayloadField.Value = BuildMonthSchedule(userId, first, last);
            upMemberList.Update();
        }

        protected void Page_Load(object sender, EventArgs e)//Không bind data ở đây: lúc page cha load chưa có data, gọi bind data sẽ gây lỗi
        {
            PrepareSearchControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            // Phòng trường hợp template của modal chưa được tạo ở Page_Load
            PrepareSearchControls();
            base.OnPreRender(e);
        }

        // Nút tìm kiếm phải là async postback (giống trang danh sách nhân viên) để popup không bị load lại
        private void PrepareSearchControls()
        {
            if (_searchControlsReady || lbtSearchSingle == null || txtSearchSingle == null) return;

            System.Web.UI.ScriptManager script = System.Web.UI.ScriptManager.GetCurrent(Page);
            if (script == null) return;

            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            _searchControlsReady = true;
        }

        public void OpenPicker()//gọi từ trang cha để mở pop up
        {
            if (!StartDate.HasValue || !EndDate.HasValue || EndDate.Value.Date < StartDate.Value.Date)
            {
                // Dùng ResourceKey cấu hình cảnh báo
                ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_SELECT_START_AND_END_DATE), MSGType.Error);
                return;
            }

            if (SelectedUserIds == null)
            {
                SelectedUserIds = new List<Guid>();//tạo list rỗng để tránh lỗi 
            }
            SetPickedIds(SelectedUserIds);
            MiniUserField.Value = string.Empty; MiniMonthField.Value = string.Empty; MiniPayloadField.Value = "{}";
            OriginalUserIds = new List<Guid>(SelectedUserIds);
            OriginalIdsField.Value = string.Join(",", OriginalUserIds.Select(x => x.ToString()));
            // Tiêu đề form
            mdlMemberPicker.Title = GetResourceText(BackEndResourceKeys.SELECT_EMPLOYEE);
            btnConfirm.ToolTip = btnConfirm.Text = GetResourceText(BackEndResourceKeys.SAVE);
            // TÍNH TOÁN THỜI HẠN
            int totalTaskDays = 0;
            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value >= StartDate.Value)
            {
                totalTaskDays = LichBieuChungManager.Instance.CountWorkingDaysInRange(StartDate.Value.Date, EndDate.Value.Date);
            }

            // Không còn Timeline theo cửa sổ 28 ngày: chỉ hiển thị thời hạn thật của dự án.
            ltrInfoNote.Text = "<i class='far fa-calendar-alt me-1'></i> Thời gian dự án: <b>" +
                StartDate.Value.ToString("dd/MM/yyyy") + " – " + EndDate.Value.ToString("dd/MM/yyyy") + "</b>" +
                $"<span class='badge ms-2' style='background-color:#dbeafe;color:#1e40af;border:1px solid #bfdbfe;font-size:11px'>Thời hạn: {totalTaskDays} ngày</span>";

            InitSearchControls();
            BindData();
            mdlMemberPicker.OpenModal(true);
            upnlMemberPicker.Update();
        }

        // Reset ô tìm kiếm + dropdown chức danh + tag mỗi lần mở popup
        private void InitSearchControls()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            ddlSearchChucDanh.SearchTagItemText = GetResourceText(BackEndResourceKeys.CHUC_DANH);
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);

            new ControlHelpers().BindChucDanh(ddlSearchChucDanh);
            ddlSearchChucDanh.ClearSelection();
            txtSearchSingle.Text = string.Empty;
            System.Web.UI.ScriptManager.RegisterStartupScript(Page, GetType(), "ClearMemberKeyword",
                string.Format("$('#{0}').val('');", txtSearchSingle.ClientID), true);

            searchTagBox.TagItems.Clear();
            searchTagBox.Update();
            upnlSearchDefault.Update();
            upSearchTagBox.Update();
        }

        /* ===================== ĐIỀU KIỆN LỌC ===================== */

        private bool HasChucDanhFilter
        {
            get
            {
                string value = ddlSearchChucDanh.SelectedValue;
                return !string.IsNullOrEmpty(value) && value != Guid.Empty.ToString();
            }
        }

        private string Keyword
        {
            get { return (txtSearchSingle.Text ?? string.Empty).Trim(); }
        }

        // Gọi SearchUsers giống trang danh sách nhân viên: keyword + chức danh -> DataTable (có Email, TenChucDanh...)
        private Dictionary<Guid, DataRow> SearchEmployeeRows(bool ignoreFilters = false)
        {
            var result = new Dictionary<Guid, DataRow>();

            Dictionary<string, object> keyValueSearchs = ignoreFilters ? new Dictionary<string, object>() : (new ControlHelpers().GetControlValues(pnlSearchDefault)
                                                         ?? new Dictionary<string, object>());
            keyValueSearchs[KEY_LA_NHAN_VIEN] = true; // chỉ lấy nhân viên

            int totalRows;
            DataTable dt = UserManager.Instance.SearchUsers(ignoreFilters ? string.Empty : Keyword, keyValueSearchs, "DisplayName ASC", 0, MAX_ROWS, out totalRows);

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

        /* ===================== LƯU TRẠNG THÁI TICK (kể cả người đang bị ẩn do lọc) ===================== */

        private List<Guid> GetPickedIds()
        {
            var list = new List<Guid>();
            string raw = ViewState["CtrlChonNhanVien_Picked"] as string;
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
            string value = ids == null ? string.Empty : string.Join(",", ids.Distinct().Select(x => x.ToString()));
            ViewState["CtrlChonNhanVien_Picked"] = value;
            if (PickedField != null) PickedField.Value = value;
        }
        // Dùng HiddenField chung từ cả hai cách xem, tránh mất ID khi đang lọc.
        private void SyncPickedFromRepeater()
        {
            if (PickedField == null) return;
            var ids = new List<Guid>();
            foreach (string raw in (PickedField.Value ?? "").Split(','))
            {
                Guid id;
                if (Guid.TryParse(raw, out id) && !ids.Contains(id)) ids.Add(id);
            }
            SetPickedIds(ids);
        }



        /* ===================== BIND DATA ===================== */

        // Cùng cách tính với CtrlChonNhanVienTask: chỉ tính những ngày làm việc
        // có công việc chưa hoàn thành; ngày lễ/cuối tuần không thuộc mẫu số Y.
        // Với popup chọn người cho dự án, KHÔNG loại trừ công việc thuộc dự án
        // hiện tại vì tất cả công việc đang thực hiện đều chiếm lịch của nhân viên.
        private Dictionary<Guid, int> CalculateBusyDaysForProjectPeriod(IEnumerable<AspnetUser> users,
            DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<Guid, int>();
            foreach (AspnetUser user in users)
            {
                var schedule = LichTrinhManager.Instance.LayLichTrinhNhanVien(
                    user.UserId, startDate.Date, endDate.Date);
                int busyDays = 0;
                if (schedule != null)
                {
                    busyDays = schedule
                        .Where(day => day.Ngay.Date >= startDate.Date
                            && day.Ngay.Date <= endDate.Date
                            && day.TrangThaiLich == "busy"
                            && day.DanhSachCongViec != null
                            && day.DanhSachCongViec.Any(task => task.TrangThai != 2 && task.TrangThai != 3))
                        .Select(day => day.Ngay.Date)
                        .Distinct()
                        .Count();
                }
                result[user.UserId] = busyDays;
            }
            return result;
        }

        private void BindData()
        {
            if (!StartDate.HasValue || !EndDate.HasValue) return;
            List<AspnetUser> allUsers = UserManager.Instance.GetAllActiveNhanVien() ?? new List<AspnetUser>();
            if (IdNhanVienQuanLy.HasValue && IdNhanVienQuanLy.Value != Guid.Empty)
                allUsers = allUsers.Where(u => u.UserId != IdNhanVienQuanLy.Value).ToList();
            HashSet<Guid> validIds = new HashSet<Guid>(allUsers.Select(u => u.UserId));
            List<Guid> picked = GetPickedIds().Where(validIds.Contains).Distinct().ToList();
            SetPickedIds(picked);
            _pickedForBind = picked;
            // Thông tin email/chức danh phải tồn tại cho cả người đang chọn, kể cả không khớp lọc.
            Dictionary<Guid, DataRow> allRows = SearchEmployeeRows(true);
            HashSet<Guid> filtered = null;
            if (HasChucDanhFilter || Keyword.Length > 0)
                filtered = new HashSet<Guid>(SearchEmployeeRows(false).Keys);
            List<AspnetUser> visible = allUsers.Where(u => picked.Contains(u.UserId) || filtered == null || filtered.Contains(u.UserId)).ToList();
            Dictionary<Guid, int> tasks = new Dictionary<Guid, int>();
            if (CurrentIdDuAn.HasValue && CurrentIdDuAn.Value != Guid.Empty)
                tasks = TaskManager.Instance.GetTaskCountsByProject(CurrentIdDuAn.Value);
            // Một lần truy vấn chi tiết công việc trong dự án, dùng chung cho mọi badge.
            // Đối chiếu IdNhanVien thực sự được gán, không nhận các task cha chỉ để hiển thị cây.
            Dictionary<Guid, List<object>> tooltipTasks = new Dictionary<Guid, List<object>>();
            if (CurrentIdDuAn.HasValue && CurrentIdDuAn.Value != Guid.Empty && tasks.Count > 0)
            {
                DataTable taskRows = TaskManager.Instance.FetchByIdAndOrderASCMaCV(CurrentIdDuAn.Value);
                if (taskRows != null)
                {
                    foreach (DataRow taskRow in taskRows.Rows)
                    {
                        if (!taskRows.Columns.Contains("IdNhanVien") || taskRow["IdNhanVien"] == DBNull.Value) continue;
                        string idList = Convert.ToString(taskRow["IdNhanVien"]);
                        foreach (string rawId in (idList ?? string.Empty).Split(','))
                        {
                            Guid memberId;
                            if (!Guid.TryParse(rawId.Trim(), out memberId) || !tasks.ContainsKey(memberId)) continue;
                            List<object> memberTasks;
                            if (!tooltipTasks.TryGetValue(memberId, out memberTasks))
                                tooltipTasks[memberId] = memberTasks = new List<object>();
                            memberTasks.Add(new
                            {
                                code = GetRowString(taskRow, "MaCongViec"),
                                name = GetRowString(taskRow, "TenCongViec"),
                                start = FormatTaskDate(taskRow, "NgayBatDau"),
                                end = FormatTaskDate(taskRow, "NgayKetThuc")
                            });
                        }
                    }
                }
            }
            // X/Y dựa trên khoảng thời gian THẬT của dự án, không dựa vào tháng
            // đang mở của lịch mini. Một nhân viên bận nhiều task trong cùng ngày
            // cũng chỉ bị trừ một ngày.
            int totalWorkingDays = Math.Max(0,
                LichBieuChungManager.Instance.CountWorkingDaysInRange(StartDate.Value.Date, EndDate.Value.Date));
            Dictionary<Guid, int> busyDaysByUser = CalculateBusyDaysForProjectPeriod(
                visible, StartDate.Value.Date, EndDate.Value.Date);

            HashSet<Guid> pickedSet = new HashSet<Guid>(picked);
            visible = visible.OrderByDescending(u => pickedSet.Contains(u.UserId))
                .ThenBy(u => u.DisplayName).ThenBy(u => u.UserId).ToList();
            var list = new List<object>();
            foreach (var user in visible)
            {
                DataRow info; allRows.TryGetValue(user.UserId, out info);
                int count = tasks.ContainsKey(user.UserId) ? tasks[user.UserId] : 0;
                int busyDays = busyDaysByUser.ContainsKey(user.UserId) ? busyDaysByUser[user.UserId] : 0;
                int freeDays = Math.Max(0, totalWorkingDays - Math.Min(totalWorkingDays, busyDays));
                list.Add(new
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    Email = GetRowString(info, "Email"),
                    ChucDanh = GetRowString(info, "TenChucDanh"),
                    AvatarHtml = GetSingleAvatarHtml(user.DisplayName, user.Avatar, user.UserId),
                    IsOriginal = OriginalUserIds.Contains(user.UserId),
                    IsPicked = picked.Contains(user.UserId),
                    TaskCount = count,
                    BusyDays = busyDays,
                    FreeDays = freeDays,
                    TotalWorkingDays = totalWorkingDays,
                    TaskTooltipJson = JsonConvert.SerializeObject(tooltipTasks.ContainsKey(user.UserId) ? tooltipTasks[user.UserId] : new List<object>())
                });
            }
            rptCompanyMembers.DataSource = list; rptCompanyMembers.DataBind();
            ltrCountCompany.Text = list.Count.ToString();
            ltrNoData.Text = GetResourceText(BackEndResourceKeys.NO_DATA);
            pnlNoData.Visible = list.Count == 0;
            MiniUserField.Value = string.Empty; MiniMonthField.Value = string.Empty; MiniPayloadField.Value = "{}";
            upMemberList.Update();
        }



        private static string FormatTaskDate(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return "Chưa xác định";
            DateTime date;
            return DateTime.TryParse(Convert.ToString(row[column]), out date) ? date.ToString("dd/MM/yyyy") : "Chưa xác định";
        }

        /* ===================== SỰ KIỆN TÌM KIẾM / LỌC / TAG ===================== */

        protected void ddlSearchChucDanh_SelectedValueChanged(object sender, EventArgs e)
        {
            RefreshFilteredList();
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            RefreshFilteredList();
        }

        protected void searchTagBox_TagClosed(object sender, SweetSoft.QLDA.Controls.SearchTagItem tag)
        {
            try
            {
                if (tag != null && tag.Key == txtSearchSingle.ClientID)
                {
                    txtSearchSingle.Text = string.Empty;
                    // Ô nhập nằm ngoài UpdatePanel nên phải xóa cả phía client
                    System.Web.UI.ScriptManager.RegisterClientScriptBlock(Page, GetType(), "ClearMemberKeyword",
                        string.Format("$('#{0}').val('');", txtSearchSingle.ClientID), true);
                }
                else
                {
                    ddlSearchChucDanh.ClearSelection();
                    upnlSearchDefault.Update();
                }
                RefreshFilteredList();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void RefreshFilteredList()
        {
            try
            {
                SyncPickedFromRepeater(); // giữ lại các nhân viên đã tick trước khi danh sách bị lọc
                UpdateSearchTags();
                BindData();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        // Dựng tag dưới ô tìm kiếm (giống MasterTemplate.GetValueForExtraSearchBox nhưng chỉ có 2 điều kiện)
        private void UpdateSearchTags()
        {
            searchTagBox.TagItems.Clear();

            if (HasChucDanhFilter && ddlSearchChucDanh.SearchTagItem != null)
                searchTagBox.TagItems.Add(ddlSearchChucDanh.SearchTagItem);

            if (Keyword.Length > 0 && txtSearchSingle.SearchTagItem != null)
                searchTagBox.TagItems.Add(txtSearchSingle.SearchTagItem);

            searchTagBox.Update();
            searchTagBox.Visible = true;
            upSearchTagBox.Update();
        }

        /* ===================== AVATAR + LỊCH ===================== */

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string GetSingleAvatarHtml(string name, string avatar, Guid id)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            uint hash = 2166136261;
            foreach (char ch in id.ToString("N")) hash = unchecked((hash ^ (uint)ch) * 16777619);
            string color = colors[(int)(hash % (uint)colors.Length)];
            string safeInitials = System.Web.HttpUtility.HtmlEncode(GetInitials(name));
            string fallback = "<span class='single-avatar-circle' style='background:" + color + "'>" + safeInitials + "</span>";
            bool isDefault = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
            if (isDefault) return fallback;
            string url = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
            string safeUrl = System.Web.HttpUtility.HtmlAttributeEncode(url);
            return "<img src=\"" + safeUrl + "\" class=\"single-avatar-circle\" alt=\"Avatar\" onerror=\"this.onerror=null;this.style.display='none';this.nextElementSibling.style.display='inline-flex';\" />" +
                "<span class='single-avatar-circle' style='display:none;background:" + color + "'>" + safeInitials + "</span>";
        }


        // Chỉ truy vấn lịch khi người dùng mở lịch mini / tới tháng chưa có cache.
        // Tải tháng trước + tháng hiện tại + tháng sau (kèm ô ngày giáp tháng) cho đúng một nhân viên.
        private string BuildMonthSchedule(Guid userId, DateTime start, DateTime end)
        {
            var days = LichTrinhManager.Instance.LayLichTrinhNhanVien(userId, start, end);
            var result = new Dictionary<string, object>();
            if (days == null) return "{}";
            foreach (var day in days)
            {
                string status = day.TrangThaiLich ?? "free";
                var activeTasks = day.DanhSachCongViec == null ? null :
                    day.DanhSachCongViec.Where(t => t.TrangThai != 2 && t.TrangThai != 3).ToList();
                int count = activeTasks == null ? 0 : activeTasks.Count;
                var tasks = activeTasks == null ? new object[0] : activeTasks.Select(t => (object)new
                {
                    code = t.MaCongViec,
                    name = t.TenCongViec,
                    projectName = t.TenDuAn,
                    currentProject = CurrentIdDuAn.HasValue && t.IdDuAn == CurrentIdDuAn.Value
                }).ToArray();
                string label = status == "holiday" ? "Nghỉ lễ" : status == "weekend" ? "Nghỉ" : count > 0 ? count + " công việc" : "Trống";
                if (count > 0) status = "busy";
                else if (status == "busy") status = "free";
                result[day.Ngay.ToString("yyyy-MM-dd")] = new { status = status, text = label, tasks = tasks };
            }
            return JsonConvert.SerializeObject(result);
        }

        protected void rptCompanyMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                HiddenField hdfUserId = (HiddenField)e.Item.FindControl("hdfUserId");
                CheckBox chkSelect = (CheckBox)e.Item.FindControl("chkSelect");

                if (hdfUserId != null && chkSelect != null)
                {
                    Guid currentUserId = Guid.Parse(hdfUserId.Value);

                    if (_pickedForBind.Contains(currentUserId))
                    {
                        chkSelect.Checked = true;
                    }
                }
            }
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            SyncPickedFromRepeater();
            List<Guid> tempSelectedIds = GetPickedIds();

            // Lọc ra nhân viên cũ bị kick
            List<Guid> removedIds = OriginalUserIds.Where(id => !tempSelectedIds.Contains(id)).ToList();

            if (removedIds.Count > 0 && CurrentIdDuAn.HasValue)
            {
                Dictionary<Guid, int> taskCounts = TaskManager.Instance.GetTaskCountsByProject(CurrentIdDuAn.Value);
                List<string> warningDetails = new List<string>();

                foreach (Guid rId in removedIds)
                {
                    if (taskCounts.ContainsKey(rId) && taskCounts[rId] > 0)
                    {
                        AspnetUser u = UserManager.Instance.GetUserById(rId);
                        string name = System.Web.HttpUtility.HtmlEncode(u != null ? u.DisplayName : "Nhân viên");

                        string taskText = string.Format(GetResourceText(BackEndResourceKeys.DOING_X_TASKS) ?? "Đảm nhận {0} công việc", taskCounts[rId]);
                        warningDetails.Add($"• <b>{name}</b> ({taskText})");
                    }
                }

                // Nếu có người vướng Task -> Gọi MessageBox hệ thống
                if (warningDetails.Count > 0)
                {
                    string detailHtml = string.Join("<br/>", warningDetails);
                    string formattedList = $"<div style='text-align: left; background: #fffbeb; padding: 10px; border: 1px solid #fde68a; border-radius: 6px; font-size: 13px; margin-bottom: 10px;'>{detailHtml}</div>";

                    string rawMessage = GetResourceText(BackEndResourceKeys.WARNING_REMOVE_ACTIVE_MEMBERS_MSG) ??
                                        "Những nhân viên sau hiện đang đảm nhiệm công việc trong dự án:<br/><br/>{0}<br/>Việc loại bỏ họ sẽ khiến các công việc trên bị trống người thực hiện. Bạn có chắc chắn muốn tiếp tục xóa?";
                    string finalMessage = string.Format(rawMessage, formattedList);

                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "CONFIRM_REMOVE_PROJECT_MEMBERS";
                    result.Value = tempSelectedIds; // Gói danh sách đã tick vào đây
                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.PERSONNEL_WARNING) ?? "Cảnh báo nhân sự", finalMessage, MSGButton.DeleteCancel, MSGIcon.Warning);
                    OpenMessageBox(msg, result, false, false);

                    return; // Đứng lại chờ User xác nhận
                }
            }

            // Nếu an toàn, lưu ngay lập tức
            mdlMemberPicker.CloseModal();
            if (OnConfirmSelection != null)
            {
                OnConfirmSelection(tempSelectedIds);
            }
        }
        // Bắt tín hiệu "Đồng ý Xóa" từ MessageBox
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName == "CONFIRM_REMOVE_PROJECT_MEMBERS")
            {
                List<Guid> finalIds = e.Value as List<Guid>;
                if (finalIds != null)
                {
                    // 1. Cập nhật trạng thái đóng trên Server
                    mdlMemberPicker.CloseModal();

                    // 2. Trả danh sách về để update UI Form Dự Án (Nó sẽ tự lo việc hiển thị)
                    if (OnConfirmSelection != null)
                    {
                        OnConfirmSelection(finalIds);
                    }
                }
            }
        }
    }
}
