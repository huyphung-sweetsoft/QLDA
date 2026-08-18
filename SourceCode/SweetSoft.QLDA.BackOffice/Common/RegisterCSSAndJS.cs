using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public class RegisterCSSAndJS
    {
        #region Private Fields
        private string _cssPlaceHolder;
        private string _jsPlaceHolder;
        private List<string> _cssLinks;
        private List<string> _jsLinks;
        private bool _ready;
        private List<string> _keyControls = new List<string>();
        #endregion
        #region Private Methods
        private void init(string cssPlaceHolder, string jsPlaceHolder, List<string> cssLinks, List<string> jsLinks)
        {
            _cssPlaceHolder = cssPlaceHolder;
            _jsPlaceHolder = jsPlaceHolder;
            _cssLinks = cssLinks;
            _jsLinks = jsLinks;
            _ready = !((cssLinks == null || cssLinks.Count == 0) && (jsLinks == null || jsLinks.Count == 0));
        }
        #endregion
        #region Public Constructors
        public RegisterCSSAndJS(string cssPlaceHolder, string jsPlaceHolder, List<string> cssLinks, List<string> jsLinks)
        {
            init(cssPlaceHolder, jsPlaceHolder, cssLinks, jsLinks);
        }
        public RegisterCSSAndJS() : this(string.Empty, string.Empty, null, null) { }
        #endregion
        #region Public Methods
        public void Register()
        {
            lock (_keyControls)
            {
                if (!_ready)
                    return;

                Page page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;
                if (page == null)
                    return;

                // Define the resource name and type.
                Type thisT = this.GetType().BaseType;

                // Get a ClientScriptManager reference from the Page class.
                ClientScriptManager cs = page.ClientScript;
                #region css
                if (_cssLinks != null && _cssLinks.Count > 0)
                    foreach (string cssLink in _cssLinks)
                    {
                        string baseCSS = page.ResolveClientUrl(cssLink);
                        // Check to see if the startup script is already registered.
                        if (!string.IsNullOrEmpty(baseCSS) && !_keyControls.Exists(t => t == baseCSS))
                        {
                            _keyControls.Add(baseCSS);
                            if (!cs.IsStartupScriptRegistered(thisT, baseCSS))
                            {
                                string strCss = "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + baseCSS + "\" />";
                                LiteralControl link = new LiteralControl();
                                link.EnableViewState = false;
                                link.Text = strCss;

                                //if (string.IsNullOrEmpty(_cssPlaceHolder))
                                //    page.Header.Controls.Add(link);
                                //else
                                //{
                                ContentPlaceHolder cpCss = page.Header.FindControl(_cssPlaceHolder) as ContentPlaceHolder;
                                if (cpCss == null)
                                    cpCss = page.FindControl(_cssPlaceHolder) as ContentPlaceHolder;
                                if (cpCss != null)
                                {
                                    cpCss.Controls.Add(link);
                                    ScriptManager.RegisterStartupScript(page, thisT, baseCSS, "", true);
                                }
                                //else
                                //    page.Header.Controls.Add(link);
                                //}
                            }
                        }
                    }
                #endregion

                #region js
                if (_jsLinks != null && _jsLinks.Count > 0)
                    foreach (string jsLink in _jsLinks)
                    {
                        string baseJS = page.ResolveClientUrl(jsLink);
                        if (!string.IsNullOrEmpty(baseJS) && !_keyControls.Exists(t => t == baseJS))
                        {
                            _keyControls.Add(baseJS);
                            if (!cs.IsStartupScriptRegistered(thisT, baseJS))
                            {
                                string strJs = "<script src='" + baseJS + "'></script> ";
                                // Register the client resource with the page.
                                //if (string.IsNullOrEmpty(_jsPlaceHolder))
                                //    ScriptManager.RegisterStartupScript(page, thisT, baseJS, strJs, false);
                                //else
                                //{
                                ContentPlaceHolder cpJs = page.Header.FindControl(_jsPlaceHolder) as ContentPlaceHolder;
                                if (cpJs == null)
                                    cpJs = page.FindControl(_jsPlaceHolder) as ContentPlaceHolder;
                                if (cpJs != null)
                                {
                                    cpJs.Controls.Add(new LiteralControl(strJs));
                                    ScriptManager.RegisterStartupScript(page, thisT, baseJS, "", true);
                                }
                                //else
                                //    ScriptManager.RegisterStartupScript(page, thisT, baseJS, strJs, false);
                                //}
                            }
                        }
                    }
                #endregion
            }
        }
        #endregion
    }
}