using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.QRCode
{
    /// <summary>
    /// Provides configuration for embedding a logo into a QR code.
    /// </summary>
    public class QrCodeLogoOptions
    {
        private const int DefaultSizePercent = 20;
        private readonly Func<Bitmap> _logoFactory;
        private int _sizePercent = DefaultSizePercent;
        private Size? _explicitSize;
        private int _backgroundPadding;
        private int _cornerRadius;
        private Color? _backgroundColor;
        private Point? _position;
        private bool _enableAntialiasing = true;

        private QrCodeLogoOptions(Func<Bitmap> logoFactory)
        {
            if (logoFactory == null)
            {
                throw new ArgumentNullException("logoFactory");
            }

            _logoFactory = logoFactory;
        }

        /// <summary>
        /// Gets or sets the relative width of the logo compared to the QR code width.
        /// The value is expressed as a percentage.
        /// </summary>
        public int SizePercent
        {
            get { return _sizePercent; }
            set
            {
                if (value <= 0 || value > 100)
                {
                    throw new ArgumentOutOfRangeException("value", "Size percent must be between 1 and 100.");
                }

                _sizePercent = value;
            }
        }

        /// <summary>
        /// Gets or sets an explicit size for the logo. When specified, this value takes precedence over <see cref="SizePercent"/>.
        /// </summary>
        public Size? ExplicitSize
        {
            get { return _explicitSize; }
            set
            {
                if (value.HasValue)
                {
                    if (value.Value.Width <= 0 || value.Value.Height <= 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Explicit size must be positive.");
                    }
                }

                _explicitSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the background padding around the logo in pixels.
        /// </summary>
        public int BackgroundPadding
        {
            get { return _backgroundPadding; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Padding must be zero or positive.");
                }

                _backgroundPadding = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color to draw behind the logo.
        /// </summary>
        public Color? BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the corner radius used when drawing the background rectangle.
        /// </summary>
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Corner radius cannot be negative.");
                }

                _cornerRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets the explicit position of the logo. If not provided, the logo will be centered.
        /// </summary>
        public Point? Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether high quality drawing should be used when rendering the logo.
        /// </summary>
        public bool EnableAntialiasing
        {
            get { return _enableAntialiasing; }
            set { _enableAntialiasing = value; }
        }

        /// <summary>
        /// Creates a new <see cref="QrCodeLogoOptions"/> instance from a file path.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the logo file.</param>
        /// <returns>A configured <see cref="QrCodeLogoOptions"/> instance.</returns>
        public static QrCodeLogoOptions FromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or whitespace.", "filePath");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified logo file could not be found.", filePath);
            }

            return new QrCodeLogoOptions(() => new Bitmap(filePath));
        }

        /// <summary>
        /// Creates a new <see cref="QrCodeLogoOptions"/> instance from raw image bytes.
        /// </summary>
        /// <param name="logoBytes">The logo image represented as a byte array.</param>
        /// <returns>A configured <see cref="QrCodeLogoOptions"/> instance.</returns>
        public static QrCodeLogoOptions FromBytes(byte[] logoBytes)
        {
            if (logoBytes == null || logoBytes.Length == 0)
            {
                throw new ArgumentException("Logo bytes cannot be null or empty.", "logoBytes");
            }

            byte[] logoCopy = (byte[])logoBytes.Clone();
            return new QrCodeLogoOptions(() => CreateBitmapFromBytes(logoCopy));
        }

        /// <summary>
        /// Creates a new <see cref="QrCodeLogoOptions"/> instance from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the logo image data.</param>
        /// <returns>A configured <see cref="QrCodeLogoOptions"/> instance.</returns>
        public static QrCodeLogoOptions FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (MemoryStream buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                return FromBytes(buffer.ToArray());
            }
        }

        /// <summary>
        /// Creates a new <see cref="QrCodeLogoOptions"/> instance using a custom factory.
        /// </summary>
        /// <param name="factory">A factory that creates a fresh <see cref="Bitmap"/> on each invocation.</param>
        /// <returns>A configured <see cref="QrCodeLogoOptions"/> instance.</returns>
        public static QrCodeLogoOptions FromFactory(Func<Bitmap> factory)
        {
            return new QrCodeLogoOptions(factory);
        }

        internal Bitmap CreateLogo()
        {
            return _logoFactory != null ? _logoFactory() : null;
        }

        internal void Validate()
        {
            if (_logoFactory == null)
            {
                throw new InvalidOperationException("Logo factory was not supplied.");
            }

            if (_explicitSize.HasValue && (_explicitSize.Value.Width <= 0 || _explicitSize.Value.Height <= 0))
            {
                throw new ArgumentOutOfRangeException("ExplicitSize", "Explicit size must be greater than zero in both dimensions.");
            }

            if (_sizePercent <= 0 || _sizePercent > 100)
            {
                throw new ArgumentOutOfRangeException("SizePercent", "Size percent must be between 1 and 100.");
            }
        }

        internal QrCodeLogoOptions Clone()
        {
            QrCodeLogoOptions clone = new QrCodeLogoOptions(_logoFactory)
            {
                SizePercent = SizePercent,
                ExplicitSize = ExplicitSize,
                BackgroundPadding = BackgroundPadding,
                BackgroundColor = BackgroundColor,
                CornerRadius = CornerRadius,
                Position = Position,
                EnableAntialiasing = EnableAntialiasing
            };

            return clone;
        }

        private static Bitmap CreateBitmapFromBytes(byte[] logoBytes)
        {
            using (MemoryStream ms = new MemoryStream(logoBytes))
            {
                using (Image image = Image.FromStream(ms))
                {
                    return new Bitmap(image);
                }
            }
        }
    }
}
