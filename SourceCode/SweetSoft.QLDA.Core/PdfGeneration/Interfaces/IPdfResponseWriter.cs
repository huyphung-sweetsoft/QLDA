using SweetSoft.QLDA.Core.PdfGeneration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.PdfGeneration.Interfaces
{
    /// <summary>
    /// Abstraction responsible for writing a generated PDF into the ASP.NET response pipeline.
    /// </summary>
    public interface IPdfResponseWriter
    {
        /// <summary>
        /// Writes the provided <paramref name="document"/> to the given <paramref name="response"/>.
        /// </summary>
        /// <param name="response">The current HTTP response.</param>
        /// <param name="document">The generated PDF document.</param>
        /// <param name="asAttachment">Determines whether the PDF should be downloaded (attachment) or displayed inline.</param>
        void WritePdf(HttpResponse response, PdfDocument document, bool asAttachment = true);
    }
}
