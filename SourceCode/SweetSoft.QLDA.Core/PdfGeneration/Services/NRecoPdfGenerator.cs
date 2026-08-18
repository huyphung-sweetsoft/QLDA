using NReco.PdfGenerator;
using SweetSoft.QLDA.Core.PdfGeneration.Configuration;
using SweetSoft.QLDA.Core.PdfGeneration.Interfaces;
using SweetSoft.QLDA.Core.PdfGeneration.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.PdfGeneration.Services
{
    /// <summary>
    /// Generates PDF documents using the NReco wrapper around wkhtmltopdf.
    /// </summary>
    public class NRecoPdfGenerator : IPdfGenerator
    {
        private readonly IHtmlToPdfConverterFactory _converterFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NRecoPdfGenerator"/> class.
        /// </summary>
        /// <param name="converterFactory">Factory used to instantiate configured <see cref="HtmlToPdfConverter"/> objects.</param>
        public NRecoPdfGenerator(IHtmlToPdfConverterFactory converterFactory)
        {
            _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
        }

        /// <inheritdoc />
        public PdfDocument Generate(PdfGenerationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                var converter = _converterFactory.Create();
                ApplyOptions(converter, request.Options);

                var html = request.ContentProvider.GetHtml();
                var baseUrl = request.ContentProvider.BaseUrl;

                var pdfBytes = string.IsNullOrWhiteSpace(baseUrl)
                    ? converter.GeneratePdf(html)
                    : converter.GeneratePdf(html, baseUrl);

                return new PdfDocument(pdfBytes, request.FileName, request.ContentType);
            }
            catch (Exception ex)
            {
                throw new PdfGenerationException("An error occurred while generating the PDF document.", ex);
            }
        }

        private static void ApplyOptions(HtmlToPdfConverter converter, PdfGenerationOptions options)
        {
            if (options == null)
            {
                return;
            }

            ApplyPageOptions(converter, options.Page);

            if (!string.IsNullOrWhiteSpace(options.HeaderHtml))
            {
                converter.PageHeaderHtml = options.HeaderHtml;
            }

            if (!string.IsNullOrWhiteSpace(options.FooterHtml))
            {
                converter.PageFooterHtml = options.FooterHtml;
            }

            if (options.Zoom.HasValue)
            {
                converter.Zoom = options.Zoom.Value;
            }

            ApplyHttpConfiguration(converter, options);
            ApplyAdditionalArguments(converter, options.AdditionalWkHtmlArguments);
            ApplyCompressionIfAvailable(converter, options.EnableCompression);
        }

        private static void ApplyPageOptions(HtmlToPdfConverter converter, PdfPageSettings page)
        {
            if (page == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(page.Size) && Enum.TryParse(page.Size, true, out PageSize wkSize))
            {
                converter.Size = wkSize;
            }

            if (!string.IsNullOrWhiteSpace(page.Orientation) && Enum.TryParse(page.Orientation, true, out PageOrientation orientation))
            {
                converter.Orientation = orientation;
            }

            if (page.Margins != null)
            {
                converter.Margins = new PageMargins
                {
                    Top = page.Margins.Top,
                    Bottom = page.Margins.Bottom,
                    Left = page.Margins.Left,
                    Right = page.Margins.Right
                };
            }

            if (page.Width.HasValue)
            {
                converter.PageWidth = page.Width.Value;
            }

            if (page.Height.HasValue)
            {
                converter.PageHeight = page.Height.Value;
            }
        }

        private static void ApplyHttpConfiguration(HtmlToPdfConverter converter, PdfGenerationOptions options)
        {
            if (options.HttpHeaders.Any())
            {
                if (converter.GetType().GetProperty("HttpHeaders")?.GetValue(converter) is IDictionary<string, string> httpHeaders)
                {
                    foreach (var header in options.HttpHeaders)
                    {
                        httpHeaders[header.Key] = header.Value;
                    }
                }
                else
                {
                    foreach (var header in options.HttpHeaders)
                    {
                        converter.CustomWkHtmlArgs = AppendCommandLineArgument(converter.CustomWkHtmlArgs, FormatHeaderArgument(header.Key, header.Value));
                    }
                }
            }
        }

        private static void ApplyAdditionalArguments(HtmlToPdfConverter converter, IList<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return;
            }

            var sanitizedArgs = arguments
                .Where(arg => !string.IsNullOrWhiteSpace(arg))
                .Select(arg => arg.Trim());

            foreach (var argument in sanitizedArgs)
            {
                converter.CustomWkHtmlArgs = AppendCommandLineArgument(converter.CustomWkHtmlArgs, argument);
            }
        }

        private static string AppendCommandLineArgument(string existingArgs, string argument)
        {
            if (string.IsNullOrWhiteSpace(existingArgs))
            {
                return argument;
            }

            return string.Join(" ", new[] { existingArgs, argument });
        }

        private static string FormatHeaderArgument(string name, string value)
        {
            var escapedValue = value?.Replace("\"", "\\\"") ?? string.Empty;
            return string.Format(CultureInfo.InvariantCulture, "--custom-header \"{0}\" \"{1}\"", name, escapedValue);
        }

        private static void ApplyCompressionIfAvailable(HtmlToPdfConverter converter, bool enableCompression)
        {
            var property = converter.GetType().GetProperty("EnableCompression");
            if (property != null && property.CanWrite)
            {
                property.SetValue(converter, enableCompression);
            }
        }
    }
}
