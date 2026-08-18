using SubSonic;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
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
    public class RoleRepository : BaseRepository<AspnetRole>
    {
        public RoleRepository(AuditManager auditManager) : base(auditManager) { }

        public override DataTable SearchPaging(string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
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
                        from aspnet_Roles f
                        where f.IsDeleted = 0 
                        and (@singleKeyWord = N'%%'
                        or RoleName LIKE @singleKeyWord
                        or LoweredRoleName LIKE @singleKeyWord
                        or Description LIKE @singleKeyWord
                        or CreatedBy LIKE @singleKeyWord
                        or UpdatedBy LIKE @singleKeyWord)
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
                DECLARE @roleName NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters[AspnetRole.Columns.RoleName])}%';
                DECLARE @isActivated BIT = {InlineQueryHelpers.SQLEncode(parameters[AspnetRole.Columns.IsActivated])};
                DECLARE @createdBy VARCHAR(150) = '{InlineQueryHelpers.SQLEncode(parameters[AspnetRole.Columns.CreatedBy])}';
                DECLARE @updatedBy VARCHAR(150) = '{InlineQueryHelpers.SQLEncode(parameters[AspnetRole.Columns.UpdatedBy])}';
                DECLARE @createdDateFrom VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["CreatedDateFrom"])}';
                DECLARE @createdDateTo VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["CreatedDateTo"])}';
                DECLARE @updatedDateFrom VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["UpdatedDateFrom"])}';
                DECLARE @updatedDateTo VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["UpdatedDateTo"])}';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , COUNT(1) OVER() AS total_records
                        from aspnet_Roles f 
                        where f.IsDeleted = 0 
                        and (@roleName = N'%%' or f.RoleName like @roleName)
                        and (@createdBy = '' or f.CreatedBy = @createdBy)
                        and (@updatedBy = '' or f.UpdatedBy = @updatedBy)
                        and (@isActivated is null or f.IsActivated = @isActivated)
                        and (@createdDateFrom = '' or @createdDateTo = '' or f.CreatedDate BETWEEN @createdDateFrom AND @createdDateTo)
                        and (@updatedDateFrom = '' or @updatedDateTo = '' or f.UpdatedDate BETWEEN @updatedDateFrom AND @updatedDateTo)
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
        public override AspnetRole Insert(AspnetRole item)
        {
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, Guid.Parse(item.GetColumnValue("RoleId").ToString())).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for AspnetRole");
                }
            });
            return item;
        }

        public override AspnetRole Update(AspnetRole itemNew)
        {
            var id = Guid.Parse(itemNew.GetColumnValue("RoleId").ToString());
            AspnetRole itemOld = GetById(id);
            itemNew.Save();
            string updatedBy = string.Empty;
            try
            {
                updatedBy = itemNew.GetColumnValue("UpdatedBy")?.ToString();
            }
            catch { }
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, itemNew, _tableName, id, updatedBy).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for AspnetRole");
                }
            });
            return itemNew;
        }

        public override bool Delete(AspnetRole item)
        {
            if (item == null)
                return false;
            //---------------------------------------
            new Delete().From(AspnetUsersInRole.Schema).Where(AspnetUsersInRole.RoleIdColumn).IsEqualTo(item.RoleId).Execute();
            //---------------------------------------
            new Delete().From(AspnetAssignRole.Schema).Where(AspnetAssignRole.RoleIdColumn).IsEqualTo(item.RoleId).Execute();
            //---------------------------------------
            ActiveRecord<AspnetRole>.Delete("RoleId", item.GetColumnValue("RoleId"));
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.DELETE, item, _tableName, Guid.Parse(item.GetColumnValue("RoleId").ToString())).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log DELETE action for AspnetRole");
                }
            });
            return true;
        }
        public override AspnetRole GetById(Guid id)
        {
            return new Select()
                .From(AspnetRole.Schema)
                .Where(AspnetRole.RoleIdColumn).IsEqualTo(id)
                .And(AspnetRole.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetRole>();
        }
        public AspnetRole GetByRoleName(string roleName)
        {
            return new Select()
                .From(AspnetRole.Schema)
                .Where(AspnetRole.RoleNameColumn).IsEqualTo(roleName)
                .And(AspnetRole.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetRole>();
        }
        public List<AspnetRole> GetAllAspnetRoles()
        {
            Select select = new Select();
            select.From(AspnetRole.Schema);
            select.And(AspnetRole.IsDeletedColumn).IsEqualTo(false);
            return select.ExecuteTypedList<AspnetRole>();
        }
        public AspnetRole GetRoleById(object RoleId)
        {
            return new Select().From(AspnetRole.Schema)
                    .Where(AspnetRole.RoleIdColumn).IsEqualTo(RoleId)
                    .And(AspnetRole.IsDeletedColumn).IsEqualTo(false)
                    .ExecuteSingle<AspnetRole>();
        }
        public bool IsUserInRole(object userId, object roleId)
        {
            return new Select().From(AspnetUsersInRole.Schema)
                      .Where(AspnetUsersInRole.UserIdColumn).IsEqualTo(userId)
                      .And(AspnetUsersInRole.RoleIdColumn).IsEqualTo(roleId)
                      .GetRecordCount() > 0;
        }
        public bool IsAllowAction(Guid userId, ModuleKeys functionCode, ActionKeys actionCode)
        {
            if (UserManager.Instance.IsAdministrator(userId))
                return true;
            string sql = string.Format(@"select COUNT(*) from aspnet_UsersInRoles b 
				    left join aspnet_Roles c on b.RoleId = c.RoleId 
				    left join aspnet_AssignRoles d on d.RoleId = c.RoleId and d.IsAllowed = 1
				    left join aspnet_Permission e on d.PermissionKey = e.PermissionKey
				    left join aspnet_Functions f on e.FunctionId = f.Id
                where b.UserId= '{0}' and f.FunctionCode = '{1}' and e.PermissionKey LIKE N'%{2}';", userId, functionCode, actionCode);
            return new InlineQuery().ExecuteScalar<int>(sql) > 0;
        }
        public bool IsAssignPermission(object userId)
        {
            Select select = new Select();
            select.From(AspnetUsersInRole.Schema);
            select.Where(AspnetUsersInRole.UserIdColumn).IsEqualTo(userId);
            return select.GetRecordCount() > 0;
        }
        public List<AspnetFunction> GetAllAspnetFunctions()
        {
            Select select = new Select();
            select.From(AspnetFunction.Schema);
            select.Where(AspnetFunction.IsActivatedColumn).IsEqualTo(1);
            return select.ExecuteTypedList<AspnetFunction>();
        }
        public List<string> GetFunctionCodesByUserId(Guid userId)
        {
           List<AspnetFunction> functionCodes = GetAspnetFunctionByUserId(userId);
            if (functionCodes == null || functionCodes.Count <= 0)
                return null;
            return functionCodes.Select(t => t.FunctionCode).ToList();
        }
        public List<AspnetFunction> GetAspnetFunctionByUserId(Guid userId)
        {
            string sql = string.Format(@"with t as (
	                select f.* from aspnet_UsersInRoles b 
				    left join aspnet_Roles c on b.RoleId = c.RoleId 
				    left join aspnet_AssignRoles d on d.RoleId = c.RoleId and d.IsAllowed = 1
				    left join aspnet_Permission e on d.PermissionKey = e.PermissionKey
				    left join aspnet_Functions f on e.FunctionId = f.Id
	                where b.UserId = '{0}' 
                ) 
                select * from aspnet_Functions f where f.FunctionCode IN (select t.ParentCode from t)
                UNION ALL
                select distinct f.* from aspnet_UsersInRoles b
				    left join aspnet_Roles c on b.RoleId = c.RoleId 
				    left join aspnet_AssignRoles d on d.RoleId = c.RoleId and d.IsAllowed = 1
				    left join aspnet_Permission e on d.PermissionKey = e.PermissionKey
				    left join aspnet_Functions f on e.FunctionId = f.Id 
	                where b.UserId = '{0}' order by DisplayOrder asc;", userId);
            return new InlineQuery().ExecuteTypedList<AspnetFunction>(sql);
        }
        public List<AspnetFunction> GetAspnetFunctionActiveByUserId(Guid userId)
        {
            string sql = string.Format(@"with t as (
	                select f.* from aspnet_UsersInRoles b 
				    left join aspnet_Roles c on b.RoleId = c.RoleId 
				    left join aspnet_AssignRoles d on d.RoleId = c.RoleId and d.IsAllowed = 1
				    left join aspnet_Permission e on d.PermissionKey = e.PermissionKey
				    left join aspnet_Functions f on e.FunctionId = f.Id and f.IsActivated = 1
	                where b.UserId = '{0}' and f.IsActivated = 1
                ) 
                select * from aspnet_Functions f where f.FunctionCode IN (select t.ParentCode from t)
                UNION ALL
                select distinct f.* from aspnet_UsersInRoles b
				    left join aspnet_Roles c on b.RoleId = c.RoleId 
				    left join aspnet_AssignRoles d on d.RoleId = c.RoleId and d.IsAllowed = 1
				    left join aspnet_Permission e on d.PermissionKey = e.PermissionKey
				    left join aspnet_Functions f on e.FunctionId = f.Id and f.IsActivated = 1
	                where b.UserId = '{0}' and f.IsActivated = 1 order by DisplayOrder asc;", userId);
            return new InlineQuery().ExecuteTypedList<AspnetFunction>(sql);
        }
        public List<AspnetAssignRole> GetAspnetAssignRoles(Guid roleId)
        {
            Select select = new Select();
            select.From(AspnetAssignRole.Schema);
            select.Where(AspnetAssignRole.RoleIdColumn).IsEqualTo(roleId);
            return select.ExecuteTypedList<AspnetAssignRole>();
        }
        public bool IsFunctionCodeExisted(string functionCode)
        {
            Select select = new Select();
            select.From(AspnetFunction.Schema);
            select.Where(AspnetFunction.FunctionCodeColumn).IsEqualTo(functionCode);
            select.And(AspnetFunction.IsActivatedColumn).IsEqualTo(1);
            return select.GetRecordCount() > 0;
        }
        public AspnetAssignRole GetAssignRole(Guid roleId, string permissionKey)
        {
            Select select = new Select();
            select.From(AspnetAssignRole.Schema);
            select.Where(AspnetAssignRole.RoleIdColumn).IsEqualTo(roleId);
            select.And(AspnetAssignRole.PermissionKeyColumn).IsEqualTo(permissionKey);
            return select.ExecuteSingle<AspnetAssignRole>();
        }
        public AspnetRole GetRoleByUserId(Guid userId)
        {
            Select select = new Select();
            select.From(AspnetRole.Schema);
            select.InnerJoin(AspnetUsersInRole.RoleIdColumn, AspnetRole.RoleIdColumn);
            select.Where(AspnetUsersInRole.UserIdColumn).IsEqualTo(userId);
            select.And(AspnetRole.IsDeletedColumn).IsEqualTo(false);
            return select.ExecuteSingle<AspnetRole>();
        }
        public void RemoveAllRoleOfUser(Guid userId)
        {
            new Delete().From(AspnetUsersInRole.Schema)
                .Where(AspnetUsersInRole.UserIdColumn).IsEqualTo(userId)
                .Execute();
        }
        public List<AspnetFunction> GetAspnetFunctionWithPermissionKey()
        {
            string sql = @"SELECT 
                    f.*,
                    p.PermissionKey
                FROM aspnet_Functions f
                LEFT JOIN aspnet_Permission p ON f.Id = p.FunctionId
                ORDER BY f.DisplayOrder";
            return new InlineQuery().ExecuteTypedList<AspnetFunction>(sql);
        }
        public List<AspnetPermission> GetAspnetPermissions()
        {
            Select select=new Select();
            select.From(AspnetPermission.Schema);
            return select.ExecuteTypedList<AspnetPermission>();
        }
    }
}
