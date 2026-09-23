using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fThanhToan.Controls
{
    public partial class CtrlThanhToan : BaseAdminUserControl
    {
        public EventHandler NewPaymentHandlerCallback;
        public EventHandler EditPaymentHandlerCallback;

        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager.GetCurrent(Page).RegisterAsyncPostBackControl(lbtSearchSingle);
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
        }

        public void InitControls()
        {
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.PAYMENT_SEARCH_PLACEHOLDER);
            lbtSearchSingle.ToolTip = GetResourceText(BackEndResourceKeys.SEARCH);
            lbtAdd.Text = lbtAdd.ToolTip = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtAdd.Visible = CURRENT_PAGE.IsAdd;
            grvData.HeaderTexts = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.PAYMENT_CODE),
                GetResourceText(BackEndResourceKeys.PAYMENT_NAME),
                GetResourceText(BackEndResourceKeys.NOTE),
                GetResourceText(BackEndResourceKeys.PAYMENT_AMOUNT),
                GetResourceText(BackEndResourceKeys.PAYMENT_DUE_DATE),
                GetResourceText(BackEndResourceKeys.PAYMENT_ACTUAL_DATE),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "MaDotThanhToan";
            grvData.CurrentSortDerection = "ASC";
            Rebind();
            pnlButtons.Update();
        }

        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                int startRow = (grvData.CurrentPageIndex - 1) * grvData.CurrentPageSize + 1;
                int totalRows;
                DataTable data = ThanhToanManager.Instance.SearchThanhToans(CURRENT_PAGE.CurrentProjectId,
                    txtSearchSingle.Text, grvData.CurrentSortExpression, grvData.CurrentSortDerection,
                    startRow, startRow + grvData.CurrentPageSize - 1, out totalRows);
                grvData.VirtualItemCount = totalRows;
                grvData.DataSource = data;
                grvData.DataBind();
                ctrlGridviewPaging.Visible = totalRows > 0;
                ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                ctrlGridviewPaging.TotalItems = totalRows;
                ctrlGridviewPaging.InitLoad();
                upMain.Update();
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

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            Guid id;
            switch (e.CommandName)
            {
                case "ITEM_EDIT":
                    if (!CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    if (!TryGetPaymentId(e, out id))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    EditPaymentHandlerCallback?.Invoke(id, EventArgs.Empty);
                    break;
                case "ITEM_DELETE":
                    if (!CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    if (!TryGetPaymentId(e, out id))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    try
                    {
                        ThanhToanManager.Instance.DeletePayment(id, CURRENT_PAGE.CurrentProjectId);
                        ShowSuccessDeleteData();
                        Rebind();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        ShowAccessDeniedNotify();
                    }
                    catch (Exception exc)
                    {
                        ShowNotify(exc.Message, MSGType.Error);
                    }
                    break;
                case "ITEM_QUICK_APPROVE":
                    if (!CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    if (!TryGetPaymentId(e, out id))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    try
                    {
                        ThanhToanManager.Instance.ApprovePayment(id, CURRENT_PAGE.CurrentProjectId, DateTime.Today);
                        ShowSuccessSaveData();
                        Rebind();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        ShowAccessDeniedNotify();
                        Rebind();
                    }
                    catch (Exception exc)
                    {
                        ShowNotify(exc.Message, MSGType.Error);
                        Rebind();
                    }
                    break;
            }
        }

        private bool TryGetPaymentId(GridViewCommandEventArgs e, out Guid id)
        {
            id = Guid.Empty;
            int rowIndex = e.CommandSource is GridviewExtension
                ? Convert.ToInt32(e.CommandArgument)
                : ((GridViewRow)((LinkButton)e.CommandSource).NamingContainer).RowIndex;
            return rowIndex >= 0 && rowIndex < grvData.DataKeys.Count
                && Guid.TryParse(Convert.ToString(grvData.DataKeys[rowIndex].Value), out id);
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e) => Rebind();

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            NewPaymentHandlerCallback?.Invoke(Guid.Empty, EventArgs.Empty);
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        protected string FormatDate(object value)
        {
            return value == null || value == DBNull.Value ? "—" : ((DateTime)value).ToString("dd/MM/yyyy");
        }

        protected string GetStatusText(object value)
        {
            return IsPaid(value)
                ? GetResourceText(BackEndResourceKeys.PAYMENT_PAID)
                : GetResourceText(BackEndResourceKeys.PAYMENT_UNPAID);
        }

        protected string GetStatusCss(object value)
        {
            return IsPaid(value)
                ? "badge rounded-pill bg-success"
                : "badge rounded-pill bg-secondary";
        }

        protected string GetDueDateCss(object value, object dueDate)
        {
            return IsOverdue(value, dueDate)
                ? "text-danger fw-semibold"
                : string.Empty;
        }

        protected string GetDueDateTitle(object value, object dueDate)
        {
            return IsOverdue(value, dueDate)
                ? GetResourceText(BackEndResourceKeys.PAYMENT_OVERDUE)
                : string.Empty;
        }

        protected bool IsPaid(object value)
        {
            return value != null && value != DBNull.Value
                && Convert.ToByte(value) == (byte)ThanhToanStatus.DaThanhToan;
        }

        protected bool IsOverdue(object value, object dueDate)
        {
            if (IsPaid(value) || dueDate == null || dueDate == DBNull.Value)
                return false;

            DateTime deadline;
            if (dueDate is DateTime)
                deadline = (DateTime)dueDate;
            else if (!DateTime.TryParse(Convert.ToString(dueDate), out deadline))
                return false;
            return deadline.Date < DateTime.Today;
        }

        protected string GetQuickApproveConfirmScript(object paymentCode)
        {
            return BuildBrowserConfirmScript(
                BackEndResourceKeys.PAYMENT_QUICK_APPROVE_CONFIRM, paymentCode);
        }

        protected string GetDeleteConfirmScript(object paymentCode)
        {
            return BuildBrowserConfirmScript(
                BackEndResourceKeys.PAYMENT_DELETE_CONFIRM, paymentCode);
        }

        private string BuildBrowserConfirmScript(string resourceKey, object paymentCode)
        {
            string message = string.Format(GetResourceText(resourceKey), Convert.ToString(paymentCode));
            return string.Format(
                "if (!window.confirm('{0}')) {{ if ('checked' in this) this.checked = false; return false; }}",
                System.Web.HttpUtility.JavaScriptStringEncode(message));
        }

    }
}
