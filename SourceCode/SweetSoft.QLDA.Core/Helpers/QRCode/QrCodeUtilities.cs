using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.QRCode
{
    /// <summary>
    /// Helper class that generates QR code images using QRCoder.
    /// </summary>
    public static class QrCodeUtilities
    {
        /// <summary>
        /// Generates a QR code as a Base64 encoded string using the specified content and options.
        /// </summary>
        /// <param name="content">The content to encode into the QR code.</param>
        /// <param name="options">Optional configuration used while generating the QR code.</param>
        /// <returns>A Base64 string representing the QR code image.</returns>
        public static string GenerateBase64(string content, QrCodeGenerationOptions options = null)
        {
            byte[] qrBytes = GenerateBytes(content, options);
            return Convert.ToBase64String(qrBytes);
        }

        /// <summary>
        /// Generates a QR code image as an array of bytes.
        /// </summary>
        /// <param name="content">The content to encode into the QR code.</param>
        /// <param name="options">Optional configuration used while generating the QR code.</param>
        /// <returns>A byte array containing the QR code image data.</returns>
        public static byte[] GenerateBytes(string content, QrCodeGenerationOptions options = null)
        {
            using (Bitmap qrBitmap = GenerateBitmap(content, options))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    ImageFormat format = (options != null ? options.ImageFormat : null) ?? ImageFormat.Png;
                    qrBitmap.Save(stream, format);
                    return stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Generates a QR code image as a <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="content">The content to encode.</param>
        /// <param name="options">Optional configuration used while generating the QR code.</param>
        /// <returns>A <see cref="Bitmap"/> instance containing the QR code.</returns>
        public static Bitmap GenerateBitmap(string content, QrCodeGenerationOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be null or whitespace.", "content");
            }

            QrCodeGenerationOptions effectiveOptions = options != null ? options.Clone() : QrCodeGenerationOptions.CreateDefault();
            effectiveOptions.EnsureIsValid();

            using (QRCodeGenerator generator = new QRCodeGenerator())
            {
                using (QRCodeData data = generator.CreateQrCode(content, effectiveOptions.ErrorCorrectionLevel, effectiveOptions.ForceUtf8, effectiveOptions.Utf8Bom, effectiveOptions.EciMode, effectiveOptions.RequestedVersion))
                {
                    using (QRCoder.QRCode qrCode = new QRCoder.QRCode(data))
                    {
                        Bitmap qrBitmap = qrCode.GetGraphic(effectiveOptions.PixelsPerModule, effectiveOptions.DarkColor, effectiveOptions.LightColor, effectiveOptions.DrawQuietZones);

                        if (effectiveOptions.Logo != null)
                        {
                            AttachLogo(qrBitmap, effectiveOptions.Logo);
                        }

                        return qrBitmap;
                    }
                }
            }
        }

        private static void AttachLogo(Bitmap qrCodeBitmap, QrCodeLogoOptions logoOptions)
        {
            using (Bitmap logo = logoOptions.CreateLogo())
            {
                if (logo == null)
                {
                    return;
                }

                Size logoSize = GetLogoSize(qrCodeBitmap.Size, logo.Size, logoOptions);

                using (Bitmap resizedLogo = new Bitmap(logo, logoSize))
                {
                    Rectangle destinationRect = GetLogoRectangle(qrCodeBitmap.Size, logoSize, logoOptions);

                    using (Graphics graphics = Graphics.FromImage(qrCodeBitmap))
                    {
                        if (logoOptions.EnableAntialiasing)
                        {
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                        }

                        DrawLogoBackground(graphics, destinationRect, logoOptions);
                        graphics.DrawImage(resizedLogo, destinationRect);
                    }
                }
            }
        }

        private static void DrawLogoBackground(Graphics graphics, Rectangle destinationRect, QrCodeLogoOptions logoOptions)
        {
            if (!logoOptions.BackgroundColor.HasValue)
            {
                return;
            }

            Rectangle padded = InflateRectangle(destinationRect, logoOptions.BackgroundPadding);
            using (SolidBrush brush = new SolidBrush(logoOptions.BackgroundColor.Value))
            {
                if (logoOptions.CornerRadius > 0)
                {
                    using (GraphicsPath path = CreateRoundedRectanglePath(padded, logoOptions.CornerRadius))
                    {
                        graphics.FillPath(brush, path);
                    }
                }
                else
                {
                    graphics.FillRectangle(brush, padded);
                }
            }
        }

        private static Rectangle InflateRectangle(Rectangle rectangle, int padding)
        {
            if (padding <= 0)
            {
                return rectangle;
            }

            rectangle.Inflate(padding, padding);
            return rectangle;
        }

        private static Size GetLogoSize(Size qrSize, Size originalLogoSize, QrCodeLogoOptions logoOptions)
        {
            if (logoOptions.ExplicitSize.HasValue)
            {
                return logoOptions.ExplicitSize.Value;
            }

            int maxWidth = Math.Max(1, (qrSize.Width * logoOptions.SizePercent) / 100);
            int maxHeight = Math.Max(1, (qrSize.Height * logoOptions.SizePercent) / 100);

            double widthRatio = (double)maxWidth / originalLogoSize.Width;
            double heightRatio = (double)maxHeight / originalLogoSize.Height;
            double scale = Math.Min(widthRatio, heightRatio);

            int targetWidth = Math.Max(1, (int)Math.Round(originalLogoSize.Width * scale));
            int targetHeight = Math.Max(1, (int)Math.Round(originalLogoSize.Height * scale));

            return new Size(targetWidth, targetHeight);
        }

        private static Rectangle GetLogoRectangle(Size qrSize, Size logoSize, QrCodeLogoOptions logoOptions)
        {
            int x;
            int y;

            if (logoOptions.Position.HasValue)
            {
                Point position = logoOptions.Position.Value;
                x = position.X;
                y = position.Y;
            }
            else
            {
                x = (qrSize.Width - logoSize.Width) / 2;
                y = (qrSize.Height - logoSize.Height) / 2;
            }

            return new Rectangle(x, y, logoSize.Width, logoSize.Height);
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            int diameter = cornerRadius * 2;
            if (diameter <= 0)
            {
                GraphicsPath straightPath = new GraphicsPath();
                straightPath.AddRectangle(rect);
                return straightPath;
            }

            int adjustedDiameter = Math.Min(diameter, Math.Min(rect.Width, rect.Height));
            GraphicsPath path = new GraphicsPath();
            Rectangle arc = new Rectangle(rect.Location, new Size(adjustedDiameter, adjustedDiameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - adjustedDiameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - adjustedDiameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
