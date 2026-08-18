using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace SweetSoft.QLDA.Controls.Helpers
{
    internal static class HtmlRender
    {
        public static void WriteHtmlElement(this HtmlTextWriter output, HtmlTextWriterTag element, string elementContent, params HtmlAttribute[] attributes)
        {
            if (attributes != null && attributes.Length > 0)
            {
                foreach (HtmlAttribute attribute in attributes)
                {
                    if (attribute != null)
                        output.AddAttribute(attribute.Name, attribute.Value);
                }
            }

            if (element.ToString().Length > 0)
                output.RenderBeginTag(element);

            output.Write(elementContent);

            if (element.ToString().Length > 0)
                output.RenderEndTag();
        }

        public static void WriteHtmlElement(this HtmlTextWriter output, HtmlElement element, bool? renderEndTag = true)
        {
            if (!element.IsRendered)
                return;

            if (renderEndTag.HasValue == false)
                renderEndTag = true;

            if (element.Attributes != null && element.Attributes.Count > 0)
            {
                foreach (HtmlAttribute attribute in element.Attributes)
                {
                    if (attribute != null && attribute.IsRendered)
                        output.AddAttribute(attribute.Name, attribute.Value);
                }
            }

            if (element.Name.Length > 0)
                output.RenderBeginTag(element.Name);

            if (element.Elements != null && element.Elements.Count > 0)
            {
                foreach (HtmlElement childElement in element.Elements)
                {
                    if (childElement != null && childElement.IsRendered)
                        output.WriteHtmlElement(childElement, null);
                }
            }

            if (element.Controls != null && element.Controls.Count > 0)
            {
                foreach (Control control in element.Controls)
                {
                    if (control != null && control.Visible)
                        control.RenderControl(output);
                }
            }

            if (element.Name.Length > 0)
            {
                if (renderEndTag.HasValue && renderEndTag.Value == true)
                    output.RenderEndTag();
            }
        }
    }
}
