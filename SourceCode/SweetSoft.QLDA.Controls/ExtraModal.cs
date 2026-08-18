//-----------------------PROGRAMER LOGS---------------------------
using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [ParseChildren(true), PersistChildren(false)]
    [ToolboxData("<{0}:ExtraModal runat=\"server\"></{0}:ExtraModal>")]
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ExtraModal : WebControl, INamingContainer
    {
        private ITemplate _headerTemPlate;
        private ITemplate _containerTemPlate;
        private ITemplate _footerTemPlate;

        private Control _headerControl;
        private Control _containerControl;
        private Control _footerControl;

        private UpdatePanel panelModal;

        #region Properties
        protected T GetViewState<T>(string key, T defaultValue)
        {
            object obj = ViewState[key];
            return obj != null ? (T)obj : defaultValue;
        }

        protected void SetViewState<T>(string key, T value)
        {
            ViewState[key] = value;
        }
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate ContentTemplate
        {
            get { return _containerTemPlate; }
            set { _containerTemPlate = value; }
        }

        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate HeaderTemplate
        {
            get { return _headerTemPlate; }
            set { _headerTemPlate = value; }
        }

        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate FooterTemplate
        {
            get { return _footerTemPlate; }
            set { _footerTemPlate = value; }
        }

        [Category("Appearance")]
        [Description("Style css class header")]
        [DefaultValue("")]
        [Browsable(false)]
        public string HeaderClass
        {
            get => GetViewState("HeaderClass", string.Empty);
            set => SetViewState("HeaderClass", value);
        }

        [Category("Appearance")]
        [Description("Style css class body")]
        [DefaultValue("")]
        [Browsable(false)]
        public string BodyClass
        {
            get => GetViewState("BodyClass", string.Empty);
            set => SetViewState("BodyClass", value);
        }

        [Category("Appearance")]
        [Description("Style css class footer")]
        [DefaultValue("")]
        [Browsable(false)]
        public string FooterClass
        {
            get => GetViewState("FooterClass", string.Empty);
            set => SetViewState("FooterClass", value);
        }

        [Category("Appearance")]
        [Description("Effect open modal")]
        [DefaultValue(ModalEffect.FadeIn)]
        [Browsable(false)]
        public ModalEffect Effect
        {
            get => GetViewState("Effect", ModalEffect.FadeIn);
            set => SetViewState("Effect", value);
        }

        [Category("Appearance")]
        [Description("Size of modal")]
        [DefaultValue(ModalSize.Normal)]
        [Browsable(false)]
        public ModalSize Size
        {
            get => GetViewState("Size", ModalSize.Normal);
            set => SetViewState("Size", value);
        }

        [Category("Appearance")]
        [Description("Position modal")]
        [DefaultValue("")]
        [Browsable(false)]
        public string Position
        {
            get => GetViewState("ModalPosition", "");
            set => SetViewState("ModalPosition", value);
        }

        [Category("Appearance")]
        [Description("Show button close on header")]
        [DefaultValue(true)]
        [Browsable(false)]
        public bool HeaderButtonClose
        {
            get => GetViewState("HeaderButtonClose", true);
            set => SetViewState("HeaderButtonClose", value);
        }

        [Category("Appearance")]
        [Description("Show header")]
        [DefaultValue(true)]
        [Browsable(false)]
        public bool ShowHeader
        {
            get => GetViewState("ShowHeader", true);
            set => SetViewState("ShowHeader", value);
        }

        [Category("Appearance")]
        [Description("Show button close on footer")]
        [DefaultValue(true)]
        [Browsable(false)]
        public bool FooterButtonClose
        {
            get => GetViewState("FooterButtonClose", true);
            set => SetViewState("FooterButtonClose", value);
        }

        [Category("Appearance")]
        [Description("Show footer")]
        [DefaultValue(true)]
        [Browsable(false)]
        public bool ShowFooter
        {
            get => GetViewState("ShowFooter", true);
            set => SetViewState("ShowFooter", value);
        }

        [Category("Appearance")]
        [Description("Enable Draggable modal")]
        [DefaultValue(false)]
        [Browsable(false)]
        public bool EnableDraggable
        {
            get => GetViewState("EnableDraggable", false);
            set => SetViewState("EnableDraggable", value);
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        [Category("Appearance")]
        [Description("Title modal")]
        [DefaultValue("")]
        [Browsable(false)]
        public string Title
        {
            get => GetViewState("Title", string.Empty);
            set => SetViewState("Title", value);
        }

        [Category("Appearance")]
        [Description("Default button submit")]
        [DefaultValue("")]
        [Browsable(false)]
        public string DefaultButton
        {
            get => GetViewState("DefaultButton", string.Empty);
            set => SetViewState("DefaultButton", value);
        }

        [Category("Appearance")]
        [Description("Style modal")]
        [DefaultValue(ModalStyle.Info)]
        [Browsable(false)]
        public ModalStyle Type
        {
            get => GetViewState("Style", ModalStyle.Info);
            set => SetViewState("Style", value);
        }

        [Category("Appearance")]
        [Description("Text for button Close")]
        [Browsable(false)]
        public string CloseText
        {
            get => GetViewState("CloseText", "Đóng");
            set => SetViewState("CloseText", value);
        }

        [Browsable(false)]
        [DefaultValue(true)]
        private bool AppendUpdatePanel
        {
            get => GetViewState("AppendUpdatePanel", true);
            set => SetViewState("AppendUpdatePanel", value);
        }

        [Browsable(false)]
        [DefaultValue(UpdatePanelUpdateMode.Conditional)]
        public UpdatePanelUpdateMode UpdateMode
        {
            get => GetViewState("UpdateMode", UpdatePanelUpdateMode.Conditional);
            set => SetViewState("UpdateMode", value);
        }

        [Browsable(false)]
        [DefaultValue(true)]
        public bool ChildrenAsTriggers
        {
            get => GetViewState("ChildrenAsTriggers", true);
            set => SetViewState("ChildrenAsTriggers", value);
        }

        [Browsable(false)]
        [DefaultValue(false)]
        private bool IsOnLoad
        {
            get => GetViewState("IsOnLoad", false);
            set => SetViewState("IsOnLoad", value);
        }

        #endregion

        #region Events
        public event ModalCloseEvent ModalClosed;
        public event ModalOpenEvent ModalOpen;
        #endregion
        private bool _childControlsCreated = false;
        protected override void OnInit(EventArgs e)
        {
            ExtraScriptRegister.RegisterModal = true;
            Page page = this.Page;
            if (page != null && !page.IsPostBack)
                EnsureChildControls();
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            EnsureChildControls();
            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }

        public override void DataBind()
        {
            base.DataBind();
        }
        protected override void CreateChildControls()
        {
            if (_childControlsCreated)
                return;
            _childControlsCreated = true;
            Controls.Clear();
            #region Build Modal
            HtmlGenericControl controlModal = new HtmlGenericControl("div");
            controlModal.Attributes.Add("id", this.ClientID);
            controlModal.Attributes.Add("class", "modal fade bs-example-modal-center extra-modal");
            controlModal.Attributes.Add("aria-hidden", "true");

            if (!string.IsNullOrEmpty(DefaultButton))
            {
                controlModal.Attributes.Add("data-submit-form", "true");
                controlModal.Attributes.Add("data-enter-id", DefaultButton);
                controlModal.Attributes.Add("onkeydown", "CMSMasterJs.EnterSubmit(event, this);");
            }

            List<HtmlAttribute> attrCol = this.GetListAttributes();
            if (attrCol.Count > 0)
            {
                foreach (HtmlAttribute attr in attrCol)
                    controlModal.Attributes.Add(attr.Name, attr.Value);
            }

            HtmlGenericControl modalDialog = new HtmlGenericControl("div");
            string divModalStyle = "";
            modalDialog.Attributes.Add("class", string.Format("modal-dialog {0} {1} ", Size.ToRender(), Position));
            if (Width.Value > 0)
                divModalStyle += string.Format("width:{0};", Width);
            if (!string.IsNullOrEmpty(divModalStyle))
                modalDialog.Attributes.Add("style", divModalStyle);

            HtmlGenericControl modalContent = new HtmlGenericControl("div");
            modalContent.Attributes.Add("class", "modal-content");
            #endregion

            #region Header
            if (ShowHeader)
            {
                HtmlGenericControl modalHeader = new HtmlGenericControl("div");

                string _class = "bg-primary";
                if (HeaderClass != string.Empty)
                    _class = HeaderClass;

                modalHeader.Attributes.Add("class", $"modal-header p-1 {_class} {this.Type.ToRender()}");
                modalHeader.Attributes.Add("id", string.Format("{0}_header", this.ClientID));

                //'**Change 01: remove condition with 'Title'
                //if (!string.IsNullOrEmpty(Title))
                //{
                //    HtmlGenericControl lbTitle = new HtmlGenericControl("span");
                //    lbTitle.InnerText = Title;
                //    lbTitle.Attributes.Add("class", "modal-title");
                //    modalHeader.Controls.Add(lbTitle);
                //}
                HtmlGenericControl lbTitle = new HtmlGenericControl("span");
                lbTitle.InnerText = Title;
                lbTitle.Attributes.Add("class", "modal-title");
                modalHeader.Controls.Add(lbTitle);

                if (HeaderButtonClose)
                {
                    HtmlGenericControl headerButtonClose = new HtmlGenericControl("a");
                    headerButtonClose.Attributes.Add("class", "btn-close mt-0 me-1");
                    headerButtonClose.Attributes.Add("onclick", $"CMSMasterJs.CloseDialog('#{this.ClientID}');");
                    headerButtonClose.Attributes.Add("aria-label", CloseText);
                    headerButtonClose.Attributes.Add("title", CloseText);
                    modalHeader.Controls.Add(headerButtonClose);
                }

                _headerControl = new Panel();
                if (HeaderTemplate != null)
                    HeaderTemplate.InstantiateIn(_headerControl);
                modalHeader.Controls.Add(_headerControl);

                modalContent.Controls.Add(modalHeader);
            }
            #endregion

            #region Body
            HtmlGenericControl modalBody = new HtmlGenericControl("div");
            modalBody.Attributes.Add("class", string.Format("modal-body {0}", BodyClass));
            _containerControl = new Panel();
            if (ContentTemplate != null)
                ContentTemplate.InstantiateIn(_containerControl);
            modalBody.Controls.Add(_containerControl);
            modalContent.Controls.Add(modalBody);
            #endregion

            #region Footer
            if (ShowFooter)
            {
                HtmlGenericControl modalFooter = new HtmlGenericControl("div");
                modalFooter.Attributes.Add("class", string.Format("modal-footer p-1 {0}", FooterClass));

                _footerControl = new Panel();
                if (FooterTemplate != null)
                    FooterTemplate.InstantiateIn(_footerControl);
                modalFooter.Controls.Add(_footerControl);

                if (FooterButtonClose)
                {
                    HtmlGenericControl buttonClose = new HtmlGenericControl("a");
                    buttonClose.Attributes.Add("class", "btn btn-outline-dark waves-effect waves-light d-flex w-auto");
                    buttonClose.Attributes.Add("onclick", $"CMSMasterJs.CloseDialog('#{this.ClientID}');");
                    buttonClose.Attributes.Add("title", CloseText);
                    buttonClose.InnerHtml = string.Format("<i class='bx bx-x'></i>{0}", CloseText);
                    modalFooter.Controls.Add(buttonClose);
                }
                modalContent.Controls.Add(modalFooter);
            }
            #endregion

            if (AppendUpdatePanel)
            {
                panelModal = new UpdatePanel();
                panelModal.UpdateMode = UpdateMode;
                panelModal.ID = this.ID + "_UpdatePanelModal";
                panelModal.ContentTemplateContainer.Controls.Add(modalContent);

                //'**Change 01
                panelModal.ChildrenAsTriggers = this.ChildrenAsTriggers;

                modalDialog.Controls.Add(panelModal);
            }
            else
                modalDialog.Controls.Add(panelModal);
            controlModal.Controls.Add(modalDialog);
            Controls.Add(controlModal);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            foreach (Control control in Controls)
            {
                control.RenderControl(writer);
            }
        }
        #region function
        public void CloseModal(bool isUpdate = false)
        {
            Page page = this.Page;
            if (page == null)
                page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;

            if (page == null)
                return;
            Type thisT = this.GetType();
            if (thisT != typeof(ExtraScriptRegister))
                thisT = thisT.BaseType;
            ClientScriptManager cs = page.ClientScript;
            ScriptManager.RegisterClientScriptBlock(Page, thisT, "CloseModal", string.Format("CMSMasterJs.CloseDialog('#{0}');", this.ClientID), true);
            if (ModalClosed != null)
            {
                ExtraControlEventArg e = new ExtraControlEventArg();
                ModalClosed(this, e);
            }

            if (isUpdate)
                UpdateContentModal();

        }
        public void OpenModal(bool isUpdate = false, int timeout = 0)
        {
            if (isUpdate)
                UpdateContentModal();

            Page page = this.Page;
            if (page == null)
                page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;
            if (page == null)
                return;
            Type thisT = this.GetType();
            if (thisT != typeof(ExtraScriptRegister))
                thisT = thisT.BaseType;

            string script = $"CMSMasterJs.OpenDialog('#{this.ClientID}', '{this.Title}');";
            if (timeout > 0)
                script = $"setTimeout(() => {{ CMSMasterJs.OpenDialog('#{this.ClientID}', '{this.Title}'); }}, {timeout})";
            ScriptManager.RegisterClientScriptBlock(page, thisT, "OpenModal", script, true);
        }
        public void UpdateContentModal()
        {
            if (panelModal != null && panelModal.UpdateMode == UpdatePanelUpdateMode.Conditional)
                panelModal.Update();
        }
        #endregion
    }
    public class ModalContentTemplate2 : ITemplate
    {
        public void InstantiateIn(Control container)
        {

        }
    }

    public delegate void ModalCloseEvent(object obj, ExtraControlEventArg e);
    public delegate void ModalOpenEvent(object obj, ExtraControlEventArg e);

    public enum ModalEffect
    {
        [Render("")]
        None,
        [Render("fade")]
        FadeIn,
        [Render("fade")]
        FadeOut,
    }

    public enum ModalSize
    {
        [Render("modal-normal")]
        Normal,
        [Render("modal-sm")]
        Small,
        [Render("modal-lg")]
        Large,
        [Render("modal-xl")]
        ExtraLarge,
        [Render("modal-fullscreen")]
        FullScreen
    }

    public enum ModalStyle
    {
        [Render("modal-default")]
        Default,
        [Render("modal-primary")]
        Primary,
        [Render("modal-success")]
        Success,
        [Render("modal-info")]
        Info,
        [Render("modal-warning")]
        Warning,
        [Render("modal-danger")]
        Danger,
        [Render("modal-violet")]
        Violet,
    }
}
