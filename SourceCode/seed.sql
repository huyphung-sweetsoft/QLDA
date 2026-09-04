SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
BEGIN TRANSACTION;

DECLARE @IdNhanVien UNIQUEIDENTIFIER = 'E84386B1-A92A-4B4D-A7F8-9E601B45B4EB';
DECLARE @DA1 UNIQUEIDENTIFIER = 'B0B18ED0-CC5F-4E02-A013-8DFDD85BB6A2';
DECLARE @DA2 UNIQUEIDENTIFIER = '9134A001-5F02-0070-A14D-80FC10DE5C57';
DECLARE @DA3 UNIQUEIDENTIFIER = '5836A001-D8F4-0070-8C25-902D2BDA5491';

DECLARE @Task1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Task2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Task3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Task4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Task5 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM dbo.TblThanhVienDuAn WHERE IdDuAn = @DA1 AND IdNhanVien = @IdNhanVien AND DaXoa = 0)
BEGIN
    INSERT INTO dbo.TblThanhVienDuAn (IdThanhVienDuAn, IdDuAn, IdNhanVien, IdVaiTroDuAn, NgayThamGia, GhiChu, DaXoa, NguoiTao, NgayTao)
    VALUES (NEWID(), @DA1, @IdNhanVien, NULL, '2026-08-01', N'Dữ liệu mẫu Dashboard Employee', 0, N'SeedDashboard', GETDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TblThanhVienDuAn WHERE IdDuAn = @DA2 AND IdNhanVien = @IdNhanVien AND DaXoa = 0)
BEGIN
    INSERT INTO dbo.TblThanhVienDuAn (IdThanhVienDuAn, IdDuAn, IdNhanVien, IdVaiTroDuAn, NgayThamGia, GhiChu, DaXoa, NguoiTao, NgayTao)
    VALUES (NEWID(), @DA2, @IdNhanVien, NULL, '2026-08-26', N'Dữ liệu mẫu Dashboard Employee', 0, N'SeedDashboard', GETDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TblThanhVienDuAn WHERE IdDuAn = @DA3 AND IdNhanVien = @IdNhanVien AND DaXoa = 0)
BEGIN
    INSERT INTO dbo.TblThanhVienDuAn (IdThanhVienDuAn, IdDuAn, IdNhanVien, IdVaiTroDuAn, NgayThamGia, GhiChu, DaXoa, NguoiTao, NgayTao)
    VALUES (NEWID(), @DA3, @IdNhanVien, NULL, '2026-08-24', N'Dữ liệu mẫu Dashboard Employee', 0, N'SeedDashboard', GETDATE());
END;

INSERT INTO dbo.TblCongViec (IdCongViec, IdDuAn, MaCongViec, TenCongViec, MoTa, NgayBatDau, ThoiHanNgay, NgayKetThuc, NgayHoanThanhThucTe, PhanTramHoanThanh, TrangThai, DaXoa, NguoiTao, NgayTao)
VALUES (@Task1, @DA1, 'HAO-001', N'Hoàn thành tài liệu yêu cầu', N'Hoàn thiện tài liệu yêu cầu nghiệp vụ', '2026-08-01', 5, '2026-08-08', '2026-08-08', 100, 2, 0, N'SeedDashboard', GETDATE());

INSERT INTO dbo.TblCongViec (IdCongViec, IdDuAn, MaCongViec, TenCongViec, MoTa, NgayBatDau, ThoiHanNgay, NgayKetThuc, NgayHoanThanhThucTe, PhanTramHoanThanh, TrangThai, DaXoa, NguoiTao, NgayTao)
VALUES (@Task2, @DA1, 'HAO-002', N'Phân tích API', N'Phân tích và xây dựng đặc tả API', '2026-08-20', 10, '2026-08-29', NULL, 60, 1, 0, N'SeedDashboard', GETDATE());

INSERT INTO dbo.TblCongViec (IdCongViec, IdDuAn, MaCongViec, TenCongViec, MoTa, NgayBatDau, ThoiHanNgay, NgayKetThuc, NgayHoanThanhThucTe, PhanTramHoanThanh, TrangThai, DaXoa, NguoiTao, NgayTao)
VALUES (@Task3, @DA2, 'HAO-003', N'Fix lỗi đăng nhập', N'Xử lý lỗi xác thực người dùng', '2026-08-20', 5, '2026-08-24', NULL, 20, 1, 0, N'SeedDashboard', GETDATE());

INSERT INTO dbo.TblCongViec (IdCongViec, IdDuAn, MaCongViec, TenCongViec, MoTa, NgayBatDau, ThoiHanNgay, NgayKetThuc, NgayHoanThanhThucTe, PhanTramHoanThanh, TrangThai, DaXoa, NguoiTao, NgayTao)
VALUES (@Task4, @DA3, 'HAO-004', N'Thiết kế giao diện', N'Thiết kế giao diện module', '2026-08-24', 7, '2026-08-31', NULL, 40, 1, 0, N'SeedDashboard', GETDATE());

INSERT INTO dbo.TblCongViec (IdCongViec, IdDuAn, MaCongViec, TenCongViec, MoTa, NgayBatDau, ThoiHanNgay, NgayKetThuc, NgayHoanThanhThucTe, PhanTramHoanThanh, TrangThai, DaXoa, NguoiTao, NgayTao)
VALUES (@Task5, @DA3, 'HAO-005', N'Chuẩn bị tài liệu triển khai', N'Chuẩn bị tài liệu và hướng dẫn triển khai', '2026-08-25', 7, '2026-09-01', NULL, 0, 0, 0, N'SeedDashboard', GETDATE());

INSERT INTO dbo.TblCongViec_NhanVien (IdCongViec, IdNhanVien, NgayPhanCong, GhiChu)
VALUES
(@Task1, @IdNhanVien, '2026-08-01', N'Phân công HaoNK'),
(@Task2, @IdNhanVien, '2026-08-20', N'Phân công HaoNK'),
(@Task3, @IdNhanVien, '2026-08-20', N'Phân công HaoNK'),
(@Task4, @IdNhanVien, '2026-08-24', N'Phân công HaoNK'),
(@Task5, @IdNhanVien, '2026-08-25', N'Phân công HaoNK');

COMMIT TRANSACTION;
