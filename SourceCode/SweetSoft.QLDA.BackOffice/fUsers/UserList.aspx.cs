using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.ExpressionGraph.FunctionCompilers;
using OfficeOpenXml.Style;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.ExcelManager;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;

namespace SweetSoft.QLDA.BackOffice.fUsers
{
    public partial class UserList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.User;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlUsers1.NewUserHandlerCallback += NewUserAction;
            CtrlUsers1.EditUserHandlerCallback += EditUserAction;
            CtrlUsers1.SendMailHandlerCallback += SendMail;

            // Lắng nghe sự kiện Lưu thành công từ Popup để Rebind lại lưới
            CtrlUserPopup1.SavedHandlerCallback += (s, ev) => { CtrlUsers1.Rebind(); };

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.USER_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.USER_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
        {
            {RewriteURLHelper.Users, GetResourceText(BackEndResourceKeys.USER_LIST) }
        };
                CtrlUserPopup1.InitControls();
                CtrlUsers1.InitControls();

                string userQueryId = CommonHelpers.QueryString("userId");
                if (string.IsNullOrEmpty(userQueryId)) return;
                if (!Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(userQueryId), out Guid tempId)) return;

                EditUserAction(tempId, EventArgs.Empty);
            }
        }
        private void NewUserAction(object sender, EventArgs e)
        {
            // Chỉ việc gọi hàm AddNew của Control, nó sẽ tự xử lý UI
            CtrlUserPopup1.AddNew();
        }

        private void EditUserAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }
            Guid userId = (Guid)sender;

            if (userId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            CtrlUserPopup1.Edit(userId);
        }

        #region Email Sender
        private void SendMail(object sender, EventArgs e)
        {
            if (sender == null)
                return;
            dynamic dic = (dynamic)sender;
            if (dic == null)
                return;
            AspnetUser user = dic.User;
            if (user == null)
                return;
            if (string.IsNullOrEmpty(user.Email))
            {
                MembershipUser membershipUser = Membership.GetUser(user.UserName);
                if (membershipUser != null)
                    user.Email = membershipUser.Email;
            }
            if (string.IsNullOrEmpty(user.Email))
                return;
            string password = dic.Password;
            string companyName = SettingManager.Instance.GetSettingValue(SettingKeys.CompanyName);
            string companyEmail = SettingManager.Instance.GetSettingValue(SettingKeys.CompanyEmail);
            var appContext = SweetContext.Current;
            string hostPath = CommonHelpers.GetHostPath().TrimEnd('/');
            Task.Run(async () =>
            {
                await new EmailManager(appContext).SendEmailWithTemplateAsync(
                 user.UserId,
                 EmailType.System,
                 user.UserId,
                 user.Email,
                 EmailTemplateKeys.AdminTemplate.TemplateAccountInformation,
                 EmailFormatTypes.Admin,
                 new Dictionary<string, string>
                 {
                    { EmailKeys.USER_NAME, user.UserName },
                    { EmailKeys.FULL_NAME, user.DisplayName },
                    { EmailKeys.PASSWORD, password },
                    { EmailKeys.EMAIL, user.Email },
                    { EmailKeys.PHONE_NUMBER, user.MobileAlias },
                    { EmailKeys.LOGIN_URL, $"{hostPath}/login" },
                    { EmailKeys.COMPANY_NAME, companyName },
                    { EmailKeys.CURRENT_YEAR, DateTime.UtcNow.Year.ToString() },
                    { EmailKeys.SUPPORT_EMAIL, companyEmail },

                 },
                 attachments: null,
                 useBackgroundThread: true
             );
            });
        }
        #endregion

        #region Button

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlUsers1.ConfirmRequest(e);
        }
        #endregion

    }
}