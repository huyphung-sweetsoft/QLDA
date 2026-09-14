using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class ProjectReportManager : BaseManager
    {
        private static readonly Lazy<ProjectReportManager> _instance = new Lazy<ProjectReportManager>(() => new ProjectReportManager());
        public static ProjectReportManager Instance => _instance.Value;
        private readonly ProjectReportRepository _repository;
        private readonly AuditManager _auditManager;

        public ProjectReportManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new ProjectReportRepository(_auditManager);
        }
        public DataTable GetProjectInfo(Guid projectId)
        {
            return _repository.GetProjectInfo(projectId);
        }
        public string GetTaskDateFilter(DateTime? fromDate, DateTime? toDate)
        {
            return _repository.GetTaskDateFilter(fromDate, toDate);
        }
        public string GetIssueDateFilter(DateTime? fromDate, DateTime? toDate)
        {
            return _repository.GetIssueDateFilter(fromDate, toDate);
        }
        public int GetTotalTasks(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            return _repository.GetTotalTasks(projectId, fromDate, toDate);
        }
        public DataTable GetCompletedTasks(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            return _repository.GetCompletedTasks(projectId, fromDate, toDate);
        }
        public DataTable GetOverdueTasks(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            return _repository.GetOverdueTasks(projectId, fromDate, toDate);
        }
        public DataTable GetIssues(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            return _repository.GetIssues(projectId, fromDate, toDate);
        }
    }
}
