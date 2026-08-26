using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Security;
using System.Transactions;
namespace SweetSoft.QLDA.Core.Managers
{
    public class NhanVienManager : BaseManager
    {
        //Đây là mẫu thiết kế Singleton, thay vì khởi tạo đối tượng new NhanVienManager ngay từ khi mở web thì Lazy sẽ trì hoãn nó, chỉ khi nào 
        //có đoạn code gọi NhanVienManager.Instance thì mới bắt đầu sinh cái đối tượng kia
        //Giải quyết lun cái An toàn luồng, nghĩa là giả sử 10 thk cùng gửi request 1 lúc, thì chỉ có 1 thằng được tạo ra, 9 thằng còn lại sẽ chờ tới khi thằng này xong
        //
        private static readonly Lazy<NhanVienManager> _instance = new Lazy<NhanVienManager>(() => new NhanVienManager());
        public static NhanVienManager Instance = _instance.Value;
        private readonly NhanVienRepository _nhanVienRepository;
        private readonly AuditManager _auditManager;
        private readonly UserRepository _userRepository;
        //Contructor tiếp nhận context, khởi tạo Audit rồi truyền xuống các repo phụ thuộc
        public NhanVienManager(IAppContext applicationContext =  null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());//GetClientInfo từ BaseManager để thu thập siêu dữ liệu của phiên làm việc hiện tại,
            //gồm: 4 metadata, qua bên BaseManager xem
            _nhanVienRepository = new NhanVienRepository (_auditManager);//truyền audit vô để nó kích hoạt Auto Audit Logging
            _userRepository = new UserRepository (_auditManager);
        }   
        //3 thằng search
        public DataTable SearchNhanVien(string searchTerm, Guid userId, Guid maPhongBan, Guid maChucDanh, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _nhanVienRepository.SearchPaging(searchTerm,userId,maPhongBan,maChucDanh,orderBy, pageNumber, pageSize, out totalRecord);
        }
        public DataTable SearchNhanVien(string searchTerm, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _nhanVienRepository.SearchPaging(searchTerm,parameters,orderBy,pageNumber,pageSize,out totalRecord);
        }
        public DataTable SearchNhanVien(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _nhanVienRepository.SearchPaging(parameters,orderBy,pageNumber,pageSize,out totalRecord);
        }
        //đám truy vân dữ liệu
        public TblNhanVien GetNhanVienById(Guid id)
        {
            return _nhanVienRepository.GetById(id);
        }
        public TblNhanVien GetNhanVienByUserId(Guid userId)
        {
            return _nhanVienRepository.GetByIdUser(userId);
        }
        public List<TblNhanVien> GetAllActiveNhanVien()
        {
            return _nhanVienRepository.GetAllActive();
        }
        public List<TblNhanVien> GetNhanVienByPhongBan(Guid idPhongBan)
        {
            return _nhanVienRepository.GetByPhongBan(idPhongBan);
        }
        public List<TblNhanVien> GetNhanVienByChucDanh(Guid idChucDanh)
        {
            return _nhanVienRepository.GetByChucDanh(idChucDanh);
        }
        //nhóm Crud + blabla
        //public TblNhanVien CreateOrUpdate(TblNhanVien dto)
        //{
        //    //BussinessValidator là lớp tiện ích tĩnh, áp dụng nguyên lý thất bại sớm: hễ gặp dữ liệu sai là nó ngắt luồng liền và quăng lỗi nghiệp vụ chuẩn hóa ra
        //    //nói chung để check lỗi data?
        //    BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);//nếu dữ liệu bị null, gọi invalid data
        //    BusinessValidator.ThrowIfNullOrEmpty(dto.TenNhanVien, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenNhanVien));//Kiểm tra chuỗi bị null or khoảng trắng
        //    BusinessValidator.ThrowIfNullOrEmpty(dto.DiaChi, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.DiaChi));//nói chung bảng có ô input text nào muốn check thì thêm dô
        //    //if 1: Kiểm tra xem, với nhân viên mới này, chua có id nhân viên, cccd có trùng với ô nào trong data ko, nếu có là lỗi
        //    if (dto.IdNhanVien == Guid.Empty)
        //    {
        //        BusinessValidator.ThrowIf(_nhanVienRepository.GetByCCCD(dto.IdCCCD) != null,
        //            BackEndResourceKeys.CCCD_ALREADY_EXISTS, nameof(dto.IdCCCD), ErrorCodes.Conflict);
        //    }
        //    TblNhanVien nhanVien;
        //    // Sửa lỗi gõ phím "! -" thành "!="
        //    if (dto.IdNhanVien != Guid.Empty)
        //    {
        //        // 1. Cập nhật (Update)
        //        nhanVien = _nhanVienRepository.GetById(dto.IdNhanVien);
        //        BusinessValidator.ThrowIfNull(nhanVien, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdNhanVien), ErrorCodes.NotFound);

