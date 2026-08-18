using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.Controls
{
    public partial class Captcha : BaseAdminUserControl
    {
        public string ClientId
        {
            get
            {
                return ViewState["ClientId"] as string ?? "Captcha";
            }
            set
            {
                ViewState["ClientId"] = value;
            }
        }

        private const int CAPTCHA_WIDTH = 100;
        private const int CAPTCHA_HEIGHT = 30;
        private string CaptchaCode
        {
            get
            {
                return (string)ViewState["CaptchaCode"];
            }
            set
            {
                ViewState["CaptchaCode"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && !ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
            {
                txtValidCode.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_SECURIRY_CODE);
                RefreshCaptcha();
            }

            txtValidCode.Attributes.Add("onkeydown", "return preventEnterKey(event);");

            if (Page.Form != null)
            {
                Page.Form.DefaultButton = "";
            }
        }

        public bool CheckValidCode()
        {
            string input = txtValidCode.Text.Trim();
            string expected = this.CaptchaCode;

            bool isValid = !string.IsNullOrEmpty(input) &&
                          !string.IsNullOrEmpty(expected) &&
                          string.Equals(input, expected, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                AddValidationError(GetResourceText(BackEndResourceKeys.SECURIRY_CODE_IS_INCORRECT));
                RefreshCaptcha();
                ClearText();
            }

            return isValid;
        }

        protected void ChangeCaptchaImage(object sender, EventArgs e)
        {
            RefreshCaptcha();
            txtValidCode.Focus();
        }
        public void RefreshCaptcha()
        {
            this.CaptchaCode = GenerateRandomCode();
            string imageUrl = GetImageCaptcha(this.CaptchaCode);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                imgCaptcha.ImageUrl = imageUrl;
                pnlCaptcha.Update();
            }
        }

        private void ClearText()
        {
            txtValidCode.Text = string.Empty;
            txtValidCode.Focus();
        }

        private void AddValidationError(string message)
        {
            try
            {
                var engine = ValidationEngine.Instance(Page);
                engine.AddErrorPrompt(txtValidCode.ClientID, $"* {message}");
                engine.ShowErrorPrompt();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {ex.Message}");
            }
        }

        private static string GenerateRandomCode()
        {
            var rand = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[rand.Next(s.Length)]).ToArray());
        }

        private string GetImageCaptcha(string code)
        {
            try
            {
                using (var img = new RandomImage(code, CAPTCHA_WIDTH, CAPTCHA_HEIGHT))
                {
                    using (var ms = new MemoryStream())
                    {
                        img.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        string base64 = Convert.ToBase64String(ms.ToArray());
                        return $"data:image/png;base64,{base64}";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Captcha image generation error: {ex.Message}");
                return string.Empty;
            }
        }

        private class RandomImage : IDisposable
        {
            public string Text { get; }
            public Bitmap Image { get; private set; }
            public int Width { get; }
            public int Height { get; }

            private readonly Random _random = new Random();

            public RandomImage(string text, int width, int height)
            {
                if (width <= 0 || height <= 0)
                    throw new ArgumentOutOfRangeException("Dimensions must be positive.");
                if (string.IsNullOrEmpty(text))
                    throw new ArgumentException("Text cannot be null or empty.");

                Text = text;
                Width = width;
                Height = height;

                GenerateImage();
            }

            private void GenerateImage()
            {
                Image = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

                using (var g = Graphics.FromImage(Image))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    var rect = new Rectangle(0, 0, Width, Height);

                    using (var backgroundBrush = new LinearGradientBrush(rect, Color.LightBlue, Color.White, 45f))
                    {
                        g.FillRectangle(backgroundBrush, rect);
                    }

                    float fontSize = Math.Min(Width / Text.Length * 1.2f, Height * 0.7f);
                    fontSize = Math.Max(fontSize, 8f); // Minimum font size

                    using (var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.DarkBlue))
                    {
                        var format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        using (var path = new GraphicsPath())
                        {
                            path.AddString(Text, font.FontFamily, (int)font.Style, fontSize, rect, format);

                            var points = new PointF[]
                            {
                            new PointF(_random.Next(5, 10), _random.Next(5, 10)),
                            new PointF(Width - _random.Next(5, 10), _random.Next(5, 10)),
                            new PointF(_random.Next(5, 10), Height - _random.Next(5, 10)),
                            new PointF(Width - _random.Next(5, 10), Height - _random.Next(5, 10))
                            };

                            var matrix = new Matrix();
                            path.Warp(points, rect, matrix, WarpMode.Perspective, 0.01f);
                            g.FillPath(textBrush, path);
                        }
                    }

                    AddNoise(g);
                }
            }

            private void AddNoise(Graphics g)
            {
                using (var noiseBrush = new SolidBrush(Color.FromArgb(50, Color.Gray)))
                {
                    for (int i = 0; i < Width * Height / 100; i++)
                    {
                        int x = _random.Next(Width);
                        int y = _random.Next(Height);
                        g.FillRectangle(noiseBrush, x, y, 1, 1);
                    }
                }
            }

            public void Dispose()
            {
                Image?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}