/*
    XOA DU LIEU MAU CUA MAN HINH DANH SACH HO SO

    File nay dua database ve trang thai truoc khi chay
    20260827_03_ThemDuLieuMauDanhSachHoSo.sql.

    Pham vi xoa:
    - Chi cac ho so co NguoiTao = [DEMO.DanhSachHoSo].
    - Cac phien ban, lich su, trinh ky, gui khach, luu tru vat ly va
      metadata file gan voi chinh cac ho so demo do.
    - Khong xoa nhom tai lieu, loai tai lieu, menu, quyen hay tai khoan.

    File co y khong co lenh USE; database dich do nguoi chay chon.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.TblTaiLieu', N'U') IS NULL
        THROW 51300, N'Khong tim thay bang dbo.TblTaiLieu.', 1;

    DECLARE @NguoiTao NVARCHAR(150) = N'[DEMO.DanhSachHoSo]';
    DECLARE @SoHoSoDaXoa INT = 0;

    DECLARE @TaiLieuMau TABLE
    (
        IdTaiLieu UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
    );

    INSERT INTO @TaiLieuMau (IdTaiLieu)
    SELECT IdTaiLieu
    FROM dbo.TblTaiLieu
    WHERE NguoiTao = @NguoiTao;

    SELECT @SoHoSoDaXoa = COUNT(1)
    FROM @TaiLieuMau;

    /* Go lien ket toi file chinh thuc truoc khi xoa metadata file. */
    UPDATE t
    SET IdFileBanChinhThuc = NULL
    FROM dbo.TblTaiLieu t
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = t.IdTaiLieu;

    IF OBJECT_ID(N'dbo.TblLichSuChinhSua', N'U') IS NOT NULL
    BEGIN
        DELETE ls
        FROM dbo.TblLichSuChinhSua ls
        INNER JOIN dbo.TblPhienBanTaiLieu p
            ON p.IdPhienBanTaiLieu = ls.IdPhienBanTaiLieu
        INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = p.IdTaiLieu;
    END;

    IF OBJECT_ID(N'dbo.TblTrinhKyTaiLieu', N'U') IS NOT NULL
    BEGIN
        DELETE tk
        FROM dbo.TblTrinhKyTaiLieu tk
        INNER JOIN dbo.TblPhienBanTaiLieu p
            ON p.IdPhienBanTaiLieu = tk.IdPhienBanTaiLieu
        INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = p.IdTaiLieu;
    END;

    IF OBJECT_ID(N'dbo.TblGuiNhanKhachHang', N'U') IS NOT NULL
    BEGIN
        DELETE g
        FROM dbo.TblGuiNhanKhachHang g
        INNER JOIN dbo.TblPhienBanTaiLieu p
            ON p.IdPhienBanTaiLieu = g.IdPhienBanTaiLieu
        INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = p.IdTaiLieu;
    END;

    IF OBJECT_ID(N'dbo.TblLichSuTaiLieu', N'U') IS NOT NULL
    BEGIN
        DELETE ls
        FROM dbo.TblLichSuTaiLieu ls
        INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = ls.IdTaiLieu;
    END;

    IF OBJECT_ID(N'dbo.TblLuuTruVatLy', N'U') IS NOT NULL
    BEGIN
        DELETE lt
        FROM dbo.TblLuuTruVatLy lt
        INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = lt.IdTaiLieu;
    END;

    UPDATE p
    SET IdPhienBanNguon = NULL
    FROM dbo.TblPhienBanTaiLieu p
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = p.IdTaiLieu;

    DELETE p
    FROM dbo.TblPhienBanTaiLieu p
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = p.IdTaiLieu;

    DELETE t
    FROM dbo.TblTaiLieu t
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = t.IdTaiLieu;

    /* Xoa ca file demo mac dinh va file nguoi dung co the da tai vao ho so demo. */
    DELETE f
    FROM dbo.TblUploadFile f
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = f.RefId
    WHERE f.RefType = 'DocumentVersion';

    COMMIT TRANSACTION;

    SELECT N'Đã xóa dữ liệu mẫu danh sách hồ sơ.' AS KetQua,
           @SoHoSoDaXoa AS SoHoSoDaXoa;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
