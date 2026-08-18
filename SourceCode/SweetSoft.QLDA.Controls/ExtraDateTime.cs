using SweetCMS.Controls.Helpers;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Controls.Interfaces;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ExtraDateTime runat=\"server\"></{0}:ExtraDateTime>")]
    public class ExtraDateTime : WebControl, INamingContainer, IPostBackDataHandler, IPostBackEventHandler
    {
        public static IDateTimeConverter DateTimeConverter { get; set; }
        private static string hdfId = "_hdfDRPValue";
        [Description("Fires when the date has been changed and AutoPostBack is set to 'true'.")]
        public event EventHandler<DateChangedEventArgs> DateChanged;
        JavaScriptSerializer jss = new JavaScriptSerializer();
        #region SearchTag
        public bool SearchTagHasTime
        {
            get
            {
                object obj = ViewState["SearchTagHasTime"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["SearchTagHasTime"] = value; }
        }
        public string DateValueInString
        {
            get
            {
                string startDate = DateTimeHelper.ConvertDateTime(StartValue, SearchTagHasTime);
                string endDate = DateTimeHelper.ConvertDateTime(EndValue, SearchTagHasTime);
                return string.Format("{0}|{1}", startDate, endDate);
            }
        }
        private string _searchTagItemText
        {
            get
            {
                string startDate = StartValue == null ? "" : GetFormatDisplay(StartValue.Value, SearchTagHasTime);
                string endDate = EndValue == null ? "" : GetFormatDisplay(EndValue.Value, SearchTagHasTime);

                if (DateTimeHelper.IsEnglish)
                {
                    if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                    {
                        return string.Format("{0} {1} {2} {3}"
                       , "From"
                       , startDate
                       , "to"
                       , endDate);
                    }
                    if (!string.IsNullOrEmpty(startDate))
                        return string.Format("{0} {1}", "From", startDate);
                    if (!string.IsNullOrEmpty(endDate))
                        return string.Format("{0} {1}", "to", endDate);
                }
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    return string.Format("{0} {1} {2} {3}"
                   , "Từ ngày"
                   , startDate
                   , "đến ngày"
                   , endDate);
                }
                if (!string.IsNullOrEmpty(startDate))
                    return string.Format("{0} {1}", "Từ ngày", startDate);
                if (!string.IsNullOrEmpty(endDate))
                    return string.Format("{0} {1}", "đến ngày", endDate);


                return string.Empty;
            }
        }
        public string SearchTagItemText
        {
            get
            {

                object obj = ViewState["SearchTagItemText"];
                return (obj == null) ? "" : string.Format((string)obj, _searchTagItemText);
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
                string dateValueInString = DateValueInString;
                if (string.IsNullOrEmpty(dateValueInString) || dateValueInString == "-" || dateValueInString == "|")
                    return null;
                else
                    return new SearchTagItem(SearchTagItemKey, SearchTagItemText, dateValueInString, this.ID);
            }
        }
        #endregion

        #region Properties

        public string SearchColumn
        {
            get
            {
                object obj = ViewState["SearchColumn"];
                return (obj == null) ? "" : (string)obj;
            }
            set { ViewState["SearchColumn"] = value; }
        }
        private string _prefixKey
        {
            get
            {
                return "_SWEET";
                //return string.Format("_{0}", new Guid());
            }
        }
        public bool AutoPostBack
        {
            get
            {
                object obj = ViewState["AutoPostBack" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["AutoPostBack" + _prefixKey] = value;
            }
        }
        public override string CssClass
        {
            get
            {
                object obj = ViewState["CssClass" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["CssClass" + _prefixKey] = value;
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
        public DateTime? DateValue
        {
            get
            {
                var obj = ViewState["DateValue" + _prefixKey];
                if (obj == null) return null;

                DateTime dateValue = (DateTime)obj;
                if (dateValue == DateTimeHelper.MinValueSQL) return null;

                // Trả về UTC time cho business logic
                return DateTimeConverter?.ConvertSettingTimeToUtc(dateValue);
            }
            set
            {
                if (value == null || value == DateTimeHelper.MinValueSQL)
                {
                    ViewState["DateValue" + _prefixKey] = DateTimeHelper.MinValueSQL;
                }
                else
                {
                    // Lưu setting time vào ViewState để hiển thị đúng
                    ViewState["DateValue" + _prefixKey] = DateTimeConverter?.ConvertUTCToSettingTime(value.Value);
                }
            }
        }

        public DateTime? StartValue
        {
            get
            {
                var obj = ViewState["StartValue" + _prefixKey];
                if (obj == null) return null;

                DateTime dateValue = (DateTime)obj;
                if (dateValue == DateTimeHelper.MinValueSQL) return null;

                // Trả về UTC time cho business logic
                return DateTimeConverter?.ConvertSettingTimeToUtc(dateValue);
            }
            set
            {
                if (value == null)
                {
                    ViewState["StartValue" + _prefixKey] = null;
                }
                else
                {
                    ViewState["StartValue" + _prefixKey] = DateTimeConverter?.ConvertUTCToSettingTime(value.Value);
                }
            }
        }

        public DateTime? EndValue
        {
            get
            {
                var obj = ViewState["EndValue" + _prefixKey];
                if (obj == null) return null;

                DateTime dateValue = (DateTime)obj;
                if (dateValue == DateTimeHelper.MinValueSQL) return null;

                // Trả về UTC time cho business logic
                return DateTimeConverter?.ConvertSettingTimeToUtc(dateValue);
            }
            set
            {
                if (value == null)
                {
                    ViewState["EndValue" + _prefixKey] = null;
                }
                else
                {
                    ViewState["EndValue" + _prefixKey] = DateTimeConverter?.ConvertUTCToSettingTime(value.Value);
                }
            }
        }

        public DateTime? DateValueForDisplay
        {
            get
            {
                var obj = ViewState["DateValue" + _prefixKey];
                if (obj == null) return null;

                DateTime dateValue = (DateTime)obj;
                if (dateValue == DateTimeHelper.MinValueSQL) return null;

                // Trả về setting time để hiển thị
                return dateValue;
            }
        }

        public DateTime? StartValueForDisplay
        {
            get
            {
                var obj = ViewState["StartValue" + _prefixKey];
                if (obj == null) return null;

                DateTime dateValue = (DateTime)obj;
                return dateValue; // Trả về setting time để hiển thị
            }
        }

        public DateTime? EndValueForDisplay
        {
            get
            {
                var obj = ViewState["EndValue" + _prefixKey];
                if (obj == null) return null;

                DateTime dateValue = (DateTime)obj;
                return dateValue; // Trả về setting time để hiển thị
            }
        }

        public DateTime? MinDate
        {
            get
            {
                object obj = ViewState["MinDate" + _prefixKey];
                return (obj == null) ? null : (DateTime?)obj;
            }
            set
            {
                ViewState["MinDate" + _prefixKey] = value;
            }
        }
        public DateTime MaxDate
        {
            get
            {
                object obj = ViewState["MaxDate" + _prefixKey];
                return (obj == null) ? DateTime.MaxValue : (DateTime)obj;
            }
            set
            {
                ViewState["MaxDate" + _prefixKey] = value;
            }
        }
        public bool ShowDropdown
        {
            get
            {
                object obj = ViewState["ShowDropdown" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["ShowDropdown" + _prefixKey] = value;
            }
        }
        public int MinYear
        {
            get
            {
                object obj = ViewState["MinYear" + _prefixKey];
                return (obj == null) ? 0 : (int)obj;
            }
            set
            {
                ViewState["MinYear" + _prefixKey] = value;
            }
        }
        public int MaxYear
        {
            get
            {
                object obj = ViewState["MaxYear" + _prefixKey];
                return (obj == null) ? 0 : (int)obj;
            }
            set
            {
                ViewState["MaxYear" + _prefixKey] = value;
            }
        }
        public bool ShowWeekNumber
        {
            get
            {
                object obj = ViewState["ShowWeekNumber" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["ShowWeekNumber" + _prefixKey] = value;
            }
        }
        public bool ShowISOWeekNumber
        {
            get
            {
                object obj = ViewState["ShowISOWeekNumber" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["ShowISOWeekNumber" + _prefixKey] = value;
            }
        }
        public bool TimePicker
        {
            get
            {
                object obj = ViewState["TimePicker" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["TimePicker" + _prefixKey] = value;
            }
        }
        public int TimePickerIncrement
        {
            get
            {
                object obj = ViewState["TimePickerIncrement" + _prefixKey];
                return (obj == null) ? 0 : (int)obj;
            }
            set
            {
                ViewState["TimePickerIncrement" + _prefixKey] = value;
            }
        }
        public bool TimePicker24Hour
        {
            get
            {
                object obj = ViewState["TimePicker24Hour" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["TimePicker24Hour" + _prefixKey] = value;
            }
        }
        public bool TimePickerSecond
        {
            get
            {
                object obj = ViewState["TimePickerSecond" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["TimePickerSecond" + _prefixKey] = value;
            }
        }
        public bool ShowCustomRangeLabel
        {
            get
            {
                object obj = ViewState["ShowCustomRangeLabel" + _prefixKey];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["ShowCustomRangeLabel" + _prefixKey] = value;
            }
        }
        public bool AlwaysShowCalendar
        {
            get
            {
                object obj = ViewState["AlwaysShowCalendar" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["AlwaysShowCalendar" + _prefixKey] = value;
            }
        }
        public OpenDates Opens
        {
            get
            {
                object obj = ViewState["Opens" + _prefixKey];
                return (obj == null) ? OpenDates.Auto : (OpenDates)obj;
            }
            set
            {
                ViewState["Opens" + _prefixKey] = value;
            }
        }
        public DropDates Drops
        {
            get
            {
                object obj = ViewState["Drops" + _prefixKey];
                return (obj == null) ? DropDates.Auto : (DropDates)obj;
            }
            set
            {
                ViewState["Drops" + _prefixKey] = value;
            }
        }
        public string ButtonClasses
        {
            get
            {
                object obj = ViewState["ButtonClasses" + _prefixKey];
                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["ButtonClasses" + _prefixKey] = value;
            }
        }
        public string ApplyButtonClasses
        {
            get
            {
                object obj = ViewState["ApplyButtonClasses" + _prefixKey];
                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["ApplyButtonClasses" + _prefixKey] = value;
            }
        }
        public string CancelButtonClasses
        {
            get
            {
                object obj = ViewState["CancelButtonClasses" + _prefixKey];
                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["CancelButtonClasses" + _prefixKey] = value;
            }
        }
        public string Locale
        {
            get
            {
                object obj = ViewState["Locale" + _prefixKey];
                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["Locale" + _prefixKey] = value;
            }
        }
        public bool SingleDatePicker
        {
            get
            {
                object obj = ViewState["SingleDatePicker" + _prefixKey];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["SingleDatePicker" + _prefixKey] = value;
            }
        }
        public bool AllowNullDate
        {
            get
            {
                object obj = ViewState["AllowNullDate" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["AllowNullDate" + _prefixKey] = value;
            }
        }
        public bool AutoApply
        {
            get
            {
                object obj = ViewState["AutoApply" + _prefixKey];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["AutoApply" + _prefixKey] = value;
            }
        }
        public bool LinkedCalendar
        {
            get
            {
                object obj = ViewState["LinkedCalendar" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["LinkedCalendar" + _prefixKey] = value;
            }
        }
        public bool IsPredefinedDateRanges
        {
            get
            {
                object obj = ViewState["IsPredefinedDateRanges" + _prefixKey];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IsPredefinedDateRanges" + _prefixKey] = value;
            }
        }
        public string IsInvalidDate
        {
            get
            {
                object obj = ViewState["isInvalidDate" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["isInvalidDate" + _prefixKey] = value;
            }
        }
        public string IsCustomDate
        {
            get
            {
                object obj = ViewState["IsCustomDate" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["IsCustomDate" + _prefixKey] = value;
            }
        }
        public bool AutoUpdateInput
        {
            get
            {
                object obj = ViewState["AutoUpdateInput" + _prefixKey];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["AutoUpdateInput" + _prefixKey] = value;
            }
        }
        public string ParentEl
        {
            get
            {
                object obj = ViewState["ParentEl" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["ParentEl" + _prefixKey] = value;
            }
        }
        public string DateFormat
        {
            get
            {
                object obj = ViewState["DateFormat" + _prefixKey];
                string dateFormat = string.Empty;
                if (CultureInfo.CurrentCulture.Name == "en-US")
                {

                    if (obj == null)
                        dateFormat = "MM/dd/yyyy";
                    else
                        dateFormat = (string)obj;
                }
                else
                {
                    if (obj == null)
                        dateFormat = "DD/MM/YYYY";
                    else
                        dateFormat = (string)obj;
                }

                if (TimePicker)
                {
                    string timeFormat = string.Empty;
                    if (TimePicker24Hour)
                        timeFormat = "HH:mm";
                    else
                        timeFormat = "hh:mm";

                    if (TimePickerSecond && !string.IsNullOrEmpty(timeFormat))
                        timeFormat = $"{timeFormat}:ss";

                    if (!string.IsNullOrEmpty(timeFormat))
                        dateFormat = $"{dateFormat} {timeFormat}";
                }
                return dateFormat;
            }
            set
            {
                ViewState["DateFormat" + _prefixKey] = value;
            }
        }
        public string PlaceHolder
        {
            get
            {
                object obj = ViewState["PlaceHolder" + _prefixKey];
                return (obj == null) ? "Chọn ngày" : (string)obj;
            }
            set
            {
                ViewState["PlaceHolder" + _prefixKey] = value;
            }
        }
        public string Disable
        {
            get
            {
                object obj = ViewState["Disable" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["Disable" + _prefixKey] = value;
            }
        }

        public string OnChange
        {
            get
            {
                object obj = ViewState["OnChange" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["OnChange" + _prefixKey] = value;
            }
        }

        public string OnClose
        {
            get
            {
                object obj = ViewState["OnClose" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["OnClose" + _prefixKey] = value;
            }
        }

        public string OnOpen
        {
            get
            {
                object obj = ViewState["OnOpen" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["OnOpen" + _prefixKey] = value;
            }
        }

        public string OnReady
        {
            get
            {
                object obj = ViewState["OnReady" + _prefixKey];
                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["OnReady" + _prefixKey] = value;
            }
        }

        public void ClearDate()
        {
            DateValue = StartValue = EndValue = DateTime.MinValue;
        }
        #endregion
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Input;
            }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ExtraScriptRegister.RegisterDateTime = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
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
            #region RegionName
            writer.Write("<div class=\"input-group\" data-test=\"rerender\" >");
            string _cssClass = string.Format("form-control text-white {0}", this.CssClass); // Default css class
            if (DateValue != DateTime.MinValue || StartValue != DateTime.MinValue)
            {
                _cssClass = $"{_cssClass} ignore valid-success";
                writer.AddAttribute("data-valid-success", "true");
            }    
            if (Required && Enabled)
                _cssClass += " validate[required]";
            writer.AddAttribute("data-selector", this.ID);
            writer.AddAttribute("autocomplete", "off");
            //writer.AddAttribute("readonly", "readonly");
            writer.AddAttribute("data-control", "extra-datetime");
            writer.AddAttribute("class", _cssClass);

            writer.AddAttribute("placeholder", !string.IsNullOrEmpty(PlaceHolder) ? PlaceHolder : DateFormat);

            if (IsPredefinedDateRanges)
            {
                SingleDatePicker = false;
                ShowDropdown = true;
            }

            writer.AddAttribute("data-allowNullDate", AllowNullDate ? "true" : "false");
            if (SingleDatePicker)
            {
                if (!AllowNullDate && (DateValueForDisplay == null
                || DateValueForDisplay == DateTime.MinValue
                || DateValueForDisplay == DateTimeHelper.MinValueSQL))
                    {
                        // Set default value as setting time
                        var defaultTime = DateTimeConverter?.ConvertUTCToSettingTime(DateTime.UtcNow) ?? DateTime.UtcNow;
                        DateValue = DateTime.UtcNow; // This will be converted and stored properly
                        writer.AddAttribute("data-startDate", GetFormat(defaultTime));
                        writer.AddAttribute("data-endDate", GetFormat(defaultTime));
                    }
                    else
                    {
                        if (DateValueForDisplay != null && DateValueForDisplay != DateTime.MinValue
                            && DateValueForDisplay != DateTimeHelper.MinValueSQL)
                        {
                            writer.AddAttribute("data-startDate", GetFormat(DateValueForDisplay.Value));
                            writer.AddAttribute("data-endDate", GetFormat(DateValueForDisplay.Value));
                        }
                    }
            }
            else
            {
                if (StartValueForDisplay != null && StartValueForDisplay != DateTime.MinValue
                    && StartValueForDisplay != DateTimeHelper.MinValueSQL)
                    writer.AddAttribute("data-startDate", GetFormat(StartValueForDisplay.Value));

                if (EndValueForDisplay != null && EndValueForDisplay != DateTime.MinValue
                    && EndValueForDisplay != DateTimeHelper.MinValueSQL)
                    writer.AddAttribute("data-endDate", GetFormat(EndValueForDisplay.Value));
            }


            if (MinDate != null)
                writer.AddAttribute("data-minDate", MinDate.Value.ToString("dd/MM/yyyy HH:mm:ss"));

            if (MaxDate != DateTime.MaxValue)
                writer.AddAttribute("data-maxDate", MaxDate.ToString("dd/MM/yyyy HH:mm:ss"));


            if (MinYear > 0)
                writer.AddAttribute("data-minYear", MinYear.ToString());

            if (MaxYear > 0)
                writer.AddAttribute("data-maxYear", MaxYear.ToString());


            writer.AddAttribute("data-predefinedDateRanges", IsPredefinedDateRanges ? "true" : "false");

            writer.AddAttribute("data-showDropdowns", ShowDropdown ? "true" : "false");

            writer.AddAttribute("data-showWeekNumbers", ShowWeekNumber ? "true" : "false");

            writer.AddAttribute("data-showISOWeekNumbers", ShowISOWeekNumber ? "true" : "false");

            writer.AddAttribute("data-timePicker", TimePicker ? "true" : "false");

            if (TimePickerIncrement > 0)
                writer.AddAttribute("data-timePickerIncrement", TimePickerIncrement.ToString());

            writer.AddAttribute("data-timePicker24Hour", TimePicker24Hour ? "true" : "false");

            writer.AddAttribute("data-timePickerSeconds", TimePickerSecond ? "true" : "false");

            writer.AddAttribute("data-showCustomRangeLabel", ShowCustomRangeLabel ? "true" : "false");

            writer.AddAttribute("data-alwaysShowCalendars", AlwaysShowCalendar ? "true" : "false");

            writer.AddAttribute("data-opens", Opens.ToRender());

            writer.AddAttribute("data-drops", Drops.ToRender());

            if (!string.IsNullOrEmpty(ButtonClasses))
                writer.AddAttribute("data-buttonClasses", ButtonClasses);

            if (!string.IsNullOrEmpty(ApplyButtonClasses))
                writer.AddAttribute("data-applyButtonClasses", ApplyButtonClasses);

            if (!string.IsNullOrEmpty(CancelButtonClasses))
                writer.AddAttribute("data-cancelButtonClasses", CancelButtonClasses);

            if (!string.IsNullOrEmpty(DateFormat))
                writer.AddAttribute("data-dateFormat", DateFormat.Replace("MM/dd/yyyy", "MM/DD/YYYY").Replace("dd/MM/yyyy", "DD/MM/YYYY"));

            writer.AddAttribute("data-singleDatePicker", SingleDatePicker ? "true" : "false");

            writer.AddAttribute("data-autoApply", AutoApply ? "true" : "false");

            writer.AddAttribute("data-linkedCalendars", LinkedCalendar ? "true" : "false");

            if (!string.IsNullOrEmpty(IsInvalidDate))
                writer.AddAttribute("data-isInvalidDate", IsInvalidDate);

            if (!string.IsNullOrEmpty(IsCustomDate))
                writer.AddAttribute("data-isCustomDate", IsCustomDate);

            writer.AddAttribute("data-autoUpdateInput", AutoUpdateInput ? "true" : "false");

            if (!string.IsNullOrEmpty(ParentEl))
                writer.AddAttribute("data-parentEl", ParentEl);

            if (!string.IsNullOrEmpty(OnChange))
                writer.AddAttribute("data-onChange", OnChange);

            if (!string.IsNullOrEmpty(OnClose))
                writer.AddAttribute("data-onClose", OnClose);

            if (!string.IsNullOrEmpty(OnOpen))
                writer.AddAttribute("data-onOpen", OnOpen);

            if (!string.IsNullOrEmpty(OnReady))
                writer.AddAttribute("data-onReady", OnReady);

            if (!string.IsNullOrEmpty(Disable))
                writer.AddAttribute("data-disable", Disable);

            writer.AddAttribute("data-hdf", ClientID + hdfId);

            Page page = Page;
            if (page != null)
                page.VerifyRenderingInServerForm(this);

            //if (page != null)
            //    page.ClientScript.RegisterForEventValidation(UniqueID, String.Empty);

            if (this.AutoPostBack && page != null)
            {
                string onchange = page.ClientScript.GetPostBackEventReference(GetPostBackOptions(), true);
                onchange = String.Concat("ExtraDateTimeChange('", onchange.Replace("\\", "\\\\").Replace("'", "\\'"), "')");
                writer.AddAttribute("onchange", onchange);
            }

            writer.AddAttribute("data-allowNullDate", AllowNullDate ? "true" : "false");
            if (SingleDatePicker)
            {
                if (!AllowNullDate && (DateValueForDisplay == null
                    || DateValueForDisplay == DateTime.MinValue
                    || DateValueForDisplay == DateTimeHelper.MinValueSQL))
                {
                    var defaultTime = DateTimeConverter?.ConvertUTCToSettingTime(DateTime.UtcNow) ?? DateTime.UtcNow;
                    DateValue = DateTime.UtcNow;
                    writer.AddAttribute("value", GetFormat(defaultTime));
                }
                else
                {
                    if (DateValueForDisplay != null && DateValueForDisplay != DateTime.MinValue
                        && DateValueForDisplay != DateTimeHelper.MinValueSQL)
                        writer.AddAttribute("value", GetFormat(DateValueForDisplay.Value));
                }
            }
            else
            {
                if (StartValueForDisplay != null && StartValueForDisplay != DateTime.MinValue
                    && StartValueForDisplay != DateTimeHelper.MinValueSQL
                    && EndValueForDisplay != null && EndValueForDisplay != DateTime.MinValue
                    && EndValueForDisplay != DateTimeHelper.MinValueSQL)
                    writer.AddAttribute("value", string.Format("{0} - {1}",
                        GetFormat(StartValueForDisplay.Value), GetFormat(EndValueForDisplay.Value)));
            }
            #endregion

            //using JavaScriptSerializer
            string startValue = StartValueForDisplay == null || StartValueForDisplay == DateTime.MinValue
                || StartValueForDisplay == DateTimeHelper.MinValueSQL ? "" : GetFormat(StartValueForDisplay.Value);
            string endValue = EndValueForDisplay == null || EndValueForDisplay == DateTime.MinValue
                || EndValueForDisplay == DateTimeHelper.MinValueSQL ? "" : GetFormat(EndValueForDisplay.Value);
            string date = DateValueForDisplay == null || DateValueForDisplay == DateTime.MinValue
                || DateValueForDisplay == DateTimeHelper.MinValueSQL ? "" : GetFormat(DateValueForDisplay.Value);
            base.Render(writer);
            writer.Write("<span class='input-group-text calendar-open'>" +
                                    "<i class='fas fa-calendar-alt'></i>" +
                                "</span>");

            if (SingleDatePicker)
            {
                writer.WriteHtmlElement(new HtmlElement(string.Format("{0}", HtmlTextWriterTag.Input), "",
            ClientID + hdfId, null, null, new HtmlAttribute[] {
                     new HtmlAttribute("type", "hidden", null),
                     new HtmlAttribute("value", date + "-" + date, null),
                     new HtmlAttribute("name", UniqueID + hdfId, null) }, true, null), true);
            }
            else
            {
                writer.WriteHtmlElement(new HtmlElement(string.Format("{0}", HtmlTextWriterTag.Input), "",
           ClientID + hdfId, null, null, new HtmlAttribute[] {
                     new HtmlAttribute("type", "hidden", null),
                     new HtmlAttribute("value", startValue + "-" + endValue, null),
                     new HtmlAttribute("name", UniqueID + hdfId, null) }, true, null), true);
            }
            writer.Write("</div>");
        }

        /// <summary>
        /// Raise the DateChanged event.
        /// </summary>
        /// <param name="eventArgument"></param>
        public void RaisePostBackEvent(string eventArgument)
        {
            if (DateChanged != null)
            {
                if (SingleDatePicker)
                    DateChanged(this, new DateChangedEventArgs(DateValue));
                else
                    DateChanged(this, new DateChangedEventArgs(StartValue, EndValue));
            }
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            if (DateChanged != null)
            {
                if (SingleDatePicker)
                    DateChanged(this, new DateChangedEventArgs(DateValue));
                else
                    DateChanged(this, new DateChangedEventArgs(StartValue, EndValue));
            }
        }

        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool isChanged = false;
            string rawValue = postCollection[postDataKey + hdfId];

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                DateValue = StartValue = EndValue = null;
                return true;
            }

            string[] dateArray = rawValue.Split(',');
            string[] parts = (dateArray.Length > 1 ? dateArray[1] : dateArray[0]).Split('-');

            if (parts.Length == 0)
            {
                DateValue = StartValue = EndValue = null;
                return true;
            }

            // Lấy giá trị hiện tại từ ViewState (đã là setting time)
            DateTime? tempStartValue = StartValueForDisplay;
            DateTime? tempEndValue = EndValueForDisplay;
            DateTime? tempDateValue = DateValueForDisplay;

            if (parts.Length == 2)
            {
                isChanged |= SetDateFromClient(ref tempStartValue, parts[0]);
                isChanged |= SetDateFromClient(ref tempEndValue, parts[1]);
                tempDateValue = tempStartValue;
            }
            else if (parts.Length == 1)
            {
                isChanged |= SetDateFromClient(ref tempDateValue, parts[0]);
                tempStartValue = tempDateValue;
            }

            // Convert sang UTC khi set vào properties chính
            if (tempStartValue.HasValue)
                StartValue = DateTimeConverter?.ConvertSettingTimeToUtc(tempStartValue.Value);
            else
                StartValue = null;

            if (tempEndValue.HasValue)
                EndValue = DateTimeConverter?.ConvertSettingTimeToUtc(tempEndValue.Value);
            else
                EndValue = null;

            if (tempDateValue.HasValue)
                DateValue = DateTimeConverter?.ConvertSettingTimeToUtc(tempDateValue.Value);
            else
                DateValue = null;

            return isChanged;
        }
        private bool SetDateFromClient(ref DateTime? currentValue, string newValueRaw)
        {
            string newValueStr = newValueRaw?.Trim();
            if (string.IsNullOrEmpty(newValueStr) || newValueStr.ToLower() == "invalid date")
            {
                if (currentValue.HasValue)
                {
                    currentValue = null;
                    return true;
                }
                return false;
            }

            DateTime parsedValue;
            string[] allowedFormats = new[] { DateFormat, "dd/MM/yyyy", "dd/MM/yyyy HH:mm", "MM/dd/yyyy", "MM/dd/yyyy HH:mm" };
            var culture = new System.Globalization.CultureInfo(CultureInfo.CurrentCulture.Name);

            bool parsed = DateTime.TryParseExact(
                newValueStr, allowedFormats, culture,
                System.Globalization.DateTimeStyles.None, out parsedValue
            );

            if (!parsed)
            {
                try
                {
                    parsedValue = jss.Deserialize<DateTime>($"\"{newValueStr}\"");
                }
                catch
                {
                    return false;
                }
            }

            // So sánh với giá trị hiện tại
            if (!currentValue.HasValue || currentValue.Value != parsedValue)
            {
                currentValue = parsedValue; // Đây là setting time từ client
                return true;
            }

            return false;
        }
        PostBackOptions GetPostBackOptions()
        {
            PostBackOptions options = new PostBackOptions(this);
            options.ActionUrl = null;
            options.ValidationGroup = null;
            options.Argument = String.Empty;
            options.RequiresJavaScriptProtocol = false;
            options.ClientSubmit = true;

            return options;
        }
        private string GetFormat(DateTime dt)
        {
            if (DateTimeHelper.IsEnglish)
            {
                if (TimePicker24Hour)
                    return dt.ToString("MM/dd/yyyy HH:mm");
                if (TimePickerSecond)
                    return dt.ToString("MM/dd/yyyy HH:mm:ss");
                return dt.ToString("MM/dd/yyyy");
            }
            else
            {
                if (TimePicker24Hour)
                    return dt.ToString("dd/MM/yyyy HH:mm");
                if (TimePickerSecond)
                    return dt.ToString("dd/MM/yyyy HH:mm:ss");
                return dt.ToString("dd/MM/yyyy");
            }
        }
        private string GetFormatDisplay(DateTime dt, bool hasTime)
        {
            if (DateTimeHelper.IsEnglish)
            {
                if (hasTime)
                    return dt.ToString("dd MMM yyyy HH:mm:ss");
                return dt.ToString("dd MMM yyyy");
            }
            else
            {
                if (hasTime)
                    return dt.ToString("dd/MM/yyyy HH:mm:ss");
                return dt.ToString("dd/MM/yyyy");
            }
        }
    }

    /// <summary>
    /// Arguments of the DateChanged event.
    /// </summary>
    public class DateChangedEventArgs : EventArgs
    {
        private DateTime? _StartDateValue;
        private DateTime? _EndDateValue;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="date"></param>
        public DateChangedEventArgs(DateTime? StartDateValue, DateTime? EndDateValue)
        {
            this._StartDateValue = StartDateValue;
            this._EndDateValue = EndDateValue;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="date"></param>
        public DateChangedEventArgs(DateTime? StartDateValue)
        {
            this._StartDateValue = StartDateValue;
            this._EndDateValue = null;
        }

        /// <summary>
        /// Get the date 1.
        /// </summary>
        public DateTime? StartDateValue
        {
            get { return _StartDateValue; }
        }

        /// <summary>
        /// Get the date 2.
        /// </summary>
        public DateTime? EndDateValue
        {
            get { return _EndDateValue; }
        }
    }

    public enum ModeDate
    {
        [Render("single")]
        Single,
        [Render("multiple")]
        Multiple,
        [Render("range")]
        Range
    }

    public enum OpenDates
    {
        [Render("auto")]
        Auto,
        [Render("left")]
        Left,
        [Render("center")]
        Center,
        [Render("right")]
        Right
    }
    public enum DropDates
    {
        [Render("auto")]
        Auto,
        [Render("down")]
        Down,
        [Render("up")]
        Up
    }

    public enum MonthSelectorTypeDate
    {
        [Render("dropdown")]
        Dropdown,
        [Render("static")]
        Static,
    }
}
