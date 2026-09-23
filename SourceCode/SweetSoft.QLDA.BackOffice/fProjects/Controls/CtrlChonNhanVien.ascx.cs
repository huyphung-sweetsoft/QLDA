using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using SweetSoft.QLDA.Core.ScheduleManager;
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

        public delegate void ConfirmSelectionHandler(List<Guid> selectedIds);
        public event ConfirmSelectionHandler OnConfirmSelection;

        private const string KEY_LA_NHAN_VIEN = "LaNhanVien";
        private const int MAX_ROWS = 10000; // đủ lớn để lấy hết nhân viên (không phân trang trong popup)

        private bool _searchControlsReady;
        private List<Guid> _pickedForBind = new List<Guid>();

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
      
            _searchControlsReady = true;
        }

        public void OpenPicker()//gọi từ trang cha để mở pop up
        {
            if (!StartDate.HasValue || !EndDate.HasValue)
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

            // Tiêu đề form
            mdlMemberPicker.Title = GetResourceText(BackEndResourceKeys.SELECT_EMPLOYEE);

            // Format InfoNote: "Hiển thị lịch từ {0} đến {1}"
            ltrInfoNote.Text = string.Format(GetResourceText(BackEndResourceKeys.SCHEDULE_INFO_FORMAT),
                StartDate.Value.ToString("dd/MM/yyyy"),
                EndDate.Value.ToString("dd/MM/yyyy"));

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
        private Dictionary<Guid, DataRow> SearchEmployeeRows()
        {
            var result = new Dictionary<Guid, DataRow>();

            Dictionary<string, object> keyValueSearchs = new ControlHelpers().GetControlValues(pnlSearchDefault)
                                                         ?? new Dictionary<string, object>();
            keyValueSearchs[KEY_LA_NHAN_VIEN] = true; // chỉ lấy nhân viên

            int totalRows;
            DataTable dt = UserManager.Instance.SearchUsers(Keyword, keyValueSearchs, "DisplayName ASC", 0, MAX_ROWS, out totalRows);

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
            ViewState["CtrlChonNhanVien_Picked"] = ids == null ? string.Empty : string.Join(",", ids.Select(x => x.ToString()));
        }

        // Cập nhật danh sách đã tick theo những dòng đang hiển thị; dòng đang bị ẩn thì giữ nguyên trạng thái cũ
        private void SyncPickedFromRepeater()
        {
            List<Guid> picked = GetPickedIds();

            foreach (RepeaterItem item in rptCompanyMembers.Items)
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
            SetPickedIds(picked);
        }

        /* ===================== BIND DATA ===================== */

        private void BindData()
        {
            List<AspnetUser> allUsers = UserManager.Instance.GetAllActiveNhanVien();

            // Loại PM ra khỏi danh sách chọn 
            if (IdNhanVienQuanLy.HasValue && IdNhanVienQuanLy.Value != Guid.Empty)
            {
                allUsers = allUsers.Where(u => u.UserId != IdNhanVienQuanLy.Value).ToList();
            }

            // Bỏ những id đã chọn nhưng không còn nằm trong danh sách hợp lệ (nghỉ việc, là PM...)
            HashSet<Guid> validIds = new HashSet<Guid>(allUsers.Select(u => u.UserId));
            List<Guid> picked = GetPickedIds().Where(id => validIds.Contains(id)).ToList();
            SetPickedIds(picked);
            _pickedForBind = picked;

            // Email lấy từ SearchUsers; có keyword / chức danh thì danh sách cũng lọc theo kết quả này
            Dictionary<Guid, DataRow> employeeRows = SearchEmployeeRows();
            if (Keyword.Length > 0 || HasChucDanhFilter)
            {
                allUsers = allUsers.Where(u => employeeRows.ContainsKey(u.UserId)).ToList();
            }

            //THUẬT TOÁN SORT (Người đã được chọn lên đầu -> Còn lại xếp ABC)
            if (picked.Count > 0)
            {
                allUsers = allUsers
                    .OrderByDescending(u => picked.Contains(u.UserId))
                    .ThenBy(u => u.DisplayName)
                    .ToList();
            }
            else
            {
                allUsers = allUsers.OrderBy(u => u.DisplayName).ToList();
            }

            var listMembers = new List<object>();

            // CHUYỂN TỪ FOREACH SANG FOR ĐỂ LẤY INDEX ĐỔI MÀU AVATAR
            for (int i = 0; i < allUsers.Count; i++)
            {
                var user = allUsers[i];
                DataRow info;
                employeeRows.TryGetValue(user.UserId, out info);

                listMembers.Add(new
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    Email = GetRowString(info, "Email"),
                    // BƠM AVATAR VÀO ĐÂY
                    AvatarHtml = GetSingleAvatarHtml(user.DisplayName, user.Avatar, i),
                    ScheduleJson = GenerateScheduleJson(user.UserId, StartDate.Value, EndDate.Value)
                });
            }

            rptCompanyMembers.DataSource = listMembers;
            rptCompanyMembers.DataBind();

            ltrCountCompany.Text = listMembers.Count.ToString();

            ltrNoData.Text = GetResourceText(BackEndResourceKeys.NO_DATA);
            pnlNoData.Visible = listMembers.Count == 0;

            upMemberList.Update();
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

        private string GetSingleAvatarHtml(string name, string avatar, int index)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            string color = colors[index % colors.Length];
            bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                string fallbackHtml = $"<div class=\\'single-avatar-circle\\' style=\\'background-color: {color};\\'>{GetInitials(name)}</div>";
                return $"<img src='{avatarUrl}' class='single-avatar-circle' style='object-fit: cover;' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
            }
            else
            {
                return $"<div class='single-avatar-circle' style='background-color: {color};'>{GetInitials(name)}</div>";
            }
        }
        private string GenerateScheduleJson(Guid userId, DateTime start, DateTime end)
        {
            // DÙNG CHUNG LOGIC LỊCH TRÌNH VỚI BÊN TASK (ĐÃ CÓ TRẠNG THÁI BUSY)
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
                        // Hiển thị số lượng công việc y hệt bên Task
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
            // Gộp trạng thái tick hiện tại vào danh sách đã lưu (gồm cả người đang bị ẩn do lọc)
            SyncPickedFromRepeater();
            List<Guid> tempSelectedIds = GetPickedIds();

            mdlMemberPicker.CloseModal();

            if (OnConfirmSelection != null)
            {
                OnConfirmSelection(tempSelectedIds);
            }
        }
    }
}
