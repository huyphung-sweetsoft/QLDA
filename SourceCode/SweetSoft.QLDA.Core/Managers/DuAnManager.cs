using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Managers
{
    public class DuAnManager: BaseManager
    {
        private static readonly Lazy<DuAnManager> _instance = new Lazy<DuAnManager>(() => new DuAnManager());
        public static DuAnManager Instance => _instance.Value;
        private readonly DuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public DuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new DuAnRepository(_auditManager);
        }

        public DataTable SearchDuAns(string searchTerm, Dictionary<string, object> parameters ,string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, parameters,orderBy, pageNumber, pageSize, out totalRecord);
        }

        public TblDuAn CreateOrUpdate(TblDuAn dto)
        {
            //validate input data
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenDuAn, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenDuAn));
            BusinessValidator.ThrowIf(dto.IdLoaiDuAn == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdLoaiDuAn));
            BusinessValidator.ThrowIf(dto.IdKhachHang == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdKhachHang));
            BusinessValidator.ThrowIf(dto.IdNhanVienQuanLy == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdNhanVienQuanLy));
            BusinessValidator.ThrowIf(dto.NgayDuKienHoanThanh < dto.NgayBatDau, BackEndResourceKeys.INVALID_DATA, nameof(dto.NgayDuKienHoanThanh));

            if (dto.IdHopDongThucHien.HasValue && dto.IdHopDongThucHien != Guid.Empty)
            {
                TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetHopDongById(dto.IdHopDongThucHien.Value);
                BusinessValidator.ThrowIfNull(hopDong, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdHopDongThucHien), ErrorCodes.NotFound);

                bool hopDongDaSuDung = _repository.IsContractUsed(dto.IdHopDongThucHien.Value, dto.IdDuAn);
                BusinessValidator.ThrowIf(hopDongDaSuDung, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdHopDongThucHien), ErrorCodes.Conflict);
            }
            else
            {
                dto.IdHopDongThucHien = null;
            }

            TblDuAn duAn;

            if (dto.IdDuAn != Guid.Empty)
            {
                duAn = _repository.GetById(dto.IdDuAn);
                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdDuAn), ErrorCodes.NotFound);

                ObjectHelper.CopyBusinessProperties(
                     dto,
                     duAn,
                     x => x.IdDuAn,
                     x => x.MaDuAn,
                     x => x.NgayHoanThanhThucTe,
                     x => x.DaXoa,
                     x => x.NguoiTao,
                     x => x.NgayTao,
                     x => x.NguoiCapNhat,
                     x => x.NgayCapNhat,
                     x => x.IdHopDongThucHien);

                Guid? idHopDong = dto.IdHopDongThucHien;
                duAn.TrangThai = dto.TrangThai;
                duAn.IdHopDongThucHien = idHopDong.HasValue && idHopDong.Value != Guid.Empty ? idHopDong : null;
                duAn.NguoiCapNhat = SweetContext.Current.UserName;
                duAn.NgayCapNhat = DateTime.UtcNow;
                duAn = _repository.Update(duAn);
                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                AddNhanVienQuanLy(duAn);
                return duAn;
            }
            else
            {
                duAn = dto.Clone() as TblDuAn;
                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.INVALID_DATA);

                duAn.IdDuAn = UUIDv7.NewGuid();
                duAn.MaDuAn = GenerateProjectCode();
                BusinessValidator.ThrowIf(_repository.GetByMaDuAn(duAn.MaDuAn) != null, BackEndResourceKeys.INVALID_DATA, nameof(duAn.MaDuAn), ErrorCodes.Conflict);
                duAn.DaXoa = false;
                duAn.NguoiTao = SweetContext.Current.UserName;
                duAn.NgayTao = DateTime.UtcNow;
                duAn.NguoiCapNhat = null;
                duAn.NgayCapNhat = null;

                duAn = _repository.Insert(duAn);
                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                AddNhanVienQuanLy(duAn);
                return duAn;
            }
        }
        //Thêm 1 cái overload cho CreateOrUpdate cái này gộp chung với cục trên cũng đc nhưng loạn nên làm tạm này
        public TblDuAn CreateOrUpdate(TblDuAn dto, List<Guid> selectedMemberIds)
        {
            using (var scope = new TransactionScope())//Gọi cái TransactionScope là để đảm bảo ACID gì đó, nói chung là lưu dự án + lưu ds tv thành công cùng lúc
            {                                        //không để xảy ra tình trạng lưu thk này lỗi thk kia                   
                TblDuAn duAn = CreateOrUpdate(dto);
                ReplaceThanhVienDuAn(duAn.IdDuAn, selectedMemberIds);//Gọi thk này để đồng bộ danh sách nhân viên 
                scope.Complete();
                return duAn;
            }
        }
        public bool Delete(TblDuAn dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            TblDuAn duAn = _repository.GetById(dto.IdDuAn);
            BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdDuAn), ErrorCodes.NotFound);

            duAn.DaXoa = true;
            duAn.NguoiCapNhat = SweetContext.Current.UserName;
            duAn.NgayCapNhat = DateTime.UtcNow;
            duAn = _repository.Update(duAn);
            BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return true;
        }

        public TblDuAn GetDuAnById(Guid id)
        {
            return _repository.GetById(id);
        }

        public DataTable GetDetailDuAnById(Guid id)
        {
            return _repository.GetDetailById(id);
        }

        public string GenerateProjectCode()
        {
            return _repository.GenerateMaDuAn();
        }
        // Sửa thằng AddNhanVienQuanLy dưới cho hợp lý hơn tý, phục vụ luôn cho trường hợp đổi PM A sang B
        public void AddNhanVienQuanLy(TblDuAn duAn)
        {
            BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.NOT_FOUND);
            if (!duAn.IdNhanVienQuanLy.HasValue || duAn.IdNhanVienQuanLy == Guid.Empty)
            {
                return;
            }

            TblVaiTroDuAn vaiTroQuanLy = VaiTroDuAnManager.Instance.GetActiveByIdVaiTro("QUAN_LY_DU_AN");
            //Thêm dòng dưới để dọn thk PM A trước khi cho gán Pm cho thk B
            //cái đứa được except ở đây lại chính là thk A trong trường hợp update ko đổi PM, thì lúc này PM A vẫn là A, duAN.IdNhanVienQuanLy vẫn = A
            //thk B ko bao giờ được except, đúng hơn là ko dùng cho thk B vì trong lúc đổi PM, thk B chưa được lưu vào Db nên nó ko dính dáng j tới thk except cả
            ThanhVienDuAnManager.Instance.DeleteByDuAnAndVaiTroExcept(duAn.IdDuAn, vaiTroQuanLy.IdVaiTroDuAn, duAn.IdNhanVienQuanLy.Value);//thêm đúng dòng này thôi
            //Thêm thêm cục dưới đây để fix lỗi treo trong trường hợp đổi từ PM A sang PM B, nhưng B vốn đang là thành viên thuộc dự án, cái dòng này sẽ xóa mềm vai trò thành viên của B và gán mới PM cho B
            TblVaiTroDuAn vaiTroThanhVien = VaiTroDuAnManager.Instance.GetActiveByIdVaiTro("NGUOI_THAM_GIA");
            if (vaiTroThanhVien != null)
            {
                ThanhVienDuAnManager.Instance.DeleteOne(duAn.IdDuAn, vaiTroThanhVien.IdVaiTroDuAn, duAn.IdNhanVienQuanLy.Value);
            }
            TblThanhVienDuAn tv = new TblThanhVienDuAn();
            tv.IdDuAn = duAn.IdDuAn;
            tv.IdNhanVien = duAn.IdNhanVienQuanLy;
            tv.IdVaiTroDuAn = vaiTroQuanLy.IdVaiTroDuAn;
            ThanhVienDuAnManager.Instance.AddOrUpdate(tv);
        }
        //Thêm mới các hàm sau:
        //1. ReplaceThanhVienDuAn: như tên, dùng để cập nhật danh sách thành viên của 1 dự án thôi, dùng trong edit
        //Giải thích logic cho dễ hiểu thì: Giả sử dự án đang có nv BCDE, sau đó muốn bỏ E thêm F thì thay vì nó xóa mềm hết 4 thk cũ rồi thêm 4 dòng mới là BCDF
        //Thì nó chỉ cần xóa mềm thk E và thêm thk F thôi, đỡ rác db
        private void ReplaceThanhVienDuAn(Guid idDuAn, List<Guid> memberIds)
        {
            TblVaiTroDuAn vaiTroThanhVien = VaiTroDuAnManager.Instance.GetActiveByIdVaiTro("NGUOI_THAM_GIA");
            memberIds = (memberIds ?? new List<Guid>()).Distinct().ToList();

            List<Guid> danhSachCu = ThanhVienDuAnManager.Instance.GetIdNhanVienByDuAnAndVaiTro(idDuAn, vaiTroThanhVien.IdVaiTroDuAn);

            List<Guid> canXoa = danhSachCu.Except(memberIds).ToList();
            List<Guid> canThem = memberIds.Except(danhSachCu).ToList();

            foreach (Guid id in canXoa)
                ThanhVienDuAnManager.Instance.DeleteOne(idDuAn, vaiTroThanhVien.IdVaiTroDuAn, id);

            foreach (Guid id in canThem)
                ThanhVienDuAnManager.Instance.AddOrUpdate(new TblThanhVienDuAn
                {
                    IdDuAn = idDuAn,
                    IdNhanVien = id,
                    IdVaiTroDuAn = vaiTroThanhVien.IdVaiTroDuAn
                });
        }
        //2. GetMemberIds: Dùng lấy đống idNhanVien đã có trong dự án để đánh tích cái checkbox, dùng để hiển thị trong edit
        public List<Guid> GetMemberIds(Guid idDuAn)
        {
            TblVaiTroDuAn vaiTroThanhVien = VaiTroDuAnManager.Instance.GetActiveByIdVaiTro("NGUOI_THAM_GIA");
            return ThanhVienDuAnManager.Instance.GetIdNhanVienByDuAnAndVaiTro(idDuAn, vaiTroThanhVien.IdVaiTroDuAn);
        }

        public DataTable GetProjectHistory(Guid idDuAn, Guid? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return _auditManager.GetProjectHistory(idDuAn, userId, fromDate, toDate);
        }

    }
}
