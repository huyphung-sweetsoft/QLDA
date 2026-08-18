using SweetSoft.QLDA.Core.PdfGeneration.Interfaces;
using SweetSoft.QLDA.Core.PdfGeneration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.PdfGeneration.Utilities
{
    /// <summary>
    /// Writes generated PDF content to an <see cref="HttpResponse"/> instance.
    /// </summary>
    public class PdfResponseWriter : IPdfResponseWriter
    {
        /// <inheritdoc />
        public void WritePdf(HttpResponse response, PdfDocument document, bool asAttachment = true)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            response.Clear();
            response.Buffer = true;
            response.ContentType = document.ContentType;
            response.Charset = string.Empty;
            response.AddHeader("Content-Length", document.Content.Length.ToString());

            var disposition = asAttachment ? "attachment" : "inline";
            response.AddHeader("Content-Disposition", $"{disposition}; filename=\"{document.FileName}\"");
            response.BinaryWrite(document.Content);
            response.Flush();
        }
    }
}
