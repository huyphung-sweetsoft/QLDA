using SweetSoft.QLDA.Core.PdfGeneration.Configuration;
using SweetSoft.QLDA.Core.PdfGeneration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Interfaces
{
    /// <summary>
    /// Defines the contract for components capable of transforming HTML content into a PDF document.
    /// </summary>
    public interface IPdfGenerator
    {
        /// <summary>
        /// Generates a PDF document using the specified <paramref name="request" />.
        /// </summary>
        /// <param name="request">Encapsulates the HTML content and generation options.</param>
        /// <returns>A <see cref="PdfDocument"/> that represents the generated PDF file.</returns>
        PdfDocument Generate(PdfGenerationRequest request);
    }
}