        //        // Kiểm tra xem CCCD sửa lại có bị trùng với nhân viên KHÁC không
        //        BusinessValidator.ThrowIf(_nhanVienRepository.IsCCCDExist(nhanVien.IdNhanVien, dto.IdCCCD),
        //            BackEndResourceKeys.CCCD_ALREADY_EXISTS, nameof(dto.IdCCCD), ErrorCodes.Conflict);

        //        // Tạm thời bỏ qua xử lý User theo yêu cầu, chỉ copy các thuộc tính kinh doanh từ dto sang bản ghi gốc
        //        ObjectHelper.CopyBusinessProperties(dto, nhanVien,
        //            x => x.IdNhanVien,
        //            x => x.DaXoa,
        //            x => x.NgayTao,
        //            x => x.NguoiTao,
        //            x => x.IsNew); // Không copy đè những cột hệ thống này

        //        // Gọi hàm Update có lưu vết (Audit) của Repository
        //        nhanVien = _nhanVienRepository.Update(nhanVien);
        //        BusinessValidator.ThrowIfNull(nhanVien, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

        //        return nhanVien;
        //    }
        //    else
        //    {
        //        // 2. Thêm mới (Insert)
        //        nhanVien = dto.Clone() as TblNhanVien;
        //        BusinessValidator.ThrowIfNull(nhanVien, BackEndResourceKeys.INVALID_DATA);

        //        nhanVien.IdNhanVien = Guid.NewGuid();
        //        nhanVien.DaXoa = false;
        //        nhanVien.IsNew = true; // Bắt buộc gán true để báo SubSonic thực hiện câu lệnh Insert

        //        // Mở comment bên dưới nếu DB có yêu cầu lưu thời gian/người tạo
        //        nhanVien.NgayTao = DateTime.Now;
        //        nhanVien.NguoiTao = SweetContext.Current.UserName;

        //        // Tạm thời chưa sinh tài khoản tự động, lưu trực tiếp dữ liệu nhân viên
        //        nhanVien.Save();

