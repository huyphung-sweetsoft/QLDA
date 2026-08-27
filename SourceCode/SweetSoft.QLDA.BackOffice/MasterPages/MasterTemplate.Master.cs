//------------------PROGRAMER LOGS------------------------
//Created by:
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Expressions;
using System.Web.UI.WebControls.WebParts;
namespace SweetSoft.QLDA.BackOffice.MasterPages
{
    public partial class MasterTemplate : System.Web.UI.MasterPage
    {
        public string FolderPath
        {
            get
            {
                if (ViewState["_FolderPath"] == null || string.IsNullOrEmpty((string)ViewState["_FolderPath"]))
                    return "~/uploads";
                return (string)ViewState["_FolderPath"];
            }
            set
            {
                ViewState["_FolderPath"] = value;
            }
        }
        public string FolderName
        {
            get
            {
                if (ViewState["_FolderName"] == null || string.IsNullOrEmpty((string)ViewState["_FolderName"]))
                    ViewState["_FolderName"] = "File Manager";
                return (string)ViewState["_FolderName"];
            }
            set
            {
                ViewState["_FolderName"] = value;
            }
        }
        protected string GetRelativeClientPath(string virtualPath)
        {
            //if (!virtualPath.ToLower().StartsWith("/adminpanel") && !virtualPath.ToLower().StartsWith("adminpanel"))
            //    virtualPath = string.Format("/AdminPanel/{0}", virtualPath.TrimStart('/'));
            return CommonHelpers.GetRelativeClientPath(Page, virtualPath);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            scriptManager.RegisterAsyncPostBackControl(btnRefreshUser);
            scriptManager.RegisterAsyncPostBackControl(btnLoadTab);
            scriptManager.RegisterAsyncPostBackControl(btnRefreshPermission);
            scriptManager.RegisterAsyncPostBackControl(lbtLogOut);
            if (!IsPostBack)
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                lbVersion.InnerText = version;
                if (!string.IsNullOrEmpty(FolderPath))
                    hdfFolderKey.Value = SecurityUtilities.ProtectUrlParameter(FolderPath);
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
                if (isLockScreen)
                    Response.Redirect(GetRelativeClientPath("/lock-screen"), true);

                //string theme = "light";
                //if (Request.Cookies["data-layout-mode"] != null)
                //    theme = Request.Cookies["data-layout-mode"].Value;
                //tagBody.Attributes["data-layout-mode"] = theme;
                //tagBody.Attributes["data-topbar"] = theme;
                //tagBody.Attributes["data-sidebar"] = theme;
                byte currentLangId = SweetContext.Current.CurrentLanguageId;
                BindLanguages(currentLangId);
                imgLanguage.Src = LanguageHelpers.GetCMSLanguageImage(currentLangId);
                AspnetUser user = SweetContext.Current.User;
                if (user != null)
                {
                    hdfCSRF.Value = SecurityUtilities.EncryptContent(user.UserId.ToString());
                    TblUploadFile tblUploadFile = UploadManager.Instance.GetUploadFileByRefIdAndRefType(user.UserId, FileUploadTypes.UserAvatar);
                    SetUserInfomation(user.DisplayName, tblUploadFile?.FileUrl ?? string.Empty);
                }
                btnCancel.ToolTip = btnCancel.Text = GetResourceText(BackEndResourceKeys.CLOSE);
                BindLeftMenu();
            }
        }

