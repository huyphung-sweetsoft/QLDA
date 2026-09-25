using NReco.PdfGenerator;
using System;
using System.IO;
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
                if (_instance == null)
                    _instance = new PdfManager();
                return _instance;
            }
        }
        private PdfManager() { }

        /// <summary>
        /// Hàm chung: Chuyển đổi toàn bộ nội dung HTML thành file PDF và tải xuống
        /// </summary>
        /// <param name="htmlContent">Chuỗi HTML (Nên bao gồm cả thẻ <style> bên trong)</param>
        /// <param name="fileName">Tên file xuất ra (VD: BaoCao.pdf)</param>
        /// <param name="Response">Đối tượng HttpResponse để tải file</param>
        public void ExportHtmlToPdf(string htmlContent, string fileName, HttpResponse Response)
        {
            try
            {
                var htmlToPdf = new HtmlToPdfConverter();

                htmlToPdf.Size = PageSize.A4;

                htmlToPdf.Margins = new PageMargins { Top = 15, Bottom = 15, Left = 15, Right = 15 };

                string finalHtml = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                    </head>
                    <body style='font-family: Arial, sans-serif;'>
                        {htmlContent}
                    </body>
                    </html>";

                byte[] pdfBytes = htmlToPdf.GeneratePdf(finalHtml);

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename={fileName}");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(pdfBytes);
                Response.End();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo file PDF: " + ex.Message);
            }
        }

        public byte[] GeneratePdf(string htmlContent)
        {
            try
            {
                var htmlToPdf = new HtmlToPdfConverter();
                htmlToPdf.Size = PageSize.A4;
                htmlToPdf.Margins = new PageMargins { Top = 15, Bottom = 15, Left = 15, Right = 15 };

                string finalHtml = $@"<!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            @page {{
                                size: A4;
                            }}

                            html, body {{
                                margin: 0;
                                padding: 0;
                                font-family: Arial, sans-serif;
                                font-size: 13px;
                                line-height: 1.5;
                            }}

                            body {{
                                word-wrap: break-word;
                            }}

                            img {{
                                max-width: 100%;
                                height: auto;
                            }}

                            table {{
                                max-width: 100%;
                                border-collapse: collapse;
                            }}
                        </style>
                    </head>
                    <body>{htmlContent}</body>
                    </html>";
                return htmlToPdf.GeneratePdf(finalHtml);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo file PDF: " + ex.Message);
            }
        }
    }
}