param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\SweetSoft.QLDA.DataAccess')
)

$ErrorActionPreference = 'Stop'

function New-Column {
    param(
        [string]$Name,
        [string]$ClrType,
        [string]$DbType,
        [int]$MaxLength,
        [bool]$Nullable,
        [bool]$PrimaryKey = $false,
        [string]$Default = '',
        [bool]$ForeignKey = $false,
        [string]$ForeignTable = '',
        [string]$DatabaseName = ''
    )

    [pscustomobject]@{
        Name = $Name
        DatabaseName = $(if ($DatabaseName) { $DatabaseName } else { $Name })
        ClrType = $ClrType
        DbType = $DbType
        MaxLength = $MaxLength
        Nullable = $Nullable
        PrimaryKey = $PrimaryKey
        Default = $Default
        ForeignKey = $ForeignKey
        ForeignTable = $ForeignTable
    }
}

function New-Table {
    param(
        [string]$ClassName,
        [string]$TableName,
        [object[]]$Columns
    )

    [pscustomobject]@{
        ClassName = $ClassName
        TableName = $TableName
        Columns = $Columns
    }
}

$tables = @(
    (New-Table 'TblKhachHang' 'TblKhachHang' @(
        (New-Column 'IdKhachHang' 'Guid' 'Guid' 0 $false $true '(newid())')
        (New-Column 'TenKhachHang' 'string' 'String' 250 $false)
        (New-Column 'IdLoaiKhachHang' 'Guid' 'Guid' 0 $false $false '' $true 'TblLoaiKhachHang')
        (New-Column 'IdSoThue' 'string' 'AnsiString' 50 $true)
        (New-Column 'SoDienThoai' 'string' 'AnsiString' 30 $true)
        (New-Column 'ThuDienTu' 'string' 'String' 256 $true)
        (New-Column 'DiaChi' 'string' 'String' 500 $true)
        (New-Column 'TenNguoiDaiDien' 'string' 'String' 250 $true)
        (New-Column 'TenNguoiLienHe' 'string' 'String' 250 $true)
        (New-Column 'DienThoaiLienHe' 'string' 'AnsiString' 30 $true)
        (New-Column 'Email' 'string' 'String' 256 $true)
        (New-Column 'GhiChu' 'string' 'String' 1000 $true)
        (New-Column 'KichHoat' 'bool' 'Boolean' 0 $false $false '((1))')
        (New-Column 'DaXoa' 'bool' 'Boolean' 0 $false $false '((0))')
        (New-Column 'NguoiTao' 'string' 'String' 150 $false)
        (New-Column 'NgayTao' 'DateTime' 'DateTime' 0 $false $false '(getdate())')
        (New-Column 'NguoiCapNhat' 'string' 'String' 150 $true)
        (New-Column 'NgayCapNhat' 'DateTime?' 'DateTime' 0 $true)
    ))
    (New-Table 'TblLichHop' 'TblLichHop' @(
        (New-Column 'IdLichHop' 'Guid' 'Guid' 0 $false $true '(newid())')
        (New-Column 'IdDuAn' 'Guid' 'Guid' 0 $false $false '' $true 'TblDuAn')
        (New-Column 'MaLichHop' 'string' 'AnsiString' 50 $true)
        (New-Column 'TenCuocHop' 'string' 'String' 255 $false)
        (New-Column 'NoiDungCuocHop' 'string' 'String' -1 $true)
        (New-Column 'ThoiGianBatDau' 'DateTime' 'DateTime' 0 $false)
        (New-Column 'ThoiGianKetThuc' 'DateTime' 'DateTime' 0 $false)
        (New-Column 'DiaDiemHop' 'string' 'String' 255 $false)
        (New-Column 'TrangThai' 'byte' 'Byte' 0 $false $false '((1))')
        (New-Column 'DaXoa' 'bool' 'Boolean' 0 $false $false '((0))')
        (New-Column 'IdNguoiTao' 'Guid' 'Guid' 0 $false)
        (New-Column 'NgayTao' 'DateTime' 'DateTime' 0 $false $false '(getdate())')
        (New-Column 'IdNguoiCapNhat' 'Guid?' 'Guid' 0 $true)
        (New-Column 'NgayCapNhat' 'DateTime?' 'DateTime' 0 $true)
    ))
    (New-Table 'TblRuiRo' 'TblRuiRo' @(
        (New-Column 'IdRuiRo' 'Guid' 'Guid' 0 $false $true)
        (New-Column 'TenRuiRo' 'string' 'String' 255 $false)
    ))
    (New-Table 'TblRuiRoDuAn' 'TblRuiRo_DuAn' @(
        (New-Column 'IdRuiRoDuAn' 'Guid' 'Guid' 0 $false $true '(newid())' $false '' 'IdRuiRo_DuAn')
        (New-Column 'IdDuAn' 'Guid' 'Guid' 0 $false $false '' $true 'TblDuAn')
        (New-Column 'IdNhanVienXuLy' 'Guid?' 'Guid' 0 $true $false '' $true 'aspnet_Users')
        (New-Column 'TenRuiRo' 'string' 'String' 255 $false)
        (New-Column 'XacSuatXayRa' 'int?' 'Int32' 0 $true)
        (New-Column 'MucDoAnhHuong' 'int?' 'Int32' 0 $true)
        (New-Column 'DiemRuiRo' 'float?' 'Single' 0 $true)
        (New-Column 'KeHoachPhongNgua' 'string' 'String' -1 $true)
        (New-Column 'KeHoachUngPho' 'string' 'String' -1 $true)
        (New-Column 'DaXoa' 'bool' 'Boolean' 0 $false $false '((0))')
        (New-Column 'NguoiTao' 'string' 'String' 150 $false)
        (New-Column 'NgayTao' 'DateTime' 'DateTime' 0 $false $false '(getdate())')
        (New-Column 'NguoiCapNhat' 'string' 'String' 150 $true)
        (New-Column 'NgayCapNhat' 'DateTime?' 'DateTime' 0 $true)
    ))
    (New-Table 'TblVanDe' 'TblVanDe' @(
        (New-Column 'IdVanDe' 'Guid' 'Guid' 0 $false $true '(newid())')
        (New-Column 'IdDuAn' 'Guid' 'Guid' 0 $false $false '' $true 'TblDuAn')
        (New-Column 'IdCongViecBiAnhHuong' 'Guid?' 'Guid' 0 $true $false '' $true 'TblCongViec')
        (New-Column 'TenVanDe' 'string' 'String' 255 $false)
        (New-Column 'MoTaChiTiet' 'string' 'String' -1 $true)
        (New-Column 'MucDoAnhHuong' 'int?' 'Int32' 0 $true)
        (New-Column 'KeHoachXuLy' 'string' 'String' -1 $true)
        (New-Column 'TrangThai' 'byte' 'Byte' 0 $false $false '((0))')
        (New-Column 'DaXoa' 'bool' 'Boolean' 0 $false $false '((0))')
        (New-Column 'NguoiTao' 'string' 'String' 150 $false)
        (New-Column 'NgayTao' 'DateTime' 'DateTime' 0 $false $false '(getdate())')
        (New-Column 'NguoiCapNhat' 'string' 'String' 150 $true)
        (New-Column 'NgayCapNhat' 'DateTime?' 'DateTime' 0 $true)
        (New-Column 'MaVanDe' 'string' 'AnsiString' 50 $true)
        (New-Column 'IdCongViecPhatSinh' 'Guid?' 'Guid' 0 $true $false '' $true 'TblCongViec')
        (New-Column 'NguonGocVanDe' 'int?' 'Int32' 0 $true)
    ))
    (New-Table 'TblVanDeNhanVien' 'TblVanDe_NhanVien' @(
        (New-Column 'IdVanDe' 'Guid' 'Guid' 0 $false $true)
        (New-Column 'IdNhanVien' 'Guid' 'Guid' 0 $false $true)
    ))
    (New-Table 'TblCauHinhTuanLamViec' 'TblCauHinhTuanLamViec' @(
        (New-Column 'IdCauHinh' 'Guid' 'Guid' 0 $false $true '(newid())')
        (New-Column 'NgayTrongTuan' 'byte' 'Byte' 0 $false)
        (New-Column 'LaNgayLamViec' 'bool' 'Boolean' 0 $false)
        (New-Column 'GioBatDauSang' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'GioKetThucSang' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'GioBatDauChieu' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'GioKetThucChieu' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'NguoiTao' 'string' 'AnsiString' 50 $true)
        (New-Column 'NgayTao' 'DateTime?' 'DateTime' 0 $true)
        (New-Column 'NguoiCapNhat' 'string' 'AnsiString' 50 $true)
        (New-Column 'NgayCapNhat' 'DateTime?' 'DateTime' 0 $true)
    ))
    (New-Table 'TblLichNgoaiLe' 'TblLichNgoaiLe' @(
        (New-Column 'IdNgoaiLe' 'Guid' 'Guid' 0 $false $true '(newid())')
        (New-Column 'TenNgoaiLe' 'string' 'String' 255 $false)
        (New-Column 'NgayBatDau' 'DateTime' 'Date' 0 $false)
        (New-Column 'NgayKetThuc' 'DateTime' 'Date' 0 $false)
        (New-Column 'LaNgayLamViec' 'bool' 'Boolean' 0 $false)
        (New-Column 'GioBatDauSang' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'GioKetThucSang' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'GioBatDauChieu' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'GioKetThucChieu' 'TimeSpan?' 'Time' 0 $true)
        (New-Column 'MoTa' 'string' 'String' -1 $true)
        (New-Column 'DaXoa' 'bool' 'Boolean' 0 $false $false '((0))')
        (New-Column 'NguoiTao' 'string' 'AnsiString' 50 $true)
        (New-Column 'NgayTao' 'DateTime?' 'DateTime' 0 $true)
        (New-Column 'NguoiCapNhat' 'string' 'AnsiString' 50 $true)
        (New-Column 'NgayCapNhat' 'DateTime?' 'DateTime' 0 $true)
    ))
)

