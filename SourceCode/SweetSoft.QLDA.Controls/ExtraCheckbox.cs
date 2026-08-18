using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [ToolboxData("<{0}:ExtraCheckbox runat=\"server\"></{0}:ExtraCheckbox>")]
    public class ExtraCheckbox : CheckBox
    {
        #region base
        public string SearchColumn
        {
            get
            {
                object obj = ViewState["SearchColumn"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["SearchColumn"] = value; }
        }
        public override bool Checked
        {
            get
            {
                object obj = ViewState["Checked"];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["Checked"] = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                object obj = ViewState["Enabled"];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["Enabled"] = value;
            }
        }

        public string OffText
        {
            get
            {
                object obj = ViewState["OffText"];
                return (obj == null) ? "Off" : (string)obj;
            }
            set { ViewState["OffText"] = value; }
        }

        public string OnText
        {
            get
            {
                object obj = ViewState["OnText"];
                return (obj == null) ? "On" : (string)obj;
            }
            set { ViewState["OnText"] = value; }
        }
        public string OnChange
        {
            get
            {
                object obj = ViewState["OnChange"];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set { ViewState["OnChange"] = value; }
        }
        public string Selector
        {
            get
            {
                object obj = ViewState["Selector"];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set { ViewState["Selector"] = value; }
        }
        public SwitchTypes SwitchType
        {
            get
            {
                object obj = ViewState["SwitchType"];
                return (obj == null) ? SwitchTypes.None : (SwitchTypes)obj;
            }
            set
            {
                ViewState["SwitchType"] = value;
            }
        }

        public bool Required
        {
            get
            {
                object obj = ViewState["Required"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["Required"] = value; }
        }
        public string RequiredAdvanced
        {
            get
            {
                object obj = ViewState["RequiredAdvanced"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["RequiredAdvanced"] = value; }
        }
        #endregion
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            string cssClass = string.Empty;
            if (Required)
            {
                if (!string.IsNullOrEmpty(RequiredAdvanced))
                    cssClass += string.Format(" validate[required,{0}]", RequiredAdvanced);
                else
                    cssClass += string.Format(" validate[required]");
            }
            writer.Write("<div class='group-switch " + cssClass + "' id='switch_" + this.ClientID + "'>");
            writer.AddAttribute("switch", SwitchType.ToRender());

            if (Checked)
                writer.AddAttribute("checked", "checked");

            if (!Enabled)
                writer.AddAttribute("disabled", "disabled");


            if (!string.IsNullOrEmpty(OnChange))
                writer.AddAttribute("onchange", OnChange);

            if (!string.IsNullOrEmpty(Selector))
                writer.AddAttribute("data-selector", Selector);

            Page page = Page;
            if (page != null)
                page.VerifyRenderingInServerForm(this);

            base.Render(writer);
            HtmlElement _tagLabel = new HtmlElement("label");
            _tagLabel.Attributes.Add(new HtmlAttribute("id", this.ClientID + "_label"));
            _tagLabel.Attributes.Add(new HtmlAttribute("for", this.ClientID));
            _tagLabel.Attributes.Add(new HtmlAttribute("class", "m-0"));
            if (!string.IsNullOrEmpty(OffText))
                _tagLabel.Attributes.Add(new HtmlAttribute("data-off-label", OffText));

            if (!string.IsNullOrEmpty(OnText))
                _tagLabel.Attributes.Add(new HtmlAttribute("data-on-label", OnText));


            writer.WriteHtmlElement(_tagLabel);
            writer.Write("</div>");
        }

        public enum SwitchTypes
        {
            [Render("none")]
            None,
            [Render("bool")]
            Bool,
            [Render("default")]
            Default,
            [Render("primary")]
            Primary,
            [Render("info")]
            Info,
            [Render("warning")]
            Warning,
            [Render("danger")]
            Danger,
            [Render("success")]
            Success,
            [Render("dark")]
            Dark
        }
    }
}
