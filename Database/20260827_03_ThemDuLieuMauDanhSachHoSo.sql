/*
    THEM DU LIEU MAU CHO MAN HINH DANH SACH HO SO

    Muc dich:
    - Tao 12 ho so de xem giao dien danh sach.
    - Neu database co du an, 5 ho so se duoc gan vao cac du an hien co;
      cac ho so con lai la ho so dung chung cua cong ty.
    - Tao mot so file/phien ban gia lap de xem giao dien chi tiet ho so.
    - Khong tao file vat ly tren o dia; bam tai file mau se khong co noi dung that.

    Huong dan:
    1. Trong SSMS, chon database can thu nghiem (vi du SweetSoft_QLDA3).
    2. Chay toan bo file nay.
    3. Khi khong con can du lieu mau, chay file
       20260827_04_XoaDuLieuMauDanhSachHoSo.sql.

    An toan:
    - File co the chay lai; du lieu mau cu se duoc lam moi.
    - Chi xu ly cac ban ghi co NguoiTao = [DEMO.DanhSachHoSo].
    - Neu trung ma/Id voi du lieu that, script dung lai va rollback.
    - File co y khong co lenh USE; database dich do nguoi chay chon.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.TblTaiLieu', N'U') IS NULL
        THROW 51200, N'Khong tim thay bang dbo.TblTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblLoaiTaiLieu', N'U') IS NULL
        THROW 51201, N'Khong tim thay bang dbo.TblLoaiTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblPhienBanTaiLieu', N'U') IS NULL
        THROW 51202, N'Khong tim thay bang dbo.TblPhienBanTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblUploadFile', N'U') IS NULL
        THROW 51203, N'Khong tim thay bang dbo.TblUploadFile.', 1;

    DECLARE @NguoiTao NVARCHAR(150) = N'[DEMO.DanhSachHoSo]';
    DECLARE @NgayHienTai DATETIME = GETUTCDATE();
    DECLARE @OwnerId UNIQUEIDENTIFIER;
    DECLARE @IdNhanVienPhuTrach UNIQUEIDENTIFIER;

    SELECT TOP (1) @OwnerId = UserId
    FROM dbo.aspnet_Users
    ORDER BY UserName, UserId;

    IF @OwnerId IS NULL
        THROW 51204, N'Can it nhat mot tai khoan trong aspnet_Users de gan chu so huu cho file mau.', 1;

    /* Neu co nhan vien thi gan luan phien; neu chua co, cot nguoi phu trach de trong. */
    SELECT TOP (1) @IdNhanVienPhuTrach = IdNhanVien
    FROM dbo.TblNhanVien
    WHERE DaXoa = 0
    ORDER BY TenNhanVien, IdNhanVien;

    DECLARE @DuAnMau TABLE
    (
        ThuTuDuAn INT NOT NULL PRIMARY KEY,
        IdDuAn UNIQUEIDENTIFIER NOT NULL
    );

    INSERT INTO @DuAnMau (ThuTuDuAn, IdDuAn)
    SELECT ROW_NUMBER() OVER (ORDER BY TenDuAn, IdDuAn),
           IdDuAn
    FROM
    (
        SELECT TOP (3) IdDuAn, TenDuAn
        FROM dbo.TblDuAn
        WHERE DaXoa = 0
        ORDER BY TenDuAn, IdDuAn
    ) d;

    DECLARE @TaiLieuMau TABLE
    (
        IdTaiLieu UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        MaTaiLieu VARCHAR(100) NOT NULL,
        TenTaiLieu NVARCHAR(255) NOT NULL,
        TenLoai NVARCHAR(255) NOT NULL,
        MoTa NVARCHAR(1000) NULL,
        CanTrinhKy BIT NOT NULL,
        HinhThucKy VARCHAR(20) NULL,
        TrangThaiTaiLieu VARCHAR(50) NOT NULL,
        CanGuiKhachHang BIT NOT NULL,
        TrangThaiGuiKhach VARCHAR(50) NOT NULL,
        CanLuuVatLy BIT NOT NULL,
        TrangThaiLuuTru VARCHAR(50) NOT NULL,
        SoNgayTruoc INT NOT NULL,
        ThuTuDuAn INT NULL,
        IdFileBanChinhThuc UNIQUEIDENTIFIER NULL
    );

    INSERT INTO @TaiLieuMau
    (
        IdTaiLieu, MaTaiLieu, TenTaiLieu, TenLoai, MoTa,
        CanTrinhKy, HinhThucKy, TrangThaiTaiLieu,
        CanGuiKhachHang, TrangThaiGuiKhach,
        CanLuuVatLy, TrangThaiLuuTru,
        SoNgayTruoc, ThuTuDuAn, IdFileBanChinhThuc
    )
    VALUES
        ('10000000-0000-7000-8000-000000000001', 'DEMO-HS-001',
         N'Hợp đồng triển khai Website ABC', N'Hợp đồng',
         N'Hợp đồng đã được công ty và khách hàng ký; có bản Word chốt nội dung và bản PDF scan chính thức.',
         1, 'GIAY', 'DA_KY', 1, 'DA_NHAN_LAI', 1, 'DA_LUU', 120, NULL,
         '20000000-0000-7000-8000-000000000001'),

        ('10000000-0000-7000-8000-000000000002', 'DEMO-HS-002',
         N'Báo giá hệ thống quản lý nội bộ', N'Báo giá',
         N'Báo giá đã gửi khách hàng, không yêu cầu lưu bản cứng.',
         0, NULL, 'HOAN_TAT', 1, 'DA_GUI', 0, 'CHUA_LUU', 90, NULL, NULL),

        ('10000000-0000-7000-8000-000000000003', 'DEMO-HS-003',
         N'Biên bản họp khởi động dự án', N'Biên bản họp',
         N'Biên bản tổng hợp nội dung, kết luận và phân công sau cuộc họp kick-off.',
         0, NULL, 'HOAN_TAT', 0, 'CHUA_GUI', 0, 'CHUA_LUU', 75, NULL, NULL),

        ('10000000-0000-7000-8000-000000000004', 'DEMO-HS-004',
         N'Yêu cầu thay đổi giao diện báo cáo', N'Yêu cầu thay đổi',
         N'Khách hàng yêu cầu điều chỉnh phạm vi báo cáo trước khi xác nhận.',
         1, 'GIAY', 'YEU_CAU_DIEU_CHINH', 1, 'CHO_NHAN_LAI', 0, 'CHUA_LUU', 45, NULL, NULL),

        ('10000000-0000-7000-8000-000000000005', 'DEMO-HS-005',
         N'Đặc tả yêu cầu phần mềm phiên bản đầu', N'Đặc tả yêu cầu phần mềm',
         N'Tài liệu đang được nhóm phân tích hoàn thiện.',
         0, NULL, 'DANG_SOAN_THAO', 1, 'CHUA_GUI', 0, 'CHUA_LUU', 30, 3, NULL),

        ('10000000-0000-7000-8000-000000000006', 'DEMO-HS-006',
         N'Báo cáo tiến độ tháng 08/2026', N'Báo cáo tiến độ',
         N'Báo cáo tiến độ, vướng mắc và kế hoạch xử lý trong kỳ tiếp theo.',
         0, NULL, 'HOAN_TAT', 1, 'DA_GUI', 0, 'CHUA_LUU', 20, 1, NULL),

        ('10000000-0000-7000-8000-000000000007', 'DEMO-HS-007',
         N'Thỏa thuận bảo mật với đối tác hạ tầng', N'Thỏa thuận bảo mật (NDA)',
         N'Hồ sơ đang chờ giám đốc ký số bên ngoài hệ thống.',
         1, 'DIEN_TU', 'DANG_TRINH_KY', 1, 'CHUA_GUI', 1, 'CHUA_LUU', 14, NULL, NULL),

        ('10000000-0000-7000-8000-000000000008', 'DEMO-HS-008',
         N'Biên bản nghiệm thu giai đoạn 1', N'Biên bản nghiệm thu',
         N'Biên bản đã gửi khách kiểm tra và đang chờ nhận bản ký lại.',
         1, 'GIAY', 'DANG_TRINH_KY', 1, 'CHO_NHAN_LAI', 1, 'CHUA_LUU', 10, 1, NULL),

        ('10000000-0000-7000-8000-000000000009', 'DEMO-HS-009',
         N'Kế hoạch triển khai hệ thống tại văn phòng', N'Kế hoạch dự án',
         N'Kế hoạch đang soạn, chưa gửi khách hàng và không lưu bản cứng.',
         0, NULL, 'DANG_SOAN_THAO', 0, 'CHUA_GUI', 0, 'CHUA_LUU', 7, 2, NULL),

        ('10000000-0000-7000-8000-000000000010', 'DEMO-HS-010',
         N'Tài liệu khảo sát hiện trạng VCN Nha Trang', N'Tài liệu khảo sát hiện trạng',
         N'Tổng hợp quy trình hiện tại, biểu mẫu và nhu cầu của các phòng ban.',
         0, NULL, 'HOAN_TAT', 1, 'DA_NHAN_LAI', 0, 'CHUA_LUU', 5, 3,
         '20000000-0000-7000-8000-000000000002'),

        ('10000000-0000-7000-8000-000000000011', 'DEMO-HS-011',
         N'Công văn đến về lịch kiểm tra định kỳ', N'Công văn đến',
         N'Bản giấy đang được lấy khỏi tủ để xử lý công việc.',
         0, NULL, 'HOAN_TAT', 0, 'CHUA_GUI', 1, 'DANG_LAY_RA', 3, NULL, NULL),

        ('10000000-0000-7000-8000-000000000012', 'DEMO-HS-012',
         N'Quy định quản lý và bàn giao thiết bị', N'Quy định',
         N'Quy định nội bộ đã ban hành và đã lưu bản giấy chính thức.',
         1, 'GIAY', 'DA_KY', 0, 'CHUA_GUI', 1, 'DA_LUU', 1, NULL,
         '20000000-0000-7000-8000-000000000003');

    IF EXISTS
    (
        SELECT 1
        FROM @TaiLieuMau d
        LEFT JOIN dbo.TblLoaiTaiLieu l
            ON l.TenLoai = d.TenLoai
           AND l.DaXoa = 0
           AND l.KichHoat = 1
        WHERE l.IdLoaiTaiLieu IS NULL
    )
    BEGIN
        DECLARE @LoaiConThieu NVARCHAR(2048);
        SELECT @LoaiConThieu = STUFF
        (
            (
                SELECT DISTINCT N', ' + d.TenLoai
                FROM @TaiLieuMau d
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.TblLoaiTaiLieu l
                    WHERE l.TenLoai = d.TenLoai
                      AND l.DaXoa = 0
                      AND l.KichHoat = 1
                )
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'),
            1, 2, N''
        );

        DECLARE @ThongBaoLoai NVARCHAR(2048) =
            N'Thieu loai tai lieu dang hoat dong: ' + ISNULL(@LoaiConThieu, N'');
        THROW 51205, @ThongBaoLoai, 1;
    END;

    /* Khong cho phep du lieu mau ghi de ban ghi that. */
    IF EXISTS
    (
        SELECT 1
        FROM dbo.TblTaiLieu t
        INNER JOIN @TaiLieuMau d
            ON d.IdTaiLieu = t.IdTaiLieu
            OR d.MaTaiLieu = t.MaTaiLieu
        WHERE t.NguoiTao <> @NguoiTao
    )
        THROW 51206, N'Id hoac ma DEMO-HS dang duoc mot ban ghi that su dung. Khong co du lieu nao duoc thay doi.', 1;

    /* Lam sach lan chay demo truoc, neu co. */
    UPDATE t
    SET IdFileBanChinhThuc = NULL
    FROM dbo.TblTaiLieu t
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = t.IdTaiLieu
    WHERE t.NguoiTao = @NguoiTao;

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
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = t.IdTaiLieu
    WHERE t.NguoiTao = @NguoiTao;

    DELETE f
    FROM dbo.TblUploadFile f
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = f.RefId
    WHERE f.RefType = 'DocumentVersion';

    /* File gia lap: co metadata de hien thi, khong tao file vat ly. */
    DECLARE @FileMau TABLE
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        RefId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(255) NOT NULL,
        FileUrl NVARCHAR(1000) NOT NULL,
        Ext VARCHAR(20) NOT NULL,
        FileSize INT NOT NULL,
        MimeType VARCHAR(150) NOT NULL,
        OriginalFileName NVARCHAR(255) NOT NULL,
        DisplayOrder INT NOT NULL
    );

    INSERT INTO @FileMau
    (
        Id, RefId, Name, FileUrl, Ext, FileSize,
        MimeType, OriginalFileName, DisplayOrder
    )
    VALUES
        ('20000000-0000-7000-8000-000000000001', '10000000-0000-7000-8000-000000000001',
         N'Hợp đồng Website ABC - bản scan đã ký',
         N'/Uploads/DocumentVersion/DEMO/Hop_dong_Website_ABC_da_ky.pdf',
         '.pdf', 2457600, 'application/pdf', N'Hop_dong_Website_ABC_da_ky.pdf', 4),
        ('20000000-0000-7000-8000-000000000002', '10000000-0000-7000-8000-000000000010',
         N'Khảo sát hiện trạng VCN - bản chính thức',
         N'/Uploads/DocumentVersion/DEMO/Khao_sat_hien_trang_VCN_chinh_thuc.pdf',
         '.pdf', 1784500, 'application/pdf', N'Khao_sat_hien_trang_VCN_chinh_thuc.pdf', 2),
        ('20000000-0000-7000-8000-000000000003', '10000000-0000-7000-8000-000000000012',
         N'Quy định quản lý thiết bị - bản ban hành',
         N'/Uploads/DocumentVersion/DEMO/Quy_dinh_quan_ly_thiet_bi_ban_hanh.pdf',
         '.pdf', 985600, 'application/pdf', N'Quy_dinh_quan_ly_thiet_bi_ban_hanh.pdf', 2),

        ('20000000-0000-7000-8000-000000000011', '10000000-0000-7000-8000-000000000001',
         N'Hợp đồng Website ABC v1.0',
         N'/Uploads/DocumentVersion/DEMO/Hop_dong_Website_ABC_v1_0.docx',
         '.docx', 126500, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         N'Hop_dong_Website_ABC_v1_0.docx', 1),
        ('20000000-0000-7000-8000-000000000012', '10000000-0000-7000-8000-000000000001',
         N'Hợp đồng Website ABC v2.0',
         N'/Uploads/DocumentVersion/DEMO/Hop_dong_Website_ABC_v2_0.docx',
         '.docx', 139800, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         N'Hop_dong_Website_ABC_v2_0.docx', 2),
        ('20000000-0000-7000-8000-000000000013', '10000000-0000-7000-8000-000000000001',
         N'Hợp đồng Website ABC v3.0 - bản Word chốt',
         N'/Uploads/DocumentVersion/DEMO/Hop_dong_Website_ABC_v3_0_chot.docx',
         '.docx', 145200, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         N'Hop_dong_Website_ABC_v3_0_chot.docx', 3),
        ('20000000-0000-7000-8000-000000000021', '10000000-0000-7000-8000-000000000003',
         N'Biên bản họp kick-off v1.0',
         N'/Uploads/DocumentVersion/DEMO/Bien_ban_hop_kickoff_v1_0.docx',
         '.docx', 86400, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         N'Bien_ban_hop_kickoff_v1_0.docx', 1),
        ('20000000-0000-7000-8000-000000000022', '10000000-0000-7000-8000-000000000003',
         N'Biên bản họp kick-off v2.0 - đã chốt',
         N'/Uploads/DocumentVersion/DEMO/Bien_ban_hop_kickoff_v2_0.pdf',
         '.pdf', 310500, 'application/pdf', N'Bien_ban_hop_kickoff_v2_0.pdf', 2),
        ('20000000-0000-7000-8000-000000000031', '10000000-0000-7000-8000-000000000007',
         N'Thỏa thuận bảo mật - bản chờ ký số',
         N'/Uploads/DocumentVersion/DEMO/NDA_doi_tac_ha_tang_cho_ky.docx',
         '.docx', 97200, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         N'NDA_doi_tac_ha_tang_cho_ky.docx', 1),
        ('20000000-0000-7000-8000-000000000041', '10000000-0000-7000-8000-000000000010',
         N'Khảo sát hiện trạng VCN - bản tổng hợp',
         N'/Uploads/DocumentVersion/DEMO/Khao_sat_hien_trang_VCN_v1_0.docx',
         '.docx', 218600, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         N'Khao_sat_hien_trang_VCN_v1_0.docx', 1),
        ('20000000-0000-7000-8000-000000000051', '10000000-0000-7000-8000-000000000012',
         N'Quy định quản lý thiết bị - bản soạn thảo',
         N'/Uploads/DocumentVersion/DEMO/Quy_dinh_quan_ly_thiet_bi_v1_0.docx',
         '.docx', 77500, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         N'Quy_dinh_quan_ly_thiet_bi_v1_0.docx', 1);

    INSERT INTO dbo.TblUploadFile
    (
        Id, OwnerId, CreatedDate, IsDeleted, Name, FileUrl,
        FileType, Ext, RefId, RefType, DisplayOrder, FileSize,
        MimeType, OriginalFileName, IsHost, IsSecretary, IsParticipant
    )
    SELECT f.Id,
           @OwnerId,
           DATEADD(DAY, -d.SoNgayTruoc, @NgayHienTai),
           0,
           f.Name,
           f.FileUrl,
           'Internal',
           f.Ext,
           f.RefId,
           'DocumentVersion',
           f.DisplayOrder,
           f.FileSize,
           f.MimeType,
           f.OriginalFileName,
           0, 0, 0
    FROM @FileMau f
    INNER JOIN @TaiLieuMau d ON d.IdTaiLieu = f.RefId;

    INSERT INTO dbo.TblTaiLieu
    (
        IdTaiLieu, IdDuAn, IdLoaiTaiLieu, MaTaiLieu, TenTaiLieu, MoTa,
        IdNhanVienPhuTrach, CanTrinhKy, HinhThucKy, TrangThaiTaiLieu,
        CanGuiKhachHang, TrangThaiGuiKhach, CanLuuVatLy,
        TrangThaiLuuTru, DaXoa, NguoiTao, NgayTao,
        NguoiCapNhat, NgayCapNhat, IdFileBanChinhThuc
    )
    SELECT d.IdTaiLieu,
           (
               SELECT p.IdDuAn
               FROM @DuAnMau p
               WHERE p.ThuTuDuAn = d.ThuTuDuAn
           ),
           (
               SELECT TOP (1) l.IdLoaiTaiLieu
               FROM dbo.TblLoaiTaiLieu l
               WHERE l.TenLoai = d.TenLoai
                 AND l.DaXoa = 0
                 AND l.KichHoat = 1
               ORDER BY l.IdLoaiTaiLieu
           ),
           d.MaTaiLieu,
           d.TenTaiLieu,
           d.MoTa,
           @IdNhanVienPhuTrach,
           d.CanTrinhKy,
           d.HinhThucKy,
           d.TrangThaiTaiLieu,
           d.CanGuiKhachHang,
           d.TrangThaiGuiKhach,
           d.CanLuuVatLy,
           d.TrangThaiLuuTru,
           0,
           @NguoiTao,
           DATEADD(DAY, -d.SoNgayTruoc, @NgayHienTai),
           NULL,
           NULL,
           d.IdFileBanChinhThuc
    FROM @TaiLieuMau d;

    DECLARE @PhienBanMau TABLE
    (
        IdPhienBanTaiLieu UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        IdTaiLieu UNIQUEIDENTIFIER NOT NULL,
        SoPhienBan VARCHAR(50) NOT NULL,
        IdPhienBanNguon UNIQUEIDENTIFIER NULL,
        MoTaPhienBan NVARCHAR(1000) NULL,
        LaPhienBanHienTai BIT NOT NULL,
        IdFileNoiDung UNIQUEIDENTIFIER NOT NULL,
        SoNgayTruoc INT NOT NULL
    );

    INSERT INTO @PhienBanMau
    (
        IdPhienBanTaiLieu, IdTaiLieu, SoPhienBan, IdPhienBanNguon,
        MoTaPhienBan, LaPhienBanHienTai, IdFileNoiDung, SoNgayTruoc
    )
    VALUES
        ('30000000-0000-7000-8000-000000000011', '10000000-0000-7000-8000-000000000001',
         '1.0', NULL, N'Bản dự thảo hợp đồng ban đầu.', 0,
         '20000000-0000-7000-8000-000000000011', 120),
        ('30000000-0000-7000-8000-000000000012', '10000000-0000-7000-8000-000000000001',
         '2.0', '30000000-0000-7000-8000-000000000011', N'Điều chỉnh phạm vi và tiến độ theo góp ý khách hàng.', 0,
         '20000000-0000-7000-8000-000000000012', 115),
        ('30000000-0000-7000-8000-000000000013', '10000000-0000-7000-8000-000000000001',
         '3.0', '30000000-0000-7000-8000-000000000012', N'Bản Word cuối đã chốt nội dung trước khi ký.', 1,
         '20000000-0000-7000-8000-000000000013', 110),

        ('30000000-0000-7000-8000-000000000021', '10000000-0000-7000-8000-000000000003',
         '1.0', NULL, N'Biên bản ghi nhanh sau cuộc họp.', 0,
         '20000000-0000-7000-8000-000000000021', 75),
        ('30000000-0000-7000-8000-000000000022', '10000000-0000-7000-8000-000000000003',
         '2.0', '30000000-0000-7000-8000-000000000021', N'Bản đã bổ sung kết luận và xác nhận của các bên tham dự.', 1,
         '20000000-0000-7000-8000-000000000022', 74),

        ('30000000-0000-7000-8000-000000000031', '10000000-0000-7000-8000-000000000007',
         '1.0', NULL, N'Bản đang chờ ký số bên ngoài hệ thống.', 1,
         '20000000-0000-7000-8000-000000000031', 14),

        ('30000000-0000-7000-8000-000000000041', '10000000-0000-7000-8000-000000000010',
         '1.0', NULL, N'Bản tổng hợp kết quả khảo sát hiện trạng.', 1,
         '20000000-0000-7000-8000-000000000041', 5),

        ('30000000-0000-7000-8000-000000000051', '10000000-0000-7000-8000-000000000012',
         '1.0', NULL, N'Bản soạn thảo trước khi ban hành.', 1,
         '20000000-0000-7000-8000-000000000051', 1);

    INSERT INTO dbo.TblPhienBanTaiLieu
    (
        IdPhienBanTaiLieu, IdTaiLieu, SoPhienBan, NguonTao,
        IdPhienBanNguon, MoTaPhienBan, NoiDungTrucTiep,
        LaPhienBanHienTai, DaXoa, NguoiTao, NgayTao,
        NguoiCapNhat, NgayCapNhat, IdFileNoiDung
    )
    SELECT p.IdPhienBanTaiLieu,
           p.IdTaiLieu,
           p.SoPhienBan,
           'UPLOAD',
           p.IdPhienBanNguon,
           p.MoTaPhienBan,
           NULL,
           p.LaPhienBanHienTai,
           0,
           @NguoiTao,
           DATEADD(DAY, -p.SoNgayTruoc, @NgayHienTai),
           NULL,
           NULL,
           p.IdFileNoiDung
    FROM @PhienBanMau p;

    COMMIT TRANSACTION;

    SELECT N'Đã thêm dữ liệu mẫu danh sách hồ sơ.' AS KetQua,
           COUNT(1) AS SoHoSoMau
    FROM dbo.TblTaiLieu
    WHERE NguoiTao = @NguoiTao;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
