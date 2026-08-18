using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [ToolboxData("<{0}:ExtraButton runat=\"server\"></{0}:ExtraButton>")]
    public class ExtraButton : LinkButton, IPostBackEventHandler
    {
        #region Properties

        private string _prefixKey
        {
            get
            {
                return string.Format("_{0}", new Guid());
            }
        }

        public override string Text
        {
            get
            {
                object obj = ViewState["Text" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["Text" + _prefixKey] = value;
            }
        }

        public override string CssClass
        {
            get
            {
                object obj = ViewState["CssClass" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["CssClass" + _prefixKey] = value;
            }
        }
        public string NavigateUrl
        {
            get
            {
                object obj = ViewState["NavigateUrl" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["NavigateUrl" + _prefixKey] = value;
            }
        }

        public string Tagert
        {
            get
            {
                object obj = ViewState["Tagert" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["Tagert" + _prefixKey] = value;
            }
        }
        [Browsable(true)]
        [DefaultValue(false)]
        public bool AutoPostBack
        {
            get
            {
                object obj = ViewState["AutoPostBack" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["AutoPostBack" + _prefixKey] = value;
            }
        }
        public bool IsCustomClass
        {
            get
            {
                object obj = ViewState["IsCustomClass" + _prefixKey];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["IsCustomClass" + _prefixKey] = value;
            }
        }
        public ButtonsStyle ButtonStyle
        {
            get
            {
                object obj = ViewState["ButtonStyle" + _prefixKey];
                return (obj == null) ? ButtonsStyle.None : (ButtonsStyle)obj;
            }
            set
            {
                ViewState["ButtonStyle" + _prefixKey] = value;
            }
        }

        public ButtonsSize ButtonSize
        {
            get
            {
                object obj = ViewState["ButtonsSize" + _prefixKey];
                return (obj == null) ? ButtonsSize.None : (ButtonsSize)obj;
            }
            set
            {
                ViewState["ButtonsSize" + _prefixKey] = value;
            }
        }

        public ButtonsIcon ButtonIcon
        {
            get
            {
                object obj = ViewState["ButtonsIcon" + _prefixKey];
                return (obj == null) ? ButtonsIcon.None : (ButtonsIcon)obj;
            }
            set
            {
                ViewState["ButtonsIcon" + _prefixKey] = value;
            }
        }

        public ButtonsIcon ButtonIconCollapse
        {
            get
            {
                object obj = ViewState["ButtonIconCollapse" + _prefixKey];
                return (obj == null) ? ButtonsIcon.None : (ButtonsIcon)obj;
            }
            set
            {
                ViewState["ButtonIconCollapse" + _prefixKey] = value;
            }
        }

        public string TextCollapse
        {
            get
            {
                object obj = ViewState["TextCollapse" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["TextCollapse" + _prefixKey] = value;
            }
        }

        public AnimationSubmits AnimationSubmit
        {
            get
            {
                object obj = ViewState["AnimationSubmit" + _prefixKey];
                return (obj == null) ? AnimationSubmits.ZoomOut : (AnimationSubmits)obj;
            }
            set
            {
                ViewState["AnimationSubmit" + _prefixKey] = value;
            }
        }
        public bool IsBlockButton
        {
            get
            {
                object obj = ViewState["IsBlockButton" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IsBlockButton" + _prefixKey] = value;
            }
        }
        public bool IsExcludeLock
        {
            get
            {
                object obj = ViewState["IsExcludeLock" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IsExcludeLock" + _prefixKey] = value;
            }
        }
        public bool IsActive
        {
            get
            {
                object obj = ViewState["IsActive" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IsActive" + _prefixKey] = value;
            }
        }

        public bool Disabled
        {
            get
            {
                object obj = ViewState["Disabled" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["Disabled" + _prefixKey] = value;
            }
        }
        public string Role
        {
            get
            {
                object obj = ViewState["Role" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["Role" + _prefixKey] = value;
            }
        }
        public string DataBsToggle
        {
            get
            {
                object obj = ViewState["DataBsToggle" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["DataBsToggle" + _prefixKey] = value;
            }
        }
        public string DataBsTarget
        {
            get
            {
                object obj = ViewState["DataBsTarget" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["DataBsTarget" + _prefixKey] = value;
            }
        }

        public string DataBsOpenButton
        {
            get
            {
                object obj = ViewState["DataBsOpenButton" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["DataBsOpenButton" + _prefixKey] = value;
            }
        }
        public string Selector
        {
            get
            {
                object obj = ViewState["Selector" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["Selector" + _prefixKey] = value;
            }
        }

        public string AriaExpanded
        {
            get
            {
                object obj = ViewState["AriaExpanded" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["AriaExpanded" + _prefixKey] = value;
            }
        }

        public string AriaControls
        {
            get
            {
                object obj = ViewState["AriaControls" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["AriaControls" + _prefixKey] = value;
            }
        }

        public bool IsPace
        {
            get
            {
                object obj = ViewState["IsPace" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IsPace" + _prefixKey] = value;
            }
        }
        public bool IsLoading
        {
            get
            {
                object obj = ViewState["IsLoading" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IsLoading" + _prefixKey] = value;
            }
        }
        public int TimeOut
        {
            get
            {
                object obj = ViewState["TimeOut" + _prefixKey];
                return (obj == null) ? 2000 : (int)obj;
            }
            set
            {
                ViewState["TimeOut" + _prefixKey] = value;
            }
        }

        public bool IsSubmit
        {
            get
            {
                object obj = ViewState["IsSubmit" + _prefixKey];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["IsSubmit" + _prefixKey] = value;
            }
        }
        #endregion
        protected override void OnInit(EventArgs e)
        {
            ExtraScriptRegister.RegisterButton = true;
            base.OnInit(e);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            string _cssClass = string.Format("btn {0}", this.CssClass); // Default css class
            if (IsCustomClass)
                _cssClass += string.Format(" {0} {1} btn-ladda btn-spiner btn-custom", ButtonStyle.ToRender(), ButtonSize.ToRender());
            else
                _cssClass += string.Format(" {0} {1} btn-ladda btn-spiner", ButtonStyle.ToRender(), ButtonSize.ToRender());

            Page page = Page;
            HtmlElement _tagButton = new HtmlElement("a");
            _tagButton.Attributes.AddRange(this.GetListAttributes());

            if (IsActive)
                _cssClass += string.Format(" {0}", "active");

            if (IsBlockButton || !Enabled)
                _cssClass += string.Format(" {0}", "btn-block");

            if (!string.IsNullOrEmpty(Role))
                _tagButton.Attributes.Add(new HtmlAttribute("role", Role));

            if (!string.IsNullOrEmpty(DataBsToggle))
                _tagButton.Attributes.Add(new HtmlAttribute("data-bs-toggle", DataBsToggle));

            if (!string.IsNullOrEmpty(DataBsTarget))
                _tagButton.Attributes.Add(new HtmlAttribute("data-bs-target", DataBsTarget));

            if (!string.IsNullOrEmpty(DataBsOpenButton))
                _tagButton.Attributes.Add(new HtmlAttribute("data-bs-open-button", DataBsOpenButton));

            if (!string.IsNullOrEmpty(AriaExpanded))
                _tagButton.Attributes.Add(new HtmlAttribute("aria-expanded", AriaExpanded));

            if (!string.IsNullOrEmpty(AriaControls))
                _tagButton.Attributes.Add(new HtmlAttribute("aria-controls", AriaControls));

            if (!string.IsNullOrEmpty(Selector))
                _tagButton.Attributes.Add(new HtmlAttribute("data-selector", Selector));

            if (ButtonIcon != ButtonsIcon.None)
                _tagButton.Attributes.Add(new HtmlAttribute("data-icon-default", ButtonIcon.ToRender()));

            if (!string.IsNullOrEmpty(this.Text))
                _tagButton.Attributes.Add(new HtmlAttribute("data-title", this.Text));

            if (ButtonIconCollapse != ButtonsIcon.None)
                _tagButton.Attributes.Add(new HtmlAttribute("data-icon-collapse", ButtonIconCollapse.ToRender()));

            if (!string.IsNullOrEmpty(TextCollapse))
                _tagButton.Attributes.Add(new HtmlAttribute("data-text-collapse", TextCollapse));

            if (Disabled)
                _tagButton.Attributes.Add(new HtmlAttribute("disabled", "disabled"));

            _tagButton.Attributes.Add(new HtmlAttribute("class", _cssClass));
            _tagButton.Attributes.Add(new HtmlAttribute("data-id", ID));
            _tagButton.Attributes.Add(new HtmlAttribute("data-timeout", TimeOut));
            _tagButton.Attributes.Add(new HtmlAttribute("data-loading", IsLoading));
            _tagButton.Attributes.Add(new HtmlAttribute("data-style", AnimationSubmit.ToRender()));

            #region Build script
            string _extendScript = this.OnClientClick;
            string _postbackScript = string.Empty;
            if (page != null)
                _postbackScript = HtmlElement.MergeScript(_postbackScript, page.ClientScript.GetPostBackClientHyperlink(this, ""));

            if (base.HasAttributes)
            {
                string baseClick = base.Attributes["onclick"];
                if (baseClick != null && !string.IsNullOrEmpty(baseClick))
                {
                    _extendScript = HtmlElement.MergeScript(_extendScript, baseClick);
                    base.Attributes.Remove("onclick");
                }
            }
            #endregion

            #region Build Icon
            if (ButtonIcon != ButtonsIcon.None)
            {
                string icon = ButtonIcon.ToRender();
                _tagButton.Attributes.Add(new HtmlAttribute("data-before-icon", icon));

                HtmlElement _tagIcon = new HtmlElement("i");
                _tagIcon.Attributes.Add(new HtmlAttribute("id", this.ClientID + "_icon"));
                _tagIcon.Attributes.Add(new HtmlAttribute("class", "icon " + icon));
                //HtmlElement _tagBold = new HtmlElement("b");
                //_tagBold.Elements.Add(_tagIcon);
                _tagButton.Elements.Add(_tagIcon);
            }
            #endregion

            HtmlElement _tagSpanText = new HtmlElement("span", string.Format("title-button button_{0} ladda-label ms-1", this.ClientID),
                                               string.Format("{0}_text", this.ClientID), this.Text,
                                               null, null, true, null);
            if (!string.IsNullOrEmpty(Text))
                _tagButton.Elements.Add(_tagSpanText);


            if (string.IsNullOrEmpty(NavigateUrl))
            {
                if (IsPace)
                    _extendScript = string.Format("CMSMasterJs.PaceRestart();{0}", _extendScript);
                if (!string.IsNullOrEmpty(_extendScript) && this.Enabled)
                    _tagButton.Attributes.Add(new HtmlAttribute("onclick", _extendScript, IsEnabled));
                if (!string.IsNullOrEmpty(_postbackScript) && this.Enabled)
                    _tagButton.Attributes.Add(new HtmlAttribute("href", _postbackScript));
            }
            else if (this.Enabled)
            {
                _tagButton.Attributes.Add(new HtmlAttribute("href", NavigateUrl));
                _tagButton.Attributes.Add(new HtmlAttribute("target", Tagert));
            }
            if(!Enabled)
                _tagButton.Attributes.Add(new HtmlAttribute("disabled", "disabled"));
            if (!string.IsNullOrEmpty(Text))
                _tagButton.Attributes.Add(new HtmlAttribute("title", Text));

            if (!string.IsNullOrEmpty(ToolTip))
                _tagButton.Attributes.Add(new HtmlAttribute("title", ToolTip));

            _tagButton.Attributes.Add(new HtmlAttribute("data-submit", IsSubmit));

            _tagButton.Attributes.Add(new HtmlAttribute("name", this.UniqueID));

            _tagButton.Attributes.Add(new HtmlAttribute("id", this.ClientID));

            if (string.IsNullOrEmpty(NavigateUrl))
                _tagButton.Attributes.Add(new HtmlAttribute("type", "submit"));

            string _tagButtonStyle = "";
            if (this.Width.Value > 0)
                _tagButtonStyle += string.Format("width:{0}", this.Width);
            if (!string.IsNullOrEmpty(_tagButtonStyle))
                _tagButton.Attributes.Add(new HtmlAttribute("style", _tagButtonStyle));
            if (TabIndex > 0)
                _tagButton.Attributes.Add(new HtmlAttribute("tabindex", TabIndex));
            writer.WriteHtmlElement(_tagButton);
        }

        public enum ButtonsStyle
        {
            [Render("")]
            None,
            [Render("btn-default")]
            Default,
            [Render("btn-primary")]
            Primary,
            [Render("btn-info")]
            Info,
            [Render("btn-warning")]
            Warning,
            [Render("btn-danger")]
            Danger,
            [Render("btn-success")]
            Success,
            [Render("btn-secondary")]
            Secondary,
            [Render("btn-outline-primary waves-effect waves-light")]
            OutLinePrimary,
            [Render("btn-outline-info waves-effect waves-light")]
            OutLineInfo,
            [Render("btn-outline-warning waves-effect waves-light")]
            OutLineWarning,
            [Render("btn-outline-danger waves-effect waves-light")]
            OutLineDanger,
            [Render("btn-outline-success waves-effect waves-light")]
            OutLineSuccess,
            [Render("btn-outline-secondary waves-effect waves-light")]
            OutLineSecondary,
        }
        public enum AnimationSubmits
        {
            [Render("expand-left")]
            ExpandLeft,
            [Render("expand-right")]
            ExpandRight,
            [Render("expand-up")]
            ExpandUp,
            [Render("expand-down")]
            ExpandDown,
            [Render("slide-left")]
            SlideLeft,
            [Render("slide-right")]
            SlideRight,
            [Render("slide-up")]
            SlideUp,
            [Render("slide-down")]
            SlideDown,
            [Render("zoom-in")]
            ZoomIn,
            [Render("zoom-out")]
            ZoomOut
        }
        public enum ButtonsSize
        {
            [Render("")]
            None,
            [Render("btn-sm")]
            Small,
            [Render("btn-md")]
            Flat,
            [Render("btn-lg")]
            Large
        }

        public enum ButtonsIcon
        {
            [Render("")]
            None,
            [Render("fas fa-search")]
            Search,
            [Render("fas fa-plus")]
            Add,
            [Render("fas fa-pencil-alt")]
            Edit,
            [Render("fas fa-trash")]
            Remove,
            [Render("fas fa-save")]
            Save,
            [Render("fas fa-check")]
            Check,
            [Render("fas fa-retweet")]
            Retweet,
            [Render("fas fa-unlock")]
            UnLock,
            [Render("fas fa-lock")]
            Lock,
            [Render("fas fa-sign-out-alt")]
            SignOut,
            [Render("fas fa-redo")]
            Repeat,
            [Render("fas fa-check-double")]
            Accept,
            [Render("far fa-window-close")]
            Close,
            [Render("fas fa-pencil-alt")]
            Detail,
            [Render("fas fa-reply-all")]
            Reply,
            [Render("fas fa-eye")]
            View,
            [Render("fas fa-angle-double-down")]
            DoubleDown,
            [Render("fas fa-angle-double-up")]
            DoubleUp,
            [Render("fas fa-angle-up")]
            Up,
            [Render("fas fa-angle-down")]
            Down,
            [Render("fas fa-download")]
            DownLoad,
            [Render("fas fa-clone")]
            Clone,
            [Render("fas fa-copy")]
            Coppy,
            [Render("fas fa-envelope")]
            Envelope,
            [Render("fas fa-envelope-open")]
            EnvelopeOpen,
            [Render("fas fa-sync-alt")]
            Refresh,
            [Render("fas fa-file-alt")]
            File,
            [Render("fas fa-file-excel")]
            Excel,
            [Render("fas fa-file-word")]
            Word,
            [Render("fas fa-file-pdf")]
            Pdf,
            [Render("fas fa-snowflake")]
            Snowflake,
            [Render("fas fa-database")]
            Database,
            [Render("fas fa-upload")]
            Upload,
            [Render("fas fa-chart-bar")]
            ChartBar,
            [Render("fas fa-chart-line")]
            ChartLine,
            [Render("fas fa-random")]
            Random,
            [Render("fas fa-paper-plane")]
            Send,
            [Render("fas fa-file-invoice")]
            Invoice,
            [Render("fas fa-list")]
            List,
            [Render("fas fa-qrcode")]
            QRCode,
            [Render("fas fa-credit-card")]
            Payment,
            [Render("bx bx-transfer")]
            Transfer,
            [Render("fas fa-file-signature")]
            Confirm,
            [Render("fas fa-cog")]
            Setting,
            [Render("fas fa-cogs")]
            Settings,
            [Render("fas fa-power-off")]
            PowerOff,
            [Render("fas fa-times")]
            Cancel,
            [Render("fas fa-undo")]
            Undo,
            [Render("fas fa-users")]
            User,
            [Render("fas fa-unlink")]
            Unlink,
            [Render("fas fa-print")]
            Print,
            [Render("fas fa-calendar-alt")]
            Calendar,
            [Render("fas fa-bell")]
            Bell
        }
    }
}
