using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
namespace SweetSoft.QLDA.Controls
{
    public delegate void GridNeedDataSourceHandler(object sender, ExtraGridEventArg e);

    public class ExtraGridEventArg
    {
        public bool IsInsert { get; set; }
    }

    public class GridviewExtension : GridView
    {
        public event GridNeedDataSourceHandler NeedDataSource;

        private const string _currentPageIndex = "_currentPageIndex";
        private const string _currentPageSize = "_currentPageSize";
        private const string _gridInsert = "_gridInsert";
        private const string _gridSortExpression = "_gridSortExpression";
        private const string _gridSortDerection = "_gridSortDerection";
        private const string _gridVirtualCount = "_gridVirtualCount";
        private const string _gridBindingWithFiltering = "_gridBindingWithFiltering";
        private const string _gridSearchType = "_gridSearchType";

        private string cr_columnVisible = string.Empty;
        private string cr_pagingName = "paging";
        private readonly string hdfValue = "_hdfTableValue";

        protected T GetViewState<T>(string key, T defaultValue)
        {
            object obj = ViewState[key];
            return obj != null ? (T)obj : defaultValue;
        }

        protected void SetViewState<T>(string key, T value)
        {
            ViewState[key] = value;
            if (base.Initialized)
            {
                base.RequiresDataBinding = true;
            }
        }


        public GridviewPaging PagingControl
        {
            get
            {
                if (BottomPagerRow != null && BottomPagerRow.Controls.Count > 0 && !string.IsNullOrEmpty(PagingControlName))
                {
                    BottomPagerRow.Visible = true;
                    return BottomPagerRow.FindControl(PagingControlName) as GridviewPaging;
                }
                return null;
            }
        }

        public string HdfValue => hdfValue;

        public void Rebind() => LoadDataSource();

        private int GetColumnSort()
        {
            if (string.IsNullOrEmpty(CurrentSortExpression))
                return -1;
            for (int j = 0; j < Columns.Count; j++)
            {
                if (Columns[j].SortExpression?.IndexOf(CurrentSortExpression, StringComparison.OrdinalIgnoreCase) >= 0)
                    return j;
            }
            return -1;
        }

        private void LoadDataSource()
        {
            if (NeedDataSource != null)
            {
                var e = new ExtraGridEventArg { IsInsert = IsInsert };
                NeedDataSource(this, e);
                AllowSorting = true;
                if (DataSource != null)
                {
                    DataBind();
                    RenderPaging();
                }
                UpdateColumnSorting();
            }
        }

        private void UpdateColumnSorting()
        {
            if (!AllowSorting) return;
            int column = GetColumnSort();
            if (column > -1 && HeaderRow != null)
            {
                HeaderRow.Cells[column].CssClass = CurrentSortDerection == "DESC"
                    ? "sorting_desc text-center"
                    : "sorting_asc text-center";
            }
        }

        private void RenderPaging()
        {
            if (AllowPaging && PagerSettings.Position == PagerPosition.Bottom)
            {
                var pageControl = PagingControl;
                if (pageControl != null)
                {
                    pageControl.BindingWithFiltering = BindingWithFiltering;
                    pageControl.RenderButtonControl(CurrentPageIndex, CurrentPageSize, pageControl.PageCount, VirtualItemCount);
                }
            }
        }

        #region Properties
        //private readonly Guid _prefixKey = clientid;
        [Category("Custom")]
        [DefaultValue(GridSearchType.Single)]
        public GridSearchType GridSearchType
        {
            get => GetViewState(_gridSearchType, GridSearchType.Single);
            set => SetViewState(_gridSearchType, value);
        }

        [Category("Custom")]
        [DefaultValue("")]
        public string ColumnVisibleDefault
        {
            get => cr_columnVisible;
            set => cr_columnVisible = value;
        }

        [Category("Custom")]
        [Description("Set the virtual item count for this grid")]
        [DefaultValue(-1)]
        public override int VirtualItemCount
        {
            get => GetViewState(_gridVirtualCount, -1);
            set => SetViewState(_gridVirtualCount, value);
        }

