using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

namespace SweetSoft.QLDA.BackOffice.fFilesBox
{
    public class SecureFileUploadHandler : IHttpHandler, IRequiresSessionState
    {
        #region Configuration
        private readonly FileUploadConfig _config = new FileUploadConfig();

        public class FileUploadConfig
        {
            public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
            public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".doc", ".docx", ".xlsx", ".xls", ".pdf", ".svg", ".mp4", ".mp3", ".avi", ".webp" };
            public string[] AllowedMimeTypes { get; set; } = {
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
        };
            public string UploadBasePath { get; set; } = "/Uploads/";
            public bool RequireAuthentication { get; set; } = true;
            public bool ScanForMalware { get; set; } = true;
            public int MaxFilenameLength { get; set; } = 255;
        }
        #endregion

        #region Models
        public class UploadRequest
        {
            public Guid RefId { get; set; }
            public string RefType { get; set; }
            public string FileTitle { get; set; }
            public int Order { get; set; }
            public HttpPostedFile File { get; set; }
        }

        public class UploadResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public Guid? FileId { get; set; }
            public string ErrorCode { get; set; }
            public string DebugMsg { get; set; }
        }
        #endregion

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                var result = ProcessUploadRequest(context);
                context.Response.Write(SerializeResult(result));
            }
            catch (Exception ex)
            {
                LogError(ex);
                var errorResult = new UploadResult
                {
                    Success = false,
                    Message = "Lỗi máy chủ nội bộ",
                    ErrorCode = "INTERNAL_ERROR",
                    DebugMsg = ex.Message
                };
                context.Response.Write(SerializeResult(errorResult));
                context.Response.StatusCode = 500;
            }
        }

        private UploadResult ProcessUploadRequest(HttpContext context)
        {
            // 1. Validate authentication
            var authResult = ValidateAuthentication();
            if (!authResult.Success) return authResult;

            // 2. Parse and validate request
            var parseResult = ParseRequest(context);
            if (!parseResult.Success) return parseResult.Result;

            var request = parseResult.Request;

            // 3. Validate file
            var fileValidationResult = ValidateFile(request.File);
            if (!fileValidationResult.Success) return fileValidationResult;

            // 4. Validate business rules
            var businessValidationResult = ValidateBusinessRules(request);
            if (!businessValidationResult.Success) return businessValidationResult;

            // 5. Process upload
            return ProcessFileUpload(request);
        }

        #region Authentication
        private UploadResult ValidateAuthentication()
        {
            if (!_config.RequireAuthentication)
                return new UploadResult { Success = true };

            var currentUser = SweetContext.Current.User;
            if (currentUser == null)
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "Yêu cầu xác thực",
                    ErrorCode = "AUTH_REQUIRED"
                };
            }

            return new UploadResult { Success = true };
        }
        #endregion

        #region Request Parsing
        private (bool Success, UploadRequest Request, UploadResult Result) ParseRequest(HttpContext context)
        {
            try
            {
                var request = new UploadRequest();

                // Parse RefId
                string strRefId = SanitizeInput(context.Request.QueryString["RefId"]);
                if (!Guid.TryParse(strRefId, out Guid refId))
                {
                    return (false, null, new UploadResult
                    {
                        Success = false,
                        Message = "Định dạng RefId không hợp lệ",
                        ErrorCode = "INVALID_REFID"
                    });
                }
                request.RefId = refId;

                // Parse RefType với validation
                request.RefType = SanitizeRefType(context.Request.QueryString["RefType"]);
                if (string.IsNullOrEmpty(request.RefType))
                {
                    return (false, null, new UploadResult
                    {
                        Success = false,
                        Message = "RefType là bắt buộc",
                        ErrorCode = "REFTYPE_REQUIRED"
                    });
                }

                // Parse Order
                string strOrder = SanitizeInput(context.Request.QueryString["Order"]);
                if (!int.TryParse(strOrder, out int order))
                    order = 0;
                request.Order = order;

                // Parse FileTitle
                request.FileTitle = SanitizeInput(context.Request.QueryString["FileTitle"]);
                if (string.IsNullOrEmpty(request.FileTitle))
                    request.FileTitle = "Untitled";

                // Get file
                if (context.Request.Files.Count == 0)
                {
                    return (false, null, new UploadResult
                    {
                        Success = false,
                        Message = "Không có tập tin nào được tải lên",
                        ErrorCode = "NO_FILE"
                    });
                }

                request.File = context.Request.Files[0];

                return (true, request, null);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return (false, null, new UploadResult
                {
                    Success = false,
                    Message = "Định dạng yêu cầu không hợp lệ",
                    ErrorCode = "INVALID_REQUEST",
                    DebugMsg = ex.Message
                });
            }
        }
        #endregion

        #region File Validation
        private UploadResult ValidateFile(HttpPostedFile file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "Tệp tin trống hoặc không được cung cấp",
                    ErrorCode = "EMPTY_FILE"
                };
            }

            // Validate file size
            if (file.ContentLength > _config.MaxFileSize)
            {
                return new UploadResult
                {
                    Success = false,
                    Message = $"Kích thước tập tin vượt quá giới hạn {_config.MaxFileSize / (1024 * 1024)}MB",
                    ErrorCode = "FILE_TOO_LARGE"
                };
            }

            // Validate filename length
            if (file.FileName.Length > _config.MaxFilenameLength)
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "Tên tệp quá dài",
                    ErrorCode = "FILENAME_TOO_LONG"
                };
            }

            // Validate file extension
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_config.AllowedExtensions.Contains(extension))
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "Loại tệp không được phép",
                    ErrorCode = "INVALID_FILE_TYPE"
                };
            }

            // Validate MIME type
            if (!_config.AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "Loại MIME không hợp lệ",
                    ErrorCode = "INVALID_MIME_TYPE"
                };
            }

            // Validate file signature (magic bytes)
            //var signatureValidation = ValidateFileSignature(file);
            //if (!signatureValidation.Success) return signatureValidation;

            // Scan for malware (if enabled)
            if (_config.ScanForMalware)
            {
                var malwareResult = ScanForMalware(file);
                if (!malwareResult.Success) return malwareResult;
            }

            return new UploadResult { Success = true };
        }

        private UploadResult ValidateFileSignature(HttpPostedFile file)
        {
            try
            {
                file.InputStream.Position = 0;
                byte[] buffer = new byte[16]; // Tăng lên 16 bytes để đọc đủ signature dài
                int bytesRead = file.InputStream.Read(buffer, 0, 16);
                file.InputStream.Position = 0;

                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Kiểm tra magic bytes cho các định dạng phổ biến
                var signatures = GetFileSignatures();
                if (signatures.ContainsKey(extension))
                {
                    var validSignatures = signatures[extension];
                    bool isValid = false;

                    foreach (var signature in validSignatures)
                    {
                        if (bytesRead >= signature.Length)
                        {
                            bool matches = true;
                            for (int i = 0; i < signature.Length; i++)
                            {
                                if (buffer[i] != signature[i])
                                {
                                    matches = false;
                                    break;
                                }
                            }
                            if (matches)
                            {
                                isValid = true;
                                break;
                            }
                        }
                    }

                    // Kiểm tra đặc biệt cho AVI (cần kiểm tra thêm bytes 8-11 cho "AVI ")
                    if (!isValid && extension == ".avi" && bytesRead >= 12)
                    {
                        if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                            buffer[8] == 0x41 && buffer[9] == 0x56 && buffer[10] == 0x49 && buffer[11] == 0x20)
                        {
                            isValid = true;
                        }
                    }

                    // Kiểm tra đặc biệt cho DOCX/XLSX (ZIP-based, cần kiểm tra thêm content)
                    if (!isValid && (extension == ".docx" || extension == ".xlsx"))
                    {
                        isValid = ValidateOfficeDocument(file, extension);
                    }

                    if (!isValid)
                    {
                        return new UploadResult
                        {
                            Success = false,
                            Message = "Chữ ký tệp không khớp với phần mở rộng",
                            ErrorCode = "INVALID_FILE_SIGNATURE"
                        };
                    }
                }

                return new UploadResult { Success = true };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new UploadResult
                {
                    Success = false,
                    Message = "Xác thực chữ ký tệp không thành công",
                    ErrorCode = "SIGNATURE_VALIDATION_ERROR"
                };
            }
        }

        private bool ValidateOfficeDocument(HttpPostedFile file, string extension)
        {
            try
            {
                // Đọc thêm bytes để kiểm tra ZIP structure và Office-specific content
                file.InputStream.Position = 0;
                byte[] buffer = new byte[100];
                int bytesRead = file.InputStream.Read(buffer, 0, 100);
                file.InputStream.Position = 0;

                // Kiểm tra ZIP signature
                bool isZip = (buffer[0] == 0x50 && buffer[1] == 0x4B) &&
                            (buffer[2] == 0x03 || buffer[2] == 0x05 || buffer[2] == 0x07) &&
                            (buffer[3] == 0x04 || buffer[3] == 0x06 || buffer[3] == 0x08);

                if (!isZip) return false;

                // Có thể thêm kiểm tra sâu hơn bằng cách đọc ZIP entries
                // để tìm các file đặc trưng như word/document.xml, xl/workbook.xml
                // Tuy nhiên điều này phức tạp và có thể làm chậm quá trình upload

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Business Rules Validation
        private UploadResult ValidateBusinessRules(UploadRequest request)
        {
            // Validate RefType against whitelist
            if (!Helpers.IsValidEnumValue<FileUploadTypes>(request.RefType))
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "RefType không hợp lệ",
                    ErrorCode = "INVALID_REFTYPE"
                };
            }

            // Check user permissions for RefType
            var currentUser = SweetContext.Current.User;
            if (!HasPermissionForRefType(currentUser, request.RefType, request.RefId))
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "Quyền không đủ",
                    ErrorCode = "ACCESS_DENIED"
                };
            }

            return new UploadResult { Success = true };
        }
        #endregion

        #region File Processing
        private UploadResult ProcessFileUpload(UploadRequest request)
        {
            try
            {
                // Create upload directory
                CreateUploadDirectory(request.RefType);

                // Generate secure filename
                string secureFileName = GenerateSecureFileName(request.File.FileName);
                string filePath = CreateSecureFilePath(request.RefType, secureFileName);

                // Save file
                string fullPath = HttpContext.Current.Server.MapPath(filePath);
                request.File.SaveAs(fullPath);

                // Create database record
                var fileUpload = CreateFileRecord(request, filePath);
                var savedFile = UploadManager.Instance.Create(fileUpload);

                return new UploadResult
                {
                    Success = true,
                    FileId = savedFile.Id,
                    Message = "Tệp đã được tải lên thành công"
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new UploadResult
                {
                    Success = false,
                    Message = "Tải lên không thành công",
                    ErrorCode = "UPLOAD_FAILED",
                    DebugMsg = ex.Message
                };
            }
        }
        #endregion

        #region Helper Methods
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Remove potentially dangerous characters
            return Regex.Replace(input.Trim(), @"[<>""'%;()&+]", "");
        }

        private string SanitizeRefType(string refType)
        {
            if (string.IsNullOrEmpty(refType)) return string.Empty;

            // Only allow alphanumeric and underscore
            return Regex.Replace(refType.Trim(), @"[^a-zA-Z0-9_]", "");
        }

        private string GenerateSecureFileName(string originalFileName)
        {
            if (string.IsNullOrEmpty(originalFileName))
                return UUIDv7.NewGuid().ToString();

            string nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            string extension = Path.GetExtension(originalFileName);

            // Sanitize filename
            nameWithoutExt = Regex.Replace(nameWithoutExt, @"[^a-zA-Z0-9_\-]", "_");

            // Add timestamp and random component for uniqueness
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string randomSuffix = GenerateRandomString(6);

            return $"{nameWithoutExt}_{timestamp}_{randomSuffix}{extension}";
        }

        private string CreateSecureFilePath(string refType, string fileName)
        {
            // Create subdirectories by date for better organization
            string dateFolder = DateTime.Now.ToString("yyyy/MM");
            string relativePath = $"{_config.UploadBasePath}{refType}/{dateFolder}/{fileName}";

            return relativePath;
        }

        private void CreateUploadDirectory(string refType)
        {
            string dateFolder = DateTime.Now.ToString("yyyy/MM");
            string subPath = $"{_config.UploadBasePath}{refType}/{dateFolder}/";
            string fullPath = HttpContext.Current.Server.MapPath(subPath);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
        }

        private TblUploadFile CreateFileRecord(UploadRequest request, string filePath)
        {
            var currentUser = SweetContext.Current.User;

            return new TblUploadFile
            {
                CreatedDate = DateTime.UtcNow,
                OwnerId = currentUser?.UserId ?? Guid.Empty,
                IsDeleted = false,
                Name = request.FileTitle,
                FileUrl = filePath,
                FileType = FileTypes.Internal,
                Ext = Path.GetExtension(request.File.FileName).ToLowerInvariant() ?? string.Empty,
                RefId = request.RefId,
                RefType = request.RefType,
                DisplayOrder = request.Order,
                FileSize = request.File.ContentLength,
                MimeType = request.File.ContentType,
                OriginalFileName = request.File.FileName,
                IsHost = true,
                IsSecretary = true,
                IsParticipant = true,
            };
        }

        private Dictionary<string, byte[][]> GetFileSignatures()
        {
            return new Dictionary<string, byte[][]>
        {
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }
        };
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
            }
        }

        private bool HasPermissionForRefType(AspnetUser user, string refType, Guid refId)
        {
            // Implement your permission logic here
            // This is a placeholder - implement based on your business rules
            return user != null;
        }

        private UploadResult ScanForMalware(HttpPostedFile file)
        {
            // Placeholder for malware scanning
            // Implement integration with antivirus API or service
            // For now, just return success
            return new UploadResult { Success = true };
        }

        private string SerializeResult(UploadResult result)
        {
            // Simple JSON serialization - consider using Newtonsoft.Json for production
            if (result.Success)
            {
                return $"{{\"success\": true, \"fileId\": \"{result.FileId}\", \"message\": \"{EscapeJsonString(result.Message)}\"}}";
            }
            else
            {
                return $"{{\"success\": false, \"message\": \"{EscapeJsonString(result.Message)}\", \"errorCode\": \"{EscapeJsonString(result.ErrorCode)}\", \"debug\": \"{EscapeJsonString(result.DebugMsg)}\"}}";
            }
        }

        private string EscapeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private void LogError(Exception ex)
        {
            // Implement your logging mechanism
            //System.Diagnostics.Debug.WriteLine($"Upload Error: {ex}");
        }
        #endregion
    }

}