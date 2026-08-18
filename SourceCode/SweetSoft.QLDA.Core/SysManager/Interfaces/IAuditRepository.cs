using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.SysManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Interfaces
{
    public interface IAuditRepository
    {
        Task AddAuditLogAsync(AuditLog auditLog);
        Task<PagedResult<AuditLogDto>> SearchPagedAsync(AuditSearchRequest searchRequest);
        DataTable SearchPagedAsync(int year, string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord);
        DataTable SearchPagedAsync(int year, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord);
        Task<AuditLogDto> GetByIdAsync(Guid auditLogId);
        Task<List<AuditLogDto>> GetAuditTrailAsync(string tableName, Guid recordId, DateTime? fromDate, DateTime? toDate);
        Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate, DateTime? toDate, string tableName, string userId);
        Task<List<T>> ExecuteQueryAsync<T>(string query) where T : new();
        Task<int> ExecuteNonQueryAsync(string query);
    }
}
