//-----------------------PROGRAMER LOGS---------------------------
using SweetSoft.QLDA.BackOffice.Controls.AutoComplete;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Helpers.Security.Encryption;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Windows.Documents;
namespace SweetSoft.QLDA.BackOffice.Common
{
    public class BaseAdminPage : System.Web.UI.Page
    {
        #region Function Format
        public virtual CultureInfo ENCulture
        {
            get { return new CultureInfo(LanguageHelpers.LanguageCode[SweetContext.Current.CurrentLanguageId]); }
        }
        public virtual NumberStyles ENNumberStyles
        {
            get { return NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands; }
        }

        public string ConvertNumber(object number)
        {
            if (number == null || string.IsNullOrEmpty(number.ToString()))
                return "0";
            decimal value;
            if (decimal.TryParse(number.ToString(), out value))
                return FormatHelpers.ConvertDecimalToStringByLanguage(value, SweetContext.Current.CurrentLanguageCode);
            return "0";
        }
        public string ConvertNumber(string number)
        {
            return FormatHelpers.ConvertDoubleToStringByLanguage(number, SweetContext.Current.CurrentLanguageCode);
        }
        public string ConvertNumber(double number)
        {
            return FormatHelpers.ConvertDoubleToStringByLanguage(number, SweetContext.Current.CurrentLanguageCode);
        }
        public string ConvertNumber(byte number)
        {
            return FormatHelpers.ConvertDoubleToStringByLanguage(number, SweetContext.Current.CurrentLanguageCode);
        }
        public string ConvertNumber(int number)
        {
            return FormatHelpers.ConvertDoubleToStringByLanguage(number, SweetContext.Current.CurrentLanguageCode);
        }
        public string ConvertNumber(decimal number, bool isRounding = false)
        {
            return FormatHelpers.ConvertDecimalToStringByLanguage(number, SweetContext.Current.CurrentLanguageCode);
        }

        public bool GetData(CtrExtraAutoComplete acbb, out decimal value)
        {
            return decimal.TryParse(
                GetData(acbb)
                //
                , NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands
                //"$6,032.51"
                //, NumberStyles.Number | NumberStyles.AllowCurrencySymbol
                , new CultureInfo(SweetContext.Current.CurrentLanguageCode, false), out value);
        }
        public void GetData(CtrExtraAutoComplete acbb, out Guid? id)
        {
            Guid temp;
            if (Guid.TryParse(acbb.Data, out temp))
                id = temp;
            else
                id = null;
        }
        public bool GetData(CtrExtraAutoComplete acbb, out Guid id)
        {
            return Guid.TryParse(acbb.Data, out id);
        }
        public string GetData(CtrExtraAutoComplete acbb)
        {
            return acbb.Data;
        }

        public void GetValue(TextBox input, out TimeSpan? timeSpan)
        {
            TimeSpan t;
            if (!TimeSpan.TryParse(input.Text, out t))
                timeSpan = null;

            timeSpan = t;
        }

