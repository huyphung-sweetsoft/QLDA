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

        private Guid CurrentEditId
        {
            get
            {
                object value = ViewState["CurrentEditId"];
                if (value == null)
                    return Guid.Empty;
                Guid id;
                return Guid.TryParse(value.ToString(), out id) ? id : Guid.Empty;
            }
            set
            {
                ViewState["CurrentEditId"] = value.ToString();
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

        protected void rblStageType_SelectedIndexChanged(
    object sender,
    EventArgs e)
        {
            ToggleStageTypeControls();

            if (rblStageType.SelectedValue == "CUSTOM")
            {
                if (ddlCommonStage.Items.Count > 0)
                    ddlCommonStage.SelectedIndex = 0;
            }
            else
            {
                txtCustomStageName.Text =
                    string.Empty;
            }

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

        protected void lbtCancelStage_Click(object sender, EventArgs e)
        {
            ResetForm();

            pnlStageForm.Visible = false;

            upnlStageManagement.Update();
            OpenDrawer();
        }

        private void ResetForm()
        {
            CurrentEditId =
                Guid.Empty;

            rblStageType.SelectedValue =
                "COMMON";

            pnlCommonStage.Visible =
                true;

            pnlCustomStage.Visible =
                false;

            if (ddlCommonStage.Items.Count > 0)
            {
                ddlCommonStage.SelectedIndex =
                    0;
            }

            txtCustomStageName.Text =
                string.Empty;

            txtStartDate.Text =
                string.Empty;

            txtExpectedEndDate.Text =
                string.Empty;

            txtExpectedEndDate.ReadOnly =
                false;

            RemoveCssClass(
                txtExpectedEndDate,
                "bg-light");

            txtActualEndDate.Text =
                string.Empty;

            txtStageOrder.Text =
                string.Empty;

            txtStageDescription.Text =
                string.Empty;

            lblStageError.Text =
                string.Empty;

            lblStageError.Visible =
                false;

            lblStageFormTitle.Text =
                "Thêm giai đoạn";
        }

        private TblGiaiDoanDuAn BuildStageDto()
        {
            TblGiaiDoanDuAn dto =
                new TblGiaiDoanDuAn();

            /*
             * Guid.Empty: thêm mới.
             * Có ID: cập nhật.
             */
            dto.IdGiaiDoanDuAn =
                CurrentEditId;

            dto.IdDuAn =
                IdDuAn;

            bool isCustom =
                rblStageType.SelectedValue ==
                "CUSTOM";

            if (isCustom)
            {
                dto.IdGiaiDoan =
                    null;

                dto.TenGiaiDoanTuyChinh =
                    string.IsNullOrWhiteSpace(
                        txtCustomStageName.Text)
                            ? null
                            : txtCustomStageName.Text.Trim();
            }
            else
            {
                Guid idGiaiDoan;

                dto.IdGiaiDoan =
                    Guid.TryParse(
                        ddlCommonStage.SelectedValue,
                        out idGiaiDoan)
                        ? idGiaiDoan
                        : (Guid?)null;

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

            /*
             * Không lấy thứ tự từ giao diện.
             * Khi thêm, Manager tự sinh.
             * Khi sửa, Manager giữ giá trị trong DB.
             */
            dto.ThuTuGiaiDoan =
                0;

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

        protected void lbtSaveStage_Click(
    object sender,
    EventArgs e)
        {
            if (ddlCommonStage.Items.Count == 0)
            {
                BindCommonStages();
            }

            bool isCustom =
                rblStageType.SelectedValue ==
                "CUSTOM";

            if (isCustom)
            {
                if (string.IsNullOrWhiteSpace(
                    txtCustomStageName.Text))
                {
                    ShowError(
                        "Vui lòng nhập tên giai đoạn.");

                    upnlStageManagement.Update();
                    OpenDrawer();

                    return;
                }
            }
            else
            {
                Guid idGiaiDoan;

                bool validCommonStage =
                    Guid.TryParse(
                        ddlCommonStage.SelectedValue,
                        out idGiaiDoan) &&
                    idGiaiDoan != Guid.Empty;

                if (!validCommonStage)
                {
                    ShowError(
                        "Vui lòng chọn giai đoạn từ danh sách.");

                    upnlStageManagement.Update();
                    OpenDrawer();

                    return;
                }
            }

            try
            {
                bool isEdit =
                    CurrentEditId != Guid.Empty;

                TblGiaiDoanDuAn dto =
                    BuildStageDto();

                TblGiaiDoanDuAn result =
                    GiaiDoanDuAnManager.Instance
                        .CreateOrUpdate(dto);

                if (result == null)
                {
                    ShowError(
                        "Không thể lưu giai đoạn.");

                    upnlStageManagement.Update();
                    OpenDrawer();

                    return;
                }

                ResetForm();

                pnlStageForm.Visible =
                    false;

                BindProjectStages();

                upnlStageManagement.Update();

                OpenDrawer();

                ShowNotify(
                    isEdit
                        ? "Cập nhật giai đoạn thành công."
                        : "Thêm giai đoạn thành công.",
                    MSGType.Success);
            }
            catch (Exception exc)
            {
                ShowError(exc.Message);

                upnlStageManagement.Update();

                OpenDrawer();
            }
        }

        protected void rptStageManagement_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "EDIT_STAGE")
                return;

            Guid idGiaiDoanDuAn;

            bool isValidId = Guid.TryParse(Convert.ToString(e.CommandArgument), out idGiaiDoanDuAn);

            if (!isValidId)
            {
                ShowNotify(
                    "Giai đoạn không hợp lệ.",
                    MSGType.Error);

                return;
            }

            TblGiaiDoanDuAn stage = GiaiDoanDuAnManager.Instance.GetById(idGiaiDoanDuAn);

            if (stage == null)
            {
                ShowNotify(
                    "Không tìm thấy giai đoạn.",
                    MSGType.Error);

                return;
            }

            TblCongViec rootTask = TaskManager.Instance.GetRootTaskByStageId(idGiaiDoanDuAn);

            if (rootTask == null)
            {
                ShowNotify(
                    "Không tìm thấy công việc gốc của giai đoạn.",
                    MSGType.Error);

                return;
            }

            CurrentEditId = idGiaiDoanDuAn;

            BindStageEditForm(stage, rootTask);

            lblStageFormTitle.Text = "Sửa giai đoạn";

            pnlStageForm.Visible = true;

            upnlStageManagement.Update();
            OpenDrawer();
        }

        private void BindStageEditForm(TblGiaiDoanDuAn stage, TblCongViec rootTask)
        {
            bool isCommonStage = stage.IdGiaiDoan.HasValue && stage.IdGiaiDoan.Value != Guid.Empty;

            rblStageType.SelectedValue = isCommonStage ? "COMMON" : "CUSTOM";

            if (isCommonStage)
            {
                ddlCommonStage.SelectedValue = stage.IdGiaiDoan.Value.ToString();
                txtCustomStageName.Text = string.Empty;
            }
            else
            {
                ddlCommonStage.SelectedValue = string.Empty;

                txtCustomStageName.Text = stage.TenGiaiDoanTuyChinh ?? string.Empty;
            }

            txtStartDate.Text = stage.NgayBatDau.HasValue
                ? stage.NgayBatDau.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            txtExpectedEndDate.Text = stage.NgayDuKienHoanThanh.HasValue
                    ? stage.NgayDuKienHoanThanh.Value.ToString("yyyy-MM-dd")
                    : string.Empty;

            txtActualEndDate.Text = stage.NgayHoanThanhThucTe.HasValue
                ? stage.NgayHoanThanhThucTe.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            txtStageDescription.Text = stage.MoTa ?? string.Empty;

            bool hasChildTasks = TaskManager.Instance.CheckHasChildTasks(stage.IdDuAn, rootTask);

            /*
            * Chưa có công việc con:
            * người dùng được sửa ngày dự kiến hoàn thành.
            *
            * Đã có công việc con:
            * ngày kết thúc được tính từ công việc con.
            */
            txtExpectedEndDate.ReadOnly =
                hasChildTasks;

            RemoveCssClass(
    txtExpectedEndDate,
    "bg-light");

            if (hasChildTasks)
            {
                txtExpectedEndDate.CssClass =
                    string.IsNullOrWhiteSpace(
                        txtExpectedEndDate.CssClass)
                            ? "bg-light"
                            : txtExpectedEndDate.CssClass +
                                " bg-light";
            }

            ToggleStageTypeControls();
        }

        private void ShowError(string message)
        {
            lblStageError.Text    = HttpUtility.HtmlEncode(message);
            lblStageError.Visible = true;
        }

        private void ToggleStageTypeControls()
        {
            bool isCustom =
                rblStageType.SelectedValue ==
                "CUSTOM";

            pnlCommonStage.Visible =
                !isCustom;

            pnlCustomStage.Visible =
                isCustom;
        }

        private void RemoveCssClass(
    WebControl control,
    string cssClass)
        {
            if (control == null ||
                string.IsNullOrWhiteSpace(
                    control.CssClass))
            {
                return;
            }

            control.CssClass =
                string.Join(
                    " ",
                    control.CssClass
                        .Split(
                            new[] { ' ' },
                            StringSplitOptions
                                .RemoveEmptyEntries)
                        .Where(
                            item =>
                                !item.Equals(
                                    cssClass,
                                    StringComparison
                                        .OrdinalIgnoreCase)));
        }


    }
}