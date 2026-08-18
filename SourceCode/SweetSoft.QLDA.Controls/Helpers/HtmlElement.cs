using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace SweetSoft.QLDA.Controls.Helpers
{
    internal class HtmlElement
    {
        public bool IsRendered { get; private set; }
        public string Name { get; private set; }

        private List<HtmlAttribute> _attributes;
        public List<HtmlAttribute> Attributes
        {
            get
            {
                if (_attributes == null)
                    _attributes = new List<HtmlAttribute>();

                return _attributes;
            }
        }

        private List<HtmlElement> _elements;
        public List<HtmlElement> Elements
        {
            get
            {
                if (_elements == null)
                    _elements = new List<HtmlElement>();

                return _elements;
            }
        }

        private List<Control> _controls;
        public List<Control> Controls
        {
            get
            {
                if (_controls == null)
                    _controls = new List<Control>();

                return _controls;
            }
        }
        public HtmlElement(HtmlTextWriterTag type = HtmlTextWriterTag.Div)
            : this(string.Format("{0}", type))
        { }

        public HtmlElement(HtmlTextWriterTag type = HtmlTextWriterTag.Div, string className = "", string id = "", string content = null, HtmlElement[] elements = null, HtmlAttribute[] attributes = null, bool isRendered = true, Control[] controls = null)
            : this(string.Format("{0}", type), className, id, content, elements, attributes, isRendered, controls)
        { }

        public HtmlElement(string name, string className = "", string id = "", string content = "",
            HtmlElement[] elements = null, HtmlAttribute[] attributes = null, bool isRendered = true, Control[] controls = null)
        {
            if (string.IsNullOrEmpty(name))
                name = string.Format("{0}", HtmlTextWriterTag.Div);

            this.Name = name;
            this.IsRendered = isRendered;

            if (!string.IsNullOrEmpty(className))
                this.Attributes.Add(new HtmlClassAttribute(className));

            if (!string.IsNullOrEmpty(id))
                this.Attributes.Add(new HtmlIdAttribute(id));

            if (elements != null && elements.Length > 0)
                this.Elements.AddRange(elements);

            if (attributes != null && attributes.Length > 0)
                this.Attributes.AddRange(attributes);

            if (content != null)
                this.Controls.Add(new LiteralControl(content));

            if (controls != null && controls.Length > 0)
                this.Controls.AddRange(controls);

        }

        public static string MergeScript(string firstScript, string secondScript)
        {
            if (string.IsNullOrEmpty(firstScript))
            {
                if (secondScript.Length > 0)
                {
                    if (secondScript.StartsWith("javascript:"))
                        return secondScript;
                    else
                        return ("javascript:" + secondScript);
                }
            }

            if (!firstScript.StartsWith("javascript:"))
                firstScript = "javascript:" + firstScript;

            if (secondScript.StartsWith("javascript:"))
                secondScript = secondScript.Substring("javascript:".Length);

            if (!string.IsNullOrEmpty(firstScript)) return (firstScript + (firstScript.EndsWith(";") ? "" : ";") + secondScript);
            if (secondScript.TrimStart(new char[0]).StartsWith("javascript:", StringComparison.Ordinal))
            {
                return secondScript + ";";
            }
            if (!secondScript.EndsWith(";"))
                secondScript = secondScript + ";";
            return string.Empty;
        }
    }

    internal class HtmlCaretElement : HtmlElement
    {
        public HtmlCaretElement(bool isRendered)
            : base(string.Format("{0}", HtmlTextWriterTag.Button), string.Empty, string.Empty,
            "<span class='caret'></span>", null, null, isRendered, null)
        {
        }
    }

    internal class HtmlAddonElement
    {
        public static void Render(HtmlTextWriter writer, string addon, bool? renderEndTag)
        {
            if (renderEndTag.HasValue == false)
                renderEndTag = true;

            writer.WriteHtmlElement(new HtmlElement(null, string.Empty,
                string.Empty, string.Empty, new HtmlElement[] {
                            new HtmlElement(
                            string.Format("{0}",HtmlTextWriterTag.Span),string.Empty,string.Empty,addon,
                            null,
                            new HtmlAttribute[] {
                                new HtmlClassAttribute("input-group-addon")
                            },
                            false,null
                        )
                    },
                        new HtmlAttribute[] { new HtmlClassAttribute("input-group") },
                        false, null
              ), renderEndTag);
        }
    }
}
