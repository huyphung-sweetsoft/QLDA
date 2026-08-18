using NReco.PdfGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Interfaces
{
    /// <summary>
    /// Provides a seam for creating <see cref="HtmlToPdfConverter"/> instances.
    /// </summary>
    public interface IHtmlToPdfConverterFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="HtmlToPdfConverter"/> configured with default values.
        /// </summary>
        /// <returns>A configured <see cref="HtmlToPdfConverter"/>.</returns>
        HtmlToPdfConverter Create();
    }
}
