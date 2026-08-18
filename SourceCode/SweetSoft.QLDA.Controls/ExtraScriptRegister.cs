using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [ToolboxData("<{0}:ExtraScriptRegister runat=\"server\" />")]
    public class ExtraScriptRegister : HiddenField
    {
        public static bool RegisterGrid = false;
        public static bool RegisterDialog = false;
        public static bool RegisterTextBox = false;
        public static bool RegisterButton = false;
        public static bool RegisterExtraDropdown = false;
        public static bool RegisterDateTime = false;
        public static bool RegisterSearchBox = false;
        public static bool RegisterTabPanel = false;
        public static bool RegisterModal = false;
        public static string CssPlaceHolder = "cpHeadVendor";
        public static string JSPlaceHolder = "cpVendorScript";
        public static string aspnetForm = "aspnetForm";

        public static string ScriptRunStartUp = string.Empty;

        #region Properties

        public string AspnetFormClientID { get { return aspnetForm; } set { aspnetForm = value; } }


        /// <summary>
        /// Placeholder script
        /// </summary>
        public string ScriptPlaceHolder
        {
            get
            {
                return JSPlaceHolder;
            }
            set { JSPlaceHolder = value; }
        }
        /// <summary>
        /// Placeholder style
        /// </summary>
        public string StylePlaceHolder
        {
            get
            {
                return CssPlaceHolder;
            }
            set { CssPlaceHolder = value; }
        }

        #endregion

        #region On Event
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RegisterScriptAndStyle();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //RegisterScriptAndStyle();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //RegisterScriptAndStyle();
        }
        #endregion

        #region Function
        public void RegisterScriptAndStyle()
        {
            Page page = this.Page;
            if (page == null)
                page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;

            if (page != null)
            {
                ClientScriptManager cs = page.ClientScript;

                // Define the resource name and type.
                Type thisT = this.GetType();
                if (thisT != typeof(ExtraScriptRegister))
                    thisT = thisT.BaseType;


                // Check to see if the startup script is already registered.
                if (!cs.IsStartupScriptRegistered(thisT, "RegisterBaseScript"))
                {

                    string templatescript = "<script type=\"text/javascript\" src=\"{0}\"></script> ";
                    string templatestyle = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
                    string CssPath = "/";
                    string ScriptPath = "/";

                    #region Style

                    #region Grid
                    string gridStyle = string.Empty;
                    if (RegisterGrid)
                    {
                        gridStyle += string.Format(templatestyle, (CssPath + "Styles/plugins/rwd-table/rwd-table.min.css?v=201909261356"));

                    }
                    #endregion

                    #region Datetime
                    string datetimeStyle = string.Empty;
                    if (RegisterDateTime)
                    {
                        datetimeStyle += string.Format(templatestyle, (CssPath + "Styles/plugins/datetime/daterangepicker.css?v=201909261356"));

                    }
                    #endregion

                    #region Dialog
                    string dialogStyle = string.Empty;
                    if (RegisterDialog)
                    {
                        dialogStyle += string.Format(templatestyle,
                                                                 (CssPath + "Styles/plugins/dialog/bootstrap-dialog.min.css?v=201909261356"));
                    }
                    #endregion

                    #region Dropdown
                    string dropdownStyle = string.Empty;
                    if (RegisterExtraDropdown)
                    {
                        dropdownStyle += string.Format(templatestyle, (CssPath + "Styles/plugins/select2/select2-theme1.css?v=201909261356"));
                    }
                    #endregion

                    #region commonstyle
                    string cr_style = gridStyle + dialogStyle + dropdownStyle + datetimeStyle;
                    LiteralControl link = new LiteralControl(cr_style);
                    link.EnableViewState = false;


                    if (string.IsNullOrEmpty(StylePlaceHolder))
                        page.Header.Controls.Add(link);
                    else
                    {
                        ContentPlaceHolder cpCss = page.Master.FindControl(StylePlaceHolder.ToLower()) as ContentPlaceHolder;
                        if (cpCss != null)
                            cpCss.Controls.Add(link);
                        else
                            page.Header.Controls.Add(link);
                    }
                    #endregion


                    #endregion

                    #region Script

                    string gridScript = string.Empty;
                    #region Grid Script
                    if (RegisterGrid)
                    {
                        gridScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/rwd-table/rwd-table.min.js?v=201909261356"));
                        gridScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/rwd-table/ExtraGridview.js?v=201909261356"));
                    }
                    #endregion

                    string dialogScript = string.Empty;
                    #region Dialog Script
                    if (RegisterDialog)
                    {
                        dialogScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/dialog/run_prettify.min.js?v=201909261356"));
                        dialogScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/dialog/bootstrap-dialog.min.js?v=201909261356"));
                        dialogScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/dialog/ExtraDialog.js?v=201909261356"));
                    }
                    #endregion

                    string datetimeScript = string.Empty;

                    #region Datetime script
                    if (RegisterDateTime)
                    {
                        datetimeScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/datetime/moment.min.js?v=201909261356"));
                        datetimeScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/datetime/daterangepicker.min.js?v=201909261356"));
                        //datetimeScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/datetime/language/vn.js?v=201909261356"));
                        datetimeScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/datetime/ExtraDatetime.js?v=201909261356"));
                    }
                    #endregion

                    #region DropdownScript
                    string dropdownScript = string.Empty;
                    if (RegisterExtraDropdown)
                    {
                        dropdownScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/select2/select2.full.js?v=201909261356"));
                        dropdownScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/select2/Dropdown.js?v=201909261356"));
                    }
                    #endregion

                    #region Button
                    string buttonScript = string.Empty;
                    if (RegisterButton)
                        buttonScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/button/Button.js?v=201909261356"));
                    #endregion

                    #region Textbox
                    string textboxScript = string.Empty;
                    if (RegisterTextBox)
                    {
                        textboxScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/imask/imask.min.js?v=201909261356"));
                        textboxScript += string.Format(templatescript, (ScriptPath + "Styles/plugins/imask/ExtraTextbox.js?v=201909261356"));
                    }
                    #endregion
                    string commonScript = string.Empty;
                    #region Common script
                    commonScript += gridScript + dialogScript + datetimeScript + dropdownScript + buttonScript + textboxScript;

                    LiteralControl link_script = new LiteralControl(commonScript);
                    link_script.EnableViewState = false;

                    if (string.IsNullOrEmpty(ScriptPlaceHolder))
                        cs.RegisterStartupScript(thisT, "RegisterBaseScript", commonScript, false);
                    else
                    {
                        ContentPlaceHolder cpJs = page.FindControl(ScriptPlaceHolder) as ContentPlaceHolder;
                        if (cpJs != null)
                            cpJs.Controls.Add(link_script);
                        else
                            cs.RegisterStartupScript(thisT, "RegisterBaseScript", commonScript, false);
                    }
                    #endregion


                    #endregion

                    string scriptHiddenPropertiesID = string.Format("CMSMasterJs.hiddenPropertiesClientID = '{0}';", this.ClientID);
                    if (!string.IsNullOrEmpty(ExtraScriptRegister.ScriptRunStartUp))
                        scriptHiddenPropertiesID += string.Format("setTimeout(function(){{{0}}},800);", ExtraScriptRegister.ScriptRunStartUp);
                    ScriptManager.RegisterStartupScript(this.Page, thisT, "RegisterHiddenPropertiesID", scriptHiddenPropertiesID, true);
                    ExtraScriptRegister.ScriptRunStartUp = string.Empty;
                }
            }
        }

        #endregion

    }
}