        private void BindLeftMenu()
        {
            try
            {
                string MENU_CACHE_KEY = $"MENU_LEFT_CMS_{SweetContext.Current.UserId}_{SweetContext.Current.CurrentLanguageId}";
                string menuCache = AppCache.Get(MENU_CACHE_KEY) as string;
                if (string.IsNullOrEmpty(menuCache))
                {
                    bool isDev = AppSettingHelpers.GetSetting<bool>("IsDevelopment");
                    List<AspnetFunction> aspnetFunctions = FunctionManager.Instance.GetAspnetFunctionByUserId(SweetContext.Current.UserId, isDev);
                    if (aspnetFunctions == null)
                        return;

                    aspnetFunctions.RemoveAll(s => s == null);

                    // Đếm số lượng submenu (ParentCode khác rỗng hoặc null)
                    var subMenus = aspnetFunctions.Where(f => !string.IsNullOrEmpty(f.ParentCode)).ToList();

                    if (subMenus.Count < 1)
                    {
                        StringBuilder sbBasic = new StringBuilder();
                        var menu = aspnetFunctions.FirstOrDefault(f => f.FunctionCode == ModuleKeys.JoinCompetition.ToString());
                        if (menu != null)
                        {
                            sbBasic.AppendFormat(itemTemplateSingleParent.InnerHtml,
                                         GetResourceText(menu.FunctionName),
                                         menu.Icon,
                                         !string.IsNullOrEmpty(menu.PageUrl) ? GetRelativeClientPath(menu.PageUrl) : "javascript:;");
                            sbBasic.Append("<li class=\"border-right\"></li>");
                        }
                        // Hiển thị tất cả submenu theo itemTemplateBasic (không phân cấp)
                        
                        foreach (var item in subMenus)
                        {
                            sbBasic.AppendFormat(itemTemplateBasic.InnerHtml,
                                !string.IsNullOrEmpty(item.PageUrl) ? GetRelativeClientPath(item.PageUrl) : "javascript:;",
                                GetResourceText(item.FunctionName));
                            sbBasic.Append("<li class=\"border-right\"></li>");
                        }
                        menuCache = sbBasic.ToString();
                    }
                    else
                    {
                        Func<string, string> BuildMenu = null;
                        BuildMenu = (parentId) =>
                        {
                            List<AspnetFunction> childs = aspnetFunctions
                                .Where(t => t.Id != Guid.Empty && t.ParentCode == parentId)
                                .OrderBy(t => t.DisplayOrder)
                                .ToList();

                            StringBuilder sbLi = new StringBuilder();
                            foreach (var item in childs)
                            {
                                string childMenu = BuildMenu(item.FunctionCode);
                                if (!string.IsNullOrEmpty(childMenu))
                                {
                                    sbLi.AppendFormat(itemTemplateMulplite.InnerHtml,
                                        GetResourceText(item.FunctionName),
                                        item.Icon,
                                        childMenu,
                                        item.Id);
                                    sbLi.Append("<li class=\"border-right\"></li>");
                                }
                                else if (item.FunctionCode == ModuleKeys.JoinCompetition.ToString())
                                {
                                    sbLi.AppendFormat(itemTemplateSingleParent.InnerHtml,
                                       GetResourceText(item.FunctionName),
                                       item.Icon,
                                       !string.IsNullOrEmpty(item.PageUrl) ? GetRelativeClientPath(item.PageUrl) : "javascript:;");
                                    sbLi.Append("<li class=\"border-right\"></li>");
                                }
                                else
                                {
                                    sbLi.AppendFormat(itemTemplateSingle.InnerHtml,
                                        !string.IsNullOrEmpty(item.PageUrl) ? GetRelativeClientPath(item.PageUrl) : "javascript:;",
                                        GetResourceText(item.FunctionName),
                                        item.Icon);
                                }
                            }
                            return sbLi.ToString();
                        };

                        menuCache = BuildMenu(string.Empty);
                    }

                    AppCache.Insert(MENU_CACHE_KEY, menuCache);
                }

                ltrMenu.Text = menuCache;
            }
            catch
            {
                return;
            }
        }


