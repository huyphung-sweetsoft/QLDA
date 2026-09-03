/*
    Cap nhat database cho chuc nang Quan ly ho so

    Gom 3 thay doi:
    1. Cho phep dbo.TblTaiLieu.IdDuAn nhan NULL.
    2. Them 5 cot lien ket file va khoa ngoai den dbo.TblUploadFile(Id).
    3. Tao dbo.TblLichSuChinhSua de luu snapshot khi sua noi dung truc tiep.

    Luu y:
    - Script nay danh cho database SweetSoft_QLDA.
    - Co the chay lai; cac thanh phan da ton tai se khong bi tao trung.
    - ON DELETE NO ACTION duoc dung vi file/tai lieu dang theo co che xoa mem.
*/

USE [SweetSoft_QLDA];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    /* Kiem tra cac bang nen truoc khi thay doi. */
    IF OBJECT_ID(N'dbo.TblTaiLieu', N'U') IS NULL
        THROW 50001, N'Khong tim thay bang dbo.TblTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblMauTaiLieu', N'U') IS NULL
        THROW 50002, N'Khong tim thay bang dbo.TblMauTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblPhienBanTaiLieu', N'U') IS NULL
        THROW 50003, N'Khong tim thay bang dbo.TblPhienBanTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblTrinhKyTaiLieu', N'U') IS NULL
        THROW 50004, N'Khong tim thay bang dbo.TblTrinhKyTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblGuiNhanKhachHang', N'U') IS NULL
        THROW 50005, N'Khong tim thay bang dbo.TblGuiNhanKhachHang.', 1;

    IF OBJECT_ID(N'dbo.TblUploadFile', N'U') IS NULL
        THROW 50006, N'Khong tim thay bang dbo.TblUploadFile.', 1;

    IF OBJECT_ID(N'dbo.TblNhanVien', N'U') IS NULL
        THROW 50007, N'Khong tim thay bang dbo.TblNhanVien.', 1;

    BEGIN TRANSACTION;

    /* ================================================================
       1. Tai lieu cong ty khong bat buoc thuoc mot du an
       IdDuAn IS NULL     : tai lieu dung chung cua cong ty
       IdDuAn IS NOT NULL : tai lieu cua mot du an cu the
       ================================================================ */
    IF EXISTS
    (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.TblTaiLieu')
          AND name = N'IdDuAn'
          AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TblTaiLieu
            ALTER COLUMN IdDuAn UNIQUEIDENTIFIER NULL;
    END;

    /* ================================================================
       2. Them 5 cot lien ket file
       ================================================================ */
    IF COL_LENGTH(N'dbo.TblMauTaiLieu', N'IdFileMau') IS NULL
    BEGIN
        ALTER TABLE dbo.TblMauTaiLieu
            ADD IdFileMau UNIQUEIDENTIFIER NULL;
    END;

    IF COL_LENGTH(N'dbo.TblPhienBanTaiLieu', N'IdFileNoiDung') IS NULL
    BEGIN
        ALTER TABLE dbo.TblPhienBanTaiLieu
            ADD IdFileNoiDung UNIQUEIDENTIFIER NULL;
    END;

    IF COL_LENGTH(N'dbo.TblTrinhKyTaiLieu', N'IdFileSauKy') IS NULL
    BEGIN
        ALTER TABLE dbo.TblTrinhKyTaiLieu
            ADD IdFileSauKy UNIQUEIDENTIFIER NULL;
    END;

    IF COL_LENGTH(N'dbo.TblGuiNhanKhachHang', N'IdFileNhanLai') IS NULL
    BEGIN
        ALTER TABLE dbo.TblGuiNhanKhachHang
            ADD IdFileNhanLai UNIQUEIDENTIFIER NULL;
    END;

    IF COL_LENGTH(N'dbo.TblTaiLieu', N'IdFileBanChinhThuc') IS NULL
    BEGIN
        ALTER TABLE dbo.TblTaiLieu
            ADD IdFileBanChinhThuc UNIQUEIDENTIFIER NULL;
    END;

    /* Khoa ngoai cho file mau. */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_key_columns AS fkc
        WHERE fkc.parent_object_id = OBJECT_ID(N'dbo.TblMauTaiLieu')
          AND fkc.parent_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblMauTaiLieu'), N'IdFileMau', 'ColumnId')
          AND fkc.referenced_object_id = OBJECT_ID(N'dbo.TblUploadFile')
          AND fkc.referenced_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblUploadFile'), N'Id', 'ColumnId')
    )
    BEGIN
        ALTER TABLE dbo.TblMauTaiLieu WITH CHECK
            ADD CONSTRAINT FK_TblMauTaiLieu_UploadFile_Mau
            FOREIGN KEY (IdFileMau)
            REFERENCES dbo.TblUploadFile (Id)
            ON DELETE NO ACTION;

        ALTER TABLE dbo.TblMauTaiLieu
            CHECK CONSTRAINT FK_TblMauTaiLieu_UploadFile_Mau;
    END;

    /* Khoa ngoai cho file noi dung cua phien ban. */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_key_columns AS fkc
        WHERE fkc.parent_object_id = OBJECT_ID(N'dbo.TblPhienBanTaiLieu')
          AND fkc.parent_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblPhienBanTaiLieu'), N'IdFileNoiDung', 'ColumnId')
          AND fkc.referenced_object_id = OBJECT_ID(N'dbo.TblUploadFile')
          AND fkc.referenced_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblUploadFile'), N'Id', 'ColumnId')
    )
    BEGIN
        ALTER TABLE dbo.TblPhienBanTaiLieu WITH CHECK
            ADD CONSTRAINT FK_TblPhienBanTaiLieu_UploadFile_NoiDung
            FOREIGN KEY (IdFileNoiDung)
            REFERENCES dbo.TblUploadFile (Id)
            ON DELETE NO ACTION;

        ALTER TABLE dbo.TblPhienBanTaiLieu
            CHECK CONSTRAINT FK_TblPhienBanTaiLieu_UploadFile_NoiDung;
    END;

    /* Khoa ngoai cho file nhan lai sau khi trinh ky. */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_key_columns AS fkc
        WHERE fkc.parent_object_id = OBJECT_ID(N'dbo.TblTrinhKyTaiLieu')
          AND fkc.parent_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblTrinhKyTaiLieu'), N'IdFileSauKy', 'ColumnId')
          AND fkc.referenced_object_id = OBJECT_ID(N'dbo.TblUploadFile')
          AND fkc.referenced_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblUploadFile'), N'Id', 'ColumnId')
    )
    BEGIN
        ALTER TABLE dbo.TblTrinhKyTaiLieu WITH CHECK
            ADD CONSTRAINT FK_TblTrinhKyTaiLieu_UploadFile_SauKy
            FOREIGN KEY (IdFileSauKy)
            REFERENCES dbo.TblUploadFile (Id)
            ON DELETE NO ACTION;

        ALTER TABLE dbo.TblTrinhKyTaiLieu
            CHECK CONSTRAINT FK_TblTrinhKyTaiLieu_UploadFile_SauKy;
    END;

    /* Khoa ngoai cho file khach hang gui/ky tra lai. */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_key_columns AS fkc
        WHERE fkc.parent_object_id = OBJECT_ID(N'dbo.TblGuiNhanKhachHang')
          AND fkc.parent_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblGuiNhanKhachHang'), N'IdFileNhanLai', 'ColumnId')
          AND fkc.referenced_object_id = OBJECT_ID(N'dbo.TblUploadFile')
          AND fkc.referenced_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblUploadFile'), N'Id', 'ColumnId')
    )
    BEGIN
        ALTER TABLE dbo.TblGuiNhanKhachHang WITH CHECK
            ADD CONSTRAINT FK_TblGuiNhanKhachHang_UploadFile_NhanLai
            FOREIGN KEY (IdFileNhanLai)
            REFERENCES dbo.TblUploadFile (Id)
            ON DELETE NO ACTION;

        ALTER TABLE dbo.TblGuiNhanKhachHang
            CHECK CONSTRAINT FK_TblGuiNhanKhachHang_UploadFile_NhanLai;
    END;

    /* Khoa ngoai cho file chinh thuc hien tai cua tai lieu. */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_key_columns AS fkc
        WHERE fkc.parent_object_id = OBJECT_ID(N'dbo.TblTaiLieu')
          AND fkc.parent_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblTaiLieu'), N'IdFileBanChinhThuc', 'ColumnId')
          AND fkc.referenced_object_id = OBJECT_ID(N'dbo.TblUploadFile')
          AND fkc.referenced_column_id = COLUMNPROPERTY(
                OBJECT_ID(N'dbo.TblUploadFile'), N'Id', 'ColumnId')
    )
    BEGIN
        ALTER TABLE dbo.TblTaiLieu WITH CHECK
            ADD CONSTRAINT FK_TblTaiLieu_UploadFile_BanChinhThuc
            FOREIGN KEY (IdFileBanChinhThuc)
            REFERENCES dbo.TblUploadFile (Id)
            ON DELETE NO ACTION;

        ALTER TABLE dbo.TblTaiLieu
            CHECK CONSTRAINT FK_TblTaiLieu_UploadFile_BanChinhThuc;
    END;

    /* ================================================================
       3. Lich su chinh sua trong mot phien ban

       Bang nay chi dung khi sua noi dung truc tiep tren he thong va can
       xem/khoi phuc snapshot cu. Viec tao phien ban file moi van duoc luu
       trong dbo.TblPhienBanTaiLieu; cac hanh dong nghiep vu van duoc ghi
       trong dbo.TblLichSuTaiLieu.
       ================================================================ */
    IF OBJECT_ID(N'dbo.TblLichSuChinhSua', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.TblLichSuChinhSua
        (
            IdLichSuChinhSua UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_TblLichSuChinhSua_Id DEFAULT (NEWID()),
            IdPhienBanTaiLieu UNIQUEIDENTIFIER NOT NULL,
            NoiDungSnapshot NVARCHAR(MAX) NULL,
            MoTaThayDoi NVARCHAR(500) NULL,
            IdNhanVienThucHien UNIQUEIDENTIFIER NULL,
            NguoiThucHien NVARCHAR(150) NOT NULL,
            LaMocThuCong BIT NOT NULL
                CONSTRAINT DF_TblLichSuChinhSua_LaMocThuCong DEFAULT ((0)),
            NgayTao DATETIME NOT NULL
                CONSTRAINT DF_TblLichSuChinhSua_NgayTao DEFAULT (GETDATE()),

            CONSTRAINT PK_TblLichSuChinhSua
                PRIMARY KEY CLUSTERED (IdLichSuChinhSua),

            CONSTRAINT FK_TblLichSuChinhSua_PhienBan
                FOREIGN KEY (IdPhienBanTaiLieu)
                REFERENCES dbo.TblPhienBanTaiLieu (IdPhienBanTaiLieu)
                ON DELETE NO ACTION,

            CONSTRAINT FK_TblLichSuChinhSua_NhanVien
                FOREIGN KEY (IdNhanVienThucHien)
                REFERENCES dbo.TblNhanVien (IdNhanVien)
                ON DELETE NO ACTION
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.TblLichSuChinhSua')
          AND name = N'IX_TblLichSuChinhSua_PhienBan_NgayTao'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_TblLichSuChinhSua_PhienBan_NgayTao
            ON dbo.TblLichSuChinhSua (IdPhienBanTaiLieu, NgayTao DESC);
    END;

    COMMIT TRANSACTION;

    PRINT N'Cap nhat database Quan ly ho so thanh cong.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

/* Kiem tra nhanh ket qua sau khi chay. */
SELECT
    OBJECT_NAME(c.object_id) AS TenBang,
    c.name AS TenCot,
    TYPE_NAME(c.user_type_id) AS KieuDuLieu,
    c.is_nullable AS ChoPhepNull
FROM sys.columns AS c
WHERE
       (c.object_id = OBJECT_ID(N'dbo.TblTaiLieu')
        AND c.name IN (N'IdDuAn', N'IdFileBanChinhThuc'))
    OR (c.object_id = OBJECT_ID(N'dbo.TblMauTaiLieu')
        AND c.name = N'IdFileMau')
    OR (c.object_id = OBJECT_ID(N'dbo.TblPhienBanTaiLieu')
        AND c.name = N'IdFileNoiDung')
    OR (c.object_id = OBJECT_ID(N'dbo.TblTrinhKyTaiLieu')
        AND c.name = N'IdFileSauKy')
    OR (c.object_id = OBJECT_ID(N'dbo.TblGuiNhanKhachHang')
        AND c.name = N'IdFileNhanLai')
ORDER BY TenBang, TenCot;

SELECT
    OBJECT_SCHEMA_NAME(t.object_id) AS TenSchema,
    t.name AS TenBang
FROM sys.tables AS t
WHERE t.object_id = OBJECT_ID(N'dbo.TblLichSuChinhSua');
GO
