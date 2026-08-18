using NReco.PdfGenerator;
using SweetSoft.QLDA.Core.PdfGeneration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Providers
{
    /// <summary>
    /// Provides a configurable factory for creating <see cref="HtmlToPdfConverter"/> instances.
    /// </summary>
    public class DefaultHtmlToPdfConverterFactory : IHtmlToPdfConverterFactory
    {
        private readonly Action<HtmlToPdfConverter> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHtmlToPdfConverterFactory"/> class.
        /// </summary>
        /// <param name="configure">Optional delegate for configuring newly created instances.</param>
        public DefaultHtmlToPdfConverterFactory(Action<HtmlToPdfConverter> configure = null)
        {
            _configure = configure;
        }

        /// <inheritdoc />
        public HtmlToPdfConverter Create()
        {
            var converter = new HtmlToPdfConverter();
            _configure?.Invoke(converter);
            return converter;
        }
    }
}
