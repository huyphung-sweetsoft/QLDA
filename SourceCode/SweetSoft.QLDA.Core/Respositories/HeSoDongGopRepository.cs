using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class HeSoDongGopRepository : BaseRepository<TblHeSoDongGop>
    {
        public HeSoDongGopRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        #region NHÓM 1: LẤY HỆ SỐ MẶC ĐỊNH CỦA HỆ THỐNG

        public DataTable GetSystemDefaults()
        {
            const string sql = @"
                SELECT
                    ut.IdDoUuTien,
                    ut.MaDoUuTien,
                    ut.TenDoUuTien,
                    ut.DiemUuTien,
                    hs.IdHeSoDongGop,
                    hs.HeSoDongGop
                FROM TblDoUuTien ut
                LEFT JOIN TblHeSoDongGop hs
                    ON hs.IdDoUuTien = ut.IdDoUuTien
                    AND hs.IdDuAn IS NULL
                    AND hs.DaXoa = 0
                ORDER BY
                    ut.DiemUuTien ASC;
            ";

            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            DataSet ds = DataService.GetDataSet(cmd);

            return (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();
        }

        public TblHeSoDongGop GetSystemCoefficient(Guid idDoUuTien)
        {
            return new Select()
                .From(TblHeSoDongGop.Schema)
                // THÊM .ToString() VÀO ĐÂY:
                .Where(TblHeSoDongGop.IdDoUuTienColumn).IsEqualTo(idDoUuTien.ToString())
                .And(TblHeSoDongGop.IdDuAnColumn).IsNull()
                .And(TblHeSoDongGop.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblHeSoDongGop>();
        }

        #endregion

        #region NHÓM 2: LẤY HỆ SỐ CỦA DỰ ÁN

        public DataTable GetProjectCoefficients(Guid idDuAn)
        {
            const string sql = @"
                SELECT
                    ut.IdDoUuTien,
                    ut.MaDoUuTien,
                    ut.TenDoUuTien,
                    ut.DiemUuTien,
                    hs.IdHeSoDongGop,
                    hs.IdDuAn,
                    hs.HeSoDongGop
                FROM TblDoUuTien ut
                LEFT JOIN TblHeSoDongGop hs
                    ON hs.IdDoUuTien = ut.IdDoUuTien
                    AND hs.IdDuAn = @IdDuAn
                    AND hs.DaXoa = 0
                ORDER BY
                    ut.DiemUuTien ASC;
            ";

            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@IdDuAn", idDuAn, DbType.Guid);
            DataSet ds = DataService.GetDataSet(cmd);

            return (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();
        }

        public TblHeSoDongGop GetProjectCoefficient(Guid idDuAn, Guid idDoUuTien)
        {
            return new Select()
                .From(TblHeSoDongGop.Schema)
                // THÊM .ToString() VÀO 2 CHỖ NÀY:
                .Where(TblHeSoDongGop.IdDuAnColumn).IsEqualTo(idDuAn.ToString())
                .And(TblHeSoDongGop.IdDoUuTienColumn).IsEqualTo(idDoUuTien.ToString())
                .And(TblHeSoDongGop.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblHeSoDongGop>();
        }

        #endregion

        #region NHÓM 4: INSERT HỆ SỐ

        public TblHeSoDongGop InsertHeSoDongGop(TblHeSoDongGop item, string description = null)
        {
            if (item == null) return null;

            Guid id = Guid.Parse(item.GetColumnValue("IdHeSoDongGop").ToString());

            string sql = @"INSERT INTO TblHeSoDongGop (IdHeSoDongGop, IdDuAn, IdDoUuTien, HeSoDongGop, DaXoa, NguoiTao, NgayTao)
                           VALUES (@IdHeSoDongGop, @IdDuAn, @IdDoUuTien, @HeSoDongGop, 0, @NguoiTao, @NgayTao)";

            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@IdHeSoDongGop", item.IdHeSoDongGop, DbType.Guid);
            cmd.AddParameter("@IdDuAn", item.IdDuAn.HasValue ? (object)item.IdDuAn.Value : DBNull.Value, DbType.Guid);
            cmd.AddParameter("@IdDoUuTien", item.IdDoUuTien, DbType.Guid);
            cmd.AddParameter("@HeSoDongGop", item.HeSoDongGop, DbType.Decimal);

            // [THÊM CHỐNG NULL]: An toàn tuyệt đối cho Database
            cmd.AddParameter("@NguoiTao", string.IsNullOrEmpty(item.NguoiTao) ? (object)DBNull.Value : item.NguoiTao, DbType.String);
            cmd.AddParameter("@NgayTao", item.NgayTao ?? DateTime.Now, DbType.DateTime);

            DataService.ExecuteQuery(cmd);

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, id, item.NguoiTao, item.IdDuAn ?? Guid.Empty, string.Empty, description).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblHeSoDongGop");
                }
            });

            return item;
        }

        #endregion

        #region NHÓM 5: UPDATE HỆ SỐ

        public TblHeSoDongGop UpdateHeSoDongGop(TblHeSoDongGop item, string description = null)
        {
            if (item == null)
                return null;

            Guid id = Guid.Parse(item.GetColumnValue("IdHeSoDongGop").ToString());
            TblHeSoDongGop itemOld = new Select()
            .From(TblHeSoDongGop.Schema)
            .Where("IdHeSoDongGop").IsEqualTo(id.ToString())
            .ExecuteSingle<TblHeSoDongGop>();

            // [SỬA LỖI]: Bỏ item.Save(), dùng QueryCommand để ép kiểu
            string sql = @"UPDATE TblHeSoDongGop 
                           SET HeSoDongGop = @HeSoDongGop, 
                               DaXoa = @DaXoa,
                               NguoiCapNhat = @NguoiCapNhat, 
                               NgayCapNhat = @NgayCapNhat 
                           WHERE IdHeSoDongGop = @IdHeSoDongGop";   
            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@HeSoDongGop", item.HeSoDongGop, DbType.Decimal);
            cmd.AddParameter("@DaXoa", item.DaXoa, DbType.Boolean);
            cmd.AddParameter("@NguoiCapNhat", item.NguoiCapNhat, DbType.String);
            cmd.AddParameter("@NgayCapNhat", item.NgayCapNhat ?? DateTime.Now, DbType.DateTime);
            cmd.AddParameter("@IdHeSoDongGop", item.IdHeSoDongGop, DbType.Guid);
            DataService.ExecuteQuery(cmd);

            string updatedBy = string.Empty;
            try
            {
                updatedBy = item.GetColumnValue("NguoiCapNhat")?.ToString();
            }
            catch { }

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, item, _tableName, id, updatedBy).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblHeSoDongGop");
                }
            });

            return item;
        }

        #endregion
    }
}