# Hướng Dẫn Thiết Lập & Cấu Hình Đăng Nhập (SweetSoft QLDA BackOffice)

Tài liệu này hướng dẫn chi tiết cách thiết lập môi trường, cấu hình `Web.config`, cơ sở dữ liệu và xử lý các vấn đề liên quan đến chức năng đăng nhập/xác thực cho hệ thống **SweetSoft QLDA BackOffice**.

---

## 1. Yêu cầu môi trường (Prerequisites)

- **Hệ điều hành**: Windows 10/11 hoặc Windows Server.
- **Web Server**: Internet Information Services (IIS) 8.0 trở lên.
- **Framework**: .NET Framework 4.8.
- **Cơ sở dữ liệu**: Microsoft SQL Server 2012 trở lên (SQL Server Express hoặc Standard).
- **Công cụ phát triển / Build**: Visual Studio 2022 hoặc MSBuild 17+.

---

## 2. Cấu hình Domain & File Hosts

Hệ thống sử dụng cookie xác thực được gắn theo `domain`. Do đó, cần cấu hình tên miền cục bộ (hoặc tên miền máy chủ):

1. Mở Notepad / Text Editor bằng quyền **Administrator**.
2. Mở file: `C:\Windows\System32\drivers\etc\hosts`
3. Thêm dòng sau vào cuối file:
   ```text
   127.0.0.1    qlda.local
   ```
4. Lưu file lại.

> **Lưu ý**: Nếu bạn triển khai trên tên miền khác (ví dụ `adm.domain.com`), hãy đảm bảo tên miền đó trỏ về đúng IP máy chủ và cập nhật thông số `domain` trong `Web.config`.

---

## 3. Cấu hình IIS (Internet Information Services)

### 3.1. Tạo Application Pool
- **Tên AppPool**: `SweetSoft.QLDA.Pool` (hoặc tên tùy chọn).
- **.NET CLR Version**: `.NET CLR Version v4.0.30319`
- **Managed Pipeline Mode**: `Integrated`
- **Advanced Settings**:
  - `Enable 32-Bit Applications`: `True` (nếu hệ thống sử dụng các DLL 32-bit như SQLite/SubSonic native).
  - `Identity`: `ApplicationPoolIdentity` (hoặc tài khoản có quyền truy cập SQL Server và thư mục mã nguồn).

### 3.2. Tạo Website trên IIS
- **Site Name**: `SweetSoft.QLDA`
- **Application Pool**: Chọn AppPool vừa tạo ở bước trên.
- **Physical Path**: Trỏ tới thư mục chứa project BackOffice:
  ```text
  d:\Working\HocViec\Source-QLDA\SourceCode\SweetSoft.QLDA.BackOffice
  ```
- **Binding**:
  - Type: `http` (hoặc `https` nếu có chứng chỉ SSL).
  - IP Address: `All Unassigned`
  - Port: `80` (hoặc `443` cho https).
  - Host name: `qlda.local`

### 3.3. Phân quyền thư mục (Folder Permissions)
Cấp quyền **Read & Write (Modify)** cho tài khoản `IIS_IUSRS` và `IUSR` tại thư mục mã nguồn, đặc biệt là thư mục:
- `SweetSoft.QLDA.BackOffice\Uploads` (lưu file đính kèm, avatar, log).

---

## 4. Cấu hình Cơ sở dữ liệu (Database Setup)

Trong file `SweetSoft.QLDA.BackOffice\Web.config`, kiểm tra và cập nhật chuỗi kết nối:

```xml
<connectionStrings>
  <remove name="SweetSoft.QLDA.BackOffice" />
  <add name="SweetSoft.QLDA.BackOffice" 
       connectionString="Data Source=SWEET-QUOCHUY\SQLEXPRESS01;Initial Catalog=SweetSoft_QLDA;Persist Security Info=True;User ID=sa;Password=your_password" />
</connectionStrings>
```

### Các bảng dữ liệu phục vụ đăng nhập:
Hệ thống sử dụng cơ chế **ASP.NET SQL Membership Provider**:
- `aspnet_Applications`: Lưu thông tin ứng dụng (`SweetSoft.QLDA.BackOffice`).
- `aspnet_Users`: Thông tin người dùng (UserId, UserName, DisplayName, AuthenticatorKey,...).
- `aspnet_Membership`: Thông tin mật khẩu, mã hóa, trạng thái kích hoạt (`IsApproved`, `IsLockedOut`).
- `aspnet_Roles` & `aspnet_UsersInRoles`: Phân quyền nhóm người dùng.
- `TblAuditLog_{year}`: Lưu nhật ký đăng nhập/đăng xuất theo từng năm.

---

## 5. Cấu hình Xác thực trong Web.config

Mở file `SweetSoft.QLDA.BackOffice\Web.config` và cấu hình các mục quan trọng sau:

### 5.1. Cấu hình `appSettings`
```xml
<appSettings>
  <!-- Bật cơ chế Task-friendly SynchronizationContext cho ASP.NET 4.8 -->
  <add key="aspnet:UseTaskFriendlySynchronizationContext" value="true" />
  
  <!-- Khóa Cookie và Session hệ thống -->
  <add key="CookieKeyPanel" value="_QLDA_PANEL_DEV_" />
  <add key="CookieKeyClient" value="_QLDA_CLIENT_DEV_" />
  <add key="SessionAppContext" value="_QLDA_SESSION_APP_CONTEXT_PANEL_DEV_" />
  
  <!-- Tùy chọn mật khẩu mặc định khi reset -->
  <add key="IsUsedDefaultPassword" value="true" />
  <add key="DefaultPassword" value="123456" />
</appSettings>
```

