using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SweetSoft.QLDA.Core.Utils
{
    public class MIMEAssistant
    {

        #region RegionName

        private static readonly Dictionary<string, string> MIMETypesDictionary =
            new Dictionary<string, string>
  {
    {"ai", "application/postscript"},
    {"aif", "audio/x-aiff"},
    {"aifc", "audio/x-aiff"},
    {"aiff", "audio/x-aiff"},
    {"asc", "text/plain"},
    {"atom", "application/atom+xml"},
    {"au", "audio/basic"},
    {"avi", "video/x-msvideo"},
    {"bcpio", "application/x-bcpio"},
    {"bin", "application/octet-stream"},
    {"cdf", "application/x-netcdf"},
    {"class", "application/octet-stream"},
    {"cpio", "application/x-cpio"},
    {"cpt", "application/mac-compactpro"},
    {"csh", "application/x-csh"},
    {"css", "text/css"},
    {"dcr", "application/x-director"},
    {"dif", "video/x-dv"},
    {"dir", "application/x-director"},
    {"dll", "application/octet-stream"},
    {"dmg", "application/octet-stream"},
    {"dms", "application/octet-stream"},
    {"doc", "application/msword"},
    {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
    {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
    {"docm","application/vnd.ms-word.document.macroEnabled.12"},
    {"dotm","application/vnd.ms-word.template.macroEnabled.12"},
    {"dtd", "application/xml-dtd"},
    {"dv", "video/x-dv"},
    {"dvi", "application/x-dvi"},
    {"dxr", "application/x-director"},
    {"eps", "application/postscript"},
    {"etx", "text/x-setext"},
    {"exe", "application/octet-stream"},
    {"ez", "application/andrew-inset"},
    {"gram", "application/srgs"},
    {"grxml", "application/srgs+xml"},
    {"gtar", "application/x-gtar"},
    {"hdf", "application/x-hdf"},
    {"hqx", "application/mac-binhex40"},
    {"htm", "text/html"},
    {"html", "text/html"},
    {"ice", "x-conference/x-cooltalk"},
    {"ics", "text/calendar"},
    {"ifb", "text/calendar"},
    {"iges", "model/iges"},
    {"igs", "model/iges"},
    {"jnlp", "application/x-java-jnlp-file"},
    {"js", "application/x-javascript"},
    {"kar", "audio/midi"},
    {"latex", "application/x-latex"},
    {"lha", "application/octet-stream"},
    {"lzh", "application/octet-stream"},
    {"m3u", "audio/x-mpegurl"},
    {"m4a", "audio/mp4a-latm"},
    {"m4b", "audio/mp4a-latm"},
    {"m4p", "audio/mp4a-latm"},
    {"m4u", "video/vnd.mpegurl"},
    {"m4v", "video/x-m4v"},
    {"man", "application/x-troff-man"},
    {"mathml", "application/mathml+xml"},
    {"me", "application/x-troff-me"},
    {"mesh", "model/mesh"},
    {"mid", "audio/midi"},
    {"midi", "audio/midi"},
    {"mif", "application/vnd.mif"},
    {"mov", "video/quicktime"},
    {"movie", "video/x-sgi-movie"},
    {"mp2", "audio/mpeg"},
    {"mp3", "audio/mpeg"},
    {"mp4", "video/mp4"},
    {"flv", "video/x-flv"},
    {"wmv", "video/x-ms-wmv"},
    {"mpe", "video/mpeg"},
    {"mpeg", "video/mpeg"},
    {"mpg", "video/mpeg"},
    {"mpga", "audio/mpeg"},
    {"ms", "application/x-troff-ms"},
    {"msh", "model/mesh"},
    {"mxu", "video/vnd.mpegurl"},
    {"nc", "application/x-netcdf"},
    {"oda", "application/oda"},
    {"ogg", "application/ogg"},
    {"pbm", "image/x-portable-bitmap"},
    {"pct", "image/pict"},
    {"mac", "image/x-macpaint"},
    {"bmp", "image/bmp"},
    {"cgm", "image/cgm"},
    {"djv", "image/vnd.djvu"},
    {"djvu", "image/vnd.djvu"},
    {"gif", "image/gif"},
    {"ico", "image/x-icon"},
    {"ief", "image/ief"},
    {"jp2", "image/jp2"},
    {"jpe", "image/jpeg"},
    {"jpeg", "image/jpeg"},
    {"jpg", "image/jpeg"},
    {"pgm", "image/x-portable-graymap"},
    {"pic", "image/pict"},
    {"pict", "image/pict"},
    {"png", "image/png"},
    {"webp", "image/webp"},
    {"pnm", "image/x-portable-anymap"},
    {"pnt", "image/x-macpaint"},
    {"pntg", "image/x-macpaint"},
    {"ppm", "image/x-portable-pixmap"},
    {"xpm", "image/x-xpixmap"},
    {"xbm", "image/x-xbitmap"},
    {"wbmp", "image/vnd.wap.wbmp"},
    {"tif", "image/tiff"},
    {"tiff", "image/tiff"},
    {"svg", "image/svg+xml"},
    {"rgb", "image/x-rgb"},
    {"qti", "image/x-quicktime"},
    {"qtif", "image/x-quicktime"},
    {"ras", "image/x-cmu-raster"},
    {"xwd", "image/x-xwindowdump"},
    {"pdb", "chemical/x-pdb"},
    {"pdf", "application/pdf"},
    {"pgn", "application/x-chess-pgn"},
    {"ppt", "application/vnd.ms-powerpoint"},
    {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
    {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
    {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
    {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
    {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
    {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
    {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
    {"ps", "application/postscript"},
    {"qt", "video/quicktime"},
    {"ra", "audio/x-pn-realaudio"},
    {"ram", "audio/x-pn-realaudio"},
    {"rdf", "application/rdf+xml"},
    {"rm", "application/vnd.rn-realmedia"},
    {"roff", "application/x-troff"},
    {"rtf", "text/rtf"},
    {"rtx", "text/richtext"},
    {"sgm", "text/sgml"},
    {"sgml", "text/sgml"},
    {"sh", "application/x-sh"},
    {"shar", "application/x-shar"},
    {"silo", "model/mesh"},
    {"sit", "application/x-stuffit"},
    {"skd", "application/x-koan"},
    {"skm", "application/x-koan"},
    {"skp", "application/x-koan"},
    {"skt", "application/x-koan"},
    {"smi", "application/smil"},
    {"smil", "application/smil"},
    {"snd", "audio/basic"},
    {"so", "application/octet-stream"},
    {"spl", "application/x-futuresplash"},
    {"src", "application/x-wais-source"},
    {"sv4cpio", "application/x-sv4cpio"},
    {"sv4crc", "application/x-sv4crc"},
    {"swf", "application/x-shockwave-flash"},
    {"t", "application/x-troff"},
    {"tar", "application/x-tar"},
    {"tcl", "application/x-tcl"},
    {"tex", "application/x-tex"},
    {"texi", "application/x-texinfo"},
    {"texinfo", "application/x-texinfo"},
    {"tr", "application/x-troff"},
    {"tsv", "text/tab-separated-values"},
    {"txt", "text/plain"},
    {"ustar", "application/x-ustar"},
    {"vcd", "application/x-cdlink"},
    {"vrml", "model/vrml"},
    {"vxml", "application/voicexml+xml"},
    {"wav", "audio/x-wav"},
    {"wbmxl", "application/vnd.wap.wbxml"},
    {"wml", "text/vnd.wap.wml"},
    {"wmlc", "application/vnd.wap.wmlc"},
    {"wmls", "text/vnd.wap.wmlscript"},
    {"wmlsc", "application/vnd.wap.wmlscriptc"},
    {"wrl", "model/vrml"},
    {"xht", "application/xhtml+xml"},
    {"xhtml", "application/xhtml+xml"},
    {"xls", "application/vnd.ms-excel"},
    {"xml", "application/xml"},
    {"xsl", "application/xml"},
    {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
    {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
    {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
    {"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
    {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
    {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
    {"xslt", "application/xslt+xml"},
    {"xul", "application/vnd.mozilla.xul+xml"},
    {"xyz", "chemical/x-xyz"},
    {"zip", "application/zip"}
  };

        #endregion

        private static readonly byte[] WEBP = { 82, 73, 70, 70 };
        private static readonly byte[] BMP = { 66, 77 };
        private static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };
        private static readonly byte[] GIF = { 71, 73, 70, 56 };
        private static readonly byte[] JPG = { 255, 216, 255 };
        private static readonly byte[] ICO = { 0, 0, 1, 0 };
        private static readonly byte[] TIFF = { 73, 73, 42, 0 };

        private static readonly byte[] DOC = { 208, 207, 17, 224, 161, 177, 26, 225 };
        private static readonly byte[] EXE_DLL = { 77, 90 };
        private static readonly byte[] MP3 = { 73, 68, 51 };
        private static readonly byte[] OGG = { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly byte[] PDF = { 37, 80, 68, 70, 45, 49, 46 };
        private static readonly byte[] RAR = { 82, 97, 114, 33, 26, 7, 0 };
        private static readonly byte[] SWF = { 70, 87, 83 };
        private static readonly byte[] TORRENT = { 100, 56, 58, 97, 110, 110, 111, 117, 110, 99, 101 };
        private static readonly byte[] TTF = { 0, 1, 0, 0, 0 };
        private static readonly byte[] WAV_AVI_WEBP = { 82, 73, 70, 70 };
        private static readonly byte[] WMV_WMA = { 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 };
        private static readonly byte[] ZIP_DOCX = { 80, 75, 3, 4 };

        public static string GetMimeTypeByFileName(string fileName)
        {
            //get file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension.Length > 0 &&
                MIMETypesDictionary.ContainsKey(extension.Remove(0, 1)))
            {
                return MIMETypesDictionary[extension.Remove(0, 1)];
            }

            //return "unknown/unknown";
            return "application/octet-stream";
        }

        public static string GetMimeType(byte[] file)
        {
            return GetMimeTypeWithByteArray(file, string.Empty);
        }

        public static string GetImageExtension(string type)
        {
            if (string.IsNullOrEmpty(type) == false)
            {
                var found = MIMETypesDictionary.Where(x => x.Value.ToLower() == type.ToLower());
                if (found != null && found.Any())
                {
                    if (found.First().Value.ToLower().Contains("image/") == true)
                        return "." + found.First().Key;
                }
            }
            return string.Empty;
        }

        public static bool IsImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) == false)
            {
                string type = GetMimeTypeWithByteArray(fileName);

                if (type.ToLower().IndexOf("image") == 0)
                    return true;
            }

            return false;
        }

        public static bool IsImage(byte[] fileBytes)
        {
            if (fileBytes != null && fileBytes.Length > 0)
            {
                string type = GetMimeTypeWithByteArray(fileBytes, string.Empty);

                if (type.ToLower().IndexOf("image") == 0)
                    return true;
            }

            return false;
        }

        //public static bool IsImage(Stream fileStream)
        //{
        //    if (fileStream != null)
        //        return IsImage(ImageResizer.Util.StreamUtils.CopyToBytes(fileStream));
        //    else
        //        return false;
        //}

        public static bool IsWebpImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) == false)
            {
                string type = GetMimeTypeWithByteArray(fileName);

                if (type.ToLower().IndexOf("image") == 0
                    && type.ToLower().Contains("webp") == true)
                    return true;
            }

            return false;
        }

        public static bool IsWebpImage(byte[] fileBytes)
        {
            if (fileBytes != null && fileBytes.Length > 0)
            {
                string type = GetMimeTypeWithByteArray(fileBytes, string.Empty);

                if (type.ToLower().IndexOf("image") == 0
                    && type.ToLower().Contains("webp") == true)
                    return true;
            }

            return false;
        }

        //public static bool IsWebpImage(Stream fileStream)
        //{
        //    if (fileStream != null)
        //        return IsWebpImage(ImageResizer.Util.StreamUtils.CopyToBytes(fileStream));
        //    else
        //        return false;
        //}

        public static string GetMimeTypeWithByteArray(byte[] fileBytes, string fileName)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                if (string.IsNullOrEmpty(fileName))
                    return string.Empty;
            }

            string mime = ""; //DEFAULT UNKNOWN MIME TYPE
            /*
            //Ensure that the filename isn't empty or null
            if (string.IsNullOrEmpty(fileName))
            {
                return mime;
            }
            */

            //Get the file extension
            string extension = Path.GetExtension(fileName) == null
                                   ? string.Empty
                                   : Path.GetExtension(fileName).ToUpper();

            //Get the MIME Type
            if (fileBytes.Take(2).SequenceEqual(BMP))
            {
                mime = "image/bmp";
            }
            else if (fileBytes.Take(4).SequenceEqual(GIF))
            {
                mime = "image/gif";
            }
            else if (fileBytes.Take(4).SequenceEqual(ICO))
            {
                mime = "image/x-icon";
            }
            else if (fileBytes.Take(3).SequenceEqual(JPG))
            {
                mime = "image/jpeg";
            }
            else if (fileBytes.Take(16).SequenceEqual(PNG))
            {
                mime = "image/png";
            }
            else if (fileBytes.Take(4).SequenceEqual(TIFF))
            {
                mime = "image/tiff";
            }

            else if (fileBytes.Take(8).SequenceEqual(DOC))
            {
                mime = "application/msword";
            }
            else if (fileBytes.Take(2).SequenceEqual(EXE_DLL))
            {
                mime = "application/x-msdownload"; //both use same mime type
            }

            else if (fileBytes.Take(3).SequenceEqual(MP3))
            {
                mime = "audio/mpeg";
            }
            else if (fileBytes.Take(14).SequenceEqual(OGG))
            {
                if (extension == ".OGX")
                {
                    mime = "application/ogg";
                }
                else if (extension == ".OGA")
                {
                    mime = "audio/ogg";
                }
                else
                {
                    mime = "video/ogg";
                }
            }
            else if (fileBytes.Take(7).SequenceEqual(PDF))
            {
                mime = "application/pdf";
            }
            else if (fileBytes.Take(7).SequenceEqual(RAR))
            {
                mime = "application/x-rar-compressed";
            }
            else if (fileBytes.Take(3).SequenceEqual(SWF))
            {
                mime = "application/x-shockwave-flash";
            }
            else if (fileBytes.Take(11).SequenceEqual(TORRENT))
            {
                mime = "application/x-bittorrent";
            }
            else if (fileBytes.Take(5).SequenceEqual(TTF))
            {
                mime = "application/x-font-ttf";
            }
            else if (fileBytes.Take(4).SequenceEqual(WAV_AVI_WEBP))
            {
                if (extension == ".WEBP" || extension == ".PNG" || extension == ".JPEG"
                    || extension == ".JPG" || extension == ".BMP" || extension == ".GIF")
                    mime = "image/webp";
                else
                    mime = extension == ".AVI" ? "video/x-msvideo" : "audio/x-wav";
            }
            else if (fileBytes.Take(16).SequenceEqual(WMV_WMA))
            {
                mime = extension == ".WMA" ? "audio/x-ms-wma" : "video/x-ms-wmv";
            }
            else if (fileBytes.Take(4).SequenceEqual(ZIP_DOCX))
            {
                mime = extension == ".DOCX" ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" : "application/x-zip-compressed";
            }

            if (string.IsNullOrEmpty(mime))
            {
                if (string.IsNullOrEmpty(fileName) == false)
                    mime = GetMimeTypeByFileName(fileName);
            }

            if (string.IsNullOrEmpty(mime))
                mime = "application/octet-stream";
            return mime;
        }

        public static string GetMimeTypeWithByteArray(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                byte[] buffer = new byte[256];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int bytesRead = fs.Read(buffer, 0, Math.Min(256, (int)fs.Length));
                }

                return GetMimeTypeWithByteArray(buffer, Path.GetFileName(filePath));
            }
            return string.Empty;
        }

        public static string GetExtensionWithByteArray(byte[] fileBytes, string fileName)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                if (string.IsNullOrEmpty(fileName))
                    return string.Empty;
            }

            string mime = ""; //DEFAULT UNKNOWN MIME TYPE
            /*
            //Ensure that the filename isn't empty or null
            if (string.IsNullOrEmpty(fileName))
            {
                return mime;
            }
            */

            //Get the file extension
            string extension = Path.GetExtension(fileName) == null
                                   ? string.Empty
                                   : Path.GetExtension(fileName).ToUpper();

            //Get the MIME Type
            if (fileBytes.Take(2).SequenceEqual(BMP))
            {
                mime = ".bmp";
            }
            else if (fileBytes.Take(4).SequenceEqual(GIF))
            {
                mime = ".gif";
            }
            else if (fileBytes.Take(4).SequenceEqual(ICO))
            {
                mime = ".ico";
            }
            else if (fileBytes.Take(3).SequenceEqual(JPG))
            {
                mime = ".jpg";
            }
            else if (fileBytes.Take(16).SequenceEqual(PNG))
            {
                mime = ".png";
            }
            else if (fileBytes.Take(4).SequenceEqual(TIFF))
            {
                mime = ".tiff";
            }

            else if (fileBytes.Take(8).SequenceEqual(DOC))
            {
                mime = ".doc";
            }

            else if (fileBytes.Take(3).SequenceEqual(MP3))
            {
                mime = ".mp3";
            }

            else if (fileBytes.Take(7).SequenceEqual(PDF))
            {
                mime = ".pdf";
            }
            else if (fileBytes.Take(7).SequenceEqual(RAR))
            {
                mime = ".rar";
            }
            else if (fileBytes.Take(3).SequenceEqual(SWF))
            {
                mime = ".swf";
            }
            else if (fileBytes.Take(11).SequenceEqual(TORRENT))
            {
                mime = ".torrent";
            }
            else if (fileBytes.Take(5).SequenceEqual(TTF))
            {
                mime = ".ttf";
            }

            if (string.IsNullOrEmpty(mime))
                mime = extension;

            return mime;
        }

        public static string GetExtensionWithByteArray(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) == false)
            {
                if (File.Exists(filePath))
                {
                    byte[] buffer = new byte[256];
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        if (fs.Length >= 256)
                            fs.Read(buffer, 0, 256);
                        else
                            fs.Read(buffer, 0, (int)fs.Length);
                    }

                    return GetExtensionWithByteArray(buffer, Path.GetFileName(filePath));
                }
            }

            return string.Empty;
        }
    }
}