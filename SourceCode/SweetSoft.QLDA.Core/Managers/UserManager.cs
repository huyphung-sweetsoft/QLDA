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
    //Khai báo enum dùng cho CreateOrUpdate
    public enum AccountAction
    {
        None,
        CreateGhost,//Tạo nhân sự chay (account ma)
        CreateReal,//Tạo nhân sự có quyền đăng nhập
        UpdateGhost,//Chỉnh sửa nhân sự chay (thông tin cá nhân nhân sự)
        UpgradeToReal,//Nâng cấp nhân sự chay lên có quyền đăng nhập
        UpdateReal//Chỉnh sửa nhân sự có quyền đăng nhập (thông tin cá nhân + thông tin tài khoản)
    }
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
        //Sửa toàn diện CreateOrUpdate
        public AspnetUser CreateOrUpdate(AspnetUser dto)
        {
            // TRẠM 1: TIỀN KIỂM TRA CHUNG (GLOBAL VALIDATION)
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.DisplayName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.DisplayName));
            BusinessValidator.ThrowIfNullOrEmpty(dto.Email, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.Email));

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            bool isInsert = (dto.UserId == Guid.Empty);
            //string unencryptedPassword = "";
            AspnetUser resultUser = null;
            //Phân tích ý đồ và gán account action
            AccountAction currentAction = AccountAction.None;
            AspnetUser existingUser = null;
            if (isInsert)
            {
                //Nếu là tạo mới, dựa vào việc DTO UserName có hay ko để quyết định enum
                currentAction = string.IsNullOrEmpty(dto.UserName) ? AccountAction.CreateGhost : AccountAction.CreateReal;
            }
            else
            {
                //Nếu là update, phải lấy DB lên trước để so sánh
                existingUser = _repository.GetById(dto.UserId);
                BusinessValidator.ThrowIfNull(existingUser, BackEndResourceKeys.NOT_FOUND, nameof(dto.UserId), ErrorCodes.NotFound);
                if (existingUser.LaNhanVien && !dto.LaNhanVien)
                {
                    dto.LaNhanVien = true; // Ép ngược lại thành Nhân viên
                    dto.IdCCCD = existingUser.IdCCCD;
                    dto.IdPhongBan = existingUser.IdPhongBan;
                    dto.IdChucDanh = existingUser.IdChucDanh;
                    dto.NgaySinh = existingUser.NgaySinh;
                    dto.GioiTinh = existingUser.GioiTinh;
                    dto.DiaChi = existingUser.DiaChi;
                    dto.NgayGiaNhap = existingUser.NgayGiaNhap;
                }
                bool oldIsGhost = IsGhostAccount(existingUser.UserName);
                bool newIsGhost = string.IsNullOrEmpty(dto.UserName);//Lúc này giao diện ko có dto.Username -> nghĩa là vẫn muốn giữ tài khoản ma
                if (oldIsGhost && newIsGhost)
                {
                    currentAction = AccountAction.UpdateGhost;
                }
                else if (oldIsGhost && !newIsGhost)//nhân viên hiện tại có tài khoản ma + ô dto.username có data -> muốn up lên tài khoản real cho nhân viên này
                {
                    currentAction = AccountAction.UpgradeToReal;
                }
                else if (!oldIsGhost && !newIsGhost)//Nhân viên hiện tại là riu hết 
                {
                    currentAction = AccountAction.UpdateReal;
                }
                else
                {
                    throw new InvalidOperationException("Bảo mật: Không được phép hạ cấp tài khoản thật (Real) thành tài khoản ma (Ghost)");
                }
            }
            if (!dto.LaNhanVien && (currentAction == AccountAction.CreateGhost || currentAction == AccountAction.UpdateGhost))
            {
                throw new InvalidOperationException("Bảo mật: Tài khoản hệ thống (System User) không được phép tồn tại ở trạng thái Tài khoản ma (Ghost). Bắt buộc phải có Tên đăng nhập.");
            }
            // TRẠM 2: VALIDATION ĐỘC QUYỀN VÀ NGHIỆP VỤ BẮT BUỘC
            // 1. Kiểm tra Email duy nhất toàn hệ thống (Áp dụng cho mọi luồng)
            BusinessValidator.ThrowIf(_repository.IsEmailExist(dto.UserId, dto.Email),
                BackEndResourceKeys.EMAIL_ALREADY_EXISTS, nameof(dto.Email), ErrorCodes.Conflict);

            // 2. Kiểm tra CCCD duy nhất & Dọn rác
            if (dto.LaNhanVien)
            {
                BusinessValidator.ThrowIfNullOrEmpty(dto.IdCCCD, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.IdCCCD));
                BusinessValidator.ThrowIf(_repository.IsCCCDExist(dto.UserId, dto.IdCCCD),
                    BackEndResourceKeys.CCCD_ALREADY_EXISTS, nameof(dto.IdCCCD), ErrorCodes.Conflict);
            }
            else
            {
                // Tài khoản hệ thống thì dọn rác các thuộc tính của nhân viên
                dto.IdPhongBan = null; dto.IdChucDanh = null; dto.IdCCCD = null;
                dto.NgaySinh = null; dto.NgayGiaNhap = null; dto.GioiTinh = null; dto.DiaChi = null;
            }

            // 3. Validation Vành đai thép: Chỉ dành cho các luồng TÀI KHOẢN THẬT (Real Account)
            if (currentAction == AccountAction.CreateReal ||
                currentAction == AccountAction.UpgradeToReal ||
                currentAction == AccountAction.UpdateReal)
            {
                // a. Username không được bỏ trống
                BusinessValidator.ThrowIfNullOrEmpty(dto.UserName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.UserName));

                // b. CẤM giả mạo Ghost: Admin tự gõ Username không được bắt đầu bằng "EMP_"
                if (dto.UserName.StartsWith("EMP_", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Tên đăng nhập không hợp lệ! Không được phép sử dụng tiền tố bảo lưu 'EMP_'.");
                }

                // c. Kiểm tra Username duy nhất toàn hệ thống
                BusinessValidator.ThrowIf(_repository.IsUserNameExist(dto.UserId, dto.UserName),
                    BackEndResourceKeys.USERNAME_ALREADY_EXISTS, nameof(dto.UserName), ErrorCodes.Conflict);

                // d. BẮT BUỘC có Role: Có tài khoản login là phải có quyền
                if (dto.RoleId == Guid.Empty)
                {
                    throw new Exception("Bảo mật: Tài khoản được cấp quyền truy cập hệ thống bắt buộc phải gán vào một Nhóm quyền (Role).");
                }
            }
            // TRẠM 3: Phần chính xử lý
            // Khởi tạo các biến Tracking cho Email (nằm ngoài TransactionScope để đảm bảo an toàn luồng)
            bool sendMailRequired = false;
            string finalUserNameToSend = string.Empty;
            string finalPasswordToSend = string.Empty;

            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
            {
                if (isInsert)
                {
                    // ---------------------------------------------------------
                    // LUỒNG INSERT (THÊM MỚI NHÂN SỰ)
                    // ---------------------------------------------------------
                    Guid generatedUserId = Guid.NewGuid();

                    if (currentAction == AccountAction.CreateGhost)
                    {
                        // A. TẠO TÀI KHOẢN MA
                        dto.UserName = "EMP_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                        string trashPassword = SecurityUtilities.CreateAlphaNumericString(16);

                        MembershipUser membershipUser = Membership.CreateUser(dto.UserName, trashPassword, dto.Email);
                        membershipUser.IsApproved = false; // Ép cứng: Cấm đăng nhập
                        Membership.UpdateUser(membershipUser);

                        generatedUserId = (Guid)membershipUser.ProviderUserKey;
                    }
                    else if (currentAction == AccountAction.CreateReal)
                    {
                        // B. TẠO TÀI KHOẢN THẬT
                        finalPasswordToSend = SecurityUtilities.CreateAlphaNumericString(8);
                        finalUserNameToSend = dto.UserName;

                        MembershipUser membershipUser = Membership.CreateUser(dto.UserName, finalPasswordToSend, dto.Email);
                        membershipUser.IsApproved = dto.IsActivated; // Mở/Khóa theo tình trạng công tác
                        Membership.UpdateUser(membershipUser);

                        generatedUserId = (Guid)membershipUser.ProviderUserKey;
                        sendMailRequired = true;
                    }

                    // Ghi dữ liệu vào bảng aspnet_Users
                    // BẢN FIX: Lấy bản ghi mà Membership API vừa tự động tạo ra trong Database
                    AspnetUser newUser = _repository.GetById(generatedUserId);
                    if (newUser != null)
                    {
                        // Cập nhật thêm các trường thông tin Nhân sự/Hệ thống vào bản ghi đã có
                        newUser.UserName = dto.UserName;
                        newUser.Email = dto.Email;
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
                        newUser.IsActivated = dto.IsActivated; // Tình trạng HR
                        newUser.RoleId = currentAction == AccountAction.CreateReal ? dto.RoleId : Guid.Empty;
                        newUser.NgayTao = DateTime.Now;
                        newUser.NguoiTao = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

                        // Dùng UPDATE thay vì INSERT để tránh lỗi trùng khóa chính (Primary Key)
                        resultUser = _repository.Update(newUser);
                    }
                    else
                    {
                        throw new Exception("Lỗi hệ thống: Không tìm thấy tài khoản vừa được khởi tạo bởi Membership API.");
                    }

                    // Gán Role nếu là Tài khoản thật
                    if (currentAction == AccountAction.CreateReal && dto.RoleId != Guid.Empty)
                    {
                        AspnetRole role = RoleManager.Instance.GetRoleById(dto.RoleId);
                        if (role != null) Roles.AddUserToRole(newUser.UserName, role.LoweredRoleName);
                    }

                    resultUser = newUser;
                }
                else
                {
                    // ---------------------------------------------------------
                    // LUỒNG UPDATE (CẬP NHẬT/NÂNG CẤP NHÂN SỰ)
                    // ---------------------------------------------------------

                    if (currentAction == AccountAction.UpdateGhost)
                    {
                        // C. SỬA THÔNG TIN NHÂN SỰ CHAY (VẪN LÀ GHOST)
                        MembershipUser membershipUser = Membership.GetUser(existingUser.UserName);
                        if (membershipUser != null)
                        {
                            membershipUser.Email = dto.Email;
                            membershipUser.IsApproved = false; // Tiếp tục ép cứng
                            Membership.UpdateUser(membershipUser);
                        }
                    }
                    else if (currentAction == AccountAction.UpgradeToReal)
                    {
                        // D. NÂNG CẤP TỪ GHOST LÊN REAL (TỬ HUYỆT DIRECT SQL)
                        finalUserNameToSend = dto.UserName;
                        finalPasswordToSend = SecurityUtilities.CreateAlphaNumericString(8);

                        // D.1. Lách luật Membership API: Dùng SubSonic đổi trực tiếp Username trong DB
                        new Update(AspnetUser.Schema)
                            .Set(AspnetUser.Columns.UserName).EqualTo(dto.UserName)
                            .Set("LoweredUserName").EqualTo(dto.UserName.ToLower())
                            .Where(AspnetUser.Columns.UserId).IsEqualTo(existingUser.UserId)
                            .Execute();

                        // Đồng bộ thực thể C# hiện tại với DB vừa Update
                        existingUser.UserName = dto.UserName;

                        // D.2. Update Membership (Lúc này API đã nhận diện được tên mới)
                        MembershipUser membershipUser = Membership.GetUser(dto.UserName);
                        if (membershipUser != null)
                        {
                            membershipUser.Email = dto.Email;
                            membershipUser.IsApproved = dto.IsActivated; // Mở khóa
                            Membership.UpdateUser(membershipUser);
                            // Reset password ma cũ và chèn password 8 ký tự mới sinh
                            string oldTrashPass = membershipUser.ResetPassword();
                            membershipUser.ChangePassword(oldTrashPass, finalPasswordToSend);
                        }
                        if (dto.RoleId != Guid.Empty)
                        {
                            AspnetRole role = RoleManager.Instance.GetRoleById(dto.RoleId);
                            if (role != null) Roles.AddUserToRole(dto.UserName, role.LoweredRoleName);
                        }
                        sendMailRequired = true;
                    }
                    else if (currentAction == AccountAction.UpdateReal)
                    {
                        // E. CẬP NHẬT TÀI KHOẢN THẬT
                        MembershipUser membershipUser = Membership.GetUser(existingUser.UserName);
                        if (membershipUser != null)
                        {
                            // Giải cứu tài khoản nếu bị khóa tự động (IsLockedOut)
                            if (dto.IsActivated && membershipUser.IsLockedOut) membershipUser.UnlockUser();

                            membershipUser.Email = dto.Email;
                            membershipUser.IsApproved = dto.IsActivated;

                            // Nếu Admin nhập pass thủ công
                            if (!string.IsNullOrEmpty(dto.Password))
                            {
                                string oldPass = membershipUser.ResetPassword();
                                membershipUser.ChangePassword(oldPass, dto.Password);

                                finalUserNameToSend = existingUser.UserName;
                                finalPasswordToSend = dto.Password;
                                sendMailRequired = true;
                            }
                            Membership.UpdateUser(membershipUser);
                        }

                        // Đồng bộ Role (Thu hồi Role cũ, cấp Role mới)
                        if (dto.RoleId != existingUser.RoleId)
                        {
                            RoleManager.Instance.RemoveAllRoleOfUser(existingUser.UserId);
                            AspnetRole role = RoleManager.Instance.GetRoleById(dto.RoleId);
                            if (role != null) Roles.AddUserToRole(existingUser.UserName, role.LoweredRoleName);
                        }
                    }

                    // D.3 & E.3: Cập nhật thông tin thực thể aspnet_Users
                    existingUser.DisplayName = dto.DisplayName;
                    existingUser.Email = dto.Email;
                    existingUser.MobileAlias = dto.MobileAlias;
                    existingUser.Avatar = dto.Avatar ?? string.Empty;
                    existingUser.IsActivated = dto.IsActivated;
                    existingUser.LaNhanVien = dto.LaNhanVien;

                    // Xử lý RoleId trong bảng phụ
                    existingUser.RoleId = (currentAction == AccountAction.UpgradeToReal || currentAction == AccountAction.UpdateReal)
                                          ? dto.RoleId : Guid.Empty;

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
                scope.Complete();
            }
                // TRẠM 4: Gửi Email bất đồng bộ 
                if (sendMailRequired && resultUser != null && !string.IsNullOrEmpty(finalUserNameToSend))
                {
                    // Lấy thông tin đích danh từ resultUser để đảm bảo data khớp 100% với DB
                    Guid userIdToSend = resultUser.UserId;
                    string emailToSend = resultUser.Email;
                    string displayNameToSend = resultUser.DisplayName;

                    // Lấy Username và Password đã được tracking ở Trạm 3
                    string userNameToSend = finalUserNameToSend;
                    string passwordToSend = finalPasswordToSend;

                    // Đẩy vào Task.Run để chạy ngầm, không làm treo giao diện (UI) của Admin lúc bấm Lưu
                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(1500);
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

                            // CHỐT 2: Dùng chung template "TemplateAccountInformation" cho cả tạo mới và đổi pass
                            await emailManager.SendEmailWithTemplateAsync(
                                refId: userIdToSend,
                                refType: EmailType.Notification,
                                customerId: userIdToSend,
                                toEmail: emailToSend,
                                templateKey: "TemplateAccountInformation",
                                formatType: EmailFormatTypes.Admin,
                                placeholdersBody: placeholdersBody,
                                attachments: null,
                                useBackgroundThread: false
                            );
                        }
                        catch (Exception ex)
                        {
                            // Lỗi mail không làm chết dữ liệu. Chỉ ghi log để IT check lại SMTP.
                            SysLogger.LogError(ex, $"Lỗi gửi email cấp/đổi tài khoản cho hệ thống. UserName: {userNameToSend}");
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
        public bool IsGhostAccount(string userName)
        {
            return string.IsNullOrEmpty(userName) || userName.StartsWith("EMP_", StringComparison.OrdinalIgnoreCase);
        }

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