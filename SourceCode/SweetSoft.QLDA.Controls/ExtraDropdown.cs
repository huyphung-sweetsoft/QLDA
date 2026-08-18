using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [ToolboxData("<{0}:ExtraDropdown  runat=\"server\" />")]
    public class ExtraDropdown : DropDownList, IPostBackDataHandler, IPostBackEventHandler
    {
        #region SearchTag
        public string HdfValue
        {
            get { return this.hdfValue; }
        }
        public string HdfText
        {
            get { return this.hdfText; }
        }
        public string SearchTagItemText
        {
            get
            {
                object obj = ViewState["SearchTagItemText"];
                return (obj == null) ? "" : string.Format((string)obj, SelectedItem.Text);
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
                if (SelectedItem == null || string.IsNullOrEmpty(SelectedItem.Text) || string.IsNullOrEmpty(SelectedItem.Value))
                    return null;
                else
                    return new SearchTagItem(SearchTagItemKey, SearchTagItemText, SelectedItem.Value, this.ID);
            }
        }
        public List<SearchTagItem> ListSearchTagItem
        {
            get
            {
                if (SelectedItems == null || SelectedItems.Count == 0)
                    return null;
                else
                {
                    List<SearchTagItem> lisTag = new List<SearchTagItem>();
                    foreach (ListItem item in SelectedItems)
                    {
                        if (!string.IsNullOrEmpty(item.Text) && !string.IsNullOrEmpty(item.Value))
                            lisTag.Add(new SearchTagItem(SearchTagItemKey, string.Format(SearchTagItemTextFormat, item.Text), item.Value, this.ID));
                    }
                    return lisTag;
                }
            }
        }
        public string DefaultSearchValue
        {
            get
            {
                object obj = ViewState["DefaultSearchValue"];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set { ViewState["DefaultSearchValue"] = value; }
        }
        #endregion
        #region field

        string hdfValue = "_hdfDDLValue";
        string hdfText = "_hdfDDLText";

        string valueToIgnoreAddEmptyItem = "-1";

        List<int> _disabledIndex = null;

        #endregion

        #region CssClass method

        string _sCssClass = "";

        /// <summary>
        /// Adds the CSS class.
        /// </summary>
        /// <param name="cssClass">The CSS class.</param>
        private void AddCssClass(string cssClass)
        {
            if (String.IsNullOrEmpty(this._sCssClass))
            {
                this._sCssClass = cssClass;
            }
            else
            {
                this._sCssClass += " " + cssClass;
            }
        }

        #endregion

        #region RegionName
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
        /// Gets or sets the default item text
        /// </summary>
        /// <value>
        /// The string of default item text
        /// </value>
        [Category("Appearance")]
        public string EmptyItemText
        {
            get
            {
                object obj = ViewState["EmptyItemText"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["EmptyItemText"] = value; }
        }

        /// <summary>
        /// Gets or sets the default item value
        /// </summary>
        /// <value>
        /// The string of default item value
        /// </value>
        [Category("Appearance")]
        public string EmptyItemValue
        {
            get
            {
                object obj = ViewState["EmptyItemValue"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["EmptyItemValue"] = value; }
        }

        public List<int> DisabledIndex
        {
            get { return _disabledIndex; }
            set { _disabledIndex = value; }
        }

        public bool Tags
        {
            get
            {
                object obj = ViewState["Tags"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["Tags"] = value; }
        }

        public string[] TokenSeparators
        {
            get
            {
                object obj = ViewState["TokenSeparators"];
                return (obj == null) ? null : (string[])obj;
            }
            set { ViewState["TokenSeparators"] = value; }
        }
        [Category("Custom")]
        [DefaultValue(false)]
        public bool AlowClear
        {
            get
            {
                object obj = ViewState["AlowClear"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["AlowClear"] = value; }
        }

        public string JsonDataArray
        {
            get
            {
                object obj = ViewState["JsonDataArray"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["JsonDataArray"] = value; }
        }

        public string MaximumSelectionLength
        {
            get
            {
                object obj = ViewState["MaximumSelectionLength"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["MaximumSelectionLength"] = value; }
        }

        public string MinimumInputLength
        {
            get
            {
                object obj = ViewState["MinimumInputLength"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["MinimumInputLength"] = value; }
        }

        public string MaximumInputLength
        {
            get
            {
                object obj = ViewState["MaximumInputLength"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["MaximumInputLength"] = value; }
        }

        public string MinimumResultsForSearch
        {
            get
            {
                object obj = ViewState["MinimumResultsForSearch"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["MinimumResultsForSearch"] = value; }
        }

        public bool Multiple
        {
            get
            {
                object obj = ViewState["Multiple"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["Multiple"] = value; }
        }

        public bool SelectOnClose
        {
            get
            {
                object obj = ViewState["SelectOnClose"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["SelectOnClose"] = value; }
        }

        public bool CloseOnSelect
        {
            get
            {
                object obj = ViewState["CloseOnSelect"];
                return (obj == null) ? true : (bool)obj;
            }
            set { ViewState["CloseOnSelect"] = value; }
        }

        public bool EncryptHtml
        {
            get
            {
                object obj = ViewState["EncryptHtml"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["EncryptHtml"] = value; }
        }

        /// <summary>
        /// Gets or sets place holder text
        /// </summary>
        /// <value>
        /// The string of place holder
        /// </value>
        [Category("Appearance")]
        public string PlaceHolder
        {
            get
            {
                object obj = ViewState["PlaceHolder"];
                return (obj == null) ? "Chọn giá trị" : (string)obj;
            }
            set { ViewState["PlaceHolder"] = value; }
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
        public string DropdownCssClass
        {
            get
            {
                object obj = ViewState["DropdownCssClass"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["DropdownCssClass"] = value; }
        }

        public string ContainerCssClass
        {
            get
            {
                object obj = ViewState["ContainerCssClass"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["ContainerCssClass"] = value; }
        }

        public string Language
        {
            get
            {
                object obj = ViewState["Language"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["Language"] = value; }
        }

        public string Theme
        {
            get
            {
                object obj = ViewState["Theme"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["Theme"] = value; }
        }

        public string Dir
        {
            get
            {
                object obj = ViewState["Dir"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["Dir"] = value; }
        }

        public string AfterInitFunction
        {
            get
            {
                object obj = ViewState["AfterInitFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AfterInitFunction"] = value; }
        }

        public string EscapeMarkupFunction
        {
            get
            {
                object obj = ViewState["EscapeMarkupFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["EscapeMarkupFunction"] = value; }
        }

        public string SorterFunction
        {
            get
            {
                object obj = ViewState["SorterFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["SorterFunction"] = value; }
        }

        public bool Debug
        {
            get
            {
                object obj = ViewState["Debug"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["Debug"] = value; }
        }

        public bool DropdownAutoWidth
        {
            get
            {
                object obj = ViewState["DropdownAutoWidth"];
                return (obj == null) ? true : (bool)obj;
            }
            set { ViewState["DropdownAutoWidth"] = value; }
        }

        public bool InitAfterLoad
        {
            get
            {
                object obj = ViewState["InitAfterLoad"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["InitAfterLoad"] = value; }
        }

        public string DropdownParent
        {
            get
            {
                object obj = ViewState["DropdownParent"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["DropdownParent"] = value; }
        }

        #region Ajax setting

        public string AjaxDataPath
        {
            get
            {
                object obj = ViewState["AjaxDataPath"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxDataPath"] = value; }
        }

        public string AjaxInfoPath
        {
            get
            {
                object obj = ViewState["AjaxInfoPath"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxInfoPath"] = value; }
        }

        public string AjaxPageSize
        {
            get
            {
                object obj = ViewState["AjaxPageSize"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxPageSize"] = value; }
        }

        public string AjaxCacheNumerPage
        {
            get
            {
                object obj = ViewState["AjaxCacheNumerPage"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxCacheNumerPage"] = value; }
        }

        public string AjaxCacheKeyWordMinLength
        {
            get
            {
                object obj = ViewState["CacheKeyWordMinLength"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["CacheKeyWordMinLength"] = value; }
        }

        public string AjaxCascadingWith
        {
            get
            {
                object obj = ViewState["AjaxCascadingWith"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxCascadingWith"] = value; }
        }

        public bool AjaxCacheOnLoad
        {
            get
            {
                object obj = ViewState["AjaxCacheOnLoad"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["AjaxCacheOnLoad"] = value; }
        }

        public bool AjaxCacheExclusive
        {
            get
            {
                object obj = ViewState["AjaxCacheExclusive"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["AjaxCacheExclusive"] = value; }
        }

        public bool AjaxAutoCache
        {
            get
            {
                object obj = ViewState["AjaxAutoCache"];
                return (obj == null) ? true : (bool)obj;
            }
            set { ViewState["AjaxAutoCache"] = value; }
        }

        public string AjaxDataType
        {
            get
            {
                object obj = ViewState["AjaxDataType"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxDataType"] = value; }
        }

        public string AjaxInfoDataType
        {
            get
            {
                object obj = ViewState["AjaxInfoDataType"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxInfoDataType"] = value; }
        }

        public string AjaxDataMethodType
        {
            get
            {
                object obj = ViewState["AjaxDataMethodType"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxDataMethodType"] = value; }
        }

        public string AjaxInfoMethodType
        {
            get
            {
                object obj = ViewState["AjaxInfoMethodType"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxInfoMethodType"] = value; }
        }

        public string AjaxDataContentType
        {
            get
            {
                object obj = ViewState["AjaxDataContentType"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxDataContentType"] = value; }
        }

        public string AjaxInfoContentType
        {
            get
            {
                object obj = ViewState["AjaxInfoContentType"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxInfoContentType"] = value; }
        }

        public string AjaxProcessResultsFunction
        {
            get
            {
                object obj = ViewState["AjaxProcessResultsFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxProcessResultsFunction"] = value; }
        }

        public string AjaxDataFunction
        {
            get
            {
                object obj = ViewState["AjaxDataFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxDataFunction"] = value; }
        }

        public string AjaxBeforeSendFunction
        {
            get
            {
                object obj = ViewState["AjaxBeforeSendFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxBeforeSendFunction"] = value; }
        }

        public string AjaxTransportFunction
        {
            get
            {
                object obj = ViewState["AjaxTransportFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxTransportFunction"] = value; }
        }

        public string AjaxErrorFunction
        {
            get
            {
                object obj = ViewState["AjaxErrorFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxErrorFunction"] = value; }
        }

        public string AjaxSuccessFunction
        {
            get
            {
                object obj = ViewState["AjaxSuccessFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxSuccessFunction"] = value; }
        }

        public string TemplateResultFunction
        {
            get
            {
                object obj = ViewState["TemplateResultFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["TemplateResultFunction"] = value; }
        }

        public string TemplateSelectionFunction
        {
            get
            {
                object obj = ViewState["TemplateSelectionFunction"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["TemplateSelectionFunction"] = value; }
        }


        public string AjaxObjectSetting
        {
            get
            {
                object obj = ViewState["AjaxObjectSetting"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["AjaxObjectSetting"] = value; }
        }


        #endregion

        private EventHandler _changed;
        public event EventHandler SelectedIndexChanged
        {
            add { _changed += value; }
            remove { _changed -= value; }
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

        public string RequiredText
        {
            get
            {
                object obj = ViewState["RequiredText"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["RequiredText"] = value; }
        }

        #endregion

        public void RaisePostBackEvent(string eventArgument)
        {
            if (_changed != null && AutoPostBack)
                _changed(this, new EventArgs());
        }

        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            //string presentValue = Text;
            string postedValue = postCollection[postDataKey + hdfValue];
            string postedText = postCollection[postDataKey + hdfText];

            if (postedValue != null)
            {
                bool isMulti = this.Multiple;
                if (isMulti == false)
                {
                    SelectedValue = postedValue;
                    List<string> _selectedValues = new List<string>();
                    _selectedValues.Add(SelectedValue);
                    SelectedValues = _selectedValues;
                }
                else
                {
                    List<string> _selectedValues = null;
                    string test = postedValue.Trim('"').TrimStart('[').TrimEnd(']');
                    if (string.IsNullOrEmpty(test))
                        _selectedValues = new List<string>();
                    else
                    {
                        string[] data = test.Split(',');
                        if (data != null && data.Length > 0)
                        {
                            List<string> lst = new List<string>();
                            foreach (string item in data)
                                lst.Add(item.Substring(1, item.Length - 2));
                            _selectedValues = lst;
                        }
                    }

                    SelectedValues = _selectedValues;
                }
            }


            if (postedText != null)
            {
                bool isMulti = this.Multiple;
                if (isMulti == false)
                {
                    ViewState["SelectedText"] = postedText;
                }
                else
                {
                    List<string> _selectedTexts = new List<string>();
                    string test = postedText.Trim('"').TrimStart('[').TrimEnd(']');
                    if (string.IsNullOrEmpty(test))
                        _selectedTexts = new List<string>();
                    else
                    {
                        string[] data = test.Split(',');
                        if (data != null && data.Length > 0)
                        {
                            List<string> lst = new List<string>();
                            foreach (string item in data)
                                lst.Add(item.Substring(1, item.Length - 2));
                            _selectedTexts = lst;
                        }
                    }

                    ViewState["SelectedTexts"] = _selectedTexts;
                }
            }
            return false;
        }

        public List<int> SelectedIndexs
        {
            get
            {
                List<int> lst = new List<int>();
                ListItemCollection _items = Items;
                List<string> _selectedValues = SelectedValues;
                if (_items != null && _items.Count > 0 && _selectedValues != null)
                {
                    if (IsList(_selectedValues))
                    {
                        //list value
                        IEnumerable myList = _selectedValues as IEnumerable;
                        if (myList != null)
                        {
                            foreach (string element in myList)
                            {
                                for (int i = 0, j = _items.Count; i < j; i++)
                                {
                                    if (_items[i].Value == element)
                                    {
                                        lst.Add(i);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return lst;
            }
            set
            {
                if (value != null && value.Count > 0)
                {
                    List<string> lstData = new List<string>();
                    List<string> lstDataText = new List<string>();
                    ListItemCollection _items = Items;
                    if (_items != null && _items.Count > 0)
                    {
                        foreach (int item in value)
                        {
                            if (_items[item] != null)
                            {
                                lstData.Add(_items[item].Value);
                                lstDataText.Add(_items[item].Text);
                            }
                        }
                    }
                    ViewState["SelectedValues"] = lstData;
                    ViewState["SelectedTexts"] = lstDataText;
                }
            }
        }



        public override int SelectedIndex
        {
            get
            {
                int indx = -1;
                ListItemCollection _items = Items;
                string _selectedValues = SelectedValue;
                if (_items != null && _items.Count > 0 && _selectedValues != null)
                {
                    Type valueType = _selectedValues.GetType();
                    if (valueType == typeof(string))
                    {
                        for (int i = 0, j = _items.Count; i < j; i++)
                        {
                            if (_items[i].Value == _selectedValues.ToString())
                            {
                                indx = i;
                                break;
                            }
                        }
                    }
                }
                return indx;
            }
            set
            {
                ViewState["SelectedValue"] = string.Empty;
                ViewState["SelectedText"] = string.Empty;
                this.SelectedItems = null;
                if (value > -1)
                {
                    ListItemCollection _items = Items;
                    if (_items != null && _items.Count > 0)
                    {
                        if (_items[value] != null)
                        {
                            ViewState["SelectedValue"] = _items[value].Value;
                            ViewState["SelectedText"] = _items[value].Text;
                        }
                    }
                }
            }
        }

        public override ListItem SelectedItem
        {
            get
            {
                ListItemCollection _items = Items;
                string _selectedValues = SelectedValue;
                if (_items != null && _items.Count > 0 && _selectedValues != null)
                {
                    Type valueType = _selectedValues.GetType();
                    if (valueType == typeof(string))
                    {
                        foreach (ListItem item in _items)
                        {
                            if (item.Value == _selectedValues.ToString())
                                return item;
                        }
                    }
                }
                else
                {
                    if (_selectedValues != null)
                    {
                        Type valueType = _selectedValues.GetType();
                        if (valueType == typeof(string))
                            return new ListItem(SelectedText, _selectedValues.ToString());
                    }
                }
                return null;
            }
        }

        public void SetSelectedItem(ListItem value)
        {
            if (value == null)
            {
                ViewState["SelectedValue"] = null;
                ViewState["SelectedText"] = null;
            }
            else
            {
                ListItemCollection _items = Items;
                ListItem li = null;
                if (_items != null && _items.Count > 0)
                {
                    li = _items.FindByValue(value.Value);
                    if (li != null)
                    {
                        ViewState["SelectedText"] = li.Text;
                        ViewState["SelectedValue"] = li.Value;
                    }
                }
            }
        }

        public void SetSelectedItems(List<ListItem> value)
        {
            if (value == null || value.Count == 0)
            {
                ViewState["SelectedValues"] = null;
                ViewState["SelectedTexts"] = null;
            }
            else
            {
                ListItemCollection _items = Items;
                if (_items != null && _items.Count > 0)
                {
                    List<string> lstValue = new List<string>();
                    List<string> lstText = new List<string>();
                    foreach (ListItem listItem in value)
                    {
                        if (_items.FindByValue(listItem.Value) != null)
                        {
                            if (lstValue.Contains(listItem.Value) == false)
                                lstValue.Add(listItem.Value);
                            if (lstText.Contains(listItem.Text) == false)
                                lstText.Add(listItem.Text);
                        }
                    }
                    ViewState["SelectedValues"] = lstValue;
                    ViewState["SelectedTexts"] = lstText;
                }
                else
                {
                    List<string> lstValue = new List<string>();
                    List<string> lstText = new List<string>();
                    foreach (ListItem listItem in value)
                    {
                        if (lstValue.Contains(listItem.Value) == false)
                            lstValue.Add(listItem.Value);
                        if (lstText.Contains(listItem.Text) == false)
                            lstText.Add(listItem.Text);
                    }
                    ViewState["SelectedValues"] = lstValue;
                    ViewState["SelectedTexts"] = lstText;
                }
            }
        }

        public override string SelectedValue
        {
            get
            {
                object obj = ViewState["SelectedValue"];
                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["SelectedValue"] = value;
                SetSelectedValue(value);
            }
        }

        public string SelectedText
        {
            get
            {
                object obj = ViewState["SelectedText"];
                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                SetSelectedText(value);
            }
        }

        public void SetSelectedText(string value)
        {
            ClearSelection();
            ViewState["SelectedText"] = value;
            if (value == null)
                ViewState["SelectedValue"] = null;
            else
            {
                ListItemCollection _items = Items;
                if (_items != null && _items.Count > 0)
                {
                    ListItem li = _items.FindByText(value);
                    if (li != null)
                        ViewState["SelectedValue"] = li.Value;
                }
                else
                    ViewState["SelectedValue"] = null;
            }
        }

        public void SetSelectedValue(string value)
        {
            ClearSelection();
            ListItemCollection _items = Items;
            if (_items != null && _items.Count > 0)
            {
                List<string> selectedValues = new List<string>();
                ListItem li = _items.FindByValue(value);
                if (li != null)
                {
                    ViewState["SelectedValue"] = value;
                    ViewState["SelectedText"] = li.Text;
                    selectedValues.Add(value);
                    SelectedValues = selectedValues;
                }
            }
            //else
            //{
            //    ViewState["SelectedValue"] = null;
            //    ViewState["SelectedText"] = null;
            //    SelectedValues = null;
            //}
        }

        public bool SimpleInit
        {
            get
            {
                object obj = ViewState["SimpleInit"];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                Width = Unit.Percentage(100);
                ViewState["SimpleInit"] = value;
                CssClass += "hide";
                MinimumResultsForSearch = "Infinity";
            }
        }

        public bool SimpleAjaxInit
        {
            get
            {
                object obj = ViewState["SimpleAjaxInit"];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                Width = Unit.Percentage(100);
                ViewState["SimpleAjaxInit"] = value;
                EmptyItemValue = "-1";
                MinimumResultsForSearch = "0";
                AjaxDataType = "json";
                CssClass += "hide";
                AjaxDataMethodType = "post";
            }
        }

        public List<ListItem> SelectedItems
        {
            get
            {
                List<ListItem> lst = new List<ListItem>();
                ListItemCollection _items = Items;
                List<string> _selectedValues = SelectedValues;
                List<string> _selectedTexts = SelectedTexts;
                if (_items != null && _items.Count > 0 && _selectedValues != null)
                {
                    if (IsList(_selectedValues) == true)
                    {
                        IEnumerable myList = _selectedValues as IEnumerable;
                        if (myList != null)
                        {
                            foreach (string element in myList)
                            {
                                if (_items.FindByValue(element) != null)
                                    lst.Add(_items.FindByValue(element));
                            }
                        }
                    }
                }
                else
                {
                    if (_selectedValues != null && IsList(_selectedValues) == true)
                    {
                        var myListValue = (_selectedValues as IEnumerable).Cast<string>();
                        var myListText = (_selectedTexts as IEnumerable).Cast<string>();
                        if (myListValue != null && myListText != null)
                        {
                            for (int i = 0; i < myListValue.Count(); i++)
                                lst.Add(new ListItem(myListText.ElementAt(i), myListValue.ElementAt(i)));
                        }
                    }
                }
                return lst;
            }
            set
            {
                SetSelectedItems(value);
            }
        }

        public List<string> SelectedValues
        {
            get
            {
                object obj = ViewState["SelectedValues"];
                return (obj == null) ? null : (List<string>)obj;
            }
            set
            {
                ViewState["SelectedValues"] = value;
                if (value != null && value.Count > 0)
                {
                    ListItemCollection _items = Items;
                    if (_items != null && _items.Count > 0)
                    {
                        List<string> lst = new List<string>();
                        ListItem li = null;
                        foreach (string item in value)
                        {
                            li = _items.FindByValue(item);
                            if (li != null)
                                lst.Add(li.Text);
                        }
                        ViewState["SelectedTexts"] = lst;
                    }
                }
                else
                    ViewState["SelectedTexts"] = null;
            }
        }

        public List<string> SelectedTexts
        {
            get
            {
                object obj = ViewState["SelectedTexts"];
                return (obj == null) ? null : (List<string>)obj;
            }
            set
            {
                ViewState["SelectedTexts"] = value;
                if (value != null && value.Count > 0)
                {
                    ListItemCollection _items = Items;
                    if (_items != null && _items.Count > 0)
                    {
                        List<string> lst = new List<string>();
                        ListItem li = null;
                        foreach (string item in value)
                        {
                            li = _items.FindByText(item);
                            if (li != null)
                                lst.Add(li.Value);
                        }

                        ViewState["SelectedValues"] = lst;
                    }
                }
                else
                    ViewState["SelectedValues"] = null;
            }
        }

        public bool ForceCheckOnSubmit
        {
            get
            {
                object obj = ViewState["ForceCheckOnSubmit"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["ForceCheckOnSubmit"] = value; }
        }

        public bool ValueIsOfTypeGUID
        {
            get
            {
                object obj = ViewState["ValueIsOfTypeGUID"];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["ValueIsOfTypeGUID"] = value;
            }
        }
        #region RegionName

        private string MergeScript(string firstScript, string secondScript)
        {
            if (!string.IsNullOrEmpty(firstScript)) return (firstScript + (firstScript.EndsWith(";") ? "" : ";") + secondScript);
            if (secondScript.TrimStart(new char[0]).StartsWith("javascript:", StringComparison.Ordinal))
            {
                return secondScript + ";";
            }
            if (!secondScript.EndsWith(";"))
                secondScript = secondScript + ";";
            return ("javascript:" + secondScript);
        }

        private string EnsureEndWithSemiColon(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Trim();

                if (!value.EndsWith(";"))
                    value += ";";
            }

            return value;
        }

        #endregion

        bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        protected override void OnInit(EventArgs e)
        {
            ExtraScriptRegister.RegisterExtraDropdown = true;
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (string.IsNullOrEmpty(AjaxDataPath) == false)
                Items.Clear();

            #region RegionName

            ListItemCollection _Items = Items;

            if (_Items != null && _Items.Count > 0 && _disabledIndex != null && _disabledIndex.Count > 0)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    if (_disabledIndex.Contains(i))
                        _Items[i].Attributes.Add("disabled", "disabled");
                }
            }
            else
            {
                if (_Items != null && _Items.Count > 0)
                {
                    if (Multiple)
                    {
                        bool hasSelected = false;
                        List<string> lstValue = new List<string>();
                        List<string> lstText = new List<string>();
                        foreach (ListItem item in _Items)
                        {
                            if (item.Selected == true)
                            {
                                hasSelected = true;
                                if (lstValue.Contains(item.Value) == false)
                                    lstValue.Add(item.Value);
                                if (lstText.Contains(item.Text) == false)
                                    lstText.Add(item.Text);
                            }
                        }

                        this.ClearSelection();

                        if (hasSelected == true)
                        {
                            ViewState["SelectedValues"] = lstValue;
                            ViewState["SelectedTexts"] = lstText;
                        }
                    }
                    else
                    {
                        string _selectedValues = SelectedValue;
                        foreach (ListItem item in _Items)
                        {
                            if (item.Selected == true && _selectedValues != item.Value)
                            {
                                ViewState["SelectedText"] = item.Text;
                                ViewState["SelectedValue"] = item.Value;
                                break;
                            }
                        }
                    }
                }
            }

            if (EmptyItemValue != valueToIgnoreAddEmptyItem && _Items.Count > 0 && Multiple == false)
            {
                if (!string.IsNullOrEmpty(_Items[0].Value))
                    _Items.Insert(0, new ListItem(EmptyItemText, EmptyItemValue));
            }

            #endregion

            Page page = this.Page;

            if (page == null)
                page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;

            //Need to write this, so that LoadPostData() gets called.
            if (page != null)
                page.RegisterRequiresPostBack(this);

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div class=\"w-100 wrap-select\">");
            #region RegionName
            if (Required && Enabled)
            {
                writer.AddAttribute("required", "required");
                this.AddCssClass("validate[required]");
                //this.ContainerCssClass += string.Format(" validate[required]");
                if (RequiredText != null && RequiredText.Length > 0)
                    writer.AddAttribute("data-msg-required", RequiredText);
            }
            this.AddCssClass(this.CssClass);
            this.AddCssClass("select2");
            CssClass = _sCssClass;

            if (!string.IsNullOrEmpty(PlaceHolder))
                writer.AddAttribute("data-placeholder", PlaceHolder);

            if (!string.IsNullOrEmpty(Selector))
                writer.AddAttribute("data-selector", Selector);


            if (string.IsNullOrEmpty(DropdownParent) == false)
                writer.AddAttribute("data-dropdownParent", DropdownParent);

            if (string.IsNullOrEmpty(JsonDataArray) == false)
                writer.AddAttribute("data-jsonDataArray", JsonDataArray);

            if (string.IsNullOrEmpty(MaximumSelectionLength) == false)
                writer.AddAttribute("data-maximumSelectionLength", MaximumSelectionLength.ToString());

            if (string.IsNullOrEmpty(MinimumInputLength) == false)
                writer.AddAttribute("data-minimumInputLength", MinimumInputLength.ToString());

            if (string.IsNullOrEmpty(MaximumInputLength) == false)
                writer.AddAttribute("data-maximumInputLength", MaximumInputLength.ToString());

            if (string.IsNullOrEmpty(MaximumSelectionLength) == false)
                writer.AddAttribute("data-maximumSelectionLength", MaximumSelectionLength.ToString());

            if (string.IsNullOrEmpty(MinimumResultsForSearch) == false)
                writer.AddAttribute("data-minimumResultsForSearch", MinimumResultsForSearch.ToString());

            writer.AddAttribute("data-hdfValue", ClientID + hdfValue);
            writer.AddAttribute("data-hdfText", ClientID + hdfText);

            #region RegionName

            if (InitAfterLoad)
                writer.AddAttribute("data-initAfterLoad", "true");
            if (Multiple)
                writer.AddAttribute("multiple", "multiple");
            if (Debug)
                writer.AddAttribute("data-debug", "true");
            if (SelectOnClose)
                writer.AddAttribute("data-selectOnClose", "true");
            if (CloseOnSelect == false)
                writer.AddAttribute("data-closeOnSelect", "false");
            if (DropdownAutoWidth)
                writer.AddAttribute("data-dropdownAutoWidth", "true");
            if (Tags)
                writer.AddAttribute("data-tags", "true");
            writer.AddAttribute("data-allowClear", AlowClear ? "true" : "false");
            writer.AddAttribute("data-EncryptHtml", EncryptHtml ? "true" : "false");
            //Dir="ltr";
            if (string.IsNullOrEmpty(Dir) == false)
                writer.AddAttribute("data-dir", Dir);
            if (string.IsNullOrEmpty(AfterInitFunction) == false)
                writer.AddAttribute("data-afterInitFunction", AfterInitFunction);
            if (string.IsNullOrEmpty(EscapeMarkupFunction) == false)
                writer.AddAttribute("data-escapeMarkupFunction", EscapeMarkupFunction);
            if (string.IsNullOrEmpty(ContainerCssClass) == false)
                writer.AddAttribute("data-containerCssClass", ContainerCssClass);
            if (string.IsNullOrEmpty(DropdownCssClass) == false)
                writer.AddAttribute("data-dropdownCssClass", DropdownCssClass);

            //Theme = "default";
            if (string.IsNullOrEmpty(Theme) == false)
                writer.AddAttribute("data-theme", Theme);
            //Language = "en";
            if (string.IsNullOrEmpty(Language) == false)
                writer.AddAttribute("data-language", Language);
            if (string.IsNullOrEmpty(SorterFunction) == false)
                writer.AddAttribute("data-sorterFunction", SorterFunction);

            if (TokenSeparators != null && TokenSeparators.Length > 0)
                writer.AddAttribute("data-tokenSeparators", "['" + string.Join("','", TokenSeparators) + "']");

            #endregion

            #region Ajax

            if (string.IsNullOrEmpty(AjaxDataPath) == false)
            {
                writer.AddAttribute("data-ajax-url", AjaxDataPath);

                if (string.IsNullOrEmpty(AjaxInfoPath) == false)
                    writer.AddAttribute("data-ajax-infourl", AjaxInfoPath);
                if (string.IsNullOrEmpty(AjaxInfoDataType) == false)
                    writer.AddAttribute("data-ajax-infodataType", AjaxInfoDataType);
                if (string.IsNullOrEmpty(AjaxInfoMethodType) == false)
                    writer.AddAttribute("data-ajax-infomethodType", AjaxInfoMethodType);
                if (string.IsNullOrEmpty(AjaxInfoContentType) == false)
                    writer.AddAttribute("data-ajax-infocontentType", AjaxInfoContentType);

                if (string.IsNullOrEmpty(AjaxCascadingWith) == false)
                    writer.AddAttribute("data-ajaxCascadingWith", AjaxCascadingWith);
                if (AjaxCacheOnLoad == true)
                    writer.AddAttribute("data-ajax-cacheOnLoad", "true");
                if (AjaxCacheExclusive == true)
                    writer.AddAttribute("data-ajax-cacheExclusive", "true");
                if (string.IsNullOrEmpty(AjaxCacheNumerPage) == false)
                    writer.AddAttribute("data-ajax-cacheNumerPage", AjaxCacheNumerPage);
                if (string.IsNullOrEmpty(AjaxCacheKeyWordMinLength) == false)
                    writer.AddAttribute("data-ajax-cacheKeyWordMinLength", AjaxCacheKeyWordMinLength);
                if (AjaxAutoCache == false)
                    writer.AddAttribute("data-ajax-autoCache", "false");

                if (string.IsNullOrEmpty(AjaxPageSize) == false)
                    writer.AddAttribute("data-ajax-pageSize", AjaxPageSize);
                if (string.IsNullOrEmpty(AjaxDataType) == false)
                    writer.AddAttribute("data-ajax-dataType", AjaxDataType);
                if (string.IsNullOrEmpty(AjaxDataMethodType) == false)
                    writer.AddAttribute("data-ajax-datamethodType", AjaxDataMethodType);
                if (string.IsNullOrEmpty(AjaxDataContentType) == false)
                    writer.AddAttribute("data-ajax-datacontentType", AjaxDataContentType);
                if (string.IsNullOrEmpty(AjaxProcessResultsFunction) == false)
                    writer.AddAttribute("data-ajax-processResultsFunction", AjaxProcessResultsFunction);
                if (string.IsNullOrEmpty(AjaxDataFunction) == false)
                    writer.AddAttribute("data-ajax-dataFunction", AjaxDataFunction);
                if (string.IsNullOrEmpty(AjaxBeforeSendFunction) == false)
                    writer.AddAttribute("data-ajax-beforeSendFunction", AjaxBeforeSendFunction);
                if (string.IsNullOrEmpty(AjaxTransportFunction) == false)
                    writer.AddAttribute("data-ajax-transportFunction", AjaxTransportFunction);
                if (string.IsNullOrEmpty(AjaxErrorFunction) == false)
                    writer.AddAttribute("data-ajax-errorFunction", AjaxErrorFunction);
                if (string.IsNullOrEmpty(AjaxSuccessFunction) == false)
                    writer.AddAttribute("data-ajax-successFunction", AjaxSuccessFunction);
            }

            if (string.IsNullOrEmpty(AjaxObjectSetting) == false)
                writer.AddAttribute("data-ajax-objectSetting", AjaxObjectSetting);

            #endregion

            if (string.IsNullOrEmpty(TemplateResultFunction) == false)
                writer.AddAttribute("data-templateResultFunction", TemplateResultFunction);
            if (string.IsNullOrEmpty(TemplateSelectionFunction) == false)
                writer.AddAttribute("data-templateSelectionFunction", TemplateSelectionFunction);

            #endregion

            Page page = this.Page;
            if (page == null)
                page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;


            string onChangeScript = String.Empty;
            bool isEnabled = base.IsEnabled;
            if (isEnabled)
            {
                onChangeScript = EnsureEndWithSemiColon(Attributes["onchange"]);

                if (_changed != null && AutoPostBack == true)
                    onChangeScript = MergeScript(onChangeScript, Page.ClientScript.GetPostBackEventReference(this, String.Empty));

                if (!string.IsNullOrEmpty(onChangeScript))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onchange, onChangeScript);
                }

                if (page != null)
                {
                    string postBackEventReference = page.ClientScript.GetPostBackClientHyperlink(this, "");

                    if (postBackEventReference != null)
                        onChangeScript = MergeScript(onChangeScript, postBackEventReference);
                }
            }

            string valueHidden = string.Empty;
            string textHidden = string.Empty;
            ListItemCollection _items = Items;

            #region RegionName

            List<string> _selectedValues = SelectedValues;
            List<string> _selectedTexts = SelectedTexts;
            string _selectedValue = SelectedValue;
            string _selectedText = SelectedText;

            if (_items != null && _items.Count > 0)
            {
                if (Multiple && _selectedValues != null)
                {
                    var myListValue = _selectedValues != null ? (_selectedValues as IEnumerable).Cast<string>() : null;
                    List<string> lstValue = new List<string>();

                    var myListText = _selectedTexts != null ? (_selectedTexts as IEnumerable).Cast<string>() : null;
                    List<string> lstText = new List<string>();

                    if (myListValue != null)
                    {
                        for (int i = 0; i < myListValue.Count(); i++)
                        {
                            if (_items.FindByValue(myListValue.ElementAt(i)) != null)
                            {
                                if (myListText != null)
                                    lstText.Add(myListText.ElementAt(i));
                                lstValue.Add(myListValue.ElementAt(i));
                            }
                        }
                    }
                    else if (myListText != null)
                    {
                        for (int i = 0; i < myListText.Count(); i++)
                        {
                            if (_items.FindByText(myListText.ElementAt(i)) != null)
                            {
                                if (myListValue != null)
                                    lstValue.Add(myListValue.ElementAt(i));
                                lstText.Add(myListText.ElementAt(i));
                            }
                        }
                    }

                    if (lstValue.Count > 0)
                        valueHidden = "['" + string.Join("','", lstValue.Select(x => x.Replace("'", @"\'")).ToArray()) + "']";
                    else
                        valueHidden = "[]";

                    if (lstText.Count > 0)
                        textHidden = "['" + string.Join("','", lstText.Select(x => x.Replace("'", @"\'")).ToArray()) + "']";
                    else
                        textHidden = "[]";
                }
                else if (_selectedValue != null)
                {
                    if (_items.FindByValue(_selectedValue.ToString()) != null)
                    {
                        valueHidden = _selectedValue.ToString();
                        textHidden = _selectedText != null ? _selectedText.ToString() : string.Empty;
                    }
                }
            }
            else
            {
                if (Multiple && _selectedValues != null)
                {
                    var myList = _selectedValues != null ? (_selectedValues as IEnumerable).Cast<string>() : null;
                    if (myList != null && myList.Any())
                        valueHidden = "['" + string.Join("','", myList.Select(x => x.Replace("'", @"\'")).ToArray()) + "']";
                    else
                        valueHidden = "[]";

                    myList = _selectedTexts != null ? (_selectedTexts as IEnumerable).Cast<string>() : null;
                    if (myList != null && myList.Any())
                        textHidden = "['" + string.Join("','", myList.Select(x => x.Replace("'", @"\'")).ToArray()) + "']";
                    else
                        textHidden = "[]";
                }
                else if (_selectedValue != null)
                {
                    if (_selectedValue != null)
                        valueHidden = _selectedValue.ToString();
                    if (_selectedText != null)
                        textHidden = _selectedText.ToString();
                }
            }

            #endregion

            base.Render(writer);

            writer.WriteHtmlElement(new HtmlElement(string.Format("{0}", HtmlTextWriterTag.Input), "",
                ClientID + hdfValue, null, null,
              new HtmlAttribute[] {
                            new HtmlAttribute("type", "hidden", null),
                            new HtmlAttribute("value", valueHidden, null),
                            new HtmlAttribute("name", UniqueID + hdfValue, null) }, true, null), null);

            writer.WriteHtmlElement(new HtmlElement(string.Format("{0}", HtmlTextWriterTag.Input), "",
                ClientID + hdfText, null, null,
              new HtmlAttribute[] {
                            new HtmlAttribute("type", "hidden", null),
                            new HtmlAttribute("value", textHidden, null),
                            new HtmlAttribute("name", UniqueID + hdfText, null) }, true, null), null);
            writer.Write("</div>");
        }
    }
}