        [Category("Custom")]
        [DefaultValue(false)]
        public bool IsInsert
        {
            get => GetViewState(_gridInsert, false);
            set => SetViewState(_gridInsert, value);
        }

        [Category("Custom")]
        [DefaultValue("")]
        [Browsable(false)] 
        public string CurrentSortExpression
        {
            get => GetViewState<string>(_gridSortExpression, string.Empty);
            set
            {
                var prev = GetViewState<string>(_gridSortExpression, string.Empty);
                if (!string.Equals(prev, value, StringComparison.Ordinal))
                    CurrentSortDerection = "ASC";
                else
                    CurrentSortDerection = CurrentSortDerection == "ASC" ? "DESC" : "ASC";

                SetViewState(_gridSortExpression, value);
            }
        }

        [Category("Custom")]
        [DefaultValue("DESC")]
        [Browsable(false)]
        public string CurrentSortDerection
        {
            get => GetViewState(_gridSortDerection, "DESC");
            set => SetViewState(_gridSortDerection, value);
        }

        [Category("Custom")]
        [DefaultValue(false)]
        public bool BindingWithFiltering
        {
            get => GetViewState(_gridBindingWithFiltering, false);
            set => SetViewState(_gridBindingWithFiltering, value);
        }

        [Category("Custom")]
        [DefaultValue(1)]
        public int CurrentPageIndex
        {
            get
            {
                var index = GetViewState(_currentPageIndex, 1);
                PageIndex = index;
                return index;
            }
            set
            {
                SetViewState(_currentPageIndex, value);
                PageIndex = value;
            }
        }

        [Category("Custom")]
        [DefaultValue(30)]
        public int CurrentPageSize
        {
            get
            {
                var size = GetViewState(_currentPageSize, 30);
                PageSize = size;
                return size;
            }
            set
            {
                SetViewState(_currentPageSize, value);
                PageSize = value;
            }
        }

        [Category("Custom")]
        [DefaultValue("")]
        public string PagingControlName
        {
            get => cr_pagingName;
            set => cr_pagingName = value;
        }

        [Category("Extra")]
        [DefaultValue("priority-columns")]
        public string Pattern
        {
            get => GetViewState("Pattern", "priority-columns");
            set => SetViewState("Pattern", value);
        }

        [Category("Extra")]
        [DefaultValue(true)]
        public bool IsStickyTableHeader
        {
            get => GetViewState("IsStickyTableHeader", true);
            set => SetViewState("IsStickyTableHeader", value);
        }

        [Category("Extra")]
        [DefaultValue("")]
        public string TableCustomClass
        {
            get => GetViewState("TableCustomClass", string.Empty);
            set => SetViewState("TableCustomClass", value);
        }

        [Category("Extra")]
        [DefaultValue("")]
        public string FixedNavbar
        {
            get => GetViewState("FixedNavbar", string.Empty);
            set => SetViewState("FixedNavbar", value);
        }

        [Category("Extra")]
        [DefaultValue(true)]
        public bool IsEnableDisplayAllBtn
        {
            get => GetViewState("IsEnableDisplayAllBtn", true);
            set => SetViewState("IsEnableDisplayAllBtn", value);
        }

        [Category("Extra")]
        [DefaultValue(false)]
        public bool IsEnableFocusBtn
        {
            get => GetViewState("IsEnableFocusBtn", false);
            set => SetViewState("IsEnableFocusBtn", value);
        }

        [Category("Extra")]
        [DefaultValue("")]
        public string FocusBtnIcon
        {
            get => GetViewState("FocusBtnIcon", string.Empty);
            set => SetViewState("FocusBtnIcon", value);
        }

        [Category("Extra")]
        [DefaultValue("")]
        public string VisibledColumns
        {
            get => GetViewState("VisibledColumns", string.Empty);
            set => SetViewState("VisibledColumns", value);
        }

