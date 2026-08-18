using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Models
{
    /// <summary>
    /// Represents a generated PDF document and its metadata.
    /// </summary>
    public class PdfDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDocument"/> class.
        /// </summary>
        /// <param name="content">Binary content of the generated PDF.</param>
        /// <param name="fileName">Suggested file name when downloading the PDF.</param>
        /// <param name="contentType">The MIME type of the generated document.</param>
        public PdfDocument(byte[] content, string fileName = null, string contentType = "application/pdf")
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            FileName = string.IsNullOrWhiteSpace(fileName) ? "document.pdf" : fileName;
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType;
        }

        /// <summary>
        /// Gets the generated binary content.
        /// </summary>
        public byte[] Content { get; }

        /// <summary>
        /// Gets the file name recommended for the client.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the MIME type of the document.
        /// </summary>
        public string ContentType { get; }
    }
}
