using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Configuration
{
    /// <summary>
    /// Describes PDF page layout related settings.
    /// </summary>
    public class PdfPageSettings
    {
        /// <summary>
        /// Gets a default <see cref="PdfPageSettings"/> instance representing an A4 portrait page.
        /// </summary>
        public static PdfPageSettings Default => new PdfPageSettings();

        /// <summary>
        /// Gets or sets the expected page size. Use values compatible with wkhtmltopdf (e.g. A4, Letter).
        /// </summary>
        public string Size { get; set; } = "A4";

        /// <summary>
        /// Gets or sets the orientation. Valid values are "Portrait" or "Landscape".
        /// </summary>
        public string Orientation { get; set; } = "Portrait";

        /// <summary>
        /// Gets or sets the page width in millimeters. When set together with <see cref="Height"/> the converter switches to a custom page size.
        /// </summary>
        public float? Width { get; set; }

        /// <summary>
        /// Gets or sets the page height in millimeters. When set together with <see cref="Width"/> the converter switches to a custom page size.
        /// </summary>
        public float? Height { get; set; }

        /// <summary>
        /// Gets or sets the page margin configuration.
        /// </summary>
        public PdfMargins Margins { get; set; } = PdfMargins.Default;
    }
}
