using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class LichBieuChungManager : BaseManager
    {
        private static readonly Lazy<LichBieuChungManager> _instance = new Lazy<LichBieuChungManager>(() => new LichBieuChungManager());
        public static LichBieuChungManager Instance => _instance.Value;

        private readonly CauHinhTuanLamViecRepository _tuanRepository;
        private readonly LichNgoaiLeRepository _ngoaiLeRepository;
        private readonly AuditManager _auditManager;

        public LichBieuChungManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _tuanRepository = new CauHinhTuanLamViecRepository(_auditManager);
            _ngoaiLeRepository = new LichNgoaiLeRepository(_auditManager);
        }

        #region NHÓM 1: QUẢN LÝ CẤU HÌNH TUẦN LÀM VIỆC MẶC ĐỊNH (LỚP 1)

        public List<TblCauHinhTuanLamViec> GetAllCauHinhTuan()
        {
            return _tuanRepository.GetAll();
        }

        public TblCauHinhTuanLamViec UpdateCauHinhTuan(TblCauHinhTuanLamViec item)
        {
            if (item == null) return null;

            // Lấy ID người dùng đăng nhập hiện tại từ SweetContext (Chuẩn Framework)
            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;

            // Bơm UserId vào đối tượng trước khi đẩy xuống Repository
            item.NguoiCapNhat = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

            return _tuanRepository.Update(item);
        }

        #endregion

        #region NHÓM 2: QUẢN LÝ LỊCH NGOẠI LỆ - LỄ TẾT/LÀM BÙ (LỚP 2)

        public DataTable SearchLichNgoaiLePaging(string searchTerm, bool? isWorkingDay, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _ngoaiLeRepository.SearchPaging(searchTerm, isWorkingDay, orderBy, pageNumber, pageSize, out totalRecord);
        }
        public TblLichNgoaiLe GetLichNgoaiLeById(Guid id)
        {
            return _ngoaiLeRepository.GetById(id);
        }

        // Nhớ using SweetSoft.QLDA.Core.ExceptionHelpers; nếu chưa có để dùng BusinessValidator

        public TblLichNgoaiLe CreateOrUpdate(TblLichNgoaiLe dto)
        {
            // 1. Validate dữ liệu cơ bản (Trạm Tiền kiểm tra)
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenNgoaiLe, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenNgoaiLe));

            // 2. Lấy ID người dùng đăng nhập hiện tại từ SweetContext (Chuẩn Framework)
            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;

            // 3. Phân luồng: Thêm mới hay Cập nhật?
            bool isInsert = (dto.IdNgoaiLe == Guid.Empty);

            if (isInsert)
            {
                // XỬ LÝ THÊM MỚI
                dto.IdNgoaiLe = Guid.NewGuid();
                dto.NgayTao = DateTime.Now;
                dto.NguoiTao = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

                return _ngoaiLeRepository.Insert(dto);
            }
            else
            {
                // XỬ LÝ CẬP NHẬT
                TblLichNgoaiLe existingItem = _ngoaiLeRepository.GetById(dto.IdNgoaiLe);
                BusinessValidator.ThrowIfNull(existingItem, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdNgoaiLe), ErrorCodes.NotFound);

                // Map dữ liệu từ UI (dto) sang Object DB (existingItem)
                existingItem.TenNgoaiLe = dto.TenNgoaiLe;
                existingItem.NgayBatDau = dto.NgayBatDau;
                existingItem.NgayKetThuc = dto.NgayKetThuc;
                existingItem.MoTa = dto.MoTa;
                existingItem.LaNgayLamViec = dto.LaNgayLamViec;

                // Cập nhật vết audit
                existingItem.NgayCapNhat = DateTime.Now;
                existingItem.NguoiCapNhat = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

                return _ngoaiLeRepository.Update(existingItem);
            }
        }

        public bool DeleteLichNgoaiLe(Guid id)
        {
            var item = _ngoaiLeRepository.GetById(id);
            if (item != null)
            {
                return _ngoaiLeRepository.Delete(item);
            }
            return false;
        }

        #endregion

        #region NHÓM 3: ĐỘNG CƠ TÍNH TOÁN THỜI GIAN (CORE ENGINE)

        public bool CheckIsWorkingDay(DateTime date)
        {
            // Bước 1: Ưu tiên check Lớp 2 (Ngoại lệ / Lễ Tết / Làm bù) trước
            // Dùng hàm GetExceptionsInRange mà bạn đã có ở Repository
            var exceptions = _ngoaiLeRepository.GetExceptionsInRange(date, date);

            if (exceptions != null && exceptions.Count > 0)
            {
                // Có sự kiện ngoại lệ rơi vào ngày này!
                // Trả về true nếu là lịch làm bù, false nếu là lịch nghỉ lễ
                return exceptions[0].LaNgayLamViec;
            }

            // Bước 2: Nếu không có lễ tết gì, check Lớp 1 (Lịch chuẩn theo thứ trong tuần)
            int dayOfWeek = (int)date.DayOfWeek; // 0: CN, 1: T2, ..., 6: T7
            var standardConfig = _tuanRepository.GetByDayOfWeek(dayOfWeek);

            if (standardConfig != null)
            {
                return standardConfig.LaNgayLamViec;
            }

            // Mặc định an toàn (Phòng hờ lỗi DB)
            return false;
        }

        public DateTime CalculateTaskEndDate(DateTime ngayBatDau, int thoiHanNgay)
        {
            // Nếu PM nhập thời hạn = 0 hoặc số âm (lỗi nhập liệu), trả về luôn ngày bắt đầu
            if (thoiHanNgay <= 0)
                return ngayBatDau;

            DateTime currentDate = ngayBatDau.Date; // Bỏ qua giờ phút giây, chỉ lấy ngày
            int remainingDays = thoiHanNgay; // Biến đếm ngược số ngày cần phân bổ

            // Vòng lặp rải ngày: Chừng nào chưa rải hết số ngày công thì chưa dừng
            while (remainingDays > 0)
            {
                // Kiểm tra xem ngày đang xét có phải ngày đi làm không
                if (CheckIsWorkingDay(currentDate))
                {
                    // Trừ đi 1 ngày công cần phân bổ
                    remainingDays--;

                    // NẾU ĐÃ RẢI HẾT NGÀY CÔNG (remainingDays == 0), thì DỪNG LẠI NGAY!
                    // currentDate lúc này chính là ngày hoàn thành cuối cùng.
                    if (remainingDays == 0)
                    {
                        break;
                    }
                }

                // Nhảy sang ngày tiếp theo để kiểm tra (Vòng lặp tiếp tục)
                currentDate = currentDate.AddDays(1);
            }

            return currentDate;
        }

        #endregion
        
    }
}