using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BootstrapDropdown runat=server></{0}:BootstrapDropdown>")]
    public class BootstrapDropdown : WebControl, IPostBackEventHandler, IPostBackDataHandler
    {
        private bool _hasRaisedEvent = false; // Flag để tránh raise event nhiều lần
        #region Properties

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? "Chọn..." : s);
            }
            set
            {
                ViewState["Text"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        public string SelectedValue
        {
            get
            {
                String s = (String)ViewState["SelectedValue"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["SelectedValue"] = value;
            }
        }
        [Bindable(false)]
        [Browsable(false)]
        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DropdownItem SelectedItem
        {
            get
            {
                string selectedValue = SelectedValue;
                if (!string.IsNullOrEmpty(selectedValue))
                {
                    var item = Items.Find(x => x.Value == selectedValue);
                    return item ?? new DropdownItem("", selectedValue);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    SelectedValue = value.Value;
                }
                else
                {
                    SelectedValue = string.Empty;
                }
            }
        }

        [Bindable(false)]
        [Browsable(false)]
        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                DropdownItem selectedItem = SelectedItem;
                return selectedItem != null ? selectedItem.Text : string.Empty;
            }
        }

        [Bindable(false)]
        [Browsable(false)]
        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get
            {
                string selectedValue = SelectedValue;
                if (!string.IsNullOrEmpty(selectedValue))
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Value == selectedValue)
                            return i;
                    }
                }
                return -1;
            }
            set
            {
                if (value >= 0 && value < Items.Count)
                {
                    SelectedValue = Items[value].Value;
                }
                else
                {
                    SelectedValue = string.Empty;
                }
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool AutoPostBack
        {
            get
            {
                object b = ViewState["AutoPostBack"];
                return ((b == null) ? false : (bool)b);
            }
            set
            {
                ViewState["AutoPostBack"] = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool AllowClear
        {
            get
            {
                object b = ViewState["AllowClear"];
                return ((b == null) ? false : (bool)b);
            }
            set
            {
                ViewState["AllowClear"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string ClearText
        {
            get
            {
                String s = (String)ViewState["ClearText"];
                return ((s == null) ? "-- Bỏ chọn --" : s);
            }
            set
            {
                ViewState["ClearText"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        public string DataTextField
        {
            get
            {
                String s = (String)ViewState["DataTextField"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["DataTextField"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        public string DataValueField
        {
            get
            {
                String s = (String)ViewState["DataValueField"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["DataValueField"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get
            {
                return ViewState["DataSource"];
            }
            set
            {
                ViewState["DataSource"] = value;
            }
        }

        private List<DropdownItem> _items = new List<DropdownItem>();
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Category("Data")]
        public List<DropdownItem> Items
        {
            get
            {
                List<DropdownItem> items = ViewState["Items"] as List<DropdownItem>;
                if (items == null)
                {
                    items = _items;
                    ViewState["Items"] = items;
                }
                return items;
            }
            set
            {
                ViewState["Items"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("btn-info")]
        public string ButtonClass
        {
            get
            {
                String s = (String)ViewState["ButtonClass"];
                return ((s == null) ? "btn-info" : s);
            }
            set
            {
                ViewState["ButtonClass"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
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

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool EnableSearch
        {
            get
            {
                object b = ViewState["EnableSearch"];
                return ((b == null) ? false : (bool)b);
            }
            set
            {
                ViewState["EnableSearch"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string SearchPlaceholder
        {
            get
            {
                String s = (String)ViewState["SearchPlaceholder"];
                return ((s == null) ? "Tìm kiếm..." : s);
            }
            set
            {
                ViewState["SearchPlaceholder"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string NoResultsText
        {
            get
            {
                String s = (String)ViewState["NoResultsText"];
                return ((s == null) ? "Không tìm thấy kết quả" : s);
            }
            set
            {
                ViewState["NoResultsText"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string DefaultSearchValue
        {
            get
            {
                String s = (String)ViewState["DefaultSearchValue"];
                return ((s == null) ? "" : s);
            }
            set
            {
                ViewState["DefaultSearchValue"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        public string SearchColumn
        {
            get
            {
                String s = (String)ViewState["SearchColumn"];
                return ((s == null) ? "" : s);
            }
            set
            {
                ViewState["SearchColumn"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
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

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        public string SearchTagItemTextFormat
        {
            get
            {
                object obj = ViewState["SearchTagItemText"];
                return (obj == null) ? "" : (string)obj;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        public string SearchTagItemKey
        {
            get
            {
                object obj = ViewState["SearchTagItemKey"];
                return (obj == null) ? this.ClientID : (string)obj;
            }
            set { ViewState["SearchTagItemKey"] = value; }
        }


        [Bindable(true)]
        [Category("Data")]
        [DefaultValue(null)]
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
        #endregion

        #region Events

        public delegate void SelectedValueChangedEventHandler(object sender, EventArgs e);

        [Category("Action")]
        public event SelectedValueChangedEventHandler SelectedValueChanged;

        protected virtual void OnSelectedValueChanged(EventArgs e)
        {
            if (SelectedValueChanged != null && !_hasRaisedEvent)
            {
                _hasRaisedEvent = true;
                SelectedValueChanged(this, e);
            }
        }

        #endregion

        #region Data Binding Methods

        public override void DataBind()
        {
            if (DataSource != null)
            {
                Items.Clear();

                IEnumerable dataSourceEnumerable = null;

                // Xử lý các loại DataSource khác nhau
                if (DataSource is IEnumerable)
                {
                    dataSourceEnumerable = (IEnumerable)DataSource;
                }
                else
                {
                    throw new ArgumentException("DataSource must implement IEnumerable interface");
                }

                foreach (object dataItem in dataSourceEnumerable)
                {
                    string textValue = string.Empty;
                    string valueValue = string.Empty;

                    if (dataItem != null)
                    {
                        if (!string.IsNullOrEmpty(DataTextField))
                        {
                            textValue = GetPropertyValue(dataItem, DataTextField);
                        }
                        else
                        {
                            textValue = dataItem.ToString();
                        }

                        if (!string.IsNullOrEmpty(DataValueField))
                        {
                            valueValue = GetPropertyValue(dataItem, DataValueField);
                        }
                        else
                        {
                            valueValue = dataItem.ToString();
                        }

                        Items.Add(new DropdownItem(textValue, valueValue));
                    }
                }
            }
        }

        private string GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return string.Empty;

            try
            {
                Type objType = obj.GetType();
                PropertyInfo propInfo = objType.GetProperty(propertyName);

                if (propInfo != null)
                {
                    object value = propInfo.GetValue(obj, null);
                    return value != null ? value.ToString() : string.Empty;
                }

                // Nếu không tìm thấy property, thử field
                FieldInfo fieldInfo = objType.GetField(propertyName);
                if (fieldInfo != null)
                {
                    object value = fieldInfo.GetValue(obj);
                    return value != null ? value.ToString() : string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Cannot find property or field '{propertyName}' in object of type '{obj.GetType().Name}'", ex);
            }

            return string.Empty;
        }

        #endregion

        #region Methods

        public void AddItem(string text, string value)
        {
            Items.Add(new DropdownItem { Text = text, Value = value });
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public void ClearSelection()
        {
            SelectedValue = string.Empty;
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            string dropdownId = ClientID + "_dropdown";
            string searchInputId = ClientID + "_search";
            string menuId = ClientID + "_menu";

            output.AddAttribute(HtmlTextWriterAttribute.Class, $"bootstrap-dropdown dropdown mt-4 mt-sm-0 w-100 d-block border-radius-0 {(Enabled ? "" : "disabled")} " + CssClass);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute(HtmlTextWriterAttribute.Href, "#");
            output.AddAttribute(HtmlTextWriterAttribute.Class, $"btn {ButtonClass} dropdown-toggle ignore w-100 d-block border-radius-0 {(Enabled ? "" : "disabled")} " + CssClass);
            output.AddAttribute("data-bs-toggle", "dropdown");
            output.AddAttribute("aria-expanded", "false");
            output.AddAttribute(HtmlTextWriterAttribute.Id, dropdownId);
            output.RenderBeginTag(HtmlTextWriterTag.A);

            string displayText = Text;
            if (!string.IsNullOrEmpty(SelectedValue))
            {
                var selectedItem = Items.Find(x => x.Value == SelectedValue);
                if (selectedItem != null)
                    displayText = selectedItem.Text;
            }
            output.Write(displayText);
            output.RenderEndTag();

            output.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown-menu");
            output.AddAttribute(HtmlTextWriterAttribute.Id, menuId);
            if (EnableSearch)
            {
                output.AddAttribute(HtmlTextWriterAttribute.Style, "min-width: 250px;");
            }
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            if (EnableSearch)
            {
                // Search container - sticky
                output.AddAttribute(HtmlTextWriterAttribute.Class, "sticky-top bg-white border-bottom p-2");
                output.RenderBeginTag(HtmlTextWriterTag.Div);

                output.AddAttribute(HtmlTextWriterAttribute.Class, "input-group input-group-sm");
                output.RenderBeginTag(HtmlTextWriterTag.Div);

                // Search input
                output.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                output.AddAttribute(HtmlTextWriterAttribute.Class, "form-control");
                output.AddAttribute(HtmlTextWriterAttribute.Id, searchInputId);
                output.AddAttribute("placeholder", SearchPlaceholder);
                output.AddAttribute("autocomplete", "off");
                output.RenderBeginTag(HtmlTextWriterTag.Input);
                output.RenderEndTag();

                // Clear search button
                output.AddAttribute(HtmlTextWriterAttribute.Class, "btn btn-outline-secondary");
                output.AddAttribute(HtmlTextWriterAttribute.Type, "button");
                output.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_clearSearch");
                output.RenderBeginTag(HtmlTextWriterTag.Button);
                output.Write("×");
                output.RenderEndTag();

                output.RenderEndTag(); 
                output.RenderEndTag(); 
            }
            // Scrollable items container
            string itemsContainerClass = "dropdown-items-container";
            if (EnableSearch)
            {
                itemsContainerClass += " " + ClientID + "_items";
            }
            output.AddAttribute(HtmlTextWriterAttribute.Class, itemsContainerClass);
            if (EnableSearch)
            {
                output.AddAttribute(HtmlTextWriterAttribute.Style, "max-height: 250px; overflow-y: auto;");
            }
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            if (AllowClear)
            {
                output.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown-item pt-1 pb-1");
                output.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                output.AddAttribute("data-value", "");
                output.AddAttribute("data-text", ClearText);
                if (AutoPostBack)
                {
                    //string postBackScript = Page.ClientScript.GetPostBackEventReference(this, "");
                    //output.AddAttribute(HtmlTextWriterAttribute.Onclick,
                    //    $"document.getElementById('{ClientID}_selectedValue').value = '{(ValueIsOfTypeGUID ? Guid.Empty.ToString() : "")}'; {postBackScript}; return false;");
                    //output.AddAttribute(HtmlTextWriterAttribute.Onclick,
                    //$"document.getElementById('{ClientID}_selectedValue').value = '{(ValueIsOfTypeGUID ? Guid.Empty.ToString() : "")}';  return false;");
                    string clearValue = ValueIsOfTypeGUID ? Guid.Empty.ToString() : "";
                    output.AddAttribute(HtmlTextWriterAttribute.Onclick,
                        $"document.getElementById('{ClientID}_selectedValue').value = '{clearValue}'; " +
                        $"setTimeout(function(){{ __doPostBack('{ClientID}', ''); }}, 10); return false;");
                }
                output.RenderBeginTag(HtmlTextWriterTag.A);
                output.Write($"<em>{ClearText}</em>");
                output.RenderEndTag();

                output.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown-divider m-0");
                output.RenderBeginTag(HtmlTextWriterTag.Hr);
                output.RenderEndTag();
            }

            // Render dropdown items
            foreach (var item in Items)
            {
                string itemClass = "dropdown-item p-2";
                if (item.Value == SelectedValue && !string.IsNullOrEmpty(SelectedValue))
                {
                    itemClass += " active";
                }

                output.AddAttribute(HtmlTextWriterAttribute.Class, itemClass);
                //output.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                output.AddAttribute("data-value", item.Value);

                if (AutoPostBack)
                {
                    //string postBackScript = Page.ClientScript.GetPostBackEventReference(this, "");
                    //output.AddAttribute(HtmlTextWriterAttribute.Onclick,
                    //    $"document.getElementById('{ClientID}_selectedValue').value = '{item.Value}'; {postBackScript}; return false;");
                    //output.AddAttribute(HtmlTextWriterAttribute.Onclick,
                    //$"document.getElementById('{ClientID}_selectedValue').value = '{item.Value}';  return false;");

                    string clearValue = ValueIsOfTypeGUID ? Guid.Empty.ToString() : "";
                    output.AddAttribute(HtmlTextWriterAttribute.Onclick,
                        $"document.getElementById('{ClientID}_selectedValue').value = '{item.Value}'; " +
                        $"setTimeout(function(){{ __doPostBack('{ClientID}', ''); }}, 10); return false;");
                }

                output.RenderBeginTag(HtmlTextWriterTag.Div);

                output.AddAttribute(HtmlTextWriterAttribute.Class, "form-check");
                output.RenderBeginTag(HtmlTextWriterTag.Div);
                // Render radio button cho item
                output.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_radioGroup_" + item.Value); 
                output.AddAttribute(HtmlTextWriterAttribute.Class, "form-check-input me-1");
                output.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                output.AddAttribute(HtmlTextWriterAttribute.Name, ClientID + "_radioGroup"); 
                output.AddAttribute(HtmlTextWriterAttribute.Value, item.Value); 
                if (SelectedValue == item.Value)
                    output.AddAttribute(HtmlTextWriterAttribute.Checked, "checked"); 
                output.RenderBeginTag(HtmlTextWriterTag.Input);
                output.RenderEndTag(); 

                // Render text của item
                output.AddAttribute(HtmlTextWriterAttribute.Class, "form-check-label");
                output.AddAttribute(HtmlTextWriterAttribute.For, ClientID + "_radioGroup_" + item.Value); 
                output.RenderBeginTag(HtmlTextWriterTag.Label);
                output.Write(item.Text);
                output.RenderEndTag(); // Close span label
                output.RenderEndTag(); // Close form-check div
                output.RenderEndTag(); // Close a div tag (dropdown-item)
            }

            // No results message
            if (EnableSearch)
            {
                output.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown-item-text text-muted text-center p-2 d-none");
                output.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_noResults");
                output.RenderBeginTag(HtmlTextWriterTag.Div);
                output.Write(NoResultsText);
                output.RenderEndTag();
            }

            output.RenderEndTag(); // Close items container
            output.RenderEndTag(); // Close dropdown-menu
            output.RenderEndTag(); // Close container div

            // Hidden field để lưu selected value
            output.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
            output.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_selectedValue");
            output.AddAttribute(HtmlTextWriterAttribute.Name, ClientID + "_selectedValue");
            output.AddAttribute(HtmlTextWriterAttribute.Value, SelectedValue);
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();

            // JavaScript for search functionality
            RegisterStartupScript();
        }
        private void RegisterStartupScript()
        {
            string scriptKey = "BootstrapDropdown_" + ClientID;

            if (!Page.ClientScript.IsStartupScriptRegistered(this.GetType(), scriptKey))
            {
                string script = GenerateJavaScript();
                Page.ClientScript.RegisterStartupScript(this.GetType(), scriptKey, script, true);
            }
        }

        private string GenerateJavaScript()
        {
            string dropdownId = ClientID + "_dropdown";
            string searchInputId = ClientID + "_search";
            string menuId = ClientID + "_menu";

            StringBuilder script = new StringBuilder();

            string functionName = $"initDropdown_{ClientID.Replace("$", "_").Replace(":", "_")}";

            script.AppendLine($"function {functionName}() {{");
            script.AppendLine($"    try {{");

            if (EnableSearch)
            {
                script.AppendLine($"        var searchInput = document.getElementById('{searchInputId}');");
                script.AppendLine($"        var clearBtn = document.getElementById('{ClientID}_clearSearch');");
                script.AppendLine($"        var dropdownItems = document.querySelectorAll('#{menuId} .{ClientID}_items .dropdown-item');");
                script.AppendLine($"        var noResults = document.getElementById('{ClientID}_noResults');");
                script.AppendLine($"        var hiddenField = document.getElementById('{ClientID}_selectedValue');");
                script.AppendLine($"        var toggleButton = document.getElementById('{dropdownId}');");
                script.AppendLine($"        var allItems = Array.from(dropdownItems);");

                // Check if elements exist before proceeding
                script.AppendLine($"        if (!searchInput || !clearBtn || !hiddenField || !toggleButton) {{");
                script.AppendLine($"            console.warn('BootstrapDropdown: Some elements not found for {ClientID}');");
                script.AppendLine($"            return;");
                script.AppendLine($"        }}");

                // Search functionality
                script.AppendLine($"        function filterItems() {{");
                script.AppendLine($"            var searchTerm = searchInput.value.toLowerCase();");
                script.AppendLine($"            var visibleCount = 0;");
                script.AppendLine($"            allItems.forEach(function(item) {{");
                script.AppendLine($"                var text = item.getAttribute('data-text') || item.textContent;");
                script.AppendLine($"                var isVisible = text.toLowerCase().indexOf(searchTerm) > -1;");
                script.AppendLine($"                item.style.display = isVisible ? 'block' : 'none';");
                script.AppendLine($"                if (isVisible) visibleCount++;");
                script.AppendLine($"            }});");
                script.AppendLine($"            if (noResults) noResults.classList.toggle('d-none', visibleCount > 0);");
                script.AppendLine($"        }}");

                // Remove existing event listeners to prevent duplicates
                script.AppendLine($"        // Remove existing listeners");
                script.AppendLine($"        searchInput.onkeydown = null;");
                script.AppendLine($"        searchInput.oninput = null;");
                script.AppendLine($"        clearBtn.onclick = null;");

                script.AppendLine($"        searchInput.addEventListener('input', filterItems);");
                script.AppendLine($"        searchInput.addEventListener('keydown', function(e) {{");
                script.AppendLine($"            e.stopPropagation();");
                script.AppendLine($"            // Prevent form submission on Enter");
                script.AppendLine($"            if (e.keyCode === 13) {{");
                script.AppendLine($"                e.preventDefault();");
                script.AppendLine($"                return false;");
                script.AppendLine($"            }}");
                script.AppendLine($"        }});");

                // Clear search
                script.AppendLine($"        clearBtn.onclick = function(e) {{");
                script.AppendLine($"            e.preventDefault();");
                script.AppendLine($"            e.stopPropagation();");
                script.AppendLine($"            searchInput.value = '';");
                script.AppendLine($"            filterItems();");
                script.AppendLine($"            searchInput.focus();");
                script.AppendLine($"            return false;");
                script.AppendLine($"        }};");

                // Item selection (only if not AutoPostBack)
                if (!AutoPostBack)
                {
                    script.AppendLine($"        allItems.forEach(function(item) {{");
                    script.AppendLine($"            item.onclick = function(e) {{");
                    script.AppendLine($"                e.preventDefault();");
                    script.AppendLine($"                e.stopPropagation();");
                    script.AppendLine($"                var value = this.getAttribute('data-value');");
                    script.AppendLine($"                var text = this.getAttribute('data-text');");
                    script.AppendLine($"                hiddenField.value = value;");
                    script.AppendLine($"                allItems.forEach(function(i) {{ i.classList.remove('active'); }});");
                    script.AppendLine($"                this.classList.add('active');");
                    script.AppendLine($"                toggleButton.innerHTML = value === '' ? '{Text}' : text;");
                    script.AppendLine($"                return false;");
                    script.AppendLine($"            }};");
                    script.AppendLine($"        }});");
                }

                // Bootstrap dropdown events 
                script.AppendLine($"        if (typeof jQuery !== 'undefined') {{");
                script.AppendLine($"            $('#{dropdownId}').off('shown.bs.dropdown.{ClientID}').on('shown.bs.dropdown.{ClientID}', function() {{");
                script.AppendLine($"                setTimeout(function() {{ if(searchInput) searchInput.focus(); }}, 100);");
                script.AppendLine($"            }});");
                script.AppendLine($"            $('#{dropdownId}').off('hidden.bs.dropdown.{ClientID}').on('hidden.bs.dropdown.{ClientID}', function() {{");
                script.AppendLine($"                searchInput.value = '';");
                script.AppendLine($"                filterItems();");
                script.AppendLine($"            }});");
                script.AppendLine($"        }} else {{");
                script.AppendLine($"            var dropdown = document.getElementById('{dropdownId}');");
                script.AppendLine($"            if (dropdown) {{");
                script.AppendLine($"                dropdown.addEventListener('shown.bs.dropdown', function() {{");
                script.AppendLine($"                    setTimeout(function() {{ if(searchInput) searchInput.focus(); }}, 100);");
                script.AppendLine($"                }});");
                script.AppendLine($"                dropdown.addEventListener('hidden.bs.dropdown', function() {{");
                script.AppendLine($"                    searchInput.value = '';");
                script.AppendLine($"                    filterItems();");
                script.AppendLine($"                }});");
                script.AppendLine($"            }}");
                script.AppendLine($"        }}");
            }
            else if (!AutoPostBack)
            {
                // Basic dropdown without search
                script.AppendLine($"        var dropdownItems = document.querySelectorAll('#{dropdownId} + .dropdown-menu .dropdown-item');");
                script.AppendLine($"        var hiddenField = document.getElementById('{ClientID}_selectedValue');");
                script.AppendLine($"        var toggleButton = document.getElementById('{dropdownId}');");
                script.AppendLine($"        ");
                script.AppendLine($"        if (!hiddenField || !toggleButton) {{");
                script.AppendLine($"            console.warn('BootstrapDropdown: Elements not found for {ClientID}');");
                script.AppendLine($"            return;");
                script.AppendLine($"        }}");
                script.AppendLine($"        ");
                script.AppendLine($"        dropdownItems.forEach(function(item) {{");
                script.AppendLine($"            item.onclick = function(e) {{");
                script.AppendLine($"                e.preventDefault();");
                script.AppendLine($"                e.stopPropagation();");
                script.AppendLine($"                var value = this.getAttribute('data-value');");
                script.AppendLine($"                var text = this.getAttribute('data-text');");
                script.AppendLine($"                hiddenField.value = value;");
                script.AppendLine($"                dropdownItems.forEach(function(i) {{ i.classList.remove('active'); }});");
                script.AppendLine($"                this.classList.add('active');");
                script.AppendLine($"                toggleButton.innerHTML = value === '' ? '{Text}' : text;");
                script.AppendLine($"                return false;");
                script.AppendLine($"            }};");
                script.AppendLine($"        }});");
            }

            script.AppendLine($"    }} catch(ex) {{");
            script.AppendLine($"        console.error('BootstrapDropdown error for {ClientID}:', ex);");
            script.AppendLine($"    }}");
            script.AppendLine($"}}");

            // Call the function with different approaches
            script.AppendLine($"// Immediate execution");
            script.AppendLine($"setTimeout({functionName}, 10);");

            // Handle both jQuery and vanilla JS ready states
            script.AppendLine($"if (typeof jQuery !== 'undefined') {{");
            script.AppendLine($"    $(document).ready(function() {{ setTimeout({functionName}, 50); }});");
            script.AppendLine($"}} else {{");
            script.AppendLine($"    if (document.readyState === 'loading') {{");
            script.AppendLine($"        document.addEventListener('DOMContentLoaded', function() {{ setTimeout({functionName}, 50); }});");
            script.AppendLine($"    }} else {{");
            script.AppendLine($"        setTimeout({functionName}, 50);");
            script.AppendLine($"    }}");
            script.AppendLine($"}}");

            // Handle partial postbacks (UpdatePanel) with namespace to avoid conflicts
            script.AppendLine($"if (typeof Sys !== 'undefined') {{");
            script.AppendLine($"    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {{");
            script.AppendLine($"        setTimeout({functionName}, 100);");
            script.AppendLine($"    }});");
            script.AppendLine($"}}");

            return script.ToString();
        }
        #endregion

        #region IPostBackDataHandler Implementation

        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            string expectedKey = ClientID + "_selectedValue";
            string postedValue = postCollection[expectedKey];

            if (postedValue != SelectedValue)
            {
                SelectedValue = postedValue ?? string.Empty;
                return true;
            }

            return false;
        }

        public void RaisePostDataChangedEvent()
        {
            if (SelectedValueChanged != null && AutoPostBack && !_hasRaisedEvent)
                SelectedValueChanged(this, new EventArgs());
        }

        #endregion

        #region IPostBackEventHandler Implementation

        public void RaisePostBackEvent(string eventArgument)
        {
            if (_hasRaisedEvent)
                return;
            string eventTarget = HttpContext.Current.Request["__EVENTTARGET"];

            if (eventTarget == UniqueID && SelectedValueChanged != null && AutoPostBack && !_hasRaisedEvent)
            {
                SelectedValueChanged(this, EventArgs.Empty);
            }
        }


        #endregion

        #region Override Methods

        protected override void OnInit(EventArgs e)
        {
            // Luôn đăng ký để nhận được postback data
            Page.RegisterRequiresPostBack(this);
            // Đảm bảo __doPostBack function được generate
            Page.ClientScript.GetPostBackEventReference(this, "");
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _hasRaisedEvent = false;
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (ScriptManager.GetCurrent(Page) != null && ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                RegisterStartupScript();
            }
        }

        #endregion
    }

    [Serializable]
    public class DropdownItem
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public DropdownItem() { }

        public DropdownItem(string text, string value)
        {
            Text = text;
            Value = value;
        }
    }
}
