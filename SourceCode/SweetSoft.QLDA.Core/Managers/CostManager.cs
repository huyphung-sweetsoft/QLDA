using SweetSoft.QLDA.Core.EnumHelper.Defines;
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
    public class CostManager : BaseManager
    {
        private static readonly Lazy<CostManager> _instance = new Lazy<CostManager>(() => new CostManager());
        public static CostManager Instance => _instance.Value;
        private readonly CostRepository _repository;
        private readonly AuditManager _auditManager;

        public CostManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new CostRepository(_auditManager);
        }
        public DataTable SearchCost(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            return _repository.SearchCost(projectId, searchTerm, parameters, orderBy, startRow, endRow, out totalRecord);
        }
        public string GenerateMaChiPhi(Guid projectId)
        {
            return _repository.GenerateMaChiPhi(projectId);
        }
        public string GetValueForTrangThaiChiPhi(TrangThaiChiPhi source)
        {
            switch (source)
            {
                case TrangThaiChiPhi.NotApproved:
                    return "NOT_APPROVED";
                case TrangThaiChiPhi.Approved:
                    return "APPROVED";
                case TrangThaiChiPhi.Rejected:
                    return "REJECTED";
                default:
                    return source.ToString();
            }
        }
        public void DeleteCost(TblChiPhi cost)
        {
            _repository.DeleteCost(cost);
        }
        public TblChiPhi CreateOrUpdate(TblChiPhi dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenKhoanChi, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenKhoanChi));
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty || dto.IdDuAn == null, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn));

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            bool isInsert = (dto.IdChiPhi == Guid.Empty);
            TblChiPhi result = null;

            dto.SoTien = (dto.DonGia ?? 0) * (dto.SoLuong ?? 0);

            if (isInsert)
            {
                dto.IdChiPhi = Guid.NewGuid();
                dto.DaXoa = false;
                dto.NgayTao = DateTime.Now;
                dto.IdNhanVienDeNghi = currentUserId;

                dto.MaChiPhi = GenerateMaChiPhi(dto.IdDuAn);

                if (dto.TrangThai == null) dto.TrangThai = 0;

                dto.Save();
                result = dto;
            }
            else
            {
                TblChiPhi existingCost = TblChiPhi.FetchByID(dto.IdChiPhi);
                BusinessValidator.ThrowIfNull(existingCost, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdChiPhi), ErrorCodes.NotFound);

                existingCost.TenKhoanChi = dto.TenKhoanChi;
                existingCost.MoTaChiTiet = dto.MoTaChiTiet;
                existingCost.DonGia = dto.DonGia;
                existingCost.SoLuong = dto.SoLuong;
                existingCost.SoTien = dto.SoTien;
                existingCost.TrangThai = dto.TrangThai;

                existingCost.Save();
                result = existingCost;
            }

            return result;
        }
    }
}
