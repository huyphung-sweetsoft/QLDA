using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public class BindingExtraControls
    {
        public static void BindDropdownEnum<TEnum>(ExtraDropdown dropdown, bool isAll = false, List<string> excludedFields = null) where TEnum : struct, Enum
        {
            dropdown.Items.Clear();
            if (isAll)
            {
                dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL), " "));
            }

            excludedFields = excludedFields ?? new List<string>();

            foreach (var enumValue in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (excludedFields.Contains(enumValue.ToString()))
                {
                    continue;
                }

                string displayName = enumValue.GetDisplayText();
                string value = Convert.ToInt32(enumValue).ToString();
                if (displayName == value)
                    displayName = UITextsReader.GetBackEndResourceText(displayName.ToUpper());
                dropdown.Items.Add(new ListItem(displayName, value));
            }

            dropdown.SelectedIndex = isAll ? -1 : 0;
        }
        public static void BindDropdownEnum<TEnum>(BootstrapDropdown dropdown, List<string> excludedFields = null) where TEnum : struct, Enum
        {
            dropdown.Items.Clear();
            excludedFields = excludedFields ?? new List<string>();

            foreach (var enumValue in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (excludedFields.Contains(enumValue.ToString()))
                {
                    continue;
                }

                string displayName = enumValue.GetDisplayText();
                string value = Convert.ToInt32(enumValue).ToString();
                dropdown.AddItem(displayName, value);
            }

            dropdown.SelectedIndex = -1;
        }

        public static void BindFunctionCode(ref ExtraDropdown dropdown, bool isAll = false, List<string> excludedFields = null)
        {
            dropdown.Items.Clear();
            if (isAll)
                dropdown.Items.Add(new ListItem(UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ALL), " "));

            foreach (ModuleKeys suit in (ModuleKeys[])Enum.GetValues(typeof(ModuleKeys)))
            {
                dropdown.Items.Add(new ListItem(suit.ToString(), suit.ToString()));
            }
            dropdown.SelectedIndex = -1;
        }
        public static void BindLanguagesDropdown(ExtraDropdown ddl, string currentLangId)
        {
            string textEN = LanguageHelpers.LanguageName[LanguageHelpers.English];
            byte langId = SweetContext.Current.CurrentLanguageId;
            if (langId == LanguageHelpers.English)
            {
                textEN = LanguageHelpers.LanguageText[LanguageHelpers.English];
            }
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(textEN, "1"));
            ddl.SelectedValue = currentLangId;
        }
    }
}