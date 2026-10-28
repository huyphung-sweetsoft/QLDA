using SubSonic;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Models;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Org.BouncyCastle.Asn1.Crmf;
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
        private sealed class GioPerson
        {
            public Guid IdNhanVien;
            public long GiayThuong, GiayTangCa;
            public long? PhutTuKhaiThuong, PhutTuKhaiTangCa;
            public long? PhutPmThuong, PhutPmTangCa;
            public bool CoPhien, CoPhienMo, DaPmChot, CoPmNgoaiDanhSach;
        }
        private sealed class GioTask
        {
            public Guid IdTask, IdDuAn;
            public int TrangThai;
            public readonly List<GioPerson> Nguoi = new List<GioPerson>();
            public bool DaChot
            {
                get
                {
                    return (TrangThai == 2 || TrangThai == 3) && Nguoi.Count > 0
                        && Nguoi.All(x=>x.DaPmChot && x.PhutPmThuong.HasValue && x.PhutPmTangCa.HasValue)
                        && !Nguoi.Any(x=>x.CoPhienMo || x.CoPmNgoaiDanhSach);
                }
            }
        }
        private static object Val(DataRow row,string col)
        {
            return row.Table.Columns.Contains(col) && row[col]!=DBNull.Value ? row[col] : null;
        }
        private static Guid G(DataRow row,string col) { object v=Val(row,col);return v==null?Guid.Empty:(Guid)v; }
        private static string S(DataRow row,string col) { object v=Val(row,col);return v==null?string.Empty:Convert.ToString(v); }
        private static long L(DataRow row,string col) { object v=Val(row,col);return v==null?0:Convert.ToInt64(v); }
        private static long? LN(DataRow row,string col) { object v=Val(row,col);return v==null?(long?)null:Convert.ToInt64(v); }
        private static DateTime? DT(DataRow row,string col) { object v=Val(row,col);return v==null?(DateTime?)null:Convert.ToDateTime(v); }
        private static bool B(DataRow row,string col) { return L(row,col)!=0; }
        private static byte By(DataRow row,string col) { object v=Val(row,col);return v==null?(byte)0:Convert.ToByte(v); }

        private Dictionary<Guid,List<GioTask>> LayGioV2(Guid idNhanVien,IEnumerable<Guid> idDuAns)
        {
            DataTable dt=_repository.GetGioCongNhanVienV2TheoDanhSachDuAnData(idNhanVien,idDuAns);
            var result=new Dictionary<Guid,List<GioTask>>();
            if(dt==null || dt.Rows.Count==0)return result;
            foreach(var projectGroup in dt.AsEnumerable().GroupBy(r=>G(r,"IdDuAn")))
            {
                var tasks=new List<GioTask>();
                foreach(var taskGroup in projectGroup.GroupBy(r=>G(r,"IdCongViec")))
                {
                    DataRow first=taskGroup.First();
                    var task=new GioTask { IdDuAn=projectGroup.Key,IdTask=taskGroup.Key,
                        TrangThai=Convert.ToInt32(Val(first,"TrangThaiTask")??0) };
                    foreach(DataRow row in taskGroup)
                    {
                        task.Nguoi.Add(new GioPerson {
                            IdNhanVien=G(row,"IdNhanVien"),GiayThuong=L(row,"GiayThuong"),GiayTangCa=L(row,"GiayTangCa"),
                            CoPhien=B(row,"CoPhien"),CoPhienMo=B(row,"CoPhienDangMo"),
                            PhutTuKhaiThuong=LN(row,"PhutThuongTuKhai"),PhutTuKhaiTangCa=LN(row,"PhutTangCaTuKhai"),
                            PhutPmThuong=LN(row,"PhutThuongPmChot"),PhutPmTangCa=LN(row,"PhutTangCaPmChot"),
                            DaPmChot=B(row,"DaPmChot"),CoPmNgoaiDanhSach=B(row,"CoPmChotNgoaiDanhSach")
                        });
                    }
                    tasks.Add(task);
                }
                result[projectGroup.Key]=tasks;
            }
            return result;
        }

        private static void TongHop(NhanVienGioCongSummaryDTO target,IEnumerable<GioTask> tasks,Guid idNhanVien,bool isProject)
        {
            var list=tasks.ToList();
            target.SoTaskToanDuAnCanChot=list.Count;
            target.SoTaskToanDuAnDaChot=list.Count(t=>t.DaChot);
            target.CoPhienDangMo=list.Any(t=>t.Nguoi.Any(p=>p.CoPhienMo));
            target.TongPhutPmChotDuAn=list.Where(t=>t.DaChot).Sum(t=>t.Nguoi.Sum(p=>p.PhutPmThuong.GetValueOrDefault()+p.PhutPmTangCa.GetValueOrDefault()));
            foreach(GioTask task in list)
            {
                GioPerson p=task.Nguoi.FirstOrDefault(x=>x.IdNhanVien==idNhanVien);
                if(p==null)continue;
                target.SoTaskLienQuan++;
                target.GiayThuongHeThong+=p.GiayThuong;
                target.GiayTangCaHeThong+=p.GiayTangCa;
                target.CoGioHeThong |= p.CoPhien;
                bool reported=p.PhutTuKhaiThuong.HasValue && p.PhutTuKhaiTangCa.HasValue;
                if(reported)
                {
                    target.SoTaskCoTuKhai++;
                    target.CoTuKhai=true;
                    target.PhutThuongTuKhai+=p.PhutTuKhaiThuong.Value;
                    target.PhutTangCaTuKhai+=p.PhutTuKhaiTangCa.Value;
                }
                if(task.DaChot && p.DaPmChot)
                {
                    target.SoTaskDaChot++;
                    target.CoPmChot=true;
                    target.PhutThuongPmChot+=p.PhutPmThuong.GetValueOrDefault();
                    target.PhutTangCaPmChot+=p.PhutPmTangCa.GetValueOrDefault();
                }
            }
            if(isProject)
                target.DaKetToanDuAn=target.LaDuAnKetThuc && list.Count>0 && list.All(t=>t.DaChot)
                    && !target.CoPhienDangMo;
        }

        private static void DocProject(DataRow row,NhanVienProjectSummaryDTO item)
        {
            item.IdDuAn=G(row,"IdDuAn"); item.MaDuAn=S(row,"MaDuAn");item.TenDuAn=S(row,"TenDuAn");
            item.VaiTro=S(row,"VaiTro");item.TrangThai=By(row,"TrangThaiDuAn");
            item.ProjectStartDate=DT(row,"ProjectStartDate");item.ProjectEndDate=DT(row,"ProjectEndDate");
            item.ProjectActualEndDate=DT(row,"ProjectActualEndDate");item.IsCurrentMember=B(row,"IsCurrentMember");
            item.TrangThaiDuAnCode=item.TrangThai;
            DuAnStatus status=(DuAnStatus)item.TrangThai;
            item.LaDuAnKetThuc=status==DuAnStatus.HoanThanh || status==DuAnStatus.KetThuc;
        }
        // One lightweight summary row per project; detailed report stays on the V5 query.
        public List<NhanVienProjectSummaryDTO> GetDanhSachDuAnCuaNhanVien(Guid idNhanVien,string keyword=null,byte? statusId=null)
        {
            var watch=System.Diagnostics.Stopwatch.StartNew();
            DataTable dt=_repository.GetDanhSachDuAnCuaNhanVienData(idNhanVien,keyword,statusId);
            long listMs=watch.ElapsedMilliseconds;
            var result=new List<NhanVienProjectSummaryDTO>();
            if(dt==null || dt.Rows.Count==0)
            {
                GhiThoiGianTruyVan("list",listMs,0,watch.ElapsedMilliseconds,0);
                return result;
            }
            var ids=dt.AsEnumerable().Select(r=>G(r,"IdDuAn")).Where(x=>x!=Guid.Empty).Distinct().ToList();
            DataTable summary=_repository.GetTongHopGioCongNhanVienV2DanhSachData(idNhanVien,ids);
            long hourMs=watch.ElapsedMilliseconds-listMs;
            var byProject=summary.AsEnumerable().ToDictionary(r=>G(r,"IdDuAn"),r=>r);
            foreach(DataRow row in dt.Rows)
            {
                var project=new NhanVienProjectSummaryDTO();DocProject(row,project);
                DataRow data;
                if(byProject.TryGetValue(project.IdDuAn,out data))
                {
                    project.SoTaskToanDuAnCanChot=Convert.ToInt32(L(data,"SoTaskToanDuAnCanChot"));
                    project.SoTaskToanDuAnDaChot=Convert.ToInt32(L(data,"SoTaskToanDuAnDaChot"));
                    project.CoPhienDangMo=B(data,"CoPhienDangMo");
                    project.TongPhutPmChotDuAn=L(data,"TongPhutPmChotDuAn");
                    project.SoTaskLienQuan=Convert.ToInt32(L(data,"SoTaskLienQuan"));
                    project.GiayThuongHeThong=L(data,"GiayThuongHeThong");
                    project.GiayTangCaHeThong=L(data,"GiayTangCaHeThong");
                    project.CoGioHeThong=B(data,"CoGioHeThong");
                    project.SoTaskCoTuKhai=Convert.ToInt32(L(data,"SoTaskCoTuKhai"));
                    project.CoTuKhai=project.SoTaskCoTuKhai>0;
                    project.PhutThuongTuKhai=L(data,"PhutThuongTuKhai");
                    project.PhutTangCaTuKhai=L(data,"PhutTangCaTuKhai");
                    project.SoTaskDaChot=Convert.ToInt32(L(data,"SoTaskDaChot"));
                    project.CoPmChot=project.SoTaskDaChot>0;
                    project.PhutThuongPmChot=L(data,"PhutThuongPmChot");
                    project.PhutTangCaPmChot=L(data,"PhutTangCaPmChot");
                    project.DaKetToanDuAn=project.LaDuAnKetThuc && project.SoTaskToanDuAnCanChot>0
                        && project.SoTaskToanDuAnDaChot==project.SoTaskToanDuAnCanChot && !project.CoPhienDangMo;
                }
                result.Add(project);
            }
            GhiThoiGianTruyVan("list",listMs,hourMs,watch.ElapsedMilliseconds,result.Count);
            return result;
        }

        private static void GhiThoiGianTruyVan(string action,long listMs,long summaryMs,long totalMs,int projects)
        {
            // Diagnostic opt-in: add NvdPerfDiagnostics=true to appSettings on TEST only.
            // No request data, employee ID, or search keyword goes into diagnostics.
            if(!string.Equals(System.Configuration.ConfigurationManager.AppSettings["NvdPerfDiagnostics"],"true",StringComparison.OrdinalIgnoreCase))return;
            System.Diagnostics.Trace.WriteLine(string.Format("[NVD PERF] {0} projects={1} listSql={2}ms summarySql={3}ms total={4}ms",
                action,projects,listMs,summaryMs,totalMs));
        }

        public NhanVienProjectDTO GetChiTietDuAnCuaNhanVien(Guid idNhanVien,Guid idDuAn)
        {
            DataTable dt=_repository.GetChiTietDuAnCuaNhanVienData(idNhanVien,idDuAn);
            if(dt==null || dt.Rows.Count==0)return null;
            var project=new NhanVienProjectDTO();DocProject(dt.Rows[0],project);
            var report=LayGioV2(idNhanVien,new[]{idDuAn});
            List<GioTask> facts;
            if(!report.TryGetValue(idDuAn,out facts))facts=new List<GioTask>();
            TongHop(project,facts,idNhanVien,true);
            var byTask=facts.ToDictionary(x=>x.IdTask);
            var taskRows = dt.AsEnumerable().Where(x => Val(x,"IdTask") != null)
                .GroupBy(x => G(x,"IdTask")).Select(x => x.First());
            foreach (DataRow row in taskRows)
            {
                Guid idTask = G(row,"IdTask");
                var task = new NhanVienTaskDTO {
                    IdTask=idTask, MaTask=S(row,"MaTask"), TenTask=S(row,"TenTask"),
                    NgayBatDau=DT(row,"NgayBatDau"), NgayKetThuc=DT(row,"NgayKetThuc"),
                    NgayHoanThanhThucTe=DT(row,"NgayHoanThanhThucTe"),
                    ThoiHanNgay=Convert.ToInt32(L(row,"ThoiHanNgay")),
                    DiemUuTien=Convert.ToInt32(L(row,"DiemUuTien")),TenDoUuTien=S(row,"TenDoUuTien"),
                    TrangThaiTask=By(row,"TrangThaiTask"),LyDoTreHan=S(row,"LyDoTre"),
                    IsMyTask=true,IsCurrentAssignee=B(row,"IsCurrentAssignee"),
                    IsArchivedTask=B(row,"IsArchivedTask")
                };
                GioTask source;
                if (byTask.TryGetValue(idTask,out source))
                {
                    TongHop(task,new[]{source},idNhanVien,false);
                    task.DaChotTask=source.DaChot;
                }
                project.Tasks.Add(task);
            }
            // Sap xep de nhan vien tim viec dang lam/cho duyet nhanh, roi theo han.
            project.Tasks=project.Tasks
                .OrderBy(t => t.IsArchivedTask ? 1 : 0)
                .ThenBy(t => t.TrangThaiTask == 1 ? 0 : t.TrangThaiTask == 4 ? 1 : t.TrangThaiTask == 0 ? 2 : 3)
                .ThenBy(t => t.NgayKetThuc ?? DateTime.MaxValue)
                .ThenBy(t => t.MaTask ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
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

        public TblThanhVienDuAn GetByNhanVienAndDuAn(Guid idNhanVien, Guid idDuAn)
        {
            return _repository.GetNhanVienIsActiveInDuAn(idNhanVien, idDuAn);
        }
    }
}
