using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Configuration
{
    /// <summary>
    /// Encapsulates configuration options for the HTML to PDF conversion process.
    /// </summary>
    public class PdfGenerationOptions
    {
        /// <summary>
        /// Gets or sets page related settings.
        /// </summary>
        public PdfPageSettings Page { get; set; } = PdfPageSettings.Default;

        /// <summary>
        /// Gets or sets the HTML content that will be rendered in the document header.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML content that will be rendered in the document footer.
        /// </summary>
        public string FooterHtml { get; set; }

        /// <summary>
        /// Gets or sets the zoom factor applied during rendering.
        /// </summary>
        public float? Zoom { get; set; }

        /// <summary>
        /// Gets or sets additional HTTP headers sent when fetching external resources.
        /// </summary>
        public IDictionary<string, string> HttpHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a collection of extra command line arguments passed to wkhtmltopdf.
        /// </summary>
        public IList<string> AdditionalWkHtmlArguments { get; } = new List<string>();

        /// <summary>
        /// Gets or sets a flag indicating whether the generated PDF should be compressed.
        /// </summary>
        public bool EnableCompression { get; set; } = true;
    }
}
