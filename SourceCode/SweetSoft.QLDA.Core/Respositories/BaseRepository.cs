using SubSonic;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class BaseRepository<T> where T : ActiveRecord<T>, new()
    {
        protected readonly AuditManager _auditManager;
        protected readonly string _tableName;
        protected readonly TableSchema.Table _schema;

        public BaseRepository(AuditManager auditManager)
        {
            _auditManager = auditManager;
            string name = new T().GetType().Name;
            _schema = DataService.GetSchema(name, null);
            if (_schema != null)
                _tableName = _schema.TableName;
            else
                _tableName = name;
        }
        public virtual DataTable SearchPaging(string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            return null;
        }
        public virtual DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = BuildSearchSQL(_tableName, parameters, orderBy, pageNumber, pageSize);
            IDataReader reader = new InlineQuery().ExecuteReader(sql);
            if (reader == null) return null;

            DataTable dt = new DataTable();
            dt.Load(reader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }

        public virtual T GetById(Guid id)
        {
            return new Select()
                .From(_schema)
                .Where("Id").IsEqualTo(id)
                .And("IsDeleted").IsEqualTo(false)
                .ExecuteSingle<T>();
        }

        public virtual T Insert(T item)
        {
            item.Save();
            Task.Run(async () =>
            {
                await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, Guid.Parse(item.GetColumnValue("Id").ToString()));
            });
            return item;
        }

        public virtual T Update(T itemNew)
        {
            var id = Guid.Parse(itemNew.GetColumnValue("Id").ToString());
            T itemOld = GetById(id);
            itemNew.Save();
            string updatedBy = string.Empty;
            try
            {
                updatedBy = itemNew.GetColumnValue("UpdatedBy")?.ToString() ?? itemNew.GetColumnValue("UpdatedUser")?.ToString();
            }
            catch
            {
                updatedBy = "";
            }
            Task.Run(async () =>
            {
                await _auditManager.LogChangesAsync(itemOld, itemNew, _tableName, id, updatedBy);
            });
            return itemNew;
        }

        public virtual bool Delete(T item)
        {
            ActiveRecord<T>.Delete("Id", item.GetColumnValue("Id"));
            Task.Run(async () =>
            {
                await _auditManager.LogActionAsync(LogActions.Actions.DELETE, item, _tableName, Guid.Parse(item.GetColumnValue("Id").ToString()));
            });
            return true;
        }

        public virtual void UpdateDisplayOrder(Guid id, int displayOrder)
        {
            string sql = $"UPDATE {_tableName} SET DisplayOrder = {displayOrder} WHERE Id COLLATE utf8mb4_unicode_ci = '{id}'";
            new InlineQuery().Execute(sql);
        }
        private static string BuildSearchSQL(string tableName, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize)
        {
            var whereClauses = new List<string> { "IsDeleted = 0" };

            foreach (var param in parameters)
            {
                if (param.Value == null) continue;

                if (param.Key.ToLower().Contains("name") || param.Key.ToLower().Contains("code"))
                    whereClauses.Add($"{param.Key} LIKE '%{param.Value}%'");
                else if (param.Value is bool || param.Value is int)
                    whereClauses.Add($"{param.Key} = {param.Value}");
                else
                    whereClauses.Add($"({param.Key} = '{param.Value}' OR {param.Key} = '')");
            }

            //int offset = (pageNumber - 1) * pageSize;
            string whereClause = whereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", whereClauses)}" : "";

            return $@"
            SELECT SQL_CALC_FOUND_ROWS * FROM {tableName}
            {whereClause}
            ORDER BY {orderBy}
            LIMIT {pageNumber} OFFSET {pageSize};
            SELECT FOUND_ROWS() as TotalRecord;";
        }
    }

}
