using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlLichSuDuAn : BaseAdminUserControl
    {
        public Guid IdDuAn
        {
            get
            {
                if (ViewState["IdDuAn"] == null)
                    return Guid.Empty;
                return (Guid)ViewState["IdDuAn"];
            }
            set
            {
                ViewState["IdDuAn"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void OpenDrawer()
        {
            BindHistoryUsers();
            BindHistory();
            ShowDrawer();
        }

        private void BindHistory()
        {
            Guid? userId =
                GetSelectedUserId();

            DateTime? fromDate =
                ParseDate(
                    txtHistoryFromDate.Text);

            DateTime? toDate =
                ParseDate(
                    txtHistoryToDate.Text);

            DataTable history =
                DuAnManager.Instance
                    .GetProjectHistory(
                        IdDuAn,
                        userId,
                        fromDate,
                        toDate);

            bool hasData =
                history != null &&
                history.Rows.Count > 0;

            rptProjectHistory.Visible =
                hasData;

            pnlEmptyHistory.Visible =
                !hasData;

            rptProjectHistory.DataSource =
                hasData
                    ? history
                    : null;

            rptProjectHistory.DataBind();

            upnlProjectHistory.Update();
        }

        private void BindHistoryUsers()
        {
            string selectedValue =
                ddlHistoryUser.SelectedValue;

            DataTable history =
                DuAnManager.Instance
                    .GetProjectHistory(
                        IdDuAn);

            ddlHistoryUser.Items.Clear();

            ddlHistoryUser.Items.Add(
                new ListItem(
                    "Tất cả người thực hiện",
                    string.Empty));

            if (history == null)
                return;

            HashSet<Guid> addedUsers =
                new HashSet<Guid>();

            foreach (DataRow row
                in history.Rows)
            {
                Guid userId;

                if (!TryGetGuid(
                        row,
                        "UserId",
                        out userId))
                {
                    continue;
                }

                if (!addedUsers.Add(userId))
                    continue;

                string displayName =
                    GetColumnText(
                        row,
                        "ChangedBy");

                if (string.IsNullOrWhiteSpace(
                        displayName))
                {
                    displayName =
                        userId.ToString();
                }

                ddlHistoryUser.Items.Add(
                    new ListItem(
                        displayName,
                        userId.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(
                    selectedValue) &&
                ddlHistoryUser.Items.FindByValue(
                    selectedValue) != null)
            {
                ddlHistoryUser.SelectedValue =
                    selectedValue;
            }
        }

        protected void lbtFilterHistory_Click(
            object sender,
            EventArgs e)
        {
            DateTime? fromDate =
                ParseDate(
                    txtHistoryFromDate.Text);

            DateTime? toDate =
                ParseDate(
                    txtHistoryToDate.Text);

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date >
                toDate.Value.Date)
            {
                ShowNotify(
                    "Từ ngày không được lớn hơn đến ngày.",
                    MSGType.Error);

                ShowDrawer();
                return;
            }

            BindHistory();
            ShowDrawer();
        }

        protected void lbtClearHistoryFilter_Click(
            object sender,
            EventArgs e)
        {
            ddlHistoryUser.SelectedIndex = 0;

            txtHistoryFromDate.Text =
                string.Empty;

            txtHistoryToDate.Text =
                string.Empty;

            BindHistory();
            ShowDrawer();
        }

        protected void rptProjectHistory_ItemDataBound(
            object sender,
            RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType !=
                    ListItemType.Item &&
                e.Item.ItemType !=
                    ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row =
                e.Item.DataItem as DataRowView;

            if (row == null)
                return;

            Label lblContent =
                e.Item.FindControl(
                    "lblHistoryContent")
                as Label;

            Label lblTime =
                e.Item.FindControl(
                    "lblHistoryTime")
                as Label;

            if (lblContent != null)
            {
                lblContent.Text =
                    HttpUtility.HtmlEncode(
                        BuildHistoryContent(
                            row.Row));
            }

            if (lblTime != null)
            {
                lblTime.Text =
                    FormatHistoryTime(
                        row.Row);
            }
        }

        private Guid? GetSelectedUserId()
        {
            Guid userId;

            if (Guid.TryParse(
                    ddlHistoryUser.SelectedValue,
                    out userId) &&
                userId != Guid.Empty)
            {
                return userId;
            }

            return null;
        }

        private DateTime? ParseDate(
            string value)
        {
            DateTime result;

            if (DateTime.TryParseExact(
                    value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out result))
            {
                return result;
            }

            return null;
        }

        private bool TryGetGuid(
            DataRow row,
            string columnName,
            out Guid value)
        {
            value = Guid.Empty;

            if (row == null ||
                !row.Table.Columns.Contains(
                    columnName) ||
                row[columnName] == null ||
                row[columnName] == DBNull.Value)
            {
                return false;
            }

            return Guid.TryParse(
                Convert.ToString(
                    row[columnName]),
                out value);
        }

        private void ShowDrawer()
        {
            string script = @"
                (function () {
                    var element =
                        document.getElementById(
                            'project-history-offcanvas');

                    if (!element ||
                        typeof bootstrap === 'undefined') {
                        return;
                    }

                    var drawer =
                        bootstrap.Offcanvas
                            .getOrCreateInstance(
                                element);

                    drawer.show();
                })();";

            ScriptManager.RegisterStartupScript(
                Page,
                Page.GetType(),
                "OpenProjectHistoryDrawer",
                script,
                true);
        }

        // Đưa các hàm format lịch sử hiện có
        // từ DuAnDetail.aspx.cs vào control này:
        //
        // BuildHistoryContent(DataRow row)
        // GetHistoryEntityName(string tableName)
        // BuildDefaultHistoryContent(...)
        // FormatHistoryTime(DataRow row)
        // GetColumnText(DataRow row, string columnName)
        private string BuildHistoryContent(
    DataRow row)
        {
            string resourceKey =
                GetColumnText(
                    row,
                    "Description");

            string actor =
                GetColumnText(
                    row,
                    "ChangedBy");

            string tableName =
                GetColumnText(
                    row,
                    "TableName");

            string title =
                GetColumnText(
                    row,
                    "Title");

            if (string.IsNullOrWhiteSpace(actor) ||
                string.Equals(
                    actor,
                    "[System]",
                    StringComparison.OrdinalIgnoreCase))
            {
                actor = "Hệ thống";
            }

            if (string.IsNullOrWhiteSpace(title))
                title = "Chưa xác định";

            string entityName =
                GetHistoryEntityName(
                    tableName);

            string template =
                string.IsNullOrWhiteSpace(resourceKey)
                    ? null
                    : GetResourceText(resourceKey);

            if (string.IsNullOrWhiteSpace(template) ||
                string.Equals(
                    template,
                    resourceKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                return BuildDefaultHistoryContent(
                    row,
                    actor,
                    entityName,
                    title);
            }

            bool isContainerAction = string.Equals(resourceKey, BackEndResourceKeys.HISTORY_ADDED_TO_CONTAINER, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(resourceKey, BackEndResourceKeys.HISTORY_REMOVED_FROM_CONTAINER, StringComparison.OrdinalIgnoreCase);
            if (isContainerAction)
            {
                string containerName = GetResourceText(BackEndResourceKeys.PROJECT);
                return string.Format(
                    template,
                    actor,          // {0}: Administrator
                    title,          // {1}: Nguyễn Thị A
                    containerName);
            }

            try
            {
                return string.Format(
                    template,
                    actor,
                    entityName,
                    title);
            }
            catch (FormatException)
            {
                return BuildDefaultHistoryContent(
                    row,
                    actor,
                    entityName,
                    title);
            }
        }

        private string GetHistoryEntityName(
    string tableName)
        {
            switch (tableName)
            {
                case nameof(TblDuAn):
                    return "dự án";

                case nameof(TblGiaiDoanDuAn):
                    return "giai đoạn";

                case nameof(TblCongViec):
                    return "công việc";

                case nameof(TblThanhVienDuAn):
                    return "thành viên dự án";

                case nameof(TblHopDongThucHien):
                    return "hợp đồng";

                default:
                    return "thông tin";
            }
        }

        private string BuildDefaultHistoryContent(
    DataRow row,
    string actor,
    string entityName,
    string title)
        {
            string actionType =
                GetColumnText(
                    row,
                    "ActionType");

            switch (actionType)
            {
                case "CREATE":
                    return string.Format(
                        "{0} đã thêm {1} \"{2}\".",
                        actor,
                        entityName,
                        title);

                case "UPDATE":
                    return string.Format(
                        "{0} đã cập nhật {1} \"{2}\".",
                        actor,
                        entityName,
                        title);

                case "DELETE":
                    return string.Format(
                        "{0} đã xóa {1} \"{2}\".",
                        actor,
                        entityName,
                        title);

                default:
                    return string.Format(
                        "{0} đã thao tác trên {1} \"{2}\".",
                        actor,
                        entityName,
                        title);
            }
        }

        private string FormatHistoryTime(
    DataRow row)
        {
            if (!row.Table.Columns.Contains(
                    "ChangedAt") ||
                row["ChangedAt"] == null ||
                row["ChangedAt"] == DBNull.Value)
            {
                return string.Empty;
            }

            DateTime changedAt =
                Convert.ToDateTime(
                    row["ChangedAt"]);

            return changedAt
                .ToLocalTime()
                .ToString("dd/MM/yyyy HH:mm");
        }

        private string GetColumnText(
    DataRow row,
    string columnName)
        {
            if (row == null ||
                !row.Table.Columns.Contains(
                    columnName) ||
                row[columnName] == null ||
                row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(
                row[columnName]);
        }
    }
}