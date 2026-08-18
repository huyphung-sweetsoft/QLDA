//-----------------------PROGRAMER LOGS---------------------------
using SweetSoft.QLDA.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace SweetSoft.QLDA.Core.Utils
{
    public class FileHelpers
    {
        private static string[] AllowedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".doc", ".docx", ".xlsx", ".xls", ".pdf", ".svg", ".mp4", ".mp3", ".avi", ".webp" };

        private static readonly List<string> AllowedContentTypes = new List<string>
        {
            "image/jpeg",  // .jpg, .jpeg
            "image/png",   // .png
            "image/gif",   // .gif
            "image/bmp",   // .bmp
            "image/svg",   // .svg
            "image/webp",   // .webp
            "application/msword",  // .doc
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",  // .docx
            "application/vnd.ms-excel",  // .xls
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",  // .xlsx
            "application/pdf",  // .pdf
            "application/x-zip-compressed",
            "application/zip",
            "video/mp4",  // .mp4
            "video/mpeg",
            "video/quicktime",
            "video/x-ms-wmv",
            "video/x-msvideo",
            "video/x-flv",
            "audio/mpeg",//mp3
        };
        private static readonly List<string> AllowedMimeTypes = new List<string>
        {
            "image/jpeg",  // .jpg
            "image/png",   // .png
            "image/gif",   // .gif
            "image/bmp",   // .bmp
            "image/svg",   // .svg
            "image/webp",   // .webp
            "application/msword",  // .doc
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",  // .docx
            "application/vnd.ms-excel",  // .xls
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",  // .xlsx
            "application/x-zip-compressed",
            "application/pdf",  // .pdf
            "application/zip",  // .pdf
            "video/mp4",  // .mp4
            "video/mpeg",
            "video/quicktime",
            "video/x-ms-wmv",
            "video/x-msvideo",
            "video/x-flv",
            "audio/mpeg",//mp3
        };
        private static readonly string[] DangerousExtensions = new string[]
        {
            ".aspx", ".ashx", ".asmx", ".cshtml", ".vbhtml", // ASP.NET
            ".php", ".phtml", ".php3", ".php4",             // PHP
            ".asp",                                          // Classic ASP
            ".jsp", ".jspx",                                // Java
            ".exe", ".dll", ".bat", ".cmd", ".com", ".vbs", // Windows binaries & scripts
            ".sh", ".py", ".pl", ".cgi",                    // Linux script
            ".htaccess", ".config", ".json", ".env",        // Configurations
            ".jar", ".class", ".war",                       // Java bytecode
            ".ps1", ".psm1"                                  // PowerShell
        };

        private static string numberPattern = "-{0}";
        public static string ChangeFileName(string path)
        {
            string pathNonUnicode = Regex.Replace(path, @"[^\u0000-\u007F]", string.Empty);
            return Regex.Replace(pathNonUnicode, @"\s+", "-");
        }
        public static string NextAvailableFilename(string path)
        {
            // Short-cut if already available
            if (!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if (Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern);
        }

        private static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }
        public static bool IsFileAllowed(string fileName)
        {
            if (!IsValidFilename(fileName)) return false;

            string extension = Path.GetExtension(fileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                return false;
            string mimeType = MimeMapping.GetMimeMapping(fileName);
            return AllowedMimeTypes.Contains(mimeType);
        }
        public static bool IsFileAllowed(Stream fileStream, string fileName, string contentType)
        {
            if (!IsValidFilename(fileName)) return false;

            string extension = Path.GetExtension(fileName).ToLower();

            if (!AllowedExtensions.Contains(extension))
                return false;

            if (!AllowedContentTypes.Contains(contentType))
                return false;

            try
            {
                byte[] buffer = new byte[256];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, buffer.Length);
                if(contentType == "audio/mpeg")//File mp3 thi pass
                    return AllowedMimeTypes.Contains(contentType);
                
                string detectedMime = GetMimeFromMagicBytes(buffer, fileStream);
                return AllowedMimeTypes.Contains(detectedMime);
            }
            catch
            {
                return false;
            }
        }
        public static bool IsValidFilename(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string name = Path.GetFileName(fileName);
            string extension = Path.GetExtension(name).ToLower();

            // Extension phải nằm trong danh sách cho phép
            if (!AllowedExtensions.Contains(extension))
                return false;

            // Không chứa ký tự đặc biệt nguy hiểm
            if (name.Contains(";") || name.Contains("..\\") || name.Contains("../") || name.Contains("~"))
                return false;

            // Không cho phép tên file kiểu .aspx;.jpg
            if (ContainsDangerousExtension(name))
                return false;

            // Không có double extension sau đuôi chính: ví dụ .pdf.exe
            string[] parts = name.Split('.');
            if (parts.Length > 2)
            {
                string lastExt = "." + parts[parts.Length - 1].ToLower();
                string secondLastExt = "." + parts[parts.Length - 2].ToLower();

                if (AllowedExtensions.Contains(lastExt) && !AllowedExtensions.Contains(secondLastExt))
                {
                    // Cho phép .signed.pdf, .signed.signed.pdf
                    return true;
                }

                // Trường hợp kiểu abc.pdf.exe => chặn
                if (!AllowedExtensions.Contains(lastExt))
                    return false;
            }

            return true;
        }
        public static bool ContainsDangerousExtension(string fileName)
        {
            string lowerName = fileName.ToLowerInvariant();

            foreach (var ext in DangerousExtensions)
            {
                if (lowerName.Contains(ext))
                    return true;
            }

            return false;
        }

        public static bool IsAllowedContentType(string contentType)
        {
            return AllowedContentTypes.Contains(contentType);
        }

        public static Bitmap LoadBitmapKeepOriginalOrientation(string path)
        {
            using (var original = new Bitmap(path))
            {
                // Clone để không bị ảnh hưởng bởi EXIF Orientation khi truy cập thông tin
                var clone = new Bitmap(original);

                const int orientationId = 0x0112; // EXIF orientation tag
                if (original.PropertyIdList.Contains(orientationId))
                {
                    var prop = original.GetPropertyItem(orientationId);
                    int orientationValue = prop.Value[0];

                    RotateFlipType flipType = RotateFlipType.RotateNoneFlipNone;

                    switch (orientationValue)
                    {
                        case 2: flipType = RotateFlipType.RotateNoneFlipX; break;
                        case 3: flipType = RotateFlipType.Rotate180FlipNone; break;
                        case 4: flipType = RotateFlipType.Rotate180FlipX; break;
                        case 5: flipType = RotateFlipType.Rotate90FlipX; break;
                        case 6: flipType = RotateFlipType.Rotate90FlipNone; break;
                        case 7: flipType = RotateFlipType.Rotate270FlipX; break;
                        case 8: flipType = RotateFlipType.Rotate270FlipNone; break;
                    }

                    clone.RotateFlip(flipType);

                    // Optionally: Remove orientation property
                    clone.RemovePropertyItem(orientationId);
                }

                return clone;
            }
        }
        public static string GetMimeFromMagicBytes(byte[] fileHeader, Stream fullStream)
        {
            if (fileHeader == null || fileHeader.Length < 4)
                return "unknown/unknown";

            // JPEG
            if (fileHeader[0] == 0xFF && fileHeader[1] == 0xD8)
                return "image/jpeg";

            // PNG
            if (fileHeader[0] == 0x89 && fileHeader[1] == 0x50 &&
                fileHeader[2] == 0x4E && fileHeader[3] == 0x47)
                return "image/png";

            // GIF
            if (fileHeader[0] == 0x47 && fileHeader[1] == 0x49 &&
                fileHeader[2] == 0x46)
                return "image/gif";

            // PDF
            if (fileHeader[0] == 0x25 && fileHeader[1] == 0x50 &&
                fileHeader[2] == 0x44 && fileHeader[3] == 0x46)
                return "application/pdf";

            // DOC, XLS - Binary format
            if (fileHeader.Length >= 8 &&
                fileHeader[0] == 0xD0 && fileHeader[1] == 0xCF &&
                fileHeader[2] == 0x11 && fileHeader[3] == 0xE0)
                return "application/msword"; // or .xls depending on usage

            // Office OpenXML: docx, xlsx, pptx
            if (fileHeader[0] == 0x50 && fileHeader[1] == 0x4B &&
                fileHeader[2] == 0x03 && fileHeader[3] == 0x04)
            {
                try
                {
                    fullStream.Seek(0, SeekOrigin.Begin);
                    using (var archive = new ZipArchive(fullStream, ZipArchiveMode.Read, true))
                    {
                        var contentTypesEntry = archive.GetEntry("[Content_Types].xml");
                        if (contentTypesEntry != null)
                        {
                            using (var reader = new StreamReader(contentTypesEntry.Open()))
                            {
                                var xml = XDocument.Load(reader);

                                var overrides = xml.Descendants().Where(e => e.Name.LocalName == "Override");

                                foreach (var o in overrides)
                                {
                                    var partName = o.Attribute("PartName")?.Value;
                                    var contentType = o.Attribute("ContentType")?.Value;

                                    if (contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                                        return contentType;

                                    if (contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                        return contentType;

                                    if (contentType == "application/vnd.openxmlformats-officedocument.presentationml.presentation")
                                        return contentType;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback nếu lỗi
                }

                return "application/zip"; // fallback chung
            }

            // MP4
            if (fileHeader.Length >= 12 &&
                fileHeader[4] == 0x66 && fileHeader[5] == 0x74 &&
                fileHeader[6] == 0x79 && fileHeader[7] == 0x70)
                return "video/mp4";

            // MP3
            if (fileHeader.Length >= 3 &&
                ((fileHeader[0] == 0x49 && fileHeader[1] == 0x44 && fileHeader[2] == 0x33) || // "ID3"
                 (fileHeader[0] == 0xFF && fileHeader[1] == 0xFB))) // MPEG frame
                return "audio/mpeg";

            // AVI
            if (fileHeader.Length >= 12 &&
                fileHeader[0] == 0x52 && fileHeader[1] == 0x49 &&
                fileHeader[2] == 0x46 && fileHeader[3] == 0x46 &&
                fileHeader[8] == 0x41 && fileHeader[9] == 0x56 &&
                fileHeader[10] == 0x49 && fileHeader[11] == 0x20)
                return "video/x-msvideo";

            // MPEG
            if (fileHeader.Length >= 4 &&
                fileHeader[0] == 0x00 && fileHeader[1] == 0x00 &&
                fileHeader[2] == 0x01 && fileHeader[3] == 0xBA)
                return "video/mpeg";

            // WEBP
            if (fileHeader.Length >= 12 &&
                fileHeader[0] == 0x52 && fileHeader[1] == 0x49 &&
                fileHeader[2] == 0x46 && fileHeader[3] == 0x46 &&
                fileHeader[8] == 0x57 && fileHeader[9] == 0x45 &&
                fileHeader[10] == 0x42 && fileHeader[11] == 0x50)
                return "image/webp";

            return "unknown/unknown";
        }
        public static string NormalizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string name = System.IO.Path.GetFileNameWithoutExtension(input);
            string ext = System.IO.Path.GetExtension(input).ToLowerInvariant();

            name = RemoveDiacritics(name);
            name = name.ToLowerInvariant();

            name = Regex.Replace(name, @"\s+", "-");

            // Giữ lại a-z, 0-9, "-", "_"  
            name = Regex.Replace(name, @"[^a-z0-9\-_]", "");

            name = Regex.Replace(name, @"-+", "-").Trim('-');

            return name + ext;
        }
        private static string RemoveDiacritics(string phrase)
        {
            //First to lower case 
            phrase = VnUnicodeHelpers.ReplaceVietnameseCharacters(phrase).ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(phrase);

            phrase = Encoding.ASCII.GetString(bytes);

            //Replace spaces 
            phrase = Regex.Replace(phrase, @"\s", "-", RegexOptions.Compiled);

            //Remove invalid chars 
            phrase = Regex.Replace(phrase, @"[^\w\s\p{Pd}]", "", RegexOptions.Compiled);

            //Trim dashes from end 
            phrase = phrase.Trim('-', '_');

            //Replace double occurences of - or \_ 
            phrase = Regex.Replace(phrase, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return phrase;
        }
        public static string IsValidPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "javascript:;";
            if (path.StartsWith("https://") || path.StartsWith("http://"))
                return path;
            return $"{CommonHelpers.GetHostPath().TrimEnd('/')}/{path.TrimStart('/')}";
        }
    }

    public class ImageValidator
    {
        private string _base64Image;
        private long _maxKilobytesSize;
        private string _extension;
        private byte[] _imageBytes;
        public ImageValidator() { }
        public ImageValidator(string base64, long maxKilobytesSize)
        {
            _base64Image = base64;
            _maxKilobytesSize = maxKilobytesSize;
        }
        public string Extension
        {
            get
            {
                if (string.IsNullOrEmpty(this._base64Image))
                    return string.Empty;
                if (!string.IsNullOrEmpty(this._extension))
                    return this._extension;
                this._extension = this._base64Image.StartsWith("/") ? ".jpg" : this._base64Image.StartsWith("i") ? ".png" : this._base64Image.StartsWith("R") ? ".gif" : null;
                return this._extension;
            }
        }
        public byte[] ImageBytes
        {
            get
            {
                if (string.IsNullOrEmpty(this._base64Image))
                    return null;
                if (this._imageBytes != null)
                    return this._imageBytes;
                this._imageBytes = Convert.FromBase64String(this._base64Image);
                return this._imageBytes;
            }
        }
        public bool IsValid(out string message)
        {
            try
            {
                if (string.IsNullOrEmpty(this._base64Image))
                {
                    message = "No image";
                    return false;
                }

                this._extension = this._base64Image.StartsWith("/") ? ".jpg" : this._base64Image.StartsWith("i") ? ".png" : this._base64Image.StartsWith("R") ? ".gif" : null;

                if (this._extension == null)
                {
                    message = "Please choose only .jpg, .jpeg, .png and .gif image types!";
                    return false;
                }

                this._imageBytes = Convert.FromBase64String(this._base64Image);
                if (this._imageBytes == null || this._imageBytes.Length == 0)
                {
                    message = $"File is empty. (0 Kilobytes)";
                    return false;
                }
                if (this._imageBytes.Length > this._maxKilobytesSize * 1024)
                {
                    message = $"File is too large. (max. {this._maxKilobytesSize} Kilobytes)";
                    return false;
                }

                message = "Successful";
                return true;
            }
            catch
            {
                message = "Invalid base64 image";
                return false;
            }
        }
        public bool IsValidDocument(out string message)
        {
            try
            {
                if (string.IsNullOrEmpty(this._base64Image))
                {
                    message = "No image";
                    return false;
                }

                this._extension = this._base64Image.StartsWith("/") ? ".jpg" : this._base64Image.StartsWith("i") ? ".png" : (this._base64Image.StartsWith("jv") || this._base64Image.StartsWith("JV")) ? ".pdf" : null;

                if (this._extension == null)
                {
                    message = "Please choose only .jpg, .png, .pdf types!";
                    return false;
                }

                this._imageBytes = Convert.FromBase64String(this._base64Image);
                if (this._imageBytes == null || this._imageBytes.Length == 0)
                {
                    message = $"File is empty. (0 Kilobytes)";
                    return false;
                }
                if (this._imageBytes.Length > this._maxKilobytesSize * 1024)
                {
                    message = $"File is too large. (max. {this._maxKilobytesSize} Kilobytes)";
                    return false;
                }

                message = "Successful";
                return true;
            }
            catch
            {
                message = "Invalid base64";
                return false;
            }
        }
    }

    public class FolderHelper
    {
        private string _folder;
        private bool _folderTime;
        public FolderHelper() { }
        public FolderHelper(string folder)
        {
            this._folder = folder;
        }
        public FolderHelper(string folder, bool folderTime)
        {
            this._folder = folder;
            this._folderTime = folderTime;
        }

        public bool CreateFolder(out string subPath, out string fullPath)
        {
            subPath = "";
            fullPath = "";
            try
            {
                string subApp = AppSettingHelpers.GetSetting("SubApp") ?? "";
                if (!string.IsNullOrEmpty(subApp))
                    subPath = string.Format("/{0}/uploads/{1}/", subApp, this._folder);
                else
                    subPath = string.Format("/uploads/{0}/", this._folder);
                if (this._folderTime)
                    subPath = subPath + DateTime.UtcNow.ToString("yyyyMM") + "/";
                fullPath = HttpContext.Current.Server.MapPath(subPath);

                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
