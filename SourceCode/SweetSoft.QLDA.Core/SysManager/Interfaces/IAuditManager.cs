using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.SysManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Interfaces
{
    public interface IAuditManager
    {
        Task LogActionAsync<T>(LogActions.Actions action, T entity, string tableName, Guid entityId, string user = "[System]");
        Task LogChangesAsync<T>(T oldEntity, T newEntity, string tableName, Guid entityId, string user = "[System]");
        Task<PagedResult<AuditLogDto>> SearchAuditLogsAsync(AuditSearchRequest searchRequest);
        Task<AuditLogDto> GetAuditLogByIdAsync(Guid auditLogId);
        Task<List<AuditLogDto>> GetAuditTrailAsync(string tableName, Guid recordId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, string tableName = null, string userId = null);
        Task ExecuteUpdateWithLogAsync<T>(string selectQuery, string updateQuery, string tableName, Guid recordId, string changedBy, Guid userId, Guid? refId = null) where T : new();
    }
}
