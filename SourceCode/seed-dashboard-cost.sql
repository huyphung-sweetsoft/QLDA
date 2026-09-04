SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @SeedUser NVARCHAR(150) = N'CODEX_DASHBOARD_COST_DEMO';
    DECLARE @CustomerId UNIQUEIDENTIFIER =
    (
        SELECT TOP (1) IdKhachHang
        FROM dbo.TblKhachHang
        WHERE DaXoa = 0
        ORDER BY
            CASE WHEN TenKhachHang = N'Công ty TNHH Khách hàng mẫu'
                THEN 0 ELSE 1 END,
            TenKhachHang
    );

    IF @CustomerId IS NULL
    BEGIN
        THROW 51000, N'Cần có ít nhất một khách hàng chưa xóa để tạo dữ liệu demo.', 1;
    END;

    DECLARE @Contract1 UNIQUEIDENTIFIER =
        'C05D0001-0000-4000-8000-000000000001';
    DECLARE @Contract2 UNIQUEIDENTIFIER =
        'C05D0002-0000-4000-8000-000000000002';
    DECLARE @Contract3 UNIQUEIDENTIFIER =
        'C05D0003-0000-4000-8000-000000000003';

    INSERT INTO dbo.TblHopDongThucHien
    (
        IdHopDongThucHien,
        SoHopDong,
        TenHopDong,
        IdKhachHang,
        GiaTriHopDong,
        NgayKy,
        NgayHieuLuc,
        NgayHetHan,
        MoTa,
        DaXoa,
        NguoiTao,
        NgayTao
    )
    SELECT
        source.IdHopDong,
        source.SoHopDong,
        source.TenHopDong,
        @CustomerId,
        source.GiaTriHopDong,
        source.NgayKy,
        source.NgayHieuLuc,
        source.NgayHetHan,
        N'Dữ liệu kiểm thử Dashboard chi phí',
        0,
        @SeedUser,
        GETDATE()
    FROM
    (
        VALUES
        (@Contract1, 'COST-DEMO-HD-001', N'Hợp đồng demo triển khai ERP', CAST(1200000000 AS DECIMAL(18, 2)), CAST('20260401' AS DATETIME), CAST('20260401' AS DATETIME), CAST('20260901' AS DATETIME)),
        (@Contract2, 'COST-DEMO-HD-002', N'Hợp đồng demo cổng thông tin', CAST(800000000 AS DECIMAL(18, 2)), CAST('20260301' AS DATETIME), CAST('20260301' AS DATETIME), CAST('20260818' AS DATETIME)),
        (@Contract3, 'COST-DEMO-HD-003', N'Hợp đồng demo ứng dụng di động', CAST(500000000 AS DECIMAL(18, 2)), CAST('20260201' AS DATETIME), CAST('20260201' AS DATETIME), CAST('20260728' AS DATETIME))
    ) source
    (
        IdHopDong,
        SoHopDong,
        TenHopDong,
        GiaTriHopDong,
        NgayKy,
        NgayHieuLuc,
        NgayHetHan
    )
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblHopDongThucHien existing
        WHERE existing.IdHopDongThucHien = source.IdHopDong
           OR (existing.SoHopDong = source.SoHopDong AND existing.DaXoa = 0)
    );

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.TblHopDongThucHien
        WHERE IdHopDongThucHien = @Contract1 AND DaXoa = 0
    )
    OR NOT EXISTS
    (
        SELECT 1 FROM dbo.TblHopDongThucHien
        WHERE IdHopDongThucHien = @Contract2 AND DaXoa = 0
    )
    OR NOT EXISTS
    (
        SELECT 1 FROM dbo.TblHopDongThucHien
        WHERE IdHopDongThucHien = @Contract3 AND DaXoa = 0
    )
    BEGIN
        THROW 51001, N'Mã hợp đồng demo bị trùng với dữ liệu khác.', 1;
    END;

    DECLARE @Project1 UNIQUEIDENTIFIER =
        'C05D1001-0000-4000-8000-000000000001';
    DECLARE @Project2 UNIQUEIDENTIFIER =
        'C05D1002-0000-4000-8000-000000000002';
    DECLARE @Project3 UNIQUEIDENTIFIER =
        'C05D1003-0000-4000-8000-000000000003';

    INSERT INTO dbo.TblDuAn
    (
        IdDuAn,
        MaDuAn,
        TenDuAn,
        IdKhachHang,
        IdHopDongThucHien,
        NgayBatDau,
        NgayDuKienHoanThanh,
        NgayHoanThanhThucTe,
        TrangThai,
        MoTa,
        DaXoa,
        NguoiTao,
        NgayTao
    )
    SELECT
        source.IdDuAn,
        source.MaDuAn,
        source.TenDuAn,
        @CustomerId,
        source.IdHopDong,
        source.NgayBatDau,
        source.NgayDuKienHoanThanh,
        source.NgayHoanThanhThucTe,
        2,
        N'Dữ liệu kiểm thử Dashboard chi phí',
        0,
        @SeedUser,
        GETDATE()
    FROM
    (
        VALUES
        (@Project1, 'COST-DEMO-001', N'[DEMO] Triển khai hệ thống ERP', @Contract1, CAST('20260401' AS DATETIME), CAST('20260831' AS DATETIME), CAST('20260901' AS DATETIME)),
        (@Project2, 'COST-DEMO-002', N'[DEMO] Xây dựng cổng thông tin', @Contract2, CAST('20260301' AS DATETIME), CAST('20260820' AS DATETIME), CAST('20260818' AS DATETIME)),
        (@Project3, 'COST-DEMO-003', N'[DEMO] Phát triển ứng dụng di động', @Contract3, CAST('20260201' AS DATETIME), CAST('20260731' AS DATETIME), CAST('20260728' AS DATETIME))
    ) source
    (
        IdDuAn,
        MaDuAn,
        TenDuAn,
        IdHopDong,
        NgayBatDau,
        NgayDuKienHoanThanh,
        NgayHoanThanhThucTe
    )
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblDuAn existing
        WHERE existing.IdDuAn = source.IdDuAn
           OR (existing.MaDuAn = source.MaDuAn AND existing.DaXoa = 0)
    );

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.TblDuAn
        WHERE IdDuAn = @Project1 AND DaXoa = 0
    )
    OR NOT EXISTS
    (
        SELECT 1 FROM dbo.TblDuAn
        WHERE IdDuAn = @Project2 AND DaXoa = 0
    )
    OR NOT EXISTS
    (
        SELECT 1 FROM dbo.TblDuAn
        WHERE IdDuAn = @Project3 AND DaXoa = 0
    )
    BEGIN
        THROW 51002, N'Mã dự án demo bị trùng với dữ liệu khác.', 1;
    END;

    INSERT INTO dbo.TblChiPhi
    (
        IdChiPhi,
        IdDuAn,
        IdCongViec,
        IdNhanVienDeNghi,
        MaKhoanChi,
        TenKhoanChi,
        NgayPhatSinh,
        SoTien,
        MoTaChiTiet,
        TrangThai,
        DaXoa,
        NguoiTao,
        NgayTao
    )
    SELECT
        source.IdChiPhi,
        source.IdDuAn,
        NULL,
        NULL,
        source.MaKhoanChi,
        source.TenKhoanChi,
        source.NgayPhatSinh,
        source.SoTien,
        N'Dữ liệu kiểm thử Dashboard chi phí',
        2,
        0,
        @SeedUser,
        GETDATE()
    FROM
    (
        VALUES
        (CAST('C05D2001-0000-4000-8000-000000000001' AS UNIQUEIDENTIFIER), @Project1, 'COST-DEMO-CP-001', N'Khảo sát và phân tích nghiệp vụ', CAST('20260410' AS DATETIME), CAST(120000000 AS DECIMAL(18, 2))),
        (CAST('C05D2002-0000-4000-8000-000000000002' AS UNIQUEIDENTIFIER), @Project1, 'COST-DEMO-CP-002', N'Thiết kế hệ thống', CAST('20260515' AS DATETIME), CAST(180000000 AS DECIMAL(18, 2))),
        (CAST('C05D2003-0000-4000-8000-000000000003' AS UNIQUEIDENTIFIER), @Project1, 'COST-DEMO-CP-003', N'Phát triển phần mềm', CAST('20260630' AS DATETIME), CAST(260000000 AS DECIMAL(18, 2))),
        (CAST('C05D2004-0000-4000-8000-000000000004' AS UNIQUEIDENTIFIER), @Project1, 'COST-DEMO-CP-004', N'Triển khai và đào tạo', CAST('20260825' AS DATETIME), CAST(160000000 AS DECIMAL(18, 2))),
        (CAST('C05D2005-0000-4000-8000-000000000005' AS UNIQUEIDENTIFIER), @Project2, 'COST-DEMO-CP-005', N'Khảo sát nội dung cổng thông tin', CAST('20260312' AS DATETIME), CAST(90000000 AS DECIMAL(18, 2))),
        (CAST('C05D2006-0000-4000-8000-000000000006' AS UNIQUEIDENTIFIER), @Project2, 'COST-DEMO-CP-006', N'Thiết kế giao diện và trải nghiệm', CAST('20260422' AS DATETIME), CAST(210000000 AS DECIMAL(18, 2))),
        (CAST('C05D2007-0000-4000-8000-000000000007' AS UNIQUEIDENTIFIER), @Project2, 'COST-DEMO-CP-007', N'Lập trình chức năng', CAST('20260618' AS DATETIME), CAST(200000000 AS DECIMAL(18, 2))),
        (CAST('C05D2008-0000-4000-8000-000000000008' AS UNIQUEIDENTIFIER), @Project2, 'COST-DEMO-CP-008', N'Kiểm thử và bàn giao', CAST('20260810' AS DATETIME), CAST(120000000 AS DECIMAL(18, 2))),
        (CAST('C05D2009-0000-4000-8000-000000000009' AS UNIQUEIDENTIFIER), @Project3, 'COST-DEMO-CP-009', N'Phân tích và thiết kế ứng dụng', CAST('20260220' AS DATETIME), CAST(100000000 AS DECIMAL(18, 2))),
        (CAST('C05D2010-0000-4000-8000-000000000010' AS UNIQUEIDENTIFIER), @Project3, 'COST-DEMO-CP-010', N'Phát triển ứng dụng đa nền tảng', CAST('20260428' AS DATETIME), CAST(240000000 AS DECIMAL(18, 2))),
        (CAST('C05D2011-0000-4000-8000-000000000011' AS UNIQUEIDENTIFIER), @Project3, 'COST-DEMO-CP-011', N'Kiểm thử, phát hành và bảo hành', CAST('20260720' AS DATETIME), CAST(220000000 AS DECIMAL(18, 2)))
    ) source
    (
        IdChiPhi,
        IdDuAn,
        MaKhoanChi,
        TenKhoanChi,
        NgayPhatSinh,
        SoTien
    )
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.TblChiPhi existing
        WHERE existing.IdChiPhi = source.IdChiPhi
    );

    INSERT INTO dbo.TblThanhToan
    (
        IdThanhToan,
        IdDuAn,
        MaDotThanhToan,
        TenDotThanhToan,
        SoTien,
        HanThanhToan,
        NgayThanhToanThucTe,
        TrangThai,
        GhiChu,
        DaXoa,
        NguoiTao,
        NgayTao
    )
    SELECT
        source.IdThanhToan,
        source.IdDuAn,
        source.MaDotThanhToan,
        source.TenDotThanhToan,
        source.SoTien,
        source.HanThanhToan,
        source.NgayThanhToanThucTe,
        CASE WHEN source.NgayThanhToanThucTe IS NULL THEN 0 ELSE 2 END,
        N'Dữ liệu kiểm thử Dashboard chi phí',
        0,
        @SeedUser,
        GETDATE()
    FROM
    (
        VALUES
        (CAST('C05D3001-0000-4000-8000-000000000001' AS UNIQUEIDENTIFIER), @Project1, 'COST-DEMO-TT-001', N'Tạm ứng dự án ERP', CAST(500000000 AS DECIMAL(18, 2)), CAST('20260615' AS DATETIME), CAST('20260615' AS DATETIME)),
        (CAST('C05D3002-0000-4000-8000-000000000002' AS UNIQUEIDENTIFIER), @Project1, 'COST-DEMO-TT-002', N'Thanh toán nghiệm thu ERP', CAST(500000000 AS DECIMAL(18, 2)), CAST('20260830' AS DATETIME), CAST('20260830' AS DATETIME)),
        (CAST('C05D3003-0000-4000-8000-000000000003' AS UNIQUEIDENTIFIER), @Project1, 'COST-DEMO-TT-003', N'Thanh toán bảo hành ERP', CAST(200000000 AS DECIMAL(18, 2)), CAST('20260915' AS DATETIME), NULL),
        (CAST('C05D3004-0000-4000-8000-000000000004' AS UNIQUEIDENTIFIER), @Project2, 'COST-DEMO-TT-004', N'Tạm ứng cổng thông tin', CAST(400000000 AS DECIMAL(18, 2)), CAST('20260520' AS DATETIME), CAST('20260520' AS DATETIME)),
        (CAST('C05D3005-0000-4000-8000-000000000005' AS UNIQUEIDENTIFIER), @Project2, 'COST-DEMO-TT-005', N'Thanh toán bàn giao cổng thông tin', CAST(400000000 AS DECIMAL(18, 2)), CAST('20260815' AS DATETIME), CAST('20260815' AS DATETIME)),
        (CAST('C05D3006-0000-4000-8000-000000000006' AS UNIQUEIDENTIFIER), @Project3, 'COST-DEMO-TT-006', N'Tạm ứng ứng dụng di động', CAST(250000000 AS DECIMAL(18, 2)), CAST('20260415' AS DATETIME), CAST('20260415' AS DATETIME)),
        (CAST('C05D3007-0000-4000-8000-000000000007' AS UNIQUEIDENTIFIER), @Project3, 'COST-DEMO-TT-007', N'Thanh toán khi phát hành', CAST(100000000 AS DECIMAL(18, 2)), CAST('20260720' AS DATETIME), CAST('20260720' AS DATETIME)),
        (CAST('C05D3008-0000-4000-8000-000000000008' AS UNIQUEIDENTIFIER), @Project3, 'COST-DEMO-TT-008', N'Thanh toán sau bảo hành', CAST(150000000 AS DECIMAL(18, 2)), CAST('20260815' AS DATETIME), NULL)
    ) source
    (
        IdThanhToan,
        IdDuAn,
        MaDotThanhToan,
        TenDotThanhToan,
        SoTien,
        HanThanhToan,
        NgayThanhToanThucTe
    )
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.TblThanhToan existing
        WHERE existing.IdThanhToan = source.IdThanhToan
    );

    COMMIT TRANSACTION;

    SELECT N'Dự án demo' AS N'Nhóm dữ liệu', COUNT(*) AS N'Số lượng'
    FROM dbo.TblDuAn WHERE NguoiTao = @SeedUser AND DaXoa = 0
    UNION ALL
    SELECT N'Khoản chi demo', COUNT(*)
    FROM dbo.TblChiPhi WHERE NguoiTao = @SeedUser AND DaXoa = 0
    UNION ALL
    SELECT N'Thanh toán demo', COUNT(*)
    FROM dbo.TblThanhToan WHERE NguoiTao = @SeedUser AND DaXoa = 0;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

/*
Xóa riêng dữ liệu demo khi không còn cần:

BEGIN TRANSACTION;
DELETE FROM dbo.TblThanhToan WHERE NguoiTao = N'CODEX_DASHBOARD_COST_DEMO';
DELETE FROM dbo.TblChiPhi WHERE NguoiTao = N'CODEX_DASHBOARD_COST_DEMO';
DELETE FROM dbo.TblDuAn WHERE NguoiTao = N'CODEX_DASHBOARD_COST_DEMO';
DELETE FROM dbo.TblHopDongThucHien WHERE NguoiTao = N'CODEX_DASHBOARD_COST_DEMO';
COMMIT TRANSACTION;
*/
