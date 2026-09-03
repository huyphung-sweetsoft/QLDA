//-----------------------PROGRAMER LOGS---------------------------
using SubSonic;
using SweetSoft.QLDA.BackOffice.Controls;
using SweetSoft.QLDA.BackOffice.Controls.AutoComplete;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public class ControlHelpers
    {
        //'**Change 01: start
        private string _languageCode { get; set; }
        private static readonly string _vnCountryId = AppSettingHelpers.GetSetting<string>("VNCountryId");
        //'**Change 02
        private byte _languageId
        {
            get
            {
                return LanguageHelpers.GetLanguageCodeByCultureName(this._languageCode);
            }
        }

        public ControlHelpers(string languageCode)
        {
            this._languageCode = languageCode;
        }
        public ControlHelpers()
        {
            this._languageCode = LanguageHelpers.GetLanguageCode(LanguageHelpers.Defaultlanguage);
        }
        //'**Change 01: end
        #region Core

        #endregion
        #region Get controls values
        public void ClearControlValues(ControlCollection controls)
        {
            if (controls == null || controls.Count == 0)
                return;
            foreach (System.Web.UI.Control control in controls)
            {
                #region Extra Controls
                // Check if the control is of ExtraTextBox type
                if (control is ExtraTextBox)
                {
                    var textBox = (ExtraTextBox)control;
                    if (textBox != null)
                    {
                        if (textBox.IsNumber)
                            textBox.Text = "0";
                        else
                            textBox.Text = string.Empty;
                    }
                }
                // Check if the control is of ExtraComboBox type
                else if (control is ExtraDropdown)
                {
                    var dropDownList = (ExtraDropdown)control;
                    if (dropDownList != null)
                    {
                        if (!string.IsNullOrEmpty(dropDownList.DefaultSearchValue) && dropDownList.DefaultSearchValue != "null")
                            dropDownList.SelectedValue = dropDownList.DefaultSearchValue;
                        else
                            dropDownList.SelectedIndex = -1;
                    }
                }
                else if (control is BootstrapDropdown)
                {
                    var bootstrapDropdown = (BootstrapDropdown)control;
                    if (bootstrapDropdown != null)
                    {
                        bootstrapDropdown.SelectedIndex = -1;
                        bootstrapDropdown.Text = string.Empty;
                        bootstrapDropdown.ClearSelection();
                    }
                }
                else if (control is ExtraDateTime)
                {
                    var dateTime = (ExtraDateTime)control;
                    if (dateTime != null)
                        dateTime.ClearDate();
                }
                else if (control is HiddenField)
                {
                    var hiddenField = (HiddenField)control;
                    if (hiddenField != null)
                        hiddenField.Value = string.Empty;
                }
                else if (control is TextBox)
                {
                    var textBox = (TextBox)control;
                    if (textBox != null)
                        textBox.Text = string.Empty;
                }
                else if (control is HtmlInputText)
                {
                    var htmlInput = (HtmlInputText)control;
                    if (htmlInput != null)
                        htmlInput.Value = string.Empty;
                }
                else if (control is HtmlInputHidden)
                {
                    var htmlInput = (HtmlInputHidden)control;
                    if (htmlInput != null)
                        htmlInput.Value = string.Empty;
                }
                #endregion
                // If the control has child controls, recurse through them
                if (control.HasControls())
                {
                    ClearControlValues(control.Controls);
                }
            }
        }
        public Dictionary<string, object> GetControlValues(Panel panel)
        {
            var controlValues = new Dictionary<string, object>();
            RecursivelyFindControls(panel.Controls, controlValues);
            return controlValues;
        }
        private void RecursivelyFindControls(ControlCollection controls, Dictionary<string, object> controlValues)
        {
            foreach (System.Web.UI.Control control in controls)
            {
                #region Extra Controls
                // Check if the control is of ExtraTextBox type
                if (control is ExtraTextBox)
                {
                    var textBox = (ExtraTextBox)control;
                    controlValues.Add(textBox.SearchColumn, InlineQueryHelpers.SQLEncode(textBox.Text));
                }
                // Check if the control is of ExtraComboBox type
                else if (control is ExtraDropdown)
                {
                    var dropDownList = (ExtraDropdown)control;
                    switch (dropDownList.SearchColumn)
                    {
                        case "IsActive":
                        case "IsActivated":
                        case "IsDeleted":
                            if (string.IsNullOrEmpty(dropDownList.SelectedValue) && !string.IsNullOrEmpty(dropDownList.DefaultSearchValue))
                                controlValues.Add(dropDownList.SearchColumn, dropDownList.DefaultSearchValue);
                            else
                                controlValues.Add(dropDownList.SearchColumn, InlineQueryHelpers.SQLEncode(dropDownList.SelectedValue));
                            break;
                        default:
                            if (string.IsNullOrEmpty(dropDownList.SelectedValue) && !string.IsNullOrEmpty(dropDownList.DefaultSearchValue))
                                controlValues.Add(dropDownList.SearchColumn, dropDownList.DefaultSearchValue);
                            else
                            {
                                //'**Change 01: start
                                //controlValues.Add(dropDownList.SearchColumn, InlineQueryHelpers.SQLEncode(dropDownList.SelectedValue));
                                if (!dropDownList.Multiple)
                                {
                                    if (dropDownList.ValueIsOfTypeGUID && string.IsNullOrEmpty(dropDownList.SelectedValue))
                                        controlValues.Add(dropDownList.SearchColumn, Guid.Empty);
                                    else
                                        controlValues.Add(dropDownList.SearchColumn, InlineQueryHelpers.SQLEncode(dropDownList.SelectedValue));
                                }
                                else
                                {
                                    string castSelectedValues;
                                    if (dropDownList.SelectedValues == null)
                                    {
                                        if (dropDownList.ValueIsOfTypeGUID)
                                            castSelectedValues = Guid.Empty.ToString();
                                        else
                                            castSelectedValues = "";
                                    }
                                    else
                                        castSelectedValues = string.Join(",", dropDownList.SelectedValues);
                                    controlValues.Add(dropDownList.SearchColumn, InlineQueryHelpers.SQLEncode(castSelectedValues));
                                }
                                //'**Change 01: end
                            }
                            break;
                    }

                }
                else if (control is BootstrapDropdown)
                {
                    var bootstrapDropdown = (BootstrapDropdown)control;
                    if (bootstrapDropdown != null)
                    {
                        if (bootstrapDropdown.ValueIsOfTypeGUID && string.IsNullOrEmpty(bootstrapDropdown.SelectedValue))
                            controlValues.Add(bootstrapDropdown.SearchColumn, Guid.Empty);
                        else if (string.IsNullOrEmpty(bootstrapDropdown.SelectedValue) && !string.IsNullOrEmpty(bootstrapDropdown.DefaultSearchValue))
                            controlValues.Add(bootstrapDropdown.SearchColumn, bootstrapDropdown.DefaultSearchValue);
                        else
                            controlValues.Add(bootstrapDropdown.SearchColumn, InlineQueryHelpers.SQLEncode(bootstrapDropdown.SelectedValue));
                    }

                }
                else if (control is ExtraDateTime)
                {
                    var dateTime = (ExtraDateTime)control;
                    if (dateTime.DateValue == null
                        || dateTime.DateValue == DateTime.MinValue
                        || dateTime.DateValue == DateTimeHelper.MinValueSQL)
                    {
                        if (!dateTime.SingleDatePicker || dateTime.IsPredefinedDateRanges)
                        {
                            controlValues.Add(dateTime.SearchColumn + "From", "");
                            controlValues.Add(dateTime.SearchColumn + "To", "");
                        }
                        else
                            controlValues.Add(dateTime.SearchColumn, "");
                        continue;
                    }
                    if (!dateTime.SingleDatePicker || dateTime.IsPredefinedDateRanges)
                    {
                        controlValues.Add(dateTime.SearchColumn + "From", InlineQueryHelpers.SQLStartDate(dateTime.StartValue));
                        controlValues.Add(dateTime.SearchColumn + "To", InlineQueryHelpers.SQLEndDate(dateTime.EndValue));
                    }
                    else
                    {
                        controlValues.Add(dateTime.SearchColumn, InlineQueryHelpers.SQLStartDate(dateTime.DateValue));
                    }
                }
                #endregion
                // If the control has child controls, recurse through them
                if (control.HasControls())
                {
                    RecursivelyFindControls(control.Controls, controlValues);
                }
            }
        }
        #endregion
        public void DisabledControls(ControlCollection controls)
        {
            if (controls == null || controls.Count == 0)
                return;
            foreach (System.Web.UI.Control control in controls)
            {
                #region Extra Controls
                // Check if the control is of ExtraTextBox type
                if (control is ExtraTextBox)
                {
                    var textBox = (ExtraTextBox)control;
                    if (textBox != null)
                    {
                        textBox.Enabled = false;
                        textBox.ReadOnly = true;
                    }
                }
                // Check if the control is of ExtraComboBox type
                else if (control is ExtraDropdown)
                {
                    var dropDownList = (ExtraDropdown)control;
                    if (dropDownList != null)
                        dropDownList.Enabled = false;
                }
                else if (control is BootstrapDropdown)
                {
                    var bootstrapDropdown = (BootstrapDropdown)control;
                    if (bootstrapDropdown != null)
                        bootstrapDropdown.Enabled = false;
                }
                else if (control is ExtraDateTime)
                {
                    var dateTime = (ExtraDateTime)control;
                    if (dateTime != null)
                        dateTime.Enabled = false;
                }
                else if (control is ExtraCheckbox)
                {
                    var checkbox = (ExtraCheckbox)control;
                    if (checkbox != null)
                        checkbox.Enabled = false;
                }
                else if (control is ExtraButton)
                {
                    var button = (ExtraButton)control;
                    if (button != null && !button.IsExcludeLock)
                        button.Enabled = false;
                }
                else if (control is CtrlAutoCompleteUser)
                {
                    var autoCompleteUser = (CtrlAutoCompleteUser)control;
                    if (autoCompleteUser != null)
                        autoCompleteUser.IsEnable = false;
                }
                else if (control is CtrExtraAutoComplete)
                {
                    var autoComplete = (CtrExtraAutoComplete)control;
                    if (autoComplete != null)
                        autoComplete.Enabled = false;
                }
                else if (control is LinkButton)
                {
                    var button = (LinkButton)control;
                    if (button != null)
                        button.Enabled = false;
                }
                else if (control is HtmlInputRadioButton)
                {
                    var htmlInputRadio = (HtmlInputRadioButton)control;
                    if (htmlInputRadio != null)
                        htmlInputRadio.Attributes["disabled"] = "disabled";
                }
                else if (control is HtmlInputCheckBox)
                {
                    var htmlInputCheckBox = (HtmlInputCheckBox)control;
                    if (htmlInputCheckBox != null)
                        htmlInputCheckBox.Attributes["disabled"] = "disabled";
                }
                else if (control is HtmlInputText)
                {
                    var htmlInputText = (HtmlInputText)control;
                    if (htmlInputText != null)
                        htmlInputText.Attributes["disabled"] = "disabled";
                }
                else if (control is HtmlTextArea)
                {
                    var htmlTextArea = (HtmlTextArea)control;
                    if (htmlTextArea != null)
                    {
                        htmlTextArea.Attributes["disabled"] = "disabled";
                        htmlTextArea.Attributes["mg-disabled"] = "disabled";
                    }
                }
                else if (control is TextBox)
                {
                    var htmlTextBox = (TextBox)control;
                    if (htmlTextBox != null)
                        htmlTextBox.Attributes["disabled"] = "disabled";
                }
                #endregion
                // If the control has child controls, recurse through them
                if (control.HasControls())
                {
                    DisabledControls(control.Controls);
                }
            }
        }
        public void EnableControls(ControlCollection controls)
        {
            if (controls == null || controls.Count == 0)
                return;
            foreach (System.Web.UI.Control control in controls)
            {
                #region Extra Controls
                // Check if the control is of ExtraTextBox type
                if (control is ExtraTextBox)
                {
                    var textBox = (ExtraTextBox)control;
                    if (textBox != null)
                    {
                        textBox.Enabled = true;
                        textBox.ReadOnly = false;
                    }
                }
                // Check if the control is of ExtraComboBox type
                else if (control is ExtraDropdown)
                {
                    var dropDownList = (ExtraDropdown)control;
                    if (dropDownList != null)
                        dropDownList.Enabled = true;
                }
                else if (control is BootstrapDropdown)
                {
                    var bootstrapDropdown = (BootstrapDropdown)control;
                    if (bootstrapDropdown != null)
                        bootstrapDropdown.Enabled = true;
                }
                else if (control is ExtraDateTime)
                {
                    var dateTime = (ExtraDateTime)control;
                    if (dateTime != null)
                        dateTime.Enabled = true;
                }
                else if (control is ExtraCheckbox)
                {
                    var checkbox = (ExtraCheckbox)control;
                    if (checkbox != null)
                        checkbox.Enabled = true;
                }
                else if (control is ExtraButton)
                {
                    var button = (ExtraButton)control;
                    if (button != null && !button.IsExcludeLock)
                        button.Enabled = true;
                }
                //else if (control is CtrlAutoCompleteCustomer)
                //{
                //    var autoCompleteCustomer = (CtrlAutoCompleteCustomer)control;
                //    if (autoCompleteCustomer != null)
                //        autoCompleteCustomer.IsEnable = true;
                //}
                else if (control is CtrlAutoCompleteUser)
                {
                    var autoCompleteUser = (CtrlAutoCompleteUser)control;
                    if (autoCompleteUser != null)
                        autoCompleteUser.IsEnable = true;
                }
                else if (control is CtrExtraAutoComplete)
                {
                    var autoComplete = (CtrExtraAutoComplete)control;
                    if (autoComplete != null)
                        autoComplete.Enabled = true;
                }
                else if (control is LinkButton)
                {
                    var button = (LinkButton)control;
                    if (button != null)
                        button.Enabled = true;
                }
                else if (control is HtmlInputRadioButton)
                {
                    var htmlInputRadio = (HtmlInputRadioButton)control;
                    if (htmlInputRadio != null)
                        htmlInputRadio.Attributes["disabled"] = "";
                }
                else if (control is HtmlInputCheckBox)
                {
                    var htmlInputCheckBox = (HtmlInputCheckBox)control;
                    if (htmlInputCheckBox != null)
                        htmlInputCheckBox.Attributes["disabled"] = "";
                }
                else if (control is HtmlInputText)
                {
                    var htmlInputText = (HtmlInputText)control;
                    if (htmlInputText != null)
                        htmlInputText.Attributes["disabled"] = "";
                }
                else if (control is HtmlTextArea)
                {
                    var htmlTextArea = (HtmlTextArea)control;
                    if (htmlTextArea != null)
                    {
                        htmlTextArea.Attributes["disabled"] = "";
                        htmlTextArea.Attributes["mg-disabled"] = "";
                    }
                }
                else if (control is TextBox)
                {
                    var htmlTextBox = (TextBox)control;
                    if (htmlTextBox != null)
                        htmlTextBox.Attributes["disabled"] = "";
                }
                #endregion
                // If the control has child controls, recurse through them
                if (control.HasControls())
                {
                    EnableControls(control.Controls);
                }
            }
        }
        #region Binding controls
        public void BindYears(ExtraDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            for (int i = 2025; i <= DateTime.UtcNow.Year; i++)
                dropdown.Items.Add(new ListItem(i.ToString(), i.ToString()));
            dropdown.SelectedValue = DateTime.UtcNow.Year.ToString();
        }
        public void BindStatus(ExtraDropdown dropdown, bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.PlaceHolder = string.Empty;
                dropdown.EmptyItemText = UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = "";
            }
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ACTIVE), "1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INACTIVE), "0"));
            dropdown.SelectedIndex = -1;
        }
        public void BindStatus(BootstrapDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.AddItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ACTIVE), "1");
            dropdown.AddItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.INACTIVE), "0");
            dropdown.SelectedIndex = -1;
        }
        public void BindRoles(ExtraDropdown ddl)
        {
            ddl.Items.Clear();
            List<AspnetRole> aspnetRoles = RoleManager.Instance.GetAllRoles();
            if (aspnetRoles == null)
                aspnetRoles = new List<AspnetRole>();
            ddl.DataTextField = AspnetRole.Columns.RoleName;
            ddl.DataValueField = AspnetRole.Columns.RoleId;
            ddl.DataSource = aspnetRoles;
            ddl.DataBind();
        }
        public void BindRoles(BootstrapDropdown ddl)
        {
            ddl.Items.Clear();
            List<AspnetRole> aspnetRoles = RoleManager.Instance.GetAllRoles();
            if (aspnetRoles == null)
                aspnetRoles = new List<AspnetRole>();
            ddl.DataTextField = AspnetRole.Columns.RoleName;
            ddl.DataValueField = AspnetRole.Columns.RoleId;
            ddl.DataSource = aspnetRoles;
            ddl.DataBind();
        }
        public void BindStatusOnOff(ExtraDropdown dropdown, bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.AlowClear = isAll;
            if (isAll)
            {
                dropdown.EmptyItemText = UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = string.Empty;
            }
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ON), "1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.OFF), "0"));
            dropdown.SelectedIndex = -1;
        }
        public void BindStatusYesNo(ExtraDropdown dropdown, bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.AlowClear = isAll;
            if (isAll)
            {
                dropdown.EmptyItemText = UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = string.Empty;
            }
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.YES), "1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.NO), "0"));
            dropdown.SelectedIndex = -1;
        }
        public void BindReadStatus(ExtraDropdown dropdown, bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.AlowClear = isAll;
            if (isAll)
                dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL), "-1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.READ), "1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.UNREAD), "0"));
            dropdown.SelectedIndex = -1;
        }
        public void BindSendingStatus(ExtraDropdown dropdown, bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.AlowClear = isAll;
            if (isAll)
                dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL), "-1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.SENT), "1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.UNSENT), "0"));
            dropdown.SelectedIndex = -1;
        }
        public void BindUsers(ExtraDropdown dropdown, bool isAll = false, string dataValueField = "UserName")
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "";

            List<AspnetUser> aspnetUsers = UserManager.Instance.GetAllAspnetUsers();
            if (aspnetUsers == null)
                aspnetUsers = new List<AspnetUser>();
            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.PlaceHolder = string.Empty;
                dropdown.EmptyItemText = UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = "";
            }
            dropdown.DataSource = aspnetUsers;
            dropdown.DataValueField = dataValueField;
            dropdown.DataTextField = "DisplayName";
            dropdown.DataBind();
            dropdown.SelectedIndex = -1;
        }
        public void BindUsers(BootstrapDropdown dropdown, string dataValueField = "UserName")
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "";
            List<AspnetUser> aspnetUsers = UserManager.Instance.GetAllAspnetUsers();
            if (aspnetUsers == null)
                aspnetUsers = new List<AspnetUser>();
            dropdown.DataSource = aspnetUsers;
            dropdown.DataValueField = dataValueField;
            dropdown.DataTextField = "DisplayName";
            dropdown.DataBind();
            dropdown.SelectedIndex = -1;
        }
        public void BindDocumentGroups(ExtraDropdown dropdown, bool isAll = false)
        {
            dropdown.Items.Clear();

            List<TblNhomTaiLieu> groups =
                DocumentGroupManager.Instance.GetAll()
                ?? new List<TblNhomTaiLieu>();

            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.DefaultSearchValue = string.Empty;
                dropdown.Items.Add(
                    new ListItem(
                        "Tất cả nhóm tài liệu",
                        string.Empty));
            }
            else
            {
                dropdown.Items.Add(
                    new ListItem(
                        "Chọn nhóm tài liệu",
                        string.Empty));
            }

            foreach (TblNhomTaiLieu group in groups)
            {
                string text = group.TenNhom;

                if (!group.KichHoat)
                    text += " (Đã khóa)";

                dropdown.Items.Add(
                    new ListItem(
                        text,
                        group.IdNhomTaiLieu.ToString()));
            }

            dropdown.SelectedIndex = -1;
        }
        public void BindDocumentGroups(BootstrapDropdown dropdown)
        {
            dropdown.Items.Clear();

            List<TblNhomTaiLieu> groups =
                DocumentGroupManager.Instance.GetAll()
                ?? new List<TblNhomTaiLieu>();

            foreach (TblNhomTaiLieu group in groups)
            {
                string text = group.TenNhom;

                if (!group.KichHoat)
                    text += " (Đã khóa)";

                dropdown.AddItem(
                    text,
                    group.IdNhomTaiLieu.ToString());
            }

            dropdown.ClearSelection();
        }

        public void BindDocumentTypes(ExtraDropdown dropdown, bool isAll = false)
        {
            dropdown.Items.Clear();

            List<TblLoaiTaiLieu> documentTypes =
                DocumentTypeManager.Instance.GetAll()
                ?? new List<TblLoaiTaiLieu>();

            Dictionary<Guid, string> groupNames =
                (DocumentGroupManager.Instance.GetAll()
                    ?? new List<TblNhomTaiLieu>())
                .ToDictionary(
                    group => group.IdNhomTaiLieu,
                    group => group.TenNhom);

            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.DefaultSearchValue = string.Empty;
                dropdown.Items.Add(new ListItem(
                    "Tất cả loại tài liệu",
                    string.Empty));
            }
            else
            {
                dropdown.Items.Add(new ListItem(
                    "Chọn loại tài liệu",
                    string.Empty));
            }

            foreach (TblLoaiTaiLieu documentType in documentTypes)
            {
                string groupName;
                groupNames.TryGetValue(
                    documentType.IdNhomTaiLieu,
                    out groupName);

                string text = string.IsNullOrEmpty(groupName)
                    ? documentType.TenLoai
                    : groupName + " / " + documentType.TenLoai;

                if (!documentType.KichHoat)
                    text += " (Đã khóa)";

                dropdown.Items.Add(new ListItem(
                    text,
                    documentType.IdLoaiTaiLieu.ToString()));
            }

            dropdown.SelectedIndex = -1;
        }

        public void BindDocumentTypes(BootstrapDropdown dropdown)
        {
            BindDocumentTypes(dropdown, null);
        }

        public void BindDocumentTypes(
            BootstrapDropdown dropdown,
            Guid? idNhomTaiLieu)
        {
            dropdown.Items.Clear();

            List<TblLoaiTaiLieu> documentTypes =
                DocumentTypeManager.Instance.GetAll(
                    null,
                    idNhomTaiLieu)
                ?? new List<TblLoaiTaiLieu>();

            Dictionary<Guid, string> groupNames =
                (DocumentGroupManager.Instance.GetAll()
                    ?? new List<TblNhomTaiLieu>())
                .ToDictionary(
                    group => group.IdNhomTaiLieu,
                    group => group.TenNhom);

            foreach (TblLoaiTaiLieu documentType in documentTypes)
            {
                string groupName;
                groupNames.TryGetValue(
                    documentType.IdNhomTaiLieu,
                    out groupName);

                string text = idNhomTaiLieu.HasValue
                    ? documentType.TenLoai
                    : string.IsNullOrEmpty(groupName)
                    ? documentType.TenLoai
                    : groupName + " / " + documentType.TenLoai;

                if (!documentType.KichHoat)
                    text += " (Đã khóa)";

                dropdown.AddItem(
                    text,
                    documentType.IdLoaiTaiLieu.ToString());
            }

            dropdown.ClearSelection();
        }

        public void BindDocumentSigningMethods(
            ExtraDropdown dropdown,
            bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue =
                isAll ? "null" : string.Empty;

            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.EmptyItemText =
                    UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = string.Empty;
            }

            dropdown.Items.Add(
                new ListItem(
                    UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.PAPER_SIGNING),
                    DocumentSigningMethodKeys.Paper));

            dropdown.Items.Add(
                new ListItem(
                    UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys
                            .EXTERNAL_DIGITAL_SIGNING),
                    DocumentSigningMethodKeys
                        .DigitalExternal));

            dropdown.SelectedIndex = isAll ? -1 : 0;
        }

        public void BindDocumentStatuses(
            ExtraDropdown dropdown,
            bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = isAll ? "null" : string.Empty;
            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.EmptyItemText =
                    UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = string.Empty;
            }

            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.DRAFTING),
                DocumentStatusKeys.Drafting));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.PENDING_SIGNATURE),
                DocumentStatusKeys.PendingSignature));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.CHANGES_REQUESTED),
                DocumentStatusKeys.ChangesRequested));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.SIGNED),
                DocumentStatusKeys.Signed));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.COMPLETED),
                DocumentStatusKeys.Completed));
            dropdown.SelectedIndex = -1;
        }

        public void BindDocumentStatuses(BootstrapDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.AddItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.DRAFTING),
                DocumentStatusKeys.Drafting);
            dropdown.AddItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.PENDING_SIGNATURE),
                DocumentStatusKeys.PendingSignature);
            dropdown.AddItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.CHANGES_REQUESTED),
                DocumentStatusKeys.ChangesRequested);
            dropdown.AddItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.SIGNED),
                DocumentStatusKeys.Signed);
            dropdown.AddItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.COMPLETED),
                DocumentStatusKeys.Completed);
            dropdown.ClearSelection();
        }

        public void BindDocumentCustomerStatuses(
            ExtraDropdown dropdown,
            bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = isAll ? "null" : string.Empty;
            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.EmptyItemText =
                    UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = string.Empty;
            }

            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.NOT_SENT),
                DocumentCustomerStatusKeys.NotSent));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.SENT),
                DocumentCustomerStatusKeys.Sent));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.WAITING_FOR_RETURN),
                DocumentCustomerStatusKeys.WaitingForReturn));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.RECEIVED_BACK),
                DocumentCustomerStatusKeys.ReceivedBack));
            dropdown.SelectedIndex = -1;
        }

        public void BindDocumentPhysicalStorageStatuses(
            ExtraDropdown dropdown,
            bool isAll = false)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = isAll ? "null" : string.Empty;
            if (isAll)
            {
                dropdown.AlowClear = true;
                dropdown.EmptyItemText =
                    UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.ALL);
                dropdown.EmptyItemValue = string.Empty;
            }

            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.NOT_STORED),
                DocumentPhysicalStorageStatusKeys.NotStored));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.STORED),
                DocumentPhysicalStorageStatusKeys.Stored));
            dropdown.Items.Add(new ListItem(
                UITextsReader.GetBackEndResourceText(
                    BackEndResourceKeys.CHECKED_OUT),
                DocumentPhysicalStorageStatusKeys.CheckedOut));
            dropdown.SelectedIndex = -1;
        }
        #endregion
    }
}
