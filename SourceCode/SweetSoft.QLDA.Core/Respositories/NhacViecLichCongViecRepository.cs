using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    /// <summary>
    /// Repository cho reminder theo từng công việc khi lịch làm việc chung thay đổi.
    /// Không dùng DaDoc của TblThongBao để biểu diễn trạng thái reminder.
    /// </summary>
    public class NhacViecLichCongViecRepository : BaseRepository<TblNhacViecLichCongViec>
    {
        public NhacViecLichCongViecRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        #region Query

        /// <summary>
        /// Lấy reminder theo Id.
        /// Không loại bỏ Processed/Superseded vì đây là dữ liệu lịch sử.
        /// </summary>
        public override TblNhacViecLichCongViec GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblNhacViecLichCongViec.Schema)
                .Where(TblNhacViecLichCongViec.IdNhacViecColumn)
                    .IsEqualTo(id)
                .ExecuteSingle<TblNhacViecLichCongViec>();
        }

        /// <summary>
        /// Lấy toàn bộ reminder Pending của một PM trong một project.
        /// </summary>
        public List<TblNhacViecLichCongViec> GetPendingByProject(Guid idDuAn, Guid userId)
        {
            if (idDuAn == Guid.Empty || userId == Guid.Empty)
                return new List<TblNhacViecLichCongViec>();

            return new Select()
                .From(TblNhacViecLichCongViec.Schema)
                .Where(TblNhacViecLichCongViec.IdDuAnColumn)
                    .IsEqualTo(idDuAn)
                .And(TblNhacViecLichCongViec.UserIdColumn)
                    .IsEqualTo(userId)
                .And(TblNhacViecLichCongViec.TrangThaiColumn)
                    .IsEqualTo(NhacViecLichCongViecStatus.Pending)
                .OrderAsc(TblNhacViecLichCongViec.IdCongViecColumn.ColumnName)
                .OrderDesc(TblNhacViecLichCongViec.NgayTaoColumn.ColumnName)
                .ExecuteTypedList<TblNhacViecLichCongViec>();
        }

        /// <summary>
        /// Lấy các reminder Pending của một task cho đúng user.
        /// </summary>
        public List<TblNhacViecLichCongViec> GetPendingByTask(Guid idCongViec, Guid userId)
        {
            if (idCongViec == Guid.Empty || userId == Guid.Empty)
                return new List<TblNhacViecLichCongViec>();

            return new Select()
                .From(TblNhacViecLichCongViec.Schema)
                .Where(TblNhacViecLichCongViec.IdCongViecColumn)
                    .IsEqualTo(idCongViec)
                .And(TblNhacViecLichCongViec.UserIdColumn)
                    .IsEqualTo(userId)
                .And(TblNhacViecLichCongViec.TrangThaiColumn)
                    .IsEqualTo(NhacViecLichCongViecStatus.Pending)
                .OrderDesc(TblNhacViecLichCongViec.NgayTaoColumn.ColumnName)
                .ExecuteTypedList<TblNhacViecLichCongViec>();
        }

        /// <summary>
        /// Lấy danh sách Id task đang có ít nhất một reminder Pending.
        /// Dùng cho Task List để xác định task nào phải hiện icon.
        /// </summary>
        public DataTable GetPendingTaskSummary(Guid idDuAn, Guid userId)
        {
            if (idDuAn == Guid.Empty || userId == Guid.Empty)
                return new DataTable();

            const string sql = @"
SELECT
    IdCongViec,
    COUNT(1) AS ReminderCount
FROM dbo.TblNhacViecLichCongViec
WHERE IdDuAn = @IdDuAn
  AND UserId = @UserId
  AND TrangThai = @PendingStatus
GROUP BY IdCongViec;";

            QueryCommand command = new QueryCommand(sql, DataService.Provider.Name);
            command.AddParameter("@IdDuAn", idDuAn, DbType.Guid);
            command.AddParameter("@UserId", userId, DbType.Guid);
            command.AddParameter("@PendingStatus", NhacViecLichCongViecStatus.Pending, DbType.Byte);

            DataSet ds = DataService.GetDataSet(command);
            return (ds != null && ds.Tables.Count > 0)
                ? ds.Tables[0]
                : new DataTable();
        }

        /// <summary>
        /// Lấy các reminder Pending của một notification.
        /// Hữu ích khi cần xem tất cả task sinh ra từ cùng một lần thay đổi lịch.
        /// </summary>
        public List<TblNhacViecLichCongViec> GetPendingByNotification(Guid idThongBao, Guid userId)
        {
            if (idThongBao == Guid.Empty || userId == Guid.Empty)
                return new List<TblNhacViecLichCongViec>();

            return new Select()
                .From(TblNhacViecLichCongViec.Schema)
                .Where(TblNhacViecLichCongViec.IdThongBaoColumn)
                    .IsEqualTo(idThongBao)
                .And(TblNhacViecLichCongViec.UserIdColumn)
                    .IsEqualTo(userId)
                .And(TblNhacViecLichCongViec.TrangThaiColumn)
                    .IsEqualTo(NhacViecLichCongViecStatus.Pending)
                .OrderDesc(TblNhacViecLichCongViec.NgayTaoColumn.ColumnName)
                .ExecuteTypedList<TblNhacViecLichCongViec>();
        }

        #endregion

        #region CRUD

        public override TblNhacViecLichCongViec Insert(TblNhacViecLichCongViec item)
        {
            if (item == null)
                return null;

            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                        LogActions.Actions.CREATE,
                        item,
                        _tableName,
                        item.IdNhacViec).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblNhacViecLichCongViec");
                }
            });

            return item;
        }

        public override TblNhacViecLichCongViec Update(TblNhacViecLichCongViec item)
        {
            if (item == null)
                return null;

            Guid id = item.IdNhacViec;
            TblNhacViecLichCongViec oldItem = GetById(id);

            item.NgayCapNhat = DateTime.Now;
            item.Save();

            string updatedBy = string.Empty;
            try
            {
                updatedBy = item.NguoiCapNhat ?? string.Empty;
            }
            catch
            {
            }

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                        oldItem,
                        item,
                        _tableName,
                        id,
                        updatedBy).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblNhacViecLichCongViec");
                }
            });

            return item;
        }

        /// <summary>
        /// Reminder không xóa vật lý. Delete được quy về Superseded để giữ lịch sử.
        /// </summary>
        public override bool Delete(TblNhacViecLichCongViec item)
        {
            if (item == null)
                return false;

            TblNhacViecLichCongViec existing = GetById(item.IdNhacViec);
            if (existing == null)
                return false;

            existing.TrangThai = NhacViecLichCongViecStatus.Superseded;
            existing.NgayCapNhat = DateTime.Now;
            existing.NguoiCapNhat = item.NguoiCapNhat;
            existing.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                        item,
                        existing,
                        _tableName,
                        existing.IdNhacViec,
                        existing.NguoiCapNhat ?? string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log supersede action for TblNhacViecLichCongViec");
                }
            });

            return true;
        }

        #endregion

        #region Status operations

        /// <summary>
        /// Đánh dấu một danh sách reminder là Processed.
        /// Chỉ chính user sở hữu reminder mới được xử lý.
        /// Chỉ Pending mới chuyển sang Processed.
        /// </summary>
        public int MarkAsProcessed(IEnumerable<Guid> reminderIds, Guid userId, string updatedBy)
        {
            if (reminderIds == null || userId == Guid.Empty)
                return 0;

            List<Guid> ids = reminderIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return 0;

            string idList = string.Join(",", ids.Select(x => $"'{x:D}'"));

            string sql = $@"
UPDATE dbo.TblNhacViecLichCongViec
SET TrangThai = {NhacViecLichCongViecStatus.Processed},
    NgayXuLy = GETDATE(),
    UserIdXuLy = '{userId:D}',
    NgayCapNhat = GETDATE(),
    NguoiCapNhat = N'{InlineQueryHelpers.SQLEncode(updatedBy ?? string.Empty)}'
WHERE UserId = '{userId:D}'
  AND TrangThai = {NhacViecLichCongViecStatus.Pending}
  AND IdNhacViec IN ({idList});";

            return new InlineQuery().ExecuteScalar<int>(sql + "\nSELECT @@ROWCOUNT;");
        }

        /// <summary>
        /// Đánh dấu các reminder cũ là Superseded.
        /// </summary>
        public int MarkAsSuperseded(IEnumerable<Guid> reminderIds, string updatedBy)
        {
            if (reminderIds == null)
                return 0;

            List<Guid> ids = reminderIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return 0;

            string idList = string.Join(",", ids.Select(x => $"'{x:D}'"));

            string sql = $@"
UPDATE dbo.TblNhacViecLichCongViec
SET TrangThai = {NhacViecLichCongViecStatus.Superseded},
    NgayCapNhat = GETDATE(),
    NguoiCapNhat = N'{InlineQueryHelpers.SQLEncode(updatedBy ?? string.Empty)}'
WHERE TrangThai = {NhacViecLichCongViecStatus.Pending}
  AND IdNhacViec IN ({idList});";

            return new InlineQuery().ExecuteScalar<int>(sql + "\nSELECT @@ROWCOUNT;");
        }

        #endregion
    }

    /// <summary>
    /// Loại nguồn sinh reminder.
    /// </summary>
    public static class NhacViecLichCongViecTypes
    {
        public const string CauHinhTuan = "CAU_HINH_TUAN";
        public const string LichNgoaiLe = "LICH_NGOAI_LE";
    }

    /// <summary>
    /// Trạng thái reminder.
    /// </summary>
    public static class NhacViecLichCongViecStatus
    {
        public const byte Pending = 0;
        public const byte Processed = 1;
        public const byte Superseded = 2;
    }
}
