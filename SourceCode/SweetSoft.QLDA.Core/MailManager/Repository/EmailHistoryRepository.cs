using SubSonic;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Models;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Repository
{
    public class EmailHistoryRepository : IEmailHistoryRepository
    {
        public Task<bool> DeleteAsync(Guid id)
        {
            string sql = $"DELETE TblEmailHistory WHERE Id = '{id}'";
            new InlineQuery().Execute(sql);
            return Task.FromResult(true);
        }

        public Task<List<TblEmailHistory>> GetByCustomerIdAsync(Guid customerId, int? limit = null)
        {
            Select select = new Select();
            if (limit != null)
                select.Top(limit.ToString());
            select.From(TblEmailHistory.Schema);
            select.Where(TblEmailHistory.CustomerIdColumn).IsEqualTo(customerId);
            var results = select.ExecuteTypedList<TblEmailHistory>();
            return Task.FromResult(results);    
        }
        public TblEmailHistory GetById(Guid id)
        {
            Select select = new Select();
            select.From(TblEmailHistory.Schema);
            select.Where(TblEmailHistory.IdColumn).IsEqualTo(id);
            return select.ExecuteSingle<TblEmailHistory>();
        }
        public Task<TblEmailHistory> GetByIdAsync(Guid id)
        {
            Select select = new Select();
            select.From(TblEmailHistory.Schema);
            select.Where(TblEmailHistory.IdColumn).IsEqualTo(id);
            var result = select.ExecuteSingle<TblEmailHistory>();
            return Task.FromResult(result);
        }

        public Task<List<TblEmailHistory>> GetByRefIdAsync(Guid refId, EmailType refType)
        {
            Select select = new Select();
            select.From(TblEmailHistory.Schema);
            select.Where(TblEmailHistory.RefIdColumn).IsEqualTo(refId);
            select.And(TblEmailHistory.RefTypeColumn).IsEqualTo(refType);
            var results = select.ExecuteTypedList<TblEmailHistory>();
            return Task.FromResult(results);
        }

        public Task<EmailStatistics> GetEmailStatisticsAsync(Guid? customerId, Guid? refId, EmailType? refType, DateTime? fromDate, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        public Task<TblEmailHistory> GetUnsentEmailByRefAsync(Guid refId, EmailType refType, Guid? customerId = null)
        {
            Select select = new Select();
            select.From(TblEmailHistory.Schema);
            select.Where(TblEmailHistory.RefIdColumn).IsEqualTo(refId);
            select.And(TblEmailHistory.RefTypeColumn).IsEqualTo(refType);
            if (customerId.HasValue)
                select.And(TblEmailHistory.CustomerIdColumn).IsEqualTo(customerId);
            select.And(TblEmailHistory.IsSentColumn).IsEqualTo(false);
            var result = select.ExecuteSingle<TblEmailHistory>();
            return Task.FromResult(result);
        }

        public Task<TblEmailHistory> InsertAsync(TblEmailHistory entity)
        {
            entity.Save();
            return Task.FromResult(entity);
        }

        public Task<bool> IsEmailSentAsync(Guid refId, EmailType refType)
        {
            Select select = new Select(TblEmailHistory.IsSentColumn);
            select.Top("1");
            select.From(TblEmailHistory.Schema);
            select.Where(TblEmailHistory.RefIdColumn).IsEqualTo(refId);
            select.And(TblEmailHistory.RefTypeColumn).IsEqualTo(refType);
            var result = select.ExecuteScalar<bool>();
            return Task.FromResult((bool)result);
        }

        public Task<PagedResult<TblEmailHistory>> SearchPagedAsync(EmailHistorySearchRequest searchRequest)
        {
            throw new NotImplementedException();
        }

        public Task<TblEmailHistory> UpdateAsync(TblEmailHistory entity)
        {
            entity.Save();
            return Task.FromResult(entity);
        }
    }
}
