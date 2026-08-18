using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.QRCode
{
    public class QrCodeGenerationOptions
    {
        private int _pixelsPerModule = 20;
        private bool _drawQuietZones = true;
        private ImageFormat _imageFormat = ImageFormat.Png;
        private QrCodeLogoOptions _logoOptions;
        private bool _forceUtf8;
        private bool _utf8Bom;
        private QRCodeGenerator.EciMode _eciMode = QRCodeGenerator.EciMode.Default;
        private int _requestedVersion = -1;

        /// <summary>
        /// Gets or sets the number of pixels that will be used for a single QR module.
        /// </summary>
        public int PixelsPerModule
        {
            get { return _pixelsPerModule; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Pixels per module must be greater than zero.");
                }

                _pixelsPerModule = value;
            }
        }

        /// <summary>
        /// Gets or sets the error correction level used by the QR code.
        /// </summary>
        public QRCodeGenerator.ECCLevel ErrorCorrectionLevel { get; set; }

        /// <summary>
        /// Gets or sets the color used for the dark modules of the QR code.
        /// </summary>
        public Color DarkColor { get; set; }

        /// <summary>
        /// Gets or sets the color used for the light modules of the QR code.
        /// </summary>
        public Color LightColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quiet zones should be rendered around the QR code.
        /// </summary>
        public bool DrawQuietZones
        {
            get { return _drawQuietZones; }
            set { _drawQuietZones = value; }
        }

        /// <summary>
        /// Gets or sets the desired output image format.
        /// </summary>
        public ImageFormat ImageFormat
        {
            get { return _imageFormat; }
            set { _imageFormat = value ?? ImageFormat.Png; }
        }

        /// <summary>
        /// Gets or sets logo rendering options.
        /// </summary>
        public QrCodeLogoOptions Logo
        {
            get { return _logoOptions; }
            set { _logoOptions = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the plain text should be encoded in UTF-8.
        /// </summary>
        public bool ForceUtf8
        {
            get { return _forceUtf8; }
            set { _forceUtf8 = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a UTF-8 byte-order mark should be added when using UTF-8 encoding.
        /// </summary>
        public bool Utf8Bom
        {
            get { return _utf8Bom; }
            set { _utf8Bom = value; }
        }

        /// <summary>
        /// Gets or sets the ECI mode used during encoding.
        /// </summary>
        public QRCodeGenerator.EciMode EciMode
        {
            get { return _eciMode; }
            set { _eciMode = value; }
        }

        /// <summary>
        /// Gets or sets the requested version (size) of the QR code. Values between 1 and 40 are valid, -1 means auto.
        /// </summary>
        public int RequestedVersion
        {
            get { return _requestedVersion; }
            set
            {
                if (value < -1 || value == 0 || value > 40)
                {
                    throw new ArgumentOutOfRangeException("value", "Requested version must be between 1 and 40 or -1 for automatic selection.");
                }

                _requestedVersion = value;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="QrCodeGenerationOptions"/> class with sensible defaults.
        /// </summary>
        public QrCodeGenerationOptions()
        {
            ErrorCorrectionLevel = QRCodeGenerator.ECCLevel.Q;
            DarkColor = Color.Black;
            LightColor = Color.White;
            DrawQuietZones = true;
            ImageFormat = ImageFormat.Png;
            RequestedVersion = -1;
        }

        /// <summary>
        /// Creates an independent copy of the current options instance.
        /// </summary>
        /// <returns>A new <see cref="QrCodeGenerationOptions"/> instance.</returns>
        public QrCodeGenerationOptions Clone()
        {
            QrCodeGenerationOptions copy = new QrCodeGenerationOptions
            {
                PixelsPerModule = PixelsPerModule,
                ErrorCorrectionLevel = ErrorCorrectionLevel,
                DarkColor = DarkColor,
                LightColor = LightColor,
                DrawQuietZones = DrawQuietZones,
                ImageFormat = ImageFormat,
                Logo = _logoOptions != null ? _logoOptions.Clone() : null,
                ForceUtf8 = ForceUtf8,
                Utf8Bom = Utf8Bom,
                EciMode = EciMode,
                RequestedVersion = RequestedVersion
            };

            return copy;
        }

        /// <summary>
        /// Creates an options object with default values applied.
        /// </summary>
        /// <returns>A new <see cref="QrCodeGenerationOptions"/> instance with default configuration.</returns>
        public static QrCodeGenerationOptions CreateDefault()
        {
            return new QrCodeGenerationOptions();
        }

        internal void EnsureIsValid()
        {
            if (ImageFormat == null)
            {
                ImageFormat = ImageFormat.Png;
            }

            if (Logo != null)
            {
                Logo.Validate();
            }
        }
    }
}
