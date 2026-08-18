using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Interfaces
{
    public interface IEmailHistoryService
    {
        TblEmailHistory GetEmailHistoryById(Guid historyId);
        Task<EmailHistory> CreateEmailHistoryAsync(EmailRequest request);
        Task<EmailHistory> GetEmailHistoryByIdAsync(Guid historyId);
        Task LogEmailErrorAsync(Guid? refId, EmailType refType, Guid customerId, string email, string errorMessage);
        Task UpdateEmailHistoryStatusAsync(Guid historyId, EmailStatus status, string message = null);
        Task SaveAsync(EmailHistory emailHistory);
    }
}