        public void OpenMessageBox(MessageBox msg, ConfirmResult result, bool isClosePostBack, bool showmodal, int timeOut = 20000)
        {
            myModalLabel.InnerText = msg.MessageTitle;
            lbMessage.Text = msg.Message;
            switch (msg.MessageButton)
            {
                case MSGButton.Close:
                    btnAccept.Visible = false;
                    btnCancel.Text = btnCancel.ToolTip = this.CURRENT_PAGE.GetResourceText(BackEndResourceKeys.CLOSE);
                    break;
                case MSGButton.OK:
                    btnAccept.Visible = false;
                    btnCancel.Text = btnCancel.ToolTip = this.CURRENT_PAGE.GetResourceText(BackEndResourceKeys.CLOSE);
                    timeOut = 2000;
                    break;
                case MSGButton.YesNo:
                    btnAccept.Visible = true;
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Check;
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.YES);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.NO);
                    break;
                case MSGButton.AcceptCancel:
                    btnAccept.Visible = true;
                    btnAccept.CssClass = "btn btn-warning";
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Accept;
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.AGREE);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
                    break;
                case MSGButton.Send:
                    btnAccept.Visible = true;
                    btnAccept.CssClass = "btn btn-warning";
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Send;
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.SEND);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
                    break;
                case MSGButton.ContinueCancel:
                    btnAccept.Visible = true;
                    btnAccept.CssClass = "btn btn-warning";
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Check;
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.CONTINUE);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
                    break;
                case MSGButton.DeleteCancel:
                    btnAccept.Visible = true;
                    btnAccept.CssClass = "btn btn-danger";
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Remove;
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.DELETE);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
                    break;
            }
            switch (msg.MessageIcon)
            {
                case MSGIcon.Error:
                    modalHeader.Attributes.Add("class", "modal-header modal-danger");
                    break;
                case MSGIcon.Info:
                    modalHeader.Attributes.Add("class", "modal-header modal-info");
                    break;
                case MSGIcon.Success:
                    modalHeader.Attributes.Add("class", "modal-header modal-success");
                    break;
                case MSGIcon.Warning:
                    modalHeader.Attributes.Add("class", "modal-header modal-warning");
                    break;
            }
            RunScript("CMSMasterJs.OpenMessageBox", string.Format("'#modal-notify', {0}", timeOut));
        }

        public void OpenMessageBox(MessageBox msg, bool runScript, int timeOut = 20000)
        {
            myModalLabel.InnerText = msg.MessageTitle;
            lbMessage.Text = msg.Message;
            switch (msg.MessageButton)
            {
                case MSGButton.OK:
                    btnAccept.Visible = false;
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CLOSE);
                    timeOut = 2000;
                    break;
                case MSGButton.YesNo:
                    btnAccept.Visible = true;
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Check;
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.YES);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.NO);
                    break;
                case MSGButton.AcceptCancel:
                    btnAccept.Visible = true;
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Accept;
                    btnAccept.CssClass = "btn btn-warning";
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.AGREE);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
                    break;
                case MSGButton.ContinueCancel:
                    btnAccept.Visible = true;
                    btnAccept.CssClass = "btn btn-warning";
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Check;
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.CONTINUE);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
                    break;
                case MSGButton.DeleteCancel:
                    btnAccept.Visible = true;
                    btnAccept.ButtonIcon = ExtraButton.ButtonsIcon.Remove;
                    btnAccept.CssClass = "btn btn-danger";
                    btnAccept.Text = btnAccept.ToolTip = GetResourceText(BackEndResourceKeys.DELETE);
                    btnCancel.Text = btnCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
                    break;
            }
            switch (msg.MessageIcon)
            {
                case MSGIcon.Error:
                    modalHeader.Attributes.Add("class", "modal-header modal-danger");
                    break;
                case MSGIcon.Info:
                    modalHeader.Attributes.Add("class", "modal-header modal-info");
                    break;
                case MSGIcon.Success:
                    modalHeader.Attributes.Add("class", "modal-header modal-success");
                    break;
                case MSGIcon.Warning:
                    modalHeader.Attributes.Add("class", "modal-header modal-warning");
                    break;
            }
            if (runScript == true)
                RunScript("CMSMasterJs.OpenMessageBox", string.Format("'#modal-notify', {0}", timeOut));
        }

        public void CloseMessageBox()
        {
            RunScript("CMSMasterJs.CloseMessageBox", "'#modal-notify'");
        }
        private void RunScript(string scriptName, string param)
        {
            string script = string.Format("{0}({1});", scriptName, param);
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "RunScript", script, true);
        }
        protected void lbtLogOut_Click(object sender, EventArgs e)
        {
            string userName = SweetContext.Current.UserName;
            FormsAuthentication.SignOut();
            SweetContext.ClearAdminData();
            AppCache.Remove(string.Format("ASP.NET_LockedId_{0}", userName));
            ExpireAllCookies();
            Response.Redirect("~/Login");

        }
        protected void btnAccept_Click(object sender, EventArgs e)
        {
            ConfirmResult result = CurrentConfirmResult;
            if (result == null)
                result = new ConfirmResult();
            result.Submit = true;
            if (CURRENT_PAGE != null)
            {
                CURRENT_PAGE.ConfirmRequest(result);
                CurrentConfirmResult = null;
            }
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ConfirmResult result = CurrentConfirmResult;
            if (result == null)
                result = new ConfirmResult();
            result.Submit = true;
            if (CURRENT_PAGE != null)
            {
                CURRENT_PAGE.CloseRequest(result);
                CurrentConfirmResult = null;
            }
        }

        protected BaseAdminPage CURRENT_PAGE
        {
            get
            {
                try
                {
                    if (this.Page is BaseAdminPage)
                        return (BaseAdminPage)this.Page;
                }
                catch (Exception) { }
                return null;
            }
        }

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


        private static string COOKIE_KEY = ConfigurationManager.AppSettings["CookieKeyPanel"];
        private void ExpireAllCookies()
        {
            if (HttpContext.Current != null)
            {
                if (Request.Cookies[COOKIE_KEY] != null)
                {
                    Response.Cookies[COOKIE_KEY].Value = string.Empty;
                    Response.Cookies.Set(Response.Cookies[COOKIE_KEY]);
                }
                else
                    Response.Cookies.Set(new HttpCookie(COOKIE_KEY, string.Empty));

                Response.Cookies[COOKIE_KEY].Expires = DateTime.UtcNow.AddDays(-1);
            }
        }
        #region SearchCriteria
        public void btnSearchAdvanced_Click(ExtraSearchBox searchTagBox, Panel parent, GridviewExtension grv)
        {
            grv.GridSearchType = GridSearchType.Multiple;
            grv.CurrentPageIndex = 1;
            grv.Rebind();
            UpdateSearchTagBox(searchTagBox, parent, grv, null);
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "HideOffcanvasSearch", "CMSMasterJs.HideOffcanvasSearch();", true);
        }
        public void btnSearchAdvanced_Click(ExtraSearchBox searchTagBox, Panel parent, Panel parentPopUp, GridviewExtension grv)
        {
            grv.GridSearchType = GridSearchType.Multiple;
            grv.CurrentPageIndex = 1;
            grv.Rebind();
            UpdateSearchTagBox(searchTagBox, parent, parentPopUp, grv, null);
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "HideOffcanvasSearch", "CMSMasterJs.HideOffcanvasSearch();", true);
        }
        public void btnSearchSingle_Click(ExtraSearchBox searchTagBox, GridviewExtension grv, ExtraTextBox txtSearchSingle)
        {
            grv.GridSearchType = GridSearchType.Single;
            grv.CurrentPageIndex = 1;
            grv.Rebind();
            UpdateSearchTagBox(searchTagBox, null, grv, txtSearchSingle);

            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "DisableContentChanged2", "setTimeout(()=> CMSMasterJs.DisableContentChanged(), 500)", true);
            //if (txtSearchSingle != null)
            //    ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "", string.Format("$('#{0}').select();", txtSearchSingle.ClientID), true);
        }
        public void btnSearchSingle_Click(ExtraSearchBox searchTagBox, Panel parent, GridviewExtension grv, ExtraTextBox txtSearchSingle)
        {
            grv.GridSearchType = GridSearchType.Single;
            grv.CurrentPageIndex = 1;
            grv.Rebind();
            UpdateSearchTagBox(searchTagBox, parent, grv, txtSearchSingle);
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "DisableContentChanged2", "setTimeout(()=> CMSMasterJs.DisableContentChanged(), 500)", true);
        }
        public void custlinkPager_PageChanged(ExtraSearchBox searchTagBox, GridviewExtension grv, ExtraTextBox txtSearchSingle, Panel parent)
        {
            if (grv.GridSearchType == GridSearchType.Single)
            {
                grv.Rebind();
                UpdateSearchTagBox(searchTagBox, null, grv, txtSearchSingle);
            }
            else if (grv.GridSearchType == GridSearchType.Multiple)
            {
                grv.Rebind();
                UpdateSearchTagBox(searchTagBox, parent, grv, null);
            }
        }
        public void searchTagBox_TagClosed(ExtraSearchBox searchTagBox, SearchTagItem tag, Panel parent, GridviewExtension grv, ExtraTextBox txtSearchSingle, out GridSearchType? searchType)
        {
            searchType = null;
            if (txtSearchSingle != null && txtSearchSingle.ClientID == tag.Key)
            {
                txtSearchSingle.Text = string.Empty;
                searchType = GridSearchType.Single;
            }
            else if (parent != null)
            {
                Control control = parent.FindControl(tag.Id);
                if (control == null)
                    control = CommonHelpers.GetControlById(parent, tag.Key);
                if (control != null)
                {
                    searchType = GridSearchType.Multiple;
                    if (control.GetType() == typeof(ExtraTextBox))
                        (control as ExtraTextBox).Text = string.Empty;
                    else if (control.GetType() == typeof(ExtraDateTime))
                    {
                        ExtraDateTime extraDateTime = control as ExtraDateTime;
                        extraDateTime.ClearDate();
                    }
                    else if (control.GetType() == typeof(ExtraDropdown))
                    {
                        ExtraDropdown extraDropdown = control as ExtraDropdown;
                        if (!extraDropdown.Multiple)
                        {
                            if (extraDropdown.Items.FindByValue(string.Empty) != null)
                                extraDropdown.SelectedValue = string.Empty;
                            else if (extraDropdown.Items.FindByValue("-1") != null)
                                extraDropdown.SelectedValue = "-1";
                            else extraDropdown.SelectedIndex = -1;
                        }
                        else
                        {
                            List<ListItem> newSelectedItems = new List<ListItem>();
                            foreach (ListItem item in extraDropdown.SelectedItems)
                                if (item.Value != tag.Value)
                                    newSelectedItems.Add(item);
                            extraDropdown.SelectedItems = newSelectedItems;
                        }
                    }
                    else if (control.GetType() == typeof(BootstrapDropdown))
                    {
                        BootstrapDropdown bootstrapDropdown = control as BootstrapDropdown;
                        if (bootstrapDropdown.Items.Select(t => t.Value == string.Empty) != null)
                            bootstrapDropdown.SelectedValue = string.Empty;
                        else if (bootstrapDropdown.Items.Select(t => t.Value == "-1") != null)
                            bootstrapDropdown.SelectedValue = "-1";
                        else bootstrapDropdown.SelectedIndex = -1;
                    }
                }
            }
            UpdateSearchTagBox(searchTagBox, parent, grv, txtSearchSingle);
            grv.Rebind();
        }
        public void searchTagBox_TagClosed(ExtraSearchBox searchTagBox, SearchTagItem tag, Panel parentDefault, Panel parent, GridviewExtension grv, ExtraTextBox txtSearchSingle, out GridSearchType? searchType)
        {
            searchType = null;
            if (txtSearchSingle != null && txtSearchSingle.ClientID == tag.Key)
            {
                txtSearchSingle.Text = string.Empty;
                searchType = GridSearchType.Single;
            }
            if (parentDefault != null)
            {
                ClearSearchTagBox(tag, parentDefault, out searchType);
            }
            if (parent != null)
            {
                ClearSearchTagBox(tag, parent, out searchType);
            }
            UpdateSearchTagBox(searchTagBox, parentDefault, parent, grv, txtSearchSingle);
            grv.Rebind();
        }
        private void ClearSearchTagBox(SearchTagItem tag, Panel parent, out GridSearchType? searchType)
        {
            searchType = null;
            if (parent == null) return;
            Control control = parent.FindControl(tag.Id);
            if (control == null)
                control = CommonHelpers.GetControlById(parent, tag.Key);
            if (control != null)
            {
                if (control.ID == tag.Id)
                    searchType = GridSearchType.Single;
                else
                    searchType = GridSearchType.Multiple;
                if (control.GetType() == typeof(ExtraTextBox))
                {
                    ExtraTextBox txtExtra = (ExtraTextBox)control;
                    txtExtra.Text = string.Empty;
                }
                else if (control.GetType() == typeof(ExtraDateTime))
                {
                    ExtraDateTime extraDateTime = control as ExtraDateTime;
                    extraDateTime.ClearDate();
                }
                else if (control.GetType() == typeof(ExtraDropdown))
                {
                    ExtraDropdown extraDropdown = control as ExtraDropdown;
                    if (!extraDropdown.Multiple)
                    {
                        if (extraDropdown.Items.FindByValue(string.Empty) != null)
                            extraDropdown.SelectedValue = string.Empty;
                        else if (extraDropdown.Items.FindByValue("-1") != null)
                            extraDropdown.SelectedValue = "-1";
                        else extraDropdown.SelectedIndex = -1;
                    }
                    else
                    {
                        List<ListItem> newSelectedItems = new List<ListItem>();
                        foreach (ListItem item in extraDropdown.SelectedItems)
                            if (item.Value != tag.Value)
                                newSelectedItems.Add(item);
                        extraDropdown.SelectedItems = newSelectedItems;
                    }
                }
                else if (control.GetType() == typeof(BootstrapDropdown))
                {
                    BootstrapDropdown bootstrapDropdown = control as BootstrapDropdown;
                    if (bootstrapDropdown != null)
                    {
                        bootstrapDropdown.ClearSelection();
                        return;
                    }
                    if (bootstrapDropdown.Items.Select(t => t.Value == string.Empty) != null)
                        bootstrapDropdown.SelectedValue = string.Empty;
                    else if (bootstrapDropdown.Items.Select(t => t.Value == "-1") != null)
                        bootstrapDropdown.SelectedValue = "-1";
                    else
                        bootstrapDropdown.ClearSelection();
                }
            }
        }
        public void LoadSessionLastSearch(ExtraSearchBox searchTagBox, Panel pnSearchField, GridviewExtension grv, ExtraTextBox txtSearchSingle)
        {
            if (searchTagBox == null)
                return;

            MasterFunctionSearchCriteria searchCriteria = GetCurrentFunctionSearchCriteria(searchTagBox.ClientID);

            searchTagBox.Visible = false;
            searchTagBox.TagItems = searchCriteria.CriteriaList;

            if (txtSearchSingle != null)
                txtSearchSingle.Text = searchCriteria.SearchText;

            if (grv != null)
            {
                if (searchCriteria.PageSize.HasValue)
                    grv.CurrentPageSize = searchCriteria.PageSize.Value;
                if (!string.IsNullOrEmpty(searchCriteria.Columns))
                    grv.ColumnVisibleDefault = searchCriteria.Columns;
                grv.GridSearchType = searchCriteria.SearchType;
            }

            if (pnSearchField != null)
            {
                IEnumerable<Control> controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(ExtraTextBox));
                foreach (Control control in controls)
                {
                    ExtraTextBox extraTextBox = control as ExtraTextBox;
                    extraTextBox.Text = searchCriteria.GetValueByKey(extraTextBox.ClientID);
                }

                controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(ExtraDateTime));
                foreach (Control control in controls)
                {
                    ExtraDateTime extraDateTime = control as ExtraDateTime;
                    DateTime startDateValue;
                    DateTime endDateValue;
                    string searchCriteriaValue = searchCriteria.GetValueByKey(extraDateTime.ClientID);
                    if (!string.IsNullOrEmpty(searchCriteriaValue))
                    {
                        string[] dateValueInString = searchCriteriaValue.Split('|');
                        if (DateTime.TryParseExact(dateValueInString[0].Trim(), DateTimeHelper.DateFormat, new CultureInfo(SweetContext.Current.CurrentLanguageCode)
                            , DateTimeStyles.None, out startDateValue))
                            extraDateTime.DateValue = extraDateTime.StartValue = startDateValue;
                        if (DateTime.TryParseExact(dateValueInString[1].Trim(), DateTimeHelper.DateFormat, new CultureInfo(SweetContext.Current.CurrentLanguageCode)
                            , DateTimeStyles.None, out endDateValue))
                            extraDateTime.EndValue = endDateValue;
                    }
                }

                controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(ExtraDropdown));
                foreach (Control control in controls)
                {
                    ExtraDropdown extraDropdown = control as ExtraDropdown;
                    if (!extraDropdown.Multiple)
                    {
                        string value;
                        if (searchCriteria.GetValueByKey(extraDropdown.ClientID, out value))
                            extraDropdown.SelectedValue = value;
                    }
                    else
                    {
                        List<string> listValue;
                        List<ListItem> newSelectedItems = new List<ListItem>();
                        if (searchCriteria.GetListValueByKey(extraDropdown.ClientID, out listValue))
                            foreach (ListItem item in extraDropdown.Items)
                                if (listValue.Contains(item.Value))
                                    newSelectedItems.Add(item);
                        extraDropdown.SelectedItems = newSelectedItems;
                    }
                }

                controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(BootstrapDropdown));
                foreach (Control control in controls)
                {
                    BootstrapDropdown bootstrapDropdown = control as BootstrapDropdown;
                    string value;
                    if (searchCriteria.GetValueByKey(bootstrapDropdown.ClientID, out value))
                        bootstrapDropdown.SelectedValue = value;
                }
            }
            UpdateSearchTagBox(searchTagBox, pnSearchField, grv, txtSearchSingle);
        }
        public void UpdateSearchTagBox(ExtraSearchBox searchTagBox, Panel pnSearchField, GridviewExtension grv, ExtraTextBox txtSearchSingle)
        {
            string searchText = string.Empty;
            searchTagBox.TagItems.Clear();
            if (grv.GridSearchType == GridSearchType.Multiple && pnSearchField != null)
                GetValueForExtraSearchBox(pnSearchField, ref searchTagBox);
            else if (grv.GridSearchType == GridSearchType.Single)
            {
                if (pnSearchField != null)
                    GetValueForExtraSearchBox(pnSearchField, ref searchTagBox);
                if (txtSearchSingle != null)
                {
                    if (txtSearchSingle.SearchTagItem != null)
                    {
                        searchText = txtSearchSingle.Text;
                        SearchTagItem searchTagItem = txtSearchSingle.SearchTagItem;
                        searchTagItem.SearchType = GridSearchType.Single;
                        searchTagBox.TagItems.Add(searchTagItem);
                    }
                }
            }

            searchTagBox.Update(grv.GridSearchType);
            searchTagBox.Visible = true;

            AddPageSearchCriteria(searchTagBox.TagItems, searchText, grv.GridSearchType, string.Empty, grv.CurrentPageIndex, grv.CurrentPageSize, grv.ColumnVisibleDefault, searchTagBox.ClientID);
        }
        public void UpdateSearchTagBox(ExtraSearchBox searchTagBox, Panel pnSearchField, Panel pnSearchPopUp, GridviewExtension grv, ExtraTextBox txtSearchSingle)
        {
            string searchText = string.Empty;
            searchTagBox.TagItems.Clear();

            if (pnSearchField != null)
                GetValueForExtraSearchBox(pnSearchField, ref searchTagBox);
            if (pnSearchPopUp != null)
                GetValueForExtraSearchBox(pnSearchPopUp, ref searchTagBox);

            searchTagBox.Update(grv.GridSearchType);
            searchTagBox.Visible = true;

            AddPageSearchCriteria(searchTagBox.TagItems, searchText, grv.GridSearchType, string.Empty, grv.CurrentPageIndex, grv.CurrentPageSize, grv.ColumnVisibleDefault, searchTagBox.ClientID);
        }
        private void GetValueForExtraSearchBox(Panel pnSearchField, ref ExtraSearchBox searchTagBox)
        {
            IEnumerable<Control> controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(ExtraTextBox));
            foreach (Control control in controls)
            {
                ExtraTextBox extraTextBox = control as ExtraTextBox;
                if (extraTextBox.SearchTagItem != null)
                    searchTagBox.TagItems.Add(extraTextBox.SearchTagItem);
            }

            controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(ExtraDateTime));
            foreach (Control control in controls)
            {
                ExtraDateTime extraDateTime = control as ExtraDateTime;
                if (extraDateTime.SearchTagItem != null)
                    searchTagBox.TagItems.Add(extraDateTime.SearchTagItem);
            }

            controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(ExtraDropdown));
            foreach (Control control in controls)
            {
                ExtraDropdown extraDropdown = control as ExtraDropdown;
                if (!extraDropdown.Multiple)
                {
                    if (extraDropdown.ValueIsOfTypeGUID && extraDropdown.SelectedValue == Guid.Empty.ToString())
                        continue;
                    if (extraDropdown.SearchTagItem != null)
                        searchTagBox.TagItems.Add(extraDropdown.SearchTagItem);
                }
                else
                {
                    if (extraDropdown.ListSearchTagItem != null && extraDropdown.ListSearchTagItem.Count > 0)
                        searchTagBox.TagItems.AddRange(extraDropdown.ListSearchTagItem);
                }
            }

            controls = CommonHelpers.GetAllControlByType(pnSearchField, typeof(BootstrapDropdown));
            foreach (Control control in controls)
            {
                BootstrapDropdown bootstrapDropdown = control as BootstrapDropdown;
                if (bootstrapDropdown.ValueIsOfTypeGUID && bootstrapDropdown.SelectedValue == Guid.Empty.ToString())
                    continue;
                if (bootstrapDropdown.SearchTagItem != null)
                    searchTagBox.TagItems.Add(bootstrapDropdown.SearchTagItem);
            }
        }
        private void AddPageSearchCriteria(List<SearchTagItem> criteria, string searchText, GridSearchType searchType, string searchSQL, int? pageIndex, int? pageSize, string columns, string searchBoxClientId)
        {
            string absoluteId = getAbsoluteId(searchBoxClientId);
            if (Session["MasterFunctionSearchCriteria"] == null) Session["MasterFunctionSearchCriteria"] = new List<MasterFunctionSearchCriteria>();
            List<MasterFunctionSearchCriteria> lst = Session["MasterFunctionSearchCriteria"] as List<MasterFunctionSearchCriteria>;
            if (lst == null)
                return;

            MasterFunctionSearchCriteria current = lst.FirstOrDefault(p => p.AbsoluteId.Equals(absoluteId));
            if (current == null)
            {
                current = new MasterFunctionSearchCriteria();
                lst.Add(current);
            }
            current.AbsoluteId = absoluteId;
            current.SearchText = searchText;
            current.SearchType = searchType;
            current.CriteriaList = criteria;
            current.PageSize = pageSize;
            current.SearchSQL = searchSQL;
            //current.PageIndex = pageIndex;
            current.Columns = columns;
        }
        public MasterFunctionSearchCriteria GetCurrentFunctionSearchCriteria(string searchBoxClientId)
        {
            MasterFunctionSearchCriteria criteria;
            GetCurrentFunctionSearchCriteria(searchBoxClientId, out criteria);
            return criteria;
        }
        protected bool GetCurrentFunctionSearchCriteria(string searchBoxClientId, out MasterFunctionSearchCriteria searchCriteria)
        {
            searchCriteria = new MasterFunctionSearchCriteria();
            if (Session["MasterFunctionSearchCriteria"] != null)
            {
                string absoluteId = getAbsoluteId(searchBoxClientId);
                List<MasterFunctionSearchCriteria> lst = Session["MasterFunctionSearchCriteria"] as List<MasterFunctionSearchCriteria>;
                if (lst != null && lst.Count > 0)
                    searchCriteria = lst.FirstOrDefault(p => p.AbsoluteId.Equals(absoluteId));
            }
            if (searchCriteria == null)
            {
                searchCriteria = new MasterFunctionSearchCriteria();
                return false;
            }
            else if (searchCriteria.CriteriaList == null || searchCriteria.CriteriaList.Count == 0)
                return false;
            return true;
        }
        private string getAbsoluteId(string searchBoxClientId)
        {
            return string.Concat(HttpContext.Current.Request.Url.AbsolutePath, searchBoxClientId);
        }
        #endregion

        protected void btnRefreshUser_ServerClick(object sender, EventArgs e)
        {
            SweetContext.Current.User = null;
        }

        protected void lbtLockScreen_Click(object sender, EventArgs e)
        {
            string userName = SweetContext.Current.UserName;
            if (string.IsNullOrEmpty(userName))
                return;
            AppCache.Remove(string.Format("ASP.NET_LockedId_{0}", userName));
            AppCache.Insert(string.Format("ASP.NET_LockedId_{0}", userName), true);
            string returnURL = "";
            if (Request.UrlReferrer != null)
                returnURL = Request.UrlReferrer.AbsolutePath;
            Response.Redirect(GetRelativeClientPath($"/lock-screen?ReturnURL={returnURL}"));
        }

        protected void btnLoadTab_ServerClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(hdfTabKey.Value))
                    throw new Exception("Empty tab key");

                CURRENT_PAGE.LoadTab(hdfTabKey.Value);
            }
            catch (Exception ex)
            {
                this.CURRENT_PAGE.ShowSystemError();
            }
        }

        public void SetUserInfomation(string displayName, string avatar)
        {
            tagName.InnerText = displayName;
            imgAvatar.Src = !string.IsNullOrEmpty(avatar) ? avatar : GetRelativeClientPath("/Styles/images/avatar.png");
            pnlInfo.Update();
        }
        public string GetResourceText(string messageId)
        {
            return CURRENT_PAGE.GetResourceText(messageId);
        }

        private void BindLanguages(byte currentLangId)
        {
            byte[] langs = LanguageHelpers.AvailableLanguages.Where(p => p != currentLangId).ToArray();
            if (langs.Length == 0)
                return;

            List<object> lstObjLanguages = new List<object>();
            foreach (var item in langs)
            {
                string name = LanguageHelpers.LanguageName[item];
                if (currentLangId != LanguageHelpers.English)
                    name = LanguageHelpers.LanguageText[item];
                lstObjLanguages.Add(new
                {
                    LangId = item,
                    Name = name,
                    ImageUrl = LanguageHelpers.GetCMSLanguageImage(item)
                });
            }
            rptLanguages.DataSource = lstObjLanguages;
            rptLanguages.DataBind();
        }

        protected void lbtChangeContentLanguage_Command(object sender, CommandEventArgs e)
        {
            if (e == null || e.CommandName == null || e.CommandArgument == null)
                return;

            switch (e.CommandName)
            {
                case "change-language":
                    byte langId = LanguageHelpers.English;
                    byte.TryParse((string)e.CommandArgument, out langId);
                    SweetContext.Current.CurrentLanguageId = langId;
                    Response.Redirect(Request.UrlReferrer.AbsolutePath, true);
                    break;
            }
        }

        protected void btnRefreshPermission_ServerClick(object sender, EventArgs e)
        {
            AppCache.Remove(string.Format("MENU_LEFT_CMS_{0}", SweetContext.Current.UserId));
            SweetContext.Current.CurrentFunctions = null;
        }
    }
    public class MasterFunctionSearchCriteria
    {
        public string AbsoluteId { get; set; }
        public string SearchText { get; set; }
        public GridSearchType SearchType { get; set; }
        public string SearchSQL { get; set; }
        public List<SearchTagItem> CriteriaList { get; set; }
        //public int? PageIndex { get; set; }
        //public int? RowIndex { get; set; }
        public int? PageSize { get; set; }
        public string Columns { get; set; }

        public MasterFunctionSearchCriteria()
        {
            //PageIndex = null;
            //RowIndex = null;
            PageSize = null;
        }

        public string GetValueByKey(string key)
        {
            if (CriteriaList != null && CriteriaList.Count > 0)
            {
                SearchTagItem tag = CriteriaList.FirstOrDefault(p => p.Key.Equals(key));
                if (tag != null) return tag.Value;
            }
            //EX: ccb search status all value = string.Empty
            //If return string.Empty the search tag box will be added "Status: All" tag
            return string.Empty;
        }
        public bool GetValueByKey(string key, out string value)
        {
            if (CriteriaList != null && CriteriaList.Count > 0)
            {
                SearchTagItem tag = CriteriaList.FirstOrDefault(p => p.Key.Equals(key));
                if (tag != null)
                {
                    value = tag.Value;
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }
        public bool GetListValueByKey(string key, out List<string> listValue)
        {
            if (CriteriaList != null && CriteriaList.Count > 0)
            {
                List<SearchTagItem> listTag = CriteriaList.Where(i => i.Key == key).ToList();
                if (listTag != null && listTag.Count > 0)
                {
                    listValue = new List<string>();
                    foreach (SearchTagItem tag in listTag)
                        listValue.Add(tag.Value);
                    return true;
                }
            }
            listValue = null;
            return false;
        }
    }
}