        internal bool GetValue(TextBox textBox, out byte result)
        {
            return byte.TryParse(GetValue(textBox), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HiddenField hiddenField, out int result)
        {
            return int.TryParse(GetValue(hiddenField), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputHidden input, out byte result)
        {
            return byte.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputText input, out byte result)
        {
            return byte.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputHidden input, out Guid result)
        {
            return Guid.TryParse(GetValue(input), out result);
        }
        internal bool GetValue(TextBox textBox, out int result)
        {
            return int.TryParse(GetValue(textBox), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputHidden input, out int result)
        {
            return int.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputText input, out int result)
        {
            return int.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputHidden input, out long result)
        {
            return long.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputText input, out long result)
        {
            return long.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(ExtraTextBox input, out int result)
        {
            return int.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(ExtraTextBox input, out long result)
        {
            return long.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(TextBox textBox, out double result)
        {
            return double.TryParse(GetValue(textBox), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputHidden input, out double result)
        {
            return double.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputText input, out double result)
        {
            return double.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }

        internal bool GetValue(TextBox textBox, out decimal result)
        {
            return decimal.TryParse(GetValue(textBox), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputHidden input, out decimal result)
        {
            return decimal.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HtmlInputText input, out decimal result)
        {
            return decimal.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(HiddenField input, out Guid result)
        {
            result = Guid.Empty;
            if (string.IsNullOrEmpty(input.Value))
                return true;
            return Guid.TryParse(GetValue(input), out result);
        }
        //'**Change 03
        internal bool GetValue(ExtraDropdown input, out decimal result)
        {
            return decimal.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(ExtraDropdown input, out int result)
        {
            return int.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(ExtraDropdown input, out byte result)
        {
            return byte.TryParse(GetValue(input), ENNumberStyles, ENCulture, out result);
        }
        internal bool GetValue(ExtraDropdown input, out Guid result)
        {
            result = Guid.Empty;
            if (string.IsNullOrEmpty(input.SelectedValue))
                return true;
            return Guid.TryParse(GetValue(input), out result);
        }

        //'**Change 02
        internal string GetText(ExtraDropdown input)
        {
            return input.SelectedText;
        }
        internal string GetValue(ExtraDropdown input)
        {
            return input.SelectedValue;
        }
        internal string GetValue(TextBox textBox)
        {
            return textBox.Text.Trim();
        }
        internal string GetValue(HiddenField hiddenField)
        {
            return hiddenField.Value.Trim();
        }
        internal string GetValue(HtmlInputControl input)
        {
            return input.Value.Trim();
        }
        internal string GetValue(HtmlTextArea input)
        {
            return input.Value.Trim();
        }
        protected string GetValueTrimSpecialCharacter(HtmlInputControl input)
        {
            try
            {
                return Regex.Replace(input.Value, @"[^0-9]+", "");
            }
            catch
            {
                return input.Value.Trim();
            }

        }

        public string ConvertDateTimeToString(object dt, bool isTime = true)
        {
            try
            {
                if (dt == null)
                    return string.Empty;
                return FormatHelpers.ConvertDateTimeToStringByLanguage(dt, isTime, SweetContext.Current.CurrentLanguageId);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GenerateTagLinkDetail(object id, object text, string slug, bool isOpenNewTab = false, bool visible = true)
        {
            string tagLink = $"<span class='text-primary'>{text}</span>";
            if (visible && this.IsView)
            {
                string _slugUrl = this.GetRelativeClientPath($"/{slug}/" + SecurityUtilities.ProtectUrlParameter(id.ToString()));
                string target = "_self";
                if (isOpenNewTab)
                    target = "_blank";
                tagLink = $"<a href='{_slugUrl}' target='{target}' title='{text}'>{text}</a>";
            }
            return tagLink;
        }
        public string GenerateButtonEdit(object id, object text, string slug, bool isOpenNewTab = false, bool visible = true)
        {
            string tagButton = string.Empty;
            if (visible && this.IsEdit)
            {
                string _slugUrl = this.GetRelativeClientPath($"/{slug}/" + SecurityUtilities.ProtectUrlParameter(id.ToString()));
                string target = "_self";
                if (isOpenNewTab)
                    target = "_blank";

                tagButton = $"<a href='{_slugUrl}' target='{target}' title='{text}' class='btn btn-sm btn-info'><b><i class='icon fas fa-pencil-alt'></i></b></a>";
            }
            return tagButton;
        }

        public string DisplayName(object userName)
        {
            if (userName == null)
                return string.Empty;
            List<AspnetUser> aspnetUsers;
            if (!CacheManager.GetCacheData("SYSTEM_USER_LIST", out aspnetUsers))
            {
                aspnetUsers = UserManager.Instance.GetAllAspnetUsers();
                CacheManager.SetCacheData("SYSTEM_USER_LIST", aspnetUsers);
            }
            if (aspnetUsers == null)
                return string.Empty;
            var user = aspnetUsers.Find(t => t.UserName == userName.ToString());
            return user?.DisplayName ?? string.Empty;
        }
        #endregion

        #region Enum
        protected enum GridCommand
        {
            Update,
            Role,
            Delete
        }

        protected virtual bool IsUsingRole
        {
            get
            {
                return true;
            }
        }

        public enum byteCutEvent
        {
            Buttonclick,
            Gridclick
        }

        #endregion

        #region Properties
        public string FolderPath
        {
            get
            {
                return this.CURRENT_MASTERPAGE.FolderPath;
            }
            set
            {
                this.CURRENT_MASTERPAGE.FolderPath = value;
            }
        }
        public string FolderName
        {
            get
            {
                return this.CURRENT_MASTERPAGE.FolderName;
            }
            set
            {
                this.CURRENT_MASTERPAGE.FolderName = value;
            }
        }
        private string RETURN_URL
        {
            get;
        } = "/";
        public string ReturnTo
        {
            get
            {
                string url = CommonHelpers.QueryString("rt");
                if (string.IsNullOrEmpty(url))
                    url = GetRelativeClientPath(RETURN_URL);
                return GetRelativeClientPath(url);
            }
        }
        public SettingManager _settingManager;
        public string GetRelativeClientPath(string virtualPath = "/")
        {
            //if (!virtualPath.StartsWith("/AdminPanel") && !virtualPath.StartsWith("/AdminPanel"))
            //    virtualPath = string.Format("/AdminPanel/{0}", virtualPath.TrimStart('/'));
            return CommonHelpers.GetRelativeClientPath(Page, virtualPath);
        }
        public virtual SweetSoft.QLDA.Core.Functions.ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return SweetSoft.QLDA.Core.Functions.ModuleKeys.None; }
        }

        protected virtual void BindData()
        {

        }

        public virtual void LoadTab(string tabKey)
        {

        }

        protected string SessionPrefix = SweetContext.SessionPrefix;
        public ConfirmResult CurrentConfirmResult
        {
            get
            {
                if (Session["CurrentConfirmResult"] != null)
                    return (ConfirmResult)Session["CurrentConfirmResult"];
                return null;
            }
            set
            {
                Session["CurrentConfirmResult"] = value;
            }
        }

        public void CheckFunctionPermission(Guid userId)
        {
            if (!SweetContext.Current.CheckFunctionPermission(userId, PAGE_FUNCTION_CODE) && !IsLogin)
                Response.Redirect(GetRelativeClientPath("/403"), true);
        }

        public SweetSoft.QLDA.BackOffice.MasterPages.MasterTemplate CURRENT_MASTERPAGE
        {
            get
            {
                try
                {
                    System.Web.UI.MasterPage masterPage = this.Master;
                    return (SweetSoft.QLDA.BackOffice.MasterPages.MasterTemplate)masterPage;
                }
                catch (Exception exc)
                {
                    return null;
                }
            }
        }

        public string DefaultPassword
        {
            get
            {
                try
                {
                    if (WebConfigurationManager.AppSettings["DefaultPassword"] != null)
                        return WebConfigurationManager.AppSettings["DefaultPassword"];
                    return "sweet$$25";
                }
                catch
                {
                    return "sweet$$25";
                }
            }
        }
        #endregion

        #region Function
        public void SetMetaTagsOgTags(string pageTitle, string mKeywords = "", string mDescription = "", string ogUrl = "", string ogImage = "")
        {
            Title = !string.IsNullOrEmpty(pageTitle) ? pageTitle + " | " + _settingManager.GetSettingValue($"{SettingKeys.TitleOfWebsite}_{SweetContext.Current.CurrentLanguageCode}") : _settingManager.GetSettingValue($"{SettingKeys.TitleOfWebsite}_{SweetContext.Current.CurrentLanguageCode}");
        }
        public bool IsView
        {
            get
            {
                try
                {
                    return IsUserRight(ActionKeys.View
                        | ActionKeys.Create
                        | ActionKeys.Update);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsAdd
        {
            get
            {
                try
                {
                    return IsUserRight(ActionKeys.Create);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsEdit
        {
            get
            {
                try
                {
                    return IsUserRight(ActionKeys.Update);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsDelete
        {
            get
            {
                try
                {
                    return IsUserRight(ActionKeys.Delete);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsExportExcel
        {
            get
            {
                try
                {
                    return IsUserRight(ActionKeys.Export);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsExportPdf
        {
            get
            {
                try
                {
                    return IsUserRight(ActionKeys.Export);
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool IsUserRight(ActionKeys permissionKeys, ModuleKeys? module = null)
        {
            try
            {
                if (module == null)
                    module = this.PAGE_FUNCTION_CODE;
                Guid userId = SweetContext.Current.UserId;
                if (userId == Guid.Empty)
                    return false;

                Dictionary<string, bool> dicRights = AppCache.Get($"USER_HAS_RIGHTS_{userId}") as Dictionary<string, bool>
                                                     ?? new Dictionary<string, bool>();

                foreach (ActionKeys key in Enum.GetValues(typeof(ActionKeys)))
                {
                    if (!permissionKeys.HasFlag(key) || key == ActionKeys.None)
                        continue;

                    string cacheKey = $"{userId}_{module}_{key}";

                    if (!dicRights.TryGetValue(cacheKey, out bool isRight))
                    {
                        isRight = FunctionManager.Instance.IsActionKeyExisted(userId, module.Value, key);
                        dicRights[cacheKey] = isRight;
                    }

                    if (isRight)
                    {
                        AppCache.Remove($"USER_HAS_RIGHTS_{userId}");
                        AppCache.Insert($"USER_HAS_RIGHTS_{userId}", dicRights);
                        return true;
                    }
                }

                AppCache.Remove($"USER_HAS_RIGHTS_{userId}");
                AppCache.Insert($"USER_HAS_RIGHTS_{userId}", dicRights);
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetStatusTextOnOff(object strStatus)
        {
            try
            {
                switch (strStatus.ToString())
                {
                    case "True":
                    case "true":
                    case "TRUE":
                    case "1":
                        return $"<span class='badge rounded-pill bg-success'>{GetResourceText(BackEndResourceKeys.ON)}</span>";
                    case "False":
                    case "false":
                    case "FALSE":
                    case "0":
                        return $"<span class='badge rounded-pill bg-danger'>{GetResourceText(BackEndResourceKeys.OFF)}</span>";
                    default:
                        return string.Empty;
                }
            }
            catch
            {
                return strStatus.ToString();
            }
        }
        public string GetStatusText(object strStatus)
        {
            try
            {
                switch (strStatus.ToString())
                {
                    case "True":
                    case "true":
                    case "TRUE":
                    case "1":
                        return $"<span class='badge rounded-pill bg-success'>{GetResourceText(BackEndResourceKeys.ACTIVE)}</span>";
                    case "False":
                    case "false":
                    case "FALSE":
                    case "0":
                        return $"<span class='badge rounded-pill bg-danger'>{GetResourceText(BackEndResourceKeys.INACTIVE)}</span>";
                    default:
                        return string.Empty;
                }
            }
            catch
            {
                return strStatus.ToString();
            }
        }
        public string GetCompetitionStatusText(object dateTime, object Completed)
        {
            try
            {
                if (Completed != null)
                {
                    bool isCompleted = false;
                    bool.TryParse(Completed.ToString(), out isCompleted);
                    if (isCompleted)
                    {
                        return $"<span class='badge rounded-pill bg-success'>{GetResourceText(BackEndResourceKeys.COMPLETED)}</span>";
                    }
                }
                if (dateTime == null)
                {
                    return string.Empty;
                }
                DateTime dateTime1 = DateTime.UtcNow;
                DateTime.TryParse(dateTime.ToString(), out dateTime1);

                if (dateTime1 < DateTime.UtcNow)
                    return $"<span class='badge rounded-pill bg-danger'>{GetResourceText(BackEndResourceKeys.IN_PROGRESS)}</span>";
                else
                    return $"<span class='badge rounded-pill bg-primary'>{GetResourceText(BackEndResourceKeys.NOT_STARTED)}</span>";
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetResourceText(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
                return "";
            //'**Change 02
            return Server.HtmlDecode(UITextsReader.GetBackEndResourceText(new CultureInfo(SweetContext.Current.CurrentLanguageCode), messageId));
            //return LanguageHelpers.GetResourceText(SweetContext.Current.CurrentLanguageCode, messageId);
        }

        /// <param name="scriptName"></param>
        /// <param name="param"></param>
        public void RunScript(string scriptName, string param)
        {
            string script = string.Format("{0}({1});", scriptName, param);
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "RunScript", script, true);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
        }

        /// <summary>
        /// Init define language to display
        /// </summary>
        protected override void InitializeCulture()
        {
            base.InitializeCulture();
            //If user define a language, save and remember this selected language
            //Otherwise, use previous selected language
            byte languageId;
            try
            {
                if (byte.TryParse(Request["langId"], out languageId))
                    SweetContext.Current.CurrentLanguageId = languageId;
            }
            catch
            {
                languageId = LanguageHelpers.Defaultlanguage;
            }

            string culture = SweetContext.Current.CurrentLanguageCode;

            UICulture = culture;
            Culture = culture;

            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(culture);
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
        }
        #endregion

        #region MasterPage Functions

        /// <param name="message">message</param>
        /// <param name="result">result</param>
        /// <param name="isClosePostBack">close dialog client (no postback)</param>
        public virtual void OpenMessageBox(MessageBox message, ConfirmResult result, bool isClosePostBack, bool showmodal, int timeOut = 15000)
        {
            if (CURRENT_MASTERPAGE != null)
            {
                CURRENT_MASTERPAGE.OpenMessageBox(message, result, isClosePostBack, showmodal, timeOut);
            }
        }

        public virtual void CloseMessageBox()
        {
            if (CURRENT_MASTERPAGE != null)
            {
                CURRENT_MASTERPAGE.CloseMessageBox();
            }
        }
        public virtual void DataCallback(string key, object value, object valueText)
        {

        }
        #endregion

        #region Inherit Functions
        private bool m_IsLogin = false;
        public virtual bool IsLogin
        {
            get
            {
                return m_IsLogin;
            }
            set
            {
                m_IsLogin = value;
            }
        }

        /// <param name="e"></param>
        public virtual void ConfirmRequest(ConfirmResult e)
        {
            if (e.FireInControl)
            {
                BaseAdminPage basepage = FindControl(e.ControlName) as BaseAdminPage;
                if (basepage != null)
                {
                    basepage.ConfirmRequest(e);
                    return;
                }
            }
        }

        /// <param name="e"></param>
        public virtual void CloseRequest(ConfirmResult e)
        {
            if (e.FireInControl)
            {
                BaseAdminPage basepage = FindControl(e.ControlName) as BaseAdminPage;
                if (basepage != null)
                {
                    basepage.CloseRequest(e);
                    return;
                }
            }
        }

        protected virtual void PageResourceText() { }

        #endregion
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PageResourceText();
            if (base.Form != null)
                base.Form.Action = Request.RawUrl;

            if (IsLogin)
            {
                _settingManager = new SettingManager();
                return;
            }
            if (SweetContext.Current.User == null)
                CheckSessionWithCookie();
            CheckFunctionPermission(SweetContext.Current.UserId);
            _settingManager = new SettingManager();
            //-----------------------------------------
            string preventScript = "";

            if (_settingManager.GetSettingValueBoolean(SettingKeys.PreventSelection, false))
            {
                preventScript += @"
        document.addEventListener('DOMContentLoaded', function () {
            document.body.style.userSelect = 'none';
            document.body.style.webkitUserSelect = 'none';
            document.body.style.msUserSelect = 'none';
            document.body.style.mozUserSelect = 'none';

            document.addEventListener('selectstart', function(e) {
                e.preventDefault();
            });
        });
    ";
            }

            if (_settingManager.GetSettingValueBoolean(SettingKeys.PreventRightClick, false))
            {
                preventScript += @"
        document.addEventListener('contextmenu', function(e) {
            e.preventDefault();
        });

        document.addEventListener('dragstart', function(e) {
            e.preventDefault();
        });

        document.addEventListener('mousedown', function(e) {
            if (e.button === 2 || e.button === 3) {
                e.preventDefault();
            }
        });
    ";
            }

            ClientScript.RegisterClientScriptBlock(
                typeof(string),
                "preventscripts",
                preventScript,
                true
            );

            //-----------------------------------------
        }
        private static string COOKIE_KEY = ConfigurationManager.AppSettings["CookieKeyPanel"];
        private void CheckSessionWithCookie()
        {
            string returnUrl = Request.RawUrl;
            bool isRedirectLogin = false;
            try
            {
                HttpCookie ck = Request.Cookies[COOKIE_KEY];
                if (SweetContext.Current.User == null)
                {
                    if (ck == null || string.IsNullOrEmpty(ck.Value))
                    {
                        isRedirectLogin = true;
                        goto check;
                    }

                    Guid userId = Guid.Empty;
                    Guid.TryParse(SecurityUtilities.DecryptContent(ck.Value), out userId);
                    if (userId == Guid.Empty)
                    {
                        isRedirectLogin = true;
                        goto check;
                    }
                    AspnetUser user = UserManager.Instance.GetUserById(userId);
                    if (user == null || !user.IsActivated
                        || user.IsDeleted)
                    {
                        isRedirectLogin = true;
                        goto check;
                    }
                    SweetContext.Current.User = user;
                    SweetContext.Current.UserName = user.UserName;
                    SweetContext.Current.UserId = user.UserId;
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    FormsAuthentication.RedirectFromLoginPage(user.UserName, true);
                    WriterValueToCookie(SecurityUtilities.EncryptContent(user.UserId.ToString()));
                }
                else
                {
                    AspnetUser user = SweetContext.Current.User;
                    if (user == null || !user.IsActivated
                        || user.IsDeleted)
                    {
                        isRedirectLogin = true;
                        goto check;
                    }
                    SweetContext.Current.User = user;
                    SweetContext.Current.UserName = user.UserName;
                    SweetContext.Current.UserId = user.UserId;
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    FormsAuthentication.RedirectFromLoginPage(user.UserName, true);
                    WriterValueToCookie(SecurityUtilities.EncryptContent(user.UserId.ToString()));
                }
            }
            catch
            {
                isRedirectLogin = true;
            }
        check:
            if (isRedirectLogin && !IsLogin)
            {
                SweetContext.ClearAdminData();
                WriterValueToCookie(string.Empty);
                Session.Abandon();
                if (string.IsNullOrEmpty(returnUrl))
                    Response.Redirect(GetRelativeClientPath("/Login"));
                else
                    Response.Redirect(GetRelativeClientPath(string.Format("/Login?rt={0}", returnUrl)));
                return;
            }

            bool isLockScreen = false;
            object lockCache = AppCache.Get(string.Format("ASP.NET_LockedId_{0}", SweetContext.Current.UserName));
            if (lockCache != null)
            {
                try
                {
                    if (!bool.TryParse(lockCache.ToString(), out isLockScreen))
                        isLockScreen = false;
                }
                catch
                {
                    isLockScreen = false;
                }
            }
            if (isLockScreen && !IsLogin)
            {
                Response.Redirect(GetRelativeClientPath("/lock-screen"), true);
                return;
            }
            return;
        }
        public void ExpireAllCookies()
        {
            if (Request.Cookies[COOKIE_KEY] != null)
            {
                Response.Cookies[COOKIE_KEY].Value = string.Empty;
                Response.Cookies.Set(Request.Cookies[COOKIE_KEY]);
            }
            else
                Response.Cookies.Set(new HttpCookie(COOKIE_KEY, string.Empty));

            Response.Cookies[COOKIE_KEY].Expires = DateTime.UtcNow.AddDays(-1);
        }
        public virtual void WriterValueToCookie(string _value)
        {
            try
            {
                if (HttpContext.Current.Request.Cookies[COOKIE_KEY] != null)
                {
                    HttpContext.Current.Request.Cookies[COOKIE_KEY].Value = _value;
                    HttpContext.Current.Response.Cookies.Set(HttpContext.Current.Request.Cookies[COOKIE_KEY]);
                }
                else
                    HttpContext.Current.Response.Cookies.Set(new HttpCookie(COOKIE_KEY, _value));
                HttpContext.Current.Response.Cookies[COOKIE_KEY].Expires = DateTime.UtcNow.AddDays(7);
            }
            catch (Exception exc)
            {
                throw new Exception("Write cookie", exc);
            }
        }
        #region Event

        protected override void OnLoad(EventArgs e)
        {
            if (CURRENT_MASTERPAGE != null)
            {
                HtmlInputHidden hdfFunction = CURRENT_MASTERPAGE.FindControl("hdfFunction") as HtmlInputHidden;
                if (hdfFunction != null)
                    hdfFunction.Value = this.PAGE_FUNCTION_CODE.ToString();
            }

            base.OnLoad(e);
            //AjaxControlsPerformance();
        }

        #endregion

        //---------------------
        //'**Change 02
        internal void ProcessException(Exception exc, string mes = "")
        {
            //if (mes == "")
            //    mes = new FunctionCode(this.PAGE_FUNCTION_CODE).GetText();
            ShowNotify(HttpUtility.UrlEncode($"[{mes}]: {exc.Message}"), MSGType.Error);
            throw new Exception(mes, exc);
        }
        internal void ShowResourceTextNotify(string resourceKey)
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Info, GetResourceText(resourceKey));
        }
        internal void ShowNotify(BusinessException exc)
        {
            CloseLoading();
            string userFriendlyField = string.Empty;
            if (exc.FieldName != null)
            {
                userFriendlyField = FieldNameDisplayMapper.GetFieldLabel(exc.FieldName);
                if (userFriendlyField == exc.FieldName)
                    userFriendlyField = GetResourceText(exc.FieldName.ToUpper());
            }
            string message = string.IsNullOrEmpty(userFriendlyField)
                ? exc.Message
                : $"{userFriendlyField}: {exc.Message.ToLower()}";
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Info, HttpUtility.HtmlEncode(message));
        }
        internal void ShowNotify(string message)
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Info, HttpUtility.HtmlEncode(message));
        }
        internal void ShowNotify(string message, string type)
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(type, HttpUtility.HtmlEncode(message));
        }
        internal void ShowInvalidDataError()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Error, GetResourceText(BackEndResourceKeys.INACCURATE_DATA_PLEASE_RELOAD_THE_PAGE_AND_TRY_AGAIN));
        }
        internal void ShowInvalidNotFoundData()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Error, GetResourceText(BackEndResourceKeys.INACCURATE_DATA_PLEASE_RELOAD_THE_PAGE_AND_TRY_AGAIN));
        }
        internal void NoDataSelectedForDeletion()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Warning, GetResourceText(BackEndResourceKeys.NO_DATA_SELECTED_FOR_DELETE));
        }
        internal void ShowSuccessSaveData()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Success, GetResourceText(BackEndResourceKeys.DATA_HAS_BEEN_UPDATED_SUCCESSFULLY));
        }
        internal void ShowSuccessAddNewData()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Success, GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
        }
        internal void ShowSuccessDeleteData()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Info, GetResourceText(BackEndResourceKeys.DATA_DELETED_SUCCESSFULLY));
        }
        internal void ShowAccessDeniedNotify()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Warning, GetResourceText(BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
        }
        internal void ShowSystemError()
        {
            CloseLoading();
            Notify notify = Notify.Instance(this.Page);
            notify.ShowNotify(MSGType.Error, GetResourceText(BackEndResourceKeys.SYSTEM_ERROR_PLEASE_RELOAD_THE_PAGE_AND_TRY_AGAIN));
        }
        internal void OpenDialog(string selector, string title)
        {
            string script = string.Format("CMSMasterJs.OpenDialog('{0}','{1}');", selector, title.Replace("'", "''"));
            string key = "OpenDialog" + selector;
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), key, script, true);
        }
        internal void CloseDialog(string selector)
        {
            string script = string.Format("CMSMasterJs.CloseDialog('{0}');", selector);
            string key = "CloseDialog" + selector;
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), key, script, true);
        }
        internal void PaceRestart()
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "PaceRestart", "CMSMasterJs.PaceRestart();", true);
        }
        internal void CloseLoading()
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "CloseLoading", "setTimeout(function(){CMSMasterJs.CloseLoading();},300);", true);
        }
    }

    public class ConfirmResult
    {
        private bool submit = false;
        private bool fireInControl = false;
        private object value = null;
        private string controlName = string.Empty;
        private string commandName = string.Empty;
        public ConfirmResult() { }
        public bool Submit
        {
            get { return this.submit; }
            set { this.submit = value; }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        public bool FireInControl
        {
            get { return this.fireInControl; }
            set { this.fireInControl = value; }
        }

        public string ControlName
        {
            get { return this.controlName; }
            set { this.controlName = value; }
        }

        public string CommandName
        {
            get { return this.commandName; }
            set { this.commandName = value; }
        }
    }
    public class Notify
    {
        private Page _page;
        private static Notify _instance;
        private static object syncLock = new object();
        // Constructor is 'protected'
        public static Notify Instance(Page page)
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new Notify(page);
                    }
                }
            }
            _instance._page = page;
            return _instance;
        }
        private Notify(Page page)
        {
            this._page = page;
        }
        public void ShowNotify(string type, string message)
        {
            string decodeStr = HttpUtility.HtmlDecode(message.Trim()).Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");
            switch (type)
            {
                case MSGType.Success:
                    ScriptManager.RegisterClientScriptBlock(_page, typeof(string), "ShowNotify", $"setTimeout(function(){{CMSMasterJs.ShowNotify('{decodeStr}','success');}},500);", true);
                    break;
                case MSGType.Danger:
                case MSGType.Error:
                    ScriptManager.RegisterClientScriptBlock(_page, typeof(string), "ShowNotify", $"setTimeout(function(){{CMSMasterJs.ShowNotify('{decodeStr}','error');}},500);", true); 
                    break;
                case MSGType.Warning:
                    ScriptManager.RegisterClientScriptBlock(_page, typeof(string), "ShowNotify", $"setTimeout(function(){{CMSMasterJs.ShowNotify('{decodeStr}','warning');}},500);", true);
                    break;
                default:
                    ScriptManager.RegisterClientScriptBlock(_page, typeof(string), "ShowNotify", $"setTimeout(function(){{CMSMasterJs.ShowNotify('{decodeStr}','info');}},500);", true);
                    break;
            }
        }
    }
    public class MSGType
    {
        public const string Success = "success";
        public const string Danger = "danger";
        public const string Error = "error";
        public const string Warning = "warning";
        public const string Info = "info";
    }
    public class ValidationEngine
    {
        private Page _page;
        private static ValidationEngine _instance;
        private static object syncLock = new object();
        private List<string> m_PromptControdClientIDs;
        private List<string> PromptControdClientIDs
        {
            get
            {
                if (m_PromptControdClientIDs == null)
                    m_PromptControdClientIDs = new List<string>();
                return m_PromptControdClientIDs;
            }
            set
            {
                m_PromptControdClientIDs = value;
            }
        }

        private List<string> m_PromptErrorMessages;
        private List<string> PromptErrorMessages
        {
            get
            {
                if (m_PromptErrorMessages == null)
                    m_PromptErrorMessages = new List<string>();
                return m_PromptErrorMessages;
            }
            set
            {
                m_PromptErrorMessages = value;
            }
        }
        // Constructor is 'protected'
        public static ValidationEngine Instance(Page page)
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new ValidationEngine(page);
                    }
                }
            }
            _instance._page = page;
            _instance.PromptControdClientIDs = null;
            _instance.PromptErrorMessages = null;
            return _instance;
        }
        private ValidationEngine(Page page)
        {
            this._page = page;
        }
        public void AddErrorPrompt(string clientID, string message)
        {
            string msgValid = string.Format("{0}", message);
            int u = -1;
            if (u > -1)
            {
                if (!string.IsNullOrEmpty(PromptErrorMessages[u]))
                {
                    PromptErrorMessages[u] += msgValid;
                }
            }
            else
            {
                PromptControdClientIDs.Add(clientID);
                PromptErrorMessages.Add(msgValid);
            }
        }
        public void ShowErrorPrompt()
        {
            if (PromptControdClientIDs == null || PromptControdClientIDs.Count <= 0)
                return;
            StringBuilder promptScript = new StringBuilder();
            for (int i = 0; i < PromptControdClientIDs.Count; i++)
                promptScript.AppendFormat("$('#{0}').validationEngine('showPrompt', '{1}', 'error','topLeft', true);  ", PromptControdClientIDs[i], PromptErrorMessages[i].Replace("'", "\'"));
            string script = "setTimeout(function(){" + promptScript.ToString() + "},100)";
            ScriptManager.RegisterStartupScript(_page, typeof(string), "RunScript", script, true);
            ScriptManager.RegisterClientScriptBlock(_page, typeof(string), "HideAllValidatorPrompts", "CMSMasterJs.HideAllValidatorPrompts();", true);
        }
        public bool IsValid
        {
            get
            {
                if (PromptControdClientIDs == null || PromptErrorMessages == null)
                    return true;
                return PromptControdClientIDs.Count == 0 && PromptErrorMessages.Count == 0;
            }
        }

