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
    public class KhachHangRepository : BaseRepository<TblKhachHang>
    {
        public KhachHangRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public DataTable SearchPaging(string searchTerms, Dictionary<string, object> parameters, string orderBy, int pageNum, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string keyword = InlineQueryHelpers.SQLEncode(searchTerms ?? string.Empty);
            string sql = $@"
                DECLARE @startRow INT = {pageNum};
                DECLARE @endRow INT = {pageSize};
                DECLARE @idLoaiKhachHang VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters[TblKhachHang.Columns.IdLoaiKhachHang])}';
                DECLARE @kichHoat BIT = {(parameters[TblKhachHang.Columns.KichHoat] == null ? "NULL" : $"'{InlineQueryHelpers.SQLEncode(parameters[TblKhachHang.Columns.KichHoat])}'")};
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerms)}%'
                select * from(
                    select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from(
                        select kh.*,
                        lkh.TenLoaiKhachHang,
                        ISNULL(d.SoLuongDuAn,0) AS SoLuongDuAn,
                        COUNT(1) OVER() AS total_records
                        from TblKhachHang as kh
                        left join TblLoaiKhachHang lkh on lkh.IdLoaiKhachHang = kh.IdLoaiKhachHang
                        left join (select 
                                    IdKhachHang,
                                    COUNT(1) AS SoluongDuAn
                                    from TblDuAn where DaXoa = 0 
                                    group by IdKhachHang)
                                   AS d on d.IdKhachHang = kh.IdKhachHang
                        where kh.DaXoa = 0
                        and (@idLoaiKhachHang = '{Guid.Empty}' or kh.IdLoaiKhachHang = @idLoaiKhachHang)
                        and (@kichHoat is null or kh.KichHoat = @kichHoat)
                        and (@singleKeyWord = N'%%'
                        or kh.TenKhachHang LIKE @singleKeyWord
                        or kh.SoDienThoai LIKE @singleKeyWord
                        or kh.Email LIKE @singleKeyWord)
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

        public override TblKhachHang Insert(TblKhachHang item)
        {
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, Guid.Parse(item.GetColumnValue("IdKhachHang").ToString())).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblKhachHang");
                }
            });
            return item;
        }

        public override TblKhachHang Update(TblKhachHang itemNew)
        {
            Guid id = Guid.Parse(itemNew.GetColumnValue("IdKhachHang").ToString());
            TblKhachHang itemOld = GetById(id);
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
                    await _auditManager.LogChangesAsync(itemOld, itemNew, _tableName, id, updatedBy).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblDuAn");
                }
            });
            return itemNew;
        }

        public override TblKhachHang GetById(Guid id)
        {
            return new Select()
                .From(TblKhachHang.Schema)
                .Where(TblKhachHang.IdKhachHangColumn).IsEqualTo(id)
                .And(TblKhachHang.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblKhachHang>();
        }

        public DataTable GetDetailById(Guid id)
        {
            string sql = $@"
                DECLARE @idKhachHang UNIQUEIDENTIFIER = '{InlineQueryHelpers.SQLEncode(id)}';
                select TOP 1
                    kh.*,
                    kht.TenLoaiKhachHang
                from TblKhachHang kh
                left join TblLoaiKhachHang kht on kht.IdLoaiKhachHang = kh.IdLoaiKhachHang
                where kh.IdKhachHang = @idKhachHang
                and kh.DaXoa = 0;";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
        }

        public List<TblKhachHang> GetAllTblKhachHang()
        {
            Select select = new Select();
            select.From(TblKhachHang.Schema);
            select.And(TblKhachHang.DaXoaColumn).IsEqualTo(false);
            return select.ExecuteTypedList<TblKhachHang>();
        }
    }
}
