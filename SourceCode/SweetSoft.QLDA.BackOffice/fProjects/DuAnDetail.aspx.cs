using SweetCMS.Controls.Helpers;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fProjects.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects
{
    public partial class DuAnDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Project;
            }
        }

        protected bool IsContractView
        {
            get
            {
                if (this.IsUserRight(ActionKeys.View, ModuleKeys.Contract))
                    return true;
                return false;
            }
        }

        private Guid IdHopDongThucHien
        {
            get
            {
                if (ViewState["IdHopDongThucHien"] != null)
                    return (Guid)ViewState["IdHopDongThucHien"];

                return Guid.Empty;
            }
            set
            {
                ViewState["IdHopDongThucHien"] = value;
            }
        }

        private Guid QueryId
        {
            get
            {
                try
                {
                    string temp = CommonHelpers.QueryString("ProjectId");
                    if (string.IsNullOrEmpty(temp))
                        return Guid.Empty;
                    return Guid.Parse(SecurityUtilities.UnprotectUrlParameter(temp));
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        private AuditManager _auditManager;
        protected byte CurrentStatusValue { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlGiaiDoanDuAn1.IdDuAn = QueryId;
            CtrlLichSuDuAn1.IdDuAn = QueryId;
            pnlContract.Visible = this.IsContractView;
            if (_auditManager == null)
                _auditManager = new AuditManager(new Core.SysManager.Models.ClientInfo()
                {
                    UserId = SweetContext.Current.UserId, IpAddress = SweetContext.Current.CurrentUserIp, UserAgent = SweetContext.Current.CurrentUserAgent
                });
            CtrlProjectTabs1.ProjectId = QueryId;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_LIST));
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Projects, GetResourceText(BackEndResourceKeys.PROJECT_LIST) }, {"javascript:", GetResourceText(BackEndResourceKeys.DETAIL) }
                };
                if (this.QueryId != Guid.Empty)
                {
                    BindData();
                    CtrlGiaiDoanDuAn1.InitControls();
                }
            }
        }

        protected override void BindData()
        {
            try
            {
                DataTable dt = DuAnManager.Instance.GetDetailDuAnById(QueryId);
                if (dt == null || dt.Rows.Count == 0)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                    return;
                }
                DataRow row = dt.Rows[0];
                BindProjectInformation(row);
                BindProjectProgress();
                BindProjectTeam();
                BindRecentProjectHistory();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void rptRecentProjectHistory_ItemDataBound( object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row = e.Item.DataItem as DataRowView;

            if (row == null)
                return;

            Label lblHistoryContent = e.Item.FindControl( "lblHistoryContent") as Label;

            Label lblHistoryTime = e.Item.FindControl( "lblHistoryTime") as Label;

            if (lblHistoryContent != null)
            {
                lblHistoryContent.Text = HttpUtility.HtmlEncode( BuildHistoryContent( row.Row));
            }

            if (lblHistoryTime != null)
            {
                lblHistoryTime.Text = FormatHistoryTime( row.Row);
            }
        }

        protected void lbtViewAllHistory_Click( object sender, EventArgs e)
        {
            CtrlLichSuDuAn1.IdDuAn = QueryId;

            CtrlLichSuDuAn1.OpenDrawer();
        }

        protected void lbtViewContract_Click(
    object sender,
    EventArgs e)
        {
            if (!this.IsView)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (this.IdHopDongThucHien ==
                Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblHopDongThucHien hopDong =
                HopDongThucHienManager
                    .Instance
                    .GetHopDongById(
                        this.IdHopDongThucHien);

            if (hopDong == null)
            {
                ShowInvalidNotFoundData();
                return;
            }

            BindContractInformation(hopDong);

            dlContractDetail.Title =
                "Thông tin hợp đồng thực hiện";

            dlContractDetail.CloseText =
                GetResourceText(
                    BackEndResourceKeys.CLOSE);

            dlContractDetail.OpenModal(true);
        }


        private void BindContractInformation(TblHopDongThucHien hopDong)
        {
            txtContractNumber.Text = hopDong.SoHopDong;
            txtContractName.Text = hopDong.TenHopDong;

            TblKhachHang khachHang = KhachHangManager.Instance.GetKhachHangById(hopDong.IdKhachHang);
            txtContractCustomer.Text = khachHang == null ? "Chưa có" : khachHang.TenKhachHang;
            txtContractValue.Text = hopDong.GiaTriHopDong.HasValue
                ? FormatHelpers.ConvertDecimalToStringByLanguage(hopDong.GiaTriHopDong.Value, "vi-VN")
                : string.Empty;
            txtContractSignDate.Text = hopDong.NgayKy.HasValue
                ? hopDong.NgayKy.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractEffectiveDate.Text = hopDong.NgayHieuLuc.HasValue
                ? hopDong.NgayHieuLuc.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractExpiryDate.Text = hopDong.NgayHetHan.HasValue
                ? hopDong.NgayHetHan.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractDescription.Text = hopDong.MoTa;
        }


        

        protected void lbtOpenContractDocument_Click(object sender, EventArgs e)
        {
            if (!IsContractView)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (IdHopDongThucHien == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                ContractDocumentLinkResult result = HopDongThucHienManager
                    .Instance
                    .GetOrCreateProjectDocument(IdHopDongThucHien);

                string url = RewriteURLHelper.ProjectDocumentDetail(
                    result.ProjectId,
                    result.DocumentId) + "?tab=versions";
                Response.Redirect(GetRelativeClientPath(url), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (UnauthorizedAccessException)
            {
                ShowAccessDeniedNotify();
            }
            catch (InvalidOperationException exception)
            {
                ShowNotify(exception.Message, MSGType.Warning);
            }
            catch (Exception exception)
            {
                ShowNotify(exception.Message, MSGType.Error);
            }
        }
        private string BuildHistoryContent(DataRow row)
        {
            string resourceKey = GetColumnText(row, "Description");

            string actor = GetColumnText(row, "ChangedBy");

            string tableName = GetColumnText(row, "TableName");

            string title = GetColumnText(row, "Title");

            if (string.IsNullOrWhiteSpace(actor) || string.Equals( actor, "[System]", StringComparison.OrdinalIgnoreCase))
            {
                actor = "Hệ thống";
            }

            if (string.IsNullOrWhiteSpace(title))
                title = "Chưa xác định";

            string entityName = GetHistoryEntityName( tableName);

            string template = string.IsNullOrWhiteSpace(resourceKey)
                    ? null
                    : GetResourceText(resourceKey);

            if (string.IsNullOrWhiteSpace(template) || string.Equals( template, resourceKey, StringComparison.OrdinalIgnoreCase))
            {
                return BuildDefaultHistoryContent( row, actor, entityName, title);
            }

            bool isContainerAction = string.Equals(resourceKey, BackEndResourceKeys.HISTORY_ADDED_TO_CONTAINER, StringComparison.OrdinalIgnoreCase) || string.Equals(resourceKey, BackEndResourceKeys.HISTORY_REMOVED_FROM_CONTAINER, StringComparison.OrdinalIgnoreCase);
            if (isContainerAction)
            {
                string containerName = GetResourceText(BackEndResourceKeys.PROJECT);
                return string.Format( template, actor,          // {0}: Administrator
                    title,          // {1}: Nguyễn Thị A
                    containerName);
            }

            try
            {
                return string.Format( template, actor, entityName, title);
            }
            catch (FormatException)
            {
                return BuildDefaultHistoryContent( row, actor, entityName, title);
            }
        }

        private string GetHistoryEntityName( string tableName)
        {
            switch (tableName)
            {
                case nameof(TblDuAn): return "dự án";

                case nameof(TblGiaiDoanDuAn): return "giai đoạn";

                case nameof(TblCongViec): return "công việc";

                case nameof(TblThanhVienDuAn): return "thành viên dự án";

                case nameof(TblHopDongThucHien): return "hợp đồng thực hiện";

                default: return "thông tin";
            }
        }

        private string BuildDefaultHistoryContent( DataRow row, string actor, string entityName, string title)
        {
            string actionType = GetColumnText( row, "ActionType");

            switch (actionType)
            {
                case "CREATE": return string.Format( "{0} đã thêm {1} \"{2}\".", actor, entityName, title);

                case "UPDATE": return string.Format( "{0} đã cập nhật {1} \"{2}\".", actor, entityName, title);

                case "DELETE": return string.Format( "{0} đã xóa {1} \"{2}\".", actor, entityName, title);

                default: return string.Format( "{0} đã thao tác trên {1} \"{2}\".", actor, entityName, title);
            }
        }

        private string FormatHistoryTime( DataRow row)
        {
            if (!row.Table.Columns.Contains( "ChangedAt") || row["ChangedAt"] == null || row["ChangedAt"] == DBNull.Value)
            {
                return string.Empty;
            }

            DateTime changedAt = Convert.ToDateTime( row["ChangedAt"]);

            return changedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        }

        private string GetColumnText( DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains( columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString( row[columnName]);
        }

        private void BindProjectProgress()
        {
            var tienDo = SweetSoft.QLDA.Core.Managers.DuAnManager.Instance.GetDuAnTienDo(QueryId);

            if (tienDo.TienDoThoiGian.HasValue)
            {
                int phanTramThoiGian = (int)Math.Round(tienDo.TienDoThoiGian.Value);
                lblTienDoThoiGian.InnerText = phanTramThoiGian + "%";
                divTienDoThoiGian.Attributes["aria-valuenow"] = phanTramThoiGian.ToString();
                divTienDoThoiGianBar.Style["width"] = phanTramThoiGian + "%";
            }
            else
            {
                lblTienDoThoiGian.InnerText = "—";
                divTienDoThoiGian.Attributes["aria-valuenow"] = "0";
                divTienDoThoiGianBar.Style["width"] = "0%";
            }

            if (tienDo.TienDoCongViec.HasValue)
            {
                int phanTramCongViec = (int)Math.Round(tienDo.TienDoCongViec.Value);
                lblTienDoCongViec.InnerText = phanTramCongViec + "%";
                divTienDoCongViec.Attributes["aria-valuenow"] = phanTramCongViec.ToString();
                divTienDoCongViecBar.Style["width"] = phanTramCongViec + "%";
            }
            else
            {
                lblTienDoCongViec.InnerText = "—";
                divTienDoCongViec.Attributes["aria-valuenow"] = "0";
                divTienDoCongViecBar.Style["width"] = "0%";
            }
        }

        private void BindProjectTeam()
        {
            // 1. Get PM ID
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(QueryId);

            if (pmId.HasValue && pmId.Value != Guid.Empty)
            {
                var pm = UserManager.Instance.GetUserById(pmId.Value);
                if (pm != null)
                {
                    lblNhanVienQuanLy.Text = pm.DisplayName;
                    imgAvatarPM.Src = !string.IsNullOrEmpty(pm.Avatar) && !pm.Avatar.Contains("no-file.png") ? pm.Avatar : "/Styles/images/user-icon.png";
                }
                else
                {
                    lblNhanVienQuanLy.Text = "Không xác định";
                    imgAvatarPM.Src = "/Styles/images/user-icon.png";
                }
            }
            else
            {
                lblNhanVienQuanLy.Text = "Chưa có Quản lý";
                imgAvatarPM.Src = "/Styles/images/user-icon.png";
            }

            // 2. Get Members
            List<Guid> memberIds = SweetSoft.QLDA.Core.Managers.ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(QueryId);
            
            // Remove PM from members list if present
            if (pmId.HasValue)
            {
                memberIds.RemoveAll(id => id == pmId.Value);
            }

            // Distinct IDs to avoid duplicates
            memberIds = memberIds.Distinct().ToList();

            if (memberIds.Count == 0)
            {
                ltrThanhVienGroup.Text = "";
                return;
            }

            // Get User objects
            List<SweetSoft.QLDA.DataAccess.AspnetUser> members = new List<SweetSoft.QLDA.DataAccess.AspnetUser>();
            foreach(var id in memberIds)
            {
                var user = SweetSoft.QLDA.Core.Managers.UserManager.Instance.GetUserById(id);
                if (user != null)
                {
                    members.Add(user);
                }
            }

            // Render Avatar Group
            int maxVisible = 6;
            int hiddenCount = members.Count > maxVisible ? members.Count - maxVisible : 0;
            var visibleMembers = members.Take(maxVisible).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"d-flex align-items-center flex-wrap gap-2\">");
            sb.Append("<div class=\"avatar-group\">");
            sb.Append("<div class=\"avatar-stack-container\">");

            foreach (var member in visibleMembers)
            {
                bool hasAvatar = !string.IsNullOrEmpty(member.Avatar) && !member.Avatar.Contains("no-file.png") && !member.Avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
                string title = member.DisplayName;

                if (hasAvatar)
                {
                    string avatarUrl = member.Avatar.StartsWith("~")
                        ? Page.ResolveUrl(member.Avatar)
                        : member.Avatar;

                    string safeAvatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                    string safeTitle = HttpUtility.HtmlAttributeEncode(title);

                    sb.Append(
                        $"<img src=\"{safeAvatarUrl}\" " +
                        $"class=\"avatar-circle\" " +
                        $"style=\"object-fit: cover;\" " +
                        $"title=\"{safeTitle}\" />"
                    );
                }
                else
                {
                    sb.Append(GetFallbackAvatarHtml(title));
                }
            }

            sb.Append("</div>"); // close avatar-stack-container

            if (hiddenCount > 0)
            {
                sb.Append($"<div class=\"avatar-circle avatar-more\">+{hiddenCount}</div>");
            }

            sb.Append("</div>"); // close avatar-group
            sb.Append("</div>"); // close d-flex gap-2

            ltrThanhVienGroup.Text = sb.ToString();
        }

        private string GetFallbackAvatarHtml(string fullName)
        {
            string initials = GetInitials(fullName);
            string bgColor = GetAvatarBackground(fullName);
            string safeName = HttpUtility.HtmlAttributeEncode(fullName);

            return $"<div class=\"avatar-circle\" " +
                   $"style=\"background-color: {bgColor};\" " +
                   $"title=\"{safeName}\">{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] words = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
            {
                return words[0].Substring(0, 1).ToUpper();
            }
            else if (words.Length > 1)
            {
                return (words[0].Substring(0, 1) + words[words.Length - 1].Substring(0, 1)).ToUpper();
            }
            return "?";
        }

        private string GetAvatarBackground(string fullName)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899", "#ef4444", "#06b6d4" };
            if (string.IsNullOrWhiteSpace(fullName)) return colors[0];
            
            int hash = Math.Abs(fullName.GetHashCode());
            return colors[hash % colors.Length];
        }

        private void BindProjectInformation(DataRow row)
        {
            Navigation1.MainTitle = GetDisplayText(row, "MaDuAn");
            lblTenDuAn.Text = GetDisplayText(row, "TenDuAn");
            lblKhachHang.Text = GetDisplayText(row, "TenKhachHang");
            lblLoaiDuAn.Text = GetDisplayText(row, "TenLoaiDuAn");
            lblSoHopDong.Text = GetDisplayText(row, "SoHopDong");
            lblGiaTriHopDong.Text = FormatMoney(row, "GiaTriHopDong");
            lblNgayKy.Text = FormatDate(row, "NgayKy");
            lblNgayBatDau.Text = FormatDate(row, "NgayBatDau");
            lblNgayHoanThanhDuKien.Text = FormatDate(row, "NgayDuKienHoanThanh");
            lblNgayHoanThanhThucTe.Text = FormatDate(row, "NgayHoanThanhThucTe");
            ltrMoTa.Text = GetHtmlText(row, "MoTa");
            lblNhanVienQuanLy.Text = GetDisplayText(row, "DisplayName");

            string avatarUrl = Convert.ToString(row["Avatar"]);
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                imgAvatarPM.Src = avatarUrl;
            }
            else
            {
                imgAvatarPM.Src = "~/Styles/images/user-icon.png";
            }

            byte trangThai = Convert.ToByte(row["TrangThai"]);
            lblTrangThai.Text = Convert.ToString(EnumHelpers.GetERenderText(typeof(DuAnStatus), (DuAnStatus)trangThai));
            CurrentStatusValue = trangThai;
            DuAnStatus statusEnum = (DuAnStatus)trangThai;
            ltrCurrentStatusName.Text = EnumHelpers.GetERenderText(typeof(DuAnStatus), statusEnum);
            iCurrentStatusIcon.Attributes["class"] = "fas fa-circle me-2 small " + GetStatusCssClass(statusEnum);
            BindStatusDropdown();

            Guid idHopDongThucHien = Guid.Empty;
            if (row.Table.Columns.Contains(
        "IdHopDongThucHien") &&
    row["IdHopDongThucHien"] !=
        DBNull.Value)
            {
                Guid.TryParse(
                    Convert.ToString(
                        row["IdHopDongThucHien"]),
                    out idHopDongThucHien);
            }

            this.IdHopDongThucHien =
                idHopDongThucHien;

            lbtViewContract.Visible =
                idHopDongThucHien != Guid.Empty;

            lblNoContract.Visible =
                idHopDongThucHien == Guid.Empty;
        }

        private bool CanOpenContractDocument(Guid idHopDongThucHien)
        {
            if (!IsContractView ||
                idHopDongThucHien == Guid.Empty ||
                QueryId == Guid.Empty ||
                !DocumentManager.Instance.CanAccessProjectDocument(
                    QueryId,
                    ActionKeys.View))
            {
                return false;
            }

            if (HopDongThucHienManager.Instance.HasLinkedDocument(
                idHopDongThucHien))
            {
                return true;
            }

            return DocumentManager.Instance.CanAccessProjectDocument(
                QueryId,
                ActionKeys.Create);
        }

        private string GetDisplayText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "Chưa có";
            }
            return HttpUtility.HtmlEncode(value);
        }

        private string GetHtmlText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "<p class='text-muted'>Chưa có nội dung mô tả</p>";
            }
            return HttpUtility.HtmlDecode(value);
        }

        private string FormatDate(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return "Chưa có";
            }
            DateTime value = Convert.ToDateTime(row[columnName]);
            return DateTimeHelper.ConvertDateTime(value, false);
        }

        private string FormatMoney(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return "Chưa có";
            }
            decimal value = Convert.ToDecimal(row[columnName]);
            return FormatHelpers.ConvertDecimalToStringByLanguage(value, "vi-VN");
        }

        private int CalculateProgressByDay(DataRow row)
        {
            if (!row.Table.Columns.Contains("NgayBatDau") || row["NgayBatDau"] == DBNull.Value)
                return 0;
            if (!row.Table.Columns.Contains("NgayDuKienHoanThanh") || row["NgayDuKienHoanThanh"] == DBNull.Value)
                return 0;
            DateTime ngayBatDau = Convert.ToDateTime(row["NgayBatDau"]);
            DateTime ngayDuKien = Convert.ToDateTime(row["NgayDuKienHoanThanh"]);
            DateTime ngayHienTai = DateTime.Now;
            double tongNgay = (ngayDuKien - ngayBatDau).TotalDays;
            if (tongNgay <= 0) return 100;
            double ngayDaQua = (ngayHienTai - ngayBatDau).TotalDays;
            int phanTram = (int)Math.Round((ngayDaQua / tongNgay) * 100);
            return Math.Max(0, Math.Min(100, phanTram));
        }

        private void BindRecentProjectHistory()
        {
            if (rptRecentProjectHistory == null)
            {
                throw new InvalidOperationException( "Không tìm thấy control rptRecentProjectHistory trong DuAnDetail.aspx.");
            }

            if (pnlEmptyRecentHistory == null)
            {
                throw new InvalidOperationException( "Không tìm thấy control pnlEmptyRecentHistory trong DuAnDetail.aspx.");
            }

            DataTable history = _auditManager.GetRecentProjectHistory( QueryId, 5);

            bool hasData = history != null && history.Rows.Count > 0;

            rptRecentProjectHistory.Visible = hasData;

            pnlEmptyRecentHistory.Visible = !hasData;

            if (!hasData)
            {
                rptRecentProjectHistory.DataSource = null;

                rptRecentProjectHistory.DataBind();
                return;
            }

            rptRecentProjectHistory.DataSource = history;

            rptRecentProjectHistory.DataBind();
        }

        protected void rptStatusDropdown_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "ChangeStatus")
                return;

            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            byte trangThai;

            if (!byte.TryParse(e.CommandArgument.ToString(), out trangThai) ||
                !Enum.IsDefined(typeof(DuAnStatus), trangThai))
            {
                ShowNotify(
                    GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE),
                    MSGType.Error
                );
                return;
            }

            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(QueryId);

            if (duAn == null || duAn.DaXoa)
            {
                ShowInvalidNotFoundData();
                return;
            }

            if (duAn.TrangThai == trangThai)
                return;

            duAn.TrangThai = trangThai;

            DuAnManager.Instance.CreateOrUpdate(duAn);

            // POST -> GET
            Response.Redirect(Request.RawUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void BindStatusDropdown()
        {
            var statuses = Enum.GetValues(typeof(DuAnStatus)).Cast<DuAnStatus>().Select(s => new {
                Value = (byte)s,
                Name = EnumHelpers.GetERenderText(typeof(DuAnStatus), s),
                CssClass = GetStatusCssClass(s)
            }).ToList();
            
            rptStatusDropdown.DataSource = statuses;
            rptStatusDropdown.DataBind();
        }

        protected string GetStatusCssClass(DuAnStatus status)
        {
            switch (status)
            {
                case DuAnStatus.ChoThucHien: return "text-warning";
                case DuAnStatus.DangThucHien: return "text-info";
                case DuAnStatus.TamDung: return "text-secondary";
                case DuAnStatus.HoanThanh: return "text-success";
                case DuAnStatus.KetThuc: return "text-dark";
                default: return "text-primary";
            }
        }
    }
}

