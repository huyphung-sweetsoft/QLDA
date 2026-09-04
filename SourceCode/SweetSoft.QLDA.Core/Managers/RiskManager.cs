using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
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
    }
}
