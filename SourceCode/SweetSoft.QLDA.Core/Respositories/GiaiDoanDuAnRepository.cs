using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class GiaiDoanDuAnRepository : BaseRepository<TblGiaiDoanDuAn>
    {
        public GiaiDoanDuAnRepository(AuditManager auditManager) : base(auditManager) { }

        public override TblGiaiDoanDuAn Insert(TblGiaiDoanDuAn item)
        {
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                        LogActions.Actions.CREATE,
                        item,
                        _tableName,
                        Guid.Parse(item.GetColumnValue("IdGiaiDoanDuAn").ToString()))
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblGiaiDoanDuAn");
                }
            });
            return item;
        }

        public override TblGiaiDoanDuAn Update(TblGiaiDoanDuAn itemNew)
        {
            Guid id = Guid.Parse(itemNew.GetColumnValue("IdGiaiDoanDuAn").ToString());
            TblGiaiDoanDuAn itemOld = GetById(id);
            itemNew.Save();
            string updatedBy = string.Empty;
            try
            {
                updatedBy = itemNew.GetColumnValue("NguoiCapNhat")?.ToString();
            }
            catch { }
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, itemNew, _tableName, id, updatedBy)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log UPDATE action for TblGiaiDoanDuAn");
                }
            });
            return itemNew;
        }


        public override TblGiaiDoanDuAn GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblGiaiDoanDuAn.Schema)
                .Where(TblGiaiDoanDuAn.IdGiaiDoanDuAnColumn).IsEqualTo(id)
                .And(TblGiaiDoanDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblGiaiDoanDuAn>();
        }

        public DataTable GetByIdDuAn(Guid idDuAn)
        {
            if (idDuAn == Guid.Empty)
                return null;

            string sql = $@"
                DECLARE @idDuAn UNIQUEIDENTIFIER = '{InlineQueryHelpers.SQLEncode(idDuAn)}';

                SELECT
                    gdda.IdGiaiDoanDuAn,
                    gdda.IdDuAn,
                    gdda.IdGiaiDoan,
                    CASE
                        WHEN gdda.IdGiaiDoan IS NOT NULL THEN gd.TenGiaiDoan
                        ELSE gdda.TenGiaiDoanTuyChinh
                    END AS TenGiaiDoan,
                    gdda.TenGiaiDoanTuyChinh,
                    CASE
                        WHEN gdda.IdGiaiDoan IS NULL THEN CAST(1 AS BIT)
                        ELSE CAST(0 AS BIT)
                    END AS LaGiaiDoanTuyChinh,
                    gdda.NgayBatDau,
                    gdda.NgayDuKienHoanThanh,
                    gdda.NgayHoanThanhThucTe,
                    gdda.ThuTuGiaiDoan,
                    gdda.MoTa
                FROM dbo.TblGiaiDoanDuAn gdda
                LEFT JOIN dbo.TblGiaiDoan gd
                    ON gd.IdGiaiDoan = gdda.IdGiaiDoan
                   AND gd.DaXoa = 0
                WHERE gdda.IdDuAn = @idDuAn
                  AND gdda.DaXoa = 0
                ORDER BY gdda.ThuTuGiaiDoan, gdda.NgayBatDau;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
        }

        public bool IsCommonStageExists(Guid idDuAn, Guid idGiaiDoan, Guid excludedId)
        {
            Select select = new Select();
            select.From(TblGiaiDoanDuAn.Schema)
                .Where(TblGiaiDoanDuAn.IdDuAnColumn).IsEqualTo(idDuAn)
                .And(TblGiaiDoanDuAn.IdGiaiDoanColumn).IsEqualTo(idGiaiDoan)
                .And(TblGiaiDoanDuAn.DaXoaColumn).IsEqualTo(false);

            if (excludedId != Guid.Empty)
                select.And(TblGiaiDoanDuAn.IdGiaiDoanDuAnColumn).IsNotEqualTo(excludedId);

            return select.ExecuteSingle<TblGiaiDoanDuAn>() != null;
        }

        public bool IsCustomStageExists(Guid idDuAn, string stageName, Guid excludedId)
        {
            if (idDuAn == Guid.Empty || string.IsNullOrWhiteSpace(stageName))
                return false;

            string sql = $@"
                DECLARE @idDuAn UNIQUEIDENTIFIER = '{InlineQueryHelpers.SQLEncode(idDuAn)}';
                DECLARE @excludedId UNIQUEIDENTIFIER = '{InlineQueryHelpers.SQLEncode(excludedId)}';
                DECLARE @stageName NVARCHAR(250) = N'{InlineQueryHelpers.SQLEncode(stageName.Trim())}';

                SELECT CASE
                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM dbo.TblGiaiDoanDuAn
                        WHERE IdDuAn = @idDuAn
                          AND IdGiaiDoan IS NULL
                          AND DaXoa = 0
                          AND (@excludedId = '{Guid.Empty}' OR IdGiaiDoanDuAn <> @excludedId)
                          AND LOWER(LTRIM(RTRIM(TenGiaiDoanTuyChinh))) =
                              LOWER(LTRIM(RTRIM(@stageName)))
                    )
                    THEN 1
                    ELSE 0
                END;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
        }

        public bool IsOrderExists(Guid idDuAn, int order, Guid excludedId)
        {
            Select select = new Select();
            select.From(TblGiaiDoanDuAn.Schema)
                .Where(TblGiaiDoanDuAn.IdDuAnColumn).IsEqualTo(idDuAn)
                .And(TblGiaiDoanDuAn.ThuTuGiaiDoanColumn).IsEqualTo(order)
                .And(TblGiaiDoanDuAn.DaXoaColumn).IsEqualTo(false);

            if (excludedId != Guid.Empty)
                select.And(TblGiaiDoanDuAn.IdGiaiDoanDuAnColumn).IsNotEqualTo(excludedId);

            return select.ExecuteSingle<TblGiaiDoanDuAn>() != null;
        }

        public int GetNextOrder(Guid idDuAn)
        {
            if (idDuAn == Guid.Empty)
                return 1;

            string sql = $@"
                DECLARE @idDuAn UNIQUEIDENTIFIER = '{InlineQueryHelpers.SQLEncode(idDuAn)}';

                SELECT ISNULL(MAX(ThuTuGiaiDoan), 0) + 1
                FROM dbo.TblGiaiDoanDuAn
                WHERE IdDuAn = @idDuAn
                  AND DaXoa = 0;";

            return new InlineQuery().ExecuteScalar<int>(sql);
        }
    }
}
