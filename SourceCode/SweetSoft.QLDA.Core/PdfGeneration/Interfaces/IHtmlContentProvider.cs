using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Interfaces
{
    /// <summary>
    /// Defines the contract for components that can supply HTML content for PDF generation.
    /// </summary>
    public interface IHtmlContentProvider
    {
        /// <summary>
        /// Gets the HTML markup that should be converted to PDF.
        /// </summary>
        /// <returns>The HTML string.</returns>
        string GetHtml();

        /// <summary>
        /// Optionally gets the base URL that should be used for resolving relative resources.
        /// </summary>
        string BaseUrl { get; }
    }
}