        public void CheckValidControls(ControlCollection controls)
        {
            if (controls == null || controls.Count == 0)
                return;
            foreach (System.Web.UI.Control control in controls)
            {
                #region Extra Controls
                if (control is ExtraTextBox)
                {
                    var textBox = (ExtraTextBox)control;
                    if (textBox == null
                        || !textBox.Required
                        || !textBox.Enabled
                        || !string.IsNullOrEmpty(textBox.SearchColumn))
                        continue;
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        if (textBox.IsIMask || textBox.IsNumber)
                            AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_VALUE));
                        else if (textBox.RequiredAdvanced.Contains("phone"))
                            AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_PHONE_NUMBER));
                        else if (textBox.RequiredAdvanced.Contains("email"))
                            AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_EMAIL));
                        else if (textBox.TextMode == TextBoxMode.Password)
                        {
                            string message = "";
                            if (!RegexUtilities.IsValidPassword(textBox.Text, ref message))
                                AddErrorPrompt(textBox.ClientID, message);
                        }
                        else if (textBox.MaxLength > 0 && textBox.Text.Length > textBox.MaxLength)
                            AddErrorPrompt(textBox.ClientID, string.Format(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE)
                                , textBox.MaxLength));
                        else if (textBox.IsNumber && textBox.MaxValue != 0)
                        {
                            decimal temp = 0;
                            if (!decimal.TryParse(textBox.Text, out temp))
                                AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_VALUE));
                            if (temp > textBox.MaxValue)
                                AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_VALUE));
                        }
                        else
                            AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                    }
                    else
                    {
                        if (textBox.IsEmail && !RegexUtilities.IsValidEmail(textBox.Text))
                            AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_EMAIL));
                        else if (textBox.IsPhone && !RegexUtilities.IsValidPhone(textBox.Text))
                            AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_PHONE_NUMBER));
                        else if (textBox.MaxLength > 0 && textBox.Text.Length > textBox.MaxLength)
                            AddErrorPrompt(textBox.ClientID, string.Format(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE)
                                , textBox.MaxLength));
                        else if ((textBox.IsNumber || textBox.IsCurrency || textBox.IsIMask))
                        {
                            decimal temp = 0;
                            if (textBox.MaxValue != int.MaxValue)
                            {
                                if (!decimal.TryParse(textBox.Text, out temp))
                                    AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_VALUE));
                                if (temp > textBox.MaxValue)
                                    AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_VALUE));
                            }
                            if (textBox.MinValue != int.MinValue)
                            {
                                temp = 0;
                                if (!decimal.TryParse(textBox.Text, out temp))
                                    AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_VALUE));
                                if (temp < textBox.MinValue)
                                    AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INVALID_VALUE));
                            }
                        }
                    }
                }
                else if (control is ExtraDropdown)
                {
                    var dropdown = (ExtraDropdown)control;
                    if (dropdown == null
                        || !dropdown.Required
                        || !dropdown.Enabled
                        || !string.IsNullOrEmpty(dropdown.SearchColumn))
                        continue;
                    if (dropdown.Required && string.IsNullOrEmpty(dropdown.SelectedValue))
                        AddErrorPrompt(dropdown.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                else if (control is ExtraDateTime)
                {
                    var dateTime = (ExtraDateTime)control;
                    if (dateTime == null
                        || !dateTime.Required
                        || !dateTime.Enabled) //|| !string.IsNullOrEmpty(dateTime.SearchColumn)
                        continue;
                    if (dateTime.DateValue == DateTime.MinValue || dateTime.DateValue == DateTimeHelper.MinValueSQL)
                        AddErrorPrompt(dateTime.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                else if (control is TextBox)
                {
                    var textBox = (TextBox)control;
                    if (textBox == null || !textBox.CausesValidation)
                        continue;
                    if (string.IsNullOrEmpty(textBox.Text))
                        AddErrorPrompt(textBox.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }
                else if (control is DropDownList)
                {
                    var dropDown = (DropDownList)control;
                    if (dropDown == null || !dropDown.CausesValidation)
                        continue;
                    if (string.IsNullOrEmpty(dropDown.SelectedValue))
                        AddErrorPrompt(dropDown.ClientID, UITextsReader.GetBackEndResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                #endregion
                if (control.HasControls())
                {
                    CheckValidControls(control.Controls);
                }
            }
        }
    }
}