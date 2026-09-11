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
    public class RiskManager: BaseManager
    {
        private static readonly Lazy<RiskManager> _instance = new Lazy<RiskManager>(() => new RiskManager());
        public static RiskManager Instance => _instance.Value;
        private readonly RiskRepository _repository;
        private readonly AuditManager _auditManager;
        public RiskManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new RiskRepository(_auditManager);
        }

        public DataTable GetRiskById(Guid projectId, bool deleted = false)
        {
            return _repository.GetRiskById(projectId, deleted);
        }

        public DataTable SearchRisk(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            return _repository.SearchRisk(projectId, searchTerm, parameters, orderBy, startRow,endRow, out totalRecord);
        }

        public DataTable GetAllNhanVienDuAnById(Guid projectId)
        {
            return _repository.GetAllNhanVienDuAnById(projectId);
        }

        public string GetValueForMucDoAnhHuong(MucDoAnhHuonEnum impact)
        {
            switch (impact)
            {
                case MucDoAnhHuonEnum.VeryLow: return "VERY_LOW";
                case MucDoAnhHuonEnum.Low: return "LOW";
                case MucDoAnhHuonEnum.Medium: return "MEDIUM";
                case MucDoAnhHuonEnum.High: return "HIGH";
                case MucDoAnhHuonEnum.VeryHigh: return "VERY_HIGH";
                default: return "—";
            }
        }
        public TblRuiRoDuAn CreateOrUpdate(TblRuiRoDuAn dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenRuiRo, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenRuiRo));
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn));

            string currentUser = SweetContext.Current != null ? SweetContext.Current.UserName : "System";
            bool isInsert = (dto.IdRuiRoDuAn == Guid.Empty);

            int xacSuat = dto.XacSuatXayRa ?? 0;
            int mucDo = dto.MucDoAnhHuong ?? 0;
            dto.DiemRuiRo = (float)(((decimal)xacSuat / 100m) * mucDo);

            TblRuiRoDuAn result = null;

            if (isInsert)
            {
                dto.IdRuiRoDuAn = Guid.NewGuid();
                dto.DaXoa = false;

                dto.NgayTao = DateTime.Now;
                dto.NguoiTao = currentUser;
                dto.NgayCapNhat = DateTime.Now;
                dto.NguoiCapNhat = currentUser;

                dto.Save(); 
                result = dto;
            }
            else
            {
                TblRuiRoDuAn existingRisk = TblRuiRoDuAn.FetchByID(dto.IdRuiRoDuAn);
                BusinessValidator.ThrowIfNull(existingRisk, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdRuiRoDuAn), ErrorCodes.NotFound);

                existingRisk.TenRuiRo = dto.TenRuiRo;
                existingRisk.IdNhanVienXuLy = dto.IdNhanVienXuLy;
                existingRisk.XacSuatXayRa = dto.XacSuatXayRa;
                existingRisk.MucDoAnhHuong = dto.MucDoAnhHuong;
                existingRisk.DiemRuiRo = dto.DiemRuiRo;
                existingRisk.KeHoachPhongNgua = dto.KeHoachPhongNgua;
                existingRisk.KeHoachUngPho = dto.KeHoachUngPho;

                existingRisk.NgayCapNhat = DateTime.Now;
                existingRisk.NguoiCapNhat = currentUser;

                existingRisk.Save();
                result = existingRisk;
            }

            return result;
        }
        public void DeleteRisk(TblRuiRoDuAn risk)
        {
            _repository.DeleteRisk(risk);
        }
    }
}
