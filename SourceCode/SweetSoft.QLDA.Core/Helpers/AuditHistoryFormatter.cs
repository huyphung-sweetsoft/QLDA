using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers
{
    public static class AuditHistoryFormatter
    {
        public static string Format(DataRow row)
        {
            if (row == null) return string.Empty;

            string descriptionKey = row["Description"].ToString();

            string actor = row["ChangedBy"].ToString();

            if (string.IsNullOrEmpty(actor))
                actor = "[System]";
            string tableName = row["TableName"].ToString();
            string title = row["Title"].ToString();
            string template = GetResourceText(descriptionKey);
            try
            {
                switch (descriptionKey)
                {
                    case BackEndResourceKeys.HISTORY_CREATED_ENTITY:
                    case BackEndResourceKeys.HISTORY_UPDATED_ENTITY:
                    case BackEndResourceKeys.HISTORY_DELETED_ENTITY:
                        return string.Format(template, actor, GetEntityDisplayName(tableName), title);
                    case BackEndResourceKeys.HISTORY_ADDED_TO_CONTAINER:
                    case BackEndResourceKeys.HISTORY_REMOVED_FROM_CONTAINER:
                    case BackEndResourceKeys.HISTORY_LINKED_ENTITY:
                    case BackEndResourceKeys.HISTORY_UNLINKED_ENTITY:
                        return string.Format(template, actor, GetEntityDisplayName(tableName), title);
                    default: return string.Format(template, actor, GetEntityDisplayName(tableName), title);
                }
            } catch(FormatException) 
            {
                    return string.Format("{0} - {1}", actor, title);
            }
        }

        private static string GetEntityDisplayName(string tableName)
        {
            string resourceKey;

            switch (tableName)
            {
                case nameof(TblDuAn): resourceKey = BackEndResourceKeys.HISTORY_ENTITY_PROJECT; break;
                case nameof(TblGiaiDoanDuAn): resourceKey = BackEndResourceKeys.HISTORY_ENTITY_STAGE; break;
                case nameof(TblCongViec): resourceKey = BackEndResourceKeys.HISTORY_ENTITY_TASK; break;
                case nameof(TblThanhVienDuAn): resourceKey = BackEndResourceKeys.HISTORY_ENTITY_MEMBER; break;
                case nameof(TblHopDongThucHien): resourceKey = BackEndResourceKeys.HISTORY_ENTITY_CONTRACT; break;
                default: return tableName;
            }

            return GetResourceText(resourceKey);
        }

        private static string GetResourceText(string resourceKey)
        {
            string value = UITextsReader.GetBackEndResourceText(resourceKey);
            return value;
        }
    }
}
