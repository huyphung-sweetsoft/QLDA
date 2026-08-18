using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls.Helpers
{
    internal static class Extensions
    {
        public static bool HasKey(this StateBag bag, string key)
        {
            foreach (string bagKey in bag.Keys)
            {
                if (bagKey == key)
                    return true;
            }
            return false;
        }

        public static T GetValue<T>(this StateBag bag, string key, T defaultValue) where T : class
        {
            if (!bag.HasKey(key))
                return defaultValue;

            return bag[key] as T;
        }

        public static void AddRange(this ControlCollection thisCollection, ControlCollection collection)
        {
            foreach (Control control in collection)
            {
                thisCollection.Add(control);
            }
        }

        public static string ToRender(this Enum enumerator)
        {
            Type type = enumerator.GetType();
            try
            {
                System.Reflection.FieldInfo fieldInfo = type.GetField(enumerator.ToString());
                RenderAttribute attribute = fieldInfo.GetCustomAttributes(typeof(RenderAttribute), false).FirstOrDefault() as RenderAttribute;
                if (attribute == null)
                    return enumerator.ToString();

                return attribute.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool HasAttribute(Type what, Type which)
        {
            return !(Attribute.GetCustomAttribute(what, which) == null);
        }

        public static List<HtmlAttribute> GetListAttributes(this WebControl control)
        {
            List<HtmlAttribute> lstReturn = new List<HtmlAttribute>();
            if (control.Attributes != null && control.Attributes.Count > 0)
            {
                foreach (string key in control.Attributes.Keys)
                {
                    if (lstReturn.Any(x => x.Name.ToLower() == key.ToLower()) == false)
                        lstReturn.Add(new HtmlAttribute(key, control.Attributes[key], null));
                }
            }
            if (control.ToolTip != null && control.ToolTip.Length > 0)
                lstReturn.Add(new HtmlAttribute("title", control.ToolTip, null));
            return lstReturn;
        }

    }
}
