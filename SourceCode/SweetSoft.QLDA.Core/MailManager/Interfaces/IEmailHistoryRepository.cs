using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Models;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Interfaces
{
    public interface IEmailHistoryRepository
    {
        TblEmailHistory GetById(Guid id);
        Task<TblEmailHistory> GetByIdAsync(Guid id);
        Task<TblEmailHistory> GetUnsentEmailByRefAsync(Guid refId, EmailType refType, Guid? customerId = null);
        Task<bool> IsEmailSentAsync(Guid refId, EmailType refType);
        Task<List<TblEmailHistory>> GetByRefIdAsync(Guid refId, EmailType refType);
        Task<List<TblEmailHistory>> GetByCustomerIdAsync(Guid customerId, int? limit = null);
        Task<PagedResult<TblEmailHistory>> SearchPagedAsync(EmailHistorySearchRequest searchRequest);
        Task<EmailStatistics> GetEmailStatisticsAsync(Guid? customerId, Guid? refId, EmailType? refType, DateTime? fromDate, DateTime? toDate);
        Task<TblEmailHistory> InsertAsync(TblEmailHistory entity);
        Task<TblEmailHistory> UpdateAsync(TblEmailHistory entity);
        Task<bool> DeleteAsync(Guid id);
    }
}