        //        return nhanVien;
        //    }
        //}
        public TblNhanVien CreateOrUpdate(TblNhanVien dto)
        {
            // 1. Validate dữ liệu đầu vào chung
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenNhanVien, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenNhanVien));
            BusinessValidator.ThrowIfNullOrEmpty(dto.Email, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, "Email");

            // Lấy ID người dùng đang thao tác (Admin)
            Guid currentUserId = SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.UserId;

            // MỞ GIAO DỊCH: Đảm bảo thao tác đa bảng phải thành công toàn bộ, nếu lỗi 1 chỗ sẽ Rollback toàn bộ
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
            {
                if (dto.IdNhanVien == Guid.Empty)
                {
                    // ==========================================
                    // NHÁNH 1: XỬ LÝ THÊM MỚI (INSERT)
                    // ==========================================
                    BusinessValidator.ThrowIf(_nhanVienRepository.GetByCCCD(dto.IdCCCD) != null,
                        BackEndResourceKeys.CCCD_ALREADY_EXISTS, nameof(dto.IdCCCD), ErrorCodes.Conflict);

                    string generatedUserName = GenerateUsername(dto.TenNhanVien, dto.NgaySinh);
                    int suffix = 1;
                    string finalUserName = generatedUserName;
                    while (_userRepository.GetByUserName(finalUserName) != null)
                    {
                        finalUserName = generatedUserName + suffix.ToString();
                        suffix++;
                    }

                    // 1. ĐẶT CỨNG MẬT KHẨU MẶC ĐỊNH (Vì Form Nhân viên không còn ô nhập mật khẩu)
                    string password = "abc@123";

                    try
                    {
                        MembershipUser membershipUser = Membership.CreateUser(finalUserName, password, dto.Email);

                        // 2. MẶC ĐỊNH KÍCH HOẠT TÀI KHOẢN KHI TẠO MỚI
                        membershipUser.IsApproved = true;
                        Membership.UpdateUser(membershipUser);

                        Guid newUserId = (Guid)membershipUser.ProviderUserKey;

                        AspnetUser aspnetUser = _userRepository.GetById(newUserId);
                        if (aspnetUser != null)
                        {
                            aspnetUser.MobileAlias = dto.PhoneNumber;
                            aspnetUser.DisplayName = dto.TenNhanVien;

                            // ĐỒNG BỘ DỮ LIỆU
                            aspnetUser.IsActivated = true; // Mặc định kích hoạt
                            aspnetUser.IsDeleted = false; // Mới tạo thì chưa xóa
                            aspnetUser.Avatar = dto.AnhDaiDien;
                            _userRepository.Update(aspnetUser);
                        }

                        // Gán các trường liên kết và Audit Trail cho Insert
                        dto.IdNhanVien = Guid.NewGuid();
                        dto.UserId = newUserId;
                        dto.DaXoa = false;
                        dto.NgayTao = DateTime.Now;
                        dto.NguoiTao = currentUserId.ToString();
                        dto.IsNew = true;

                        dto = _nhanVienRepository.Insert(dto);

                        // Chốt giao dịch thành công
                        scope.Complete();
                        return dto;
                    }
                    catch (MembershipCreateUserException ex)
                    {
                        throw new Exception("Lỗi tạo tài khoản hệ thống (Có thể Email đã tồn tại): " + ex.StatusCode.ToString());
                    }
                }
                else
                {
                    // ==========================================
                    // NHÁNH 2: XỬ LÝ CẬP NHẬT (UPDATE)
                    // ==========================================
                    TblNhanVien nhanVienOld = _nhanVienRepository.GetById(dto.IdNhanVien);
                    BusinessValidator.ThrowIfNull(nhanVienOld, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdNhanVien), ErrorCodes.NotFound);

                    BusinessValidator.ThrowIf(_nhanVienRepository.IsCCCDExist(nhanVienOld.IdNhanVien, dto.IdCCCD),
                        BackEndResourceKeys.CCCD_ALREADY_EXISTS, nameof(dto.IdCCCD), ErrorCodes.Conflict);

                    dto.NgayCapNhat = DateTime.Now;
                    dto.NguoiCapNhat = currentUserId.ToString();

                    ObjectHelper.CopyBusinessProperties(dto, nhanVienOld,
                        x => x.IdNhanVien, x => x.UserId, x => x.DaXoa, x => x.NgayTao, x => x.NguoiTao, x => x.IsNew);
                    nhanVienOld.NgaySinh = dto.NgaySinh;
                    nhanVienOld.NgayGiaNhap = dto.NgayGiaNhap;
                    nhanVienOld = _nhanVienRepository.Update(nhanVienOld);

                    // ĐỒNG BỘ 3 TRƯỜNG: SĐT, Tên, Email (Không đụng đến Mật khẩu và Trạng thái)
                    if (nhanVienOld.UserId.HasValue && nhanVienOld.UserId.Value != Guid.Empty)
                    {
                        AspnetUser aspnetUser = _userRepository.GetById(nhanVienOld.UserId.Value);
                        if (aspnetUser != null)
                        {
                            bool isUserChanged = false;

                            if (aspnetUser.MobileAlias != dto.PhoneNumber) { aspnetUser.MobileAlias = dto.PhoneNumber; isUserChanged = true; }
                            if (aspnetUser.DisplayName != nhanVienOld.TenNhanVien) { aspnetUser.DisplayName = nhanVienOld.TenNhanVien; isUserChanged = true; }
                            if (aspnetUser.IsDeleted != (nhanVienOld.DaXoa)) { aspnetUser.IsDeleted = nhanVienOld.DaXoa; isUserChanged = true; }
                            if (aspnetUser.Avatar != nhanVienOld.AnhDaiDien) { aspnetUser.Avatar = nhanVienOld.AnhDaiDien; isUserChanged = true; }

                            if (isUserChanged) _userRepository.Update(aspnetUser);

                            MembershipUser membershipUser = Membership.GetUser(aspnetUser.UserName);
                            if (membershipUser != null)
                            {
                                try
                                {
                                    if (membershipUser.Email != dto.Email)
                                    {
                                        membershipUser.Email = dto.Email;
                                        Membership.UpdateUser(membershipUser);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Lỗi cập nhật Email (Có thể Email đã tồn tại): " + ex.Message);
                                }
                            }
                            
                        }
                    }

                    scope.Complete();
                    return nhanVienOld;
                }
            } // Kết thúc using (scope), nếu chưa có lệnh scope.Complete() thì tự động Rollback
        }
        public bool Delete(TblNhanVien item)
        {
            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.INVALID_DATA);
            return _nhanVienRepository.Delete(item); // Gọi Delete có Audit từ Repository
        }
        public DataTable GetNhanVienForDetail(Guid idNhanVien)
        {
            return _nhanVienRepository.GetNhanVienForDetail(idNhanVien);
        }
        public TblNhanVien GetNhanVienByCCCD(string maCCCD)
        {
            return _nhanVienRepository.GetByCCCD(maCCCD);
        }

        public string GetTenNhanVienById(Guid id)
        {
            return _nhanVienRepository.GetTenNhanVienById(id);
        }

        public bool IsCCCDExist(Guid id, string maCCCD)
        {
            return _nhanVienRepository.IsCCCDExist(id, maCCCD);
        }

        public bool IsUserAssigned(Guid id, Guid userId)
        {
            return _nhanVienRepository.IsUserAssigned(id, userId);
        }
        private string GenerateUsername(string fullName, DateTime? dob)
        {
            if (string.IsNullOrEmpty(fullName)) return "user" + DateTime.Now.ToString("HHmmss");

            // 1. Chuyển Tiếng Việt có dấu thành không dấu
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = fullName.Normalize(NormalizationForm.FormD);
            string nameUnsigned = regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');

            // 2. Viết thường và xóa khoảng trắng
            nameUnsigned = nameUnsigned.Replace(" ", "").ToLower();

            // 3. Ghép với ngày tháng năm sinh (Định dạng: ddMMyyyy)
            string dobString = dob.HasValue ? dob.Value.ToString("ddMMyyyy") : DateTime.Now.ToString("ddMMyyyy");

            return nameUnsigned + dobString;
        }
    }
}
