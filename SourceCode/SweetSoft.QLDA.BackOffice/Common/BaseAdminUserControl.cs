//-----------------------PROGRAMER LOGS---------------------------

using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public class BaseAdminUserControl : UserControl
    {
        public void ProcessException(Exception exc, string mes = "")
        {
            this.CURRENT_PAGE.ProcessException(exc, mes);
        }
        public string GetRelativeClientPath(string virtualPath = "")
        {
            return this.CURRENT_PAGE.GetRelativeClientPath(virtualPath);
        }
        public void ShowAccessDeniedNotify()
        {
            CURRENT_PAGE.ShowAccessDeniedNotify();
        }
        public void ShowInvalidDataError()
        {
            CURRENT_PAGE.ShowInvalidDataError();
        }
        public void ShowInvalidNotFoundData()
        {
            CURRENT_PAGE.ShowInvalidNotFoundData();
        }
        public void ShowSystemError()
        {
            CURRENT_PAGE.ShowSystemError();
        }
        public void ShowSuccessSaveData()
        {
            CURRENT_PAGE.ShowSuccessSaveData();
        }
        public void ShowSuccessDeleteData()
        {
            CURRENT_PAGE.ShowSuccessDeleteData();
        }
        public void ShowNotify(string message)
        {
            CURRENT_PAGE.ShowNotify(message);
        }
        public void ShowNotify(string message, string type)
        {
            CURRENT_PAGE.ShowNotify(message, type);
        }
        public string GetResourceText(string messageId)
        {
            return CURRENT_PAGE.GetResourceText(messageId);
        }
        public string ConvertNumber(object number)
        {
            if (number == null || string.IsNullOrEmpty(number.ToString()))
                return "0";
            decimal value;
            if (decimal.TryParse(number.ToString(), out value))
                return FormatHelpers.ConvertDecimalToStringByLanguage(value);
            return "0";
        }
        public string FormatNumber(object number)
        {
            if (number == null || string.IsNullOrEmpty(number.ToString()))
                return "0";
            decimal value;
            if (decimal.TryParse(number.ToString(), out value))
                return FormatHelpers.ConvertDecimalToStringByLanguage(value, SweetContext.Current.CurrentLanguageCode, true);
            return "0";
        }
        protected BaseAdminPage CURRENT_PAGE
        {
            get
            {
                BaseAdminPage page = Page as BaseAdminPage;
                if (page == null)
                    throw new Exception("The page does not exist in current context. ");
                else
                    return page;
            }
        }

        //'**Change 01: add parameter - timeOut
        public virtual void OpenMessageBox(MessageBox message, ConfirmResult result, bool isClosePostBack, bool showmodal, int timeOut = 15000)
        {
            CURRENT_PAGE.OpenMessageBox(message, result, isClosePostBack, showmodal, timeOut);
        }
        public virtual void DataCallback(string key, object value, object valueText)
        {
            this.CURRENT_PAGE.DataCallback(key, value, valueText);
        }
        public virtual void ConfirmRequest(ConfirmResult e) { }
        public virtual void CloseRequest(ConfirmResult e) { }
        public virtual string ConvertDateTimeToString(object dt, bool isTime = true)
        {
            return this.CURRENT_PAGE.ConvertDateTimeToString(dt, isTime);
        }
    }
}