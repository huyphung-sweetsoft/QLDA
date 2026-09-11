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

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlGiaiDoanDuAn1.IdDuAn = QueryId;
            CtrlLichSuDuAn1.IdDuAn = QueryId;
            if (_auditManager == null)
                _auditManager = new AuditManager(new Core.SysManager.Models.ClientInfo()
                {
                    UserId = SweetContext.Current.UserId, IpAddress = SweetContext.Current.CurrentUserIp, UserAgent = SweetContext.Current.CurrentUserAgent
                });
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

        private string BuildHistoryContent( DataRow row)
        {
            string resourceKey = GetColumnText( row, "Description");

            string actor = GetColumnText( row, "ChangedBy");

            string tableName = GetColumnText( row, "TableName");

            string title = GetColumnText( row, "Title");

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

                case nameof(TblHopDongThucHien): return "hợp đồng";

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
            lblTrangThai.Text = Convert.ToString(EnumHelpers.GetERenderText(typeof(DuAnStatus), trangThai));
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
    }
}