using SweetSoft.QLDA.Core.PdfGeneration.Configuration;
using SweetSoft.QLDA.Core.PdfGeneration.Interfaces;
using SweetSoft.QLDA.Core.PdfGeneration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.PdfGeneration.Services
{
    /// <summary>
    /// Provides a high level service that orchestrates HTML to PDF conversion and response writing.
    /// </summary>
    public class PdfGenerationService
    {
        private readonly IPdfGenerator _generator;
        private readonly IPdfResponseWriter _responseWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerationService"/> class.
        /// </summary>
        /// <param name="generator">Underlying generator responsible for producing PDF documents.</param>
        /// <param name="responseWriter">Utility used to write a generated PDF into the HTTP response.</param>
        public PdfGenerationService(IPdfGenerator generator, IPdfResponseWriter responseWriter)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _responseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
        }

        /// <summary>
        /// Generates a PDF document using the provided <paramref name="request"/>.
        /// </summary>
        public PdfDocument Generate(PdfGenerationRequest request)
        {
            return _generator.Generate(request);
        }

        /// <summary>
        /// Generates a PDF document and writes it to the <paramref name="response"/>.
        /// </summary>
        public void GenerateAndWrite(HttpResponse response, PdfGenerationRequest request, bool asAttachment = true)
        {
            var document = Generate(request);
            _responseWriter.WritePdf(response, document, asAttachment);
        }
    }
}
