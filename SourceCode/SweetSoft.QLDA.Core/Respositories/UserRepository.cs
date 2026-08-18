using SubSonic;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class UserRepository : BaseRepository<AspnetUser>
    {
        public UserRepository(AuditManager auditManager) : base(auditManager) { }

        public DataTable SearchPaging(string searchTerm, Guid roleId, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @roleId VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(roleId)}';
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , ms.Email
                        , RoleName = (
                            select top 1 RoleName from aspnet_Roles r
                            inner join aspnet_UsersInRoles mp on mp.RoleId = r.RoleId 
                            where mp.UserId = f.UserId
                        )
                        , COUNT(1) OVER() AS total_records
                        from aspnet_Users f
                        inner join aspnet_Membership ms on ms.UserId = f.UserId
                        left join aspnet_UsersInRoles r on r.UserId = f.UserId 
                        where f.IsDeleted = 0 
                        and (@roleId = '{Guid.Empty}' or r.RoleId = @roleId)
                        and (@singleKeyWord = N'%%'
                        or Username LIKE @singleKeyWord
                        or DisplayName LIKE @singleKeyWord
                        or Email LIKE @singleKeyWord
                        or MobileAlias LIKE @singleKeyWord)
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
        public DataTable SearchPaging(string searchTerm, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @isActivated BIT = {InlineQueryHelpers.SQLEncode(parameters[AspnetUser.Columns.IsActivated])};
                DECLARE @roleId VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters[AspnetRole.Columns.RoleId])}';
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , ms.Email
                        , RoleName = (
                            select top 1 RoleName from aspnet_Roles r
                            inner join aspnet_UsersInRoles mp on mp.RoleId = r.RoleId 
                            where mp.UserId = f.UserId
                        )
                        , COUNT(1) OVER() AS total_records
                        from aspnet_Users f
                        inner join aspnet_Membership ms on ms.UserId = f.UserId
                        left join aspnet_UsersInRoles r on r.UserId = f.UserId 
                        where f.IsDeleted = 0 
                        and (@isActivated is null or f.IsActivated = @isActivated)
                        and (@roleId = '{Guid.Empty}' or r.RoleId = @roleId)
                        and (@singleKeyWord = N'%%'
                        or Username LIKE @singleKeyWord
                        or DisplayName LIKE @singleKeyWord
                        or Email LIKE @singleKeyWord
                        or MobileAlias LIKE @singleKeyWord)
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
        public override DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @userName NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters[AspnetUser.Columns.UserName])}%';
                DECLARE @displayName NVARCHAR(250) = N'%{InlineQueryHelpers.SQLEncode(parameters[AspnetUser.Columns.DisplayName])}%';
                DECLARE @email NVARCHAR(250) = N'%{InlineQueryHelpers.SQLEncode(parameters[AspnetMembership.Columns.Email])}%';
                DECLARE @phoneNumber NVARCHAR(50) = N'%{InlineQueryHelpers.SQLEncode(parameters[AspnetUser.Columns.MobileAlias])}%';
                DECLARE @isActivated BIT = {InlineQueryHelpers.SQLEncode(parameters[AspnetUser.Columns.IsActivated])};
                DECLARE @roleId VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters[AspnetRole.Columns.RoleId])}';
                DECLARE @lastActivityDateFrom VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["LastActivityDateFrom"])}';
                DECLARE @lastActivityDateTo VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["LastActivityDateTo"])}';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , ms.Email
                        , RoleName = (
                            select top 1 RoleName from aspnet_Roles r
                            inner join aspnet_UsersInRoles mp on mp.RoleId = r.RoleId 
                            where mp.UserId = f.UserId
                        )
                        , COUNT(1) OVER() AS total_records
                        from aspnet_Users f 
                        inner join aspnet_Membership ms on ms.UserId = f.UserId
                        left join aspnet_UsersInRoles r on r.UserId = f.UserId 
                        where f.IsDeleted = 0 
                        and (@userName = N'%%' or f.UserName like @userName)
                        and (@displayName = N'%%' or f.DisplayName like @displayName)
                        and (@email = N'%%' or ms.Email like @email)
                        and (@phoneNumber = N'%%' or f.MobileAlias like @phoneNumber)
                        and (@isActivated is null or f.IsActivated = @isActivated)
                        and (@roleId = '{Guid.Empty}' or r.RoleId = @roleId)
                        and (@lastActivityDateFrom = '' or @lastActivityDateTo = '' or f.LastActivityDate BETWEEN @lastActivityDateFrom AND @lastActivityDateTo)
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
        public override AspnetUser Update(AspnetUser user)
        {
            var id = Guid.Parse(user.GetColumnValue("UserId").ToString());
            AspnetUser itemOld = GetById(id);
            user.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, user, _tableName, id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for AspnetUser");
                }
            });
            return user;
        }
        public override AspnetUser GetById(Guid id)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn).IsEqualTo(id)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetUser>();
        }
        public override bool Delete(AspnetUser item)
        {
            if(item == null) return false;
            return new AspnetUserController().Delete(item.UserId);
        }
        public AspnetUser GetByUserName(string userName)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserNameColumn).IsEqualTo(userName)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetUser>();
        }
        public AspnetUser GetByEmail(string email)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .InnerJoin(AspnetMembership.UserIdColumn, AspnetUser.UserIdColumn)
                .Where(AspnetMembership.EmailColumn).IsEqualTo(email)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetUser>();
        }
        public string GetDisplayNameById(Guid ID)
        {
            return new Select(AspnetUser.DisplayNameColumn)
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn).IsEqualTo(ID)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteScalar<string>();
        }
        public string GetDisplayNameByUserName(string username)
        {
            return new Select(AspnetUser.DisplayNameColumn)
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserNameColumn).IsEqualTo(username)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteScalar<string>();
        }
        public bool ValidateUser(string username, string password)
        {
            try
            {
                Select select = new Select();
                select.From(AspnetUser.Schema);
                select.InnerJoin(AspnetMembership.UserIdColumn, AspnetUser.UserIdColumn);
                select.Where(AspnetUser.UserNameColumn).IsEqualTo(username);
                select.And(AspnetMembership.PasswordColumn).IsEqualTo(SecurityUtilities.ComputeMd5Hash(password));
                select.And(AspnetUser.IsActivatedColumn).IsEqualTo(1);
                select.And(AspnetUser.IsDeletedColumn).IsEqualTo(0);
                return select.GetRecordCount() > 0;
            }
            catch
            {
                return false;
            }
        }
        public bool IsEmailExist(Guid ID, string email)
        {
            Select select = new Select();
            select.From(AspnetMembership.Schema);
            select.Where(AspnetMembership.EmailColumn).IsEqualTo(email);
            select.And(AspnetMembership.UserIdColumn).IsNotEqualTo(ID);
            return select.GetRecordCount() > 0;
        }
        public bool IsUserNameExist(Guid ID, string userName)
        {
            Select select = new Select();
            select.From(AspnetUser.Schema);
            select.Where(AspnetUser.UserNameColumn).IsEqualTo(userName);
            select.And(AspnetUser.UserIdColumn).IsNotEqualTo(ID);
            return select.GetRecordCount() > 0;
        }
        public List<AspnetUser> GetAllAspnetUsers()
        {
            Select select = new Select();
            select.From(AspnetUser.Schema);
            return select.ExecuteTypedList<AspnetUser>();
        }
    }
}
