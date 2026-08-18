using SweetSoft.QLDA.Core.PdfGeneration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Configuration
{
    /// <summary>
    /// Represents the data required to perform a PDF generation operation.
    /// </summary>
    public class PdfGenerationRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerationRequest"/> class.
        /// </summary>
        /// <param name="contentProvider">Component capable of providing the HTML content.</param>
        public PdfGenerationRequest(IHtmlContentProvider contentProvider)
        {
            ContentProvider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));
        }

        /// <summary>
        /// Gets the provider responsible for supplying the HTML content.
        /// </summary>
        public IHtmlContentProvider ContentProvider { get; }

        /// <summary>
        /// Gets or sets the generation options.
        /// </summary>
        public PdfGenerationOptions Options { get; set; } = new PdfGenerationOptions();

        /// <summary>
        /// Gets or sets the suggested output file name.
        /// </summary>
        public string FileName { get; set; } = "document.pdf";

        /// <summary>
        /// Gets or sets the MIME type that represents the produced document.
        /// </summary>
        public string ContentType { get; set; } = "application/pdf";
    }
}
