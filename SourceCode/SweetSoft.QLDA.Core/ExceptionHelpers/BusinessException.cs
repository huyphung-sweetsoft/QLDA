using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ExceptionHelpers
{
    public class BusinessException : Exception
    {
        public int StatusCode { get; }
        public string FieldName { get; }
        public string ResourceKey { get; }

        public BusinessException(string resourceKey, string fieldName = null, int statusCode = 0)
            : base(UITextsReader.GetBackEndResourceText(resourceKey))
        {
            ResourceKey = resourceKey;
            FieldName = fieldName;
            StatusCode = statusCode;
        }

        public BusinessException(string resourceKey, string fieldName, int statusCode, Exception innerException)
            : base(UITextsReader.GetBackEndResourceText(resourceKey), innerException)
        {
            ResourceKey = resourceKey;
            FieldName = fieldName;
            StatusCode = statusCode;
        }
    }


    public static class BusinessExceptionHelper
    {
        public static BusinessException CreateAndNotify(string resourceKey, string fieldName = null, int statusCode = 400, Exception inner = null)
        {
            var ex = inner != null
                ? new BusinessException(resourceKey, fieldName, statusCode, inner)
                : new BusinessException(resourceKey, fieldName, statusCode);

            BusinessExceptionNotifierManager.Notify(ex);
            return ex;
        }

        public static void ThrowAndNotify(string resourceKey, string fieldName = null, int statusCode = 400, Exception inner = null)
        {
            throw CreateAndNotify(resourceKey, fieldName, statusCode, inner);
        }
    }

    public static class BusinessValidator
    {
        public static void ThrowIf(bool condition, string resourceKey, string fieldName = null, int statusCode = 400)
        {
            if (condition)
                BusinessExceptionHelper.ThrowAndNotify(resourceKey, fieldName, statusCode);
        }
        public static void ThrowIf(bool condition, string resourceKey, string fieldName, int statusCode, Exception inner = null)
        {
            if (condition)
                BusinessExceptionHelper.ThrowAndNotify(resourceKey, fieldName, statusCode, inner);
        }
        public static void ThrowIfNull(object obj, string resourceKey, string fieldName = null, int statusCode = 400)
        {
            if (obj == null)
                BusinessExceptionHelper.ThrowAndNotify(resourceKey, fieldName, statusCode);
        }
        public static void ThrowIfNullOrEmpty(string value, string resourceKey, string fieldName = null, int statusCode = 400)
        {
            if (string.IsNullOrEmpty(value))
                BusinessExceptionHelper.ThrowAndNotify(resourceKey, fieldName, statusCode);
        }

        public static void ThrowGuid(Guid guid, string resourceKey, string fieldName = null, int statusCode = 400)
        {
            if (guid == null || guid == Guid.Empty)
                BusinessExceptionHelper.ThrowAndNotify(resourceKey, fieldName, statusCode);
        }

        public static void ThrowDateTime(DateTime dt, string resourceKey, string fieldName = null, int statusCode = 400)
        {
            if (dt == null || dt == DateTime.MinValue
                || dt == DateTimeHelper.MinValueSQL)
                BusinessExceptionHelper.ThrowAndNotify(resourceKey, fieldName, statusCode);
        }
        public static void ThrowIf<T>(T value, Func<T, bool> predicate, string resourceKey, string fieldName = null, int statusCode = 400)
        {
            if (predicate(value))
                BusinessExceptionHelper.ThrowAndNotify(resourceKey, fieldName, statusCode);
        }
    }
    public static class ErrorCodes
    {
        public const int BadRequest = 400; // Dữ liệu không hợp lệ
        public const int Unauthorized = 401; // Chưa đăng nhập
        public const int Forbidden = 403; // Không có quyền
        public const int NotFound = 404; // Không tìm thấy
        public const int Conflict = 409; // Dữ liệu xung đột
        public const int InternalError = 500; // Lỗi hệ thống
        public const int NotImplemented = 501; // Chưa hỗ trợ
        public const int ServiceUnavailable = 503; // Hệ thống tạm ngừng
    }
}