function Add-Line {
    param([System.Text.StringBuilder]$Builder, [string]$Text = '')
    [void]$Builder.AppendLine($Text)
}

function Get-ParameterList {
    param([object[]]$Columns, [string]$Prefix = '')
    ($Columns | ForEach-Object { $_.ClrType + ' ' + $Prefix + $_.Name }) -join ','
}

function Write-Model {
    param([object]$Table)

    $className = $Table.ClassName
    $builder = [System.Text.StringBuilder]::new()
    Add-Line $builder 'using System;'
    Add-Line $builder 'using System.ComponentModel;'
    Add-Line $builder 'using System.Data;'
    Add-Line $builder 'using System.Xml.Serialization;'
    Add-Line $builder 'using SubSonic;'
    Add-Line $builder '// <auto-generated />'
    Add-Line $builder 'namespace SweetSoft.QLDA.DataAccess'
    Add-Line $builder '{'
    Add-Line $builder '    [Serializable]'
    Add-Line $builder "    public partial class ${className}Collection : ActiveList<${className}, ${className}Collection>"
    Add-Line $builder '    {'
    Add-Line $builder "        public ${className}Collection() { }"
    Add-Line $builder '    }'
    Add-Line $builder ''
    Add-Line $builder '    [Serializable]'
    Add-Line $builder "    public partial class $className : ActiveRecord<$className>, IActiveRecord"
    Add-Line $builder '    {'
    Add-Line $builder "        public $className()"
    Add-Line $builder '        {'
    Add-Line $builder '            SetSQLProps();'
    Add-Line $builder '            SetDefaults();'
    Add-Line $builder '            MarkNew();'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder "        public $className(bool useDatabaseDefaults)"
    Add-Line $builder '        {'
    Add-Line $builder '            SetSQLProps();'
    Add-Line $builder '            if (useDatabaseDefaults) ForceDefaults();'
    Add-Line $builder '            MarkNew();'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder "        public $className(object keyID)"
    Add-Line $builder '        {'
    Add-Line $builder '            SetSQLProps();'
    Add-Line $builder '            SetDefaults();'
    Add-Line $builder '            LoadByKey(keyID);'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder "        public $className(string columnName, object columnValue)"
    Add-Line $builder '        {'
    Add-Line $builder '            SetSQLProps();'
    Add-Line $builder '            SetDefaults();'
    Add-Line $builder '            LoadByParam(columnName, columnValue);'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder '        protected static void SetSQLProps() { GetTableSchema(); }'
    Add-Line $builder '        public static Query CreateQuery() { return new Query(Schema); }'
    Add-Line $builder '        public static TableSchema.Table Schema'
    Add-Line $builder '        {'
    Add-Line $builder '            get'
    Add-Line $builder '            {'
    Add-Line $builder '                if (BaseSchema == null) SetSQLProps();'
    Add-Line $builder '                return BaseSchema;'
    Add-Line $builder '            }'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder '        private static void GetTableSchema()'
    Add-Line $builder '        {'
    Add-Line $builder '            if (IsSchemaInitialized) return;'
    Add-Line $builder ('            TableSchema.Table schema = new TableSchema.Table("{0}", TableType.Table, DataService.GetInstance("DataAccessProvider"));' -f $Table.TableName)
    Add-Line $builder '            schema.Columns = new TableSchema.TableColumnCollection();'
    Add-Line $builder '            schema.SchemaName = @"dbo";'
    foreach ($column in $Table.Columns) {
        $varName = 'column' + $column.Name
        Add-Line $builder "            TableSchema.TableColumn $varName = new TableSchema.TableColumn(schema);"
        Add-Line $builder ('            {0}.ColumnName = "{1}";' -f $varName, $column.DatabaseName)
        Add-Line $builder "            $varName.DataType = DbType.$($column.DbType);"
        Add-Line $builder "            $varName.MaxLength = $($column.MaxLength);"
        Add-Line $builder "            $varName.AutoIncrement = false;"
        Add-Line $builder "            $varName.IsNullable = $($column.Nullable.ToString().ToLowerInvariant());"
        Add-Line $builder "            $varName.IsPrimaryKey = $($column.PrimaryKey.ToString().ToLowerInvariant());"
        Add-Line $builder "            $varName.IsForeignKey = $($column.ForeignKey.ToString().ToLowerInvariant());"
        Add-Line $builder "            $varName.IsReadOnly = false;"
        Add-Line $builder ('            {0}.DefaultSetting = @"{1}";' -f $varName, $column.Default)
        Add-Line $builder ('            {0}.ForeignKeyTableName = "{1}";' -f $varName, $column.ForeignTable)
        Add-Line $builder "            schema.Columns.Add($varName);"
    }
    Add-Line $builder '            BaseSchema = schema;'
    Add-Line $builder ('            DataService.Providers["DataAccessProvider"].AddSchema("{0}", schema);' -f $Table.TableName)
    Add-Line $builder '        }'
    Add-Line $builder ''
    foreach ($column in $Table.Columns) {
        Add-Line $builder ('        [XmlAttribute("{0}")]' -f $column.Name)
        Add-Line $builder '        [Bindable(true)]'
        Add-Line $builder "        public $($column.ClrType) $($column.Name)"
        Add-Line $builder '        {'
        Add-Line $builder "            get { return GetColumnValue<$($column.ClrType)>(Columns.$($column.Name)); }"
        Add-Line $builder "            set { SetColumnValue(Columns.$($column.Name), value); }"
        Add-Line $builder '        }'
        Add-Line $builder ''
    }
    $parameterList = Get-ParameterList $Table.Columns 'var'
    Add-Line $builder "        public static void Insert($parameterList)"
    Add-Line $builder '        {'
    Add-Line $builder "            $className item = new $className();"
    foreach ($column in $Table.Columns) {
        Add-Line $builder "            item.$($column.Name) = var$($column.Name);"
    }
    Add-Line $builder '            if (System.Web.HttpContext.Current != null)'
    Add-Line $builder '                item.Save(System.Web.HttpContext.Current.User.Identity.Name);'
    Add-Line $builder '            else'
    Add-Line $builder '                item.Save(System.Threading.Thread.CurrentPrincipal.Identity.Name);'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder "        public static void Update($parameterList)"
    Add-Line $builder '        {'
    Add-Line $builder "            $className item = new $className();"
    foreach ($column in $Table.Columns) {
        Add-Line $builder "            item.$($column.Name) = var$($column.Name);"
    }
    Add-Line $builder '            item.IsNew = false;'
    Add-Line $builder '            if (System.Web.HttpContext.Current != null)'
    Add-Line $builder '                item.Save(System.Web.HttpContext.Current.User.Identity.Name);'
    Add-Line $builder '            else'
    Add-Line $builder '                item.Save(System.Threading.Thread.CurrentPrincipal.Identity.Name);'
    Add-Line $builder '        }'
    Add-Line $builder ''
    for ($index = 0; $index -lt $Table.Columns.Count; $index++) {
        $column = $Table.Columns[$index]
        Add-Line $builder "        public static TableSchema.TableColumn $($column.Name)Column"
        Add-Line $builder '        {'
        Add-Line $builder "            get { return Schema.Columns[$index]; }"
        Add-Line $builder '        }'
        Add-Line $builder ''
    }
    Add-Line $builder '        public struct Columns'
    Add-Line $builder '        {'
    foreach ($column in $Table.Columns) {
        Add-Line $builder ('            public static string {0} = @"{1}";' -f $column.Name, $column.DatabaseName)
    }
    Add-Line $builder '        }'
    Add-Line $builder '    }'
    Add-Line $builder '}'

    $path = Join-Path $OutputDirectory ($className + '.cs')
    [System.IO.File]::WriteAllText($path, $builder.ToString(), [System.Text.UTF8Encoding]::new($false))
}

