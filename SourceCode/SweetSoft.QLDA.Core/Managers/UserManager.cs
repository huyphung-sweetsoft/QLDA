using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Security;

namespace SweetSoft.QLDA.Core.Managers
{
    public class UserManager : BaseManager
    {
        private static readonly Lazy<UserManager> _instance = new Lazy<UserManager>(() => new UserManager());
        public static UserManager Instance => _instance.Value;

        private readonly UserRepository _repository;
        private readonly AuditManager _auditManager;

        public UserManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new UserRepository(_auditManager);
        }

        #region Search Paging

        public DataTable SearchUsers(string searchTerm, Guid roleId, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, roleId, orderBy, pageNumber, pageSize, out totalRecord);
        }

        public DataTable SearchUsers(string searchTerm, Dictionary<string, object> searchParameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, searchParameters, orderBy, pageNumber, pageSize, out totalRecord);
        }

        public DataTable SearchUsers(Dictionary<string, object> searchParameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchParameters, orderBy, pageNumber, pageSize, out totalRecord);
        }

        #endregion

        #region Core CRUD (Create / Update / Delete)

        public AspnetUser CreateOrUpdate(AspnetUser dto)
        {
            // =========================================================================
            // TRẠM 1: TIỀN KIỂM TRA CHUNG (GLOBAL VALIDATION)
            // =========================================================================
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.UserName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.UserName));
            BusinessValidator.ThrowIfNullOrEmpty(dto.DisplayName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.DisplayName));
            BusinessValidator.ThrowIfNullOrEmpty(dto.Email, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.Email));

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            bool isInsert = (dto.UserId == Guid.Empty);
            string unencryptedPassword = "";
            AspnetUser resultUser = null;

            // =========================================================================
            // TRẠM 2: PHÂN LUỒNG DỮ LIỆU BẰNG CỜ LaNhanVien
            // =========================================================================
            if (dto.LaNhanVien)
            {
                // Nếu là Nhân viên: Ép buộc phải có CCCD
                BusinessValidator.ThrowIfNullOrEmpty(dto.IdCCCD, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.IdCCCD));
            }
            else
            {
                // Nếu là Tài khoản hệ thống: Dọn rác các thuộc tính đặc thù của nhân viên
                dto.IdPhongBan = null;
                dto.IdChucDanh = null;
                dto.IdCCCD = null;
                dto.NgaySinh = null;
                dto.NgayGiaNhap = null;
                dto.GioiTinh = null;
                dto.DiaChi = null;
            }

            // =========================================================================
            // TRẠM 3: MỞ GIAO DỊCH DATABASE (TRANSACTION SCOPE)
            // =========================================================================
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
            {
                if (isInsert)
                {
                    // =====================================================================
                    // TRẠM 4: XỬ LÝ THÊM MỚI (INSERT)
                    // =====================================================================
                    // 4.1. Kiểm tra trùng lặp
                    BusinessValidator.ThrowIf(_repository.IsUserNameExist(Guid.Empty, dto.UserName),
                        BackEndResourceKeys.USERNAME_ALREADY_EXISTS, nameof(dto.UserName), ErrorCodes.Conflict);

                    BusinessValidator.ThrowIf(_repository.IsEmailExist(Guid.Empty, dto.Email),
                        BackEndResourceKeys.EMAIL_ALREADY_EXISTS, nameof(dto.Email), ErrorCodes.Conflict);

                    if (dto.LaNhanVien)
                    {
                        BusinessValidator.ThrowIf(_repository.IsCCCDExist(Guid.Empty, dto.IdCCCD),
                            BackEndResourceKeys.CCCD_ALREADY_EXISTS, nameof(dto.IdCCCD), ErrorCodes.Conflict);
                    }

                    // 4.2. Sinh mật khẩu ngẫu nhiên
                    unencryptedPassword = SecurityUtilities.CreateAlphaNumericString(8);

                    try
                    {
                        // 4.3. Đăng ký tài khoản vào hệ thống ASP.NET Membership
                        MembershipUser membershipUser = Membership.CreateUser(dto.UserName, unencryptedPassword, dto.Email);
                        membershipUser.IsApproved = dto.IsActivated;
                        Membership.UpdateUser(membershipUser);

                        Guid newUserId = (Guid)membershipUser.ProviderUserKey;

                        // 4.4. Cập nhật các trường mở rộng vào bảng aspnet_Users
                        AspnetUser newUser = _repository.GetById(newUserId);
                        BusinessValidator.ThrowIfNull(newUser, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

                        newUser.DisplayName = dto.DisplayName;
                        newUser.MobileAlias = dto.MobileAlias;
                        newUser.Avatar = dto.Avatar ?? string.Empty;
                        newUser.LaNhanVien = dto.LaNhanVien;
                        newUser.IdPhongBan = dto.IdPhongBan;
                        newUser.IdChucDanh = dto.IdChucDanh;
                        newUser.NgaySinh = dto.NgaySinh;
                        newUser.GioiTinh = dto.GioiTinh;
                        newUser.IdCCCD = dto.IdCCCD;
                        newUser.DiaChi = dto.DiaChi;
                        newUser.NgayGiaNhap = dto.NgayGiaNhap;
                        newUser.IsDeleted = false;
                        newUser.IsActivated = dto.IsActivated;
                        newUser.NgayTao = DateTime.Now;
                        newUser.NguoiTao = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

                        _repository.Update(newUser);

                        // 4.5. Gán Quyền (Role) nếu có chọn
                        if (dto.RoleId != Guid.Empty)
                        {
                            AspnetRole role = RoleManager.Instance.GetRoleById(dto.RoleId);
                            if (role != null)
                            {
                                Roles.AddUserToRole(newUser.UserName, role.LoweredRoleName);
                            }
                        }

                        resultUser = newUser;
                    }
                    catch (MembershipCreateUserException ex)
                    {
                        throw new Exception("Lỗi khởi tạo tài khoản hệ thống: " + ex.StatusCode.ToString());
                    }
                }
                else
                {
                    // =====================================================================
                    // TRẠM 5: XỬ LÝ CẬP NHẬT (UPDATE)
                    // =====================================================================
                    AspnetUser existingUser = _repository.GetById(dto.UserId);
                    BusinessValidator.ThrowIfNull(existingUser, BackEndResourceKeys.NOT_FOUND, nameof(dto.UserId), ErrorCodes.NotFound);

                    // 5.1. Kiểm tra trùng lặp (trừ chính mình ra)
                    BusinessValidator.ThrowIf(_repository.IsUserNameExist(existingUser.UserId, dto.UserName),
                        BackEndResourceKeys.USERNAME_ALREADY_EXISTS, nameof(dto.UserName), ErrorCodes.Conflict);

                    BusinessValidator.ThrowIf(_repository.IsEmailExist(existingUser.UserId, dto.Email),
                        BackEndResourceKeys.EMAIL_ALREADY_EXISTS, nameof(dto.Email), ErrorCodes.Conflict);

                    if (dto.LaNhanVien && !string.IsNullOrEmpty(dto.IdCCCD))
                    {
                        BusinessValidator.ThrowIf(_repository.IsCCCDExist(existingUser.UserId, dto.IdCCCD),
                            BackEndResourceKeys.CCCD_ALREADY_EXISTS, nameof(dto.IdCCCD), ErrorCodes.Conflict);
                    }

                    // 5.2. Đồng bộ sang Membership (Email, Mật khẩu, Mở khóa)
                    MembershipUser membershipUser = Membership.GetUser(existingUser.UserName);
                    if (membershipUser != null)
                    {
                        if (existingUser.IsActivated && membershipUser.IsLockedOut)
                            membershipUser.UnlockUser();

                        if (membershipUser.Email != dto.Email)
                            membershipUser.Email = dto.Email;

                        membershipUser.IsApproved = dto.IsActivated;

                        // Đổi mật khẩu nếu Admin có truyền mật khẩu mới
                        if (!string.IsNullOrEmpty(dto.Password))
                        {
                            string oldPass = membershipUser.ResetPassword();
                            BusinessValidator.ThrowIf(!membershipUser.ChangePassword(oldPass, dto.Password),
                                BackEndResourceKeys.UNABLE_TO_UPDATE_PASSWORD_FOR_ACCOUNT, nameof(dto.Password));
                        }

                        Membership.UpdateUser(membershipUser);
                    }

                    // 5.3. Cập nhật Role
                    if (dto.RoleId != existingUser.RoleId)
                    {
                        AspnetRole role = RoleManager.Instance.GetRoleById(dto.RoleId);
                        if (role != null)
                        {
                            RoleManager.Instance.RemoveAllRoleOfUser(existingUser.UserId);
                            Roles.AddUserToRole(existingUser.UserName, role.LoweredRoleName);
                        }
                    }

                    // 5.4. Cập nhật thông tin thực thể
                    existingUser.DisplayName = dto.DisplayName;
                    existingUser.MobileAlias = dto.MobileAlias;
                    existingUser.Avatar = dto.Avatar ?? string.Empty;
                    existingUser.IsActivated = dto.IsActivated;
                    existingUser.LaNhanVien = dto.LaNhanVien;
                    existingUser.IdPhongBan = dto.IdPhongBan;
                    existingUser.IdChucDanh = dto.IdChucDanh;
                    existingUser.NgaySinh = dto.NgaySinh;
                    existingUser.GioiTinh = dto.GioiTinh;
                    existingUser.IdCCCD = dto.IdCCCD;
                    existingUser.DiaChi = dto.DiaChi;
                    existingUser.NgayGiaNhap = dto.NgayGiaNhap;
                    existingUser.NgayCapNhat = DateTime.Now;
                    existingUser.NguoiCapNhat = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

                    resultUser = _repository.Update(existingUser);
                }

                // =========================================================================
                // TRẠM 6: CHỐT GIAO DỊCH DATABASE (COMMIT)
                // =========================================================================
                scope.Complete();
            }

            // =========================================================================
            // TRẠM 7: GỬI EMAIL BẤT ĐỒNG BỘ (POST-PROCESSING)
            // =========================================================================
            if (isInsert && resultUser != null)
            {
                Guid userIdToSend = resultUser.UserId;
                string emailToSend = dto.Email;
                string displayNameToSend = dto.DisplayName;
                string userNameToSend = dto.UserName;
                string passwordToSend = unencryptedPassword;

                Task.Run(async () =>
                {
                    try
                    {
                        var emailManager = new EmailManager(null);
                        var placeholdersBody = new Dictionary<string, string>
                        {
                            { "[[COMPANY_NAME]]", "SweetSoft" },
                            { "[[FULL_NAME]]", displayNameToSend },
                            { "[[USER_NAME]]", userNameToSend },
                            { "[[PASSWORD]]", passwordToSend },
                            { "[[EMAIL]]", emailToSend },
                            { "[[SUPPORT_EMAIL]]", "hotro@sweetsoft.vn" },
                            { "[[LOGIN_URL]]", "http://qlda.local/Login" }
                        };

                        await emailManager.SendEmailWithTemplateAsync(
                            refId: userIdToSend,
                            refType: EmailType.Notification,
                            customerId: userIdToSend,
                            toEmail: emailToSend,
                            templateKey: "TemplateAccountInformation",
                            formatType: EmailFormatTypes.Admin,
                            placeholdersBody: placeholdersBody,
                            attachments: null,
                            useBackgroundThread: true
                        );
                    }
                    catch (Exception ex)
                    {
                        SysLogger.LogError(ex, "Lỗi gửi email cấp tài khoản tự động");
                    }
                });
            }

            return resultUser;
        }

        public bool Delete(AspnetUser item)
        {
            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.INVALID_DATA);
            return _repository.Delete(item);
        }

        #endregion

        #region Single Fetchers & Checks

        public AspnetUser GetUserById(Guid id)
        {
            return _repository.GetById(id);
        }

        public AspnetUser GetUserByUserName(string userName)
        {
            return _repository.GetByUserName(userName);
        }

        public AspnetUser GetUserByEmail(string email)
        {
            return _repository.GetByEmail(email);
        }

        public string GetDisplayNameByUserName(string username)
        {
            return _repository.GetDisplayNameByUserName(username);
        }

        public bool ValidateUser(string userName, string password)
        {
            return _repository.ValidateUser(userName, password);
        }

        public bool IsEmailExist(Guid id, string email)
        {
            return _repository.IsEmailExist(id, email);
        }

        public bool IsUserNameExist(Guid id, string userName)
        {
            return _repository.IsUserNameExist(id, userName);
        }

        public bool IsCCCDExist(Guid id, string maCCCD)
        {
            return _repository.IsCCCDExist(id, maCCCD);
        }

        public AspnetUser GetByCCCD(string maCCCD)
        {
            return _repository.GetByCCCD(maCCCD);
        }

        public DataTable GetUserForDetail(Guid userId)
        {
            return _repository.GetNhanVienForDetail(userId);
        }

        #endregion

        #region List Fetchers

        public List<AspnetUser> GetAllAspnetUsers()
        {
            return _repository.GetAllAspnetUsers();
        }

        public List<AspnetUser> GetAllActiveNhanVien()
        {
            return _repository.GetAllNhanVienActive();
        }

        public List<AspnetUser> GetByPhongBan(Guid idPhongBan)
        {
            return _repository.GetByPhongBan(idPhongBan);
        }

        public List<AspnetUser> GetByChucDanh(Guid idChucDanh)
        {
            return _repository.GetByChucDanh(idChucDanh);
        }

        #endregion

        #region Helpers & Autocomplete

        public AutocompleteObj AllUserAutocomplete(string keyword, int maxResult, string lang)
        {
            int total;
            DataTable dt = SearchUsers(keyword, Guid.Empty, "DisplayName ASC", 1, maxResult, out total);

            List<AutocompleteItem> listAutocompleteItem = new List<AutocompleteItem>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                total = dt.Rows.Count;
                foreach (DataRow row in dt.Rows)
                {
                    AutocompleteItem autocompleteItem = new AutocompleteItem();
                    string title = row["UserName"].ToString();
                    if (string.IsNullOrEmpty(title))
                        title = row["Email"].ToString();

                    autocompleteItem.Label = string.Format("<span class=\"tag activated\">{0}</span>" +
                        "<span class=\"sub-info\">" +
                        "<i>Full Name: {1}</i>" +
                        "</span>"
                            , title
                            , row["DisplayName"]);

                    autocompleteItem.Value = title;
                    autocompleteItem.Data = row["UserId"].ToString();
                    autocompleteItem.OtherData = JsonConvert.SerializeObject(new
                    {
                        DisplayName = row["DisplayName"].ToString(),
                        Email = row["Email"].ToString(),
                    });
                    listAutocompleteItem.Add(autocompleteItem);
                }
            }

            AutocompleteObj autocompleteObj = new AutocompleteObj();
            autocompleteObj.Total = total;
            autocompleteObj.ListAutocompleteItem = listAutocompleteItem;
            return autocompleteObj;
        }

        public bool IsAdministrator(Guid id)
        {
            try
            {
                string value = AppCache.Get(string.Format("IS_ADMINISTRATION_{0}", SweetContext.Current.UserId)) as string;
                if (string.IsNullOrEmpty(value))
                {
                    AspnetUser user = _repository.GetById(id);
                    bool isAdmin = user != null && user.UserName.ToLower().Equals("administrator");
                    AppCache.Insert(string.Format("IS_ADMINISTRATION_{0}", SweetContext.Current.UserId), isAdmin);
                    return isAdmin;
                }
                return bool.Parse(value);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}