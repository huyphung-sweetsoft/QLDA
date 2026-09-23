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
        public List<NhanVienProjectDTO> GetChiTietDuAnCuaNhanVien(Guid idNhanVien)
        {
            string sql = @"
    DECLARE @IdNhanVien UNIQUEIDENTIFIER = @EmpId;

    WITH MyProjects AS (
        SELECT 
            da.IdDuAn, da.MaDuAn, da.TenDuAn, da.TrangThai AS TrangThaiDuAn,
            da.NgayBatDau AS ProjectStartDate,
            ISNULL(da.NgayHoanThanhThucTe, da.NgayDuKienHoanThanh) AS ProjectEndDate,
            vt.TenVaiTro AS VaiTro
        FROM TblDuAn da
        INNER JOIN TblThanhVienDuAn tv ON da.IdDuAn = tv.IdDuAn
        INNER JOIN TblVaiTroDuAn vt ON tv.IdVaiTroDuAn = vt.IdVaiTroDuAn
        WHERE tv.IdNhanVien = @IdNhanVien AND tv.DaXoa = 0 AND da.DaXoa = 0
    ),
    TaskTree AS (
        -- Anchor
        SELECT 
            IdCongViec, IdCongViecCha, TenCongViec,
            IdCongViec AS IdPhase, 
            MaCongViec AS MaPhase, -- Móc Mã Phase
            TenCongViec AS TenPhase
        FROM TblCongViec 
        WHERE IdCongViecCha IS NULL AND DaXoa = 0
        
        UNION ALL
        
        -- Recursive
        SELECT 
            c.IdCongViec, c.IdCongViecCha, c.TenCongViec,
            t.IdPhase, 
            t.MaPhase, -- Kế thừa Mã Phase từ gốc
            t.TenPhase
        FROM TblCongViec c
        INNER JOIN TaskTree t ON c.IdCongViecCha = t.IdCongViec
        WHERE c.DaXoa = 0
    ),
    AllLeafTasks AS (
        SELECT 
            cv.IdDuAn,
            tree.IdPhase, 
            tree.MaPhase, 
            tree.TenPhase, 
            parent.MaCongViec AS MaTaskCha,
            parent.TenCongViec AS TenTaskCha, 
            cv.IdCongViec AS IdTask, cv.MaCongViec AS MaTask, cv.TenCongViec AS TenTask,
            cv.NgayBatDau, cv.NgayKetThuc, cv.ThoiHanNgay, cv.TrangThai AS TrangThaiTask,
            ISNULL(ut.DiemUuTien, 1) AS DiemUuTien,
            ISNULL(ut.TenDoUuTien, N'Thấp') AS TenDoUuTien
        FROM TblCongViec cv
        INNER JOIN MyProjects mp ON cv.IdDuAn = mp.IdDuAn
        LEFT JOIN TaskTree tree ON cv.IdCongViec = tree.IdCongViec 
        LEFT JOIN TblCongViec parent ON cv.IdCongViecCha = parent.IdCongViec 
        LEFT JOIN TblDoUuTien ut ON cv.IdDoUuTien = ut.IdDoUuTien
        WHERE cv.DaXoa = 0 
        AND cv.IdCongViecCha IS NOT NULL -- [SỬA LẠI]: Bắt buộc phải nằm dưới WHERE
        AND NOT EXISTS (SELECT 1 FROM TblCongViec child WHERE child.IdCongViecCha = cv.IdCongViec AND child.DaXoa = 0)
    ),
    TaskAssignees AS (
        SELECT 
            IdCongViec,
            COUNT(IdNhanVien) AS AssigneeCount,
            MAX(CASE WHEN IdNhanVien = @IdNhanVien THEN 1 ELSE 0 END) AS IsMyTask
        FROM TblCongViec_NhanVien
        GROUP BY IdCongViec
    )
    SELECT 
        mp.IdDuAn, mp.MaDuAn, mp.TenDuAn, mp.VaiTro, mp.TrangThaiDuAn, mp.ProjectStartDate, mp.ProjectEndDate,
        t.IdPhase, t.MaPhase, t.TenPhase, t.MaTaskCha, t.TenTaskCha, t.IdTask, t.MaTask, t.TenTask, t.NgayBatDau, t.NgayKetThuc, t.ThoiHanNgay, t.DiemUuTien, t.TenDoUuTien, t.TrangThaiTask,
        ISNULL(ta.AssigneeCount, 0) AS AssigneeCount,
        ISNULL(ta.IsMyTask, 0) AS IsMyTask
    FROM MyProjects mp
    INNER JOIN AllLeafTasks t ON mp.IdDuAn = t.IdDuAn
    LEFT JOIN TaskAssignees ta ON t.IdTask = ta.IdCongViec
    WHERE EXISTS (
        SELECT 1 FROM AllLeafTasks t2
        INNER JOIN TaskAssignees ta2 ON t2.IdTask = ta2.IdCongViec
        WHERE t2.IdPhase = t.IdPhase AND ta2.IsMyTask = 1
    )
    ORDER BY mp.ProjectStartDate DESC, t.MaTask ASC;";

            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@EmpId", idNhanVien, DbType.Guid);

            DataSet ds = DataService.GetDataSet(cmd);
            DataTable dt = (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();

            if (dt == null || dt.Rows.Count == 0) return new List<NhanVienProjectDTO>();

            var projects = new List<NhanVienProjectDTO>();

            var groupProjects = dt.AsEnumerable().GroupBy(r => r.Field<Guid>("IdDuAn"));
            foreach (var pGroup in groupProjects)
            {
                var rowProj = pGroup.First();
                var projDTO = new NhanVienProjectDTO
                {
                    IdDuAn = pGroup.Key,
                    MaDuAn = rowProj.Field<string>("MaDuAn"),
                    TenDuAn = rowProj.Field<string>("TenDuAn"),
                    VaiTro = rowProj.Field<string>("VaiTro"),
                    TrangThai = rowProj.Field<byte>("TrangThaiDuAn"),
                    ProjectStartDate = rowProj.Field<DateTime?>("ProjectStartDate"),
                    ProjectEndDate = rowProj.Field<DateTime?>("ProjectEndDate")
                };

                var groupPhases = pGroup.GroupBy(r => r.Field<Guid>("IdPhase"));
                foreach (var phGroup in groupPhases)
                {
                    var rowPhase = phGroup.First();
                    var phaseDTO = new NhanVienPhaseDTO
                    {
                        IdPhase = phGroup.Key,
                        MaPhase = rowPhase.Field<string>("MaPhase"), // Mapping Mã
                        TenPhase = rowPhase.Field<string>("TenPhase")
                    };

                    foreach (var rowTask in phGroup)
                    {
                        phaseDTO.Tasks.Add(new NhanVienTaskDTO
                        {
                            IdTask = rowTask.Field<Guid>("IdTask"),
                            MaTask = rowTask.Field<string>("MaTask"),
                            TenTask = rowTask.Field<string>("TenTask"),
                            NgayBatDau = rowTask.Field<DateTime?>("NgayBatDau"),
                            NgayKetThuc = rowTask.Field<DateTime?>("NgayKetThuc"),
                            ThoiHanNgay = rowTask.Field<int>("ThoiHanNgay"),
                            DiemUuTien = rowTask.Field<int>("DiemUuTien"),
                            TenDoUuTien = rowTask.Field<string>("TenDoUuTien"),
                            MaTaskCha = rowTask.Field<string>("MaTaskCha"), // Mapping Mã
                            TenTaskCha = rowTask.Field<string>("TenTaskCha"),
                            TenPhaseGoc = rowPhase.Field<string>("TenPhase"),
                            TrangThaiTask = rowTask.Field<byte>("TrangThaiTask"),
                            AssigneeCount = rowTask.Field<int>("AssigneeCount"),
                            IsMyTask = rowTask.Field<int>("IsMyTask") == 1
                        });
                    }
                    projDTO.Phases.Add(phaseDTO);
                }
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
        public DataTable GetThanhVienDuAnDetail(Guid idDuAn)
        {
            return _repository.GetThanhVienDuAnDetail(idDuAn);
        }
    }
}