function Write-Controller {
    param([object]$Table)

    $className = $Table.ClassName
    $primaryKeys = @($Table.Columns | Where-Object PrimaryKey)
    $builder = [System.Text.StringBuilder]::new()
    Add-Line $builder 'using System;'
    Add-Line $builder 'using System.ComponentModel;'
    Add-Line $builder 'using SubSonic;'
    Add-Line $builder '// <auto-generated />'
    Add-Line $builder 'namespace SweetSoft.QLDA.DataAccess'
    Add-Line $builder '{'
    Add-Line $builder '    [DataObject]'
    Add-Line $builder "    public partial class ${className}Controller"
    Add-Line $builder '    {'
    Add-Line $builder "        private readonly $className schemaLoader = new $className();"
    Add-Line $builder '        private string UserName'
    Add-Line $builder '        {'
    Add-Line $builder '            get'
    Add-Line $builder '            {'
    Add-Line $builder '                return System.Web.HttpContext.Current != null'
    Add-Line $builder '                    ? System.Web.HttpContext.Current.User.Identity.Name'
    Add-Line $builder '                    : System.Threading.Thread.CurrentPrincipal.Identity.Name;'
    Add-Line $builder '            }'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder '        [DataObjectMethod(DataObjectMethodType.Select, true)]'
    Add-Line $builder "        public ${className}Collection FetchAll()"
    Add-Line $builder '        {'
    Add-Line $builder "            ${className}Collection collection = new ${className}Collection();"
    Add-Line $builder "            Query query = new Query($className.Schema);"
    Add-Line $builder '            collection.LoadAndCloseReader(query.ExecuteReader());'
    Add-Line $builder '            return collection;'
    Add-Line $builder '        }'
    Add-Line $builder ''
    if ($primaryKeys.Count -eq 1) {
        $pk = $primaryKeys[0]
        Add-Line $builder '        [DataObjectMethod(DataObjectMethodType.Select, false)]'
        Add-Line $builder "        public ${className}Collection FetchByID(object $($pk.Name))"
        Add-Line $builder '        {'
        Add-Line $builder ('            return new {0}Collection().Where("{1}", {2}).Load();' -f $className, $pk.DatabaseName, $pk.Name)
        Add-Line $builder '        }'
        Add-Line $builder ''
        Add-Line $builder '        [DataObjectMethod(DataObjectMethodType.Delete, true)]'
        Add-Line $builder "        public bool Delete(object $($pk.Name)) { return $className.Delete($($pk.Name)) == 1; }"
        Add-Line $builder '        [DataObjectMethod(DataObjectMethodType.Delete, false)]'
        Add-Line $builder "        public bool Destroy(object $($pk.Name)) { return $className.Destroy($($pk.Name)) == 1; }"
        Add-Line $builder ''
    }
    Add-Line $builder '        [DataObjectMethod(DataObjectMethodType.Select, false)]'
    Add-Line $builder "        public ${className}Collection FetchByQuery(Query query)"
    Add-Line $builder '        {'
    Add-Line $builder "            ${className}Collection collection = new ${className}Collection();"
    Add-Line $builder '            collection.LoadAndCloseReader(query.ExecuteReader());'
    Add-Line $builder '            return collection;'
    Add-Line $builder '        }'
    Add-Line $builder ''
    $parameterList = Get-ParameterList $Table.Columns
    Add-Line $builder '        [DataObjectMethod(DataObjectMethodType.Insert, true)]'
    Add-Line $builder "        public void Insert($parameterList)"
    Add-Line $builder '        {'
    Add-Line $builder "            $className item = new $className();"
    foreach ($column in $Table.Columns) {
        Add-Line $builder "            item.$($column.Name) = $($column.Name);"
    }
    Add-Line $builder '            item.Save(UserName);'
    Add-Line $builder '        }'
    Add-Line $builder ''
    Add-Line $builder '        [DataObjectMethod(DataObjectMethodType.Update, true)]'
    Add-Line $builder "        public void Update($parameterList)"
    Add-Line $builder '        {'
    Add-Line $builder "            $className item = new $className();"
    Add-Line $builder '            item.MarkOld();'
    Add-Line $builder '            item.IsLoaded = true;'
    foreach ($column in $Table.Columns) {
        Add-Line $builder "            item.$($column.Name) = $($column.Name);"
    }
    Add-Line $builder '            item.Save(UserName);'
    Add-Line $builder '        }'
    Add-Line $builder '    }'
    Add-Line $builder '}'

    $path = Join-Path $OutputDirectory ($className + 'Controller.cs')
    [System.IO.File]::WriteAllText($path, $builder.ToString(), [System.Text.UTF8Encoding]::new($false))
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputDirectory)
if (-not (Test-Path -LiteralPath $resolvedOutput -PathType Container)) {
    throw "Output directory does not exist: $resolvedOutput"
}

foreach ($table in $tables) {
    Write-Model $table
    Write-Controller $table
}

Write-Output "Generated $($tables.Count) QLDA3 models and controllers in $resolvedOutput"
