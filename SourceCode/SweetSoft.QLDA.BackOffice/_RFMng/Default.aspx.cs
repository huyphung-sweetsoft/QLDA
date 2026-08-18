using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice._RFMng
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SweetContext.Current.User == null)
            {
                Response.Redirect("/403", true);
                return;
            }

            _RFMng.connectors.filemanager.ResetPath();

            string key = CommonHelpers.QueryString("key");
            string fm = CommonHelpers.QueryString("fm");
            string folderVirtual = ResolveVirtualFolder(key, fm); 
            string physicalFolder = MapSecurePath(folderVirtual); 

            if (string.IsNullOrEmpty(physicalFolder))
            {
                Response.Redirect("/403", true);
                return;
            }

            EnsureDirectoryExists(physicalFolder); 

            ScriptManager.RegisterClientScriptBlock(this, GetType(), "init", $"var forcePath='{folderVirtual.TrimStart('~')}';", true);

            RegisterImageDimensions(folderVirtual); 
        }
        private string ResolveVirtualFolder(string key, string fm)
        {
            // Luôn giới hạn thư mục cha
            string baseFolder = "~/Uploads";

            if (!string.IsNullOrEmpty(fm) && fm == "1")
                return baseFolder;

            string folder = GetFolder(key);

            if (string.IsNullOrWhiteSpace(folder))
                return baseFolder;

            // Ngăn path bắt đầu bằng "/", "~", hoặc ".."
            folder = folder.TrimStart('~', '/');
            folder = folder.Replace("..", string.Empty).Trim();
            folder = folder.Replace("Uploads/", "").Trim();
            if (string.IsNullOrEmpty(folder))
                return baseFolder;

            return $"{baseFolder}/{folder}";
        }
        private string MapSecurePath(string virtualPath)
        {
            string root = HttpContext.Current.Server.MapPath("~/Uploads");
            string fullPath = Path.GetFullPath(HttpContext.Current.Server.MapPath(virtualPath));

            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return null;

            return fullPath;
        }
        private void EnsureDirectoryExists(string fullPath)
        {
            try
            {
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
            }
            catch
            {
                // Ghi log nếu cần
            }
        }
        private void RegisterImageDimensions(string folder)
        {
            var dic = Helpers.GetListImageDimension();
            if (dic == null || dic.Count == 0) return;

            var dicInfo = new Dictionary<string, string>();
            KeyValuePair<string, string>? matched = null;

            foreach (var kv in dic)
            {
                string pathPrefix = string.Empty;
                switch (kv.Key)
                {
                    case CMSImageType.Normal:
                        pathPrefix = "/Uploads/";
                        break;
                    case CMSImageType.SocialNetwork:
                        pathPrefix = "/Uploads/social/";
                        break;
                    default:
                        pathPrefix = string.Empty;
                        break;
                }

                if (!string.IsNullOrEmpty(pathPrefix))
                {
                    if (folder.ToLower().Contains(pathPrefix.ToLower()))
                        matched = new KeyValuePair<string, string>(pathPrefix, kv.Value);

                    dicInfo[pathPrefix] = kv.Value;
                }
            }

            if (matched != null)
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(), "init2",
                    $"var imageDimessionText=[{{'info':'{matched.Value.Value}','p':'{matched.Value.Key}'}}];", true);
            }
            else if (folder.Length < 2 && dicInfo.Count > 0)
            {
                var lstInfo = dicInfo.Select(kvp => new { info = kvp.Value, p = kvp.Key }).ToList();
                ScriptManager.RegisterClientScriptBlock(this, GetType(), "init2",
                    "var imageDimessionText=" + JsonConvert.SerializeObject(lstInfo) + ";", true);
            }
        }
        static string GetFolder(string key)
        {
            try
            {
                if (Guid.TryParse(key, out var id))
                    return id.ToString(); 

                return SecurityUtilities.ProtectUrlParameter(key);
            }
            catch
            {
                return string.Empty;
            }
        }
        private static string ResolveSecureFolder(string key, bool isFileManager)
        {
            if (isFileManager)
                return "/Uploads";

            string folder = GetFolder(key);

            folder = folder?.TrimStart('~', '/').Replace("..", string.Empty).Trim();
            folder = folder.Replace("Uploads/", "").Trim();
            if (string.IsNullOrEmpty(folder))
                return "/Uploads";

            return $"/Uploads/{folder}";
        }


        [WebMethod(EnableSession = true)]
        public static string GetSetting(string key, string fm)
        {
            var isFileManager = fm == "1" || bool.TryParse(fm, out var result) && result;
            var isAdmin = SweetContext.Current.IsAdministrator == true;

            string folder = ResolveSecureFolder(key, isFileManager);

            // Đường dẫn file cấu hình
            string configPath = HttpContext.Current.Server.MapPath("~/_RFMng/scripts/filemanager.config.json");

            JObject config;
            try
            {
                if (!File.Exists(configPath))
                    return JsonConvert.SerializeObject(new { error = "Configuration file not found.", code = -1 });

                config = JObject.Parse(File.ReadAllText(configPath));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = "Invalid config format.", code = -1 });
            }

            // Cấu hình hạn chế upload
            var allowedExtensions = new[] {
        "jpg", "jpeg", "png", "gif", "bmp", "webp", "svg",
        "pdf", "doc", "docx", "xls", "xlsx",
        "mp3", "mp4", "avi"
    };

            config["security"]["uploadRestrictions"] = JToken.FromObject(allowedExtensions);

            // Phân quyền theo chế độ
            if (isFileManager)
            {
                config["upload"]["multiple"] = true;
                config["edit"]["enabled"] = true;
                config["options"]["capabilities"] = JToken.FromObject(new[] { "select", "upload", "folder", "replace", "delete" });
                config["upload"]["numberOfFiles"] = 20;
                config["upload"]["fileSizeLimit"] = 5 * 1024 * 1024; // 5MB
            }
            else
            {
                // Trường hợp quản trị bài viết
                config["upload"]["numberOfFiles"] = 10;
                config["upload"]["fileSizeLimit"] = 5 * 1024 * 1024; // 2MB
                config["upload"]["multiple"] = true;

                // Giới hạn chức năng nếu không phải admin
                if (isAdmin)
                {
                    config["edit"]["enabled"] = true;

                    // Nếu upload video thì tăng giới hạn
                    if (folder.ToLower().Contains("/Uploads/videos"))
                        config["upload"]["fileSizeLimit"] = 10 * 1024 * 1024;
                }
                else
                {
                    config["edit"]["enabled"] = false;
                    config["options"]["capabilities"] = JToken.FromObject(new[] { "select", "upload", "delete" , "folder" });
                    config["options"]["multiple"] = true;
                }

                // Luôn cho phép các chức năng cơ bản
                if (config["options"]["capabilities"] == null)
                    config["options"]["capabilities"] = JToken.FromObject(new[] { "select", "upload", "delete" });
            }

            return config.ToString(Formatting.None);
        }

    }
}