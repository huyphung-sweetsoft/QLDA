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

        public DataTable SearchPaging(string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select d.*
                        , nv.TenNhanVien
                        , COUNT(1) OVER() AS total_records
                        from TblDuAn d
                        join TblNhanVien nv on nv.IdNhanVien = d.IdNhanVienQuanLy
                        where d.DaXoa = 0 
                        and (@singleKeyWord = N'%%'
                        or TenDuAn LIKE @singleKeyWord
                        or MaDuAn LIKE @singgleKeyWord)
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

     //   public DataTable SearchPaging(string searchTerm, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
     //   {
     //       totalRecord = 0;
     //       string sql = $@"
     //           DECLARE @startRow INT = {pageNumber};
     //           DECLARE @endRow INT = {pageSize};
     //           DECLARE @isActivated BIT = {InlineQueryHelpers.SQLEncode(parameters[AspnetUser.Columns.IsActivated])};
     //           DECLARE @roleId VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters[AspnetRole.Columns.RoleId])}';
     //           DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
     //           select * from (
					//select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
					//	select f.*
     //                   , ms.Email
     //                   , RoleName = (
     //                       select top 1 RoleName from aspnet_Roles r
     //                       inner join aspnet_UsersInRoles mp on mp.RoleId = r.RoleId 
     //                       where mp.UserId = f.UserId
     //                   )
     //                   , COUNT(1) OVER() AS total_records
     //                   from aspnet_Users f
     //                   inner join aspnet_Membership ms on ms.UserId = f.UserId
     //                   left join aspnet_UsersInRoles r on r.UserId = f.UserId 
     //                   where f.IsDeleted = 0 
     //                   and (@isActivated is null or f.IsActivated = @isActivated)
     //                   and (@roleId = '{Guid.Empty}' or r.RoleId = @roleId)
     //                   and (@singleKeyWord = N'%%'
     //                   or Username LIKE @singleKeyWord
     //                   or DisplayName LIKE @singleKeyWord
     //                   or Email LIKE @singleKeyWord
     //                   or MobileAlias LIKE @singleKeyWord)
					//) as T
     //           ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow;";
     //       IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
     //       if (iDataReader == null)
     //           return null;

     //       DataTable dt = new DataTable();
     //       dt.Load(iDataReader);
     //       InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
     //       return dt;
     //   }
        public override DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @maDuAn NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters[TblDuAn.Columns.MaDuAn])}%';
                DECLARE @tenDuAn NVARCHAR(250) = N'%{InlineQueryHelpers.SQLEncode(parameters[TblDuAn.Columns.TenDuAn])}%';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select d.*
                        , nv.TenNhanVien
                        , COUNT(1) OVER() AS total_records
                        from TblDuAn d 
                        join TblNhanVien nv on nv.IdNhanVien = d.IdNhanVien
                        where d.DaXoa = 0 
                        and (@maDuAn = N'%%' or d.MaDuAn like @maDuAn)
                        and (@tenDuAn = N'%%' or f.TenDuAn like @tenDuAn)
                        
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

        public override TblDuAn Update(TblDuAn project)
        {
            var id = Guid.Parse(project.GetColumnValue("IdDuAn").ToString());
            TblDuAn itemOld = GetById(id);
            project.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, project, _tableName, id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for AspnetUser");
                }
            });
            return project;
        }

        public override TblDuAn GetById(Guid id)
        {
            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.IdDuAnColumn).IsEqualTo(id)
                .And(TblDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblDuAn>();
        }

        public override bool Delete(TblDuAn item)
        {
            if (item == null) return false;
            return new TblDuAnController().Delete(item.IdDuAn);
        }

        public TblDuAn GetByTrangThai(byte status)
        {
            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.TrangThaiColumn).IsEqualTo(status)
                .And(TblDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblDuAn>();
        }
    }
}
