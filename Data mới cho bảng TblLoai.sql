USE SweetSoft_QLDA;
GO

/* =========================================================
   1. LOẠI KHÁCH HÀNG
   ========================================================= */

INSERT INTO dbo.TblLoai
(
    IdLoai,
    DoiTuong,
    TenLoai,
    MoTa,
    ThuTuHienThi,
    KichHoat,
    DaXoa,
    NguoiTao,
    NgayTao,
    NguoiCapNhat,
    NgayCapNhat
)
VALUES
-- Dữ liệu cũ
(
    'F0D91645-6D82-4F0D-9875-07F8E5599CFA',
    'KHACH_HANG',
    N'Doanh nghiệp',
    N'Khách hàng là doanh nghiệp hoặc tổ chức.',
    1,
    1,
    0,
    'HE_THONG',
    '2026-08-21 10:04:43.697',
    NULL,
    NULL
),

-- Dữ liệu mẫu
(
    NEWID(),
    'KHACH_HANG',
    N'Cá nhân',
    N'Khách hàng là cá nhân.',
    2,
    1,
    0,
    'HE_THONG',
    GETDATE(),
    NULL,
    NULL
),
(
    NEWID(),
    'KHACH_HANG',
    N'Cơ quan nhà nước',
    N'Khách hàng là cơ quan hoặc đơn vị thuộc nhà nước.',
    3,
    1,
    0,
    'HE_THONG',
    GETDATE(),
    NULL,
    NULL
),
(
    NEWID(),
    'KHACH_HANG',
    N'Tổ chức giáo dục',
    N'Khách hàng là trường học, cơ sở hoặc tổ chức giáo dục.',
    4,
    1,
    0,
    'HE_THONG',
    GETDATE(),
    NULL,
    NULL
);
GO


/* =========================================================
   2. LOẠI DỰ ÁN
   ========================================================= */

INSERT INTO dbo.TblLoai
(
    IdLoai,
    DoiTuong,
    TenLoai,
    MoTa,
    ThuTuHienThi,
    KichHoat,
    DaXoa,
    NguoiTao,
    NgayTao,
    NguoiCapNhat,
    NgayCapNhat
)
VALUES
-- Dữ liệu cũ
(
    'EE61666C-4432-42ED-BE17-5272CAF94D71',
    'DU_AN',
    N'Phát triển phần mềm',
    N'Dự án xây dựng và triển khai sản phẩm phần mềm.',
    1,
    1,
    0,
    'HE_THONG',
    '2026-08-21 10:04:43.697',
    NULL,
    NULL
),

-- Dữ liệu mẫu
(
    NEWID(),
    'DU_AN',
    N'Bảo trì và nâng cấp hệ thống',
    N'Dự án bảo trì, cải tiến và nâng cấp hệ thống phần mềm hiện có.',
    2,
    1,
    0,
    'HE_THONG',
    GETDATE(),
    NULL,
    NULL
),
(
    NEWID(),
    'DU_AN',
    N'Triển khai hệ thống',
    N'Dự án triển khai và đưa hệ thống phần mềm vào vận hành thực tế.',
    3,
    1,
    0,
    'HE_THONG',
    GETDATE(),
    NULL,
    NULL
),
(
    NEWID(),
    'DU_AN',
    N'Tích hợp hệ thống',
    N'Dự án tích hợp nhiều hệ thống hoặc nền tảng để trao đổi và đồng bộ dữ liệu.',
    4,
    1,
    0,
    'HE_THONG',
    GETDATE(),
    NULL,
    NULL
);
GO


/* =========================================================
   3. PHÒNG BAN
   Không thêm dữ liệu mới.
   Dữ liệu hiện tại sẽ được migrate sang TblLoai riêng.
   ========================================================= */

INSERT INTO dbo.TblLoai
(
    IdLoai,
    DoiTuong,
    TenLoai,
    MoTa,
    ThuTuHienThi,
    KichHoat,
    DaXoa,
    NguoiTao,
    NgayTao,
    NguoiCapNhat,
    NgayCapNhat
)
VALUES
(
    '2ED7C324-4BCA-4F94-9ED7-3CEA60215782',
    'PHONG_BAN',
    N'Phòng Công nghệ thông tin',
    N'Phụ trách hệ thống và phát triển phần mềm',
    1,
    1,
    0,
    'administrator',
    '2026-08-22 20:49:16.577',
    NULL,
    NULL
),
(
    'F9B209E2-13C3-4CEC-891F-AC0F5ABB9A16',
    'PHONG_BAN',
    N'Phòng Tài chính - Kế toán',
    N'Quản lý ngân sách và tài chính dự án',
    3,
    1,
    0,
    'administrator',
    '2026-08-22 20:49:16.577',
    NULL,
    NULL
),
(
    'BE597AB6-FDE4-4AD8-8A11-D2E636BEEB02',
    'PHONG_BAN',
    N'Phòng Hành chính - Nhân sự',
    N'Quản trị nhân lực và đời sống cán bộ',
    2,
    1,
    0,
    'administrator',
    '2026-08-22 20:49:16.577',
    NULL,
    NULL
);
GO


/* =========================================================
   4. CHỨC DANH
   Không đưa IdPhongBan sang TblLoai.
   ========================================================= */

INSERT INTO dbo.TblLoai
(
    IdLoai,
    DoiTuong,
    TenLoai,
    MoTa,
    ThuTuHienThi,
    KichHoat,
    DaXoa,
    NguoiTao,
    NgayTao,
    NguoiCapNhat,
    NgayCapNhat
)
VALUES
(
    'F7F107F6-85B9-4030-97AF-508501FE9656',
    'CHUC_DANH',
    N'Quản lý dự án (Project Manager)',
    N'Lập kế hoạch và điều phối tiến độ',
    2,
    1,
    0,
    'administrator',
    '2026-08-22 20:49:16.577',
    NULL,
    NULL
),
(
    '593FE3E3-3DA2-48B2-8BBE-7001643550CA',
    'CHUC_DANH',
    N'Chuyên viên Nhân sự',
    N'Phụ trách tuyển dụng và nhân sự',
    3,
    1,
    0,
    'administrator',
    '2026-08-22 20:49:16.577',
    NULL,
    NULL
),
(
    'D316EB79-59F0-4845-9431-BE911836DCB1',
    'CHUC_DANH',
    N'Kỹ sư phần mềm (Developer)',
    N'Lập trình và triển khai module hệ thống',
    1,
    1,
    0,
    'administrator',
    '2026-08-22 20:49:16.577',
    NULL,
    NULL
);
GO

SELECT
    IdLoai,
    DoiTuong,
    TenLoai,
    MoTa,
    ThuTuHienThi,
    KichHoat,
    DaXoa
FROM dbo.TblLoai
ORDER BY
    DoiTuong,
    ThuTuHienThi;

SELECT
    DoiTuong,
    COUNT(*) AS SoLuong
FROM dbo.TblLoai
GROUP BY DoiTuong
ORDER BY DoiTuong;