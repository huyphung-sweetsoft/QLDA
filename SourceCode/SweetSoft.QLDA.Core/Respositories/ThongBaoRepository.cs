using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class ThongBaoRepository : BaseRepository<TblThongBao>
    {
        public ThongBaoRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        #region Query

        /// <summary>
        /// Lấy danh sách thông báo của một user, lazy load 10 item/trang.
        /// </summary>
        public DataTable GetByUser(Guid userId, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            if (userId == Guid.Empty)
                return null;

            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow   INT = {pageSize};
                DECLARE @userId   UNIQUEIDENTIFIER = '{userId}';

                SELECT *
                FROM
                (
                    SELECT ROW_NUMBER() OVER (ORDER BY NgayTao DESC) AS RowNum, T.*
                    FROM
                    (
                        SELECT
                            tb.*,
                            COUNT(1) OVER() AS total_records
                        FROM dbo.TblThongBao tb
                        WHERE tb.DaXoa  = 0
                          AND tb.UserId = @userId
                    ) AS T
                ) AS T1
                WHERE RowNum >= @startRow AND RowNum <= @endRow;";

            IDataReader reader = new InlineQuery().ExecuteReader(sql);

            if (reader == null)
                return null;

            DataTable table = new DataTable();
            table.Load(reader);

            InlineQueryHelpers.GetTotal(ref table, out totalRecord);

            return table;
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc của một user.
        /// </summary>
        public int CountUnread(Guid userId)
        {
            if (userId == Guid.Empty)
                return 0;

            string sql = $@"
                SELECT COUNT(1) AS UnreadCount
                FROM dbo.TblThongBao
                WHERE DaXoa  = 0
                  AND UserId = '{userId}'
                  AND DaDoc  = 0;";

            int result = new InlineQuery().ExecuteScalar<int>(sql);

            return result;
        }

        #endregion

        #region CRUD

        public override TblThongBao GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblThongBao.Schema)
                .Where(TblThongBao.IdThongBaoColumn).IsEqualTo(id)
                .And(TblThongBao.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblThongBao>();
        }

        public override TblThongBao Insert(TblThongBao item)
        {
            item.Save();

            Guid id = item.IdThongBao;

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, id, item.NguoiTao).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblThongBao");
                }
            });

            return item;
        }

        public override TblThongBao Update(TblThongBao item)
        {
            Guid id = item.IdThongBao;
            TblThongBao oldItem = GetById(id);

            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(oldItem, item, _tableName, id, item.NguoiCapNhat).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log UPDATE action for TblThongBao");
                }
            });

            return item;
        }

        public override bool Delete(TblThongBao item)
        {
            if (item == null)
                return false;

            TblThongBao existing = GetById(item.IdThongBao);

            if (existing == null)
                return false;

            existing.DaXoa        = true;
            existing.NguoiCapNhat = item.NguoiCapNhat;
            existing.NgayCapNhat  = DateTime.Now;
            existing.Save();

            return true;
        }

        #endregion

        #region Mark as read

        /// <summary>
        /// Đánh dấu một thông báo đã đọc.
        /// </summary>
        public void MarkAsRead(Guid idThongBao)
        {
            if (idThongBao == Guid.Empty)
                return;

            string sql = $@"
                UPDATE dbo.TblThongBao
                SET DaDoc   = 1,
                    NgayDoc = GETDATE()
                WHERE IdThongBao = '{idThongBao}'
                  AND DaDoc = 0;";

            new InlineQuery().Execute(sql);
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo của user đã đọc.
        /// </summary>
        public void MarkAllAsRead(Guid userId)
        {
            if (userId == Guid.Empty)
                return;

            string sql = $@"
                UPDATE dbo.TblThongBao
                SET DaDoc   = 1,
                    NgayDoc = GETDATE()
                WHERE UserId = '{userId}'
                  AND DaDoc  = 0
                  AND DaXoa  = 0;";

            new InlineQuery().Execute(sql);
        }

        #endregion
    }
}