        [Category("Extra")]
        [DefaultValue(true)]
        public bool IsFixedLastColumn
        {
            get => GetViewState("IsFixedLastColumn", true);
            set => SetViewState("IsFixedLastColumn", value);
        }

        [Category("Extra")]
        [Browsable(false)]
        public List<string> HeaderTexts
        {
            get => GetViewState("HeaderTexts", null as List<string>);
            set => SetViewState("HeaderTexts", value);
        }

        [Category("Custom")]
        [DefaultValue("")]
        public string ValueField
        {
            get => GetViewState("ValueField", string.Empty);
            set => SetViewState("ValueField", value);
        }

        [Category("Custom")]
        [DefaultValue("")]
        public string DataNameField
        {
            get => GetViewState("DataNameField", string.Empty);
            set => SetViewState("DataNameField", value);
        }

        [Category("Custom")]
        [DefaultValue("")]
        public string IsAllowDeletedField
        {
            get => GetViewState("IsAllowDeletedField", string.Empty);
            set => SetViewState("IsAllowDeletedField", value);
        }

        [Category("Custom")]
        [DefaultValue(false)]
        public bool IsEnableSelectColumn
        {
            get => GetViewState("IsEnableSelectColumn", false);
            set => SetViewState("IsEnableSelectColumn", value);
        }

        [Category("Custom")]
        [DefaultValue(null)]
        public List<DataTable> SelectedColumns
        {
            get => GetViewState("SelectedColumns", null as List<DataTable>);
            set => SetViewState("SelectedColumns", value);
        }

        [Category("Custom")]
        [DefaultValue(true)]
        public bool IsCustomTable
        {
            get => GetViewState("IsCustomTable", true);
            set => SetViewState("IsCustomTable", value);
        }

        [Category("Custom")]
        [DefaultValue(0)]
        public int AdjustHeight
        {
            get => GetViewState("AdjustHeight", 0);
            set => SetViewState("AdjustHeight", value);
        }
        #endregion

        #region Override Function

        protected override void OnSorting(GridViewSortEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.SortExpression))
            {
                CurrentSortExpression = e.SortExpression;
                Rebind();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            this.ShowHeaderWhenEmpty = true;
            // Add default first column based on IsEnableSelectColumn
            if (Columns.Count == 0 || !(Columns[0] is RowNumberField))
            {
                var rowNumberField = new RowNumberField(IsEnableSelectColumn, IsAllowDeletedField, ValueField, DataNameField)
                {
                    HeaderText = "#",
                    ItemStyle = { CssClass = "text-center", Width = Unit.Pixel(40) },
                    HeaderStyle = { CssClass = "text-center" }
                };
                Columns.Insert(0, rowNumberField);
            }

            base.OnInit(e);
            CssClass = IsFixedLastColumn
                ? $"extra-gridview table w-100 table-fixed-column {CssClass}"
                : $"extra-gridview table w-100 {CssClass}";
            ExtraScriptRegister.RegisterGrid = IsCustomTable;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!Visible) return;

            if (IsCustomTable)
            {
                writer.Write($"<div class=\"table-rep-plugin {TableCustomClass}\">");
                var tagTableResponsive = $"<div class=\"table-extra table-responsive mb-0\" data-adjust-height=\"{AdjustHeight}\" " +
                    (!string.IsNullOrEmpty(Pattern) ? $"data-pattern=\"{Pattern}\" " : "") +
                    $"data-stickyTableHeader=\"{(IsStickyTableHeader ? "true" : "false")}\" " +
                    (!string.IsNullOrEmpty(FixedNavbar) ? $"data-fixedNavbar=\"{FixedNavbar}\" " : "") +
                    $"data-addDisplayAllBtn=\"{(IsEnableDisplayAllBtn ? "true" : "false")}\" " +
                    $"data-addFocusBtn=\"{(IsEnableFocusBtn ? "true" : "false")}\" " +
                    (!string.IsNullOrEmpty(FocusBtnIcon) ? $"data-focusBtnIcon=\"{FocusBtnIcon}\" " : "") +
                    (!string.IsNullOrEmpty(VisibledColumns) ? $"data-VisibledColumns=\"{VisibledColumns}\" " : "") +
                    $"data-enableSelectColumn=\"{(IsEnableSelectColumn ? "true" : "false")}\" " +
                    (IsEnableSelectColumn ? $"data-hdfValue=\"{ClientID + hdfValue}\" " : "") +
                    ">";

                writer.Write(tagTableResponsive);
            }

