using SubSonic;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace SweetSoft.QLDA.Core.Managers
{
    public class ThanhVienDuAnManager :BaseManager
    {
        private static readonly Lazy<ThanhVienDuAnManager> _instance = new Lazy<ThanhVienDuAnManager>(() => new ThanhVienDuAnManager());
        public static ThanhVienDuAnManager Instance => _instance.Value;
        private readonly ThanhVienDuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public ThanhVienDuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new ThanhVienDuAnRepository(_auditManager);
        }
        public List<NhanVienProjectDTO> GetChiTietDuAnCuaNhanVien(Guid idNhanVien)
        {
            DataTable dt = _repository.GetProjectDetailByNhanVien(idNhanVien);
            if (dt == null || dt.Rows.Count == 0) return new List<NhanVienProjectDTO>();

            var projects = new List<NhanVienProjectDTO>();

            // BƯỚC 1: GOM NHÓM THEO DỰ ÁN (Project)
            var groupProjects = dt.AsEnumerable().GroupBy(r => r.Field<Guid>("IdDuAn"));

            foreach (var pGroup in groupProjects)
            {
                var rowProj = pGroup.First();
                var projDTO = new NhanVienProjectDTO
                {
                    IdDuAn = pGroup.Key,
                    MaDuAn = rowProj.Field<string>("MaDuAn"),
                    TenDuAn = rowProj.Field<string>("TenDuAn"),
                    VaiTro = rowProj["VaiTro"] != DBNull.Value ? rowProj.Field<string>("VaiTro") : "Chưa phân quyền",
                    TrangThai = rowProj.Field<byte>("TrangThaiDuAn") // Ép kiểu byte theo chuẩn Enum của ông
                };

                // BƯỚC 2: GOM NHÓM THEO GIAI ĐOẠN (Phase)
                var groupPhases = pGroup.GroupBy(r => r.Field<Guid>("IdPhase"));
                foreach (var phGroup in groupPhases)
                {
                    var rowPhase = phGroup.First();
                    var phaseDTO = new NhanVienPhaseDTO
                    {
                        IdPhase = phGroup.Key,
                        TenPhase = rowPhase.Field<string>("TenPhase"),
                        TotalWorkload = 0 // Khởi tạo biến đếm
                    };

                    // BƯỚC 3: NHỒI DANH SÁCH TASK CON (Và cộng dồn Workload)
                    foreach (var rowTask in phGroup)
                    {
                        var taskDTO = new NhanVienTaskDTO
                        {
                            IdTask = rowTask.Field<Guid>("IdTask"),
                            MaTask = rowTask.Field<string>("MaTask"),
                            TenTask = rowTask.Field<string>("TenTask"),
                            NgayBatDau = rowTask.Field<DateTime>("NgayBatDau"),
                            NgayKetThuc = rowTask.Field<DateTime>("NgayKetThuc"),
                            ThoiHanNgay = rowTask.Field<int>("ThoiHanNgay"),
                            DiemUuTien = rowTask.Field<int>("DiemUuTien"),
                            TrangThaiTask = Convert.ToInt32(rowTask["TrangThaiTask"])
                        };

                        // Định tuyến hiển thị trạng thái Lịch Task
                        if (taskDTO.TrangThaiTask == 2) taskDTO.TrangThaiText = "Hoàn thành";
                        else if (taskDTO.NgayBatDau > DateTime.Today) taskDTO.TrangThaiText = "Sắp tới (Chưa bắt đầu)";
                        else taskDTO.TrangThaiText = "Lịch hợp lệ";

                        phaseDTO.Tasks.Add(taskDTO);
                        phaseDTO.TotalWorkload += taskDTO.Workload; // Cộng dồn W = D * P
                    }

                    // BƯỚC 4: CHỐT SỐ LIỆU TỔNG QUAN CHO PHASE
                    phaseDTO.MinStartDate = phaseDTO.Tasks.Min(t => t.NgayBatDau);
                    phaseDTO.MaxEndDate = phaseDTO.Tasks.Max(t => t.NgayKetThuc);

                    // Tính Capacity (Dùng hàm có sẵn đếm ngày làm việc)
                    phaseDTO.CapacityDays = LichBieuChungManager.Instance.CountWorkingDaysInRange(phaseDTO.MinStartDate, phaseDTO.MaxEndDate);

                    projDTO.Phases.Add(phaseDTO);
                }

                // BƯỚC 5: CHỐT SỐ LIỆU TỔNG QUAN CHO PROJECT
                projDTO.MinStartDate = projDTO.Phases.Min(p => p.MinStartDate);
                projDTO.MaxEndDate = projDTO.Phases.Max(p => p.MaxEndDate);

                projects.Add(projDTO);
            }

            return projects;
        }
        public TblThanhVienDuAn AddOrUpdate(TblThanhVienDuAn dto)
        {
            BusinessValidator.ThrowIf(dto.IdNhanVien == Guid.Empty, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIf(!dto.IdVaiTroDuAn.HasValue || dto.IdVaiTroDuAn.Value == Guid.Empty, BackEndResourceKeys.INVALID_DATA);

            TblThanhVienDuAn thanhVienDuAn = _repository.GetNhanVienIsActiveInDuAn(dto.IdNhanVien.Value, dto.IdDuAn, dto.IdVaiTroDuAn.Value);

            if (thanhVienDuAn != null)
            {
                thanhVienDuAn.IdVaiTroDuAn = dto.IdVaiTroDuAn;
                thanhVienDuAn.GhiChu = dto.GhiChu;
                thanhVienDuAn.NguoiCapNhat = SweetContext.Current.UserName;
                thanhVienDuAn.NgayCapNhat = DateTime.UtcNow;
                return _repository.Update(thanhVienDuAn);
            }
            else
            {
                thanhVienDuAn = dto.Clone() as TblThanhVienDuAn;
                BusinessValidator.ThrowIfNull(thanhVienDuAn, BackEndResourceKeys.INVALID_DATA);

                thanhVienDuAn.IdThanhVienDuAn = UUIDv7.NewGuid();
                thanhVienDuAn.DaXoa = false;
                thanhVienDuAn.NgayThamGia = DateTime.UtcNow;
                thanhVienDuAn.NguoiTao = SweetContext.Current.UserName;
                thanhVienDuAn.NgayTao = DateTime.UtcNow;
                thanhVienDuAn.NguoiCapNhat = null;
                thanhVienDuAn.NgayCapNhat = null;
                thanhVienDuAn = _repository.Insert(thanhVienDuAn, BackEndResourceKeys.HISTORY_ADDED_TO_CONTAINER);
                BusinessValidator.ThrowIfNull(thanhVienDuAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return thanhVienDuAn;
            }
        }
        //Thêm đống hàm dưới đây
        //1. tHằng này là xóa mềm có ngoại lệ nói chung để từ PM A sang PM B, qua DuAnManager coi
        public void DeleteByDuAnAndVaiTroExcept(Guid idDuAn, Guid idVaiTro, Guid idNhanVienGiuLai)
        {
            foreach (var tv in _repository.GetByIdDuAnAndVaiTroExcept(idDuAn, idVaiTro, idNhanVienGiuLai))
            {
                tv.DaXoa = true;
                tv.NguoiCapNhat = SweetContext.Current.UserName;
                tv.NgayCapNhat = DateTime.UtcNow;
                _repository.Update(tv);
            }
        }

        public List<Guid> GetIdNhanVienByDuAnAndVaiTro(Guid idDuAn, Guid idVaiTro)
        {
            return _repository.GetIdNhanVienByDuAnAndVaiTro(idDuAn, idVaiTro);   // chỉ còn 1 dòng, gọi thẳng xuống Repository
        }
        public void DeleteOne(Guid idDuAn, Guid idVaiTro, Guid idNhanVien) //Cái này dùng cho logic cập nhật danh sách nhân viên được chọn, qua DuAnManager coi
        {
            TblThanhVienDuAn tv = _repository.GetNhanVienIsActiveInDuAn(idNhanVien, idDuAn, idVaiTro);
            if (tv == null) return;   // không active thì thôi, khỏi làm gì

            tv.DaXoa = true;
            tv.NguoiCapNhat = SweetContext.Current.UserName;
            tv.NgayCapNhat = DateTime.UtcNow;
            _repository.Update(tv, BackEndResourceKeys.HISTORY_REMOVED_FROM_CONTAINER);
        }
        public List<Guid> GetAllActiveMemberIds(Guid idDuAn)
        {
            var list = _repository.GetByIdDuAn(idDuAn);
            List<Guid> result = new List<Guid>();

            foreach (var item in list)
            {
                if (!item.DaXoa && item.IdNhanVien.HasValue)
                {
                    if (!result.Contains(item.IdNhanVien.Value))
                    {
                        result.Add(item.IdNhanVien.Value);
                    }
                }
            }
            return result;
        }
    }
}
