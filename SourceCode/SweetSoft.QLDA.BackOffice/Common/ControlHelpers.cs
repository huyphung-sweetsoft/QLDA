//-----------------------PROGRAMER LOGS---------------------------
using SubSonic;
using SweetSoft.QLDA.BackOffice.Controls;
using SweetSoft.QLDA.BackOffice.Controls.AutoComplete;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Core.Managers.TaskManager;

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
        public void BindLaNhanVien(ExtraDropdown dropdown, bool isAll = false)
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
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.EMPLOYEE_ACCOUNT), "1"));
            dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.SYSTEM_ACCOUNT), "0"));
            dropdown.SelectedIndex = -1;
        }
        public void BindLaNhanVien(BootstrapDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.AddItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.EMPLOYEE_ACCOUNT), "1");
            dropdown.AddItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.SYSTEM_ACCOUNT), "0");
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
        public void BindChucDanh(ExtraDropdown ddl)
        {
            ddl.Items.Clear();
            List<TblChucDanh> chucDanh = ChucDanhManager.Instance.GetListForDropdown();
            if (chucDanh == null)
                chucDanh = new List<TblChucDanh>();
            ddl.DataTextField = TblChucDanh.Columns.TenChucDanh;
            ddl.DataValueField = TblChucDanh.Columns.IdChucDanh;
            ddl.DataSource = chucDanh;
            ddl.DataBind();
        }
        public void BindChucDanh(BootstrapDropdown ddl)
        {
            ddl.Items.Clear();
            List<TblChucDanh> chucDanh = ChucDanhManager.Instance.GetListForDropdown();
            if (chucDanh == null)
                chucDanh = new List<TblChucDanh>();
            ddl.DataTextField = TblChucDanh.Columns.TenChucDanh;
            ddl.DataValueField = TblChucDanh.Columns.IdChucDanh;
            ddl.DataSource = chucDanh;
            ddl.DataBind();
        }
        public void BindPhongBan(ExtraDropdown ddl)
        {
            ddl.Items.Clear();
            List<TblPhongBan> phongBan = PhongBanManager.Instance.GetListForDropdown();
            if (phongBan == null)
                phongBan = new List<TblPhongBan>();
            ddl.DataTextField = TblPhongBan.Columns.TenPhongBan;
            ddl.DataValueField = TblPhongBan.Columns.IdPhongBan;
            ddl.DataSource = phongBan;
            ddl.DataBind();
        }
        public void BindPhongBan(BootstrapDropdown ddl)
        {
            ddl.Items.Clear();
            List<TblPhongBan> phongBan = PhongBanManager.Instance.GetListForDropdown();
            if (phongBan == null)
                phongBan = new List<TblPhongBan>();
            ddl.DataTextField = TblPhongBan.Columns.TenPhongBan;
            ddl.DataValueField = TblPhongBan.Columns.IdPhongBan;
            ddl.DataSource = phongBan;
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

        public void BindDuAnStatus(ExtraDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            foreach (DuAnStatus status in Enum.GetValues(typeof(DuAnStatus)))
            {
                string text = EnumHelpers.GetERenderText(typeof(DuAnStatus), status);
                string value = ((byte)status).ToString();
                dropdown.Items.Add(new ListItem(text, value));
            }

            dropdown.SelectedIndex = -1;
        }

        public void BindDuAnStatus(BootstrapDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            foreach (DuAnStatus status in Enum.GetValues(typeof(DuAnStatus)))
            {
                string text = EnumHelpers.GetERenderText(typeof(DuAnStatus), status);
                string value = ((byte)status).ToString();
                dropdown.AddItem(text, value);
            }

            dropdown.SelectedIndex = -1;
        }

        public void BindLoaiDuAn(ExtraDropdown ddl)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = "";
            List<TblLoaiDuAn> tblLoaiDuAns = LoaiDuAnManager.Instance.GetAllLoaiDuAn();
            if (tblLoaiDuAns == null)
                tblLoaiDuAns = new List<TblLoaiDuAn>();
            ddl.DataTextField = TblLoaiDuAn.Columns.TenLoaiDuAn;
            ddl.DataValueField = TblLoaiDuAn.Columns.IdLoaiDuAn;
            ddl.DataSource = tblLoaiDuAns;
            ddl.DataBind();
        }

        public void BindLoaiDuAn(BootstrapDropdown ddl)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = "";
            List<TblLoaiDuAn> tblLoaiDuAns = LoaiDuAnManager.Instance.GetAllLoaiDuAn();
            if (tblLoaiDuAns == null)
                tblLoaiDuAns = new List<TblLoaiDuAn>();
            ddl.DataTextField = TblLoaiDuAn.Columns.TenLoaiDuAn;
            ddl.DataValueField = TblLoaiDuAn.Columns.IdLoaiDuAn;
            ddl.DataSource = tblLoaiDuAns;
            ddl.DataBind();
        }

        public void BindKhachHang(ExtraDropdown ddl)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = "";
            List<TblKhachHang> tblKhachHangs = KhachHangManager.Instance.GetAllKhachHang();
            if (tblKhachHangs == null)
                tblKhachHangs = new List<TblKhachHang>();
            ddl.DataTextField = TblKhachHang.Columns.TenKhachHang;
            ddl.DataValueField = TblKhachHang.Columns.IdKhachHang;
            ddl.DataSource = tblKhachHangs;
            ddl.DataBind();
        }

        public void BindNhanVien(ExtraDropdown ddl)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = " ";
            List<AspnetUser> tblNhanViens = UserManager.Instance.GetAllAspnetUsers();
            if (tblNhanViens == null)
                tblNhanViens = new List<AspnetUser>();
            ddl.DataTextField = AspnetUser.Columns.DisplayName;
            ddl.DataValueField = TblNhanVien.Columns.UserId;
            ddl.DataSource = tblNhanViens;
            ddl.DataBind();
        }

        public void BindNhanVien(BootstrapDropdown ddl)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = " ";
            List<AspnetUser> tblNhanViens = UserManager.Instance.GetAllAspnetUsers();
            if (tblNhanViens == null)
                tblNhanViens = new List<AspnetUser>();
            ddl.DataTextField = AspnetUser.Columns.DisplayName;
            ddl.DataValueField = AspnetUser.Columns.UserId;
            ddl.DataSource = tblNhanViens;
            ddl.DataBind();
        }

        public void BindLoaiKhachHang(BootstrapDropdown ddl)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = " ";
            List<TblLoaiKhachHang> tblLoaiKhachHangs = LoaiKhachHangManager.Instance.GetAllLoaiKhachHang();
            if (tblLoaiKhachHangs == null)
                tblLoaiKhachHangs = new List<TblLoaiKhachHang>();
            ddl.DataTextField = TblLoaiKhachHang.Columns.TenLoaiKhachHang;
            ddl.DataValueField = TblLoaiKhachHang.Columns.IdLoaiKhachHang;
            ddl.DataSource = tblLoaiKhachHangs;
            ddl.DataBind();
        }

        public void BindLoaiKhachHang(ExtraDropdown ddl)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = " ";
            List<TblLoaiKhachHang> tblLoaiKhachHangs = LoaiKhachHangManager.Instance.GetAllLoaiKhachHang();
            if (tblLoaiKhachHangs == null)
                tblLoaiKhachHangs = new List<TblLoaiKhachHang>();
            ddl.DataTextField = TblLoaiKhachHang.Columns.TenLoaiKhachHang;
            ddl.DataValueField = TblLoaiKhachHang.Columns.IdLoaiKhachHang;
            ddl.DataSource = tblLoaiKhachHangs;
            ddl.DataBind();
        }
        #endregion
        #region Binding Task Controls
        public void BindPriorities(DropDownList ddl, Guid? selectedId = null, bool isAll = false)
        {
            ddl.Items.Clear();
            if (isAll)
                ddl.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL), ""));
            else
                ddl.Items.Add(new ListItem("-- Chọn độ ưu tiên --", ""));
            try
            {
                DataTable dt = TaskManager.Instance.GetPrioritiesTable();
                foreach (DataRow r in dt.Rows)
                {
                    string ten = r[TblDoUuTien.Columns.TenDoUuTien].ToString();
                    string val = r[TblDoUuTien.Columns.IdDoUuTien].ToString().ToLower();
                    int diem = Convert.ToInt32(r[TblDoUuTien.Columns.DiemUuTien]);

                    ListItem item = new ListItem(ten, val);
                    switch (diem)
                    {
                        case 1: item.Attributes["class"] = "opt-pri-low"; break;
                        case 2: item.Attributes["class"] = "opt-pri-med"; break;
                        case 3: item.Attributes["class"] = "opt-pri-high"; break;
                    }
                    ddl.Items.Add(item);
                }
            }
            catch { }
            if (selectedId.HasValue)
            {
                ListItem found = ddl.Items.FindByValue(selectedId.Value.ToString().ToLower());
                if (found != null)
                {
                    ddl.ClearSelection();
                    found.Selected = true;
                }
            }
        }

        public void BindProjectMembers(DropDownList ddl, Guid projectId, Guid? taskId = null)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("Không có nhân viên", "0"));
        }
        public void BindTaskStatus(DropDownList ddl, byte? selectedStatus = null)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("Chưa bắt đầu", "0") { Attributes = { ["class"] = "opt-status-todo" } });
            ddl.Items.Add(new ListItem("Đang làm", "1") { Attributes = { ["class"] = "opt-status-doing" } });
            ddl.Items.Add(new ListItem("Hoàn thành", "2") { Attributes = { ["class"] = "opt-status-done" } });

            if (selectedStatus.HasValue)
            {
                ddl.SelectedValue = selectedStatus.Value.ToString();
            }
        }
        public void BindParentTasks(DropDownList ddl, Guid projectId, Guid? excludeTaskId = null, Guid? selectedParentId = null)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- Không có --", ""));

            DataTable dt = TaskManager.Instance.FetchByIdAndOrderASCMaCV(projectId);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (Guid.TryParse(row[ColIdCongViec]?.ToString(), out Guid id))
                    {
                        if (excludeTaskId.HasValue && id == excludeTaskId.Value) continue;
                        string maCv = row[ColMaCv]?.ToString();
                        string tenCv = row[ColTenCv]?.ToString();
                        ddl.Items.Add(new ListItem($"[{maCv}] {tenCv}", id.ToString().ToLower()));
                    }
                }
            }

            if (selectedParentId.HasValue)
            {
                ListItem found = ddl.Items.FindByValue(selectedParentId.Value.ToString().ToLower());
                if (found != null)
                {
                    ddl.ClearSelection();
                    found.Selected = true;
                }
            }
        }
        public void BindDependentTasks(DropDownList ddl, Guid projectId, Guid? excludeTaskId = null, Guid? selectedDepId = null, string currentOrNewCode = null)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- Không có --", ""));
            DataTable dt = TaskManager.Instance.FetchByIdAndOrderASCMaCV(projectId);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (Guid.TryParse(row[ColIdCongViec]?.ToString(), out Guid id))
                    {
                        if (excludeTaskId.HasValue && id == excludeTaskId.Value) continue;
                        string maCv = row[ColMaCv]?.ToString() ?? "";
                        string tenCv = row[ColTenCv]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(currentOrNewCode) && TaskManager.Instance.IsAfterOrEqual(maCv, currentOrNewCode))
                            continue;

                        ddl.Items.Add(new ListItem($"[{maCv}] {tenCv}", id.ToString().ToLower()));
                    }
                }
            }

            if (selectedDepId.HasValue)
            {
                ListItem found = ddl.Items.FindByValue(selectedDepId.Value.ToString().ToLower());
                if (found != null)
                {
                    ddl.ClearSelection();
                    found.Selected = true;
                }
            }
        }
        public string GetFormattedTaskName(object maCvObj, object tenCvObj)
        {
            string maCv = maCvObj?.ToString() ?? "";
            string tenCv = tenCvObj?.ToString() ?? "";
            int level = maCv.Split('.').Length;
            if (level == 1)
            {
                return $"<div class=\"task-phase-box d-flex align-items-center\">" +
                       $"<i class=\"far fa-folder-open me-2\" style=\"color: #6f42c1;\"></i>" +
                       $"<span class=\"task-phase-text\">{maCv}. {tenCv}</span>" +
                       $"</div>";
            }
            int indentPx = (level - 1) * 20;
            return $"<div class=\"task-sub-box\" style=\"padding-left: {indentPx}px;\">" +
                   $"<span class=\"task-tree-branch me-1 text-muted\">└──</span>" +
                   $"<strong class=\"task-sub-code\">{maCv}.</strong> {tenCv}" +
                   $"</div>";
        }
        public string FormatDateTime(object dateObj, string format = "dd/MM/yyyy", string emptyText = "—")
        {
            if (dateObj != null && DateTime.TryParse(dateObj.ToString(), out DateTime date))
                return date.ToString(format);
            return emptyText;
        }
        public string GetTaskStatusBadge(object statusObj)
        {
            if (statusObj == null || statusObj == DBNull.Value)
                return "<span class=\"badge-pill-custom badge-status-todo\">Chưa bắt đầu</span>";
            int status = Convert.ToInt32(statusObj);
            switch (status)
            {
                case 1: return "<span class=\"badge-pill-custom badge-status-doing\">Đang làm</span>";
                case 2: return "<span class=\"badge-pill-custom badge-status-done\">Hoàn thành</span>";
                default: return "<span class=\"badge-pill-custom badge-status-todo\">Chưa bắt đầu</span>";
            }
        }
        public string GetTaskPriorityBadge(object tenDoUuTienObj, object diemUuTienObj)
        {
            string tenDoUuTien = tenDoUuTienObj != null ? tenDoUuTienObj.ToString() : "—";
            if (diemUuTienObj != null && int.TryParse(diemUuTienObj.ToString(), out int diem))
            {
                string cssClass = "badge-pill-custom badge-pri-med";
                switch (diem)
                {
                    case 1: cssClass = "badge-pill-custom badge-pri-low"; break;
                    case 2: cssClass = "badge-pill-custom badge-pri-med"; break;
                    case 3: cssClass = "badge-pill-custom badge-pri-high"; break;
                }
                return $"<span class=\"{cssClass}\">{tenDoUuTien}</span>";
            }
            return $"<span class=\"badge-pill-custom badge-pri-med\">{tenDoUuTien}</span>";
        }
        #endregion
        #region Bingding Risk Controls
        public void BindNhanVienDuAn(ExtraDropdown ddl, Guid projectId)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = " ";
            DataTable dt = RiskManager.Instance.GetAllNhanVienDuAnById(projectId);
            if (dt != null && dt.Rows.Count > 0)
            {
                ddl.DataTextField = "TenNhanVien";     
                ddl.DataValueField = "IdNhanVien";     
                ddl.DataSource = dt;
                ddl.DataBind();
            }
        }

        public void BindMucDoAnhHuong(ExtraDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            dropdown.Items.Add(new ListItem("-- Chọn giá trị --", ""));

            foreach (MucDoAnhHuonEnum score in Enum.GetValues(typeof(MucDoAnhHuonEnum)))
            {
                string value = ((int)score).ToString();

                var field = score.GetType().GetField(score.ToString());
                var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                     .FirstOrDefault() as DescriptionAttribute;

                string text = attribute != null ? attribute.Description : score.ToString();

                dropdown.Items.Add(new ListItem(text, value));
            }
        }
        public void BindXacSuatRuiRo(ExtraDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            foreach (XacSuatRuiRoEnum prob in Enum.GetValues(typeof(XacSuatRuiRoEnum)))
            {
                string value = ((int)prob).ToString();

                var field = prob.GetType().GetField(prob.ToString());
                var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                     .FirstOrDefault() as DescriptionAttribute;

                string text = attribute != null ? attribute.Description : prob.ToString();

                dropdown.Items.Add(new ListItem(text, value));
            }
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
        public void BindCongViecDuAn(ExtraDropdown ddl, Guid projectId)
        {
            ddl.Items.Clear();
            ddl.DefaultSearchValue = " ";
            DataTable dt = TaskManager.Instance.FetchByIdAndOrderASCMaCV(projectId);
            if (dt != null && dt.Rows.Count > 0)
            {
                dt.DefaultView.RowFilter = "MaCongViec LIKE '%.%'";
                DataTable filteredDt = dt.DefaultView.ToTable();
                if (filteredDt.Rows.Count > 0)
                {
                    filteredDt.Columns.Add("DisplayField", typeof(string), "MaCongViec + '. ' + TenCongViec");

                    ddl.DataTextField = "DisplayField";
                    ddl.DataValueField = "IdCongViec";
                    ddl.DataSource = filteredDt;
                    ddl.DataBind();
                }
            }
        }
        #endregion
        #region Bind Issue Data
        public void BindNguonGocVanDe(ExtraDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            foreach (NguonGocVanDeEnum source in Enum.GetValues(typeof(NguonGocVanDeEnum)))
            {
                string value = ((int)source).ToString();

                var field = source.GetType().GetField(source.ToString());
                var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                     .FirstOrDefault() as DescriptionAttribute;

                string text = attribute != null ? attribute.Description : source.ToString();

                dropdown.Items.Add(new ListItem(text, value));
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
            BindDocumentTypes(dropdown, null, isAll);
        }

        public void BindDocumentTypes(
            ExtraDropdown dropdown,
            Guid? idNhomTaiLieu,
            bool isAll = false)
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

                string text = idNhomTaiLieu.HasValue
                    ? documentType.TenLoai
                    : string.IsNullOrEmpty(groupName)
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
        #endregion
        #region Bind Meeting Data
        public void BindTrangThaiLichHop(ExtraDropdown dropdown)
        {
            dropdown.Items.Clear();
            dropdown.DefaultSearchValue = "null";
            foreach (TrangThaiCuocHopEnum status in Enum.GetValues(typeof(TrangThaiCuocHopEnum)))
            {
                string value = ((int)status).ToString();

                var field = status.GetType().GetField(status.ToString());
                var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                     .FirstOrDefault() as DescriptionAttribute;

                string text = attribute != null ? attribute.Description : status.ToString();

                dropdown.Items.Add(new ListItem(text, value));
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
