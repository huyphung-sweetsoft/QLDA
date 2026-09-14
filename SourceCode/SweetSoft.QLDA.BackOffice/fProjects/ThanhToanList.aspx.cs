using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects
{
    public partial class ThanhToanList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Payment;
        private Guid PaymentId
        {
            get => ViewState["PaymentId"] == null ? Guid.Empty : (Guid)ViewState["PaymentId"];
            set => ViewState["PaymentId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CurrentProjectId == Guid.Empty)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                return;
            }
            if (!IsView || !ThanhToanManager.Instance.CanAccessProject(CurrentProjectId, ActionKeys.View))
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                return;
            }
            CtrlThanhToan1.NewPaymentHandlerCallback += NewPaymentAction;
            CtrlThanhToan1.EditPaymentHandlerCallback += EditPaymentAction;
            if (!IsPostBack)
            {
                string title = GetResourceText(BackEndResourceKeys.PAYMENT_LIST);
                TblDuAn project = DuAnManager.Instance.GetDuAnById(CurrentProjectId);
                SetMetaTagsOgTags(title);
                Navigation1.MainTitle = title;
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { GetRelativeClientPath(RewriteURLHelper.ProjectDetail(CurrentProjectId)), Server.HtmlEncode(project.MaDuAn) },
                    { "javascript:;", title }
                };
                dlDetail.Title = GetResourceText(BackEndResourceKeys.PAYMENT_EDIT_STATUS);
                dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);
                lbtSubmit.Text = lbtSubmit.ToolTip = GetResourceText(BackEndResourceKeys.SAVE);
                ddlTrangThai.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
                dtHanThanhToan.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
                dtNgayThanhToan.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
                CtrlThanhToan1.InitControls();
            }
        }

        private void NewPaymentAction(object sender, EventArgs e)
        {
            if (!IsAdd || !ThanhToanManager.Instance.CanAccessProject(CurrentProjectId, ActionKeys.Create))
            {
                ShowAccessDeniedNotify();
                return;
            }
            PaymentId = Guid.Empty;
            TblDuAn project = DuAnManager.Instance.GetDuAnById(CurrentProjectId);
            if (project == null)
            {
                ShowInvalidNotFoundData();
                return;
            }
            string nextCode = ThanhToanManager.Instance.GetNextPaymentCode(CurrentProjectId);
            lblMaDotPrefix.Visible = false;
            txtMaDot.Enabled = false;
            txtMaDot.CssClass = string.Empty;
            txtMaDot.Attributes.Remove("style");
            txtMaDot.TextMode = TextBoxMode.SingleLine;
            txtMaDot.MaxLength = 50;
            txtMaDot.Attributes.Remove("min");
            txtMaDot.Attributes.Remove("step");
            txtMaDot.Text = nextCode;
            dtHanThanhToan.Enabled = true;
            txtTenDot.Text = string.Empty;
            txtSoTien.Text = string.Empty;
            dtHanThanhToan.DateValue = null;
            dtNgayThanhToan.DateValue = null;
            txtGhiChu.Text = string.Empty;
            BindStatuses((byte)ThanhToanStatus.ChuaThanhToan, null);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtSubmit.Visible = true;
            dlDetail.OpenModal(true);
        }

        private void EditPaymentAction(object sender, EventArgs e)
        {
            if (!IsEdit || !ThanhToanManager.Instance.CanAccessProject(CurrentProjectId, ActionKeys.Update))
            {
                ShowAccessDeniedNotify();
                return;
            }
            TblThanhToan item = sender is Guid
                ? ThanhToanManager.Instance.GetByProject((Guid)sender, CurrentProjectId) : null;
            if (item == null)
            {
                ShowInvalidNotFoundData();
                return;
            }
            PaymentId = item.IdThanhToan;
            lblMaDotPrefix.Visible = false;
            txtMaDot.Enabled = false;
            txtMaDot.CssClass = string.Empty;
            txtMaDot.Attributes.Remove("style");
            txtMaDot.TextMode = TextBoxMode.SingleLine;
            txtMaDot.MaxLength = 50;
            txtMaDot.Attributes.Remove("min");
            txtMaDot.Attributes.Remove("step");
            dtHanThanhToan.Enabled = false;
            txtMaDot.Text = item.MaDotThanhToan;
            txtTenDot.Text = item.TenDotThanhToan;
            txtSoTien.Text = ConvertNumber(item.SoTien);
            dtHanThanhToan.DateValue = item.HanThanhToan;
            txtGhiChu.Text = item.GhiChu;
            BindStatuses(item.TrangThai, item.HanThanhToan);
            dtNgayThanhToan.DateValue = item.NgayThanhToanThucTe;
            dlDetail.Title = GetResourceText(BackEndResourceKeys.PAYMENT_EDIT_STATUS);
            lbtSubmit.Visible = true;
            dlDetail.OpenModal(true);
        }

        private void BindStatuses(byte selectedStatus, DateTime? dueDate)
        {
            ddlTrangThai.Items.Clear();
            ThanhToanStatus unpaidStatus = dueDate.HasValue && dueDate.Value.Date < DateTime.Today
                ? ThanhToanStatus.TreHan
                : ThanhToanStatus.ChuaThanhToan;
            AddStatusItem(unpaidStatus);
            AddStatusItem(ThanhToanStatus.DaThanhToan);
            ddlTrangThai.SelectedValue = selectedStatus == (byte)ThanhToanStatus.DaThanhToan
                ? ((byte)ThanhToanStatus.DaThanhToan).ToString()
                : ((byte)unpaidStatus).ToString();
        }

        private void AddStatusItem(ThanhToanStatus status)
        {
            ddlTrangThai.Items.Add(new ListItem(
                GetResourceText(EnumHelpers.GetERenderText(typeof(ThanhToanStatus), status)),
                ((byte)status).ToString()));
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            bool isNew = PaymentId == Guid.Empty;
            ActionKeys requiredAction = isNew ? ActionKeys.Create : ActionKeys.Update;
            if ((isNew && !IsAdd) || (!isNew && !IsEdit)
                || !ThanhToanManager.Instance.CanAccessProject(CurrentProjectId, requiredAction))
            {
                ShowAccessDeniedNotify();
                return;
            }
            byte status;
            if (!byte.TryParse(ddlTrangThai.SelectedValue, out status)
                || !Enum.IsDefined(typeof(ThanhToanStatus), status))
            {
                ShowInvalidDataError();
                return;
            }
            string paymentName = (txtTenDot.Text ?? string.Empty).Trim();
            decimal amount;
            if (string.IsNullOrEmpty(paymentName) || paymentName.Length > 255
                || !GetValue(txtSoTien, out amount) || amount <= 0)
            {
                ShowInvalidDataError();
                return;
            }
            string note = (txtGhiChu.Text ?? string.Empty).Trim();
            if (note.Length > 1000)
            {
                ShowInvalidDataError();
                return;
            }
            // Thanh toán là dữ liệu chỉ có ngày, không có giờ. DateValue được
            // ExtraDateTime chuyển về UTC nên 10/09 00:00 (GMT+7) thành 09/09
            // 17:00 UTC. Lấy giá trị hiển thị để giữ nguyên ngày người dùng chọn.
            DateTime? actualPaymentDate = GetPaymentDateValue(dtNgayThanhToan);
            if (status == (byte)ThanhToanStatus.DaThanhToan)
            {
                if (!actualPaymentDate.HasValue)
                    actualPaymentDate = DateTime.Today;   // ← tự gán ngày hôm nay
                if (actualPaymentDate.Value > DateTime.Today)
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.PAYMENT_DATE_REQUIRED), MSGType.Error);
                    return;
                }
            }
            try
            {
                if (isNew)
                {
                    DateTime? dueDate = GetPaymentDateValue(dtHanThanhToan);
                    if (!dueDate.HasValue)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ThanhToanManager.Instance.CreatePayment(CurrentProjectId,
                        paymentName, amount, dueDate.Value, status, actualPaymentDate, note);
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                }
                else
                {
                    ThanhToanManager.Instance.UpdatePayment(PaymentId, CurrentProjectId, paymentName,
                        amount, status, actualPaymentDate, note);
                    ShowSuccessSaveData();
                }
                PaymentId = Guid.Empty;
                dlDetail.CloseModal();
                CtrlThanhToan1.Rebind();
            }
            catch (UnauthorizedAccessException)
            {
                ShowAccessDeniedNotify();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private DateTime? GetPaymentDateValue(ExtraDateTime control)
        {
            // Ưu tiên giá trị đang hiển thị trong ô nhập. ExtraDateTime có thể gửi
            // nhiều giá trị ở hidden field sau các lần postback của UpdatePanel;
            // lấy giá trị hidden cũ có thể làm 10/09 bị hiểu thành 09/10.
            string rawValue = Request.Form[control.UniqueID];
            if (rawValue == null)
                rawValue = Request.Form[control.UniqueID + "_hdfDRPValue"];

            if (rawValue != null)
            {
                if (string.IsNullOrWhiteSpace(rawValue))
                    return null;

                string[] postedValues = rawValue.Split(',');
                rawValue = postedValues[postedValues.Length - 1];
                Match match = Regex.Match(rawValue, @"^\s*(\d{1,2}/\d{1,2}/\d{4})");
                DateTime parsedDate;
                if (match.Success && DateTime.TryParseExact(match.Groups[1].Value,
                    new[] { "dd/MM/yyyy", "d/M/yyyy" }, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out parsedDate))
                {
                    return parsedDate.Date;
                }
            }

            return control.DateValueForDisplay.HasValue
                ? control.DateValueForDisplay.Value.Date
                : (DateTime?)null;
        }

    }
}
