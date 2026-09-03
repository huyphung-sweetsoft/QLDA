using SubSonic;
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
    public class DuAnRepository: BaseRepository<TblDuAn>
    {
        public DuAnRepository(AuditManager auditManager) : base(auditManager) { }

        public DataTable SearchPaging(string searchTerm, Dictionary<string, object> parameters,string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string keyword = InlineQueryHelpers.SQLEncode(searchTerm ?? string.Empty);
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @idLoaiDuAn VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters[TblDuAn.Columns.IdLoaiDuAn])}';
                DECLARE @idNhanVienQuanLy VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters[TblDuAn.Columns.IdNhanVienQuanLy])}';
                DECLARE @trangThai TINYINT = {(parameters[TblDuAn.Columns.TrangThai] == null ? "NULL" : $"'{InlineQueryHelpers.SQLEncode(parameters[TblDuAn.Columns.TrangThai])}'")};
                DECLARE @idKhachHang VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters[TblDuAn.Columns.IdKhachHang])}';
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                select * from (
                    select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
                        select d.*,
                        nv.TenNhanVien,
                        kh.TenKhachHang,
                        COUNT(1) OVER() AS total_records
                        from TblDuAn d
                        left join TblNhanVien nv on nv.IdNhanVien = d.IdNhanVienQuanLy
                        left join TblKhachHang kh on kh.IdKhachHang = d.IdKhachHang
                        where d.DaXoa = 0
                        and (@idLoaiDuAn = '{Guid.Empty}' or d.IdLoaiDuAn = @idLoaiDuAn)
                        and (@idNhanVienQuanLy = '{Guid.Empty}' or d.IdNhanVienQuanLy = @idNhanVienQuanLy)
                        and (@trangThai is null or d.TrangThai = @trangThai)
                        and (@idKhachHang = '{Guid.Empty}' or d.IdKhachHang = @idKhachHang)
                        and (@singleKeyWord = N'%%'
                        or d.TenDuAn LIKE @singleKeyWord
                        or d.MaDuAn LIKE @singleKeyWord)
                    ) as T
                ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow";
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
                select * from (
                    select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
                        select d.*,
                        nv.TenNhanVien,
                        kh.TenKhachHang,
                        COUNT(1) OVER() AS total_records
                        from TblDuAn d
                        left join TblNhanVien nv on nv.IdNhanVien = d.IdNhanVienQuanLy
                        left join TblKhachHang kh on kh.IdKhachHang = d.IdKhachHang
                        where d.DaXoa = 0
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

        public override TblDuAn Insert(TblDuAn item)
        {
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, Guid.Parse(item.GetColumnValue("IdDuAn").ToString())).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblDuAn");
                }
            });
            return item;
        }

        public override TblDuAn Update(TblDuAn duAn)
        {
            Guid id = Guid.Parse(duAn.GetColumnValue("IdDuAn").ToString());
            TblDuAn itemOld = GetById(id);
            duAn.Save();
            string updatedBy = string.Empty;
            try
            {
                updatedBy = duAn.GetColumnValue("NguoiCapNhat")?.ToString();
            }
            catch { }
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, duAn, _tableName, id, updatedBy).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblDuAn");
                }
            });
            return duAn;
        }

        public override TblDuAn GetById(Guid id)
        {
            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.IdDuAnColumn).IsEqualTo(id)
                .And(TblDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblDuAn>();
        }
        public DataTable GetDetailById(Guid id)
        {
            string sql = $@"
                DECLARE @idDuAn UNIQUEIDENTIFIER = '{InlineQueryHelpers.SQLEncode(id)}';
                select TOP 1
                    d.*,
                    dt.TenLoaiDuAn,
                    kh.TenKhachHang,
                    hd.SoHopDong,
                    hd.GiaTriHopDong,
                    hd.NgayKy,
                    nv.TenNhanVien,
                    nv.AnhDaiDien
                from TblDuAn d
                left join TblNhanVien nv on nv.IdNhanVien = d.IdNhanVienQuanLy
                left join TblKhachHang kh on kh.IdKhachHang = d.IdKhachHang
                left join TblLoaiDuAn dt on dt.IdLoaiDuAn = d.IdLoaiDuAn
                left join TblHopDongThucHien hd on hd.IdHopDongThucHien = d.IdHopDongThucHien
                where d.IdDuAn = @idDuAn
                and d.DaXoa = 0;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
        }
        public TblDuAn GetByMaDuAn(string maDuAn)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return null;
            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.MaDuAnColumn).IsEqualTo(maDuAn.Trim())
                .And(TblDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblDuAn>();
        }

        public string GenerateMaDuAn()
        {
            string sql = @"SELECT NEXT VALUE FOR dbo.SeqMaDuAn;";
            int nextNumber = new InlineQuery().ExecuteScalar<int>(sql);
            return string.Format("PRJ-{0:D3}", nextNumber);
        }

        public bool IsContractUsed(Guid idHopDongThucHien, Guid idDuAn)
        {
            Select select = new Select();
            select.From(TblDuAn.Schema)
                .Where(TblDuAn.IdHopDongThucHienColumn).IsEqualTo(idHopDongThucHien)
                .And(TblDuAn.DaXoaColumn).IsEqualTo(false);
            if (idDuAn != Guid.Empty)
            {
                select.And(TblDuAn.IdDuAnColumn).IsNotEqualTo(idDuAn);
            }
            TblDuAn duAn = select.ExecuteSingle<TblDuAn>();
            return duAn != null;
        }
    }
}
