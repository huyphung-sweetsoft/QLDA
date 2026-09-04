using SubSonic;
using SweetSoft.QLDA.Core.Helpers.Security;
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
    public class UserRepository : BaseRepository<AspnetUser>
    {
        public UserRepository(AuditManager auditManager) : base(auditManager) { }

        public DataTable SearchPaging(string searchTerm, Guid roleId, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
        DECLARE @startRow INT = {pageNumber};
        DECLARE @endRow INT = {pageSize};
        DECLARE @roleId VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(roleId)}';
        DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
        
        SELECT * FROM (
            SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                SELECT f.*
                , ms.Email
                , pb.TenPhongBan
                , cd.TenChucDanh
                , RoleName = (
                    SELECT TOP 1 RoleName FROM aspnet_Roles r
                    INNER JOIN aspnet_UsersInRoles mp ON mp.RoleId = r.RoleId 
                    WHERE mp.UserId = f.UserId
                )
                , COUNT(1) OVER() AS total_records
                FROM aspnet_Users f
                INNER JOIN aspnet_Membership ms ON ms.UserId = f.UserId
                LEFT JOIN aspnet_UsersInRoles r ON r.UserId = f.UserId 
                LEFT JOIN TblPhongBan pb ON pb.IdPhongBan = f.IdPhongBan
                LEFT JOIN TblChucDanh cd ON cd.IdChucDanh = f.IdChucDanh
                WHERE f.IsDeleted = 0 
                AND (@roleId = '{Guid.Empty}' OR @roleId = '' OR r.RoleId = @roleId)
                AND (@singleKeyWord = N'%%'
                OR f.Username LIKE @singleKeyWord
                OR f.DisplayName LIKE @singleKeyWord
                OR ms.Email LIKE @singleKeyWord
                OR f.MobileAlias LIKE @singleKeyWord
                OR f.IdCCCD LIKE @singleKeyWord
                OR f.DiaChi LIKE @singleKeyWord)
            ) AS T
        ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }

        public DataTable SearchPaging(string searchTerm, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
            DECLARE @startRow INT = {pageNumber};
            DECLARE @endRow INT = {pageSize};

            -- KIỂU BIT: KHÔNG BỌC NHÁY ĐƠN ĐỂ SQL NHẬN TỪ KHÓA null
            DECLARE @isActivated BIT = {InlineQueryHelpers.SQLEncode(parameters.ContainsKey(AspnetUser.Columns.IsActivated) ? parameters[AspnetUser.Columns.IsActivated] : "null")};
            DECLARE @laNhanVien BIT = {InlineQueryHelpers.SQLEncode(parameters.ContainsKey("LaNhanVien") ? parameters["LaNhanVien"] : "null")};

            -- KIỂU GUID: BỌC NHÁY ĐƠN (Vì đã được thẻ ValueIsOfTypeGUID ép về Guid.Empty)
            DECLARE @roleId VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(AspnetRole.Columns.RoleId) ? parameters[AspnetRole.Columns.RoleId] : Guid.Empty)}';
            DECLARE @idPhongBan VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("IdPhongBan") ? parameters["IdPhongBan"] : Guid.Empty)}';
            DECLARE @idChucDanh VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("IdChucDanh") ? parameters["IdChucDanh"] : Guid.Empty)}';

            DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';

            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                    SELECT f.*
                    , ms.Email
                    , pb.TenPhongBan
                    , cd.TenChucDanh
                    , RoleName = (
                        SELECT TOP 1 RoleName FROM aspnet_Roles r
                        INNER JOIN aspnet_UsersInRoles mp ON mp.RoleId = r.RoleId 
                        WHERE mp.UserId = f.UserId
                    )
                    , COUNT(1) OVER() AS total_records
                    FROM aspnet_Users f
                    INNER JOIN aspnet_Membership ms ON ms.UserId = f.UserId
                    LEFT JOIN aspnet_UsersInRoles r ON r.UserId = f.UserId 
                    LEFT JOIN TblPhongBan pb ON pb.IdPhongBan = f.IdPhongBan
                    LEFT JOIN TblChucDanh cd ON cd.IdChucDanh = f.IdChucDanh
                    WHERE f.IsDeleted = 0 
            
                    -- SỬ DỤNG LẠI CÚ PHÁP WHERE SẠCH ĐẸP CỦA CODE GỐC
                    AND (@isActivated IS NULL OR f.IsActivated = @isActivated)
                    AND (@laNhanVien IS NULL OR f.LaNhanVien = @laNhanVien)
                    AND (@roleId = '{Guid.Empty}' OR r.RoleId = @roleId)
                    AND (@idPhongBan = '{Guid.Empty}' OR f.IdPhongBan = @idPhongBan)
                    AND (@idChucDanh = '{Guid.Empty}' OR f.IdChucDanh = @idChucDanh)
                    AND (@singleKeyWord = N'%%'
                    OR f.Username LIKE @singleKeyWord
                    OR f.DisplayName LIKE @singleKeyWord
                    OR ms.Email LIKE @singleKeyWord
                    OR f.MobileAlias LIKE @singleKeyWord
                    OR f.IdCCCD LIKE @singleKeyWord
                    OR f.DiaChi LIKE @singleKeyWord)
                ) AS T
            ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null) return null;

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

    -- DỮ LIỆU CƠ BẢN DẠNG CHUỖI (CÓ NHÁY ĐƠN)
    DECLARE @userName NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(AspnetUser.Columns.UserName) ? parameters[AspnetUser.Columns.UserName] : string.Empty)}%';
    DECLARE @displayName NVARCHAR(250) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(AspnetUser.Columns.DisplayName) ? parameters[AspnetUser.Columns.DisplayName] : string.Empty)}%';
    DECLARE @email NVARCHAR(250) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("Email") ? parameters["Email"] : string.Empty)}%';
    DECLARE @phoneNumber NVARCHAR(50) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(AspnetUser.Columns.MobileAlias) ? parameters[AspnetUser.Columns.MobileAlias] : string.Empty)}%';
    DECLARE @IdCCCD VARCHAR(20) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("IdCCCD") ? parameters["IdCCCD"] : string.Empty)}%';
    DECLARE @diaChi NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("DiaChi") ? parameters["DiaChi"] : string.Empty)}%';
    DECLARE @gioiTinh NVARCHAR(10) = N'{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("GioiTinh") ? parameters["GioiTinh"] : string.Empty)}';

    DECLARE @lastActivityDateFrom VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("LastActivityDateFrom") ? parameters["LastActivityDateFrom"] : string.Empty)}';
    DECLARE @lastActivityDateTo VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("LastActivityDateTo") ? parameters["LastActivityDateTo"] : string.Empty)}';
    DECLARE @ngayGiaNhapTu VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("NgayGiaNhapTu") ? parameters["NgayGiaNhapTu"] : string.Empty)}';
    DECLARE @ngayGiaNhapDen VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("NgayGiaNhapDen") ? parameters["NgayGiaNhapDen"] : string.Empty)}';

    -- KIỂU BIT: KHÔNG BỌC NHÁY ĐƠN ĐỂ SQL NHẬN TỪ KHÓA null
    DECLARE @isActivated BIT = {InlineQueryHelpers.SQLEncode(parameters.ContainsKey(AspnetUser.Columns.IsActivated) ? parameters[AspnetUser.Columns.IsActivated] : "null")};
    DECLARE @laNhanVien BIT = {InlineQueryHelpers.SQLEncode(parameters.ContainsKey("LaNhanVien") ? parameters["LaNhanVien"] : "null")};

    -- KIỂU GUID: BỌC NHÁY ĐƠN
    DECLARE @roleId VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("RoleId") ? parameters["RoleId"] : Guid.Empty)}';
    DECLARE @idPhongBan VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("IdPhongBan") ? parameters["IdPhongBan"] : Guid.Empty)}';
    DECLARE @idChucDanh VARCHAR(36) = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("IdChucDanh") ? parameters["IdChucDanh"] : Guid.Empty)}';

    SELECT * FROM (
        SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
            SELECT f.*
            , ms.Email
            , pb.TenPhongBan
            , cd.TenChucDanh
            , RoleName = (
                SELECT TOP 1 RoleName FROM aspnet_Roles r
                INNER JOIN aspnet_UsersInRoles mp ON mp.RoleId = r.RoleId 
                WHERE mp.UserId = f.UserId
            )
            , COUNT(1) OVER() AS total_records
            FROM aspnet_Users f 
            INNER JOIN aspnet_Membership ms ON ms.UserId = f.UserId
            LEFT JOIN aspnet_UsersInRoles r ON r.UserId = f.UserId 
            LEFT JOIN TblPhongBan pb ON pb.IdPhongBan = f.IdPhongBan
            LEFT JOIN TblChucDanh cd ON cd.IdChucDanh = f.IdChucDanh
            WHERE f.IsDeleted = 0 
            
            AND (@userName = N'%%' OR f.UserName LIKE @userName)
            AND (@displayName = N'%%' OR f.DisplayName LIKE @displayName)
            AND (@email = N'%%' OR ms.Email LIKE @email)
            AND (@phoneNumber = N'%%' OR f.MobileAlias LIKE @phoneNumber)
            AND (@IdCCCD = N'%%' OR f.IdCCCD LIKE @IdCCCD)
            AND (@diaChi = N'%%' OR f.DiaChi LIKE @diaChi)
            -- Đề phòng Dropdown Giới tính trả về chữ 'null'
            AND (@gioiTinh = N'' OR @gioiTinh = 'null' OR f.GioiTinh = @gioiTinh) 
            
            AND (@lastActivityDateFrom = '' OR @lastActivityDateTo = '' OR f.LastActivityDate BETWEEN @lastActivityDateFrom AND @lastActivityDateTo)
            AND (@ngayGiaNhapTu = '' OR @ngayGiaNhapDen = '' OR f.NgayGiaNhap BETWEEN @ngayGiaNhapTu AND @ngayGiaNhapDen)

            -- CÚ PHÁP TÌM KIẾM THEO DROP-DOWN SẠCH ĐẸP
            AND (@isActivated IS NULL OR f.IsActivated = @isActivated)
            AND (@laNhanVien IS NULL OR f.LaNhanVien = @laNhanVien)
            AND (@roleId = '{Guid.Empty}' OR r.RoleId = @roleId)
            AND (@idPhongBan = '{Guid.Empty}' OR f.IdPhongBan = @idPhongBan)
            AND (@idChucDanh = '{Guid.Empty}' OR f.IdChucDanh = @idChucDanh)
        ) AS T
    ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null) return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }
        public override AspnetUser GetById(Guid id)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn).IsEqualTo(id)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetUser>();
        }
        public override AspnetUser Update(AspnetUser user)
        {
            var id = Guid.Parse(user.GetColumnValue("UserId").ToString());
            AspnetUser itemOld = GetById(id);

            // Bắt buộc bổ sung ngày cập nhật trước khi Save
            user.NgayCapNhat = DateTime.Now;

            user.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, user, _tableName, id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for AspnetUser");
                }
            });
            return user;
        }

        public override bool Delete(AspnetUser item)
        {
            if (item == null) return false;

            var id = item.UserId;
            AspnetUser itemOld = GetById(id);

            // Chuyển sang Xóa mềm (Soft Delete) giống NhanVienRepository
            item.IsDeleted = true;
            item.NgayCapNhat = DateTime.Now;
            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, item, _tableName, id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log delete for AspnetUser");
                }
            });
            return true;
        }
        public AspnetUser GetByUserName(string userName)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserNameColumn).IsEqualTo(userName)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetUser>();
        }
        public AspnetUser GetByEmail(string email)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .InnerJoin(AspnetMembership.UserIdColumn, AspnetUser.UserIdColumn)
                .Where(AspnetMembership.EmailColumn).IsEqualTo(email)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<AspnetUser>();
        }
        public string GetDisplayNameById(Guid ID)
        {
            return new Select(AspnetUser.DisplayNameColumn)
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn).IsEqualTo(ID)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteScalar<string>();
        }
        public string GetDisplayNameByUserName(string username)
        {
            return new Select(AspnetUser.DisplayNameColumn)
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserNameColumn).IsEqualTo(username)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteScalar<string>();
        }
        public bool ValidateUser(string username, string password)
        {
            try
            {
                Select select = new Select();
                select.From(AspnetUser.Schema);
                select.InnerJoin(AspnetMembership.UserIdColumn, AspnetUser.UserIdColumn);
                select.Where(AspnetUser.UserNameColumn).IsEqualTo(username);
                select.And(AspnetMembership.PasswordColumn).IsEqualTo(SecurityUtilities.ComputeMd5Hash(password));
                select.And(AspnetUser.IsActivatedColumn).IsEqualTo(1);
                select.And(AspnetUser.IsDeletedColumn).IsEqualTo(0);
                return select.GetRecordCount() > 0;
            }
            catch
            {
                return false;
            }
        }
        public bool IsEmailExist(Guid ID, string email)
        {
            Select select = new Select();
            select.From(AspnetMembership.Schema);
            select.Where(AspnetMembership.EmailColumn).IsEqualTo(email);
            select.And(AspnetMembership.UserIdColumn).IsNotEqualTo(ID);
            return select.GetRecordCount() > 0;
        }
        public bool IsUserNameExist(Guid ID, string userName)
        {
            Select select = new Select();
            select.From(AspnetUser.Schema);
            select.Where(AspnetUser.UserNameColumn).IsEqualTo(userName);
            select.And(AspnetUser.UserIdColumn).IsNotEqualTo(ID);
            return select.GetRecordCount() > 0;
        }
        public bool IsCCCDExist(Guid id, string idCCCD)
        {
            return new Select().From(AspnetUser.Schema).Where(AspnetUser.IdCCCDColumn).IsEqualTo(idCCCD).And(AspnetUser.UserIdColumn).IsNotEqualTo(id).And(AspnetUser.IsDeletedColumn).IsEqualTo(false).GetRecordCount() > 0;
        }
        public List<AspnetUser> GetAllAspnetUsers()
        {
            Select select = new Select();
            select.From(AspnetUser.Schema);
            return select.ExecuteTypedList<AspnetUser>();
        }
        public List<AspnetUser> GetAllNhanVienActive()
        {
            return new Select().From(AspnetUser.Schema).Where(AspnetUser.LaNhanVienColumn).IsEqualTo(true).And(AspnetUser.IsDeletedColumn).IsEqualTo(false).ExecuteTypedList<AspnetUser>();
        }
        public AspnetUser GetByCCCD(string idCCCD)
        {
            return new Select().From(AspnetUser.Schema).Where(AspnetUser.IdCCCDColumn).IsEqualTo(idCCCD).And(AspnetUser.IsDeletedColumn).IsEqualTo(false).ExecuteSingle<AspnetUser>();
        }
        public List<AspnetUser> GetByPhongBan(Guid idPhongBan)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.IdPhongBanColumn).IsEqualTo(idPhongBan)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteTypedList<AspnetUser>();
        }

        public List<AspnetUser> GetByChucDanh(Guid idChucDanh)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.IdChucDanhColumn).IsEqualTo(idChucDanh)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteTypedList<AspnetUser>();
        }
        public DataTable GetNhanVienForDetail(Guid userId)
        {
            string sql = $@"
        SELECT f.*
            , pb.TenPhongBan
            , cd.TenChucDanh
            , m.Email
            FROM aspnet_Users f
            LEFT JOIN TblPhongBan pb ON f.IdPhongBan = pb.IdPhongBan
            LEFT JOIN TblChucDanh cd ON f.IdChucDanh = cd.IdChucDanh
            LEFT JOIN aspnet_Membership m ON f.UserId = m.UserId
            WHERE f.UserId = '{userId}' AND f.IsDeleted = 0";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null) return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
            
        }
    }
}