### 5.2. Cấu hình `system.web`
```xml
<system.web>
  <!-- Compilation và HttpRuntime chuẩn .NET 4.8 -->
  <compilation debug="true" targetFramework="4.8" />
  <httpRuntime targetFramework="4.8" requestValidationMode="2.0" maxRequestLength="1048576" executionTimeout="3600" enableVersionHeader="false" />
  
  <globalization culture="vi-VN" uiCulture="vi-VN" />
  
  <!-- Cấu hình Cookie: nếu chạy HTTP, hãy đặt requireSSL="false". Nếu chạy HTTPS, đặt requireSSL="true" -->
  <httpCookies httpOnlyCookies="true" requireSSL="false" />
  
  <!-- Cấu hình Forms Authentication -->
  <authentication mode="Forms">
    <forms loginUrl="~/Login" 
           defaultUrl="~/" 
           name=".ASPXFORMSAUTH" 
           timeout="10080" 
           enableCrossAppRedirects="true" 
           ticketCompatibilityMode="Framework40" 
           domain="qlda.local" 
           protection="Encryption" 
           requireSSL="false" />
  </authentication>
  
  <!-- Phân quyền mặc định: từ chối người dùng ẩn danh -->
  <authorization>
    <deny users="?" />
  </authorization>
  
  <!-- MachineKey cố định để giải mã Auth Cookie và ViewState -->
  <machineKey validationKey="41EDCA53EE6C85DEAF35CDC5F12FD323A296030A4175EB4E946DFEEE0C0CC007BA29A8A59B48281A939475EB450406EC805510745ED487A83104582F52998933" 
              decryptionKey="063534281FFA966C07907DE19CB177308F8BCF35567598EE9B26D85A20D46E29" 
              validation="SHA1" 
              decryption="AES" />
</system.web>
```

> **Lưu ý về SSL & Domain**:
> - Nếu bạn chạy ở giao thức `http://qlda.local`, bắt buộc đặt `requireSSL="false"` ở cả thẻ `<httpCookies>` và `<forms>`. Nếu để `requireSSL="true"`, trình duyệt sẽ không lưu auth cookie khi truy cập qua HTTP.
> - Thuộc tính `domain="qlda.local"` trong `<forms>` phải khớp với domain bạn truy cập trên thanh địa chỉ trình duyệt. Nếu truy cập bằng `localhost` hoặc `127.0.0.1`, cookie sẽ không hợp lệ.

---

## 6. Quy trình & Các bước Đăng nhập

1. **Mã bảo vệ (Captcha)**: Hệ thống kiểm tra mã Captcha hợp lệ trước khi xác thực tài khoản.
2. **Xác thực thông tin**:
   - Kiểm tra `Membership.ValidateUser(username, password)`.
   - Kiểm tra trạng thái tài khoản (`IsApproved == true`, `IsLockedOut == false`, `IsDeleted == false`, `IsActivated == true`).
   - Kiểm tra quyền truy cập hệ thống: Tài khoản phải thuộc nhóm quản trị (`IsAdministrator`) hoặc đã được gán quyền (`RoleManager.IsAssignPermission`).
3. **Xác thực 2 yếu tố (2FA - Google Authenticator)**:
   - Nếu tài khoản có `AuthenticatorKey`, hệ thống sẽ yêu cầu nhập mã OTP 6 chữ số.
4. **Tạo Auth Ticket & Ghi nhận phiên đăng nhập**:
   - Tạo Forms Authentication Cookie.
   - Ghi nhật ký đăng nhập vào `TblAuditLog_{year}` thông qua tác vụ ngầm an toàn.
   - Chuyển hướng người dùng vào `/Home`.

---

## 7. Xử lý sự cố thường gặp (Troubleshooting)

### 1. Đăng nhập thành công nhưng bị văng lại trang `/Login`
- **Nguyên nhân 1**: Thuộc tính `domain` trong `<forms domain="qlda.local" ... />` không trùng với domain trên trình duyệt (ví dụ truy cập `http://localhost/Login` thay vì `http://qlda.local/Login`).
- **Nguyên nhân 2**: Cấu hình `requireSSL="true"` nhưng truy cập qua `http://` (không có HTTPS). Khắc phục: Đổi thành `requireSSL="false"` trong `Web.config`.

### 2. Thông báo: "Tài khoản chưa được phân quyền trên hệ thống"
- **Nguyên nhân**: Tài khoản đã tạo trong bảng `aspnet_Users` nhưng chưa được gán Role trong `aspnet_UsersInRoles` hoặc `aspnet_AssignRoles`, và không phải là Super Admin.
- **Khắc phục**: Gán Role cho tài khoản trong trang quản trị hoặc kiểm tra bảng `aspnet_UsersInRoles`.

### 3. Thông báo: "Tài khoản đã bị khóa"
- **Nguyên nhân**: Tài khoản nhập sai mật khẩu quá số lần quy định (`maxInvalidPasswordAttempts="9"`), hoặc cột `IsApproved = 0`, hoặc `IsActivated = 0`.
- **Khắc phục**: Chạy lệnh unlock trong SQL hoặc cập nhật `IsApproved = 1`, `IsLockedOut = 0` trong `aspnet_Membership`.

### 4. Lỗi Visual Studio Just-In-Time Debugger (w3wp.exe crash)
- **Nguyên nhân**: Thiếu `<httpRuntime targetFramework="4.8" />` hoặc `aspnet:UseTaskFriendlySynchronizationContext` dẫn đến lỗi `LegacyAspNetSynchronizationContext` khi ghi log bất đồng bộ.
- **Khắc phục**: Đảm bảo đã cập nhật đầy đủ các cấu hình trong mục **5.1** và **5.2**.
