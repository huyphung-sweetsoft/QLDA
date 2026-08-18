using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Models
{
    /// <summary>
    /// Represents failures that occur while converting HTML into PDF documents.
    /// </summary>
    public class PdfGenerationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerationException"/> class.
        /// </summary>
        public PdfGenerationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerationException"/> class with a specified error message.
        /// </summary>
        public PdfGenerationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerationException"/> class with a specified error message and inner exception.
        /// </summary>
        public PdfGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
