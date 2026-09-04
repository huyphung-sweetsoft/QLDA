using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlGiaiDoanDuAn : BaseAdminUserControl
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
        protected void Page_Load(object sender, EventArgs e) { }

        public void InitControls()
        {
            if (IdDuAn == Guid.Empty)
            {
                ShowEmptyData();
                return;
            }

            BindProjectStages();
            BindCommonStages();
        }

        private void BindProjectStages()
        {
            DataTable dt =
                GiaiDoanDuAnManager.Instance
                    .GetByIdDuAn(IdDuAn);

            bool hasData =
                dt != null &&
                dt.Rows.Count > 0;

            pnlEmpty.Visible =
                !hasData;

            pnlEmptyManagement.Visible =
                !hasData;

            rptStages.Visible =
                hasData;

            rptStageManagement.Visible =
                hasData;

            if (!hasData)
            {
                rptStages.DataSource = null;
                rptStages.DataBind();

                rptStageManagement.DataSource = null;
                rptStageManagement.DataBind();

                return;
            }

            AddDisplayColumns(dt);

            rptStages.DataSource = dt;
            rptStages.DataBind();

            rptStageManagement.DataSource = dt;
            rptStageManagement.DataBind();
        }

        protected void rblStageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isCustom = rblStageType.SelectedValue == "CUSTOM";

            pnlCommonStage.Visible = !isCustom;
            pnlCustomStage.Visible = isCustom;

            if (isCustom)
                ddlCommonStage.SelectedIndex = 0;
            else
                txtCustomStageName.Text = string.Empty;

            upnlStageManagement.Update();
            OpenDrawer();
        }

        private void ShowEmptyData()
        {
            rptStages.DataSource = null;
            rptStages.DataBind();

            rptStages.Visible = false;
            pnlEmpty.Visible = true;
        }

        private void AddDisplayColumns(DataTable dt)
        {
            if (!dt.Columns.Contains("TrangThaiHienThi"))
                dt.Columns.Add("TrangThaiHienThi", typeof(string));

            if (!dt.Columns.Contains("KhoangThoiGian"))
                dt.Columns.Add("KhoangThoiGian", typeof(string));

            if (!dt.Columns.Contains("DotCssClass"))
                dt.Columns.Add("DotCssClass", typeof(string));

            if (!dt.Columns.Contains("PhanTramHienThi"))
                dt.Columns.Add("PhanTramHienThi", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                string status = GetStageStatus(row);

                row["TrangThaiHienThi"] = status;
                row["KhoangThoiGian"]   = GetStageDateRange(row);
                row["DotCssClass"]      = GetDotCssClass(status);
                row["PhanTramHienThi"]  = GetPhanTramHienThi(row, status);
            }
        }

        private string GetStageStatus(
            DataRow row)
        {
            DateTime today =
                DateTime.Now.Date;

            DateTime? startDate =
                GetNullableDate(
                    row,
                    "NgayBatDau");

            DateTime? completedDate =
                GetNullableDate(
                    row,
                    "NgayHoanThanhThucTe");

            if (completedDate.HasValue)
                return "Đã hoàn thành";

            if (!startDate.HasValue ||
                startDate.Value.Date > today)
            {
                return "Chưa thực hiện";
            }

            return "Đang thực hiện";
        }

        private string GetStageDateRange(
            DataRow row)
        {
            DateTime? startDate =
                GetNullableDate(
                    row,
                    "NgayBatDau");

            DateTime? expectedEndDate =
                GetNullableDate(
                    row,
                    "NgayDuKienHoanThanh");

            if (!startDate.HasValue &&
                !expectedEndDate.HasValue)
            {
                return "Chưa thiết lập thời gian";
            }

            string startText =
                startDate.HasValue
                    ? startDate.Value.ToString(
                        "dd/MM/yyyy")
                    : "Chưa có";

            string endText =
                expectedEndDate.HasValue
                    ? expectedEndDate.Value.ToString(
                        "dd/MM/yyyy")
                    : "Chưa có";

            return string.Format(
                "{0} - {1}",
                startText,
                endText);
        }

        private DateTime? GetNullableDate(
            DataRow row,
            string columnName)
        {
            if (!row.Table.Columns.Contains(
                    columnName) ||
                row[columnName] == null ||
                row[columnName] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(
                row[columnName]);
        }

        private string GetDotCssClass(string status)
        {
            switch (status)
            {
                case "Đã hoàn thành":  return "stage-done";
                case "Đang thực hiện": return "stage-active";
                default:               return string.Empty;
            }
        }

        private string GetPhanTramHienThi(DataRow row, string status)
        {
            // Hiển thị ngày hoàn thành thực tế nếu đã xong,
            // hoặc khoảng thời gian dự kiến nếu chưa xong
            if (status == "Đã hoàn thành")
            {
                DateTime? completedDate = GetNullableDate(row, "NgayHoanThanhThucTe");
                return completedDate.HasValue
                    ? completedDate.Value.ToString("dd/MM/yyyy")
                    : "Hoàn thành";
            }

            DateTime? expectedEnd = GetNullableDate(row, "NgayDuKienHoanThanh");
            return expectedEnd.HasValue
                ? "DK: " + expectedEnd.Value.ToString("dd/MM/yyyy")
                : string.Empty;
        }

        // ---- Helpers cho rptStageManagement ----

        protected string GetFormattedDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return "—";

            DateTime date;
            if (!DateTime.TryParse(value.ToString(), out date))
                return "—";

            return date.ToString("dd/MM/yyyy");
        }

        protected string GetStatusBadgeClass(string status)
        {
            switch (status)
            {
                case "Đã hoàn thành":  return "badge-done";
                case "Đang thực hiện": return "badge-active";
                default:               return "badge-pending";
            }
        }

        protected string GetProgressFillClass(string status)
        {
            switch (status)
            {
                case "Đã hoàn thành":  return "fill-done";
                case "Đang thực hiện": return "fill-active";
                default:               return string.Empty;
            }
        }

        protected int GetHardCodedPercent(string status)
        {
            // TODO: thay bằng dữ liệu thực tế sau
            switch (status)
            {
                case "Đã hoàn thành":  return 100;
                case "Đang thực hiện": return 45;
                default:               return 0;
            }
        }

        private void BindCommonStages()
        {
            List<TblGiaiDoan> data =
                GiaiDoanManager.Instance
                    .GetAllActive();

            ddlCommonStage.DataSource =
                data;

            ddlCommonStage.DataValueField =
                TblGiaiDoan.Columns.IdGiaiDoan;

            ddlCommonStage.DataTextField =
                TblGiaiDoan.Columns.TenGiaiDoan;

            ddlCommonStage.DataBind();

            ddlCommonStage.Items.Insert(
                0,
                new ListItem(
                    "-- Chọn giai đoạn --",
                    string.Empty));
        }

        private void OpenDrawer()
        {
            ScriptManager.RegisterStartupScript(
                Page,
                Page.GetType(),
                "OpenProjectStageDrawer",
                "ProjectStageJs.ShowOffcanvas();",
                true);
        }

        protected void lbtAddStage_Click(object sender, EventArgs e)
        {
            ResetForm();

            lblStageFormTitle.Text = "Thêm giai đoạn";
            pnlStageForm.Visible = true;

            upnlStageManagement.Update();
            OpenDrawer();
        }

        protected void lbtCancelStage_Click(object sender, EventArgs e)
        {
            ResetForm();

            pnlStageForm.Visible = false;

            upnlStageManagement.Update();
            OpenDrawer();
        }

        private void ResetForm()
        {
            rblStageType.SelectedValue = "COMMON";
            pnlCommonStage.Visible = true;
            pnlCustomStage.Visible = false;

            if (ddlCommonStage.Items.Count > 0)
                ddlCommonStage.SelectedIndex = 0;

            txtCustomStageName.Text  = string.Empty;
            txtStartDate.Text        = string.Empty;
            txtExpectedEndDate.Text  = string.Empty;
            txtActualEndDate.Text    = string.Empty;
            txtStageOrder.Text       = string.Empty;
            txtStageDescription.Text = string.Empty;
            lblStageError.Text       = string.Empty;
            lblStageError.Visible    = false;
        }

        private TblGiaiDoanDuAn BuildStageDto()
        {
            TblGiaiDoanDuAn dto =
                new TblGiaiDoanDuAn();

            dto.IdDuAn =
                IdDuAn;

            bool isCustom =
                rblStageType.SelectedValue ==
                "CUSTOM";

            if (isCustom)
            {
                dto.IdGiaiDoan = null;

                dto.TenGiaiDoanTuyChinh =
                    txtCustomStageName.Text.Trim();
            }
            else
            {
                Guid idGiaiDoan;

                if (Guid.TryParse(
                    ddlCommonStage.SelectedValue,
                    out idGiaiDoan))
                {
                    dto.IdGiaiDoan =
                        idGiaiDoan;
                }
                else
                {
                    dto.IdGiaiDoan = null;
                }

                dto.TenGiaiDoanTuyChinh =
                    null;
            }

            dto.NgayBatDau =
                ParseNullableDate(
                    txtStartDate.Text);

            dto.NgayDuKienHoanThanh =
                ParseNullableDate(
                    txtExpectedEndDate.Text);

            dto.NgayHoanThanhThucTe =
                ParseNullableDate(
                    txtActualEndDate.Text);

            int order;

            dto.ThuTuGiaiDoan =
                int.TryParse(
                    txtStageOrder.Text,
                    out order)
                        ? order
                        : 0;

            dto.MoTa =
                string.IsNullOrWhiteSpace(
                    txtStageDescription.Text)
                        ? null
                        : txtStageDescription.Text.Trim();

            return dto;
        }

        private DateTime? ParseNullableDate(string value)
        {
            DateTime date;
            return DateTime.TryParse(value, out date) ? date : (DateTime?)null;
        }

        protected void lbtSaveStage_Click(object sender, EventArgs e)
        {
            // Đảm bảo dropdown không bị mất data khi postback
            if (ddlCommonStage.Items.Count == 0)
                BindCommonStages();

            bool isCustom = rblStageType.SelectedValue == "CUSTOM";

            // Validate phía server trước khi gọi Manager
            if (isCustom)
            {
                if (string.IsNullOrWhiteSpace(txtCustomStageName.Text))
                {
                    ShowError("Vui lòng nhập tên giai đoạn.");
                    upnlStageManagement.Update();
                    OpenDrawer();
                    return;
                }
            }
            else
            {
                Guid testGuid;
                if (!Guid.TryParse(ddlCommonStage.SelectedValue, out testGuid) ||
                    testGuid == Guid.Empty)
                {
                    ShowError("Vui lòng chọn giai đoạn từ danh sách.");
                    upnlStageManagement.Update();
                    OpenDrawer();
                    return;
                }
            }

            try
            {
                TblGiaiDoanDuAn dto = BuildStageDto();
                GiaiDoanDuAnManager.Instance.CreateOrUpdate(dto);

                ResetForm();
                pnlStageForm.Visible = false;
                BindProjectStages();

                upnlStageManagement.Update();
                OpenDrawer();
            }
            catch (Exception exc)
            {
                ShowError(exc.Message);

                upnlStageManagement.Update();
                OpenDrawer();
            }
        }

        private void ShowError(string message)
        {
            lblStageError.Text    = HttpUtility.HtmlEncode(message);
            lblStageError.Visible = true;
        }
    }
}