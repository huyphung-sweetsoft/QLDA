using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SweetSoft.QLDA.Core.Utils;
using System.Data;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DocumentTypeRepository : BaseRepository<TblLoaiTaiLieu>
    {
        public DocumentTypeRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        public Guid? GetDefaultStorageLocation(Guid typeId)
        {
            var command = new QueryCommand("SELECT IdNoiLuuTruMacDinh FROM dbo.TblLoaiTaiLieu WHERE IdLoaiTaiLieu=@Id AND DaXoa=0", TblLoaiTaiLieu.Schema.Provider.Name);
            command.Parameters.Add("@Id", typeId, DbType.Guid);
            object value = DataService.ExecuteScalar(command);
            return value == null || value == DBNull.Value ? (Guid?)null : (Guid)value;
        }

        // Parameterized SQL handles nullable legacy group and the additive default-location
        // column without hand-editing generated SubSonic classes.
        public TblLoaiTaiLieu SaveIndependentType(TblLoaiTaiLieu item, bool isNew, Guid? defaultLocation)
        {
            using (var scope = new TransactionScope())
            {
                var command = new QueryCommand(@"
                    DECLARE @PreviousName nvarchar(150);
                    SELECT @PreviousName=TenLoai FROM dbo.TblLoaiTaiLieu WITH(UPDLOCK,HOLDLOCK)
                      WHERE IdLoaiTaiLieu=@Id AND DaXoa=0;
                    IF @IsNew=0 AND @PreviousName IS NULL
                        THROW 51000,N'Loại hồ sơ không còn tồn tại.',1;
                    IF (@IsNew=1 OR @PreviousName<>@Name) AND EXISTS
                      (SELECT 1 FROM dbo.TblLoaiTaiLieu WITH(UPDLOCK,HOLDLOCK)
                       WHERE TenLoai=@Name AND DaXoa=0 AND IdLoaiTaiLieu<>@Id)
                        THROW 51000,N'Tên loại hồ sơ đã tồn tại.',1;
                    IF @Location IS NOT NULL AND NOT EXISTS
                      (SELECT 1 FROM dbo.TblNoiLuuTru WITH(UPDLOCK,HOLDLOCK)
                       WHERE IdNoiLuuTru=@Location AND DaXoa=0 AND KichHoat=1)
                        THROW 51000,N'Nơi lưu mặc định không tồn tại hoặc đã khóa.',1;
                    IF @IsNew=1
                      INSERT dbo.TblLoaiTaiLieu
                       (IdLoaiTaiLieu,IdNhomTaiLieu,TenLoai,MoTa,CanTrinhKy,HinhThucKyMacDinh,
                        CanGuiKhachHang,CanLuuVatLy,ThuTuHienThi,KichHoat,DaXoa,NguoiTao,NgayTao,IdNoiLuuTruMacDinh)
                      VALUES(@Id,NULL,@Name,@Description,@Signing,@Method,@Customer,@Physical,
                        @Order,@Active,0,@User,@Now,@Location);
                    ELSE UPDATE dbo.TblLoaiTaiLieu SET TenLoai=@Name,MoTa=@Description,
                        CanTrinhKy=@Signing,HinhThucKyMacDinh=@Method,CanGuiKhachHang=@Customer,
                        CanLuuVatLy=@Physical,ThuTuHienThi=@Order,KichHoat=@Active,
                        NguoiCapNhat=@User,NgayCapNhat=@Now,IdNoiLuuTruMacDinh=@Location
                      WHERE IdLoaiTaiLieu=@Id AND DaXoa=0;", TblLoaiTaiLieu.Schema.Provider.Name);
                command.Parameters.Add("@Id", item.IdLoaiTaiLieu, DbType.Guid);
                command.Parameters.Add("@IsNew", isNew, DbType.Boolean);
                command.Parameters.Add("@Name", item.TenLoai, DbType.String);
                command.Parameters.Add("@Description", item.MoTa, DbType.String);
                command.Parameters.Add("@Signing", item.CanTrinhKy, DbType.Boolean);
                command.Parameters.Add("@Method", (object)item.HinhThucKyMacDinh ?? DBNull.Value, DbType.String);
                command.Parameters.Add("@Customer", item.CanGuiKhachHang, DbType.Boolean);
                command.Parameters.Add("@Physical", item.CanLuuVatLy, DbType.Boolean);
                command.Parameters.Add("@Order", item.ThuTuHienThi, DbType.Int32);
                command.Parameters.Add("@Active", item.KichHoat, DbType.Boolean);
                command.Parameters.Add("@User", isNew ? item.NguoiTao : item.NguoiCapNhat, DbType.String);
                command.Parameters.Add("@Now", DateTime.UtcNow, DbType.DateTime);
                command.Parameters.Add("@Location", (object)defaultLocation ?? DBNull.Value, DbType.Guid);
                DataService.ExecuteQuery(command);
                scope.Complete();
            }
            // Avoid serializing the old non-nullable generated group property for NULL rows.
            if (_auditManager != null)
                Task.Run(async () => {
                    try {
                        await _auditManager.LogActionAsync(isNew ? LogActions.Actions.CREATE : LogActions.Actions.UPDATE,
                            new { item.IdLoaiTaiLieu, item.TenLoai, item.MoTa, item.CanTrinhKy,
                                item.HinhThucKyMacDinh, item.CanGuiKhachHang, item.CanLuuVatLy,
                                item.ThuTuHienThi, item.KichHoat, IdNoiLuuTruMacDinh = defaultLocation },
                            "TblLoaiTaiLieu", item.IdLoaiTaiLieu).ConfigureAwait(false);
                    } catch(Exception ex) { SysLogger.LogError(ex,"Failed to audit independent document type"); }
                });
            return GetById(item.IdLoaiTaiLieu);
        }

        public List<TblLoaiTaiLieu> GetAll(
            string keyword = null,
            Guid? idNhomTaiLieu = null)
        {
            SqlQuery query = new Select()
                .From(TblLoaiTaiLieu.Schema)
                .Where(TblLoaiTaiLieu.DaXoaColumn).IsEqualTo(false);

            if (idNhomTaiLieu.HasValue
                && idNhomTaiLieu.Value != Guid.Empty)
            {
                query.And(TblLoaiTaiLieu.IdNhomTaiLieuColumn).IsEqualTo(idNhomTaiLieu.Value);
            }

            List<TblLoaiTaiLieu> items = query.ExecuteTypedList<TblLoaiTaiLieu>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchValue = keyword.Trim();

                items = items.Where(item =>
                        ContainsIgnoreCase(item.TenLoai, searchValue)
                        || ContainsIgnoreCase(item.MoTa, searchValue))
                    .ToList();
            }

            return items
                .OrderBy(item => item.ThuTuHienThi)
                .ThenBy(item => item.TenLoai)
                .ToList();
        }
        public DataTable SearchPaging(
    string searchTerm,
    Dictionary<string, object> parameters,
    string orderBy,
    int rowOffset,
    int endRow,
    out int totalRecord)
        {
            totalRecord = 0;

            if (parameters == null)
            {
                parameters =
                    new Dictionary<string, object>();
            }

            int safeOffset = Math.Max(0, rowOffset);

            int safeEndRow =
                Math.Max(safeOffset + 1, endRow);

            // Từ khóa của ô tìm kiếm nhanh.
            string keyword =
                InlineQueryHelpers.SQLEncode(
                    searchTerm,
                    500);

            // Điều kiện của tìm kiếm nâng cao.
            string tenLoai =
                InlineQueryHelpers.SQLEncode(
                    GetParameterText(
                        parameters,
                        TblLoaiTaiLieu.Columns.TenLoai),
                    150);

            string moTa =
                InlineQueryHelpers.SQLEncode(
                    GetParameterText(
                        parameters,
                        TblLoaiTaiLieu.Columns.MoTa),
                    500);

            string hinhThucKyMacDinh =
                InlineQueryHelpers.SQLEncode(
                    GetParameterText(
                        parameters,
                        TblLoaiTaiLieu.Columns
                            .HinhThucKyMacDinh),
                    20);

            // Điều kiện GUID.
            Guid idNhomTaiLieu = Guid.Empty; // Retired group filters must not hide independent types.

            string idNhomTaiLieuSql =
                idNhomTaiLieu == Guid.Empty
                    ? "NULL"
                    : "'" + idNhomTaiLieu + "'";

            // Điều kiện boolean.
            string kichHoatSql =
                GetNullableBitSql(
                    parameters,
                    TblLoaiTaiLieu.Columns.KichHoat);

            string canTrinhKySql =
                GetNullableBitSql(
                    parameters,
                    TblLoaiTaiLieu.Columns.CanTrinhKy);

            string canGuiKhachHangSql =
                GetNullableBitSql(
                    parameters,
                    TblLoaiTaiLieu.Columns
                        .CanGuiKhachHang);

            string canLuuVatLySql =
                GetNullableBitSql(
                    parameters,
                    TblLoaiTaiLieu.Columns
                        .CanLuuVatLy);

            // Không sử dụng trực tiếp orderBy truyền vào SQL.
            string safeOrderBy =
                GetSafeOrderBy(orderBy);

            string sql = $@"
        DECLARE @offset INT = {safeOffset};
        DECLARE @endRow INT = {safeEndRow};

        DECLARE @keyword NVARCHAR(500)
            = N'%{keyword}%';

        DECLARE @tenLoai NVARCHAR(150)
            = N'%{tenLoai}%';

        DECLARE @moTa NVARCHAR(500)
            = N'%{moTa}%';

        DECLARE @hinhThucKyMacDinh VARCHAR(20)
            = '{hinhThucKyMacDinh}';

        DECLARE @idNhomTaiLieu UNIQUEIDENTIFIER
            = {idNhomTaiLieuSql};

        DECLARE @kichHoat BIT
            = {kichHoatSql};

        DECLARE @canTrinhKy BIT
            = {canTrinhKySql};

        DECLARE @canGuiKhachHang BIT
            = {canGuiKhachHangSql};

        DECLARE @canLuuVatLy BIT
            = {canLuuVatLySql};

        ;WITH SearchResult AS
        (
            SELECT
                ROW_NUMBER() OVER
                (
                    ORDER BY {safeOrderBy}
                ) AS RowNum,

                f.IdLoaiTaiLieu,
                f.IdNhomTaiLieu,
                f.TenLoai,
                f.MoTa,
                f.CanTrinhKy,
                f.HinhThucKyMacDinh,
                f.CanGuiKhachHang,
                f.CanLuuVatLy,
                f.ThuTuHienThi,
                f.KichHoat,
                f.NguoiTao,
                f.NgayTao,
                f.NguoiCapNhat,
                f.NgayCapNhat,

                ISNULL(
                    n.TenNhom,
                    N''
                ) AS TenNhom,

                COUNT(1) OVER()
                    AS total_records

            FROM TblLoaiTaiLieu f

            LEFT JOIN TblNhomTaiLieu n
                ON n.IdNhomTaiLieu
                    = f.IdNhomTaiLieu
                AND n.DaXoa = 0

            WHERE f.DaXoa = 0

                AND
                (
                    @keyword = N'%%'
                    OR f.TenLoai LIKE @keyword
                    OR ISNULL(f.MoTa, N'')
                        LIKE @keyword
                )

                AND
                (
                    @idNhomTaiLieu IS NULL
                    OR f.IdNhomTaiLieu
                        = @idNhomTaiLieu
                )

                AND
                (
                    @kichHoat IS NULL
                    OR f.KichHoat = @kichHoat
                )

                AND
                (
                    @tenLoai = N'%%'
                    OR f.TenLoai LIKE @tenLoai
                )

                AND
                (
                    @moTa = N'%%'
                    OR ISNULL(f.MoTa, N'')
                        LIKE @moTa
                )

                AND
                (
                    @canTrinhKy IS NULL
                    OR f.CanTrinhKy
                        = @canTrinhKy
                )

                AND
                (
                    @hinhThucKyMacDinh = ''
                    OR ISNULL(
                        f.HinhThucKyMacDinh,
                        ''
                    ) = @hinhThucKyMacDinh
                )

                AND
                (
                    @canGuiKhachHang IS NULL
                    OR f.CanGuiKhachHang
                        = @canGuiKhachHang
                )

                AND
                (
                    @canLuuVatLy IS NULL
                    OR f.CanLuuVatLy
                        = @canLuuVatLy
                )
        )

        SELECT *
        FROM SearchResult
        WHERE RowNum > @offset
          AND RowNum <= @endRow
        ORDER BY RowNum;
    ";

            IDataReader reader =
                new InlineQuery()
                    .ExecuteReader(sql);

            if (reader == null)
                return null;

            DataTable dataTable =
                new DataTable();

            dataTable.Load(reader);

            InlineQueryHelpers.GetTotal(
                ref dataTable,
                out totalRecord);

            return dataTable;
        }
        public override DataTable SearchPaging(
    Dictionary<string, object> parameters,
    string orderBy,
    int rowOffset,
    int endRow,
    out int totalRecord)
        {
            return SearchPaging(
                string.Empty,
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        public override TblLoaiTaiLieu GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblLoaiTaiLieu.Schema)
                .Where(TblLoaiTaiLieu.IdLoaiTaiLieuColumn).IsEqualTo(id)
                .And(TblLoaiTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblLoaiTaiLieu>();
        }

        public bool IsNameExisted(
            string name,
            Guid idNhomTaiLieu,
            Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            Select select = new Select();
            select.From(TblLoaiTaiLieu.Schema);
            select.Where(TblLoaiTaiLieu.TenLoaiColumn)
                .IsEqualTo(name.Trim());
            select.And(TblLoaiTaiLieu.DaXoaColumn)
                .IsEqualTo(false);

            if (excludeId != Guid.Empty)
            {
                select.And(TblLoaiTaiLieu.IdLoaiTaiLieuColumn)
                    .IsNotEqualTo(excludeId);
            }

            return select.GetRecordCount() > 0;
        }


        public bool IsInUse(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            int documentCount = new Select()
                .From(TblTaiLieu.Schema)
                .Where(TblTaiLieu.IdLoaiTaiLieuColumn).IsEqualTo(id)
                .And(TblTaiLieu.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount();

            if (documentCount > 0)
                return true;

            return new Select()
                .From(TblMauTaiLieu.Schema)
                .Where(TblMauTaiLieu.IdLoaiTaiLieuColumn).IsEqualTo(id)
                .And(TblMauTaiLieu.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        public override TblLoaiTaiLieu Insert(TblLoaiTaiLieu item)
        {
            if (item == null)
                return null;

            item.Save();
            LogCreate(item);
            return item;
        }

        public override TblLoaiTaiLieu Update(TblLoaiTaiLieu itemNew)
        {
            if (itemNew == null)
                return null;

            TblLoaiTaiLieu itemOld = GetById(itemNew.IdLoaiTaiLieu);
            itemNew.Save();
            LogUpdate(itemOld, itemNew);
            return itemNew;
        }

        public override bool Delete(TblLoaiTaiLieu item)
        {
            if (item == null)
                return false;

            var command = new QueryCommand("UPDATE dbo.TblLoaiTaiLieu SET DaXoa=1, NguoiCapNhat=@User, NgayCapNhat=@Now WHERE IdLoaiTaiLieu=@Id AND DaXoa=0", TblLoaiTaiLieu.Schema.Provider.Name);
            command.Parameters.Add("@Id",item.IdLoaiTaiLieu,DbType.Guid);
            command.Parameters.Add("@User",item.NguoiCapNhat,DbType.String);
            command.Parameters.Add("@Now",DateTime.UtcNow,DbType.DateTime);
            DataService.ExecuteQuery(command);
            item.DaXoa = true;
            LogDelete(item);
            return true;
        }

        private void LogCreate(TblLoaiTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.CREATE,
                            item,
                            _tableName,
                            item.IdLoaiTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log CREATE action for TblLoaiTaiLieu");
                }
            });
        }

        private void LogUpdate(
            TblLoaiTaiLieu itemOld,
            TblLoaiTaiLieu itemNew)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                            itemOld,
                            itemNew,
                            _tableName,
                            itemNew.IdLoaiTaiLieu,
                            itemNew.NguoiCapNhat ?? string.Empty)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log changes for TblLoaiTaiLieu");
                }
            });
        }

        private void LogDelete(TblLoaiTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.DELETE,
                            new { item.IdLoaiTaiLieu, item.TenLoai, item.DaXoa, item.NguoiCapNhat },
                            _tableName,
                            item.IdLoaiTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log DELETE action for TblLoaiTaiLieu");
                }
            });
        }
        private static string GetParameterText(
    Dictionary<string, object> parameters,
    string key)
        {
            if (parameters == null
                || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            object value;

            if (!parameters.TryGetValue(
                    key,
                    out value)
                || value == null)
            {
                return string.Empty;
            }

            string result =
                value.ToString().Trim();

            if (string.Equals(
                    result,
                    "null",
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return result;
        }
        private static Guid GetGuidParameter(
    Dictionary<string, object> parameters,
    string key)
        {
            string value =
                GetParameterText(
                    parameters,
                    key);

            Guid result;

            if (!Guid.TryParse(
                    value,
                    out result))
            {
                return Guid.Empty;
            }

            return result;
        }
        private static string GetNullableBitSql(
    Dictionary<string, object> parameters,
    string key)
        {
            string value =
                GetParameterText(
                    parameters,
                    key);

            if (string.Equals(
                    value,
                    "1",
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    value,
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "1";
            }

            if (string.Equals(
                    value,
                    "0",
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    value,
                    "false",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "0";
            }

            return "NULL";
        }
        private static string GetSafeOrderBy(
    string orderBy)
        {
            string columnName = string.Empty;
            string direction = "ASC";

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                string[] parts =
                    orderBy.Trim().Split(
                        new[] { ' ' },
                        StringSplitOptions
                            .RemoveEmptyEntries);

                if (parts.Length > 0)
                    columnName = parts[0];

                if (parts.Length > 1
                    && string.Equals(
                        parts[1],
                        "DESC",
                        StringComparison.OrdinalIgnoreCase))
                {
                    direction = "DESC";
                }
            }

            string sqlColumn;

            switch (columnName)
            {
                case "TenLoai":
                    sqlColumn = "f.TenLoai";
                    break;

                case "TenNhom":
                    sqlColumn = "n.TenNhom";
                    break;

                case "CanTrinhKy":
                    sqlColumn = "f.CanTrinhKy";
                    break;

                case "HinhThucKyMacDinh":
                    sqlColumn =
                        "f.HinhThucKyMacDinh";
                    break;

                case "CanGuiKhachHang":
                    sqlColumn =
                        "f.CanGuiKhachHang";
                    break;

                case "CanLuuVatLy":
                    sqlColumn =
                        "f.CanLuuVatLy";
                    break;

                case "KichHoat":
                    sqlColumn = "f.KichHoat";
                    break;

                case "NgayTao":
                    sqlColumn = "f.NgayTao";
                    break;

                case "ThuTuHienThi":
                    sqlColumn =
                        "f.ThuTuHienThi";
                    break;

                default:
                    return
                        "f.ThuTuHienThi ASC, "
                        + "f.TenLoai ASC, "
                        + "f.IdLoaiTaiLieu ASC";
            }

            return sqlColumn
                + " "
                + direction
                + ", f.IdLoaiTaiLieu ASC";
        }
        private static bool ContainsIgnoreCase(
            string source,
            string searchValue)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            return source.IndexOf(
                searchValue,
                StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}
