using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Configuration
{
    /// <summary>
    /// Represents margins applied to the generated PDF document.
    /// </summary>
    public class PdfMargins
    {
        /// <summary>
        /// Gets a default margin configuration of 10 millimeters on each side.
        /// </summary>
        public static PdfMargins Default => new PdfMargins();

        /// <summary>
        /// Gets or sets the top margin in millimeters.
        /// </summary>
        public float Top { get; set; } = 10;

        /// <summary>
        /// Gets or sets the bottom margin in millimeters.
        /// </summary>
        public float Bottom { get; set; } = 10;

        /// <summary>
        /// Gets or sets the left margin in millimeters.
        /// </summary>
        public float Left { get; set; } = 10;

        /// <summary>
        /// Gets or sets the right margin in millimeters.
        /// </summary>
        public float Right { get; set; } = 10;
    }
}
