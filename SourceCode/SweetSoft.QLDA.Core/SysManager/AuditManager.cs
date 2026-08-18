
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing;
using SubSonic;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager.Interfaces;
using SweetSoft.QLDA.Core.SysManager.Models;
using SweetSoft.QLDA.Core.SysManager.Repository;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager
{
    public class LogActions
    {
        public enum Actions
        {
            CREATE,
            UPDATE,
            DELETE,
            LOGIN,
            LOGOUT,
            EXPORT
        }
        public static string GetFullTag(string key)
        {
            return string.Format("<span class='{1}'>{0}</span>", GetText(key), GetClass(key));
        }
        public static string GetClass(string key)
        {
            if (Enum.TryParse(key, out Actions action))
            {
                switch (action)
                {
                    case Actions.CREATE:
                    case Actions.LOGIN:
                    case Actions.EXPORT:
                        //case ExportExcel:
                        //case ExportPdf:
                        return "badge bg-info";
                    //case Actions.CREATE:
                    case Actions.UPDATE:
                    case Actions.LOGOUT:
                        //case ResetPasword:
                        return "badge bg-warning";
                    //case UnLock:
                    //    return "badge bg-primary";
                    case Actions.DELETE:
                        return "badge bg-danger";
                    default:
                        return "badge badge-soft-dark";
                }
            }
            return "badge badge-soft-dark";
        }
        public static string GetText(string key)
        {
            if (Enum.TryParse(key, out Actions action))
            {
                switch (action)
                {
                    case Actions.CREATE:
                        return $"[{UITextsReader.GetBackEndResourceText(BackEndResourceKeys.ADD_NEW)}]";
                    case Actions.UPDATE:
                        return $"[{UITextsReader.GetBackEndResourceText(BackEndResourceKeys.UPDATE)}]";
                    case Actions.DELETE:
                        return $"[{UITextsReader.GetBackEndResourceText(BackEndResourceKeys.DELETE)}]";
                    case Actions.LOGIN:
                        return $"[{UITextsReader.GetBackEndResourceText(BackEndResourceKeys.LOGIN)}]";
                    case Actions.LOGOUT:
                        return $"[{UITextsReader.GetBackEndResourceText(BackEndResourceKeys.LOGOUT)}]";
                    case Actions.EXPORT:
                        return $"[{UITextsReader.GetBackEndResourceText(BackEndResourceKeys.EXPORT_EXCEL)}]";
                    default:
                        return $"[{UITextsReader.GetBackEndResourceText(key.ToUpper())}]";
                }
            }
            return key;
        }
    }
    public class AuditManager : IAuditManager
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ClientInfo _clientInfo;
        private readonly AuditConfiguration _configuration;

        private static readonly HashSet<string> ExcludedProperties = new HashSet<string>
    {
        "IsNew", "Errors", "IsDirty", "IsLoaded", "TableName",
        "DirtyColumns", "ProviderName", "NullExceptionMessage",
        "InvalidTypeExceptionMessage", "LengthExceptionMessage", "ValidateWhenSaving"
    };

        private static readonly HashSet<string> RefIdProperties = new HashSet<string>
    {
        "RefId", "OrderId", "PhysicalGoldConversionId"
    };

        public AuditManager(ClientInfo clientInfo,
                           IAuditRepository auditRepository = null,
                           AuditConfiguration configuration = null)
        {
            _clientInfo = clientInfo ?? new ClientInfo();
            _auditRepository = auditRepository ?? new AuditRepository(SubsonicHelpers.SysProvider);
            _configuration = configuration ?? new AuditConfiguration();
        }

        #region IAuditManager Implementation

        public async Task LogActionAsync<T>(LogActions.Actions action, T entity, string tableName, Guid entityId, string user = "[System]")
        {
            if (!_configuration.IsAuditEnabled || entity == null)
                return;

            try
            {
                var auditLog = await CreateAuditLogAsync(action, entity, tableName, entityId, user).ConfigureAwait(false);
                await _auditRepository.AddAuditLogAsync(auditLog).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to log action {Action} for {TableName} with ID {EntityId}",
                    action, tableName, entityId);

                if (_configuration.ThrowOnAuditFailure)
                    throw new AuditException($"Failed to log audit action: {ex.Message}", ex);
            }
        }

        public async Task LogChangesAsync<T>(T oldEntity, T newEntity, string tableName, Guid entityId, string user = "[System]")
        {
            if (!_configuration.IsAuditEnabled || oldEntity == null || newEntity == null)
                return;

            try
            {
                var changes = await GetEntityChangesAsync(oldEntity, newEntity).ConfigureAwait(false);
                if (!changes.Any())
                    return;

                var auditLog = await CreateChangeAuditLogAsync(oldEntity, newEntity, tableName, entityId, user, changes).ConfigureAwait(false);
                await _auditRepository.AddAuditLogAsync(auditLog).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to log changes for {TableName} with ID {EntityId}", tableName, entityId);

                if (_configuration.ThrowOnAuditFailure)
                    throw new AuditException($"Failed to log audit changes: {ex.Message}", ex);
            }
        }

        #endregion

        #region Public Methods

        public async Task<PagedResult<AuditLogDto>> SearchAuditLogsAsync(AuditSearchRequest searchRequest)
        {
            ValidateSearchRequest(searchRequest);

            try
            {
                return await _auditRepository.SearchPagedAsync(searchRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to search audit logs");
                throw new AuditException($"Failed to search audit logs: {ex.Message}", ex);
            }
        }
        public DataTable SearchAuditLogsAsync(int year, string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            try
            {
                return _auditRepository.SearchPagedAsync(year, searchTerm, orderBy, pageNumber, pageSize, out totalRecord);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to search audit logs");
                throw new AuditException($"Failed to search audit logs: {ex.Message}", ex);
            }
        }
        public DataTable SearchAuditLogsAsync(int year, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            try
            {
                return _auditRepository.SearchPagedAsync(year, parameters, orderBy, pageNumber, pageSize, out totalRecord);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to search audit logs");
                throw new AuditException($"Failed to search audit logs: {ex.Message}", ex);
            }
        }

        public async Task<AuditLogDto> GetAuditLogByIdAsync(Guid auditLogId)
        {
            try
            {
                return await _auditRepository.GetByIdAsync(auditLogId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get audit log by ID: {AuditLogId}", auditLogId);
                throw new AuditException($"Failed to get audit log: {ex.Message}", ex);
            }
        }

        public async Task<List<AuditLogDto>> GetAuditTrailAsync(string tableName, Guid recordId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                return await _auditRepository.GetAuditTrailAsync(tableName, recordId, fromDate, toDate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get audit trail for {TableName} with ID {RecordId}", tableName, recordId);
                throw new AuditException($"Failed to get audit trail: {ex.Message}", ex);
            }
        }

        public async Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, string tableName = null, string userId = null)
        {
            try
            {
                return await _auditRepository.GetAuditStatisticsAsync(fromDate, toDate, tableName, userId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get audit statistics");
                throw new AuditException($"Failed to get audit statistics: {ex.Message}", ex);
            }
        }

        public async Task ExecuteUpdateWithLogAsync<T>(string selectQuery, string updateQuery, string tableName, Guid recordId, string changedBy, Guid userId, Guid? refId = null) where T : new()
        {
            if (string.IsNullOrWhiteSpace(selectQuery) || string.IsNullOrWhiteSpace(updateQuery))
                return;

            try
            {
                // Get old data
                var oldData = await _auditRepository.ExecuteQueryAsync<T>(selectQuery).ConfigureAwait(false);
                if (!oldData.Any())
                {
                    SysLogger.LogError("No old data found for audit logging during update operation");
                    return;
                }

                // Execute update
                await _auditRepository.ExecuteNonQueryAsync(updateQuery).ConfigureAwait(false);

                // Get new data
                var newData = await _auditRepository.ExecuteQueryAsync<T>(selectQuery).ConfigureAwait(false);
                if (!newData.Any())
                {
                    SysLogger.LogError("No new data found for audit logging after update operation");
                    return;
                }

                // Log changes
                await LogChangesAsync(oldData.First(), newData.First(), tableName, recordId, changedBy).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to execute update with log for {TableName} with ID {RecordId}", tableName, recordId);
                throw new AuditException($"Failed to execute update with log: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private Methods

        private async Task<AuditLog> CreateAuditLogAsync<T>(LogActions.Actions action, T entity, string tableName, Guid entityId, string changeBy)
        {
            var (customerId, refId) = await ExtractIdentifiersAsync(entity).ConfigureAwait(false);
            var changes = await GetEntityPropertiesAsync(entity).ConfigureAwait(false);

            return new AuditLog
            {
                Id = UUIDv7.NewGuid(),
                Title = GetAuditTitle(tableName, action),
                CustomerId = customerId,
                RefId = refId,
                TableName = tableName,
                RecordId = entityId,
                ActionType = action.ToString(),
                Changes = changes,
                ChangedBy = !string.IsNullOrEmpty(changeBy) ? changeBy : _clientInfo.UserName,
                UserId = _clientInfo.UserId,
                IPAddress = _clientInfo.IpAddress,
                UserAgent = _clientInfo.UserAgent,
                ChangedAt = DateTime.UtcNow
            };
        }

        private async Task<AuditLog> CreateChangeAuditLogAsync<T>(T oldEntity, T newEntity, string tableName, Guid entityId, string changeBy, Dictionary<string, ChangeInfo> changes)
        {
            var (customerId, refId) = await ExtractIdentifiersAsync(newEntity, oldEntity).ConfigureAwait(false);
            return new AuditLog
            {
                Id = UUIDv7.NewGuid(),
                Title = GetAuditTitle(tableName, LogActions.Actions.UPDATE),
                CustomerId = customerId,
                RefId = refId,
                TableName = tableName,
                RecordId = entityId,
                ActionType = LogActions.Actions.UPDATE.ToString(),
                Changes = changes.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                ChangedBy = !string.IsNullOrEmpty(changeBy) ? changeBy : _clientInfo.UserName,
                UserId = _clientInfo.UserId,
                IPAddress = _clientInfo.IpAddress,
                UserAgent = _clientInfo.UserAgent,
                ChangedAt = DateTime.UtcNow
            };
        }

        private async Task<Dictionary<string, ChangeInfo>> GetEntityChangesAsync<T>(T oldEntity, T newEntity)
        {
            var changes = new Dictionary<string, ChangeInfo>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            await Task.Run(() =>
            {
                foreach (var property in properties.Where(p => !ExcludedProperties.Contains(p.Name)))
                {
                    var oldValue = SafeGetPropertyValue(oldEntity, property);
                    var newValue = SafeGetPropertyValue(newEntity, property);

                    var normalizedOld = NormalizeValue(oldValue);
                    var normalizedNew = NormalizeValue(newValue);

                    if (normalizedOld != normalizedNew)
                    {
                        changes[property.Name] = new ChangeInfo
                        {
                            OldValue = oldValue,
                            NewValue = newValue,
                            PropertyType = property.PropertyType.Name
                        };
                    }
                }
            }).ConfigureAwait(false);

            return changes;
        }

        private async Task<Dictionary<string, object>> GetEntityPropertiesAsync<T>(T entity)
        {
            var properties = new Dictionary<string, object>();
            var entityProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            await Task.Run(() =>
            {
                foreach (var property in entityProperties.Where(p => !ExcludedProperties.Contains(p.Name)))
                {
                    var value = SafeGetPropertyValue(entity, property);
                    properties[property.Name] = value;
                }
            }).ConfigureAwait(false);

            return properties;
        }

        private async Task<(Guid? customerId, Guid? refId)> ExtractIdentifiersAsync<T>(T entity, T fallbackEntity = default)
        {
            return await Task.Run(() =>
            {
                Guid? customerId = null;
                Guid? refId = null;

                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (property.Name.Equals("CustomerId", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = SafeGetPropertyValue(entity, property) ?? SafeGetPropertyValue(fallbackEntity, property);
                        customerId = ParseGuidOrDefault(value);
                    }
                    else if (RefIdProperties.Contains(property.Name))
                    {
                        var value = SafeGetPropertyValue(entity, property) ?? SafeGetPropertyValue(fallbackEntity, property);
                        refId = ParseGuidOrDefault(value);
                    }

                    if (customerId.HasValue && refId.HasValue)
                        break;
                }

                return (customerId, refId);
            }).ConfigureAwait(false);
        }

        private string GetAuditTitle(string tableName, LogActions.Actions action)
        {
            try
            {
                switch (action)
                {
                    case LogActions.Actions.LOGIN:
                        return GetResourceText(BackEndResourceKeys.LOGIN);
                    case LogActions.Actions.LOGOUT:
                        return GetResourceText(BackEndResourceKeys.LOGOUT);
                    case LogActions.Actions.EXPORT:
                        return GetResourceText(BackEndResourceKeys.EXPORT_EXCEL);
                    default:
                        return GetTableDisplayName(tableName);
                }
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get audit title for table {TableName} and action {Action}", tableName, action);
                return tableName;
            }
        }

        private string GetTableDisplayName(string tableName)
        {
            switch (tableName)
            {
                case nameof(AspnetUser):
                case "aspnet_Users":
                    return GetResourceText(BackEndResourceKeys.USER_LIST);
                case nameof(AspnetRole):
                    return GetResourceText(BackEndResourceKeys.USER_GROUP);
                case nameof(TblEmailHistory):
                    return GetResourceText(BackEndResourceKeys.SEND_MAIL);
                case nameof(TblSetting):
                    return GetResourceText(BackEndResourceKeys.SETTINGS);
                case nameof(TblUploadFile):
                    return "Tập tin hệ thống";
                default:
                    return tableName;
            }
        }

        private string GetResourceText(string key)
        {
            try
            {
                return UITextsReader.GetBackEndResourceText(key) ?? key;
            }
            catch
            {
                return key;
            }
        }

        private string SafeGetPropertyValue<T>(T entity, PropertyInfo property)
        {
            if (entity == null || property == null)
                return null;

            try
            {
                var value = property.GetValue(entity);
                return value?.ToString();
            }
            catch (Exception ex)
            {
                SysLogger.LogDebug(ex.Message, "Failed to get property value for {PropertyName}", property.Name);
                return null;
            }
        }

        private static string NormalizeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (decimal.TryParse(value, out var decimalValue))
                return decimalValue.ToString("G");

            return value.Trim();
        }

        private static Guid? ParseGuidOrDefault(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var result) && result != Guid.Empty)
                    return result;
            }
            return null;
        }

        private void ValidateSearchRequest(AuditSearchRequest searchRequest)
        {
            if (searchRequest == null)
                throw new ArgumentNullException(nameof(searchRequest));

            if (searchRequest.PageNumber < 1)
                throw new ArgumentException("PageNumber must be greater than 0", nameof(searchRequest.PageNumber));

            if (searchRequest.PageSize < 1 || searchRequest.PageSize > 1000)
                throw new ArgumentException("PageSize must be between 1 and 1000", nameof(searchRequest.PageSize));
        }

        #endregion
    }


}
