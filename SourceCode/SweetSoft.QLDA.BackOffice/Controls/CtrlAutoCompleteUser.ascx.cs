using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.Controls.AutoComplete;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Web.UI;
using SweetSoft.QLDA.Core.Helpers;

namespace SweetSoft.QLDA.BackOffice.Controls
{
    public partial class CtrlAutoCompleteUser : BaseAdminUserControl
    {
        public bool Required
        {
            get
            {
                if (ViewState["Required"] != null)
                    return (bool)ViewState["Required"];
                return false;
            }
            set
            {
                ViewState["Required"] = value;
            }
        }
        public Guid? UserId
        {
            get
            {
                Guid? userId;
                this.CURRENT_PAGE.GetData(acbbUser, out userId);
                return userId;
            }
            set
            {
                acbbUser.Data = value == null ? "" : value.ToString();
            }
        }
        public bool IsEnable
        {
            get
            {
                if (ViewState["IsEnable"] != null)
                    return (bool)ViewState["IsEnable"];
                return true;
            }
            set
            {
                ViewState["IsEnable"] = value;
            }
        }
        private AutocompleteItem _item
        {
            get
            {
                if (this.UserId != null)
                {
                    AutocompleteObj item = UserManager.Instance.AllUserAutocomplete(this.UserId.ToString(), 20, "");
                    if (item != null && item.ListAutocompleteItem != null && item.ListAutocompleteItem.Count == 1)
                    {
                        ItemEmail = JsonConvert.DeserializeObject<dynamic>(item.ListAutocompleteItem[0].OtherData)["Email"];
                        ItemFullName = JsonConvert.DeserializeObject<dynamic>(item.ListAutocompleteItem[0].OtherData)["FullName"];
                        return item.ListAutocompleteItem[0];
                    }
                }
                return null;
            }
        }
        public CtrExtraAutoComplete ExtraAutoComplete
        {
            get
            {
                return acbbUser;
            }
        }
        protected string ItemEmail
        {
            get
            {
                if (ViewState["ItemEmail"] != null)
                    return (string)ViewState["ItemEmail"];
                return string.Empty;
            }
            set
            {
                ViewState["ItemEmail"] = value;
            }
        }
        protected string ItemFullName
        {
            get
            {
                if (ViewState["ItemFullName"] != null)
                    return (string)ViewState["ItemFullName"];
                return string.Empty;
            }
            set
            {
                ViewState["ItemFullName"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                acbbUser.Required = this.Required;
                acbbUser.Enabled = this.IsEnable;
                if (this.IsPostBack)
                    return;

                acbbUser.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_ACCOUNT);
                acbbUser.Item = this._item;
                acbbUser.Source = "AllUser";

                string script = $@"
                    window['{acbbUser.ClientID}ChangeUser'] = function (ev, dataItem) {{
                        if (typeof(dataItem.item) == 'undefined' ||dataItem.item === null) {{
                            $('[data-selector=""{acbbUser.ClientID}Email""]').text('');
                            $('[data-selector=""{acbbUser.ClientID}FullName""]').text('');
                        }}
                        else {{
                            var otherData = JSON.parse(dataItem.item.OtherData);
                            $('[data-selector=""{acbbUser.ClientID}Email""]').text(otherData.Email);
                            $('[data-selector=""{acbbUser.ClientID}FullName""]').text(otherData.FullName);
                        }}
                    }}
                ";
                ScriptManager.RegisterStartupScript(this.Page, GetType(), this.ClientID + "_ChangeUser", script, true);
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
        }
        public void ValidData(ref ValidationEngine validationEngine)
        {
            if (this.Required)
            {
                if (string.IsNullOrEmpty(acbbUser.Data) || !Guid.TryParse(acbbUser.Data, out Guid idTemp))
                    validationEngine.AddErrorPrompt(acbbUser.HDFValue.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
            }
        }
        public void Reset()
        {
            acbbUser.Data = string.Empty;
        }
    }
}