using SweetSoft.QLDA.Core.PdfGeneration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Providers
{
    /// <summary>
    /// Uses a delegate to lazily obtain HTML content for PDF generation.
    /// </summary>
    public class DelegateHtmlContentProvider : IHtmlContentProvider
    {
        private readonly Func<string> _contentFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateHtmlContentProvider"/> class.
        /// </summary>
        /// <param name="contentFactory">Delegate responsible for producing the HTML markup.</param>
        /// <param name="baseUrl">Optional base URL used to resolve relative paths.</param>
        public DelegateHtmlContentProvider(Func<string> contentFactory, string baseUrl = null)
        {
            _contentFactory = contentFactory ?? throw new ArgumentNullException(nameof(contentFactory));
            BaseUrl = baseUrl;
        }

        /// <inheritdoc />
        public string GetHtml()
        {
            var html = _contentFactory();
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new InvalidOperationException("The provided delegate returned null or whitespace HTML content.");
            }

            return html;
        }

        /// <inheritdoc />
        public string BaseUrl { get; }
    }
}
