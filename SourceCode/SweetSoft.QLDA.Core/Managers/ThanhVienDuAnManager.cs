using SubSonic;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Models;
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
    public class ThanhVienDuAnManager : BaseManager
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
        public List<NhanVienProjectSummaryDTO> GetDanhSachDuAnCuaNhanVien(Guid idNhanVien, string keyword = null, byte? statusId = null)
        {
            DataTable dt = _repository.GetDanhSachDuAnCuaNhanVienData(idNhanVien, keyword, statusId);
            var result = new List<NhanVienProjectSummaryDTO>();

            if (dt.Rows.Count == 0) return result;

            foreach (DataRow row in dt.Rows)
            {
                result.Add(new NhanVienProjectSummaryDTO
                {
                    IdDuAn = row.Field<Guid>("IdDuAn"),
                    MaDuAn = row.Field<string>("MaDuAn"),
                    TenDuAn = row.Field<string>("TenDuAn"),
                    VaiTro = row.Field<string>("VaiTro"),
                    TrangThai = row.Field<byte>("TrangThaiDuAn"),
                    ProjectStartDate = row.Field<DateTime?>("ProjectStartDate"),
                    ProjectEndDate = row.Field<DateTime?>("ProjectEndDate"),
                    Total_E_All_Employees = Convert.ToDouble(row["Total_E_All_Employees"]),
                    Total_E_My_Employee = Convert.ToDouble(row["Total_E_My_Employee"])
                });
            }

            return result;
        }

        public NhanVienProjectDTO GetChiTietDuAnCuaNhanVien(Guid idNhanVien, Guid idDuAn)
        {
            DataTable dt = _repository.GetChiTietDuAnCuaNhanVienData(idNhanVien, idDuAn);

            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow projectRow = dt.Rows[0];
            var project = new NhanVienProjectDTO
            {
                IdDuAn = projectRow.Field<Guid>("IdDuAn"),
                MaDuAn = projectRow.Field<string>("MaDuAn"),
                TenDuAn = projectRow.Field<string>("TenDuAn"),
                VaiTro = projectRow.Field<string>("VaiTro"),
                TrangThai = projectRow.Field<byte>("TrangThaiDuAn"),
                ProjectStartDate = projectRow.Field<DateTime?>("ProjectStartDate"),
                ProjectEndDate = projectRow.Field<DateTime?>("ProjectEndDate")
            };

            // Project có thể hoàn toàn không có task của nhân viên.
            // Khi đó LEFT JOIN trả về 1 row Project, nhưng IdTask = DBNull.
            var taskRows = dt.AsEnumerable().Where(row => row["IdTask"] != DBNull.Value).ToList();
            if (taskRows.Count == 0) return project;

            var phaseGroups = taskRows.GroupBy(row => row.Field<Guid>("IdPhase"));

            foreach (var phaseGroup in phaseGroups)
            {
                DataRow phaseRow = phaseGroup.First();
                var phase = new NhanVienPhaseDTO
                {
                    IdPhase = phaseGroup.Key,
                    MaPhase = phaseRow.Field<string>("MaPhase"),
                    TenPhase = phaseRow.Field<string>("TenPhase")
                };

                foreach (DataRow taskRow in phaseGroup)
                {
                    var task = new NhanVienTaskDTO
                    {
                        IdTask = taskRow.Field<Guid>("IdTask"),
                        MaTask = taskRow.Field<string>("MaTask"),
                        TenTask = taskRow.Field<string>("TenTask"),
                        NgayBatDau = taskRow.Field<DateTime?>("NgayBatDau"),
                        NgayKetThuc = taskRow.Field<DateTime?>("NgayKetThuc"),
                        NgayHoanThanhThucTe = taskRow.Field<DateTime?>("NgayHoanThanhThucTe"),
                        ThoiHanNgay = taskRow.Field<int>("ThoiHanNgay"),
                        DiemUuTien = taskRow.Field<int>("DiemUuTien"),
                        HeSoDongGop = Convert.ToDouble(taskRow["HeSoDongGop"]),
                        TenDoUuTien = taskRow.Field<string>("TenDoUuTien"),
                        MaTaskCha = taskRow.Field<string>("MaTaskCha"),
                        TenTaskCha = taskRow.Field<string>("TenTaskCha"),
                        TenPhaseGoc = phaseRow.Field<string>("TenPhase"),
                        TrangThaiTask = taskRow.Field<byte>("TrangThaiTask"),
                        AssigneeCount = taskRow.Field<int>("AssigneeCount"),
                        IsMyTask = taskRow.Field<int>("IsMyTask") == 1
                    };
                    phase.Tasks.Add(task);
                }
                project.Phases.Add(phase);
            }

            return project;
        }

        public int CountDuAnCuaNhanVien(Guid idNhanVien)
        {
            return _repository.CountDuAnCuaNhanVien(idNhanVien);
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
        public DataTable GetThanhVienDuAnDetail(Guid idDuAn)
        {
            return _repository.GetThanhVienDuAnDetail(idDuAn);
        }
    }
}
