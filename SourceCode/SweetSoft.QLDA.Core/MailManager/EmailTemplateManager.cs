using SubSonic;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailTemplateManager
    {
        public EmailTemplateManager() { }

        #region Searchs
        public static DataTable SearchPaging(string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , COUNT(1) OVER() AS total_records
                        from TblEmailTemplate f
                        where f.IsDeleted = 0
                        and (@singleKeyWord = N'%%'
                        or f.Name LIKE @singleKeyWord
                        or f.Subject LIKE @singleKeyWord
                        or f.Body LIKE @singleKeyWord
                        or f.CCEmail LIKE @singleKeyWord
                        or f.BCCEmail LIKE @singleKeyWord)
					) as T
                ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow;";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }
        public static DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @name NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(parameters["Name"])}%';
                DECLARE @templateKey VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["TemplateKey"])}';
                DECLARE @isActivated BIT = {InlineQueryHelpers.SQLEncode(parameters["IsActivated"])};
                DECLARE @createdDateFrom VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["CreatedDateFrom"])}';
                DECLARE @createdDateTo VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["CreatedDateTo"])}';
                DECLARE @updatedDateFrom VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["UpdatedDateFrom"])}';
                DECLARE @updatedDateTo VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["UpdatedDateTo"])}';
                DECLARE @createdUser VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["CreatedUser"])}';
                DECLARE @updatedUser VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["UpdatedUser"])}';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , COUNT(1) OVER() AS total_records
                        from TblEmailTemplate f
                        where f.IsDeleted = 0
                        and (@templateKey = '' or f.TemplateKey = @templateKey)
                        and (@name = N'%%' or f.Name like @name)
                        and (@isActivated is null or f.IsActivated = @isActivated)
                        and (@createdDateFrom = '' or @createdDateTo = '' or f.CreatedDate BETWEEN @createdDateFrom AND @createdDateTo)
                        and (@updatedDateFrom = '' or @updatedDateTo = '' or f.UpdatedDate BETWEEN @updatedDateFrom AND @updatedDateTo)
                        and (@createdUser = '' or f.CreatedUser = @createdUser)
                        and (@updatedUser = '' or f.UpdatedUser = @updatedUser)
					) as T
                ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow;";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }
        #endregion
        public static TblEmailTemplate GetEmailTemplateById(Guid templateId)
        {
            Select select = new Select();
            select.From(TblEmailTemplate.Schema);
            select.Where(TblEmailTemplate.IdColumn).IsEqualTo(templateId);
            return select.ExecuteSingle<TblEmailTemplate>();
        }
        public static TblEmailTemplate GetEmailTemplateByTemplateKey(string templateKey, EmailFormatTypes emailType)
        {
            Select select = new Select();
            select.From(TblEmailTemplate.Schema);
            select.Where(TblEmailTemplate.TemplateKeyColumn).IsEqualTo(templateKey);
            select.And(TblEmailTemplate.EmailTypeColumn).IsEqualTo(emailType);
            select.And(TblEmailTemplate.IsDeletedColumn).IsEqualTo(false);
            return select.ExecuteSingle<TblEmailTemplate>();
        }
    }
}
