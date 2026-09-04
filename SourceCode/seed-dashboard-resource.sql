SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @SeedUser NVARCHAR(150) = N'CODEX_DASHBOARD_RESOURCE_DEMO';
    DECLARE @ProjectId UNIQUEIDENTIFIER =
        'B0B18ED0-CC5F-4E02-A013-8DFDD85BB6A2';
    DECLARE @EmployeeUnder UNIQUEIDENTIFIER =
        'CEEC7FFF-7845-4BDD-AEFF-50F8D7480155';
    DECLARE @EmployeeBalanced UNIQUEIDENTIFIER =
        'B7DD45AD-177E-400C-BD2F-D8A73B62CB70';
    DECLARE @EmployeeOver UNIQUEIDENTIFIER =
        '2EC3D762-AAD9-405B-A4D8-4481A6A67875';
    DECLARE @EmployeeAdjacent UNIQUEIDENTIFIER =
        '4C78FB7F-B1C9-426D-9004-A3C3BAA3E328';

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblDuAn
        WHERE IdDuAn = @ProjectId AND DaXoa = 0
    )
    BEGIN
        THROW 51100, N'Không tìm thấy dự án DA_001 để tạo dữ liệu nguồn lực demo.', 1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.aspnet_Users
        WHERE UserId IN
        (
            @EmployeeUnder,
            @EmployeeBalanced,
            @EmployeeOver,
            @EmployeeAdjacent
        )
          AND LaNhanVien = 1
          AND IsDeleted = 0
    ) <> 4
    BEGIN
        THROW 51101, N'Không tìm thấy đủ bốn nhân sự đang hoạt động để tạo dữ liệu demo.', 1;
    END;

    INSERT INTO dbo.TblThanhVienDuAn
    (
        IdThanhVienDuAn,
        IdDuAn,
        IdNhanVien,
        IdVaiTroDuAn,
        NgayThamGia,
        GhiChu,
        DaXoa,
        NguoiTao,
        NgayTao
    )
    SELECT
        source.IdThanhVienDuAn,
        @ProjectId,
        source.IdNhanVien,
        NULL,
        CAST('20260824' AS DATETIME),
        N'Dữ liệu kiểm thử Dashboard nguồn lực',
        0,
        @SeedUser,
        GETDATE()
    FROM
    (
        VALUES
        (CAST('D45B0001-0000-4000-8000-000000000001' AS UNIQUEIDENTIFIER), @EmployeeUnder),
        (CAST('D45B0002-0000-4000-8000-000000000002' AS UNIQUEIDENTIFIER), @EmployeeBalanced),
        (CAST('D45B0003-0000-4000-8000-000000000003' AS UNIQUEIDENTIFIER), @EmployeeOver),
        (CAST('D45B0004-0000-4000-8000-000000000004' AS UNIQUEIDENTIFIER), @EmployeeAdjacent)
    ) source (IdThanhVienDuAn, IdNhanVien)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblThanhVienDuAn existing
        WHERE existing.IdDuAn = @ProjectId
          AND existing.IdNhanVien = source.IdNhanVien
          AND existing.DaXoa = 0
    );

    INSERT INTO dbo.TblCongViec
    (
        IdCongViec,
        IdDuAn,
        MaCongViec,
        TenCongViec,
        MoTa,
        NgayBatDau,
        ThoiHanNgay,
        NgayKetThuc,
        NgayHoanThanhThucTe,
        PhanTramHoanThanh,
        TrangThai,
        DaXoa,
        NguoiTao,
        NgayTao
    )
    SELECT
        source.IdCongViec,
        @ProjectId,
        source.MaCongViec,
        source.TenCongViec,
        N'Dữ liệu kiểm thử Dashboard nguồn lực',
        source.NgayBatDau,
        source.ThoiHanNgay,
        source.NgayKetThuc,
        NULL,
        source.PhanTramHoanThanh,
        1,
        0,
        @SeedUser,
        GETDATE()
    FROM
    (
        VALUES
        (CAST('D45A0001-0000-4000-8000-000000000001' AS UNIQUEIDENTIFIER), 'RES-DEMO-001', N'Phân tích yêu cầu báo cáo', CAST('20260831' AS DATETIME), 3, CAST('20260902' AS DATETIME), 40),
        (CAST('D45A0002-0000-4000-8000-000000000002' AS UNIQUEIDENTIFIER), 'RES-DEMO-002', N'Phát triển chức năng quản lý', CAST('20260831' AS DATETIME), 5, CAST('20260904' AS DATETIME), 35),
        (CAST('D45A0003-0000-4000-8000-000000000003' AS UNIQUEIDENTIFIER), 'RES-DEMO-003', N'Kiểm thử tích hợp hệ thống', CAST('20260831' AS DATETIME), 5, CAST('20260904' AS DATETIME), 50),
        (CAST('D45A0004-0000-4000-8000-000000000004' AS UNIQUEIDENTIFIER), 'RES-DEMO-004', N'Xử lý lỗi sau kiểm thử', CAST('20260902' AS DATETIME), 3, CAST('20260904' AS DATETIME), 20),
        (CAST('D45A0005-0000-4000-8000-000000000005' AS UNIQUEIDENTIFIER), 'RES-DEMO-005', N'Chuẩn bị môi trường kiểm thử', CAST('20260824' AS DATETIME), 5, CAST('20260828' AS DATETIME), 70),
        (CAST('D45A0006-0000-4000-8000-000000000006' AS UNIQUEIDENTIFIER), 'RES-DEMO-006', N'Chuẩn bị tài liệu triển khai', CAST('20260907' AS DATETIME), 5, CAST('20260911' AS DATETIME), 0)
    ) source
    (
        IdCongViec,
        MaCongViec,
        TenCongViec,
        NgayBatDau,
        ThoiHanNgay,
        NgayKetThuc,
        PhanTramHoanThanh
    )
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblCongViec existing
        WHERE existing.IdCongViec = source.IdCongViec
           OR (existing.MaCongViec = source.MaCongViec AND existing.DaXoa = 0)
    );

    IF
    (
        SELECT COUNT(*)
        FROM dbo.TblCongViec
        WHERE IdCongViec IN
        (
            'D45A0001-0000-4000-8000-000000000001',
            'D45A0002-0000-4000-8000-000000000002',
            'D45A0003-0000-4000-8000-000000000003',
            'D45A0004-0000-4000-8000-000000000004',
            'D45A0005-0000-4000-8000-000000000005',
            'D45A0006-0000-4000-8000-000000000006'
        )
          AND DaXoa = 0
    ) <> 6
    BEGIN
        THROW 51102, N'Mã công việc demo bị trùng với dữ liệu khác.', 1;
    END;

    INSERT INTO dbo.TblCongViec_NhanVien
    (
        IdCongViec,
        IdNhanVien,
        NgayPhanCong,
        GhiChu
    )
    SELECT
        source.IdCongViec,
        source.IdNhanVien,
        source.NgayPhanCong,
        N'Dữ liệu kiểm thử Dashboard nguồn lực'
    FROM
    (
        VALUES
        (CAST('D45A0001-0000-4000-8000-000000000001' AS UNIQUEIDENTIFIER), @EmployeeUnder, CAST('20260831' AS DATETIME)),
        (CAST('D45A0002-0000-4000-8000-000000000002' AS UNIQUEIDENTIFIER), @EmployeeBalanced, CAST('20260831' AS DATETIME)),
        (CAST('D45A0003-0000-4000-8000-000000000003' AS UNIQUEIDENTIFIER), @EmployeeOver, CAST('20260831' AS DATETIME)),
        (CAST('D45A0004-0000-4000-8000-000000000004' AS UNIQUEIDENTIFIER), @EmployeeOver, CAST('20260902' AS DATETIME)),
        (CAST('D45A0005-0000-4000-8000-000000000005' AS UNIQUEIDENTIFIER), @EmployeeAdjacent, CAST('20260824' AS DATETIME)),
        (CAST('D45A0006-0000-4000-8000-000000000006' AS UNIQUEIDENTIFIER), @EmployeeAdjacent, CAST('20260907' AS DATETIME))
    ) source (IdCongViec, IdNhanVien, NgayPhanCong)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblCongViec_NhanVien existing
        WHERE existing.IdCongViec = source.IdCongViec
          AND existing.IdNhanVien = source.IdNhanVien
    );

    COMMIT TRANSACTION;

    SELECT
        N'DA_001' AS DuAn,
        COUNT(*) AS SoCongViecDemo
    FROM dbo.TblCongViec
    WHERE NguoiTao = @SeedUser
      AND DaXoa = 0;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
