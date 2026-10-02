using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NReco.PdfGenerator;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
namespace SweetSoft.QLDA.Core.Managers
{
    public class PdfManager
    {
        private static PdfManager _instance;
        public static PdfManager Instance
        {
            get
            {
                if (_instance == null) _instance = new PdfManager();
                return _instance;
            }
        }
        private PdfManager() { }
        public void ExportHtmlToPdf(string htmlContent, string fileName, HttpResponse Response)
        {
            try
            {
                byte[] pdfBytes = GeneratePdf(htmlContent);
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(pdfBytes);
                Response.Flush();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo file PDF: " + ex.Message, ex);
            }
        }
        public byte[] GeneratePdf(string htmlContent)
        {
            try
            {
                var htmlToPdf = new HtmlToPdfConverter();
                htmlToPdf.PdfToolPath = System.IO.Path.GetTempPath();
                htmlToPdf.Size = PageSize.A4;
                htmlToPdf.Margins = new PageMargins { Top = 8, Bottom = 8, Left = 8, Right = 8 };
                string finalHtml = @"<!DOCTYPE html>
                    <html>
                    <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                    <style>
                    html, body { margin: 0; padding: 0; font-family: Arial, sans-serif; font-size: 13px; line-height: 1.45; color: #334155; }
                    body { word-wrap: break-word; }
                    img { max-width: 100%; height: auto; }
                    svg { max-width: 100%; }
                    table { max-width: 100%; }
                    </style>
                    </head>
                    <body>" + (htmlContent ?? string.Empty) + @"</body>
                    </html>";
                return htmlToPdf.GeneratePdf(finalHtml);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo file PDF: " + ex.Message, ex);
            }
        }
        public string ConvertPdfToHtml(string physicalPath)
        {
            if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
                throw new FileNotFoundException("Không tìm thấy file PDF.", physicalPath);
            StringBuilder html = new StringBuilder();
            using (PdfReader reader = new PdfReader(physicalPath))
            {
                for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                {
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
                    if (string.IsNullOrWhiteSpace(pageText)) continue;
                    string encodedText = WebUtility.HtmlEncode(pageText);
                    string[] lines = encodedText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    foreach (string line in lines)
                    {
                        string content = line.Trim();
                        if (!string.IsNullOrWhiteSpace(content)) html.Append("<p>").Append(content).Append("</p>");
                    }
                    if (pageNumber < reader.NumberOfPages) html.Append("<p>&nbsp;</p>");
                }
            }
            return html.ToString();
        }
    }
}