using ImageResizer;
using Newtonsoft.Json;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

//-----------------------------------PROGRAMER LOGS----------------------------------

namespace SweetSoft.QLDA.BackOffice._RFMng.connectors
{

    public partial class filemanager : System.Web.UI.Page
    {
        public static string defaultPath = "~/Uploads/";
        static string loadPath = string.Empty;
        static string queryPath = string.Empty;
        public string IconDirectory = "/_RFMng/images/fileicons/";
        static Dictionary<string, Dictionary<CMSImageType, string>> DicResizeSetting = new Dictionary<string, Dictionary<CMSImageType, string>>() {
              {
                    "Uploads\\Videos", new Dictionary<CMSImageType, string>() {
                        { CMSImageType.Video, "Uploads\\Videos" }
                    }
               },
            {
                    "Uploads", new Dictionary<CMSImageType, string>() {
                        { CMSImageType.Normal, "Uploads" },
                        { CMSImageType.SocialNetwork, "Uploads\\SocialNetwork" },
                    }
               }
        };
        private static void LogSecurityEvent(string message, string details)
        {
            string logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [FileManager Security] {message}: {details}";

            // Log to multiple destinations
            System.Diagnostics.Trace.TraceWarning(logEntry);

            // Additional logging to file
            try
            {
                string logDirectory = HttpContext.Current.Server.MapPath("~/_Logs/FileManager");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFilePath = Path.Combine(logDirectory, "security.log");

                using (var writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{logEntry} - User: {HttpContext.Current?.User?.Identity?.Name ?? "Anonymous"}");
                }
            }
            catch
            {
                // Fail silently for logging
            }
        }
        private bool IsSafeToDelete(string directoryPath)
        {
            try
            {
                var directory = new DirectoryInfo(directoryPath);

                // Check for protected files
                foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (IsProtectedFile(file.FullName))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool IsProtectedFile(string filePath)
        {
            string[] protectedFiles = { "web.config", ".htaccess", "global.asax", "web.config.bak" };
            string fileName = Path.GetFileName(filePath).ToLowerInvariant();

            return protectedFiles.Contains(fileName);
        }

        static Dictionary<CMSImageType, string> GetResizeSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            foreach (KeyValuePair<string, Dictionary<CMSImageType, string>> keyValuePairString in DicResizeSetting)
            {
                if (keyValuePairString.Key.ToLower() == key.ToLower())
                {
                    return keyValuePairString.Value;
                }
            }
            return null;
        }

        Dictionary<string, object> GetStorageStatic(string fullPhysicalPath)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            string folderPath = GetLoadPath();
            if (string.IsNullOrEmpty(fullPhysicalPath) || fullPhysicalPath.Length < folderPath.Length)
                fullPhysicalPath = folderPath;

            if (Directory.Exists(fullPhysicalPath) == false)
                return dic;

            if (string.IsNullOrEmpty(fullPhysicalPath) == false)
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(fullPhysicalPath);

                    Int32 folderCount = 0;
                    var found = Directory.GetDirectories(dirInfo.FullName, "*", SearchOption.AllDirectories);
                    if (found != null)
                        folderCount = found.Length;

                    Int64 length = 0;
                    Int32 fileCount = 0;

                    foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        if (fileInfo.Name.ToLower() == ".htaccess"
                            || fileInfo.Name.ToLower() == "web.config")
                            continue;

                        length += fileInfo.Length;
                        fileCount++;
                    }

