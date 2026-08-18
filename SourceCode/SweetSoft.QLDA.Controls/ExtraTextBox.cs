//-----------------------PROGRAMER LOGS---------------------------
using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [DefaultProperty("Text")]
    [ParseChildren(false)]
    [ToolboxData("<{0}:ExtraTextBox runat=\"server\" ></{0}:ExtraTextBox>")]
    public class ExtraTextBox : TextBox
    {
        #region Properties
        public string SearchTagItemText
        {
            get
            {
                object obj = ViewState["SearchTagItemText"];
                return (obj == null) ? "" : string.Format((string)obj, this.Text);
            }
            set
            {
                ViewState["SearchTagItemText"] = string.Format("{0} <b>{{0}}</b>", value);
            }
        }
        public string SearchTagItemKey
        {
            get
            {
                object obj = ViewState["SearchTagItemKey"];
                return (obj == null) ? this.ClientID : (string)obj;
            }
            set { ViewState["SearchTagItemKey"] = value; }
        }
        public SearchTagItem SearchTagItem
        {
            get
            {
                if (string.IsNullOrEmpty(this.Text))
                    return null;
                else
                    return new SearchTagItem(SearchTagItemKey, SearchTagItemText, this.Text, this.ID);
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
        public string PlaceHolder
        {
            get
            {
                object obj = ViewState["PlaceHolder"];
                return (obj == null) ? "Nhập giá trị" : (string)obj;
            }
            set { ViewState["PlaceHolder"] = value; }
        }
        public string SearchColumn
        {
            get
            {
                object obj = ViewState["SearchColumn"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["SearchColumn"] = value; }
        }
        /// <summary>
        /// ID control will click when textbox press enter
        /// </summary>
        public string EnterSubmitClientID
        {
            get
            {
                object obj = ViewState["EnterSubmitClientID"];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set { ViewState["EnterSubmitClientID"] = value; }
        }

        public bool IsAddClassDefault
        {
            get
            {
                object obj = ViewState["IsAddClassDefault"];
                return (obj == null) ? true : (bool)obj;
            }
            set { ViewState["IsAddClassDefault"] = value; }
        }
        public bool IsIMask
        {
            get
            {
                object obj = ViewState["IsIMask"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["IsIMask"] = value; }
        }
        public bool IsNumber
        {
            get
            {
                object obj = ViewState["IsNumber"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["IsNumber"] = value; }
        }

        //'**Change 01: start
        public bool IsPositiveInteger
        {
            get
            {
                object obj = ViewState["IsPositiveInteger"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["IsPositiveInteger"] = value; }
        }
        //'**Change 01: end

        public bool IsEmail
        {
            get
            {
                object obj = ViewState["IsEmail"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["IsEmail"] = value; }
        }
        public bool IsPhone
        {
            get
            {
                object obj = ViewState["IsPhone"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["IsPhone"] = value; }
        }
        public bool IsCurrency
        {
            get
            {
                object obj = ViewState["IsCurrency"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["IsCurrency"] = value; }
        }
        public int MinValue
        {
            get
            {
                object obj = ViewState["MinValue"];
                return (obj == null) ? int.MinValue : (int)obj;
            }
            set { ViewState["MinValue"] = value; }
        }
        public int MaxValue
        {
            get
            {
                object obj = ViewState["MaxValue"];
                return (obj == null) ? int.MaxValue : (int)obj;
            }
            set { ViewState["MaxValue"] = value; }
        }
        public string Autocomplete
        {
            get
            {
                object obj = ViewState["Autocomplete"];
                return (obj == null) ? "off" : (string)obj;
            }
            set { ViewState["Autocomplete"] = value; }
        }

        public string Selector
        {
            get
            {
                object obj = ViewState["Selector"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["Selector"] = value; }
        }

        public string AriaLabel
        {
            get
            {
                object obj = ViewState["AriaLabel"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AriaLabel"] = value; }
        }

        public string AriaDescribedby
        {
            get
            {
                object obj = ViewState["AriaDescribedby"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AriaDescribedby"] = value; }
        }

        public string OnKeyUp
        {
            get
            {
                object obj = ViewState["OnKeyUp"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["OnKeyUp"] = value; }
        }

        public string OnKeyDown
        {
            get
            {
                object obj = ViewState["OnKeyDown"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["OnKeyDown"] = value; }
        }

        public string OnKeyPress
        {
            get
            {
                object obj = ViewState["OnKeyPress"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["OnKeyPress"] = value; }
        }

        public string OnChange
        {
            get
            {
                object obj = ViewState["OnChange"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["OnChange"] = value; }
        }
        #endregion
        PostBackOptions GetPostBackOptions()
        {
            PostBackOptions options = new PostBackOptions(this);
            options.ActionUrl = null;
            options.ValidationGroup = null;
            options.Argument = string.Empty;
            options.RequiresJavaScriptProtocol = false;
            options.ClientSubmit = true;

            return options;
        }
        protected override void OnInit(EventArgs e)
        {
            ExtraScriptRegister.RegisterTextBox = true;
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string cssClass = string.Empty;
            if (IsAddClassDefault)
                cssClass = string.Format("form-control ext-textbox {0}", this.CssClass);
            else
                cssClass = string.Format("ext-textbox {0}", this.CssClass);

            if (!string.IsNullOrEmpty(ValidationGroup))
                cssClass += string.Format(" vlg-{0}", ValidationGroup);

            if (IsIMask || IsNumber || IsCurrency || IsPositiveInteger)
                cssClass += " text-end";
            if (Required && Enabled)
            {
                if (!string.IsNullOrEmpty(RequiredAdvanced))
                    cssClass += string.Format(" validate[required,{0}]", RequiredAdvanced);
                else
                    cssClass += string.Format(" validate[required]");
            }

            HtmlElement input = new HtmlElement("input");
            Page page = Page;
            if (page != null)
                page.VerifyRenderingInServerForm(this);
            if (this.AutoPostBack && page != null)
            {
                string onchange = page.ClientScript.GetPostBackEventReference(GetPostBackOptions(), true);
                onchange = String.Concat("ExtraTextBoxChange('", onchange.Replace("\\", "\\\\").Replace("'", "\\'"), "')");
                input.Attributes.Add(new HtmlAttribute("oninput", onchange));
            }

            if (IsIMask || IsCurrency)
                input.Attributes.Add(new HtmlAttribute("data-inputmask", this.ClientID));
            if (IsNumber || IsPositiveInteger)
                input.Attributes.Add(new HtmlAttribute("data-format-number", this.ClientID));
            if (IsCurrency)
                input.Attributes.Add(new HtmlAttribute("data-format-currency", this.ClientID));
            if (this.TextMode != TextBoxMode.MultiLine)
            {
                if (this.TextMode == TextBoxMode.Number)
                    input.Attributes.Add(new HtmlAttribute("type", this.TextMode));
                else
                    input.Attributes.Add(new HtmlAttribute("type", this.TextMode == TextBoxMode.SingleLine ? "text" : "password"));
                input.Attributes.Add(new HtmlAttribute("class", cssClass));
                input.Attributes.Add(new HtmlAttribute("id", this.ClientID));
                input.Attributes.Add(new HtmlAttribute("name", this.UniqueID));
                input.Attributes.Add(new HtmlAttribute("width", this.Width));

                if (!string.IsNullOrEmpty(this.Text))
                    input.Attributes.Add(new HtmlAttribute("value", this.Text));
                else if (this.MinValue != int.MinValue)
                    input.Attributes.Add(new HtmlAttribute("value", this.MinValue));

            }
            else if (this.TextMode == TextBoxMode.MultiLine)
            {
                input = new HtmlElement("textarea", "", "", this.Text);

                if (Required && Enabled)
                {
                    if (!string.IsNullOrEmpty(RequiredAdvanced))
                        this.CssClass += string.Format(" validate[required,{0}]", RequiredAdvanced);
                    else
                        this.CssClass += string.Format(" validate[required]");
                }

                if (IsAddClassDefault)
                    input.Attributes.Add(new HtmlAttribute("class", string.Format("ext-control form-control {0}", this.CssClass)));
                else
                    input.Attributes.Add(new HtmlAttribute("class", string.Format("ext-control {0}", this.CssClass)));
                input.Attributes.Add(new HtmlAttribute("id", this.ClientID));
                input.Attributes.Add(new HtmlAttribute("name", this.UniqueID));
                input.Attributes.Add(new HtmlAttribute("width", this.Width));
                input.Attributes.Add(new HtmlAttribute("height", this.Width));
                input.Attributes.Add(new HtmlAttribute("rows", this.Rows));
                if (!string.IsNullOrEmpty(this.Text))
                    input.Attributes.Add(new HtmlAttribute("value", this.Text));
                else if (this.MinValue != int.MinValue)
                    input.Attributes.Add(new HtmlAttribute("value", this.MinValue));

            }
            if (this.ReadOnly == true || this.Enabled == false)
            {
                input.Attributes.Add(new HtmlAttribute("disabled", "disabled"));
                input.Attributes.Add(new HtmlAttribute("mg-disabled", "disabled"));
            }
            if (string.IsNullOrEmpty(PlaceHolder) == false)
                input.Attributes.Add(new HtmlAttribute("placeHolder", PlaceHolder));

            input.Attributes.Add(new HtmlAttribute("autocomplete", Autocomplete));
            if (!string.IsNullOrEmpty(Selector))
                input.Attributes.Add(new HtmlAttribute("data-selector", Selector));

            if (!string.IsNullOrEmpty(AriaLabel))
                input.Attributes.Add(new HtmlAttribute("aria-label", AriaLabel));

            if (!string.IsNullOrEmpty(OnKeyUp))
                input.Attributes.Add(new HtmlAttribute("onkeyup", OnKeyUp));

            if (!string.IsNullOrEmpty(OnKeyDown))
                input.Attributes.Add(new HtmlAttribute("onkeydown", OnKeyDown));

            if (!string.IsNullOrEmpty(OnKeyPress))
                input.Attributes.Add(new HtmlAttribute("onkeypress", OnKeyPress));

            if (!string.IsNullOrEmpty(OnChange))
                input.Attributes.Add(new HtmlAttribute("onchange", OnChange));

            if (!string.IsNullOrEmpty(EnterSubmitClientID))
            {
                input.Attributes.Add(new HtmlAttribute("data-input-enter", "true"));
                input.Attributes.Add(new HtmlAttribute("data-enter-id", EnterSubmitClientID));
            }
            input.Attributes.AddRange(this.GetListAttributes());
            if (TabIndex > 0)
                input.Attributes.Add(new HtmlAttribute("tabindex", TabIndex));
            writer.WriteHtmlElement(input);
        }
        protected override void OnTextChanged(System.EventArgs e)
        {
            base.OnTextChanged(e);
        }
    }
}
