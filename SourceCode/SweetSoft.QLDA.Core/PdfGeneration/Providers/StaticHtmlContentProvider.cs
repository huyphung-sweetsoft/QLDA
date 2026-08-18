using SweetSoft.QLDA.Core.PdfGeneration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Providers
{
    /// <summary>
    /// Provides HTML content supplied by the caller, typically for one-off conversions.
    /// </summary>
    public class StaticHtmlContentProvider : IHtmlContentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticHtmlContentProvider"/> class.
        /// </summary>
        /// <param name="html">The HTML markup that should be converted.</param>
        /// <param name="baseUrl">Optional base URL used to resolve relative paths.</param>
        public StaticHtmlContentProvider(string html, string baseUrl = null)
        {
            Html = !string.IsNullOrWhiteSpace(html)
                ? html
                : throw new ArgumentException("HTML content cannot be null or whitespace.", nameof(html));
            BaseUrl = baseUrl;
        }

        /// <inheritdoc />
        public string GetHtml() => Html;

        /// <inheritdoc />
        public string BaseUrl { get; }

        /// <summary>
        /// Gets the raw HTML string.
        /// </summary>
        protected string Html { get; }
    }
}