            base.Render(writer); 

            if (IsEnableSelectColumn)
            {
                writer.WriteHtmlElement(new HtmlElement(HtmlTextWriterTag.Input.ToString(), "",
                    ClientID + hdfValue, null, null,
                    new[]
                    {
                new HtmlAttribute("type", "hidden", null),
                new HtmlAttribute("value", string.Empty, null),
                new HtmlAttribute("name", UniqueID + hdfValue, null)
                    }, true, null), null);
            }

            if (IsCustomTable)
                writer.Write("</div></div>");
        }


        //protected override int CreateChildControls(System.Collections.IEnumerable dataSource, bool dataBinding)
        //{
        //    int count = base.CreateChildControls(dataSource, dataBinding);

        //    if (this.Rows.Count == 0 && this.ShowHeader && HeaderTexts != null)
        //    {
        //        // ⚠ Bắt buộc tạo lại Table nếu base không tạo
        //        Table table = this.Controls.OfType<Table>().FirstOrDefault();
        //        if (table == null)
        //        {
        //            table = new Table();
        //            table.ID = this.ID + "_EmptyTable";
        //            table.GridLines = this.GridLines;
        //            table.CellPadding = this.CellPadding;
        //            table.CellSpacing = this.CellSpacing;
        //            table.CssClass = this.CssClass;

        //            this.Controls.Add(table); 
        //        }

        //        GridViewRow header = new GridViewRow(0, -1, DataControlRowType.Header, DataControlRowState.Normal);
        //        int i = 0;
        //        List<string> lstHeaderVisibled = null;
        //        bool isCheckListHeader = false;
        //        if (!string.IsNullOrEmpty(this.VisibledColumns))
        //        {
        //            lstHeaderVisibled = this.VisibledColumns.Split(',').ToList();
        //            isCheckListHeader = lstHeaderVisibled != null && lstHeaderVisibled.Count > 0;
        //        }
        //        foreach (string text in HeaderTexts)
        //        {
        //            if (!isCheckListHeader || lstHeaderVisibled.Any(item => item == i.ToString()))
        //            {
        //                TableHeaderCell cell = new TableHeaderCell();
        //                cell.Text = $"<p class='text-center mb-0'>{text}</p>";
        //                header.Cells.Add(cell);
        //            }
        //            ++i;
        //        }

        //        table.Rows.AddAt(0, header);
        //    }

        //    return count;
        //}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!this.Page.IsPostBack)
            {
                PageIndex = CurrentPageIndex;
                PageSize = CurrentPageSize;
            }
            if ((ShowHeader || ShowHeaderWhenEmpty) && Rows.Count > 0)
                HeaderRow.TableSection = TableRowSection.TableHeader;

            if (FooterRow != null && ShowFooter && Rows.Count > 0)
                FooterRow.TableSection = TableRowSection.TableFooter;

            if (HeaderRow != null)
            {
                if (ShowHeader || ShowHeaderWhenEmpty)
                    HeaderRow.TableSection = TableRowSection.TableHeader;
                if (HeaderTexts?.Count > 0 && !AutoGenerateColumns)
                {
                    if (AllowSorting)
                    {
                        for (int j = 0; j < HeaderTexts.Count; j++)
                        {
                            var itemText = HeaderTexts[j];
                            Literal literal;
                            if (IsEnableSelectColumn && j == 0)
                            {
                                literal = new Literal
                                {
                                    Text = @"<div class='checkbox-wrapper-46'>
                <input class='inp-cbx inp-cbx-all' id='cbx-all' type='checkbox'>
                <label class='cbx' for='cbx-all'>
                    <span>
                        <svg width='12px' height='10px' viewBox='0 0 12 10'>
                            <polyline points='1.5 6 4.5 9 10.5 1'></polyline>
                        </svg>
                    </span>
                </label>
            </div>"
                                };
                            }
                            else
                            {
                                if (j >= Columns.Count || j >= HeaderRow.Cells.Count)
                                    continue;

                                var sortExpression = Columns[j].SortExpression;
                                literal = new Literal
                                {
                                    Text = string.IsNullOrEmpty(sortExpression)
                                        ? $"<a href=\"javascript:;\">{itemText}</a>"
                                        : $"<a href=\"javascript:__doPostBack('{UniqueID}','Sort${sortExpression}')\">{itemText}</a>"
                                };
                            }

                            if (j < HeaderRow.Cells.Count)
                            {
                                HeaderRow.Cells[j].Controls.Clear();
                                HeaderRow.Cells[j].Controls.Add(literal);
                            }
                        }
                    }
                    else
                    {
                        for(int i =0; i < HeaderTexts.Count; i++)
                        {
                            if (IsEnableSelectColumn && i == 0)
                            {
                                HeaderRow.Cells[i].Text = @"<div class='checkbox-wrapper-46'>
                                    <input class='inp-cbx inp-cbx-all' id='cbx-all' type='checkbox'>
                                    <label class='cbx' for='cbx-all'>
                                        <span>
                                            <svg width='12px' height='10px' viewBox='0 0 12 10'>
                                                <polyline points='1.5 6 4.5 9 10.5 1'></polyline>
                                            </svg>
                                        </span>
                                    </label>
                                </div>";
                            }
                            else
                            {
                                HeaderRow.Cells[i].Text = HeaderTexts[i];
                            }
                        }
                    }
                }
                
            }
        }

        public void HandleGetSelectedColumns(string postDataKey, NameValueCollection postCollection)
        {
            var postedValue = postCollection[postDataKey + hdfValue];
            if (postedValue != null && IsEnableSelectColumn)
                SelectedColumns = ConvertDataToListObject<DataTable>(postedValue);
        }
        private static List<T> ConvertDataToListObject<T>(string strData)
        {
            try
            {
                var jss = new JavaScriptSerializer();
                return jss.Deserialize<List<T>>(strData);
            }
            catch
            {
                return null;
            }
        }
        [Serializable]
        public class DataTable
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public const string TEMPLATE_WRAPPER_DELETE_MULTIPLE = @"<label>{0}</label><div class='listSearchTagBox'>{1}</div>";
        public const string TEMPLATE_DELETE_ITEM = @"<div class='searchTag flex-between w-100 px-2 py-1 mb-2'>
            <div class='searchTagItem float-start'>{0}: <b>{1}</b></div>
            <a class='searchTagAction float-end clear-item-delete' href='javascript:void(0)' data-id='{2}'
                onclick='ExtraGridviewJs.removeSelectedItem(this);'>
                <i class='bx bx-x'></i>
            </a>
        </div>";
        #endregion
    }

    // Custom field for row numbers
    public class RowNumberField : TemplateField
    {
        public bool IsEnableSelectColumn { get; set; }
        public string ValueField { get; set; }
        public string DataNameField { get; set; }
        public string IsAllowDeletedField { get; set; }
        public RowNumberField(bool enableSelect, string isAllowDeletedField, string valueField, string dataNameField)
        {
            IsEnableSelectColumn = enableSelect;
            ValueField = valueField;
            DataNameField = dataNameField;
            IsAllowDeletedField = isAllowDeletedField;
            this.ItemTemplate = new RowNumberTemplate(IsEnableSelectColumn, IsAllowDeletedField, ValueField, DataNameField);
        }

        private class RowNumberTemplate : ITemplate
        {
            private readonly bool _enableSelect;
            private readonly string _valueField;
            private readonly string _dataNameField;
            private readonly string _isAllowDeletedField;
            public RowNumberTemplate(bool enableSelect, string isAllowDeletedField, string valueField, string dataNameField)
            {
                _enableSelect = enableSelect;
                _valueField = valueField;
                _dataNameField = dataNameField;
                _isAllowDeletedField = isAllowDeletedField;
            }

            public void InstantiateIn(Control container)
            {
                container.DataBinding += (sender, e) =>
                {
                    var cell = (DataControlFieldCell)sender;
                    var gridViewRow = (GridViewRow)cell.NamingContainer;
                    var grid = (GridView)gridViewRow.NamingContainer;
                    var dataItem = gridViewRow.DataItem;

                    if (_enableSelect 
                    && !string.IsNullOrEmpty(_valueField)
                    && !string.IsNullOrEmpty(_dataNameField))
                    {
                        string value = DataBinder.Eval(dataItem, _valueField)?.ToString();
                        string name = DataBinder.Eval(dataItem, _dataNameField)?.ToString();
                        bool isAllowDeleted = true;
                        if(!string.IsNullOrEmpty(_isAllowDeletedField))
                        {
                            try
                            {
                                isAllowDeleted = Convert.ToBoolean(DataBinder.Eval(dataItem, _isAllowDeletedField));
                            }
                            catch
                            {
                                isAllowDeleted=true; // fallback to true if conversion fails
                            }
                        }
                        if(!isAllowDeleted)
                        {
                            cell.Text = "";
                            return;
                        }
                        cell.Text = $@"
<div class='checkbox-wrapper-46'>
    <input class='inp-cbx' id='cbx-{value}' type='checkbox' value='{value}' data-name='{HttpUtility.HtmlEncode(name)}' />
    <label class='cbx' for='cbx-{value}'>
        <span>
            <svg width='12px' height='10px' viewBox='0 0 12 10'>
                <polyline points='1.5 6 4.5 9 10.5 1'></polyline>
            </svg>
        </span>
    </label>
</div>";
                    }
                    else
                    {
                        int pageIndex = 0;
                        int pageSize = 10; // fallback

                        if (grid is GridviewExtension customGrid)
                        {
                            pageIndex = customGrid.CurrentPageIndex;
                            pageSize = customGrid.CurrentPageSize;
                        }
                        else
                        {
                            pageIndex = grid.PageIndex;
                            pageSize = grid.PageSize;
                        }

                        int rowIndex = gridViewRow.RowIndex + 1 + ((pageIndex - 1) * pageSize);
                        cell.Text = rowIndex.ToString();
                    }
                };
            }
        }
    }

    public class SelectCheckboxField : TemplateField
    {
        public SelectCheckboxField()
        {
            this.ItemTemplate = new SelectCheckboxTemplate();
        }

        private class SelectCheckboxTemplate : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                container.DataBinding += (sender, e) =>
                {
                    var cell = (DataControlFieldCell)sender;
                    var row = (GridViewRow)cell.NamingContainer;
                    var dataItem = row.DataItem;

                    if (dataItem != null)
                    {
                        string id = DataBinder.Eval(dataItem, "Id")?.ToString();
                        string name = DataBinder.Eval(dataItem, "WorkspaceName")?.ToString();

                        string html = $@"
<div class='checkbox-wrapper-46'>
    <input class='inp-cbx' id='cbx-{id}' type='checkbox' value='{id}' data-name='{HttpUtility.HtmlAttributeEncode(name)}' />
    <label class='cbx' for='cbx-{id}'>
        <span>
            <svg width='12px' height='10px' viewBox='0 0 12 10'>
                <polyline points='1.5 6 4.5 9 10.5 1'></polyline>
            </svg>
        </span>
    </label>
</div>";
                        cell.Text = html;
                    }
                };
            }
        }
    }

}