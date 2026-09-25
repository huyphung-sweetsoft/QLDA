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

            QueryCommand cmd =
                new QueryCommand(
                    sql,
                    DataService.Provider.Name
                );

            DataSet ds =
                DataService.GetDataSet(cmd);

            return
                (ds != null && ds.Tables.Count > 0)
                    ? ds.Tables[0]
                    : new DataTable();
        }

        public TblHeSoDongGop GetSystemCoefficient(Guid idDoUuTien)
        {
            return new Select()
                .From(TblHeSoDongGop.Schema)
                .Where(TblHeSoDongGop.IdDoUuTienColumn)
                    .IsEqualTo(idDoUuTien)
                .And(TblHeSoDongGop.IdDuAnColumn)
                    .IsNull()
                .And(TblHeSoDongGop.DaXoaColumn)
                    .IsEqualTo(false)
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

            QueryCommand cmd =
                new QueryCommand(
                    sql,
                    DataService.Provider.Name
                );

            cmd.AddParameter(
                "@IdDuAn",
                idDuAn,
                DbType.Guid
            );

            DataSet ds =
                DataService.GetDataSet(cmd);

            return
                (ds != null && ds.Tables.Count > 0)
                    ? ds.Tables[0]
                    : new DataTable();
        }

        public TblHeSoDongGop GetProjectCoefficient(
            Guid idDuAn,
            Guid idDoUuTien)
        {
            return new Select()
                .From(TblHeSoDongGop.Schema)
                .Where(TblHeSoDongGop.IdDuAnColumn)
                    .IsEqualTo(idDuAn)
                .And(TblHeSoDongGop.IdDoUuTienColumn)
                    .IsEqualTo(idDoUuTien)
                .And(TblHeSoDongGop.DaXoaColumn)
                    .IsEqualTo(false)
                .ExecuteSingle<TblHeSoDongGop>();
        }

        #endregion


        #region NHÓM 3: CẤU HÌNH PROJECT DEFAULT / CUSTOM

        public bool SetProjectUseDefault(
            Guid idDuAn,
            bool useDefault)
        {
            int affectedRows =
                new Update(TblDuAn.Schema)
                    .Set("SuDungHeSoDongGopMacDinh")
                    .EqualTo(useDefault)
                    .Where(TblDuAn.IdDuAnColumn)
                    .IsEqualTo(idDuAn)
                    .Execute();

            return affectedRows > 0;
        }

        #endregion


        #region NHÓM 4: INSERT HỆ SỐ

        public TblHeSoDongGop InsertHeSoDongGop(
            TblHeSoDongGop item,
            string description = null)
        {
            if (item == null)
                return null;

            Guid id =
                Guid.Parse(
                    item.GetColumnValue(
                        "IdHeSoDongGop"
                    ).ToString()
                );

            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                        LogActions.Actions.CREATE,
                        item,
                        _tableName,
                        id,
                        item.NguoiTao,
                        item.IdDuAn ?? Guid.Empty,
                        string.Empty,
                        description
                    ).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log CREATE action for TblHeSoDongGop"
                    );
                }
            });

            return item;
        }

        #endregion


        #region NHÓM 5: UPDATE HỆ SỐ

        public TblHeSoDongGop UpdateHeSoDongGop(
            TblHeSoDongGop item,
            string description = null)
        {
            if (item == null)
                return null;

            Guid id =
                Guid.Parse(
                    item.GetColumnValue(
                        "IdHeSoDongGop"
                    ).ToString()
                );

            TblHeSoDongGop itemOld =
                GetById(id);

            item.Save();

            string updatedBy = string.Empty;

            try
            {
                updatedBy =
                    item.GetColumnValue(
                        "NguoiCapNhat"
                    )?.ToString();
            }
            catch
            {
            }

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                        itemOld,
                        item,
                        _tableName,
                        id,
                        updatedBy
                    ).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log changes for TblHeSoDongGop"
                    );
                }
            });

            return item;
        }

        #endregion
    }
}