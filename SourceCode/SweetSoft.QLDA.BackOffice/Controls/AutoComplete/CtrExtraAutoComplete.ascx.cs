//--------------------PROGRAMER LOGS------------------------
//Created by:
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using SweetSoft.QLDA.Core.Infrastructure;

namespace SweetSoft.QLDA.BackOffice.Controls.AutoComplete
{
    public partial class CtrExtraAutoComplete : BaseAdminUserControl
    {
        #region Script + Styles
        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>();
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Controls/AutoComplete/jquery-ui.min.css"));
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Controls/AutoComplete/Autocomplete.css"));
                List<string> jsLinks = new List<string>();
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Controls/AutoComplete/jquery-ui.min.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Controls/AutoComplete/Autocomplete.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath($"/Controls/AutoComplete/Autocomplete-{SweetContext.Current.CurrentLanguageCode}.js"));
                return new RegisterCSSAndJS("cpHeadVendor", "cpVendorScript", cssLinks, jsLinks);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }
        #endregion
        #region searchTag
        public bool AutoSelect
        {
            get
            {
                if (ViewState["AutoSelect"] == null)
                    return false;
                return (bool)ViewState["AutoSelect"];
            }
            set
            {
                ViewState["AutoSelect"] = value;
            }
        }
        public string SearchTagItemText
        {
            get
            {
                object obj = ViewState["SearchTagItemText"];
                return (obj == null) ? "" : string.Format((string)obj, this.Value);
            }
            set
            {
                ViewState["SearchTagItemText"] = string.Format("{0} <b>{{0}}</b>", value);
            }
        }
        public string SearchTagItemTextFormat
        {
            get
            {
                object obj = ViewState["SearchTagItemText"];
                return (obj == null) ? "" : (string)obj;
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
                if (string.IsNullOrEmpty(this.Value))
                    return null;
                else
                {
                    return new SearchTagItem(this.SearchTagItemKey, this.SearchTagItemText, JsonConvert.SerializeObject(Item), this.ID);
                }
            }
        }
        public List<SearchTagItem> ListSearchTagItem
        {
            get
            {
                if (Items == null || Items.Count == 0)
                    return null;
                else
                {
                    List<SearchTagItem> lisTag = new List<SearchTagItem>();
                    foreach (AutocompleteItem item in Items)
                    {
                        if (!string.IsNullOrEmpty(item.Value) && !string.IsNullOrEmpty(item.Data)
                            && item.Data != "-1")
                        {
                            lisTag.Add(new SearchTagItem(SearchTagItemKey, string.Format(this.SearchTagItemTextFormat, item.Value), JsonConvert.SerializeObject(item), this.ID));
                        }
                    }
                    return lisTag;
                }
            }
        }
        #endregion 
        public event OnChanged ServerChanged;
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            hdfValue.AutoPostBack = ServerChanged != null;
        }
        #region public virtual
        public virtual AutocompleteItem Item
        {
            get { return AutocompleteHelper.GetValue(hdfValue.Text); }
            set { hdfValue.Text = AutocompleteHelper.SetValue(value); }
        }
        public List<AutocompleteItem> Items
        {
            get { return AutocompleteHelper.GetValues(hdfValue.Text); }
            set { hdfValue.Text = AutocompleteHelper.SetValues(value); }
        }
        public virtual string ItemsToString
        {
            get { return hdfValue.Text; }
            set { hdfValue.Text = value; }
        }
        public virtual string Value
        {
            get
            {
                return AutocompleteHelper.GetValue(hdfValue.Text).Value;
            }
            set
            {
                AutocompleteItem item = this.Item;
                if (item == null)
                    item = new AutocompleteItem();
                item.Value = value;
                this.Item = item;
            }
        }
        public virtual string Data
        {
            get
            {
                return AutocompleteHelper.GetValue(hdfValue.Text).Data;
            }
            set
            {
                AutocompleteItem item = this.Item;
                if (item == null)
                    item = new AutocompleteItem();
                item.Data = value;
                this.Item = item;
            }
        }
        public virtual string OtherData
        {
            get
            {
                return AutocompleteHelper.GetValue(hdfValue.Text).OtherData;
            }
            set
            {
                AutocompleteItem item = this.Item;
                if (item == null)
                    item = new AutocompleteItem();
                item.OtherData = value;
                this.Item = item;
            }
        }
        //
        // Summary:
        //     Specifies the selection mode of the System.Web.UI.WebControls.ListBox control.
        public enum ListAutocompleteSelectionMode
        {
            //
            // Summary:
            //     Single item selection mode.
            Single = 0,
            //
            // Summary:
            //     Multiple item selection mode.
            Multiple = 1,
            //
            // Summary:
            //     Multiple tags selection mode.
            MultipleTags = 2
        }
        public virtual ListAutocompleteSelectionMode SelectionMode { get; set; } = ListAutocompleteSelectionMode.Single;
        public virtual string ValidationGroup
        {
            get; set;
        }
        public virtual string ParentClass
        {
            get; set;
        }
        private string _onClientChange;
        public virtual string OnClientChange
        {
            get
            {
                return $"{this.ClientID}{this._onClientChange}";
            }
            set
            {
                this._onClientChange = value;
            }
        }
        public virtual bool InsertEmptyData
        {
            get;
            set;
        } = false;
        [Category("Data")]
        [Description("Model of combobox")]
        public virtual AutoCompleteStyle ComboStyle
        {
            get;
            set;
        }
        public virtual bool Required
        {
            get
            {
                if (ViewState["Required"] == null)
                    return false;

                return (bool)ViewState["Required"];
            }
            set { ViewState["Required"] = value; }
        }
        public virtual string Source
        {
            get
            {
                string sourceTop = (string)ViewState["Source"];
                return sourceTop;
            }
            set { ViewState["Source"] = value; }
        }
        public virtual string SourceBottom
        {
            get;
            set;
        }
        public virtual string SourceTop
        {
            get
            {
                string sourceTop = (string)ViewState["SourceTop"];
                return sourceTop;
            }
            set { ViewState["SourceTop"] = value; }
        }
        public virtual long? MaxResult
        {
            get;
            set;
        }
        public virtual string Lang
        {
            get; set;
        }

        public virtual TextBox HDFValue
        {
            get
            {
                return hdfValue;
            }
        }
        public string ValidInputId
        {
            get { return string.Format("live-text{0}", HDFValue.ClientID); }
        }
        public virtual string OnInitCallback
        {
            get;
            set;
        }
        public virtual string PlaceHolder
        {
            get
            {
                string placeHolder = (string)ViewState["PlaceHolder"];

                if (string.IsNullOrEmpty(placeHolder))
                    return "Enter value";

                return placeHolder;
            }
            set { ViewState["PlaceHolder"] = value; }
        }
        public virtual string NotFoundText
        {
            get
            {
                string notFoundText = (string)ViewState["NotFoundText"];
                return notFoundText;
            }
            set { ViewState["NotFoundText"] = value; }
        }
        public virtual bool? Enabled
        {
            get
            {
                if (ViewState["EnabledAutoComplete"] == null)
                    return true;
                return (bool)ViewState["EnabledAutoComplete"];
            }
            set { ViewState["EnabledAutoComplete"] = value; }
        }
        #endregion
        protected string RenderAttributes
        {
            get
            {
                string render = string.Empty;
                render += string.Format("data-selector=\"{0}\"", this.ID);
                render += string.Format("data-hdfvalue=\"{0}\"", hdfValue.ClientID);
                if (!string.IsNullOrEmpty(this.ValidationGroup))
                    render += string.Format("data-validationgroup=\"{0}\"", this.ValidationGroup);

                render += string.Format("data-required=\"{0}\"", this.Required ? "true" : "false");

                if (!string.IsNullOrEmpty(this.ParentClass))
                    render += string.Format("data-parentClass=\"{0}\"", this.ParentClass);
                if (this.AutoSelect)
                    render += "data-autoselect=\"1\"";
                //SelectionMode
                if (this.SelectionMode == ListAutocompleteSelectionMode.MultipleTags)
                {
                    render += "data-mutiltags=\"true\"";
                    //this.Source = "UCAutocomplete.EmptySource";
                    //this.SourceBottom = this.SourceTop = string.Empty;
                    this.SelectionMode = ListAutocompleteSelectionMode.Multiple;
                }
                render += string.Format("data-selectionmode=\"{0}\"", this.SelectionMode);

                if (!string.IsNullOrEmpty(this.OnClientChange))
                    render += string.Format("data-onchange=\"{0}\"", this.OnClientChange);
                render += string.Format("data-emptydata=\"{0}\"", this.InsertEmptyData ? 1 : 0);
                render += string.Format("data-style=\"{0}\"", EnumHelper.ToRender(this.ComboStyle));
                if (!string.IsNullOrEmpty(this.Source))
                    render += string.Format("data-source=\"{0}\"", this.Source);
                if (!string.IsNullOrEmpty(this.SourceBottom))
                    render += string.Format("data-sourcebottom=\"{0}\"", this.SourceBottom);
                if (!string.IsNullOrEmpty(this.SourceTop))
                    render += string.Format("data-sourcetop=\"{0}\"", this.SourceTop);
                if (this.MaxResult == null)
                    this.MaxResult = 15;
                render += string.Format("data-maxresult=\"{0}\"", this.MaxResult.Value);
                if (string.IsNullOrEmpty(this.Lang))
                    this.Lang = SweetContext.Current.CurrentLanguageCode;
                render += string.Format("data-lang=\"{0}\"", this.Lang);

                if (!string.IsNullOrEmpty(this.OnInitCallback))
                    render += string.Format("data-initcallback=\"{0}\"", this.OnInitCallback);
                if (!string.IsNullOrEmpty(this.PlaceHolder))
                    render += string.Format("data-placeholder=\"{0}\"", this.PlaceHolder);
                if (!string.IsNullOrEmpty(this.NotFoundText))
                    render += string.Format("data-notfoundtext=\"{0}\"", this.NotFoundText);
                if (this.Enabled != null && !this.Enabled.Value)
                    render += string.Format("data-enabled=\"{0}\"", this.Enabled);
                return render;
            }
        }
        public delegate void OnChanged(/*object sender, EventArgs e*/);
        protected void hdfValue_ServerChange(object sender, EventArgs e)
        {
            if (ServerChanged != null)
                ServerChanged(/*sender, e*/);
        }

        public enum AutoCompleteStyle
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
            Secondary
        }
    }
}