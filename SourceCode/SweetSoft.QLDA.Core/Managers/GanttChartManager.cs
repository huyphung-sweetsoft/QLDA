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
    public class GanttChartManager : BaseManager
    {
        private static readonly Lazy<GanttChartManager> _instance = new Lazy<GanttChartManager>(() => new GanttChartManager());
        public static GanttChartManager Instance => _instance.Value;
        private readonly GanttChartRepository _repository;
        private readonly AuditManager _auditManager;

        public GanttChartManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new GanttChartRepository(_auditManager);
        }
        public DataTable GetGanttTasks(Guid projectId)
        {
            return _repository.GetGanttTasks(projectId);
        }
        public DataTable GetOverdueTasks(Guid projectId, string taskCode)
        {
            return _repository.GetOverdueTasks(projectId, taskCode);
        }
        public DataTable GetTaskIssues(string taskId)
        {
            return _repository.GetTaskIssues(taskId);
        }
    }
}
