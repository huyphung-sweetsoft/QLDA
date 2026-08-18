using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ExceptionHelpers
{
    public static class FieldNameDisplayMapper
    {
        private static readonly Dictionary<string, string> _fieldLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "Id", "Mã" },
    { "ApplicationId", "Mã Ứng dụng" },
    { "RoleId", "Mã Vai trò" },
    { "RoleName", "Tên Vai trò" },
    { "LoweredRoleName", "Tên Vai trò (viết thường)" },
    { "Description", "Mô tả" },
    { "IsActivated", "Kích hoạt" },
    { "IsDeleted", "Đã xóa" },
    { "CreatedUser", "Người tạo" },
    { "CreatedDate", "Ngày tạo" },
    { "UpdatedUser", "Người cập nhật" },
    { "UpdatedDate", "Ngày cập nhật" },
    { "UserId", "Mã Người dùng" },
    { "IsAnonymous", "Ẩn danh" },
    { "UserName", "Tên Người dùng" },
    { "LoweredUserName", "Tên Người dùng (viết thường)" },
    { "DisplayName", "Tên hiển thị" },
    { "Avatar", "Ảnh đại diện" },
    { "MobileAlias", "Biệt danh di động" },
    { "LastActivityDate", "Ngày hoạt động cuối" },
    { "AuthenticatorKey", "Khóa xác thực" },
    { "ResetPasswordKey", "Khóa reset mật khẩu" },
    { "Position", "Chức vụ" },
    { "Title", "Tiêu đề" },
    { "Note", "Ghi chú" },
    { "Type", "Loại" },
    { "CreatedUser", "Người tạo" },
    { "UpdatedUser", "Người cập nhật" },
    { "DisplayOrder", "Thứ tự hiển thị" },
    { "LinkedId", "Mã liên kết" },
    { "CategoryId", "Mã Danh mục" },
    { "ISO", "Mã ISO" },
    { "NiceName", "Tên đẹp" },
    { "ISO3", "Mã ISO3" },
    { "NumberCode", "Mã số" },
    { "PhoneCode", "Mã điện thoại" },
    { "PersonalId", "CMND/CCCD" },
    { "FullName", "Họ tên" },
    { "Gender", "Giới tính" },
    { "BirthDate", "Ngày sinh" },
    { "Address", "Địa chỉ" },
    { "Phone", "Điện thoại" },
    { "NationalityId", "Quốc tịch" },
    { "Status", "Trạng thái" },
    { "RefId", "Mã tham chiếu" },
    { "RefType", "Loại tham chiếu" },
    { "SenderId", "Người gửi" },
    { "SentDate", "Ngày gửi" },
    { "Sender", "Người gửi (text)" },
    { "Subject", "Chủ đề" },
    { "EmailContent", "Nội dung email" },
    { "From_Email", "Email gửi" },
    { "To_Email", "Email nhận" },
    { "CC_Email", "Email CC" },
    { "BCC_Email", "Email BCC" },
    { "IsSent", "Đã gửi" },
    { "IsRead", "Đã đọc" },
    { "ReadDate", "Ngày đọc" },
    { "NumberOfSent", "Số lần gửi" },
    { "ErrorMessage", "Thông báo lỗi" },
    { "Body", "Nội dung" },
    { "CCEmail", "CC Email" },
    { "BCCEmail", "BCC Email" },
    { "TemplateKey", "Mã mẫu email" },
    { "EmailType", "Loại email" },
    { "Code", "Mã" },
    { "Name", "Tên" },
    { "SupplierId", "Mã nhà cung cấp" },
    { "SettingName", "Tên cấu hình" },
    { "SettingValue", "Giá trị cấu hình" },
    { "FileUrl", "Đường dẫn file" },
    { "FileType", "Loại file" },
    { "Ext", "Phần mở rộng" },
};

        public static string GetFieldLabel(string fieldName)
        {
            try
            {
                if (string.IsNullOrEmpty(fieldName)) return string.Empty;
                return _fieldLabels.TryGetValue(fieldName, out var label) ? label : fieldName;
            }
            catch
            {
                return fieldName;
            }
        }
    }
}