                    dic.Add("Code", 0);
                    dic.Add("Error", "");
                    dic.Add("Files", fileCount);
                    dic.Add("Folders", folderCount);
                    dic.Add("Size", length);
                }
                catch (Exception ex)
                {
                    dic.Add("Code", -1);
                    dic.Add("Error", "");
                    dic.Add("Files", 0);
                    dic.Add("Folders", 0);
                    dic.Add("Size", 0);
                }
            }

            return dic;
        }

        static string RenderPaging(int currentPage, int totalRow, int pageSize)
        {
            StringBuilder sbPaging = new StringBuilder();

            sbPaging.Append("<div class='fm-container'>");

            int totalPage = totalRow / pageSize;
            if (totalRow % pageSize > 0)
                totalPage += 1;

            if (totalPage > 1)
            {
                sbPaging.Append("<button class='btn-paging' title='Go to page 1'>");
                sbPaging.Append("<span data-indx='1'>");
                sbPaging.Append("<<");
                sbPaging.Append("</span>");
                sbPaging.Append("</button>");

                if (currentPage != 1)
                {
                    sbPaging.Append("<button class='btn-paging' title='Go to page " + (currentPage - 1) + "'>");
                    sbPaging.Append("<span data-indx='" + (currentPage - 1) + "'>");
                    sbPaging.Append("<");
                    sbPaging.Append("</span>");
                    sbPaging.Append("</button>");
                }

                sbPaging.Append("<div class='paging-info'>");
                sbPaging.Append("<p style='margin:0;color:#231F20'>");
                sbPaging.Append("<span>" + currentPage + "/</span>");
                sbPaging.Append("<span>" + totalPage + "</span>");
                sbPaging.Append("</p>");
                sbPaging.Append("</div>");

                if (totalPage > currentPage)
                {
                    sbPaging.Append("<button class='btn-paging' title='Go to page " + (currentPage + 1) + "'>");
                    sbPaging.Append("<span data-indx='" + (currentPage + 1) + "'>");
                    sbPaging.Append(">");
                    sbPaging.Append("</span>");
                    sbPaging.Append("</button>");
                }

                sbPaging.Append("<button class='btn-paging' title='Go to page " + totalPage + "'>");
                sbPaging.Append("<span data-indx='" + totalPage + "'>");
                sbPaging.Append(">>");
                sbPaging.Append("</span>");
                sbPaging.Append("</button>");
            }

            sbPaging.Append("</div>");

            return sbPaging.ToString();
        }

        private string getFolderInfo(string fullPhysicalPath)
        {
            int pageIndex = 1;
            if (string.IsNullOrEmpty(Request.QueryString["pi"]) == false)
            {
                if (int.TryParse(Request.QueryString["pi"], out pageIndex) == false)
                    pageIndex = 1;
            }

            if (pageIndex < 1)
                pageIndex = 1;

            int pageSize = 20;
            if (string.IsNullOrEmpty(Request.QueryString["ps"]) == false)
            {
                if (int.TryParse(Request.QueryString["ps"], out pageSize) == false)
                    pageSize = 20;
            }

            if (pageSize < 1)
                pageSize = 20;

            DirectoryInfo RootDirInfo = new DirectoryInfo(fullPhysicalPath);
            if (RootDirInfo.Exists == true)
            {
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();

                //string storagePhysicalPath = GetLoadPath().TrimEnd('\\');
                string storagePhysicalPath = HttpContext.Current.Server.MapPath(defaultPath).TrimEnd('\\');
                foreach (DirectoryInfo dirInfo in RootDirInfo.GetDirectories())
                {
                    lstData.Add(getInfoDictionary(storagePhysicalPath, dirInfo.FullName, true));
                }

                int count = RootDirInfo.GetFiles().Length;

                foreach (FileInfo fileInfo in RootDirInfo.GetFiles().Skip(pageSize * (pageIndex - 1)).Take(pageSize))
                {
                    if (fileInfo.Name.ToLower() == ".htaccess"
                        || fileInfo.Name.ToLower() == "web.config")
                        continue;

                    lstData.Add(getInfoDictionary(storagePhysicalPath, fileInfo.FullName, false));
                }

                if (count > pageSize)
                    lstData.Add(new Dictionary<string, object> { { "Paging", RenderPaging(pageIndex, count, pageSize) } });
                else
                    lstData.Add(new Dictionary<string, object> { { "Paging", "" } });

                return JsonConvert.SerializeObject(lstData);
            }
            else
            {
                return JsonConvert.SerializeObject(new
                {
                    Error = "The folder '" + fullPhysicalPath + "' does not exists.",
                    Code = -1
                });
            }
        }

        private Dictionary<string, object> getInfoDictionary(string sitePhysicalPath,
            string path, bool getInfo)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(sitePhysicalPath))
                return dic;

            FileAttributes attr = File.GetAttributes(path);

            string virtualPath = path.Remove(0, sitePhysicalPath.Length).Replace("\\", "/");
            if (!virtualPath.StartsWith("~/Uploads") && !virtualPath.StartsWith("/Uploads") && !virtualPath.StartsWith("Uploads"))
                virtualPath = "Uploads/" + virtualPath.TrimStart('/');
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);

                Dictionary<string, object> dicProp = new Dictionary<string, object>();
                dicProp.Add("Created date", dirInfo.CreationTime.ToString());
                dicProp.Add("Date Modified", dirInfo.LastWriteTime.ToString());
                string sortModified = dirInfo.LastWriteTime.ToString("yyyyMMdd-hh:mm:ss tt");
                dicProp.Add("Sort Modified", sortModified);
                dicProp.Add("Height", 0);
                dicProp.Add("Width", 0);
                dicProp.Add("Size", 0);

                dic.Add("ForcePath", "/" + path.Remove(0, Server.MapPath("~").Length).Replace("\\","/"));
                dic.Add("Path", "/" + virtualPath.Trim('/') + (getInfo ? "/" : ""));
                dic.Add("Filename", dirInfo.Name);
                dic.Add("File Type", "dir");
                dic.Add("Preview", IconDirectory + "_Close.png");
                dic.Add("Error", "No error");
                dic.Add("Code", 0);
                dic.Add("Properties", dicProp);
                dic.Add("Protected", 0);
            }
            else
            {
                FileInfo fileInfo = new FileInfo(path);

                string relativePath = path.Replace(Server.MapPath(defaultPath), "").Replace("\\", "/").TrimStart('/');

                if (getInfo)
                    dic["Path"] = "/" + fileInfo.Name.TrimStart('/');
                else
                {
                    string temp = $"{queryPath.TrimEnd('/')}/{relativePath}";

                    if (!temp.StartsWith("/"))
                        temp = "/" + temp;
                    dic["Path"] = temp.TrimStart('/');
                }
                dic["ForcePath"] = "/" + virtualPath.TrimStart('/').Replace(fileInfo.Name, "");
                dic["Filename"] = fileInfo.Name;
                dic["File Type"] = fileInfo.Extension.TrimStart('.');
                dic["Error"] = "No error";
                dic["Code"] = 0;

                if (MIMEAssistant.IsImage(fileInfo.FullName))
                {
                    dic["Preview"] = "/" + virtualPath.TrimStart('/');
                }
                else
                {
                    string mime = MIMEAssistant.GetMimeTypeByFileName(fileInfo.Name);
                    string iconFile = $"{IconDirectory}{fileInfo.Extension.TrimStart('.')}.png";

                    dic["Preview"] = iconFile;

                    if (!string.IsNullOrEmpty(mime) && mime.ToLower().StartsWith("video"))
                    {
                        dic["videourl"] = "/" + virtualPath.TrimStart('/');
                    }
                }

                Dictionary<string, object> dicProp = new Dictionary<string, object>();
                dicProp.Add("Created date", fileInfo.CreationTime.ToString());
                dicProp.Add("Date Modified", fileInfo.LastWriteTime.ToString());
                string sortModified = fileInfo.LastWriteTime.ToString("yyyyMMdd-hh:mm:ss tt");
                dicProp.Add("Sort Modified", sortModified);

                if (MIMEAssistant.IsImage(path))
                {
                    try
                    {
                        using (System.Drawing.Image img = System.Drawing.Image.FromFile(path))
                        {
                            if (img != null)
                            {
                                dicProp.Add("Height", img.Height);
                                dicProp.Add("Width", img.Width);
                            }
                            else
                            {
                                dicProp.Add("Height", 0);
                                dicProp.Add("Width", 0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        dicProp.Add("Height", 0);
                        dicProp.Add("Width", 0);
                    }
                }
                else
                {
                    dicProp.Add("Height", 0);
                    dicProp.Add("Width", 0);
                }
                dicProp.Add("Size", fileInfo.Length);
                dic.Add("Properties", dicProp);
                dic.Add("Protected", 0);
            }

            return dic;
        }

        private string getInfo(string fullPhysicalPath)
        {
            if (string.IsNullOrEmpty(fullPhysicalPath))
                return JsonConvert.SerializeObject(new { });

            // Normalize path - remove trailing slash for consistent checking
            string normalizedPath = fullPhysicalPath.TrimEnd('/');

            bool fileExists = File.Exists(normalizedPath);
            bool directoryExists = Directory.Exists(normalizedPath);

            // Determine if it's a file or directory
            bool isDirectory = false;
            bool exists = false;

            if (fileExists && directoryExists)
            {
                // Both exist - use file attributes to determine type
                FileAttributes attr = File.GetAttributes(normalizedPath);
                isDirectory = (attr & FileAttributes.Directory) == FileAttributes.Directory;
                exists = true;
            }
            else if (fileExists)
            {
                isDirectory = false;
                exists = true;
            }
            else if (directoryExists)
            {
                isDirectory = true;
                exists = true;
            }
            else
            {
                // Neither exists - try to determine intended type from original path
                isDirectory = fullPhysicalPath.EndsWith("/");
                exists = false;
            }

            if (exists)
            {
                Dictionary<string, object> dicData = null;

                if (isDirectory)
                {
                    string storagePhysicalPath = GetLoadPath();
                    dicData = getInfoDictionary(storagePhysicalPath, normalizedPath, true);
                }
                else
                {
                    string storagePhysicalPath = HttpContext.Current.Server.MapPath(defaultPath);
                    dicData = getInfoDictionary(storagePhysicalPath, normalizedPath, true);
                }

                if (dicData != null)
                    return JsonConvert.SerializeObject(dicData);
                else
                    return JsonConvert.SerializeObject(new
                    {
                        Error = $"The {(isDirectory ? "folder" : "file")} '{fullPhysicalPath}' could not be processed.",
                        Code = -1
                    });
            }
            else
            {
                return JsonConvert.SerializeObject(new
                {
                    Error = $"The {(isDirectory ? "folder" : "file")} '{fullPhysicalPath}' does not exist.",
                    Code = -1
                });
            }
        }

        private static string NormalizeAndDecodePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            // Decode URL một cách an toàn
            try
            {
                path = HttpUtility.UrlDecode(path);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid URL encoding");
            }

            // Blacklist các ký tự và pattern nguy hiểm
            //string[] dangerousPatterns = { "..", "%2e", "%2f", "%5c", "~", "..\\", "../", "...", "..\\" };
            //foreach (string pattern in dangerousPatterns)
            //{
            //    if (path.Contains(pattern))
            //    {
            //        throw new ArgumentException("Path contains dangerous characters");
            //    }
            //}

            // Chỉ cho phép các ký tự an toàn
            //if (!Regex.IsMatch(path, @"^[a-zA-Z0-9/\\_\-\s]*$"))
            //{
            //    throw new ArgumentException("Path contains invalid characters");
            //}

            path = path.Replace("/", "\\");
            path = Regex.Replace(path, @"\\+", "\\"); // collapse multiple slashes
            return path.Trim('\\');
        }

        private static bool IsRootOrAbove(string path)
        {
            string root = HttpContext.Current.Server.MapPath(defaultPath);
            return string.Equals(path.TrimEnd('\\', '/'), root.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
        }

        private static string SecureCorrectPath(string path)
        {
            string rootFolder = HttpContext.Current.Server.MapPath(defaultPath);

            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return rootFolder;

                // Validate và normalize path
                path = NormalizeAndDecodePath(path);

                // Loại bỏ prefix không cần thiết
                path = path.Replace("Uploads", string.Empty).Trim();

                // Tạo full path
                string resolvedPath = Path.GetFullPath(Path.Combine(rootFolder, path.TrimStart('~', '\\', '/')));

                // Kiểm tra path traversal
                if (!resolvedPath.StartsWith(rootFolder, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Path traversal detected");
                }

                // Kiểm tra độ dài path
                //if (resolvedPath.Length > 260) // Windows MAX_PATH
                //{
                //    throw new ArgumentException("Path too long");
                //}

                return resolvedPath;
            }
            catch (Exception ex)
            {
                LogSecurityEvent("Path validation failed", path);
                throw new SecurityException("Invalid path provided", ex);
            }
        }

        private string Rename(string path, string newName)
        {
            if (SweetContext.Current == null || SweetContext.Current.User == null)
            {
                Response.Redirect("/403", true);
                return "";
            }

            string physicalPath = SecureCorrectPath(path);
            newName = PathValidation.CleanFileName(newName);
            newName = FileHelpers.NormalizeFileName(newName);

            if (IsRootOrAbove(path))
            {
                LogSecurityEvent("Attempted rename of root directory", path);
                return JsonConvert.SerializeObject(new { Error = "Cannot rename root directory.", Code = -1 });
            }

            Dictionary<string, object> dic = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(newName))
                return JsonConvert.SerializeObject(dic);


            bool isExist = false;
            bool isFile = false;
            if (Path.GetExtension(physicalPath).Length == 0)
                isExist = Directory.Exists(physicalPath);
            else
            {
                isExist = File.Exists(physicalPath);
                isFile = true;
            }

            if (isExist)
            {
                FileAttributes attr = File.GetAttributes(physicalPath);

                dic.Add("Error", "No error");
                dic.Add("Code", 0);
                dic.Add("Old Path", path);
                dic.Add("New Path", string.Empty);
                dic.Add("New Name", string.Empty);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string _oldPath = string.Empty;
                    newName = PathValidation.CleanPath(newName);
                    DirectoryInfo dirInfo = new DirectoryInfo(physicalPath);
                    string oldName = dirInfo.Name;
                    if (newName == oldName)
                    {
                        dic["Error"] = "Source and destination path must be different.";
                        dic["Code"] = -1;
                    }
                    else
                    {
                        if (dirInfo != null)
                        {
                            if (!FileHelpers.IsFileAllowed(newName))
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    Error = "New name is invalid.",
                                    Code = -1
                                });
                            }

                            _oldPath = dirInfo.FullName;
                            dirInfo.MoveTo(Path.Combine(dirInfo.Parent.FullName, newName));
                        }
                        dic["Old Path"] = "/" + MakeRelativePath(_oldPath, true).Trim('/') + "/";
                        dic["Old Name"] = oldName;
                        dic["New Path"] = "/" + MakeRelativePath(dirInfo.FullName, true).Trim('/') + "/";
                        dic["New Name"] = dirInfo.Name;
                    }
                }
                else
                {
                    newName = PathValidation.CleanFileName(newName);
                    newName = FileHelpers.NormalizeFileName(newName);

                    FileInfo fileInfo = new FileInfo(physicalPath);
                    string oldName = fileInfo.Name;

                    if (Path.GetExtension(oldName) != Path.GetExtension(newName))
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            Error = "Extension is invalid.",
                            Code = -1
                        });
                    }

                    if (newName == oldName)
                    {
                        dic["Error"] = "Source and destination path must be different.";
                        dic["Code"] = -1;
                    }
                    else
                    {
                        if (!FileHelpers.IsFileAllowed(newName))
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                Error = "New name is invalid.",
                                Code = -1
                            });
                        }

                        string _oldFile = string.Empty;
                        if (fileInfo != null)
                        {
                            _oldFile = fileInfo.FullName;
                            fileInfo.MoveTo(Path.Combine(fileInfo.Directory.FullName, newName));
                        }

                        string op = "/" + MakeRelativePath(fileInfo.FullName, false).TrimStart('/');
                        string foldertrim = "/" + queryPath.Trim('/');
                        if (foldertrim.Length > 1)
                            op = op.Remove(0, foldertrim.Length);
                        string op2 = "/" + MakeRelativePath(_oldFile, false).TrimStart('/');
                        if (foldertrim.Length > 1)
                            op2 = op2.Remove(0, foldertrim.Length);

                        dic["Old Path"] = op;
                        dic["Old Name"] = oldName;
                        dic["New Path"] = op2;
                        dic["New Name"] = fileInfo.Name;
                    }
                }

                return JsonConvert.SerializeObject(dic);
            }
            else
            {
                if (isFile)
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "The file '" + physicalPath + "' does not exists.",
                        Code = -1
                    });
                else
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "The folder '" + physicalPath + "' does not exists.",
                        Code = -1
                    });
            }
        }

        private string Delete(string securePath)
        {
            try
            {
                if (SweetContext.Current?.User == null)
                {
                    Response.Redirect("/403", true);
                    return "";
                }
                // Prevent deletion of root directory
                if (IsRootOrAbove(securePath))
                {
                    LogSecurityEvent("Attempted delete of root directory", securePath);
                    throw new UnauthorizedAccessException("Cannot delete root directory");
                }

                // Check if file/folder exists
                if (!File.Exists(securePath) && !Directory.Exists(securePath))
                {
                    throw new FileNotFoundException("File or directory not found");
                }

                FileAttributes attr = File.GetAttributes(securePath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // Check if directory is empty or contains only allowed files
                    if (!IsSafeToDelete(securePath))
                    {
                        throw new UnauthorizedAccessException("Directory contains protected files");
                    }
                    if (Directory.Exists(securePath))
                        Directory.Delete(securePath, true);
                    return JsonConvert.SerializeObject(new
                    {
                        Path = "/" + MakeRelativePath(securePath, true).TrimStart('/'),
                        Error = "No error",
                        Code = 0
                    });
                }
                else
                {
                    if (File.Exists(securePath))
                        File.Delete(securePath);
                    string fileName = MakeRelativePath(securePath, false);
                    if (!fileName.StartsWith("/Uploads") && !fileName.StartsWith("Uploads"))
                        fileName = "Uploads/" + fileName.Trim('/');
                    return JsonConvert.SerializeObject(new
                    {
                        Path = fileName,
                        Error = "No error",
                        Code = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LogSecurityEvent("Delete operation failed", ex.Message);
                return JsonConvert.SerializeObject(new
                {
                    Error = "Delete operation failed",
                    Code = -1
                });
            }
        }

        private string AddFolder(string fullPhysicalPath, string NewFolder)
        {
            if (SweetContext.Current == null || SweetContext.Current.User == null)
            {
                Response.Redirect("/403", true);
                return "";
            }
            if (string.IsNullOrEmpty(NewFolder) || NewFolder.Contains(".."))
                return "{}";

            NewFolder = PathValidation.CleanFileName(NewFolder);
            NewFolder = FileHelpers.NormalizeFileName(NewFolder);

            if (IsRootOrAbove(fullPhysicalPath) && Directory.Exists(Path.Combine(fullPhysicalPath, NewFolder)))
            {
                LogSecurityEvent("Attempted add folder at root", fullPhysicalPath);
                return JsonConvert.SerializeObject(new { Error = "Cannot add folder at root directory.", Code = -1 });
            }

            string newPath = Path.Combine(fullPhysicalPath, NewFolder);

            try
            {
                Directory.CreateDirectory(newPath);

                return JsonConvert.SerializeObject(new
                {
                    Parent = "/" + MakeRelativePath(fullPhysicalPath, true).TrimStart('/'),
                    Name = NewFolder,
                    Error = "No error",
                    Code = 0
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    Error = "Cannot create folder '" + fullPhysicalPath + "'",
                    Code = -1
                });
            }
        }

        public static string MakeRelativePath(string physicalPath, bool isFolder)
        {
            string storagePhysicalPath = string.Empty;
            if (isFolder)
                storagePhysicalPath = GetLoadPath();
            else
                storagePhysicalPath = HttpContext.Current.Server.MapPath("~/");
            return physicalPath.Remove(0, storagePhysicalPath.Length).Replace("\\", "/");
        }
        public static string GetLoadPath()
        {
            if (string.IsNullOrEmpty(loadPath))
                return HttpContext.Current.Server.MapPath(defaultPath);
            else
                return loadPath;
        }
        public static void ResetPath()
        {
            loadPath = string.Empty;
        }
        void SaveFile()
        {
            int MAX_FILE_SIZE = 5 * 1024 * 1024; // Default 5MB
            if(!AppSettingHelpers.GetSetting("UploadMaxFileSize", out MAX_FILE_SIZE))
                MAX_FILE_SIZE = 5 * 1024 * 1024; // Default to 5MB if setting not found
            try
            {
                if (SweetContext.Current?.User == null)
                {
                    Response.Redirect("/403", true);
                    return;
                }

                // Validate request
                if (Request.Files.Count != 1)
                {
                    throw new ArgumentException("Invalid number of files");
                }

                System.Web.HttpPostedFile file = Request.Files[0];

                if (file == null || file.ContentLength == 0)
                {
                    throw new ArgumentException("No file provided");
                }

                // Validate file size
                if (file.ContentLength > MAX_FILE_SIZE)
                {
                    throw new ArgumentException("File too large");
                }

                // Validate filename
                string originalFileName = Path.GetFileName(file.FileName);
                if (string.IsNullOrEmpty(originalFileName))
                {
                    throw new ArgumentException("Invalid filename");
                }

                // Secure path handling
                string folderPath = Request["currentpath"] ?? Request["path"];
                if (string.IsNullOrEmpty(folderPath) || folderPath.TrimStart('~') == "/")
                    folderPath = Request["folder"];
                string fullPhysicalPath = SecureCorrectPath(folderPath);

                // Clean filename
                string cleanFileName = PathValidation.CleanFileName(originalFileName);
                string finalFileName = FileHelpers.ChangeFileName(cleanFileName);
                finalFileName = FileHelpers.NormalizeFileName(finalFileName);
                // Validate file extension
                string extension = Path.GetExtension(finalFileName).ToLowerInvariant();
                if (!IsAllowedExtension(extension))
                {
                    throw new ArgumentException("File type not allowed");
                }

                string targetPath = Path.Combine(fullPhysicalPath, finalFileName);

                // Read and validate file content
                byte[] fileData;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileData = binaryReader.ReadBytes(file.ContentLength);
                }

                // Enhanced MIME validation
                if (!ValidateFileContent(fileData, originalFileName))
                {
                    throw new SecurityException("File content validation failed");
                }

                // Process file with resize settings
                ProcessAndSaveFile(fileData, targetPath, originalFileName, fullPhysicalPath);

                // Success response
                Response.ContentType = "application/json";
                Response.ContentEncoding = Encoding.UTF8;
                Response.Write(JsonConvert.SerializeObject(new
                {
                    Path = folderPath,
                    Name = finalFileName,
                    Error = "No error",
                    Code = 0
                }));
            }
            catch (Exception ex)
            {
                LogSecurityEvent("File upload failed", ex.Message + " " + ex.StackTrace);
                Response.Write(JsonConvert.SerializeObject(new
                {
                    Error = $"File upload failed: {ex.Message}",
                    Code = -1
                }));
            }
        }
        private bool ValidateFileContent(byte[] fileData, string fileName)
        {
            try
            {
                // Check file size again
                if (fileData.Length == 0)
                    return false;

                // Get MIME type from content
                string mimeType = MIMEAssistant.GetMimeTypeWithByteArray(fileData, fileName);
                string extension = Path.GetExtension(fileName).ToLowerInvariant();

                // Validate MIME type matches extension
                if (!IsValidMimeTypeForExtension(mimeType, extension))
                {
                    return false;
                }

                // Use stream for detailed validation
                using (var stream = new MemoryStream(fileData))
                {
                    return FileHelpers.IsFileAllowed(stream, fileName, mimeType) &&
                           ValidateFileHeaders(fileData, extension);
                }
            }
            catch (Exception ex)
            {
                LogSecurityEvent("File content validation error", ex.Message);
                return false;
            }
        }
        public class FileHeaderRule
        {
            public byte[] Signature { get; set; }
            public int Offset { get; set; }
        }

        private static readonly Dictionary<string, List<FileHeaderRule>> AllowedHeaders =
            new Dictionary<string, List<FileHeaderRule>>(StringComparer.OrdinalIgnoreCase)
            {
                // Ảnh
                [".jpg"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0xFF, 0xD8, 0xFF }, Offset = 0 }, // JPEG
        new FileHeaderRule { Signature = new byte[] { 0x52, 0x49, 0x46, 0x46 }, Offset = 0 }, // RIFF (WebP)
            },
                [".jpeg"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0xFF, 0xD8, 0xFF }, Offset = 0 },
        new FileHeaderRule { Signature = new byte[] { 0x52, 0x49, 0x46, 0x46 }, Offset = 0 },
            },
                [".webp"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x52, 0x49, 0x46, 0x46 }, Offset = 0 }, // RIFF
        new FileHeaderRule { Signature = new byte[] { 0xFF, 0xD8, 0xFF }, Offset = 0 }, // Cho phép JPEG rename
            },
                [".png"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x89, 0x50, 0x4E, 0x47 }, Offset = 0 }
            },
                [".gif"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x47, 0x49, 0x46 }, Offset = 0 }
            },

                // Văn bản
                [".pdf"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x25, 0x50, 0x44, 0x46 }, Offset = 0 }
            },
                [".doc"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }, Offset = 0 }
            },
                [".xls"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }, Offset = 0 }
            },
                [".ppt"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }, Offset = 0 }
            },

                // Office OpenXML + zip
                [".docx"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x50, 0x4B, 0x03, 0x04 }, Offset = 0 }
            },
                [".xlsx"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x50, 0x4B, 0x03, 0x04 }, Offset = 0 }
            },
                [".pptx"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x50, 0x4B, 0x03, 0x04 }, Offset = 0 }
            },
                [".zip"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x50, 0x4B, 0x03, 0x04 }, Offset = 0 }
            },

                // Âm thanh / video
                [".mp3"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x49, 0x44, 0x33 }, Offset = 0 } // ID3
            },
                [".mp4"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x66, 0x74, 0x79, 0x70 }, Offset = 4 } // "ftyp"
            },
                [".avi"] = new List<FileHeaderRule>
            {
        new FileHeaderRule { Signature = new byte[] { 0x52, 0x49, 0x46, 0x46 }, Offset = 0 } // RIFF
            },
            };

        private bool ValidateFileHeaders(byte[] fileData, string extension)
        {
            if (extension == ".mp3")//Tam thoi cho pass mp3 file
                return true;
            if (!AllowedHeaders.TryGetValue(extension, out var rules)) return false;

            foreach (var rule in rules)
            {
                if (fileData.Length < rule.Offset + rule.Signature.Length)
                    continue;

                if (fileData.Skip(rule.Offset).Take(rule.Signature.Length).SequenceEqual(rule.Signature))
                    return true;
            }

            return false;
        }

        private bool IsAllowedExtension(string extension)
        {
            string[] allowedExtensions = {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf",
        ".doc", ".docx",
        ".xls", ".xlsx",
        ".ppt", ".pptx",
        ".zip",
        ".mp3", ".mp4", ".avi"
    };

            return allowedExtensions.Contains(extension.ToLowerInvariant());
        }

        private bool IsValidMimeTypeForExtension(string mimeType, string extension)
        {
            var validMimeTypes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg",   new[] { "image/jpeg" } },
        { ".jpeg",  new[] { "image/jpeg" } },
        { ".png",   new[] { "image/png" } },
        { ".gif",   new[] { "image/gif" } },
        { ".webp",  new[] { "image/webp" } },

        { ".pdf",   new[] { "application/pdf" } },

        { ".doc",   new[] { "application/msword" } },
        { ".docx",  new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/x-zip-compressed" } },

        { ".xls",   new[] { "application/vnd.ms-excel" } },
        { ".xlsx",  new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/x-zip-compressed" } },

        { ".ppt",   new[] { "application/vnd.ms-powerpoint" } },
        { ".pptx",  new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "application/x-zip-compressed" } },

        { ".zip",   new[] { "application/zip", "application/x-zip-compressed" } },

        { ".mp3",   new[] { "audio/mpeg" } },
        { ".mp4",   new[] { "video/mp4" } },
        { ".avi",   new[] { "video/x-msvideo" } }
    };

            return validMimeTypes.TryGetValue(extension.ToLowerInvariant(), out var validTypes)
                && validTypes.Contains(mimeType.ToLowerInvariant());
        }

        private void ProcessAndSaveFile(byte[] fileData, string targetPath, string originalFileName, string fullPhysicalPath)
        {
            string masterFolder = Server.MapPath(defaultPath);
            string key = fullPhysicalPath.Length > masterFolder.Length ?
                         fullPhysicalPath.Substring(masterFolder.Length).TrimStart('\\') :
                         string.Empty;

            // Kiểm tra loại file dựa trên extension
            string fileExtension = Path.GetExtension(originalFileName).ToLower();

            if (IsImageFile(fileExtension))
            {
                ProcessImageFile(fileData, targetPath, originalFileName, key, masterFolder);
            }
            else
            {
                ProcessNonImageFile(fileData, targetPath, originalFileName, fullPhysicalPath);
            }
        }

        private void ProcessImageFile(byte[] fileData, string targetPath, string originalFileName, string key, string masterFolder)
        {
            Dictionary<CMSImageType, string> dicResize = GetResizeSetting(key);
            if (dicResize == null)
            {
                using (var stream = new MemoryStream(fileData))
                {
                    SaveImage(stream, targetPath, string.Empty);
                }
            }
            else
            {
                foreach (KeyValuePair<CMSImageType, string> resizeSetting in dicResize)
                {
                    string folder = Path.Combine(masterFolder, resizeSetting.Value);
                    string fileName = Path.Combine(folder, FileHelpers.ChangeFileName(originalFileName));
                    fileName = FileHelpers.NextAvailableFilename(fileName);
                    using (var stream = new MemoryStream(fileData))
                    {
                        SaveImage(stream, fileName,
                                 Helpers.GetRenderAttribute(resizeSetting.Key).ToImageResizeString());
                    }
                }
            }
        }

        private void ProcessNonImageFile(byte[] fileData, string targetPath, string originalFileName, string fullPhysicalPath)
        {
            // Lưu file trực tiếp không cần xử lý resize
            string savePath = FileHelpers.NextAvailableFilename(targetPath);
            SaveNonImageFile(fileData, savePath);
        }

        private bool IsImageFile(string extension)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
            return imageExtensions.Contains(extension);
        }
        public static string SaveImage(Stream fileContent, string path, string settings)
        {
            string savePath = FileHelpers.NextAvailableFilename(path);
            string folder = Path.GetDirectoryName(savePath);

            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    LogSecurityEvent($"Error creating directory: {folder}", ex.StackTrace);
                }
            }

            if (fileContent.CanSeek)
                fileContent.Seek(0, SeekOrigin.Begin);

            bool useImageResizer = !string.IsNullOrEmpty(settings) &&
                                   (settings.Contains("width") || settings.Contains("height"));

            if (!useImageResizer)
            {
                using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    fileContent.CopyTo(fs);
                }
            }
            else
            {
                if (!settings.Contains("quality"))
                {
                    if (settings.Length > 0)
                        settings += "&";
                    settings += "quality=80";
                }

                using (var ms = new MemoryStream())
                {
                    ImageBuilder.Current.Build(new ImageJob(fileContent, ms,
                        new Instructions(settings), false, true));

                    using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(fs);
                    }
                }
            }

            return savePath;
        }

        public static string SaveNonImageFile(byte[] fileData, string path)
        {
            try
            {
                string savePath = FileHelpers.NextAvailableFilename(path);
                string folder = Path.GetDirectoryName(savePath);

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Lưu file trực tiếp
                File.WriteAllBytes(savePath, fileData);

                return savePath;
            }
            catch (Exception ex)
            {
                LogSecurityEvent($"Error saving file: {path}", ex.StackTrace);
                throw;
            }
        }

        public static string SaveNonImageFile(Stream fileContent, string path)
        {
            try
            {
                string savePath = FileHelpers.NextAvailableFilename(path);
                string folder = Path.GetDirectoryName(savePath);

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Reset stream position nếu có thể
                if (fileContent.CanSeek)
                    fileContent.Seek(0, SeekOrigin.Begin);

                // Lưu file từ stream
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    fileContent.CopyTo(fileStream);
                }

                return savePath;
            }
            catch (Exception ex)
            {
                LogSecurityEvent($"Error saving file: {path}", ex.StackTrace);
                throw;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SweetContext.Current.User == null)
            {
                Response.Redirect("/403", true);
                return;
            }
            Response.ClearHeaders();
            Response.ClearContent();
            Response.Clear();

            loadPath = "";
            queryPath = defaultPath.TrimStart('~');

            string folderLoad = Request["folder"];
            string path = Request["Path"];
            string resolvePath = path;
            
            if (!string.IsNullOrEmpty(folderLoad))
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (folderLoad.Trim('/').ToLower() == path.Trim('/').ToLower())
                        resolvePath = folderLoad;
                    else if (path.TrimStart('/').ToLower().StartsWith(folderLoad.ToLower().TrimStart('/')))
                        resolvePath = path;
                    else
                        resolvePath = folderLoad.TrimEnd('/') + "/" + path.TrimStart('/');
                }
                else
                    resolvePath = folderLoad;
            }

            resolvePath = SecureCorrectPath(resolvePath);
            loadPath = resolvePath;
            if (resolvePath.Contains("..") || resolvePath.Contains("%2e") || resolvePath.Contains("%2f") || resolvePath.Contains("%5c"))
            {
                LogSecurityEvent("Blocked path traversal attempt", resolvePath);
                Response.Write(JsonConvert.SerializeObject(new { Error = "Invalid path.", Code = -1 }));
                return;
            }

            string basePath = HttpContext.Current.Server.MapPath(defaultPath);
            switch (Request["mode"])
            {
                case "getinfo":
                    Response.ContentType = "plain/text";
                    Response.ContentEncoding = Encoding.UTF8;
                    int pos = resolvePath.IndexOf('?');
                    if (pos >= 0)
                        resolvePath = resolvePath.Substring(0, pos);
                    Response.Write(getInfo(resolvePath));
                    break;
                case "getfolder":
                    Response.ContentType = "plain/text";
                    Response.ContentEncoding = Encoding.UTF8;
                    Response.Write(getFolderInfo(resolvePath));
                    break;
                case "rename":
                    Response.ContentType = "plain/text";
                    Response.ContentEncoding = Encoding.UTF8;
                    Response.Write(Rename(Request["old"], Request["new"]));
                    break;
                case "delete":
                    Response.ContentType = "plain/text";
                    Response.ContentEncoding = Encoding.UTF8;
                    Response.Write(Delete(resolvePath));
                    break;
                case "addfolder":
                    Response.ContentType = "plain/text";
                    Response.ContentEncoding = Encoding.UTF8;
                    Response.Write(AddFolder(resolvePath, Request["name"]));
                    break;
                case "download":
                    FileInfo fi = new FileInfo(resolvePath);
                    if (fi.Exists == false)
                    {
                        Response.Write(JsonConvert.SerializeObject(new
                        {
                            Error = "The file '" + resolvePath + "' does not exists.",
                            Code = -1
                        }));
                    }
                    else
                    {
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlPathEncode(fi.Name));
                        Response.AddHeader("Content-Length", fi.Length.ToString());
                        Response.ContentType = "application/octet-stream";
                        Response.TransmitFile(fi.FullName);
                    }
                    break;
                case "add":
                    SaveFile();
                    break;
                case "replace":
                    Response.Write(JsonConvert.SerializeObject(new
                    {
                        Error = "Access denied",
                        Code = -1
                    }));
                    return;
                    Response.ContentType = "application/json";
                    Response.ContentEncoding = Encoding.UTF8;

                    System.Web.HttpPostedFile fileReplace = Request.Files[0];
                    if (fileReplace == null || !FileHelpers.IsFileAllowed(fileReplace.InputStream, fileReplace.FileName, fileReplace.ContentType))
                    {
                        Response.Write(JsonConvert.SerializeObject(new
                        {
                            Error = "Invalid path.",
                            Code = -1
                        }));
                        return;
                    }
                    string newFilePath = Request["newfilepath"];

                    string resolvePath2 = SecureCorrectPath(newFilePath);

                    if (File.Exists(resolvePath2))
                    {
                        try
                        {
                            File.Delete(resolvePath2);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    fileReplace.SaveAs(resolvePath2);

                    Response.Write(JsonConvert.SerializeObject(new
                    {
                        Error = "No error",
                        Code = 0
                    }));

                    break;
                case "summarize":
                    Response.Write(JsonConvert.SerializeObject(GetStorageStatic(resolvePath)));
                    break;
                case "move":
                    Response.Write(JsonConvert.SerializeObject(new
                    {
                        Error = "Access denied",
                        Code = -1
                    }));
                    return;
                    string oldPath = Request["old"];
                    string newPath = Request["new"];

                    string resolveOldPath = SecureCorrectPath(oldPath.TrimEnd('/'));
                    string resolveNewPath = SecureCorrectPath(newPath.TrimEnd('/').TrimStart('~').TrimStart('.').TrimStart('/'));
                    FileAttributes attr = File.GetAttributes(resolveOldPath);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        if (Directory.Exists(resolveNewPath))
                        {
                            try
                            {
                                resolveNewPath = Path.Combine(resolveNewPath, Path.GetFileName(resolveOldPath));

                                Directory.Move(resolveOldPath, resolveNewPath);
                                Response.Write(JsonConvert.SerializeObject(new Dictionary<string, object>()
                                    {
                                        { "New Name", Path.GetFileName(resolveNewPath) },
                                        { "New Path", "/"+ MakeRelativePath(resolveNewPath, true).TrimStart('/') },
                                        { "Code" , 0 }
                                    }));
                            }
                            catch (Exception ex)
                            {
                                Response.Write(JsonConvert.SerializeObject(new
                                {
                                    Error = "Cannot move this folder to " + resolveNewPath,
                                    Code = -1
                                }));
                            }
                        }
                        else
                        {
                            Response.Write(JsonConvert.SerializeObject(new
                            {
                                Error = "Folder '" + resolveNewPath + "' does not exists.",
                                Code = -1
                            }));
                        }
                    }
                    else
                    {
                        if (File.Exists(resolveOldPath))
                        {
                            if (Directory.Exists(resolveNewPath))
                            {
                                try
                                {
                                    resolveNewPath = Path.Combine(resolveNewPath, Path.GetFileName(resolveOldPath));
                                    File.Move(resolveOldPath, resolveNewPath);

                                    string foldertrim = "/" + queryPath.Trim('/');
                                    string temp = "/" + MakeRelativePath(resolveNewPath, false).TrimStart('/');
                                    if (foldertrim.Length > 1)
                                        temp = temp.Remove(0, foldertrim.Length);

                                    Response.Write(JsonConvert.SerializeObject(new Dictionary<string, object>()
                                    {
                                        { "New Name", Path.GetFileName(resolveNewPath) },
                                        { "New Path", temp },
                                        { "Code" , 0 }
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    Response.Write(JsonConvert.SerializeObject(new
                                    {
                                        Error = "Cannot move this file to " + resolveNewPath,
                                        Code = -1
                                    }));
                                }
                            }
                            else
                            {
                                Response.Write(JsonConvert.SerializeObject(new
                                {
                                    Error = "Folder '" + resolveNewPath + "' does not exists.",
                                    Code = -1
                                }));
                            }
                        }
                    }

                    break;
                default:
                    Response.Write(JsonConvert.SerializeObject(new
                    {
                        Error = "Access denied",
                        Code = -1
                    }));
                    return;
                    break;
            }
        }

    }
}