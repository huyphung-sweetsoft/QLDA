using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using System.Linq;
using System.Text;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class NhanVienRepository : BaseRepository<TblNhanVien>
    {
        public NhanVienRepository(AuditManager auditManager) : base(auditManager) { }

        #region Search Paging

        public DataTable SearchPaging(string searchTerm, Guid userId, Guid maPhongBan, Guid maChucDanh, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
        DECLARE @startRow      INT          = {pageNumber};
        DECLARE @endRow        INT          = {pageSize};
        DECLARE @userId        VARCHAR(36)  = '{InlineQueryHelpers.SQLEncode(userId)}';
        DECLARE @idPhongBan    VARCHAR(36)  = '{InlineQueryHelpers.SQLEncode(maPhongBan)}';
        DECLARE @idChucDanh    VARCHAR(36)  = '{InlineQueryHelpers.SQLEncode(maChucDanh)}';
        DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';

        SELECT * FROM (
            SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                SELECT f.*
                    , u.UserName
                    , ms.Email
                    , u.MobileAlias AS PhoneNumber -- <--- BỔ SUNG CỘT SỐ ĐIỆN THOẠI Ở ĐÂY
                    , pb.TenPhongBan
                    , cd.TenChucDanh
                    , COUNT(1) OVER() AS total_records
                FROM TblNhanVien f
                LEFT JOIN aspnet_Users u ON u.UserId = f.UserId
                LEFT JOIN aspnet_Membership ms ON ms.UserId = u.UserId
                LEFT JOIN TblPhongBan pb ON pb.IdPhongBan = f.IdPhongBan
                LEFT JOIN TblChucDanh cd ON cd.IdChucDanh = f.IdChucDanh
                WHERE f.DaXoa = 0
                    AND (@userId = '{Guid.Empty}' OR @userId = '' OR f.UserId = @userId)
                    AND (@idPhongBan = '{Guid.Empty}' OR @idPhongBan = '' OR f.IdPhongBan = @idPhongBan)
                    AND (@idChucDanh = '{Guid.Empty}' OR @idChucDanh = '' OR f.IdChucDanh = @idChucDanh)
                    AND (@singleKeyWord = N'%%'
                        OR f.TenNhanVien LIKE @singleKeyWord
                        OR f.IdCCCD LIKE @singleKeyWord
                        OR f.DiaChi LIKE @singleKeyWord
                        OR ms.Email LIKE @singleKeyWord
                        OR u.UserName LIKE @singleKeyWord
                        OR u.MobileAlias LIKE @singleKeyWord) -- <--- BỔ SUNG TÌM KIẾM THEO SĐT
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
        DECLARE @startRow      INT          = {pageNumber};
        DECLARE @endRow        INT          = {pageSize};
        DECLARE @idPhongBan    VARCHAR(36)  = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.IdPhongBan) ? parameters[TblNhanVien.Columns.IdPhongBan] : Guid.Empty)}';
        DECLARE @idChucDanh    VARCHAR(36)  = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.IdChucDanh) ? parameters[TblNhanVien.Columns.IdChucDanh] : Guid.Empty)}';
        DECLARE @gioiTinh      NVARCHAR(10) = N'{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.GioiTinh) ? parameters[TblNhanVien.Columns.GioiTinh] : string.Empty)}';
        DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';

        SELECT * FROM (
            SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                SELECT f.*
                    , u.UserName
                    , ms.Email
                    , u.MobileAlias AS PhoneNumber -- <--- BỔ SUNG CỘT SỐ ĐIỆN THOẠI Ở ĐÂY
                    , pb.TenPhongBan
                    , cd.TenChucDanh
                    , COUNT(1) OVER() AS total_records
                FROM TblNhanVien f
                LEFT JOIN aspnet_Users u ON u.UserId = f.UserId
                LEFT JOIN aspnet_Membership ms ON ms.UserId = u.UserId
                LEFT JOIN TblPhongBan pb ON pb.IdPhongBan = f.IdPhongBan
                LEFT JOIN TblChucDanh cd ON cd.IdChucDanh = f.IdChucDanh
                WHERE f.DaXoa = 0
                    AND (@idPhongBan = '{Guid.Empty}' OR @idPhongBan = '' OR f.IdPhongBan = @idPhongBan)
                    AND (@idChucDanh = '{Guid.Empty}' OR @idChucDanh = '' OR f.IdChucDanh = @idChucDanh)
                    AND (@gioiTinh = N'' OR f.GioiTinh = @gioiTinh)
                    AND (@singleKeyWord = N'%%'
                        OR f.TenNhanVien LIKE @singleKeyWord
                        OR f.IdCCCD LIKE @singleKeyWord
                        OR f.DiaChi LIKE @singleKeyWord
                        OR ms.Email LIKE @singleKeyWord
                        OR u.UserName LIKE @singleKeyWord
                        OR u.MobileAlias LIKE @singleKeyWord) -- <--- BỔ SUNG TÌM KIẾM THEO SĐT
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

        public override DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
        DECLARE @startRow        INT          = {pageNumber};
        DECLARE @endRow          INT          = {pageSize};
        DECLARE @tenNhanVien     NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.TenNhanVien) ? parameters[TblNhanVien.Columns.TenNhanVien] : string.Empty)}%';
        DECLARE @IdCCCD          VARCHAR(20)   = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.IdCCCD) ? parameters[TblNhanVien.Columns.IdCCCD] : string.Empty)}%';
        DECLARE @diaChi          NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.DiaChi) ? parameters[TblNhanVien.Columns.DiaChi] : string.Empty)}%';
        DECLARE @email           NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("Email") ? parameters["Email"] : string.Empty)}%';
        DECLARE @userName        NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("UserName") ? parameters["UserName"] : string.Empty)}%';
        DECLARE @gioiTinh        NVARCHAR(10)  = N'{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.GioiTinh) ? parameters[TblNhanVien.Columns.GioiTinh] : string.Empty)}';
        DECLARE @idPhongBan      VARCHAR(36)   = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.IdPhongBan) ? parameters[TblNhanVien.Columns.IdPhongBan] : Guid.Empty)}';
        DECLARE @idChucDanh      VARCHAR(36)   = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(TblNhanVien.Columns.IdChucDanh) ? parameters[TblNhanVien.Columns.IdChucDanh] : Guid.Empty)}';
        DECLARE @ngayGiaNhapTu   VARCHAR(50)   = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("NgayGiaNhapTu") ? parameters["NgayGiaNhapTu"] : string.Empty)}';
        DECLARE @ngayGiaNhapDen  VARCHAR(50)   = '{InlineQueryHelpers.SQLEncode(parameters.ContainsKey("NgayGiaNhapDen") ? parameters["NgayGiaNhapDen"] : string.Empty)}';
        DECLARE @phoneNumber     NVARCHAR(50) = N'%{InlineQueryHelpers.SQLEncode(parameters.ContainsKey(AspnetUser.Columns.MobileAlias) ? parameters[AspnetUser.Columns.MobileAlias] : string.Empty)}%';
         
        SELECT * FROM (
            SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                SELECT f.*
                    , u.UserName
                    , ms.Email
                    , u.MobileAlias AS PhoneNumber -- <--- BỔ SUNG CỘT SỐ ĐIỆN THOẠI Ở ĐÂY (ĐÃ CÓ SẴN Ở HÀM 3 NHƯNG ĐẢM BẢO CHUẨN ĐỒNG BỘ)
                    , pb.TenPhongBan
                    , cd.TenChucDanh
                    , COUNT(1) OVER() AS total_records
                FROM TblNhanVien f
                LEFT JOIN aspnet_Users u ON u.UserId = f.UserId
                LEFT JOIN aspnet_Membership ms ON ms.UserId = u.UserId
                LEFT JOIN TblPhongBan pb ON pb.IdPhongBan = f.IdPhongBan
                LEFT JOIN TblChucDanh cd ON cd.IdChucDanh = f.IdChucDanh
                WHERE f.DaXoa = 0
                    AND (@tenNhanVien = N'%%' OR f.TenNhanVien LIKE @tenNhanVien)
                    AND (@IdCCCD = N'%%' OR f.IdCCCD LIKE @IdCCCD)
                    AND (@diaChi = N'%%' OR f.DiaChi LIKE @diaChi)
                    AND (@email = N'%%' OR ms.Email LIKE @email)
                    AND (@phoneNumber = N'%%' OR u.MobileAlias LIKE @phoneNumber)
                    AND (@userName = N'%%' OR u.UserName LIKE @userName)
                    AND (@gioiTinh = N'' OR f.GioiTinh = @gioiTinh)
                    AND (@idPhongBan = '{Guid.Empty}' OR @idPhongBan = '' OR f.IdPhongBan = @idPhongBan)
                    AND (@idChucDanh = '{Guid.Empty}' OR @idChucDanh = '' OR f.IdChucDanh = @idChucDanh)
                    AND (@ngayGiaNhapTu = '' OR @ngayGiaNhapDen = '' OR f.NgayGiaNhap BETWEEN @ngayGiaNhapTu AND @ngayGiaNhapDen)
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
        #endregion

        #region CRUD Overrides
        public override TblNhanVien Insert(TblNhanVien nhanVien)
        {
            if (nhanVien == null)
                return null;
            nhanVien.IsNew = true; //ép subsonic hiểu đây là lệnh insert
            nhanVien.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(null, nhanVien, _tableName, nhanVien.IdNhanVien, "INSERT").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log insert for TblNhanVien");
                }
            });
            return nhanVien;
        }
        public override TblNhanVien Update(TblNhanVien nhanVien)
        {
            if (nhanVien == null)
                return null;

            var id = nhanVien.IdNhanVien;
            TblNhanVien nhanvienOld = GetById(id);
            nhanVien.NgayCapNhat = DateTime.Now;
            nhanVien.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(nhanvienOld, nhanVien, _tableName, id, string.Empty).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblNhanVien");
                }
            });
            return nhanVien;
        }

        public override bool Delete(TblNhanVien nhanVien)
        {
            if (nhanVien == null)
                return false;

            var id = nhanVien.IdNhanVien;
            TblNhanVien nhanvienOld = GetById(id);

            nhanVien.DaXoa = true;
            nhanVien.NgayCapNhat = DateTime.Now;
            nhanVien.Save();

            Task.Run(async () => //tạo luồng chạy ngầm tách biệt với luồng chính của giao diện, nói chung để hiện tbao xóa thành công mà khỏi chờ ghi log vô audit
            {
                try
                {
                    await _auditManager.LogChangesAsync(nhanvienOld, nhanVien, _tableName, id, string.Empty).ConfigureAwait(false);//nhận sự thay đổi giứa 2 thk nhân viên thuộc cùng bản ghi r ghi lại sự thay đổi, config là kĩ thuật ngăn tắc luồng
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log delete for TblNhanVien");
                }
            });
            return true;
        }

        #endregion

        #region Single Fetchers & Checks

        public DataTable GetNhanVienForDetail(Guid idNhanVien)
        {
            string sql = $@"
        SELECT f.*
             , pb.TenPhongBan
             , cd.TenChucDanh
             , u.MobileAlias AS PhoneNumber
             , u.Username
             , m.Email
        FROM TblNhanVien f
        LEFT JOIN TblPhongBan pb ON f.IdPhongBan = pb.IdPhongBan
        LEFT JOIN TblChucDanh cd ON f.IdChucDanh = cd.IdChucDanh
        LEFT JOIN aspnet_Users u ON f.UserId = u.UserId
        LEFT JOIN aspnet_Membership m ON f.UserId = m.UserId
        WHERE f.IdNhanVien = '{idNhanVien}' AND f.DaXoa = 0";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
        }
        public TblNhanVien GetByIdUser(Guid userId)
        {
            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.UserIdColumn).IsEqualTo(userId)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblNhanVien>();
        }

        public TblNhanVien GetByCCCD(string maCCCD)
        {
            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.IdCCCDColumn).IsEqualTo(maCCCD)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblNhanVien>();
        }

        public string GetTenNhanVienById(Guid id)
        {
            return new Select(TblNhanVien.TenNhanVienColumn)
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.IdNhanVienColumn).IsEqualTo(id)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteScalar<string>();
        }

        public bool IsCCCDExist(Guid id, string maCCCD)
        {
            Select select = new Select();
            select.From(TblNhanVien.Schema)
                .Where(TblNhanVien.IdCCCDColumn).IsEqualTo(maCCCD)
                .And(TblNhanVien.IdNhanVienColumn).IsNotEqualTo(id)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false);
            return select.GetRecordCount() > 0;
        }

        public bool IsUserAssigned(Guid id, Guid userId)
        {
            Select select = new Select();
            select.From(TblNhanVien.Schema)
                .Where(TblNhanVien.UserIdColumn).IsEqualTo(userId)
                .And(TblNhanVien.IdNhanVienColumn).IsNotEqualTo(id)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false);
            return select.GetRecordCount() > 0;
        }

        #endregion

        #region List Fetchers

        public List<TblNhanVien> GetAllActive()
        {
            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblNhanVien>();
        }

        public List<TblNhanVien> GetByPhongBan(Guid idPhongBan)
        {
            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.IdPhongBanColumn).IsEqualTo(idPhongBan)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblNhanVien>();
        }

        public List<TblNhanVien> GetByChucDanh(Guid idChucDanh)
        {
            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.IdChucDanhColumn).IsEqualTo(idChucDanh)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblNhanVien>();
        }

        #endregion
    }
}
