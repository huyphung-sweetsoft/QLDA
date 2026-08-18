using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.Controls
{
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class ExtraSearchBox : WebControl, INamingContainer
    {
        public event TagCloseEvent TagClosed;
        private ITemplate popupTemplate;

        #region Properties
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate PopupTemplate
        {
            get { return popupTemplate; }
            set { popupTemplate = value; }
        }

        private List<SearchTagItem> cr_items = new List<SearchTagItem>();
        public List<SearchTagItem> TagItems
        {
            get
            {

                if (this.ViewState["TagSearch"] == null) this.ViewState["TagSearch"] = new List<SearchTagItem>();
                return this.ViewState["TagSearch"] as List<SearchTagItem>;
            }
            set
            {
                this.ViewState["TagSearch"] = value;
                onLoad = false;
                //CreateChildControls();
            }
        }
        public GridSearchType SearchType
        {
            get
            {
                if (this.ViewState["SearchType"] == null)
                    return GridSearchType.Multiple;
                return (GridSearchType)this.ViewState["SearchType"];
            }
            set
            {
                this.ViewState["SearchType"] = value;
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        private bool onLoad = false;
        protected override void CreateChildControls()
        {
            if (!onLoad)
            {
                Controls.Clear();
                if (TagItems.Count > 0)
                {
                    HtmlGenericControl divRow = new HtmlGenericControl("div");
                    divRow.Attributes.Add("class", "tagbox");
                    for (int i = 0; i < TagItems.Count; i++)
                    {
                        SearchTagItem item = TagItems[i];
                        if (item.Status /*&& item.SearchType == SearchType*/)
                        {
                            HtmlGenericControl div = new HtmlGenericControl("div");
                            div.Attributes.Add("class", "search-tag input-group input-group-sm");

                            HtmlGenericControl span = new HtmlGenericControl("span");
                            span.Attributes.Add("class", "form-control");
                            span.InnerHtml = item.Text;
                            div.Controls.Add(span);


                            LinkButton btn = new LinkButton();
                            HtmlGenericControl fai = new HtmlGenericControl("i");
                            fai.Attributes.Add("class", ExtraIcon.A_Close.ToRender());
                            btn.Controls.Add(fai);
                            btn.CssClass = "input-group-addon search-criteria";
                            btn.CommandArgument = i.ToString();

                            if (TagClosed != null)
                                btn.Click += btn_Click;
                            div.Controls.Add(btn);

                            divRow.Controls.Add(div);
                        }
                    }
                    HtmlGenericControl divClearfix = new HtmlGenericControl("div");
                    divClearfix.Attributes.Add("class", "clearfix");
                    divRow.Controls.Add(divClearfix);
                    this.Controls.Add(divRow);
                }

            }
            onLoad = true;
        }

        public void Update()
        {
            Update(GridSearchType.Multiple);
        }
        public void Update(GridSearchType searchType)
        {
            SearchType = searchType;
            onLoad = false;
            CreateChildControls();
        }
        void btn_Click(object sender, EventArgs e)
        {
            LinkButton btn = sender as LinkButton;
            if (btn != null)
            {
                SearchTagItem tag = TagItems[int.Parse(btn.CommandArgument)];
                if (tag != null && TagClosed != null)
                {
                    TagClosed(this, tag);
                }
            }
        }
    }
    public delegate void TagCloseEvent(object sender, SearchTagItem tag);

    [Serializable]
    public class SearchTagItem
    {
        public GridSearchType SearchType { get; set; }
        public string Id { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public object Tag { get; set; }
        private bool cr_status = true;
        public bool Status { get { return cr_status; } set { cr_status = value; } }
        private bool cr_isNew = true;
        public bool IsNew { get { return cr_isNew; } set { cr_isNew = value; } }

        public SearchTagItem(string key, string text) { this.Key = key; this.Text = text; Tag = null; SearchType = GridSearchType.Multiple; }
        public SearchTagItem(string key, string text, string value, object tag) { this.Key = key; this.Text = text; Value = value; Tag = tag; SearchType = GridSearchType.Multiple; }
        public SearchTagItem(string key, string text, string value) { this.Key = key; this.Text = text; Value = value; SearchType = GridSearchType.Multiple; }
        public SearchTagItem(string key, string text, string value, GridSearchType searchType) { this.Key = key; this.Text = text; Value = value; SearchType = SearchType; }
        public SearchTagItem(string key, string text, string value, string id) { this.Key = key; this.Text = text; Value = value; this.Id = id; SearchType = GridSearchType.Multiple; }
    }
}
