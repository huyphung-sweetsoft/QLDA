using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.SysManager.Interfaces;
using SweetSoft.QLDA.Core.SysManager.Models;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Repository
{
    public class AuditRepository : IAuditRepository
    {
        private readonly DataProvider _dataProvider;

        public AuditRepository(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Create AuditLog table if not already there (by year)
        /// </summary>
        /// <param name="year"></param>
        private Task CreateAuditTableIfNotExistsAsync(int year)
        {
            try
            {
                string tableName = $"TblAuditLog_{year}";

                string createTableSql = $@"
            IF OBJECT_ID(N'dbo.{tableName}', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.{tableName} (
                    Id UNIQUEIDENTIFIER NOT NULL,
                    Title NVARCHAR(250),
                    CustomerId UNIQUEIDENTIFIER NULL,
                    ReferenceId UNIQUEIDENTIFIER NULL,
                    TableName NVARCHAR(100),
                    RecordId UNIQUEIDENTIFIER NULL,
                    ActionType NVARCHAR(10) NOT NULL CHECK (ActionType IN ('CREATE', 'UPDATE', 'DELETE', 'EXPORT', 'LOGIN', 'LOGOUT')),
                    Changes NVARCHAR(MAX),
                    IPAddress NVARCHAR(50),
                    UserAgent NVARCHAR(MAX),
                    UserId UNIQUEIDENTIFIER NULL,
                    ChangedBy NVARCHAR(150),
                    ChangedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT PK_{tableName} PRIMARY KEY (ChangedAt, Id),
                    CONSTRAINT UQ_{tableName} UNIQUE (ChangedAt, Id)
                );
            END";

                new InlineQuery(_dataProvider).Execute(createTableSql);
                return Task.CompletedTask;
            }
            catch (Exception exc)
            {
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                return Task.CompletedTask;
            }
        }

        public async Task AddAuditLogAsync(AuditLog auditLog)
        {
            string jsonString = "";
            try
            {
                int year = auditLog.ChangedAt.Year;
                await CreateAuditTableIfNotExistsAsync(year).ConfigureAwait(false);
                jsonString = auditLog.Changes == null ? "" : JsonConvert.SerializeObject(auditLog.Changes);

                string sql = $@"
INSERT INTO TblAuditLog_{year} (
    Id, Title, CustomerId, ReferenceId, TableName, RecordId, 
    ActionType, [Changes], IPAddress, UserAgent, UserId, ChangedBy, ChangedAt
) VALUES (
    @Id, @Title, @CustomerId, @ReferenceId, @TableName, @RecordId,
    @ActionType, @Changes, @IPAddress, @UserAgent, @UserId, @ChangedBy, @ChangedAt
)";

                var cmd = new QueryCommand(sql, _dataProvider.Name);

                // Thêm parameters với DbType.String để SubSonic xử lý đúng unicode
                cmd.AddParameter("@Id", UUIDv7.NewGuid().ToString(), DbType.String);
                cmd.AddParameter("@Title", auditLog.Title, DbType.String);
                cmd.AddParameter("@CustomerId", auditLog.CustomerId?.ToString(), DbType.String);
                cmd.AddParameter("@ReferenceId", auditLog.RefId?.ToString(), DbType.String);
                cmd.AddParameter("@TableName", auditLog.TableName, DbType.String);
                cmd.AddParameter("@RecordId", auditLog.RecordId?.ToString(), DbType.String);
                cmd.AddParameter("@ActionType", auditLog.ActionType, DbType.String);
                cmd.AddParameter("@Changes", jsonString, DbType.String);
                cmd.AddParameter("@IPAddress", auditLog.IPAddress, DbType.String);
                cmd.AddParameter("@UserAgent", auditLog.UserAgent, DbType.String);
                cmd.AddParameter("@UserId", auditLog.UserId?.ToString(), DbType.String);
                cmd.AddParameter("@ChangedBy", auditLog.ChangedBy, DbType.String);
                cmd.AddParameter("@ChangedAt", auditLog.ChangedAt, DbType.DateTime);

                DataService.ExecuteQuery(cmd);
            }
            catch (Exception exc)
            {
                SysLogger.LogDebug($"JSON string: {jsonString}");
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                throw;
            }
        }


        public Task<PagedResult<AuditLogDto>> SearchPagedAsync(AuditSearchRequest searchRequest)
        {
            try
            {
                int year = searchRequest.Year;
                string tableName = $"TblAuditLog_{year}";

                var whereConditions = new List<string>();
                var parameters = new Dictionary<string, object>();

                // Build where conditions based on search request
                if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
                {
                    whereConditions.Add(@"(
                    Title LIKE @SearchTerm OR
                    ReferenceId LIKE @SearchTerm OR
                    RecordId LIKE @SearchTerm OR
                    ActionType LIKE @SearchTerm OR
                    Changes LIKE @SearchTerm OR
                    IPAddress LIKE @SearchTerm OR
                    UserAgent LIKE @SearchTerm OR
                    ChangedBy LIKE @SearchTerm
                )");
                    parameters.Add("@SearchTerm", $"%{InlineQueryHelpers.SQLEncode(searchRequest.SearchTerm)}%");
                }

                if (!string.IsNullOrEmpty(searchRequest.IPAddress))
                {
                    whereConditions.Add("IPAddress LIKE @IPAddress");
                    parameters.Add("@IPAddress", $"%{InlineQueryHelpers.SQLEncode(searchRequest.IPAddress)}%");
                }

                if (!string.IsNullOrEmpty(searchRequest.ChangedBy))
                {
                    whereConditions.Add("ChangedBy LIKE @ChangedBy");
                    parameters.Add("@ChangedBy", $"%{InlineQueryHelpers.SQLEncode(searchRequest.ChangedBy)}%");
                }

                if (!string.IsNullOrEmpty(searchRequest.ActionType))
                {
                    whereConditions.Add("ActionType LIKE @ActionType");
                    parameters.Add("@ActionType", $"%{InlineQueryHelpers.SQLEncode(searchRequest.ActionType)}%");
                }

                if (searchRequest.FromDate.HasValue)
                {
                    whereConditions.Add("ChangedAt >= @FromDate");
                    parameters.Add("@FromDate", searchRequest.FromDate.Value);
                }

                if (searchRequest.ToDate.HasValue)
                {
                    whereConditions.Add("ChangedAt <= @ToDate");
                    parameters.Add("@ToDate", searchRequest.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(searchRequest.TableName))
                {
                    whereConditions.Add("TableName = @TableName");
                    parameters.Add("@TableName", searchRequest.TableName);
                }

                if (searchRequest.RecordId.HasValue)
                {
                    whereConditions.Add("RecordId = @RecordId");
                    parameters.Add("@RecordId", searchRequest.RecordId.Value);
                }

                string whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
                string orderBy = !string.IsNullOrEmpty(searchRequest.OrderBy) ? searchRequest.OrderBy : "ChangedAt DESC";

                int offset = (searchRequest.PageNumber - 1) * searchRequest.PageSize;

                string sql = $@"
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum,
                       *,
                       COUNT(1) OVER() AS TotalRecords
                FROM {tableName}
                {whereClause}
            ) T 
            WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize";

                parameters.Add("@Offset", offset);
                parameters.Add("@PageSize", searchRequest.PageSize);

                IDataReader iDataReader = new InlineQuery(_dataProvider).ExecuteReader(sql, parameters);
                if (iDataReader == null)
                    return null;
                DataTable dataTable = new DataTable();
                dataTable.Load(iDataReader);
                //-----------------------------------------------
                var auditLogs = new List<AuditLogDto>();
                int totalRecords = 0;

                foreach (DataRow row in dataTable.Rows)
                {
                    if (totalRecords == 0)
                        totalRecords = Convert.ToInt32(row["TotalRecords"]);

                    auditLogs.Add(new AuditLogDto
                    {
                        Id = (Guid)row["Id"],
                        Title = row["Title"]?.ToString(),
                        CustomerId = row["CustomerId"] as Guid?,
                        RefId = row["ReferenceId"] as Guid?,
                        TableName = row["TableName"]?.ToString(),
                        RecordId = (Guid)row["RecordId"],
                        ActionType = row["ActionType"].ToString(),
                        Changes = row["Changes"]?.ToString(),
                        IPAddress = row["IPAddress"]?.ToString(),
                        UserAgent = row["UserAgent"]?.ToString(),
                        UserId = row["UserId"] as Guid?,
                        ChangedBy = row["ChangedBy"]?.ToString(),
                        ChangedAt = (DateTime)row["ChangedAt"]
                    });
                }

                var result = new PagedResult<AuditLogDto>
                {
                    Items = auditLogs,
                    TotalRecords = totalRecords,
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / searchRequest.PageSize)
                };
                return Task.FromResult(result);
            }
            catch (Exception exc)
            {
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                throw;
            }
        }

        public DataTable SearchPagedAsync(int year, string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
        DECLARE @startRow INT = {pageNumber};
        DECLARE @endRow INT = {pageSize};
        DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
        select * from (
            select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum
                , M.*
                , COUNT(1) OVER() AS total_records
                from TblAuditLog_{year} M 
				where (@singleKeyWord = N'%%'
                    or M.Title LIKE @singleKeyWord
                    or M.ReferenceId LIKE @singleKeyWord
                    or M.RecordId LIKE @singleKeyWord
                    or M.ActionType LIKE @singleKeyWord
                    or M.Changes LIKE @singleKeyWord
                    or M.IPAddress LIKE @singleKeyWord
                    or M.UserAgent LIKE @singleKeyWord
                    or M.UserId LIKE @singleKeyWord
                    or M.ChangedBy LIKE @singleKeyWord)
        ) T WHERE RowNum >= @startRow AND RowNum <= @endRow";
            IDataReader iDataReader = new InlineQuery(_dataProvider).ExecuteReader(sql);
            if (iDataReader == null || iDataReader.IsClosed)
                return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            //------------------------------------------------------------
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }

        public DataTable SearchPagedAsync(int year, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
        DECLARE @startRow INT = {pageNumber};
        DECLARE @endRow INT = {pageSize};
        DECLARE @ipAddress NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters["IPAddress"])}%';
        DECLARE @userName NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters["ChangedBy"])}%';
        DECLARE @actionType NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters["ActionType"])}%';
		DECLARE	@createDateFrom VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["ChangedAtFrom"])}';
		DECLARE	@createDateTo VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters["ChangedAtTo"])}';
        select * from (
            select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum
                , M.*
                , COUNT(1) OVER() AS total_records
                from TblAuditLog_{year} M 
				where (@ipAddress = N'%%' or M.IpAddress LIKE @ipAddress) 
            and (@userName = N'%%' or M.ChangedBy LIKE @userName)
			and (@actionType = N'%%' or M.ActionType LIKE @actionType)
			and (@createDateFrom = '' or M.ChangedAt >= @createDateFrom)
			and (@createDateTo = '' or M.ChangedAt <= @createDateTo)
        ) T WHERE RowNum >= @startRow AND RowNum <= @endRow";
            IDataReader iDataReader = new InlineQuery(_dataProvider).ExecuteReader(sql);
            if (iDataReader == null || iDataReader.IsClosed)
                return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            //------------------------------------------------------------
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }

        public Task<AuditLogDto> GetByIdAsync(Guid auditLogId)
        {
            try
            {
                var currentYear = DateTime.UtcNow.Year;

                for (int year = currentYear; year >= currentYear - 5; year--)
                {
                    string tableName = $"TblAuditLog_{year}";

                    // Check if table exists
                    string checkTableSql = $@"
                SELECT COUNT(1) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '{tableName}'";

                    var tableExists = new InlineQuery(_dataProvider).ExecuteScalar<int>(checkTableSql);

                    if (tableExists > 0)
                    {
                        string sql = $"SELECT * FROM {tableName} WHERE Id = @Id";
                        var parameters = new Dictionary<string, object> { { "@Id", auditLogId } };

                        var auditLogList = new InlineQuery(_dataProvider)
                            .ExecuteTypedList<TblAuditTemp>(sql, parameters);

                        var entity = auditLogList?.FirstOrDefault();
                        if (entity == null)
                            continue;

                        var result = new AuditLogDto
                        {
                            Id = entity.Id,
                            Title = entity.Title,
                            CustomerId = entity.CustomerId,
                            RefId = entity.ReferenceId,
                            TableName = entity.TableName,
                            RecordId = entity.RecordId,
                            ActionType = entity.ActionType,
                            Changes = entity.Changes,
                            IPAddress = entity.IPAddress,
                            UserAgent = entity.UserAgent,
                            UserId = entity.UserId,
                            ChangedBy = entity.ChangedBy,
                            ChangedAt = entity.ChangedAt
                        };
                        return Task.FromResult(result);
                    }
                }

                return null;
            }
            catch (Exception exc)
            {
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                throw;
            }
        }


        public Task<List<AuditLogDto>> GetAuditTrailAsync(string tableName, Guid recordId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var auditLogs = new List<AuditLogDto>();
                var currentYear = DateTime.UtcNow.Year;
                var startYear = fromDate?.Year ?? currentYear - 2;
                var endYear = toDate?.Year ?? currentYear;

                for (int year = startYear; year <= endYear; year++)
                {
                    string auditTableName = $"TblAuditLog_{year}";

                    // Check if table exists
                    string checkTableSql = $@"
                SELECT COUNT(1) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '{auditTableName}'";

                    var tableExists = new InlineQuery(_dataProvider).ExecuteScalar<int>(checkTableSql);

                    if (tableExists > 0)
                    {
                        var whereConditions = new List<string> { "TableName = @TableName", "RecordId = @RecordId" };
                        var parameters = new Dictionary<string, object>
                    {
                        { "@TableName", tableName },
                        { "@RecordId", recordId }
                    };

                        if (fromDate.HasValue)
                        {
                            whereConditions.Add("ChangedAt >= @FromDate");
                            parameters.Add("@FromDate", fromDate.Value);
                        }

                        if (toDate.HasValue)
                        {
                            whereConditions.Add("ChangedAt <= @ToDate");
                            parameters.Add("@ToDate", toDate.Value);
                        }

                        string sql = $@"
                    SELECT * FROM {auditTableName}
                    WHERE {string.Join(" AND ", whereConditions)}
                    ORDER BY ChangedAt ASC";

                        var tblAuditTemps = new InlineQuery(_dataProvider).ExecuteTypedList<TblAuditTemp>(sql, parameters);
                        tblAuditTemps?.ForEach(entity =>
                        {
                            auditLogs.Add(new AuditLogDto
                            {
                                Id = entity.Id,
                                Title = entity.Title,
                                CustomerId = entity.CustomerId,
                                RefId = entity.ReferenceId,
                                TableName = entity.TableName,
                                RecordId = entity.RecordId,
                                ActionType = entity.ActionType,
                                Changes = entity.Changes,
                                IPAddress = entity.IPAddress,
                                UserAgent = entity.UserAgent,
                                UserId = entity.UserId,
                                ChangedBy = entity.ChangedBy,
                                ChangedAt = entity.ChangedAt
                            });
                        });
                    }
                }

                var results = auditLogs.OrderBy(x => x.ChangedAt).ToList();
                return Task.FromResult(results);
            }
            catch (Exception exc)
            {
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                throw;
            }
        }

        public Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate, DateTime? toDate, string tableName, string userId)
        {
            try
            {
                var statistics = new AuditStatistics();
                var currentYear = DateTime.UtcNow.Year;
                var startYear = fromDate?.Year ?? currentYear - 1;
                var endYear = toDate?.Year ?? currentYear;

                var whereConditions = new List<string>();
                var parameters = new Dictionary<string, object>();

                if (fromDate.HasValue)
                {
                    whereConditions.Add("ChangedAt >= @FromDate");
                    parameters.Add("@FromDate", fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    whereConditions.Add("ChangedAt <= @ToDate");
                    parameters.Add("@ToDate", toDate.Value);
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    whereConditions.Add("TableName = @TableName");
                    parameters.Add("@TableName", tableName);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    whereConditions.Add("UserId = @UserId");
                    parameters.Add("@UserId", Guid.Parse(userId));
                }

                string whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

                for (int year = startYear; year <= endYear; year++)
                {
                    string auditTableName = $"TblAuditLog_{year}";

                    // Check if table exists
                    string checkTableSql = $@"
                SELECT COUNT(1) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '{auditTableName}'";

                    var tableExists = new InlineQuery(_dataProvider).ExecuteScalar<int>(checkTableSql);

                    if (tableExists > 0)
                    {
                        // Get total count
                        string countSql = $"SELECT COUNT(1) FROM {auditTableName} {whereClause}";
                        var count = new InlineQuery(_dataProvider).ExecuteScalar<int>(countSql, parameters);
                        statistics.TotalRecords += count;

                        // Get action type breakdown
                        string actionTypesSql = $@"
                    SELECT ActionType, COUNT(1) as Count 
                    FROM {auditTableName} {whereClause}
                    GROUP BY ActionType";

                        IDataReader dataReader = new InlineQuery(_dataProvider).ExecuteReader(actionTypesSql, parameters);
                        if (dataReader == null)
                            return null;
                        DataTable actionTypesTable = new DataTable();
                        actionTypesTable.Load(dataReader);
                        if (actionTypesTable == null)
                            return null;
                        foreach (DataRow row in actionTypesTable.Rows)
                        {
                            string actionType = row["ActionType"].ToString();
                            int actionCount = Convert.ToInt32(row["Count"]);

                            if (statistics.ActionTypeCounts.ContainsKey(actionType))
                                statistics.ActionTypeCounts[actionType] += actionCount;
                            else
                                statistics.ActionTypeCounts[actionType] = actionCount;
                        }

                        // Get top users
                        string topUsersSql = $@"
                    SELECT TOP 10 ChangedBy, COUNT(1) as Count 
                    FROM {auditTableName} {whereClause}
                    AND ChangedBy IS NOT NULL
                    GROUP BY ChangedBy
                    ORDER BY COUNT(1) DESC";
                        dataReader = new InlineQuery(_dataProvider).ExecuteReader(topUsersSql, parameters);
                        if (dataReader == null)
                            return null;
                        var topUsersTable = new DataTable();
                        topUsersTable.Load(dataReader);
                        if (topUsersTable == null)
                            return null;
                        foreach (DataRow row in topUsersTable.Rows)
                        {
                            string user = row["ChangedBy"].ToString();
                            int userCount = Convert.ToInt32(row["Count"]);

                            if (statistics.TopUsers.ContainsKey(user))
                                statistics.TopUsers[user] += userCount;
                            else
                                statistics.TopUsers[user] = userCount;
                        }
                    }
                }

                // Sort top users and take top 10
                statistics.TopUsers = statistics.TopUsers
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .ToDictionary(x => x.Key, x => x.Value);

                return Task.FromResult(statistics);
            }
            catch (Exception exc)
            {
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                throw;
            }
        }

        public Task<List<T>> ExecuteQueryAsync<T>(string query) where T : new()
        {
            try
            {
                var result = new InlineQuery(_dataProvider).ExecuteTypedList<T>(query);
                return Task.FromResult(result);
            }
            catch (Exception exc)
            {
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                throw;
            }
        }


        public Task<int> ExecuteNonQueryAsync(string query)
        {
            try
            {
                var result = new InlineQuery(_dataProvider).ExecuteScalar<int>(query);
                return Task.FromResult(result);
            }
            catch (Exception exc)
            {
                SysLogger.LogError($"{exc.Message} - {exc.StackTrace}");
                throw;
            }
        }
    }
}
